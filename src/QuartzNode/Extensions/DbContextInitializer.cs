namespace QuartzNode.Extensions;

using global::Extensions.Hosting.AsyncInitialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;

public class DbContextInitializer<TContext> : IAsyncInitializer
    where TContext : DbContext
{
    private readonly TContext _context;
    private readonly IFeatureManager _featureManager;
    private readonly ILogger<DbContextInitializer<TContext>> _logger;

    public DbContextInitializer(TContext context, ILogger<DbContextInitializer<TContext>> logger,
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