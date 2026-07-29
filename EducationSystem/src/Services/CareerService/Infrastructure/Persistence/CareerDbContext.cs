using System;
using System.Collections.Generic;
using CareerService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareerService.Infrastructure.Persistence;

public partial class CareerDbContext : DbContext
{
    public virtual DbSet<ApprovedOutcomeJsonDocument> ApprovedOutcomeJsonDocuments { get; set; }

    public CareerDbContext(DbContextOptions<CareerDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CloPloMapping> CloPloMappings { get; set; }

    public virtual DbSet<CourseLearningOutcome> CourseLearningOutcomes { get; set; }

    public virtual DbSet<CurriculumVersion> CurriculumVersions { get; set; }

    public virtual DbSet<OutcomeDocumentBlock> OutcomeDocumentBlocks { get; set; }

    public virtual DbSet<OutcomeImportBatch> OutcomeImportBatches { get; set; }

    public virtual DbSet<OutcomeReviewLog> OutcomeReviewLogs { get; set; }

    public virtual DbSet<ProgramLearningOutcome> ProgramLearningOutcomes { get; set; }

    public virtual DbSet<ProgressionLevel> ProgressionLevels { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_CI_AS");

        modelBuilder.Entity<ApprovedOutcomeJsonDocument>(entity =>
        {
            entity.ToTable("ApprovedOutcomeJsonDocument", "career");

            entity.HasIndex(e => e.ImportBatchId, "UQ_ApprovedOutcomeJsonDocument_ImportBatchId").IsUnique();
            entity.HasIndex(
                    e => new { e.CurriculumVersionId, e.DocumentVersion },
                    "UQ_ApprovedOutcomeJsonDocument_CurriculumVersionId_DocumentVersion")
                .IsUnique();
            entity.HasIndex(
                e => e.CurriculumVersionId,
                "UQ_ApprovedOutcomeJsonDocument_Active")
                .IsUnique()
                .HasFilter("([Status]=N'Active')");
            entity.HasIndex(
                e => new { e.CurriculumVersionId, e.ContentHash },
                "IX_ApprovedOutcomeJsonDocument_CurriculumVersionId_ContentHash");

            entity.Property(e => e.ContentHash)
                .HasMaxLength(64)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.GeneratedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.GeneratedByExternalId).HasMaxLength(100);
            entity.Property(e => e.GeneratedByName).HasMaxLength(200);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.SchemaVersion).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(20);

            entity.HasOne(d => d.CurriculumVersion)
                .WithMany(p => p.ApprovedOutcomeJsonDocuments)
                .HasForeignKey(d => d.CurriculumVersionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApprovedOutcomeJsonDocument_CurriculumVersion");

            entity.HasOne(d => d.ImportBatch)
                .WithOne(p => p.ApprovedOutcomeJsonDocument)
                .HasForeignKey<ApprovedOutcomeJsonDocument>(d => d.ImportBatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ApprovedOutcomeJsonDocument_OutcomeImportBatch");
        });

        modelBuilder.Entity<CloPloMapping>(entity =>
        {
            entity.ToTable("CloPloMapping", "career");

            entity.HasIndex(e => e.CloId, "IX_CloPloMapping_CloId");

            entity.HasIndex(e => e.CurriculumVersionId, "IX_CloPloMapping_CurriculumVersionId");

            entity.HasIndex(e => new { e.CurriculumVersionId, e.ProgressionLevelCode }, "IX_CloPloMapping_CurriculumVersionId_ProgressionLevelCode");

            entity.HasIndex(e => e.PloId, "IX_CloPloMapping_PloId");

            entity.HasIndex(e => new { e.PloId, e.ProgressionLevelCode }, "IX_CloPloMapping_PloId_ProgressionLevelCode");

            entity.HasIndex(e => e.ProgressionLevelCode, "IX_CloPloMapping_ProgressionLevelCode");

            entity.HasIndex(e => new { e.CurriculumVersionId, e.CloId, e.PloId }, "UQ_CloPloMapping_CurriculumVersionId_CloId_PloId").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Note).HasMaxLength(1000);
            entity.Property(e => e.ProgressionLevelCode)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CurriculumVersion).WithMany(p => p.CloPloMappings)
                .HasForeignKey(d => d.CurriculumVersionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CloPloMapping_CurriculumVersion");

            entity.HasOne(d => d.ProgressionLevelCodeNavigation).WithMany(p => p.CloPloMappings)
                .HasForeignKey(d => d.ProgressionLevelCode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CloPloMapping_ProgressionLevel");

            entity.HasOne(d => d.CourseLearningOutcome).WithMany(p => p.CloPloMappings)
                .HasPrincipalKey(p => new { p.Id, p.CurriculumVersionId })
                .HasForeignKey(d => new { d.CloId, d.CurriculumVersionId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CloPloMapping_CourseLearningOutcome");

            entity.HasOne(d => d.ProgramLearningOutcome).WithMany(p => p.CloPloMappings)
                .HasPrincipalKey(p => new { p.Id, p.CurriculumVersionId })
                .HasForeignKey(d => new { d.PloId, d.CurriculumVersionId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CloPloMapping_ProgramLearningOutcome");
        });

        modelBuilder.Entity<CourseLearningOutcome>(entity =>
        {
            entity.ToTable("CourseLearningOutcome", "career");

            entity.HasIndex(e => e.CurriculumVersionId, "IX_CourseLearningOutcome_CurriculumVersionId");

            entity.HasIndex(e => new { e.CurriculumVersionId, e.Status }, "IX_CourseLearningOutcome_CurriculumVersionId_Status");

            entity.HasIndex(e => new { e.CurriculumVersionId, e.SubjectCode }, "IX_CourseLearningOutcome_CurriculumVersionId_SubjectCode");

            entity.HasIndex(e => e.Status, "IX_CourseLearningOutcome_Status");

            entity.HasIndex(e => e.SubjectCode, "IX_CourseLearningOutcome_SubjectCode");

            entity.HasIndex(e => new { e.CurriculumVersionId, e.SubjectCode, e.CloCode }, "UQ_CourseLearningOutcome_CurriculumVersionId_SubjectCode_CloCode").IsUnique();

            entity.HasIndex(e => new { e.Id, e.CurriculumVersionId }, "UQ_CourseLearningOutcome_Id_CurriculumVersionId").IsUnique();

            entity.Property(e => e.CloCode).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Draft");
            entity.Property(e => e.SubjectCode).HasMaxLength(100);
            entity.Property(e => e.SubjectName).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CurriculumVersion).WithMany(p => p.CourseLearningOutcomes)
                .HasForeignKey(d => d.CurriculumVersionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CourseLearningOutcome_CurriculumVersion");
        });

        modelBuilder.Entity<CurriculumVersion>(entity =>
        {
            entity.ToTable("CurriculumVersion", "career");

            entity.HasIndex(e => new { e.MajorCode, e.CurriculumCode, e.Version }, "UQ_CurriculumVersion_MajorCode_CurriculumCode_Version").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CurriculumCode).HasMaxLength(100);
            entity.Property(e => e.CurriculumName).HasMaxLength(255);
            entity.Property(e => e.MajorCode).HasMaxLength(100);
            entity.Property(e => e.MajorName).HasMaxLength(255);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Draft");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Version).HasMaxLength(50);
        });

        modelBuilder.Entity<OutcomeDocumentBlock>(entity =>
        {
            entity.ToTable("OutcomeDocumentBlock", "career");

            entity.HasIndex(e => new { e.ImportBatchId, e.BlockType }, "IX_OutcomeDocumentBlock_ImportBatchId_BlockType");

            entity.HasIndex(e => new { e.ImportBatchId, e.BlockId }, "UQ_OutcomeDocumentBlock_ImportBatchId_BlockId").IsUnique();

            entity.HasIndex(e => new { e.ImportBatchId, e.Sequence }, "UQ_OutcomeDocumentBlock_ImportBatchId_Sequence").IsUnique();

            entity.Property(e => e.BlockId).HasMaxLength(100);
            entity.Property(e => e.BlockType).HasMaxLength(30);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.ImportBatch).WithMany(p => p.OutcomeDocumentBlocks)
                .HasForeignKey(d => d.ImportBatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OutcomeDocumentBlock_OutcomeImportBatch");
        });

        modelBuilder.Entity<OutcomeImportBatch>(entity =>
        {
            entity.ToTable("OutcomeImportBatch", "career");

            entity.HasIndex(e => e.CreatedAt, "IX_OutcomeImportBatch_CreatedAt").IsDescending();

            entity.HasIndex(e => e.CurriculumVersionId, "IX_OutcomeImportBatch_CurriculumVersionId");

            entity.HasIndex(e => new { e.CurriculumVersionId, e.FileHash }, "IX_OutcomeImportBatch_CurriculumVersionId_FileHash");

            entity.HasIndex(e => new { e.CurriculumVersionId, e.SelectedSubjectCode }, "IX_OutcomeImportBatch_CurriculumVersionId_SelectedSubjectCode").HasFilter("([SelectedSubjectCode] IS NOT NULL)");

            entity.HasIndex(e => e.Status, "IX_OutcomeImportBatch_Status");

            entity.HasIndex(e => new { e.Status, e.CreatedAt }, "IX_OutcomeImportBatch_Status_CreatedAt").IsDescending(false, true);

            entity.Property(e => e.ApprovedByExternalId).HasMaxLength(100);
            entity.Property(e => e.ApprovedByName).HasMaxLength(200);
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.ErrorCode).HasMaxLength(100);
            entity.Property(e => e.ErrorMessage).HasMaxLength(2000);
            entity.Property(e => e.FileHash)
                .HasMaxLength(64)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ModelName).HasMaxLength(100);
            entity.Property(e => e.OriginalFileName).HasMaxLength(255);
            entity.Property(e => e.ProcessingStage).HasMaxLength(40);
            entity.Property(e => e.PromptVersion).HasMaxLength(50);
            entity.Property(e => e.Provider).HasMaxLength(50);
            entity.Property(e => e.ReviewedByExternalId).HasMaxLength(100);
            entity.Property(e => e.ReviewedByName).HasMaxLength(200);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.SelectedSubjectCode).HasMaxLength(100);
            entity.Property(e => e.SelectedSubjectName).HasMaxLength(255);
            entity.Property(e => e.Status).HasMaxLength(30);
            entity.Property(e => e.StorageKey).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.UploadedByExternalId).HasMaxLength(100);
            entity.Property(e => e.UploadedByName).HasMaxLength(200);

            entity.HasOne(d => d.CurriculumVersion).WithMany(p => p.OutcomeImportBatches)
                .HasForeignKey(d => d.CurriculumVersionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OutcomeImportBatch_CurriculumVersion");
        });

        modelBuilder.Entity<OutcomeReviewLog>(entity =>
        {
            entity.ToTable("OutcomeReviewLog", "career");

            entity.HasIndex(e => new { e.ActorExternalId, e.CreatedAt }, "IX_OutcomeReviewLog_ActorExternalId_CreatedAt").IsDescending(false, true);

            entity.HasIndex(e => new { e.ImportBatchId, e.CreatedAt }, "IX_OutcomeReviewLog_ImportBatchId_CreatedAt").IsDescending(false, true);

            entity.HasIndex(e => new { e.ImportBatchId, e.EntityDraftId }, "IX_OutcomeReviewLog_ImportBatchId_EntityDraftId");

            entity.Property(e => e.Action).HasMaxLength(30);
            entity.Property(e => e.ActorExternalId).HasMaxLength(100);
            entity.Property(e => e.ActorName).HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.EntityDraftId).HasMaxLength(100);
            entity.Property(e => e.EntityType).HasMaxLength(30);
            entity.Property(e => e.FieldName).HasMaxLength(100);
            entity.Property(e => e.Note).HasMaxLength(1000);

            entity.HasOne(d => d.ImportBatch).WithMany(p => p.OutcomeReviewLogs)
                .HasForeignKey(d => d.ImportBatchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_OutcomeReviewLog_OutcomeImportBatch");
        });

        modelBuilder.Entity<ProgramLearningOutcome>(entity =>
        {
            entity.ToTable("ProgramLearningOutcome", "career");

            entity.HasIndex(e => e.CurriculumVersionId, "IX_ProgramLearningOutcome_CurriculumVersionId");

            entity.HasIndex(e => new { e.CurriculumVersionId, e.Status }, "IX_ProgramLearningOutcome_CurriculumVersionId_Status");

            entity.HasIndex(e => e.Status, "IX_ProgramLearningOutcome_Status");

            entity.HasIndex(e => new { e.CurriculumVersionId, e.PloCode }, "UQ_ProgramLearningOutcome_CurriculumVersionId_PloCode").IsUnique();

            entity.HasIndex(e => new { e.Id, e.CurriculumVersionId }, "UQ_ProgramLearningOutcome_Id_CurriculumVersionId").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.PloCode).HasMaxLength(50);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Draft");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.CurriculumVersion).WithMany(p => p.ProgramLearningOutcomes)
                .HasForeignKey(d => d.CurriculumVersionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProgramLearningOutcome_CurriculumVersion");
        });

        modelBuilder.Entity<ProgressionLevel>(entity =>
        {
            entity.HasKey(e => e.Code);

            entity.ToTable("ProgressionLevel", "career");

            entity.HasIndex(e => e.Rank, "UQ_ProgressionLevel_Rank").IsUnique();

            entity.Property(e => e.Code)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.VietnameseName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
