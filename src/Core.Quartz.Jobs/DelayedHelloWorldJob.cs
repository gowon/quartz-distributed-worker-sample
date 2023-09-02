namespace Core.Quartz.Jobs;

using System.Security.Cryptography;
using Extensions;
using global::Quartz;
using Microsoft.Extensions.Logging;

[QuartzJobProvider(nameof(RegisterJob))]
public class DelayedHelloWorldJob : IJob
{
    public static readonly JobKey Key = new("delayed-hello-world", "samples");
    public static readonly string MaxDelayMsParameter = "max-delay-ms";

    private readonly ILogger<DelayedHelloWorldJob> _logger;

    public DelayedHelloWorldJob(ILogger<DelayedHelloWorldJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (!context.MergedJobDataMap.TryGetIntValue(MaxDelayMsParameter, out var maxDelay) || maxDelay == default)
        {
            maxDelay = 1000;
        }

        var delay = RandomNumberGenerator.GetInt32(100, maxDelay);

        try
        {
            await Task.Delay(delay, context.CancellationToken);
            _logger.LogInformation("I waited {DelayMs}ms to say Hello world!", delay);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("I tried to wait too long ({DelayMs}ms) and the task was cancelled...", delay);
        }
    }

    public static void RegisterJob(IServiceCollectionQuartzConfigurator configurator)
    {
        configurator.AddJob<DelayedHelloWorldJob>(Key, jobConfigurator =>
        {
            jobConfigurator
                .WithDescription("A 'Hello World' job with a random timed delay.")
                .UsingJobData(MaxDelayMsParameter, "0")
                .StoreDurably();
        });
    }
}