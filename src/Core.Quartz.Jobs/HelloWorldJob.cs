namespace Core.Quartz.Jobs;

using global::Quartz;
using Microsoft.Extensions.Logging;

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
}