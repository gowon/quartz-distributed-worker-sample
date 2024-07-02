namespace QuartzNode.Modules;

extern alias QuartzPreRelease;
using AppAny.Quartz.EntityFrameworkCore.Migrations;
using Carter;
using Core.Quartz.EFCore;
using Extensions;
using MassTransit;
using Messaging;
using Microsoft.EntityFrameworkCore;
using QuartzPreRelease::Quartz.Impl.AdoJobStore;

public class OrchestratorModule : ICarterModule
{
    private readonly ILogger<OrchestratorModule> _logger;

    public OrchestratorModule(ILogger<OrchestratorModule> logger)
    {
        _logger = logger;
    }

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/orchestrator")
            .WithTags("Orchestrator")
            .AddEndpointFilterForFeature(FeatureFlags.OrchestratorMode);

        group.MapGet("/nodes", async (QuartzDbContext context, CancellationToken cancellationToken) =>
        {
            var states = await context.Set<QuartzSchedulerState>().ToListAsync(cancellationToken);
            return Results.Ok(states.Select(state => new
            {
                state.InstanceName,
                LastCheckInTime = new DateTime(state.LastCheckInTime),
                CheckInInterval = new TimeSpan(state.CheckInInterval)
            }));
        });

        group.MapDelete("/jobs/{fireInstanceId}",
            async (string fireInstanceId, IPublishEndpoint publishEndpoint, CancellationToken cancellationToken) =>
            {
                _logger.LogInformation("Publishing message to terminate Job ({FireInstanceId})", fireInstanceId);
                await publishEndpoint.Publish(new TerminateJobMessage(fireInstanceId), cancellationToken);
                return Results.Ok();
            });

        group.MapPost("/jobs/delete-random",
            async (QuartzDbContext context, IPublishEndpoint publishEndpoint, CancellationToken cancellationToken) =>
            {
                // get random job
                var firedTrigger = await context.Set<QuartzFiredTrigger>()
                    .Where(trigger => trigger.State.Equals(AdoConstants.StateExecuting))
                    .OrderBy(trigger => Guid.NewGuid())
                    .FirstOrDefaultAsync(cancellationToken);

                if (firedTrigger == null)
                {
                    _logger.LogInformation("Could not find a running job to terminate.");
                    return Results.Ok();
                }

                _logger.LogInformation("Publishing message to terminate Job ({FireInstanceId})", firedTrigger.EntryId);
                await publishEndpoint.Publish(new TerminateJobMessage(firedTrigger.EntryId), cancellationToken);
                return Results.Json(firedTrigger.EntryId);
            });
    }
}