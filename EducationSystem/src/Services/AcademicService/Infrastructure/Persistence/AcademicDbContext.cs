using System;
using System.Collections.Generic;
using AcademicService.Infrastructure.Persistence.Entities;
using EducationSystem.Services.Academic.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AcademicService.Infrastructure.Persistence;

public partial class AcademicDbContext : DbContext
{
    public AcademicDbContext(DbContextOptions<AcademicDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AcademicYear> AcademicYears { get; set; }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<EvaluationCriteria> EvaluationCriterias { get; set; }

    public virtual DbSet<Faculty> Faculties { get; set; }

    public virtual DbSet<Major> Majors { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<SemesterPlan> SemesterPlans { get; set; }

    public virtual DbSet<SemesterSubject> SemesterSubjects { get; set; }

    public virtual DbSet<SemesterTuition> SemesterTuitions { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentInternship> StudentInternships { get; set; }

    public virtual DbSet<StudentProject> StudentProjects { get; set; }

    public virtual DbSet<StudentEvaluation> StudentEvaluations { get; set; }

    public virtual DbSet<StudentEvaluationDetail> StudentEvaluationDetails { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<SubjectDocument> SubjectDocuments { get; set; }

    public virtual DbSet<SubjectSchedule> SubjectSchedules { get; set; }

    public virtual DbSet<SubjectSpecialNote> SubjectSpecialNotes { get; set; }

    public virtual DbSet<SubjectStudent> SubjectStudents { get; set; }

    public virtual DbSet<SubjectTeaching> SubjectTeachings { get; set; }

    public virtual DbSet<SubjectTeachingTeacher> SubjectTeachingTeachers { get; set; }

    public virtual DbSet<TeacherFaculty> TeacherFaculties { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_CI_AS");

        modelBuilder.Entity<AcademicYear>(entity =>
        {
            entity.ToTable("AcademicYears", "academic");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.ToTable("Attendances", "academic");

            entity.HasIndex(e => e.CreatedById, "IX_Attendances_CreatedById");

            entity.HasIndex(e => new { e.StudentId, e.SubjectScheduleId }, "IX_Attendances_StudentId_SubjectScheduleId").IsUnique();

            entity.HasIndex(e => e.SubjectScheduleId, "IX_Attendances_SubjectScheduleId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Student).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.SubjectSchedule).WithMany(p => p.Attendances).HasForeignKey(d => d.SubjectScheduleId);
        });

        modelBuilder.Entity<EvaluationCriteria>(entity =>
        {
            entity.ToTable("EvaluationCriterias", "academic");

            entity.HasIndex(e => e.ParentId, "IX_EvaluationCriterias_ParentId");

            entity.HasIndex(e => e.QuestionId, "IX_EvaluationCriterias_QuestionId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Score).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Parent).WithMany(p => p.InverseParent).HasForeignKey(d => d.ParentId);
        });

        modelBuilder.Entity<Faculty>(entity =>
        {
            entity.ToTable("Faculties", "academic");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<Major>(entity =>
        {
            entity.ToTable("Majors", "academic");

            entity.HasIndex(e => e.FacultyId, "IX_Majors_FacultyId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Faculty).WithMany(p => p.Majors).HasForeignKey(d => d.FacultyId);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.ToTable("Rooms", "academic");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<SemesterPlan>(entity =>
        {
            entity.ToTable("SemesterPlans", "academic");

            entity.HasIndex(e => e.AcademicYearId, "IX_SemesterPlans_AcademicYearId");

            entity.HasIndex(e => e.MajorId, "IX_SemesterPlans_MajorId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.AcademicYear).WithMany(p => p.SemesterPlans)
                .HasForeignKey(d => d.AcademicYearId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Major).WithMany(p => p.SemesterPlans)
                .HasForeignKey(d => d.MajorId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SemesterSubject>(entity =>
        {
            entity.ToTable("SemesterSubjects", "academic");

            entity.HasIndex(e => e.SemesterPlanId, "IX_SemesterSubjects_SemesterPlanId");

            entity.HasIndex(e => e.SubjectId, "IX_SemesterSubjects_SubjectId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.SemesterPlan).WithMany(p => p.SemesterSubjects)
                .HasForeignKey(d => d.SemesterPlanId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Subject).WithMany(p => p.SemesterSubjects)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SemesterTuition>(entity =>
        {
            entity.ToTable("SemesterTuitions", "academic");

            entity.HasIndex(e => e.SemesterPlanId, "IX_SemesterTuitions_SemesterPlanId");

            entity.HasIndex(e => e.StudentId, "IX_SemesterTuitions_StudentId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.SemesterPlan).WithMany(p => p.SemesterTuitions)
                .HasForeignKey(d => d.SemesterPlanId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Student).WithMany(p => p.SemesterTuitions)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Students", "academic");

            entity.HasIndex(e => e.AcademicYearId, "IX_Students_AcademicYearId");

            entity.HasIndex(e => e.MajorId, "IX_Students_MajorId");

            entity.HasIndex(e => e.RelativeUserId, "IX_Students_RelativeUserId");

            entity.HasIndex(e => e.UserId, "IX_Students_UserId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.AcademicYear).WithMany(p => p.Students)
                .HasForeignKey(d => d.AcademicYearId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Major).WithMany(p => p.Students)
                .HasForeignKey(d => d.MajorId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<StudentProject>(entity =>
        {
            entity.ToTable("StudentProjects", "academic", table =>
                table.HasCheckConstraint("CK_StudentProjects_TeamSize", "[TeamSize] >= 1"));

            entity.HasKey(e => e.ProjectId)
                .HasName("PK_StudentProjects");

            entity.HasIndex(e => e.StudentId, "IX_StudentProjects_StudentID");

            entity.HasIndex(e => e.MappedCourseId, "IX_StudentProjects_MappedCourseID");

            entity.Property(e => e.ProjectId)
                .HasColumnName("ProjectID")
                .UseIdentityColumn();

            entity.Property(e => e.StudentId)
                .HasColumnName("StudentID");

            entity.Property(e => e.ProjectName)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.TechStack)
                .HasMaxLength(255)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(e => e.ProjectDescription)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.SourceCodeUrl)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.Property(e => e.TeamSize)
                .HasDefaultValue(1);

            entity.Property(e => e.MyRole)
                .HasMaxLength(100);

            entity.Property(e => e.MyContributions)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.MappedCourseId)
                .HasColumnName("MappedCourseID");

            entity.HasOne<Student>()
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_StudentProjects_Students_StudentID");

            entity.HasOne<Subject>()
                .WithMany()
                .HasForeignKey(e => e.MappedCourseId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("FK_StudentProjects_Subjects_MappedCourseID");
        });

        modelBuilder.Entity<StudentInternship>(entity =>
        {
            entity.ToTable("StudentInternships", "academic", table =>
                table.HasCheckConstraint(
                    "CK_StudentInternships_DateRange",
                    "[EndDate] IS NULL OR [EndDate] >= [StartDate]"));

            entity.HasKey(e => e.InternshipId)
                .HasName("PK_StudentInternships");

            entity.HasIndex(e => e.StudentId, "IX_StudentInternships_StudentID");

            entity.HasIndex(e => e.FormRequestId, "IX_StudentInternships_FormRequestID");

            entity.Property(e => e.InternshipId)
                .HasColumnName("InternshipID")
                .UseIdentityColumn();

            entity.Property(e => e.StudentId)
                .HasColumnName("StudentID");

            entity.Property(e => e.CompanyName)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.Position)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.StartDate)
                .HasColumnType("date");

            entity.Property(e => e.EndDate)
                .HasColumnType("date");

            entity.Property(e => e.TaskDescription)
                .HasColumnType("nvarchar(max)");

            entity.Property(e => e.FormRequestId)
                .HasColumnName("FormRequestID")
                .HasColumnType("uniqueidentifier");

            entity.HasOne<Student>()
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_StudentInternships_Students_StudentID");
        });

        modelBuilder.Entity<StudentEvaluation>(entity =>
        {
            entity.ToTable("StudentEvaluations", "academic");

            entity.HasIndex(e => e.QuestionId, "IX_StudentEvaluations_QuestionId");

            entity.HasIndex(e => e.SemesterPlanId, "IX_StudentEvaluations_SemesterPlanId");

            entity.HasIndex(e => e.StudentId, "IX_StudentEvaluations_StudentId");

            entity.HasIndex(e => e.SubjectTeachingExamId, "IX_StudentEvaluations_SubjectTeachingExamId");

            entity.HasIndex(e => e.SubjectTeachingId, "IX_StudentEvaluations_SubjectTeachingId");

            entity.HasIndex(e => e.TeacherId, "IX_StudentEvaluations_TeacherId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.TotalScore).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.SemesterPlan).WithMany(p => p.StudentEvaluations).HasForeignKey(d => d.SemesterPlanId);

            entity.HasOne(d => d.Student).WithMany(p => p.StudentEvaluations)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.SubjectTeaching).WithMany(p => p.StudentEvaluations).HasForeignKey(d => d.SubjectTeachingId);

            entity.HasOne(d => d.Teacher).WithMany(p => p.StudentEvaluations).HasForeignKey(d => d.TeacherId);
        });

        modelBuilder.Entity<StudentEvaluationDetail>(entity =>
        {
            entity.ToTable("StudentEvaluationDetails", "academic");

            entity.HasIndex(e => e.EvaluationCriteriaId, "IX_StudentEvaluationDetails_EvaluationCriteriaId");

            entity.HasIndex(e => e.StudentEvaluationId, "IX_StudentEvaluationDetails_StudentEvaluationId");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Score).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.StudentScore).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.EvaluationCriteria).WithMany(p => p.StudentEvaluationDetails).HasForeignKey(d => d.EvaluationCriteriaId);

            entity.HasOne(d => d.StudentEvaluation).WithMany(p => p.StudentEvaluationDetails).HasForeignKey(d => d.StudentEvaluationId);
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.ToTable("Subjects", "academic");

            entity.HasIndex(e => e.FacultyId, "IX_Subjects_FacultyId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Faculty).WithMany(p => p.Subjects).HasForeignKey(d => d.FacultyId);
        });

        modelBuilder.Entity<SubjectDocument>(entity =>
        {
            entity.ToTable("SubjectDocuments", "academic");

            entity.HasIndex(e => e.SubjectId, "IX_SubjectDocuments_SubjectId");

            entity.HasIndex(e => e.UserId, "IX_SubjectDocuments_UserId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Subject).WithMany(p => p.SubjectDocuments).HasForeignKey(d => d.SubjectId);
        });

        modelBuilder.Entity<SubjectSchedule>(entity =>
        {
            entity.ToTable("SubjectSchedules", "academic");

            entity.HasIndex(e => e.RoomId, "IX_SubjectSchedules_RoomId");

            entity.HasIndex(e => new { e.SubjectTeachingId, e.StartDateTime, e.EndDateTime }, "IX_SubjectSchedules_SubjectTeachingId_StartDateTime_EndDateTime").IsUnique();

            entity.HasIndex(e => e.TeacherId, "IX_SubjectSchedules_TeacherId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Room).WithMany(p => p.SubjectSchedules).HasForeignKey(d => d.RoomId);

            entity.HasOne(d => d.SubjectTeaching).WithMany(p => p.SubjectSchedules)
                .HasForeignKey(d => d.SubjectTeachingId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Teacher).WithMany(p => p.SubjectSchedules)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<SubjectSpecialNote>(entity =>
        {
            entity.ToTable("SubjectSpecialNotes", "academic");

            entity.HasIndex(e => e.CreatedById, "IX_SubjectSpecialNotes_CreatedById");

            entity.HasIndex(e => e.StudentId, "IX_SubjectSpecialNotes_StudentId");

            entity.HasIndex(e => e.SubjectId, "IX_SubjectSpecialNotes_SubjectId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Student).WithMany(p => p.SubjectSpecialNotes)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Subject).WithMany(p => p.SubjectSpecialNotes)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SubjectStudent>(entity =>
        {
            entity.ToTable("SubjectStudents", "academic");

            entity.HasIndex(e => e.StudentId, "IX_SubjectStudents_StudentId");

            entity.HasIndex(e => e.SubjectTeachingId, "IX_SubjectStudents_SubjectTeachingId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Student).WithMany(p => p.SubjectStudents)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.SubjectTeaching).WithMany(p => p.SubjectStudents)
                .HasForeignKey(d => d.SubjectTeachingId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SubjectTeaching>(entity =>
        {
            entity.ToTable("SubjectTeachings", "academic");

            entity.HasIndex(e => e.RoomIdDefault, "IX_SubjectTeachings_RoomIdDefault");

            entity.HasIndex(e => e.SubjectId, "IX_SubjectTeachings_SubjectId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.RoomIdDefaultNavigation).WithMany(p => p.SubjectTeachings).HasForeignKey(d => d.RoomIdDefault);

            entity.HasOne(d => d.Subject).WithMany(p => p.SubjectTeachings)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<SubjectTeachingTeacher>(entity =>
        {
            entity.ToTable("SubjectTeachingTeachers", "academic");

            entity.HasIndex(e => e.SubjectTeachingId, "IX_SubjectTeachingTeachers_SubjectTeachingId");

            entity.HasIndex(e => e.TeacherId, "IX_SubjectTeachingTeachers_TeacherId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.SubjectTeaching).WithMany(p => p.SubjectTeachingTeachers)
                .HasForeignKey(d => d.SubjectTeachingId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Teacher).WithMany(p => p.SubjectTeachingTeachers).HasForeignKey(d => d.TeacherId);
        });

        modelBuilder.Entity<TeacherFaculty>(entity =>
        {
            entity.ToTable("TeacherFaculties", "academic");

            entity.HasIndex(e => e.FacultyId, "IX_TeacherFaculties_FacultyId");

            entity.HasIndex(e => e.UserId, "IX_TeacherFaculties_UserId");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.Faculty).WithMany(p => p.TeacherFaculties)
                .HasForeignKey(d => d.FacultyId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
