namespace QuartzNode.Modules;

using Carter;
using Extensions;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.FeatureManagement;
using Prometheus;
using Quartz.AspNetCore;

public class CoreModule : ICarterModule
{
    private readonly IFeatureManager _featureManager;
    private readonly IHostEnvironment _hostEnvironment;

    public CoreModule(IFeatureManager featureManager, IHostEnvironment hostEnvironment)
    {
        _featureManager = featureManager;
        _hostEnvironment = hostEnvironment;
    }

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/", async http =>
            http.Response.Redirect(_featureManager.IsEnabled(FeatureFlags.OrchestratorMode)
                ? "/swagger"
                : "/health"));

        var healthCheckOptions = new HealthCheckOptions
        {
            Predicate = _ => true
        };

        if (!_hostEnvironment.IsProduction())
        {
            healthCheckOptions.ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse;

            app.MapGet("/config", (IConfiguration configuration) =>
            {
                var root = configuration as IConfigurationRoot;
                return Results.Text(root!.GetDebugView());
            });
        }

        app.MapHealthChecks("/health", healthCheckOptions);

        if (_featureManager.IsEnabled(FeatureFlags.PrometheusMetrics))
        {
            app.MapMetrics();
        }

        app.MapQuartzApi();
    }
}