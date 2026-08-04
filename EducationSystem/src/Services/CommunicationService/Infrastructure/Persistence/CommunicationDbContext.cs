using System;
using System.Collections.Generic;
using CommunicationService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommunicationService.Infrastructure.Persistence;

public partial class CommunicationDbContext : DbContext
{
    public CommunicationDbContext(DbContextOptions<CommunicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<FormRequest> FormRequests { get; set; }

    public virtual DbSet<FormTemplate> FormTemplates { get; set; }

    public virtual DbSet<UserAnnouncement> UserAnnouncements { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_CI_AS");

        modelBuilder.Entity<FormRequest>(entity =>
        {
            entity.ToTable("FormRequests", "communication", table =>
                table.HasCheckConstraint(
                    "CK_FormRequests_EmployerVerifiedStatus",
                    "[EmployerVerifiedStatus] IN (0, 1, 2)"));

            entity.HasIndex(e => e.ApprovalId, "IX_FormRequests_ApprovalId");

            entity.HasIndex(e => e.FormTemplateId, "IX_FormRequests_FormTemplateId");

            entity.HasIndex(e => e.StudentId, "IX_FormRequests_StudentId");

            entity.HasIndex(e => e.EmployerToken, "UX_FormRequests_EmployerToken")
                .IsUnique()
                .HasFilter("[EmployerToken] IS NOT NULL");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.EmployerToken)
                .HasColumnName("EmployerToken")
                .HasColumnType("uniqueidentifier");

            entity.Property(e => e.EmployerVerifiedStatus)
                .HasColumnName("EmployerVerifiedStatus")
                .HasDefaultValue(0);

            entity.Property(e => e.VerificationData)
                .HasColumnName("VerificationData")
                .HasColumnType("nvarchar(max)");

            entity.HasOne(d => d.FormTemplate).WithMany(p => p.FormRequests).HasForeignKey(d => d.FormTemplateId);
        });

        modelBuilder.Entity<FormTemplate>(entity =>
        {
            entity.ToTable("FormTemplates", "communication");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<UserAnnouncement>(entity =>
        {
            entity.ToTable("UserAnnouncements", "communication");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
