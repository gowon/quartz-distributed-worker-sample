namespace Core.Quartz.Jobs.Extensions;

extern alias QuartzPreRelease;
using QuartzPreRelease::Quartz;
using System.Reflection;


public static class ServiceCollectionQuartzConfiguratorExtensions
{
    public static IServiceCollectionQuartzConfigurator RegisterJobsFromAssemblyContaining(
        this IServiceCollectionQuartzConfigurator services, Type type)
    {
        return services.RegisterJobsFromAssembly(type.Assembly);
    }

    public static IServiceCollectionQuartzConfigurator RegisterJobsFromAssemblyContaining<T>(
        this IServiceCollectionQuartzConfigurator services)
    {
        return services.RegisterJobsFromAssembly(typeof(T).Assembly);
    }

    public static IServiceCollectionQuartzConfigurator RegisterJobsFromAssembly(
        this IServiceCollectionQuartzConfigurator configurator, Assembly assembly)
    {
        var providers = assembly.GetTypes()
            .Where(type => type.GetCustomAttribute<QuartzJobProviderAttribute>() != null)
            .ToDictionary(type => type.GetCustomAttribute<QuartzJobProviderAttribute>()!.MemberType ?? type,
                type => type.GetCustomAttribute<QuartzJobProviderAttribute>()!.MethodName);

        foreach (var (type, methodName) in providers)
        {
            var methodInfo = type?.GetMethod(methodName,
                BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
                new[] { typeof(IServiceCollectionQuartzConfigurator) });
            methodInfo?.Invoke(null, new object?[] { configurator });
        }

        return configurator;
    }
}