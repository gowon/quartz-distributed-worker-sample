﻿namespace Core.Quartz.Jobs;

using System.Security.Cryptography;
using Extensions;
using global::Quartz;
using Microsoft.Extensions.Logging;

[QuartzJob("timed-hello-world", "samples", "A 'Hello World' job with configurable time delay.")]
[QuartzJobData("delay-ms", "")]
public class TimedHelloWorldJob : IJob
{
    public static readonly JobKey Key = new("timed-hello-world", "samples");

    private readonly ILogger<TimedHelloWorldJob> _logger;

    public TimedHelloWorldJob(ILogger<TimedHelloWorldJob> logger)
    {
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        if (!context.MergedJobDataMap.TryGetIntValue("delay-ms", out var delay) || delay == default)
        {
            delay = RandomNumberGenerator.GetInt32(100, 1000);
        }

        await Task.Delay(delay);
        _logger.LogInformation("I waited {DelayMs}ms to say Hello world!", delay);
    }
}