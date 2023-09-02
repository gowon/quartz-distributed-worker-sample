namespace Core.Quartz.Jobs;

using Extensions;
using global::Quartz;
using Microsoft.Extensions.Logging;

[QuartzJobProvider(nameof(RegisterJob))]
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

    public static void RegisterJob(IServiceCollectionQuartzConfigurator configurator)
    {
        configurator.AddJob<NamedHelloWorldJob>(Key, jobConfigurator =>
        {
            jobConfigurator
                .WithDescription("A 'Hello World' job with an configurable name.")
                .UsingJobData("name", string.Empty)
                .StoreDurably();
        });
    }
}