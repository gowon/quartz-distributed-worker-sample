namespace QuartzNode.Extensions;

using Core.Quartz.EFCore;
using global::Extensions.Hosting.AsyncInitialization;
using Microsoft.EntityFrameworkCore;

public class QuartzJobStoreInitializer : IAsyncInitializer
{
    private readonly QuartzDbContext _context;
    private readonly ILogger<QuartzJobStoreInitializer> _logger;

    public QuartzJobStoreInitializer(QuartzDbContext context, ILogger<QuartzJobStoreInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        var connectionString = _context.Database.GetConnectionString();
        _logger.LogDebug("Starting database migration: '{ConnectionString}'", connectionString);
        await _context.Database.MigrateAsync(cancellationToken);
        _logger.LogDebug("Database migration completed");
    }
}