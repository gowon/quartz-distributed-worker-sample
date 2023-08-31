namespace QuartzNode.Extensions;

using global::Extensions.Hosting.AsyncInitialization;
using Microsoft.EntityFrameworkCore;

public class DbContextInitializer<TContext> : IAsyncInitializer
    where TContext : DbContext
{
    private readonly TContext _context;
    private readonly ILogger<DbContextInitializer<TContext>> _logger;

    public DbContextInitializer(TContext context, ILogger<DbContextInitializer<TContext>> logger)
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