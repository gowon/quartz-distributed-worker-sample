namespace QuartzNode;

using Core.Quartz.EFCore;
using Core.Quartz.Jobs;
using CrystalQuartz.AspNetCore;
using Extensions;
using global::Extensions.Options.AutoBinder;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
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
                        //var featureManager = context.Configuration.GenerateFeatureManager();

                        services.AddFeatureManagement();

                        services.AddDbContext<QuartzDbContext>(optionsBuilder =>
                        {
                            optionsBuilder.UseNpgsql(
                                context.Configuration.GetConnectionString(nameof(QuartzDbContext)));
                        });

                        services.AddAsyncInitializer<QuartzJobStoreInitializer>();

                        services.AddOptions<QuartzOptions>()
                            .AutoBind()
                            .PostConfigure(options =>
                            {
                                options.Scheduling.IgnoreDuplicates = true;
                                options.Scheduling.OverWriteExistingData = true;
                            });

                        services.AddQuartz(config =>
                        {
                            config.AddJob<HelloWorldJob>(HelloWorldJob.Key, job => job.StoreDurably());
                            config.AddJob<TimedHelloWorldJob>(TimedHelloWorldJob.Key, job => job.StoreDurably());
                            config.AddJob<NamedHelloWorldJob>(NamedHelloWorldJob.Key, job => job.StoreDurably());
                        });

                        services.AddQuartzHostedService(options => { options.WaitForJobsToComplete = true; });

                        services.AddAuthorization();
                    })
                    .Configure((context, app) =>
                    {
                        app.UseHttpsRedirection();

                        app.UseAuthorization();

                        app.UseForFeature(FeatureFlags.HostMode, builder =>
                        {
                            builder.UseCrystalQuartz(() =>
                                builder.ApplicationServices.GetRequiredService<ISchedulerFactory>().GetScheduler());
                        });

                        app.UseRouting();

                        app.UseEndpoints(endpoints =>
                        {
                            // todo: replace with HealthCheck endpoint
                            endpoints.MapGet("/", () => Results.Ok(Environment.MachineName));

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
                        });
                    });
            });
    }
}