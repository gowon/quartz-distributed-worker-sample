namespace QuartzNode.Controllers;

using AppAny.Quartz.EntityFrameworkCore.Migrations;
using Core.Quartz.EFCore;
using Fortify.QuartzNode.Messaging;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quartz.Impl.AdoJobStore;

[ApiController]
[Route("[controller]")]
public class SchedulerController : ControllerBase
{
    private readonly QuartzDbContext _context;
    private readonly ILogger<SchedulerController> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public SchedulerController(ILogger<SchedulerController> logger, QuartzDbContext context,
        IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    [Route("instances")]
    public async Task<IActionResult> GetClusteredSchedulerInstances(CancellationToken cancellationToken)
    {
        var schedulerStates = await _context.Set<QuartzSchedulerState>().ToListAsync(cancellationToken);
        return new OkObjectResult(schedulerStates.Select(state => new
        {
            state.InstanceName,
            LastCheckInTime = new DateTime(state.LastCheckInTime),
            CheckInInterval = new TimeSpan(state.CheckInInterval)
        }));
    }

    [HttpDelete]
    [Route("jobs/{fireInstanceId}")]
    public async Task<IActionResult> TerminateJob(string fireInstanceId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Publishing message to terminate Job ({FireInstanceId})", fireInstanceId);
        await _publishEndpoint.Publish(new TerminateJobMessage(fireInstanceId), cancellationToken);
        return Ok();
    }

    [HttpDelete]
    [Route("jobs")]
    public async Task<IActionResult> TerminateRandomJob(CancellationToken cancellationToken)
    {
        // get random job
        var firedTrigger = await _context.Set<QuartzFiredTrigger>()
            .Where(trigger => trigger.State.Equals(AdoConstants.StateExecuting))
            .OrderBy(trigger => Guid.NewGuid())
            .FirstOrDefaultAsync(cancellationToken);

        if (firedTrigger == null)
        {
            _logger.LogInformation("Could not find a running job to terminate.");
            return Ok();
        }

        _logger.LogInformation("Publishing message to terminate Job ({FireInstanceId})", firedTrigger.EntryId);
        await _publishEndpoint.Publish(new TerminateJobMessage(firedTrigger.EntryId), cancellationToken);
        return new OkObjectResult(firedTrigger.EntryId);
    }
}