namespace Core.Quartz.Jobs;

using Extensions;
using global::Quartz;
using Microsoft.Extensions.Logging;

[QuartzJobProvider(nameof(RegisterJob))]
public class HelloWorldJob : IJob
{
    public static readonly JobKey Key = new("hello-world", "samples");

    private readonly ILogger<HelloWorldJob> _logger;

    public HelloWorldJob(ILogger<HelloWorldJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Hello world!");
        return Task.CompletedTask;
    }

    public static void RegisterJob(IServiceCollectionQuartzConfigurator configurator)
    {
        configurator.AddJob<HelloWorldJob>(Key, jobConfigurator =>
        {
            jobConfigurator
                .WithDescription("A simple 'Hello World' job.")
                .StoreDurably();
        });
    }
}