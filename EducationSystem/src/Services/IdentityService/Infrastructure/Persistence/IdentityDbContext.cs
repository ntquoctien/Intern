using System;
using System.Collections.Generic;
using IdentityService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Infrastructure.Persistence;

public partial class IdentityDbContext : DbContext
{
    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<PasswordReset> PasswordResets { get; set; }

    public virtual DbSet<Setting> Settings { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserDevice> UserDevices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_CI_AS");

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs", "identity");

            entity.HasIndex(e => e.CreationDate, "IX_AuditLogs_CreationDate");

            entity.HasIndex(e => e.RecordId, "IX_AuditLogs_RecordId");

            entity.HasIndex(e => e.UserId, "IX_AuditLogs_UserId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<PasswordReset>(entity =>
        {
            entity.ToTable("PasswordResets", "identity");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Setting>(entity =>
        {
            entity.ToTable("Settings", "identity");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users", "identity");

            entity.HasIndex(e => e.FullName, "IX_Users_FullName");

            entity.HasIndex(e => e.Mobile, "IX_Users_Mobile");

            entity.HasIndex(e => e.UserInternalId, "IX_Users_UserInternalId");

            entity.HasIndex(e => e.UserName, "IX_Users_UserName");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<UserDevice>(entity =>
        {
            entity.ToTable("UserDevices", "identity");

            entity.HasIndex(e => e.UserId, "IX_UserDevices_UserId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.User).WithMany(p => p.UserDevices).HasForeignKey(d => d.UserId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
