namespace QuartzNode.Extensions;

using Core.Quartz.EFCore;
using global::Extensions.Hosting.AsyncInitialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;

public class QuartzJobStoreInitializer : IAsyncInitializer
{
    private readonly QuartzDbContext _context;
    private readonly IFeatureManager _featureManager;
    private readonly ILogger<QuartzJobStoreInitializer> _logger;

    public QuartzJobStoreInitializer(QuartzDbContext context, ILogger<QuartzJobStoreInitializer> logger,
        IFeatureManager featureManager)
    {
        _context = context;
        _logger = logger;
        _featureManager = featureManager;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        if (await _featureManager.IsEnabledAsync(FeatureFlags.HostMode))
        {
            _logger.LogDebug("Node running in Host Mode.");
            var connectionString = _context.Database.GetConnectionString();
            _logger.LogDebug("Starting database migration: '{ConnectionString}'", connectionString);
            await _context.Database.MigrateAsync(cancellationToken);
            _logger.LogDebug("Database migration completed");
        }
    }
}