namespace QuartzNode;

using Core.Quartz.EFCore;
using Core.Quartz.Jobs;
using Core.Quartz.Jobs.Extensions;
using CrystalQuartz.AspNetCore;
using Extensions;
using global::Extensions.Options.AutoBinder;
using HealthChecks.ApplicationStatus.DependencyInjection;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Quartz;
using Serilog;
using Serilog.Exceptions;

public class Program
{
    public static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .Enrich.WithExceptionDetails()
            .CreateBootstrapLogger();

        try
        {
            var host = CreateHostBuilder(args).Build();
            await host.InitAsync();
            await host.RunAsync();
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "Application terminated unexpectedly.");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, builder) => builder.ApplyQuartzNodeConfiguration(context, args))
            .ConfigureLogging(builder => builder.Configure(options =>
            {
                options.ActivityTrackingOptions = ActivityTrackingOptions.TraceId | ActivityTrackingOptions.SpanId |
                                                  ActivityTrackingOptions.ParentId;
            }))
            .UseSerilog((context, _, config) => config.ReadFrom.Configuration(context.Configuration))
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                    .ConfigureServices((context, services) =>
                    {
                        // generate Feature Manager to control injection, this is not a built-in feature
                        // ref: https://github.com/microsoft/FeatureManagement-Dotnet/issues/39
                        var featureManager = context.Configuration.GenerateFeatureManager();

                        services.AddFeatureManagement();

                        var healthChecksBuilder = services.AddHealthChecks()
                            .AddApplicationStatus()
                            .AddDbContextCheck<QuartzDbContext>();

                        if (featureManager.IsEnabled(FeatureFlags.HealthChecksMetrics))
                        {
                            healthChecksBuilder.ForwardToPrometheus();
                        }

                        services.AddDbContext<QuartzDbContext>(optionsBuilder =>
                        {
                            optionsBuilder.UseNpgsql(
                                context.Configuration.GetConnectionString(nameof(QuartzDbContext)));
                        });

                        services.AddAsyncInitializer<DbContextInitializer<QuartzDbContext>>();

                        services.AddOptions<QuartzOptions>()
                            .AutoBind()
                            .PostConfigure(options =>
                            {
                                options.Scheduling.IgnoreDuplicates = true;
                                options.Scheduling.OverWriteExistingData = true;
                            });

                        services.AddQuartz(config =>
                        {
                            if (featureManager.IsEnabled(FeatureFlags.HostMode))
                            {
                                config.UseDefaultThreadPool(0);
                                config.AddJobsFromAssemblyContaining<HelloWorldJob>();
                            }
                        });

                        services.AddQuartzHostedService(options => { options.WaitForJobsToComplete = true; });

                        if (featureManager.IsEnabled(FeatureFlags.OpenTelemetryTracing))
                        {
                            services.AddOptions<OtlpExporterOptions>().AutoBind();

                            var d = services.AddOpenTelemetry()
                                .WithTracing(builder => builder
                                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                                        .AddService(Environment.MachineName))
                                    .AddQuartzInstrumentation()
                                    .AddOtlpExporter());
                        }


                        services.AddAuthorization();
                    })
                    .Configure((context, app) =>
                    {
                        var featureManager = app.ApplicationServices.GetRequiredService<IFeatureManager>();

                        app.UseHttpsRedirection();

                        app.UseAuthorization();

                        app.UseForFeature(FeatureFlags.HostMode, builder =>
                        {
                            builder.UseCrystalQuartz(() =>
                                builder.ApplicationServices.GetRequiredService<ISchedulerFactory>().GetScheduler());
                        });

                        app.UseRouting();

                        app.UseForFeature(FeatureFlags.PrometheusMetrics, builder => builder.UseHttpMetrics());

                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/", async http => http.Response.Redirect("/health"));

                            endpoints.MapHealthChecks("/health", new HealthCheckOptions
                            {
                                Predicate = _ => true,
                                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                            });

                            if (!context.HostingEnvironment.IsProduction())
                            {
                                endpoints.MapGet("/config", async httpContext =>
                                {
                                    var configuration =
                                        httpContext.RequestServices.GetRequiredService<IConfiguration>() as
                                            IConfigurationRoot;
                                    await httpContext.Response.WriteAsync(configuration!.GetDebugView());
                                });
                            }

                            if (featureManager.IsEnabled(FeatureFlags.PrometheusMetrics))
                            {
                                endpoints.MapMetrics();
                            }
                        });
                    });
            });
    }
}