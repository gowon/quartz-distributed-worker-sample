namespace QuartzNode.Extensions;

using System.Reflection;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder ApplyQuartzNodeConfiguration(this IConfigurationBuilder builder,
        HostBuilderContext context, string[] args)
    {
        var environment = context.HostingEnvironment;
        builder.AddJsonFile("appsettings.json", true, true);
        builder.AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true, true);

        var isRunningInContainer = bool.TryParse(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
            out var inContainer) && inContainer;

        if (isRunningInContainer)
        {
            builder.AddJsonFile("containersettings.json", true, true);
            builder.AddJsonFile($"containersettings.{environment.EnvironmentName}.json", true, true);
        }

        builder.AddUserSecrets(Assembly.GetCallingAssembly())
            .AddCustomEnvironmentVariables()
            .AddCommandLine(args);

        return builder;
    }
}