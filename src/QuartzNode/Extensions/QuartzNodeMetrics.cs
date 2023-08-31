namespace QuartzNode.Extensions;

using System.Diagnostics.Metrics;
using System.Reflection;

internal static class QuartzNodeMetrics
{
    /// <summary>
    ///     The assembly name.
    /// </summary>
    internal static readonly AssemblyName AssemblyName = typeof(QuartzNodeMetrics).Assembly.GetName();

    /// <summary>
    ///     The activity source name.
    /// </summary>
    internal static readonly string ActivitySourceName = typeof(QuartzNodeMetrics).Namespace;

    /// <summary>
    ///     The version.
    /// </summary>
    internal static readonly Version Version = AssemblyName.Version;

    // ref: https://www.thorsten-hans.com/instrumenting-dotnet-apps-with-opentelemetry
    internal static readonly Meter Default = new(AssemblyName.Name, Version.ToString());
}