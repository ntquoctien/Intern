using AcademicService.Application.DTOs.StudentAccess;
using AcademicService.Application.Interfaces;
using AcademicService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AcademicService.Application.Services;

public sealed class StudentSelfService(
    AcademicDbContext dbContext,
    IIdentityUserClient identityUserClient,
    TimeProvider timeProvider) : IStudentSelfService
{
    public async Task<StudentProfileDto?> GetProfileAsync(
        Guid studentId,
        string studentCode,
        CancellationToken cancellationToken)
    {
        var academic = await dbContext.Students.AsNoTracking()
            .Where(student => student.Id == studentId && !student.IsDeleted)
            .Select(student => new
            {
                student.Id,
                student.UserId,
                student.StudyStatus,
                student.IsGraduated,
                student.HasIssue,
                MajorCode = student.Major.Code,
                MajorName = student.Major.Name,
                FacultyCode = student.Major.Faculty != null ? student.Major.Faculty.Code : null,
                FacultyName = student.Major.Faculty != null ? student.Major.Faculty.Name : null,
                AcademicYearName = student.AcademicYear.Name,
                AcademicYear = student.AcademicYear.Year
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (academic is null)
        {
            return null;
        }

        var user = await identityUserClient.GetAsync(academic.UserId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return null;
        }

        return new StudentProfileDto(
            academic.Id,
            academic.UserId,
            studentCode,
            user.FullName,
            user.UserName,
            user.ProfilePicUrl,
            academic.MajorCode,
            academic.MajorName,
            academic.FacultyCode,
            academic.FacultyName,
            academic.AcademicYearName,
            academic.AcademicYear,
            academic.StudyStatus,
            academic.IsGraduated,
            academic.HasIssue);
    }

    public Task<StudentProgramDto?> GetProgramAsync(Guid studentId, CancellationToken cancellationToken) =>
        dbContext.Students.AsNoTracking()
            .Where(student => student.Id == studentId && !student.IsDeleted)
            .Select(student => new StudentProgramDto(
                student.Major.Code,
                student.Major.Name,
                student.AcademicYear.Name,
                student.Major.SemesterPlans
                    .Where(plan => !plan.IsDeleted && plan.AcademicYearId == student.AcademicYearId)
                    .OrderBy(plan => plan.Semester)
                    .Select(plan => new StudentSemesterPlanDto(
                        plan.Id,
                        plan.Semester,
                        plan.StartDate,
                        plan.EndDate,
                        plan.IsActive,
                        plan.SemesterSubjects
                            .Where(subject => !subject.IsDeleted)
                            .OrderBy(subject => subject.SubjectCode)
                            .Select(subject => new StudentPlanSubjectDto(
                                subject.Id,
                                subject.SubjectId,
                                subject.SubjectCode,
                                subject.SubjectName,
                                subject.CreditPoint))
                            .ToList()))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyList<StudentSubjectDto>> GetSubjectsAsync(
        Guid studentId,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var items = await dbContext.SubjectStudents.AsNoTracking()
            .Where(enrollment => enrollment.StudentId == studentId &&
                !enrollment.Student.IsDeleted &&
                !enrollment.SubjectTeaching.IsDeleted &&
                !enrollment.SubjectTeaching.Subject.IsDeleted)
            .OrderBy(enrollment => enrollment.SubjectTeaching.StartDate)
            .Select(enrollment => new
            {
                EnrollmentId = enrollment.Id,
                TeachingId = enrollment.SubjectTeaching.Id,
                SubjectId = enrollment.SubjectTeaching.Subject.Id,
                enrollment.SubjectTeaching.Subject.SubjectCode,
                SubjectName = enrollment.SubjectTeaching.Subject.Name,
                enrollment.SubjectTeaching.Subject.CreditPoint,
                ClassName = enrollment.SubjectTeaching.Name,
                enrollment.SubjectTeaching.StartDate,
                enrollment.SubjectTeaching.EndDate,
                enrollment.SubjectTeaching.TotalSessions,
                DefaultRoom = enrollment.SubjectTeaching.RoomIdDefaultNavigation != null
                    ? enrollment.SubjectTeaching.RoomIdDefaultNavigation.Name
                    : null
            })
            .ToListAsync(cancellationToken);
        return items.Select(item => new StudentSubjectDto(
            item.EnrollmentId,
            item.TeachingId,
            item.SubjectId,
            item.SubjectCode,
            item.SubjectName,
            item.CreditPoint,
            item.ClassName,
            item.StartDate,
            item.EndDate,
            item.TotalSessions,
            item.DefaultRoom,
            item.StartDate > now ? "Sắp diễn ra" : item.EndDate < now ? "Đã kết thúc theo thời gian" : "Đang trong thời gian học"))
            .ToList();
    }

    public async Task<IReadOnlyList<StudentScheduleItemDto>> GetScheduleAsync(
        Guid studentId,
        CancellationToken cancellationToken)
    {
        var items = await dbContext.SubjectStudents.AsNoTracking()
            .Where(enrollment => enrollment.StudentId == studentId && !enrollment.Student.IsDeleted)
            .SelectMany(enrollment => enrollment.SubjectTeaching.SubjectSchedules
                .Where(schedule => !schedule.IsDeleted)
                .Select(schedule => new
                {
                    ScheduleId = schedule.Id,
                    TeachingId = enrollment.SubjectTeachingId,
                    SubjectCode = enrollment.SubjectTeaching.Subject.SubjectCode,
                    SubjectName = enrollment.SubjectTeaching.Subject.Name,
                    ClassName = enrollment.SubjectTeaching.Name,
                    schedule.StartDateTime,
                    schedule.EndDateTime,
                    schedule.ScheduleType,
                    RoomName = schedule.Room != null ? schedule.Room.Name : null,
                    TeacherUserId = schedule.Teacher != null ? (Guid?)schedule.Teacher.UserId : null,
                    schedule.Note
                }))
            .OrderBy(schedule => schedule.StartDateTime)
            .ToListAsync(cancellationToken);
        var users = await identityUserClient.GetManyAsync(
            items.Where(item => item.TeacherUserId.HasValue).Select(item => item.TeacherUserId!.Value),
            cancellationToken);
        return items.Select(item => new StudentScheduleItemDto(
            item.ScheduleId,
            item.TeachingId,
            item.SubjectCode,
            item.SubjectName,
            item.ClassName,
            item.StartDateTime,
            item.EndDateTime,
            item.ScheduleType,
            item.RoomName,
            item.TeacherUserId.HasValue && users.TryGetValue(item.TeacherUserId.Value, out var user) ? user.FullName : null,
            item.Note))
            .ToList();
    }

    public async Task<IReadOnlyList<StudentAttendanceDto>> GetAttendanceAsync(
        Guid studentId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Attendances.AsNoTracking()
            .Where(attendance => attendance.StudentId == studentId && !attendance.Student.IsDeleted)
            .OrderByDescending(attendance => attendance.SubjectSchedule.StartDateTime)
            .Select(attendance => new StudentAttendanceDto(
                attendance.Id,
                attendance.SubjectScheduleId,
                attendance.SubjectSchedule.SubjectTeaching.Subject.SubjectCode,
                attendance.SubjectSchedule.SubjectTeaching.Subject.Name,
                attendance.SubjectSchedule.SubjectTeaching.Name,
                attendance.SubjectSchedule.StartDateTime,
                attendance.SubjectSchedule.EndDateTime,
                attendance.Status,
                attendance.Notes,
                attendance.CreationDate,
                attendance.IsFirstTypeWarning,
                attendance.IsSecondTypeWarning))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StudentEvaluationDto>> GetEvaluationsAsync(
        Guid studentId,
        CancellationToken cancellationToken)
    {
        return await dbContext.StudentEvaluations.AsNoTracking()
            .Where(evaluation => evaluation.StudentId == studentId && !evaluation.IsDeleted)
            .OrderByDescending(evaluation => evaluation.CreationDate)
            .Select(evaluation => new StudentEvaluationDto(
                evaluation.Id,
                evaluation.SubjectTeachingId,
                evaluation.SubjectTeaching != null ? evaluation.SubjectTeaching.Subject.SubjectCode : null,
                evaluation.SubjectTeaching != null ? evaluation.SubjectTeaching.Subject.Name : null,
                evaluation.SubjectTeaching != null ? evaluation.SubjectTeaching.Name : null,
                evaluation.SemesterPlan != null ? evaluation.SemesterPlan.Semester : null,
                evaluation.SubjectTeachingExamId,
                evaluation.QuestionId,
                evaluation.TeacherName,
                evaluation.Type,
                evaluation.Comment,
                evaluation.TotalScore,
                evaluation.CreationDate,
                evaluation.UpdatedDate,
                evaluation.StudentEvaluationDetails
                    .Where(detail => !detail.IsDeleted)
                    .OrderBy(detail => detail.Id)
                    .Select(detail => new StudentEvaluationCriterionDto(
                        detail.Id,
                        detail.EvaluationName ?? (detail.EvaluationCriteria != null ? detail.EvaluationCriteria.Name : null),
                        detail.StudentScore,
                        detail.Score))
                    .ToList()))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StudentDocumentDto>> GetDocumentsAsync(
        Guid studentId,
        CancellationToken cancellationToken)
    {
        var documents = await dbContext.SubjectDocuments.AsNoTracking()
            .Where(document => dbContext.SubjectStudents.Any(enrollment =>
                enrollment.StudentId == studentId &&
                !enrollment.Student.IsDeleted &&
                !enrollment.SubjectTeaching.IsDeleted &&
                enrollment.SubjectTeaching.SubjectId == document.SubjectId))
            .OrderByDescending(document => document.UpdateDate)
            .Select(document => new
            {
                document.Id,
                document.SubjectId,
                document.Subject.SubjectCode,
                SubjectName = document.Subject.Name,
                document.Type,
                document.Name,
                document.Detail,
                document.Url,
                document.CreationDate,
                document.UpdateDate
            })
            .ToListAsync(cancellationToken);

        return documents.Select(document => new StudentDocumentDto(
            document.Id,
            document.SubjectId,
            document.SubjectCode,
            document.SubjectName,
            document.Type,
            document.Name,
            document.Detail,
            Uri.TryCreate(document.Url, UriKind.Absolute, out var uri) &&
            string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
                ? uri.AbsoluteUri
                : null,
            document.CreationDate,
            document.UpdateDate))
            .ToList();
    }

    public async Task<IReadOnlyList<StudentTuitionDto>> GetTuitionsAsync(
        Guid studentId,
        CancellationToken cancellationToken)
    {
        return await dbContext.SemesterTuitions.AsNoTracking()
            .Where(tuition => tuition.StudentId == studentId &&
                !tuition.IsDeleted &&
                !tuition.SemesterPlan.IsDeleted)
            .OrderByDescending(tuition => tuition.SemesterPlan.StartDate)
            .Select(tuition => new StudentTuitionDto(
                tuition.Id,
                tuition.SemesterPlanId,
                tuition.SemesterPlan.Semester,
                tuition.SemesterPlan.AcademicYear.Name,
                tuition.SemesterPlan.StartDate,
                tuition.SemesterPlan.EndDate,
                tuition.SemesterPlan.IsActive,
                tuition.Amount,
                tuition.PaidDate))
            .ToListAsync(cancellationToken);
    }
}
