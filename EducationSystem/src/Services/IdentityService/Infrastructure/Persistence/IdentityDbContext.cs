using Microsoft.EntityFrameworkCore;
using IdentityService.Infrastructure.Persistence.Entities;

namespace IdentityService.Infrastructure.Persistence;

public sealed class IdentityDbContext : DbContext
{
    public const string Schema = "identity";

    public DbSet<IdentityUser> Users => Set<IdentityUser>();

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<IdentityUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.UserName).HasMaxLength(100).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(200).IsRequired();
            entity.Property(user => user.CreatedAtUtc).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}