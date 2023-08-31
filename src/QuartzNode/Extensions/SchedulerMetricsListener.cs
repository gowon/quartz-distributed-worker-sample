namespace QuartzNode.Extensions;

using System.Diagnostics.Metrics;
using Quartz;

public class SchedulerMetricsListener : ISchedulerListener
{
    private readonly ISchedulerFactory _schedulerFactory;
    private ObservableGauge<int>? _gauge;
    private IScheduler? _scheduler;
    private int _threadPoolSize;

    public SchedulerMetricsListener(ISchedulerFactory schedulerFactory)
    {
        _schedulerFactory = schedulerFactory;
    }

    public Task JobScheduled(ITrigger trigger, CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public Task JobUnscheduled(TriggerKey triggerKey, CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public Task TriggerFinalized(ITrigger trigger, CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public Task TriggerPaused(TriggerKey triggerKey, CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public Task TriggersPaused(string? triggerGroup, CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public Task TriggerResumed(TriggerKey triggerKey, CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public Task TriggersResumed(string? triggerGroup, CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public Task JobAdded(IJobDetail jobDetail, CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public Task JobDeleted(JobKey jobKey, CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public Task JobPaused(JobKey jobKey, CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public Task JobInterrupted(JobKey jobKey, CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public Task JobsPaused(string jobGroup, CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public Task JobResumed(JobKey jobKey, CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public Task JobsResumed(string jobGroup, CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public Task SchedulerError(string msg, SchedulerException cause,
        CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public Task SchedulerInStandbyMode(CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public async Task SchedulerStarted(CancellationToken cancellationToken = new())
    {
        _scheduler ??= await _schedulerFactory.GetScheduler(cancellationToken);
        _threadPoolSize = (await _scheduler.GetMetaData(cancellationToken)).ThreadPoolSize;

        if (_threadPoolSize > 0)
        {
            _gauge = QuartzNodeMetrics.Default.CreateObservableGauge("active_job_load", () =>
            {
                var executingThreads = _scheduler.GetCurrentlyExecutingJobs(cancellationToken).Result.Count;
                return (int)(Math.Round(executingThreads / (double)_threadPoolSize, 2) * 100);
            }, "percent", "The percentage of total jobs in the thread pool currently in execution.");
        }
    }

    public Task SchedulerStarting(CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public Task SchedulerShutdown(CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public Task SchedulerShuttingdown(CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public Task SchedulingDataCleared(CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }
}