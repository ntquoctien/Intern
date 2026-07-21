namespace AcademicService.Application.DTOs.Management;

public sealed record NamedCountDto(string Name, int Count);
public sealed record ManagementDashboardDto(int Students, int Teachers, int Subjects, int Classes, int Schedules, int AttendanceRecords, IReadOnlyList<NamedCountDto> StudentsByFaculty);
public sealed record ManagementStudentSummaryDto(Guid StudentId, string StudentCode, string? FullName, string MajorCode, string MajorName, string? FacultyName, string AcademicYearName, int? RawStudyStatus, bool IsGraduated, bool? HasIssue);
public sealed record TeacherSummaryDto(Guid TeacherFacultyId, Guid UserId, string? FullName, string FacultyCode, string FacultyName, bool IsHeadOfFaculty, int ClassCount, IReadOnlyList<string> SubjectNames);
public sealed record ManagementPlanSummaryDto(Guid PlanId, string MajorCode, string MajorName, string AcademicYearName, int Semester, DateTime StartDate, DateTime EndDate, bool IsActive, int SubjectCount);
public sealed record SubjectTeachingSummaryDto(Guid ClassId, string ClassName, Guid SubjectId, string SubjectCode, string SubjectName, string? FacultyName, DateTime StartDate, DateTime EndDate, int TotalSessions, string? DefaultRoom, int StudentCount, IReadOnlyList<string> TeacherNames);
public sealed record ManagementScheduleDto(Guid ScheduleId, string SubjectCode, string SubjectName, string ClassName, DateTime StartDateTime, DateTime EndDateTime, string? RoomName, string? TeacherName, int? RawScheduleType);
public sealed record ManagementAttendanceDto(Guid AttendanceId, string SubjectCode, string SubjectName, string ClassName, DateTime StartDateTime, int RawStatus, bool? FirstWarning, bool? SecondWarning);
