using BodyStack.Server.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace BodyStack.Server.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<FitatuSession> FitatuSessions => Set<FitatuSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FitatuSession>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.FitatuUserId).IsUnique();
            entity.Property(x => x.FitatuUserId).IsRequired();
            entity.Property(x => x.TokenProtected).IsRequired();
            entity.Property(x => x.RefreshTokenProtected).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
        });
    }
}
