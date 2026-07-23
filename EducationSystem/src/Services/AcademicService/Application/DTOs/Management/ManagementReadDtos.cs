namespace AcademicService.Application.DTOs.Management;

public sealed record NamedCountDto(string Name, int Count);
public sealed record ManagementDashboardDto(int Students, int Teachers, int Subjects, int Classes, int Schedules, int AttendanceRecords, IReadOnlyList<NamedCountDto> StudentsByFaculty);
public sealed record ManagementStudentSummaryDto(Guid StudentId, Guid UserId, string StudentCode, string? FullName, string MajorCode, string MajorName, string? FacultyName, string AcademicYearName, int? RawStudyStatus, bool IsGraduated, bool? HasIssue);
public sealed record TeacherSummaryDto(Guid TeacherFacultyId, Guid UserId, string? FullName, string FacultyCode, string FacultyName, bool IsHeadOfFaculty, int ClassCount, IReadOnlyList<string> SubjectNames);
public sealed record TeacherClassDto(Guid ClassId, string ClassName, string SubjectCode, string SubjectName, DateTime StartDate, DateTime EndDate, bool? IsMainTeacher);
public sealed record TeacherScheduleItemDto(Guid ScheduleId, Guid ClassId, string ClassName, string SubjectCode, string SubjectName, DateTime StartDateTime, DateTime EndDateTime, string? RoomName);
public sealed record TeacherDetailDto(Guid TeacherFacultyId, Guid UserId, string? FullName, string? UserName, string? ProfilePicUrl, bool? IsAccountActive, Guid FacultyId, string FacultyCode, string FacultyName, bool IsHeadOfFaculty, int ClassCount, int MainClassCount, IReadOnlyList<TeacherClassDto> Classes, IReadOnlyList<TeacherScheduleItemDto> UpcomingSchedule);
public sealed record ManagementPlanSummaryDto(Guid PlanId, string MajorCode, string MajorName, string AcademicYearName, int Semester, DateTime StartDate, DateTime EndDate, bool IsActive, int SubjectCount, IReadOnlyList<Guid> SubjectIds);
public sealed record ManagementPlanSubjectDto(Guid SemesterSubjectId, Guid? SubjectId, string SnapshotCode, string SnapshotName, int SnapshotCreditPoint, string? CatalogCode, string? CatalogName, int? CatalogCreditPoint, bool CatalogMatch);
public sealed record ManagementPlanDetailDto(Guid PlanId, Guid MajorId, string MajorCode, string MajorName, string? FacultyName, Guid AcademicYearId, string AcademicYearName, int Semester, DateTime StartDate, DateTime EndDate, bool IsActive, int SubjectCount, int TotalCreditPoints, IReadOnlyList<ManagementPlanSubjectDto> Subjects);
public sealed record SubjectTeachingSummaryDto(Guid ClassId, string ClassName, Guid SubjectId, string SubjectCode, string SubjectName, string? FacultyName, DateTime StartDate, DateTime EndDate, int TotalSessions, string? DefaultRoom, int? RoomCapacity, int StudentCount, string? MainTeacherName, IReadOnlyList<string> TeacherNames);
public sealed record ClassStudentDto(Guid StudentId, string StudentCode, string? FullName, string MajorCode, string MajorName, string AcademicYearName);
public sealed record ClassTeacherDto(Guid TeacherFacultyId, Guid UserId, string? FullName, string FacultyCode, string FacultyName, bool? IsMainTeacher);
public sealed record ClassScheduleDto(Guid ScheduleId, DateTime StartDateTime, DateTime EndDateTime, string? RoomName, int? ScheduleType, int AttendanceCount);
public sealed record SubjectTeachingDetailDto(Guid ClassId, string ClassName, Guid SubjectId, string SubjectCode, string SubjectName, Guid? FacultyId, string? FacultyCode, string? FacultyName, DateTime StartDate, DateTime EndDate, int TotalSessions, Guid? DefaultRoomId, string? DefaultRoom, int? RoomCapacity, int StudentCount, int TeacherCount, int ScheduleCount, int AttendanceCount, IReadOnlyList<ClassStudentDto> Students, IReadOnlyList<ClassTeacherDto> Teachers, IReadOnlyList<ClassScheduleDto> Schedules);
public sealed record TeachingAssignmentDto(Guid AssignmentId, Guid TeacherFacultyId, Guid UserId, string? FullName, string? UserName, string? ProfilePicUrl, string FacultyCode, string FacultyName, Guid ClassId, string ClassName, Guid SubjectId, string SubjectCode, string SubjectName, DateTime StartDate, DateTime EndDate, bool? IsMainTeacher, int StudentCount, int? RoomCapacity);
public sealed record TeacherAssignmentsDetailDto(Guid TeacherFacultyId, Guid UserId, string? FullName, string? UserName, string? ProfilePicUrl, string FacultyCode, string FacultyName, int AssignmentCount, int MainAssignmentCount, int AssistantAssignmentCount, IReadOnlyList<TeachingAssignmentDto> Assignments);
public sealed record ManagementRoomDto(Guid RoomId, string Name, int NumberOfSeats, int ScheduleCount, int DefaultClassCount);
public sealed record ManagementTeachingScheduleDto(Guid ScheduleId, Guid ClassId, string ClassName, Guid SubjectId, string SubjectCode, string SubjectName, Guid? RoomId, string? RoomName, int? RoomCapacity, Guid? TeacherFacultyId, string? TeacherName, DateTime StartDateTime, DateTime EndDateTime, int? ScheduleType, string Note, int StudentCount);
public sealed record ManagementSubjectSummaryDto(Guid SubjectId, Guid? FacultyId, string? FacultyCode, string? FacultyName, string SubjectCode, string Name, int CreditPoint, int? TotalHours, bool IsActive, int PlanCount, int ClassCount, int DocumentCount, int SpecialNoteCount);
public sealed record ManagementSubjectDetailDto(Guid SubjectId, Guid? FacultyId, string? FacultyCode, string? FacultyName, string SubjectCode, string Name, int CreditPoint, int? TotalHours, string Note, bool IsActive, int PlanCount, int ClassCount, int DocumentCount, int SpecialNoteCount);
public sealed record ManagementScheduleDto(Guid ScheduleId, string SubjectCode, string SubjectName, string ClassName, DateTime StartDateTime, DateTime EndDateTime, string? RoomName, string? TeacherName, int? RawScheduleType);
public sealed record ManagementAttendanceDto(Guid AttendanceId, string SubjectCode, string SubjectName, string ClassName, DateTime StartDateTime, int RawStatus, bool? FirstWarning, bool? SecondWarning);
public sealed record ManagementAttendanceQueryDto(int PageNumber = 1, int PageSize = 10, string? Search = null, Guid? FacultyId = null, Guid? SubjectId = null, Guid? ClassId = null, Guid? RoomId = null, int? Status = null, Guid? CreatedById = null, DateTime? FromDate = null, DateTime? ToDate = null);
public sealed record AttendanceManagementItemDto(Guid AttendanceId, Guid StudentId, string StudentCode, string? StudentName, string? StudentProfilePicUrl, string MajorCode, string MajorName, string AcademicYearName, Guid ScheduleId, Guid ClassId, string ClassName, Guid SubjectId, string SubjectCode, string SubjectName, Guid? FacultyId, string? FacultyName, Guid? RoomId, string? RoomName, Guid? TeacherFacultyId, string? TeacherName, DateTime StartDateTime, DateTime EndDateTime, int RawStatus, string Notes, Guid CreatedById, string? CreatedByName, DateTime CreationDate, bool? FirstWarning, bool? SecondWarning);
public sealed record AttendanceManagementPageDto(IReadOnlyList<AttendanceManagementItemDto> Items, int PageNumber, int PageSize, int TotalItems, IReadOnlyDictionary<int, int> StatusCounts);
public sealed record StudentEvaluationManagementQueryDto(int PageNumber = 1, int PageSize = 10,
    string? Search = null, Guid? StudentId = null, Guid? ClassId = null, Guid? SemesterPlanId = null,
    Guid? TeacherId = null, int? Type = null, DateTime? FromDate = null, DateTime? ToDate = null,
    decimal? MinimumScore = null, decimal? MaximumScore = null);
public sealed record StudentEvaluationBreakdownDto(Guid DetailId, Guid? CriteriaId, string? SnapshotName,
    string? CriteriaName, decimal? StudentScore, decimal? MaximumScore, bool CriteriaResolved);
public sealed record StudentEvaluationManagementItemDto(Guid EvaluationId, Guid StudentId, Guid StudentUserId,
    string StudentCode, string? StudentName, string? StudentProfilePicUrl, string MajorName,
    string AcademicYearName, Guid? ClassId, string? ClassName, string? SubjectCode, string? SubjectName,
    Guid? SemesterPlanId, int? Semester, Guid? ExamId, Guid? QuestionId, Guid? TeacherId,
    string? TeacherName, int RawType, string? Comment, decimal? TotalScore, DateTime CreationDate,
    DateTime? UpdatedDate, IReadOnlyList<StudentEvaluationBreakdownDto> Breakdown);
public sealed record StudentEvaluationManagementPageDto(IReadOnlyList<StudentEvaluationManagementItemDto> Items,
    int PageNumber, int PageSize, int TotalItems, decimal? AverageScore, int WithCommentCount,
    int WithoutScoreCount, int StudentCount);
