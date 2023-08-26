namespace QuartzNode;

/// <summary>
/// Feature flags that control the behavior of the application.
/// </summary>
public static class FeatureFlags
{
    /// <summary>
    /// When enabled, HealthCheck results will be exported to the <c>/metrics</c> endpoint.
    /// </summary>
    public const string HealthChecksMetrics = nameof(HealthChecksMetrics);

    /// <summary>
    /// When enabled, will collect and transmit tracing data to an OTLP collector.
    /// </summary>
    public const string OpenTelemetryTracing = nameof(OpenTelemetryTracing);

    /// <summary>
    /// When enabled, will expose the Prometheus metrics on the <c>/metrics</c> endpoint.
    /// </summary>
    public const string PrometheusMetrics = nameof(PrometheusMetrics);

    /// <summary>
    /// When enabled, will set the node to run in Host Mode, as well as expose the CrystalQuartz UI on the <c>/quartz</c> endpoint.
    /// </summary>
    public const string HostMode = nameof(HostMode);
}