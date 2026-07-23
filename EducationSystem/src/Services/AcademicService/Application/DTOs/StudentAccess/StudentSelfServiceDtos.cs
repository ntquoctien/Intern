namespace AcademicService.Application.DTOs.StudentAccess;

public sealed record StudentProfileDto(
    Guid StudentId,
    Guid UserId,
    string StudentCode,
    string FullName,
    string UserName,
    string? ProfilePicUrl,
    string MajorCode,
    string MajorName,
    string? FacultyCode,
    string? FacultyName,
    string AcademicYearName,
    int AcademicYear,
    int? StudyStatus,
    bool IsGraduated,
    bool? HasIssue);

public sealed record StudentProgramDto(
    string MajorCode,
    string MajorName,
    string AcademicYearName,
    IReadOnlyList<StudentSemesterPlanDto> SemesterPlans);

public sealed record StudentSemesterPlanDto(
    Guid Id,
    int Semester,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    IReadOnlyList<StudentPlanSubjectDto> Subjects);

public sealed record StudentPlanSubjectDto(
    Guid Id,
    Guid? SubjectId,
    string SubjectCode,
    string SubjectName,
    int CreditPoint);

public sealed record StudentSubjectDto(
    Guid EnrollmentId,
    Guid SubjectTeachingId,
    Guid SubjectId,
    string SubjectCode,
    string SubjectName,
    int CreditPoint,
    string ClassName,
    DateTime StartDate,
    DateTime EndDate,
    int TotalSessions,
    string? DefaultRoom,
    string SchedulePhase);

public sealed record StudentScheduleItemDto(
    Guid ScheduleId,
    Guid SubjectTeachingId,
    string SubjectCode,
    string SubjectName,
    string ClassName,
    DateTime StartDateTime,
    DateTime EndDateTime,
    int? ScheduleType,
    string? RoomName,
    string? TeacherName,
    string Note);

public sealed record StudentAttendanceDto(
    Guid AttendanceId,
    Guid ScheduleId,
    string SubjectCode,
    string SubjectName,
    string ClassName,
    DateTime StartDateTime,
    DateTime EndDateTime,
    int RawStatus,
    string Notes,
    DateTime CreationDate,
    bool? IsFirstTypeWarning,
    bool? IsSecondTypeWarning);

public sealed record StudentEvaluationDto(
    Guid EvaluationId,
    Guid? SubjectTeachingId,
    string? SubjectCode,
    string? SubjectName,
    string? ClassName,
    int? Semester,
    Guid? SubjectTeachingExamId,
    Guid? QuestionId,
    string? TeacherName,
    int RawType,
    string? Comment,
    decimal? RawTotalScore,
    DateTime CreationDate,
    DateTime? UpdatedDate,
    IReadOnlyList<StudentEvaluationCriterionDto> Criteria);

public sealed record StudentEvaluationCriterionDto(
    Guid DetailId,
    string? SnapshotName,
    decimal? StudentScore,
    decimal? RawMaximumScore);

public sealed record StudentDocumentDto(
    Guid DocumentId,
    Guid SubjectId,
    string SubjectCode,
    string SubjectName,
    int RawType,
    string Name,
    string Detail,
    string? SafeUrl,
    DateTime CreationDate,
    DateTime UpdateDate);

public sealed record StudentTuitionDto(
    Guid TuitionId,
    Guid SemesterPlanId,
    int Semester,
    string AcademicYearName,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActivePlan,
    decimal? RawAmount,
    DateTime? PaidDate);

public sealed record SafeIdentityUserDto(
    Guid UserId,
    string FullName,
    string UserName,
    string? ProfilePicUrl,
    bool IsActive);
