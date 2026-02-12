using BodyStack.Server.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace BodyStack.Server.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<FitatuSession> FitatuSessions => Set<FitatuSession>();
    public DbSet<MonthDaySummary> MonthDaySummaries => Set<MonthDaySummary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FitatuSession>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.FitatuUserId).IsUnique();
            // Index for GetLatestAsync query optimization
            entity.HasIndex(x => x.UpdatedAt);
            entity.Property(x => x.FitatuUserId).IsRequired();
            entity.Property(x => x.TokenProtected).IsRequired();
            entity.Property(x => x.RefreshTokenProtected).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
        });

        modelBuilder.Entity<MonthDaySummary>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.FitatuUserId, x.Date }).IsUnique();
            entity.Property(x => x.FitatuUserId).IsRequired();
            entity.Property(x => x.YearMonth).IsRequired();
            entity.Property(x => x.Date).IsRequired();
            entity.Property(x => x.Energy).HasPrecision(18, 2);
            entity.Property(x => x.Protein).HasPrecision(18, 2);
            entity.Property(x => x.Fat).HasPrecision(18, 2);
            entity.Property(x => x.Carbohydrate).HasPrecision(18, 2);
            entity.Property(x => x.Fiber).HasPrecision(18, 2);
            entity.Property(x => x.Sugars).HasPrecision(18, 2);
            entity.Property(x => x.Salt).HasPrecision(18, 2);
            entity.Property(x => x.Status).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
        });
    }
}
