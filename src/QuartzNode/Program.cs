namespace QuartzNode;

extern alias QuartzPreRelease;
using System.Reflection;
using Carter;
using Core.Quartz.EFCore;
using Core.Quartz.Jobs;
using Core.Quartz.Jobs.Extensions;
using CrystalQuartz.AspNetCore;
using Extensions;
using global::Extensions.Options.AutoBinder;
using HealthChecks.ApplicationStatus.DependencyInjection;
using MassTransit;
using MassTransit.Logging;
using Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using Quartz.AspNetCore;
using QuartzPreRelease::Quartz;
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

            var featureManager = host.Services.GetRequiredService<IFeatureManager>();
            if (await featureManager.IsEnabledAsync(FeatureFlags.OrchestratorMode))
            {
                Log.ForContext<Program>().Information("Node running in Orchestrator Mode.");
                await host.InitAndRunAsync();
            }
            else
            {
                await host.RunAsync();
            }
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
                webBuilder.ConfigureServices((builderContext, services) =>
                    {
                        // generate Feature Manager to control injection, this is not a built-in feature
                        // ref: https://github.com/microsoft/FeatureManagement-Dotnet/issues/39
                        builderContext.EnableFeatureManagementDependencyInjection();
                        var featureManager = builderContext.GetFeatureManager();

                        services.AddFeatureManagement();

                        services.Configure<RouteOptions>(options =>
                        {
                            options.LowercaseUrls = true;
                            options.LowercaseQueryStrings = true;
                        });

                        services.AddCarter();

                        services.AddForFeature(FeatureFlags.OrchestratorMode, s =>
                        {
                            s.AddEndpointsApiExplorer();
                            s.AddSwaggerGen();
                        });

                        var healthChecksBuilder = services.AddHealthChecks()
                            .AddApplicationStatus()
                            .AddCheck<QuartzHealthCheck>("quartz");

                        if (featureManager.IsEnabled(FeatureFlags.HealthChecksMetrics))
                        {
                            healthChecksBuilder.ForwardToPrometheus();
                        }

                        #region Quartz.NET Configuration

                        services.AddDbContext<QuartzDbContext>(optionsBuilder =>
                        {
                            optionsBuilder.UseNpgsql(
                                builderContext.Configuration.GetConnectionString(nameof(QuartzDbContext)));
                        });

                        services.AddAsyncInitializer<DbContextInitializer<QuartzDbContext>>();

                        services.AddOptions<QuartzOptions>()
                            .AutoBind()
                            .PostConfigure(options =>
                            {
                                options.Scheduling.IgnoreDuplicates = true;
                                options.Scheduling.OverWriteExistingData = true;
                            });
                        services.AddSingleton<SchedulerMetricsListener>();
                        services.AddQuartz(config =>
                        {
                            config.AddSchedulerListener<SchedulerMetricsListener>();

                            if (featureManager.IsEnabled(FeatureFlags.OrchestratorMode))
                            {
                                // instance should not perform work while in orchestrator mode
                                config.UseDefaultThreadPool(0);
                                config.RegisterJobsFromAssemblyContaining<HelloWorldJob>();
                            }

                            config.AddHttpApi();
                        });

                        services.AddQuartzHostedService(options => { options.WaitForJobsToComplete = true; });

                        #endregion Quartz.NET Configuration

                        #region MassTransit

                        services.AddOptions<MassTransitHostOptions>().AutoBind();
                        services.AddOptions<RabbitMqTransportOptions>().AutoBind();

                        services.AddMassTransit(config =>
                        {
                            if (!featureManager.IsEnabled(FeatureFlags.OrchestratorMode))
                            {
                                config.AddConsumer<TerminateJobMessageConsumer>();
                            }

                            config.UsingRabbitMq((context, factory) => { factory.ConfigureEndpoints(context); });
                        });

                        #endregion MassTransit

                        if (featureManager.IsEnabled(FeatureFlags.OpenTelemetryTracing))
                        {
                            services.AddOptions<OtlpExporterOptions>().AutoBind();

                            services.AddOpenTelemetry()
                                .ConfigureResource(builder => builder
                                    .AddService(Environment.MachineName)
                                    .AddTelemetrySdk()
                                    .AddEnvironmentVariableDetector())
                                .WithMetrics(builder => builder
                                    .AddRuntimeInstrumentation()
                                    .AddMeter(QuartzNodeMetrics.Default.Name))
                                .WithTracing(builder => builder
                                    .AddQuartzInstrumentation()
                                    .AddSource(DiagnosticHeaders.DefaultListenerName)
                                    .AddOtlpExporter());
                        }

                        services.AddAuthorization();
                    })
                    .Configure((_, app) =>
                    {
                        app.UseHttpsRedirection();

                        app.UseAuthorization();

                        app.UseForFeature(FeatureFlags.OrchestratorMode, builder =>
                        {
                            // todo: would be better to register using endpoint builder.
                            // missing feature, ref: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/2472
                            builder.UseSwagger();
                            builder.UseSwaggerUI(options =>
                            {
                                options.IndexStream = () =>
                                    Assembly.GetExecutingAssembly()
                                        .GetManifestResourceStream($"{typeof(Program).Namespace}.Swagger.index.html");
                            });

                            builder.UseCrystalQuartz(() =>
                                builder.ApplicationServices.GetRequiredService<ISchedulerFactory>().GetScheduler());
                        });

                        app.UseRouting();

                        app.UseForFeature(FeatureFlags.PrometheusMetrics, builder => builder.UseHttpMetrics());

                        app.UseEndpoints(endpoints => endpoints.MapCarter());
                    });
            });
    }
}