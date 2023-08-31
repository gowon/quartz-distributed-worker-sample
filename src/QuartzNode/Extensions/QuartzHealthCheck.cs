namespace QuartzNode.Extensions;

using Microsoft.Extensions.Diagnostics.HealthChecks;
using Quartz;

// this is internal to the Quartz library for some reason
// ref: https://github.com/quartznet/quartznet/blob/main/src/Quartz.AspNetCore/AspNetCore/HealthChecks/QuartzHealthCheck.cs
public class QuartzHealthCheck : IHealthCheck
{
    private readonly ISchedulerFactory _schedulerFactory;

    public QuartzHealthCheck(ISchedulerFactory schedulerFactory)
    {
        _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
    }

    async Task<HealthCheckResult> IHealthCheck.CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        if (!scheduler.IsStarted)
        {
            return HealthCheckResult.Unhealthy("Quartz scheduler is not running");
        }

        try
        {
            // Ask for a job we know doesn't exist
            await scheduler.CheckExists(new JobKey(Guid.NewGuid().ToString()), cancellationToken);
        }
        catch (SchedulerException)
        {
            return HealthCheckResult.Unhealthy("Quartz scheduler cannot connect to the store");
        }

        return HealthCheckResult.Healthy("Quartz scheduler is ready");
    }
}