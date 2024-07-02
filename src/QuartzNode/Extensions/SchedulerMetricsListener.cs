namespace QuartzNode.Extensions;

extern alias QuartzPreRelease;
using System.Diagnostics.Metrics;
using QuartzPreRelease::Quartz;

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

    public ValueTask JobScheduled(ITrigger trigger, CancellationToken cancellationToken = new())
    {
        return new ValueTask(Task.CompletedTask);
    }

    public ValueTask JobUnscheduled(TriggerKey triggerKey, CancellationToken cancellationToken = new())
    {
        return new ValueTask(Task.CompletedTask);
    }

    public ValueTask TriggerFinalized(ITrigger trigger, CancellationToken cancellationToken = new())
    {
        return new ValueTask(Task.CompletedTask);
    }

    public ValueTask TriggerPaused(TriggerKey triggerKey, CancellationToken cancellationToken = new())
    {
        return new ValueTask(Task.CompletedTask);
    }

    public ValueTask TriggersPaused(string? triggerGroup, CancellationToken cancellationToken = new())
    {
        return new ValueTask(Task.CompletedTask);
    }

    public ValueTask TriggerResumed(TriggerKey triggerKey, CancellationToken cancellationToken = new())
    {
        return new ValueTask(Task.CompletedTask);
    }

    public ValueTask TriggersResumed(string? triggerGroup, CancellationToken cancellationToken = new())
    {
        return new ValueTask(Task.CompletedTask);
    }

    public ValueTask JobAdded(IJobDetail jobDetail, CancellationToken cancellationToken = new())
    {
        return new ValueTask(Task.CompletedTask);
    }

    public ValueTask JobDeleted(JobKey jobKey, CancellationToken cancellationToken = new())
    {
        return new ValueTask(Task.CompletedTask);
    }

    public ValueTask JobPaused(JobKey jobKey, CancellationToken cancellationToken = new())
    {
        return new ValueTask(Task.CompletedTask);
    }

    public ValueTask JobInterrupted(JobKey jobKey, CancellationToken cancellationToken = new())
    {
        return new ValueTask(Task.CompletedTask);
    }

    public ValueTask JobsPaused(string jobGroup, CancellationToken cancellationToken = new())
    {
        return new ValueTask(Task.CompletedTask);
    }

    public ValueTask JobResumed(JobKey jobKey, CancellationToken cancellationToken = new())
    {
        return new ValueTask(Task.CompletedTask);
    }

    public ValueTask JobsResumed(string jobGroup, CancellationToken cancellationToken = new())
    {
        return new ValueTask(Task.CompletedTask);
    }

    public ValueTask SchedulerError(string msg, SchedulerException cause,
        CancellationToken cancellationToken = new())
    {
        return new ValueTask(Task.CompletedTask);
    }

    public ValueTask SchedulerInStandbyMode(CancellationToken cancellationToken = new())
    {
        return new ValueTask(Task.CompletedTask);
    }

    public async ValueTask SchedulerStarted(CancellationToken cancellationToken = new())
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

    public ValueTask SchedulerStarting(CancellationToken cancellationToken = new())
    {
        return new ValueTask(Task.CompletedTask);
    }

    public ValueTask SchedulerShutdown(CancellationToken cancellationToken = new())
    {
        return new ValueTask(Task.CompletedTask);
    }

    public ValueTask SchedulerShuttingdown(CancellationToken cancellationToken = new())
    {
        return new ValueTask(Task.CompletedTask);
    }

    public ValueTask SchedulingDataCleared(CancellationToken cancellationToken = new())
    {
        return new ValueTask(Task.CompletedTask);
    }
}