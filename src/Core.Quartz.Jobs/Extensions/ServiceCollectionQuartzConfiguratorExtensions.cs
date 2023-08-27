namespace Core.Quartz.Jobs.Extensions;

using System.Reflection;
using global::Quartz;

public static class ServiceCollectionQuartzConfiguratorExtensions
{
    public static IServiceCollectionQuartzConfigurator AddJobsFromAssemblyContaining(
        this IServiceCollectionQuartzConfigurator services, Type type)
    {
        return services.AddJobsFromAssembly(type.Assembly);
    }

    public static IServiceCollectionQuartzConfigurator AddJobsFromAssemblyContaining<T>(
        this IServiceCollectionQuartzConfigurator services)
    {
        return services.AddJobsFromAssembly(typeof(T).Assembly);
    }

    public static IServiceCollectionQuartzConfigurator AddJobsFromAssembly(
        this IServiceCollectionQuartzConfigurator configurator, Assembly assembly)
    {
        var jobs = assembly.GetTypes().Where(type => typeof(IJob).IsAssignableFrom(type));
        foreach (var jobType in jobs)
        {
            var quartzJobAttribute = jobType.GetCustomAttribute<QuartzJobAttribute>();
            if (quartzJobAttribute == null)
            {
                continue;
            }

            var mapping = jobType.GetCustomAttributes<QuartzJobDataAttribute>()
                .DistinctBy(attribute => attribute.Name)
                .ToDictionary(attribute => attribute.Name, attribute => attribute.DefaultValue);
            var jobDataMap = new JobDataMap((IDictionary<string, object>)mapping);

            configurator.AddJob(jobType, configure: jobConfigurator =>
            {
                jobConfigurator
                    .StoreDurably()
                    .WithIdentity(quartzJobAttribute.Name, quartzJobAttribute.GroupName);

                if (jobDataMap.Any())
                {
                    jobConfigurator.SetJobData(jobDataMap);
                }

                if (quartzJobAttribute.Description != null)
                {
                    jobConfigurator.WithDescription(quartzJobAttribute.Description);
                }
            });
        }

        return configurator;
    }
}