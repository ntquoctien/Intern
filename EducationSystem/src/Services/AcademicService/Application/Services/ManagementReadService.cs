using AcademicService.Application.DTOs.Management;
using AcademicService.Application.Interfaces;
using AcademicService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AcademicService.Application.Services;

public sealed class ManagementReadService(
    AcademicDbContext dbContext,
    IIdentityUserClient identityUserClient) : IManagementReadService
{
    public async Task<ManagementDashboardDto> GetDashboardAsync(CancellationToken cancellationToken)
    {
        var students = dbContext.Students.AsNoTracking().Where(item => !item.IsDeleted);
        var teachers = dbContext.TeacherFaculties.AsNoTracking().Where(item => !item.IsDeleted);
        var subjects = dbContext.Subjects.AsNoTracking().Where(item => !item.IsDeleted);
        var classes = dbContext.SubjectTeachings.AsNoTracking().Where(item => !item.IsDeleted);
        var schedules = dbContext.SubjectSchedules.AsNoTracking().Where(item => !item.IsDeleted);
        var distributionRows = await students
            .GroupBy(student => student.Major.Faculty != null ? student.Major.Faculty.Name : "Chưa có khoa")
            .Select(group => new { Name = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);
        var distribution = distributionRows.OrderByDescending(group => group.Count)
            .Select(group => new NamedCountDto(group.Name, group.Count)).ToList();
        return new ManagementDashboardDto(
            await students.CountAsync(cancellationToken),
            await teachers.CountAsync(cancellationToken),
            await subjects.CountAsync(cancellationToken),
            await classes.CountAsync(cancellationToken),
            await schedules.CountAsync(cancellationToken),
            await dbContext.Attendances.AsNoTracking().CountAsync(cancellationToken),
            distribution);
    }

    public async Task<IReadOnlyList<ManagementStudentSummaryDto>> GetStudentsAsync(CancellationToken cancellationToken)
    {
        var rows = await StudentQuery().OrderBy(student => student.StudentCode).ToListAsync(cancellationToken);
        var users = await identityUserClient.GetManyAsync(rows.Select(row => row.UserId), cancellationToken);
        return rows.Select(row => new ManagementStudentSummaryDto(
            row.StudentId, row.StudentCode, users.GetValueOrDefault(row.UserId)?.FullName,
            row.MajorCode, row.MajorName, row.FacultyName, row.AcademicYearName,
            row.RawStudyStatus, row.IsGraduated, row.HasIssue)).ToList();
    }

    public async Task<ManagementStudentSummaryDto?> GetStudentAsync(Guid id, CancellationToken cancellationToken)
    {
        var row = await StudentQuery().SingleOrDefaultAsync(student => student.StudentId == id, cancellationToken);
        if (row is null) return null;
        var user = await identityUserClient.GetAsync(row.UserId, cancellationToken);
        return new ManagementStudentSummaryDto(row.StudentId, row.StudentCode, user?.FullName,
            row.MajorCode, row.MajorName, row.FacultyName, row.AcademicYearName,
            row.RawStudyStatus, row.IsGraduated, row.HasIssue);
    }

    public async Task<IReadOnlyList<TeacherSummaryDto>> GetTeachersAsync(CancellationToken cancellationToken)
    {
        var rows = await dbContext.TeacherFaculties.AsNoTracking()
            .Where(teacher => !teacher.IsDeleted)
            .OrderBy(teacher => teacher.Faculty.Name)
            .Select(teacher => new
            {
                teacher.Id, teacher.UserId, FacultyCode = teacher.Faculty.Code,
                FacultyName = teacher.Faculty.Name, teacher.IsHeadOfFaculty,
                ClassCount = teacher.SubjectTeachingTeachers.Count(link => !link.IsDeleted && !link.SubjectTeaching.IsDeleted),
                SubjectNames = teacher.SubjectTeachingTeachers
                    .Where(link => !link.IsDeleted && !link.SubjectTeaching.IsDeleted)
                    .Select(link => link.SubjectTeaching.Subject.Name).Distinct().OrderBy(name => name).ToList()
            }).ToListAsync(cancellationToken);
        var users = await identityUserClient.GetManyAsync(rows.Select(row => row.UserId), cancellationToken);
        return rows.Select(row => new TeacherSummaryDto(row.Id, row.UserId,
            users.GetValueOrDefault(row.UserId)?.FullName, row.FacultyCode, row.FacultyName,
            row.IsHeadOfFaculty, row.ClassCount, row.SubjectNames)).ToList();
    }

    public async Task<IReadOnlyList<ManagementPlanSummaryDto>> GetPlansAsync(CancellationToken cancellationToken) =>
        await dbContext.SemesterPlans.AsNoTracking()
            .Where(plan => !plan.IsDeleted)
            .OrderByDescending(plan => plan.AcademicYear.Year).ThenBy(plan => plan.Major.Code).ThenBy(plan => plan.Semester)
            .Select(plan => new ManagementPlanSummaryDto(plan.Id, plan.Major.Code, plan.Major.Name,
                plan.AcademicYear.Name, plan.Semester, plan.StartDate, plan.EndDate, plan.IsActive,
                plan.SemesterSubjects.Count(subject => !subject.IsDeleted)))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<SubjectTeachingSummaryDto>> GetClassesAsync(CancellationToken cancellationToken)
    {
        var rows = await dbContext.SubjectTeachings.AsNoTracking()
            .Where(item => !item.IsDeleted && !item.Subject.IsDeleted)
            .OrderByDescending(item => item.StartDate)
            .Select(item => new
            {
                item.Id, item.Name, SubjectId = item.Subject.Id, item.Subject.SubjectCode,
                SubjectName = item.Subject.Name,
                FacultyName = item.Subject.Faculty != null ? item.Subject.Faculty.Name : null,
                item.StartDate, item.EndDate, item.TotalSessions,
                DefaultRoom = item.RoomIdDefaultNavigation != null ? item.RoomIdDefaultNavigation.Name : null,
                StudentCount = item.SubjectStudents.Count,
                TeacherUserIds = item.SubjectTeachingTeachers
                    .Where(link => !link.IsDeleted && link.Teacher != null && !link.Teacher.IsDeleted)
                    .Select(link => link.Teacher!.UserId).Distinct().ToList()
            }).ToListAsync(cancellationToken);
        var users = await identityUserClient.GetManyAsync(rows.SelectMany(row => row.TeacherUserIds), cancellationToken);
        return rows.Select(row => new SubjectTeachingSummaryDto(row.Id, row.Name, row.SubjectId,
            row.SubjectCode, row.SubjectName, row.FacultyName, row.StartDate, row.EndDate,
            row.TotalSessions, row.DefaultRoom, row.StudentCount,
            row.TeacherUserIds.Select(id => users.GetValueOrDefault(id)?.FullName)
                .Where(name => name is not null).Cast<string>().ToList())).ToList();
    }

    public async Task<IReadOnlyList<ManagementScheduleDto>> GetScheduleAsync(CancellationToken cancellationToken)
    {
        var rows = await dbContext.SubjectSchedules.AsNoTracking()
            .Where(item => !item.IsDeleted && !item.SubjectTeaching.IsDeleted)
            .OrderBy(item => item.StartDateTime)
            .Select(item => new
            {
                item.Id, item.SubjectTeaching.Subject.SubjectCode,
                SubjectName = item.SubjectTeaching.Subject.Name, ClassName = item.SubjectTeaching.Name,
                item.StartDateTime, item.EndDateTime,
                RoomName = item.Room != null ? item.Room.Name : null,
                TeacherUserId = item.Teacher != null ? (Guid?)item.Teacher.UserId : null,
                item.ScheduleType
            }).ToListAsync(cancellationToken);
        var users = await identityUserClient.GetManyAsync(rows.Where(row => row.TeacherUserId.HasValue).Select(row => row.TeacherUserId!.Value), cancellationToken);
        return rows.Select(row => new ManagementScheduleDto(row.Id, row.SubjectCode, row.SubjectName,
            row.ClassName, row.StartDateTime, row.EndDateTime, row.RoomName,
            row.TeacherUserId.HasValue ? users.GetValueOrDefault(row.TeacherUserId.Value)?.FullName : null,
            row.ScheduleType)).ToList();
    }

    public async Task<IReadOnlyList<ManagementAttendanceDto>> GetAttendanceAsync(CancellationToken cancellationToken) =>
        await dbContext.Attendances.AsNoTracking()
            .OrderByDescending(item => item.SubjectSchedule.StartDateTime)
            .Select(item => new ManagementAttendanceDto(item.Id,
                item.SubjectSchedule.SubjectTeaching.Subject.SubjectCode,
                item.SubjectSchedule.SubjectTeaching.Subject.Name,
                item.SubjectSchedule.SubjectTeaching.Name,
                item.SubjectSchedule.StartDateTime, item.Status,
                item.IsFirstTypeWarning, item.IsSecondTypeWarning))
            .ToListAsync(cancellationToken);

    private IQueryable<StudentProjection> StudentQuery() =>
        dbContext.Students.AsNoTracking().Where(student => !student.IsDeleted)
            .Select(student => new StudentProjection(student.Id, student.UserId, student.Nickname ?? "",
                student.Major.Code, student.Major.Name,
                student.Major.Faculty != null ? student.Major.Faculty.Name : null,
                student.AcademicYear.Name, student.StudyStatus, student.IsGraduated, student.HasIssue));

    private sealed record StudentProjection(Guid StudentId, Guid UserId, string StudentCode,
        string MajorCode, string MajorName, string? FacultyName, string AcademicYearName,
        int? RawStudyStatus, bool IsGraduated, bool? HasIssue);
}
