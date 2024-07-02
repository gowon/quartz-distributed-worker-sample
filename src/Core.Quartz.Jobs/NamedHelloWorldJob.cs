namespace Core.Quartz.Jobs;

extern alias QuartzPreRelease;
using QuartzPreRelease::Quartz;
using Extensions;
using Microsoft.Extensions.Logging;

[QuartzJobProvider(nameof(RegisterJob))]
public class NamedHelloWorldJob : IJob
{
    public static readonly JobKey Key = new("named-hello-world", "samples");
    public static readonly string NameParameter = "name";

    private readonly ILogger<NamedHelloWorldJob> _logger;

    public NamedHelloWorldJob(ILogger<NamedHelloWorldJob> logger)
    {
        _logger = logger;
    }

    public ValueTask Execute(IJobExecutionContext context)
    {
        if (context.MergedJobDataMap.TryGetValue("name", out var name) && !string.IsNullOrWhiteSpace(name.ToString()))
        {
            _logger.LogInformation("Hello, {Name}!", name);
        }
        else
        {
            _logger.LogInformation("Hello there, you didn't provide a name!");
        }

        return new ValueTask(Task.CompletedTask);
    }

    public static void RegisterJob(IServiceCollectionQuartzConfigurator configurator)
    {
        configurator.AddJob<NamedHelloWorldJob>(Key, jobConfigurator =>
        {
            jobConfigurator
                .WithDescription("A 'Hello World' job with an configurable name.")
                .UsingJobData(NameParameter, string.Empty)
                .StoreDurably();
        });
    }
}