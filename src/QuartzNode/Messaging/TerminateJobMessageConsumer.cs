namespace Fortify.QuartzNode.Messaging;

using global::Quartz;
using MassTransit;

public record TerminateJobMessage(string FireInstanceId);

public class TerminateJobMessageConsumer : IConsumer<TerminateJobMessage>
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ILogger<TerminateJobMessageConsumer> _logger;

    public TerminateJobMessageConsumer(ISchedulerFactory schedulerFactory, ILogger<TerminateJobMessageConsumer> logger)
    {
        _schedulerFactory = schedulerFactory;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TerminateJobMessage> context)
    {
        var message = context.Message;
        var scheduler = await _schedulerFactory.GetScheduler(context.CancellationToken);

        _logger.LogInformation("Attempting to interrupt Job ({FireInstanceId})", message.FireInstanceId);
        var success = await scheduler.Interrupt(message.FireInstanceId, context.CancellationToken);
        if (success)
        {
            _logger.LogInformation("Successfully interrupted Job ({FireInstanceId})", message.FireInstanceId);
        }
    }
}