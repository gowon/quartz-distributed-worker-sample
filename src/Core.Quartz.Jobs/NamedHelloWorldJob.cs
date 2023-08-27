namespace Core.Quartz.Jobs;

using Extensions;
using global::Quartz;
using Microsoft.Extensions.Logging;

[QuartzJob("named-hello-world", "samples", "A 'Hello World' job with an configurable name.")]
[QuartzJobData("name", "")]
public class NamedHelloWorldJob : IJob
{
    public static readonly JobKey Key = new("named-hello-world", "samples");

    private readonly ILogger<NamedHelloWorldJob> _logger;

    public NamedHelloWorldJob(ILogger<NamedHelloWorldJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        if (context.MergedJobDataMap.TryGetValue("name", out var name) && !string.IsNullOrWhiteSpace(name.ToString()))
        {
            _logger.LogInformation("Hello, {Name}!", name);
        }
        else
        {
            _logger.LogInformation("Hello there, you didn't provide a name!");
        }

        return Task.CompletedTask;
    }
}