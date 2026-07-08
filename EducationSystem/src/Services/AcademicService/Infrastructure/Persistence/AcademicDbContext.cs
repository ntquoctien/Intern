using Microsoft.EntityFrameworkCore;
using AcademicService.Infrastructure.Persistence.Entities;

namespace AcademicService.Infrastructure.Persistence;

public sealed class AcademicDbContext : DbContext
{
    public const string Schema = "academic";

    public DbSet<AcademicFaculty> Faculties => Set<AcademicFaculty>();

    public AcademicDbContext(DbContextOptions<AcademicDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<AcademicFaculty>(entity =>
        {
            entity.ToTable("Faculties");
            entity.HasKey(faculty => faculty.Id);
            entity.Property(faculty => faculty.Code).HasMaxLength(50).IsRequired();
            entity.Property(faculty => faculty.Name).HasMaxLength(200).IsRequired();
            entity.Property(faculty => faculty.CreatedAtUtc).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}