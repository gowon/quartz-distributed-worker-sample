namespace Core.Quartz.EFCore;

using AppAny.Quartz.EntityFrameworkCore.Migrations;
using AppAny.Quartz.EntityFrameworkCore.Migrations.PostgreSQL;
using Microsoft.EntityFrameworkCore;

public class QuartzDbContext : DbContext
{
    public QuartzDbContext(DbContextOptions<QuartzDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // add the Postgres Extension for UUID generation
        modelBuilder.HasPostgresExtension("uuid-ossp");

        modelBuilder.AddQuartz(builder =>
            builder.UsePostgreSql(schema: null));
    }
}