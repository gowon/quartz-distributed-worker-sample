namespace Core.Quartz.EFCore;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<QuartzDbContext>
{
    public QuartzDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<QuartzDbContext>()
            .UseNpgsql();

        return new QuartzDbContext(builder.Options);
    }
}