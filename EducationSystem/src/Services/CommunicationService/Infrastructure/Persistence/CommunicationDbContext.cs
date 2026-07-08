using Microsoft.EntityFrameworkCore;
using CommunicationService.Infrastructure.Persistence.Entities;

namespace CommunicationService.Infrastructure.Persistence;

public sealed class CommunicationDbContext : DbContext
{
    public const string Schema = "communication";

    public DbSet<CommunicationFormTemplate> FormTemplates => Set<CommunicationFormTemplate>();

    public CommunicationDbContext(DbContextOptions<CommunicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<CommunicationFormTemplate>(entity =>
        {
            entity.ToTable("FormTemplates");
            entity.HasKey(formTemplate => formTemplate.Id);
            entity.Property(formTemplate => formTemplate.Code).HasMaxLength(50).IsRequired();
            entity.Property(formTemplate => formTemplate.Name).HasMaxLength(200).IsRequired();
            entity.Property(formTemplate => formTemplate.CreatedAtUtc).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}