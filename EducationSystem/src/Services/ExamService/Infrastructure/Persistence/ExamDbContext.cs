using System;
using System.Collections.Generic;
using ExamService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExamService.Infrastructure.Persistence;

public partial class ExamDbContext : DbContext
{
    public ExamDbContext(DbContextOptions<ExamDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ExamAttempt> ExamAttempts { get; set; }

    public virtual DbSet<ExamQuestionAnswer> ExamQuestionAnswers { get; set; }

    public virtual DbSet<ExamQuestionSelection> ExamQuestionSelections { get; set; }

    public virtual DbSet<ExamResult> ExamResults { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<QuestionAnswer> QuestionAnswers { get; set; }

    public virtual DbSet<QuestionSuite> QuestionSuites { get; set; }

    public virtual DbSet<SubjectTeachingExam> SubjectTeachingExams { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_CI_AS");

        modelBuilder.Entity<ExamAttempt>(entity =>
        {
            entity.ToTable("ExamAttempts", "exam");

            entity.HasIndex(e => e.StudentId, "IX_ExamAttempts_StudentId");

            entity.HasIndex(e => e.SubjectTeachingExamId, "IX_ExamAttempts_SubjectTeachingExamId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.SubjectTeachingExam).WithMany(p => p.ExamAttempts)
                .HasForeignKey(d => d.SubjectTeachingExamId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ExamQuestionAnswer>(entity =>
        {
            entity.ToTable("ExamQuestionAnswers", "exam");

            entity.HasIndex(e => e.ExamQuestionSelectionId, "IX_ExamQuestionAnswers_ExamQuestionSelectionId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.ExamQuestionSelection).WithMany(p => p.ExamQuestionAnswers).HasForeignKey(d => d.ExamQuestionSelectionId);
        });

        modelBuilder.Entity<ExamQuestionSelection>(entity =>
        {
            entity.ToTable("ExamQuestionSelections", "exam");

            entity.HasIndex(e => e.ExamAttemptId, "IX_ExamQuestionSelections_ExamAttemptId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.ExamAttempt).WithMany(p => p.ExamQuestionSelections).HasForeignKey(d => d.ExamAttemptId);
        });

        modelBuilder.Entity<ExamResult>(entity =>
        {
            entity.ToTable("ExamResults", "exam");

            entity.HasIndex(e => e.ExamAttemptId, "IX_ExamResults_ExamAttemptId");

            entity.HasIndex(e => new { e.StudentId, e.SubjectTeachingExamId }, "IX_ExamResults_StudentId_SubjectTeachingExamId");

            entity.HasIndex(e => e.SubjectTeachingExamId, "IX_ExamResults_SubjectTeachingExamId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.ExamAttempt).WithMany(p => p.ExamResults).HasForeignKey(d => d.ExamAttemptId);

            entity.HasOne(d => d.SubjectTeachingExam).WithMany(p => p.ExamResults)
                .HasForeignKey(d => d.SubjectTeachingExamId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("Questions", "exam");

            entity.HasIndex(e => e.QuestionSuiteId, "IX_Questions_QuestionSuiteId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.QuestionSuite).WithMany(p => p.Questions).HasForeignKey(d => d.QuestionSuiteId);
        });

        modelBuilder.Entity<QuestionAnswer>(entity =>
        {
            entity.ToTable("QuestionAnswers", "exam");

            entity.HasIndex(e => e.QuestionId, "IX_QuestionAnswers_QuestionId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionAnswers).HasForeignKey(d => d.QuestionId);
        });

        modelBuilder.Entity<QuestionSuite>(entity =>
        {
            entity.ToTable("QuestionSuites", "exam");

            entity.HasIndex(e => e.SubjectId, "IX_QuestionSuites_SubjectId");

            entity.HasIndex(e => e.UpdatedById, "IX_QuestionSuites_UpdatedById");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<SubjectTeachingExam>(entity =>
        {
            entity.ToTable("SubjectTeachingExams", "exam");

            entity.HasIndex(e => e.QuestionSuiteId, "IX_SubjectTeachingExams_QuestionSuiteId");

            entity.HasIndex(e => e.RoomId, "IX_SubjectTeachingExams_RoomId");

            entity.HasIndex(e => e.SubjectTeachingId, "IX_SubjectTeachingExams_SubjectTeachingId");

            entity.HasIndex(e => e.TeacherId, "IX_SubjectTeachingExams_TeacherId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.QuestionSuite).WithMany(p => p.SubjectTeachingExams).HasForeignKey(d => d.QuestionSuiteId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
