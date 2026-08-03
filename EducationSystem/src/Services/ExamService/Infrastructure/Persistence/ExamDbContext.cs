using Microsoft.EntityFrameworkCore;
using ExamService.Infrastructure.Persistence.Entities;

namespace ExamService.Infrastructure.Persistence;

public sealed class ExamDbContext : DbContext
{
    public const string Schema = "exam";

    public DbSet<ExamQuestionSuite> QuestionSuites => Set<ExamQuestionSuite>();

    public ExamDbContext(DbContextOptions<ExamDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.Entity<ExamQuestionSuite>(entity =>
        {
            entity.ToTable("QuestionSuites");
            entity.HasKey(questionSuite => questionSuite.Id);
            entity.Property(questionSuite => questionSuite.Code).HasMaxLength(50).IsRequired();
            entity.Property(questionSuite => questionSuite.Title).HasMaxLength(200).IsRequired();
            entity.Property(questionSuite => questionSuite.CreatedAtUtc).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}