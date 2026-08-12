using AcademicService.Application.DTOs.Management;
using AcademicService.Application.Interfaces;
using AcademicService.Infrastructure.Persistence;
using AcademicService.Infrastructure.Persistence.Entities;
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
        // A full COUNT(*) scan on the attendance history can take tens of
        // seconds on the development database. The dashboard only needs a
        // current cardinality indicator, so use SQL Server's maintained row
        // count metadata instead of scanning every attendance row.
        var attendanceRows = await dbContext.Database
            .SqlQuery<long>($"""
                SELECT COALESCE(SUM(CONVERT(bigint, [rows])), 0) AS [Value]
                FROM sys.partitions
                WHERE object_id = OBJECT_ID(N'[academic].[Attendances]')
                  AND index_id IN (0, 1)
                """)
            .SingleAsync(cancellationToken);
        return new ManagementDashboardDto(
            await students.CountAsync(cancellationToken),
            await teachers.CountAsync(cancellationToken),
            await subjects.CountAsync(cancellationToken),
            await classes.CountAsync(cancellationToken),
            await schedules.CountAsync(cancellationToken),
            (int)Math.Min(attendanceRows, int.MaxValue),
            distribution);
    }

    public async Task<IReadOnlyList<ManagementStudentSummaryDto>> GetStudentsAsync(CancellationToken cancellationToken)
    {
        var rows = await StudentQuery().ToListAsync(cancellationToken);
        rows = rows.OrderBy(student => student.StudentCode).ToList();
        var users = await identityUserClient.GetManyAsync(rows.Select(row => row.UserId), cancellationToken);
        return rows.Select(row => new ManagementStudentSummaryDto(
            row.StudentId, row.UserId, row.StudentCode, users.GetValueOrDefault(row.UserId)?.FullName,
            row.MajorCode, row.MajorName, row.FacultyName, row.AcademicYearName,
            row.RawStudyStatus, row.IsGraduated, row.HasIssue)).ToList();
    }

    public async Task<ManagementStudentSummaryDto?> GetStudentAsync(Guid id, CancellationToken cancellationToken)
    {
        var row = await StudentQuery(id).SingleOrDefaultAsync(cancellationToken);
        if (row is null) return null;
        var user = await identityUserClient.GetAsync(row.UserId, cancellationToken);
        return new ManagementStudentSummaryDto(row.StudentId, row.UserId, row.StudentCode, user?.FullName,
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

    public async Task<TeacherDetailDto?> GetTeacherAsync(Guid id, CancellationToken cancellationToken)
    {
        var row = await dbContext.TeacherFaculties.AsNoTracking()
            .Where(teacher => teacher.Id == id && !teacher.IsDeleted)
            .Select(teacher => new
            {
                teacher.Id, teacher.UserId, teacher.FacultyId,
                FacultyCode = teacher.Faculty.Code, FacultyName = teacher.Faculty.Name,
                teacher.IsHeadOfFaculty,
                Classes = teacher.SubjectTeachingTeachers
                    .Where(link => !link.IsDeleted && !link.SubjectTeaching.IsDeleted && !link.SubjectTeaching.Subject.IsDeleted)
                    .OrderByDescending(link => link.SubjectTeaching.StartDate)
                    .Select(link => new TeacherClassDto(link.SubjectTeaching.Id, link.SubjectTeaching.Name,
                        link.SubjectTeaching.Subject.SubjectCode, link.SubjectTeaching.Subject.Name,
                        link.SubjectTeaching.StartDate, link.SubjectTeaching.EndDate, link.IsMainTeacher))
                    .ToList(),
                UpcomingSchedule = teacher.SubjectSchedules
                    .Where(schedule => !schedule.IsDeleted && !schedule.SubjectTeaching.IsDeleted && schedule.EndDateTime >= DateTime.Today)
                    .OrderBy(schedule => schedule.StartDateTime)
                    .Take(20)
                    .Select(schedule => new TeacherScheduleItemDto(schedule.Id, schedule.SubjectTeachingId,
                        schedule.SubjectTeaching.Name, schedule.SubjectTeaching.Subject.SubjectCode,
                        schedule.SubjectTeaching.Subject.Name, schedule.StartDateTime, schedule.EndDateTime,
                        schedule.Room != null ? schedule.Room.Name : null))
                    .ToList()
            }).SingleOrDefaultAsync(cancellationToken);
        if (row is null) return null;
        var user = await identityUserClient.GetAsync(row.UserId, cancellationToken);
        return new TeacherDetailDto(row.Id, row.UserId, user?.FullName, user?.UserName,
            user?.ProfilePicUrl, user?.IsActive, row.FacultyId, row.FacultyCode, row.FacultyName,
            row.IsHeadOfFaculty, row.Classes.Count, row.Classes.Count(item => item.IsMainTeacher == true),
            row.Classes, row.UpcomingSchedule);
    }

    public async Task<IReadOnlyList<ManagementPlanSummaryDto>> GetPlansAsync(CancellationToken cancellationToken) =>
        await dbContext.SemesterPlans.AsNoTracking()
            .Where(plan => !plan.IsDeleted)
            .OrderByDescending(plan => plan.AcademicYear.Year).ThenBy(plan => plan.Major.Code).ThenBy(plan => plan.Semester)
            .Select(plan => new ManagementPlanSummaryDto(plan.Id, plan.Major.Code, plan.Major.Name,
                plan.AcademicYear.Name, plan.Semester, plan.StartDate, plan.EndDate, plan.IsActive,
                plan.SemesterSubjects.Count(subject => !subject.IsDeleted),
                plan.SemesterSubjects.Where(subject => !subject.IsDeleted && subject.SubjectId != null).Select(subject => subject.SubjectId!.Value).ToList()))
            .ToListAsync(cancellationToken);

    public async Task<ManagementPlanDetailDto?> GetPlanAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.SemesterPlans.AsNoTracking()
            .Where(plan => plan.Id == id && !plan.IsDeleted)
            .Select(plan => new ManagementPlanDetailDto(
                plan.Id, plan.MajorId, plan.Major.Code, plan.Major.Name,
                plan.Major.Faculty != null ? plan.Major.Faculty.Name : null,
                plan.AcademicYearId, plan.AcademicYear.Name, plan.Semester,
                plan.StartDate, plan.EndDate, plan.IsActive,
                plan.SemesterSubjects.Count(subject => !subject.IsDeleted),
                plan.SemesterSubjects.Where(subject => !subject.IsDeleted).Sum(subject => subject.CreditPoint),
                plan.SemesterSubjects.Where(subject => !subject.IsDeleted)
                    .OrderBy(subject => subject.SubjectCode)
                    .Select(subject => new ManagementPlanSubjectDto(
                        subject.Id, subject.SubjectId, subject.SubjectCode, subject.SubjectName, subject.CreditPoint,
                        subject.Subject != null && !subject.Subject.IsDeleted ? subject.Subject.SubjectCode : null,
                        subject.Subject != null && !subject.Subject.IsDeleted ? subject.Subject.Name : null,
                        subject.Subject != null && !subject.Subject.IsDeleted ? subject.Subject.CreditPoint : null,
                        subject.Subject != null && !subject.Subject.IsDeleted
                            && subject.SubjectCode == subject.Subject.SubjectCode
                            && subject.SubjectName == subject.Subject.Name
                            && subject.CreditPoint == subject.Subject.CreditPoint))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

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
                RoomCapacity = item.RoomIdDefaultNavigation != null ? (int?)item.RoomIdDefaultNavigation.NumberOfSeats : null,
                StudentCount = item.SubjectStudents.Count,
                MainTeacherUserId = item.SubjectTeachingTeachers
                    .Where(link => !link.IsDeleted && link.IsMainTeacher == true && link.Teacher != null && !link.Teacher.IsDeleted)
                    .Select(link => (Guid?)link.Teacher!.UserId).FirstOrDefault(),
                TeacherUserIds = item.SubjectTeachingTeachers
                    .Where(link => !link.IsDeleted && link.Teacher != null && !link.Teacher.IsDeleted)
                    .Select(link => link.Teacher!.UserId).Distinct().ToList()
            }).ToListAsync(cancellationToken);
        var users = await identityUserClient.GetManyAsync(rows.SelectMany(row => row.TeacherUserIds), cancellationToken);
        return rows.Select(row => new SubjectTeachingSummaryDto(row.Id, row.Name, row.SubjectId,
            row.SubjectCode, row.SubjectName, row.FacultyName, row.StartDate, row.EndDate,
            row.TotalSessions, row.DefaultRoom, row.RoomCapacity, row.StudentCount,
            row.MainTeacherUserId != null ? users.GetValueOrDefault(row.MainTeacherUserId.Value)?.FullName : null,
            row.TeacherUserIds.Select(id => users.GetValueOrDefault(id)?.FullName)
                .Where(name => name is not null).Cast<string>().ToList())).ToList();
    }

    public async Task<SubjectTeachingDetailDto?> GetClassAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await dbContext.SubjectTeachings.AsNoTracking()
            .Include(value => value.Subject).ThenInclude(subject => subject.Faculty)
            .Include(value => value.RoomIdDefaultNavigation)
            .Include(value => value.SubjectStudents).ThenInclude(link => link.Student).ThenInclude(student => student.Major)
            .Include(value => value.SubjectStudents).ThenInclude(link => link.Student).ThenInclude(student => student.AcademicYear)
            .Include(value => value.SubjectTeachingTeachers).ThenInclude(link => link.Teacher).ThenInclude(teacher => teacher!.Faculty)
            .Include(value => value.SubjectSchedules).ThenInclude(schedule => schedule.Room)
            .Include(value => value.SubjectSchedules).ThenInclude(schedule => schedule.Attendances)
            .SingleOrDefaultAsync(value => value.Id == id && !value.IsDeleted, cancellationToken);
        if (item is null) return null;

        var studentLinks = item.SubjectStudents.Where(link => !link.Student.IsDeleted).ToList();
        var teacherLinks = item.SubjectTeachingTeachers.Where(link => !link.IsDeleted && link.Teacher != null && !link.Teacher.IsDeleted).ToList();
        var userIds = studentLinks.Select(link => link.Student.UserId).Concat(teacherLinks.Select(link => link.Teacher!.UserId));
        var users = await identityUserClient.GetManyAsync(userIds, cancellationToken);
        var schedules = item.SubjectSchedules.Where(schedule => !schedule.IsDeleted).OrderBy(schedule => schedule.StartDateTime).ToList();

        return new SubjectTeachingDetailDto(
            item.Id, item.Name, item.SubjectId, item.Subject.SubjectCode, item.Subject.Name,
            item.Subject.FacultyId, item.Subject.Faculty?.Code, item.Subject.Faculty?.Name,
            item.StartDate, item.EndDate, item.TotalSessions, item.RoomIdDefault,
            item.RoomIdDefaultNavigation?.Name, item.RoomIdDefaultNavigation?.NumberOfSeats,
            studentLinks.Count, teacherLinks.Count, schedules.Count,
            schedules.Sum(schedule => schedule.Attendances.Count),
            studentLinks.OrderBy(link => link.Student.Nickname).Select(link => new ClassStudentDto(
                link.Student.Id, link.Student.Nickname ?? link.Student.Id.ToString(),
                users.GetValueOrDefault(link.Student.UserId)?.FullName, link.Student.Major.Code,
                link.Student.Major.Name, link.Student.AcademicYear.Name)).ToList(),
            teacherLinks.OrderByDescending(link => link.IsMainTeacher).Select(link => new ClassTeacherDto(
                link.Teacher!.Id, link.Teacher.UserId, users.GetValueOrDefault(link.Teacher.UserId)?.FullName,
                link.Teacher.Faculty.Code, link.Teacher.Faculty.Name, link.IsMainTeacher)).ToList(),
            schedules.Select(schedule => new ClassScheduleDto(schedule.Id, schedule.StartDateTime,
                schedule.EndDateTime, schedule.Room?.Name, schedule.ScheduleType, schedule.Attendances.Count)).ToList());
    }

    public async Task<IReadOnlyList<TeachingAssignmentDto>> GetTeachingAssignmentsAsync(CancellationToken cancellationToken)
    {
        var rows = await dbContext.SubjectTeachingTeachers.AsNoTracking()
            .Where(link => !link.IsDeleted && link.Teacher != null && !link.Teacher.IsDeleted
                && !link.SubjectTeaching.IsDeleted && !link.SubjectTeaching.Subject.IsDeleted)
            .OrderBy(link => link.Teacher!.Faculty.Code).ThenBy(link => link.SubjectTeaching.Subject.SubjectCode)
            .Select(link => new
            {
                AssignmentId = link.Id, TeacherFacultyId = link.Teacher!.Id, link.Teacher.UserId,
                FacultyCode = link.Teacher.Faculty.Code, FacultyName = link.Teacher.Faculty.Name,
                ClassId = link.SubjectTeaching.Id, ClassName = link.SubjectTeaching.Name,
                SubjectId = link.SubjectTeaching.Subject.Id, link.SubjectTeaching.Subject.SubjectCode,
                SubjectName = link.SubjectTeaching.Subject.Name, link.SubjectTeaching.StartDate,
                link.SubjectTeaching.EndDate, link.IsMainTeacher,
                StudentCount = link.SubjectTeaching.SubjectStudents.Count,
                RoomCapacity = link.SubjectTeaching.RoomIdDefaultNavigation != null
                    ? (int?)link.SubjectTeaching.RoomIdDefaultNavigation.NumberOfSeats : null
            }).ToListAsync(cancellationToken);
        var users = await identityUserClient.GetManyAsync(rows.Select(row => row.UserId), cancellationToken);
        return rows.Select(row => new TeachingAssignmentDto(
            row.AssignmentId, row.TeacherFacultyId, row.UserId,
            users.GetValueOrDefault(row.UserId)?.FullName, users.GetValueOrDefault(row.UserId)?.UserName,
            users.GetValueOrDefault(row.UserId)?.ProfilePicUrl, row.FacultyCode, row.FacultyName,
            row.ClassId, row.ClassName, row.SubjectId, row.SubjectCode, row.SubjectName,
            row.StartDate, row.EndDate, row.IsMainTeacher, row.StudentCount, row.RoomCapacity)).ToList();
    }

    public async Task<TeacherAssignmentsDetailDto?> GetTeacherAssignmentsAsync(Guid teacherFacultyId, CancellationToken cancellationToken)
    {
        var assignments = (await GetTeachingAssignmentsAsync(cancellationToken))
            .Where(item => item.TeacherFacultyId == teacherFacultyId).ToList();
        var first = assignments.FirstOrDefault();
        if (first is null) return null;
        return new TeacherAssignmentsDetailDto(first.TeacherFacultyId, first.UserId, first.FullName,
            first.UserName, first.ProfilePicUrl, first.FacultyCode, first.FacultyName,
            assignments.Count, assignments.Count(item => item.IsMainTeacher == true),
            assignments.Count(item => item.IsMainTeacher == false), assignments);
    }

    public async Task<IReadOnlyList<ManagementRoomDto>> GetRoomsAsync(CancellationToken cancellationToken) =>
        await dbContext.Rooms.AsNoTracking().Where(room => !room.IsDeleted).OrderBy(room => room.Name)
            .Select(room => new ManagementRoomDto(room.Id, room.Name, room.NumberOfSeats,
                room.SubjectSchedules.Count(schedule => !schedule.IsDeleted),
                room.SubjectTeachings.Count(item => !item.IsDeleted)))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ManagementTeachingScheduleDto>> GetTeachingScheduleAsync(CancellationToken cancellationToken)
    {
        var rows = await dbContext.SubjectSchedules.AsNoTracking()
            .Where(schedule => !schedule.IsDeleted && !schedule.SubjectTeaching.IsDeleted && !schedule.SubjectTeaching.Subject.IsDeleted)
            .OrderBy(schedule => schedule.StartDateTime)
            .Select(schedule => new
            {
                ScheduleId = schedule.Id, ClassId = schedule.SubjectTeaching.Id,
                ClassName = schedule.SubjectTeaching.Name, SubjectId = schedule.SubjectTeaching.Subject.Id,
                schedule.SubjectTeaching.Subject.SubjectCode, SubjectName = schedule.SubjectTeaching.Subject.Name,
                schedule.RoomId, RoomName = schedule.Room != null ? schedule.Room.Name : null,
                RoomCapacity = schedule.Room != null ? (int?)schedule.Room.NumberOfSeats : null,
                TeacherFacultyId = schedule.TeacherId,
                TeacherUserId = schedule.Teacher != null ? (Guid?)schedule.Teacher.UserId : null,
                schedule.StartDateTime, schedule.EndDateTime, schedule.ScheduleType, schedule.Note,
                StudentCount = schedule.SubjectTeaching.SubjectStudents.Count
            }).ToListAsync(cancellationToken);
        var users = await identityUserClient.GetManyAsync(rows.Where(row => row.TeacherUserId != null).Select(row => row.TeacherUserId!.Value), cancellationToken);
        return rows.Select(row => new ManagementTeachingScheduleDto(row.ScheduleId, row.ClassId, row.ClassName,
            row.SubjectId, row.SubjectCode, row.SubjectName, row.RoomId, row.RoomName, row.RoomCapacity,
            row.TeacherFacultyId, row.TeacherUserId != null ? users.GetValueOrDefault(row.TeacherUserId.Value)?.FullName : null,
            row.StartDateTime, row.EndDateTime, row.ScheduleType, row.Note, row.StudentCount)).ToList();
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

    private IQueryable<StudentProjection> StudentQuery(Guid? studentId = null)
    {
        var students = dbContext.Students.AsNoTracking().Where(student => !student.IsDeleted);
        if (studentId.HasValue)
        {
            students = students.Where(student => student.Id == studentId.Value);
        }

        return students
            .Select(student => new StudentProjection(student.Id, student.UserId, student.Nickname ?? "",
                student.Major.Code, student.Major.Name,
                student.Major.Faculty != null ? student.Major.Faculty.Name : null,
                student.AcademicYear.Name, student.StudyStatus, student.IsGraduated, student.HasIssue));
    }

    public async Task<IReadOnlyList<ManagementSubjectSummaryDto>> GetSubjectsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Subjects.AsNoTracking()
            .Where(subject => !subject.IsDeleted)
            .OrderBy(subject => subject.SubjectCode)
            .Select(subject => new ManagementSubjectSummaryDto(
                subject.Id, subject.FacultyId, subject.Faculty != null ? subject.Faculty.Code : null,
                subject.Faculty != null ? subject.Faculty.Name : null, subject.SubjectCode, subject.Name,
                subject.CreditPoint, subject.TotalHours, subject.IsActived,
                subject.SemesterSubjects.Count(item => !item.IsDeleted),
                subject.SubjectTeachings.Count(item => !item.IsDeleted),
                subject.SubjectDocuments.Count(),
                subject.SubjectSpecialNotes.Count(item => !item.IsDeleted)))
            .ToListAsync(cancellationToken);
    }

    public async Task<AttendanceManagementPageDto> GetAttendancePageAsync(ManagementAttendanceQueryDto query, CancellationToken cancellationToken)
    {
        var source = ApplyAttendanceFilters(dbContext.Attendances.AsNoTracking(), query);
        var total = await source.CountAsync(cancellationToken);
        var statusCounts = await source.GroupBy(item => item.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Status, item => item.Count, cancellationToken);
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        // Materialize the small page of keys before joining the academic graph.
        // SQL Server otherwise expands every Student column into the paged query
        // and can choose a plan that times out even for a ten-row page.
        var pageIds = await source.OrderByDescending(item => item.CreationDate)
            .Select(item => item.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        var rows = await AttendanceProjectionQuery(dbContext.Attendances.AsNoTracking()
                .Where(item => pageIds.Contains(item.Id)))
            .ToListAsync(cancellationToken);
        var positions = pageIds.Select((id, index) => new { id, index }).ToDictionary(item => item.id, item => item.index);
        rows.Sort((left, right) => positions[left.AttendanceId].CompareTo(positions[right.AttendanceId]));
        var userIds = rows.Select(item => item.StudentUserId).Concat(rows.Select(item => item.CreatedById))
            .Concat(rows.Where(item => item.TeacherUserId.HasValue).Select(item => item.TeacherUserId!.Value));
        var users = await identityUserClient.GetManyAsync(userIds, cancellationToken);
        return new AttendanceManagementPageDto(rows.Select(item => MapAttendance(item, users)).ToList(),
            pageNumber, pageSize, total, statusCounts);
    }

    public async Task<AttendanceManagementItemDto?> GetAttendanceDetailAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await AttendanceProjectionQuery(dbContext.Attendances.AsNoTracking()
                .Where(value => value.Id == id))
            .SingleOrDefaultAsync(cancellationToken);
        if (item is null) return null;
        var userIds = new[] { item.StudentUserId, item.CreatedById }
            .Concat(item.TeacherUserId.HasValue ? new[] { item.TeacherUserId.Value } : []);
        var users = await identityUserClient.GetManyAsync(userIds, cancellationToken);
        return MapAttendance(item, users);
    }

    public async Task<StudentEvaluationManagementPageDto> GetStudentEvaluationPageAsync(
        StudentEvaluationManagementQueryDto query, CancellationToken cancellationToken)
    {
        var source = ApplyEvaluationFilters(dbContext.StudentEvaluations.AsNoTracking()
            .Where(item => !item.IsDeleted), query);
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var total = await source.CountAsync(cancellationToken);
        var average = await source.Where(item => item.TotalScore.HasValue)
            .AverageAsync(item => item.TotalScore, cancellationToken);
        var withComment = await source.CountAsync(item => item.Comment != null && item.Comment != "", cancellationToken);
        var withoutScore = await source.CountAsync(item => !item.TotalScore.HasValue, cancellationToken);
        var studentCount = await source.Select(item => item.StudentId).Distinct().CountAsync(cancellationToken);
        var pageIds = await source.OrderByDescending(item => item.CreationDate).Select(item => item.Id)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        var rows = await EvaluationIncludes(dbContext.StudentEvaluations.AsNoTracking()
                .Where(item => pageIds.Contains(item.Id)))
            .ToListAsync(cancellationToken);
        var positions = pageIds.Select((id, index) => new { id, index }).ToDictionary(item => item.id, item => item.index);
        rows.Sort((left, right) => positions[left.Id].CompareTo(positions[right.Id]));
        var users = await identityUserClient.GetManyAsync(rows.Select(item => item.Student.UserId), cancellationToken);
        return new StudentEvaluationManagementPageDto(rows.Select(item => MapEvaluation(item, users)).ToList(),
            pageNumber, pageSize, total, average, withComment, withoutScore, studentCount);
    }

    public async Task<StudentEvaluationManagementItemDto?> GetStudentEvaluationAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await EvaluationIncludes(dbContext.StudentEvaluations.AsNoTracking())
            .SingleOrDefaultAsync(value => value.Id == id && !value.IsDeleted, cancellationToken);
        if (item is null) return null;
        var users = await identityUserClient.GetManyAsync(new[] { item.Student.UserId }, cancellationToken);
        return MapEvaluation(item, users);
    }

    private static IQueryable<StudentEvaluation> ApplyEvaluationFilters(
        IQueryable<StudentEvaluation> source, StudentEvaluationManagementQueryDto query)
    {
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(item => (item.Student.Nickname != null && item.Student.Nickname.Contains(search))
                || (item.Comment != null && item.Comment.Contains(search))
                || (item.TeacherName != null && item.TeacherName.Contains(search)));
        }
        if (query.StudentId.HasValue) source = source.Where(item => item.StudentId == query.StudentId);
        if (query.ClassId.HasValue) source = source.Where(item => item.SubjectTeachingId == query.ClassId);
        if (query.SemesterPlanId.HasValue) source = source.Where(item => item.SemesterPlanId == query.SemesterPlanId);
        if (query.TeacherId.HasValue) source = source.Where(item => item.TeacherId == query.TeacherId);
        if (query.Type.HasValue) source = source.Where(item => item.Type == query.Type);
        if (query.FromDate.HasValue) source = source.Where(item => item.CreationDate >= query.FromDate.Value);
        if (query.ToDate.HasValue) source = source.Where(item => item.CreationDate < query.ToDate.Value.AddDays(1));
        if (query.MinimumScore.HasValue) source = source.Where(item => item.TotalScore >= query.MinimumScore);
        if (query.MaximumScore.HasValue) source = source.Where(item => item.TotalScore <= query.MaximumScore);
        return source;
    }

    private static IQueryable<StudentEvaluation> EvaluationIncludes(IQueryable<StudentEvaluation> source) => source
        .Include(item => item.Student).ThenInclude(student => student.Major)
        .Include(item => item.Student).ThenInclude(student => student.AcademicYear)
        .Include(item => item.SubjectTeaching).ThenInclude(teaching => teaching!.Subject)
        .Include(item => item.SemesterPlan)
        .Include(item => item.StudentEvaluationDetails).ThenInclude(detail => detail.EvaluationCriteria);

    private static StudentEvaluationManagementItemDto MapEvaluation(StudentEvaluation item,
        IReadOnlyDictionary<Guid, AcademicService.Application.DTOs.StudentAccess.SafeIdentityUserDto> users) =>
        new(item.Id, item.StudentId, item.Student.UserId, item.Student.Nickname ?? item.StudentId.ToString(),
            users.GetValueOrDefault(item.Student.UserId)?.FullName,
            users.GetValueOrDefault(item.Student.UserId)?.ProfilePicUrl,
            item.Student.Major.Name, item.Student.AcademicYear.Name,
            item.SubjectTeachingId, item.SubjectTeaching?.Name,
            item.SubjectTeaching?.Subject.SubjectCode, item.SubjectTeaching?.Subject.Name,
            item.SemesterPlanId, item.SemesterPlan?.Semester, item.SubjectTeachingExamId,
            item.QuestionId, item.TeacherId, item.TeacherName, item.Type, item.Comment,
            item.TotalScore, item.CreationDate, item.UpdatedDate,
            item.StudentEvaluationDetails.Where(detail => !detail.IsDeleted)
                .Select(detail => new StudentEvaluationBreakdownDto(detail.Id,
                    detail.EvaluationCriteriaId, detail.EvaluationName, detail.EvaluationCriteria?.Name,
                    detail.StudentScore, detail.Score, detail.EvaluationCriteria != null)).ToList());

    private static IQueryable<Attendance> ApplyAttendanceFilters(IQueryable<Attendance> source, ManagementAttendanceQueryDto query)
    {
        if (!string.IsNullOrWhiteSpace(query.Search)) { var search = query.Search.Trim(); source = source.Where(item => (item.Student.Nickname != null && item.Student.Nickname.Contains(search)) || item.Notes.Contains(search)); }
        if (query.FacultyId.HasValue) source = source.Where(item => item.SubjectSchedule.SubjectTeaching.Subject.FacultyId == query.FacultyId);
        if (query.SubjectId.HasValue) source = source.Where(item => item.SubjectSchedule.SubjectTeaching.SubjectId == query.SubjectId);
        if (query.ClassId.HasValue) source = source.Where(item => item.SubjectSchedule.SubjectTeachingId == query.ClassId);
        if (query.RoomId.HasValue) source = source.Where(item => item.SubjectSchedule.RoomId == query.RoomId);
        if (query.Status.HasValue) source = source.Where(item => item.Status == query.Status);
        if (query.CreatedById.HasValue) source = source.Where(item => item.CreatedById == query.CreatedById);
        if (query.FromDate.HasValue) source = source.Where(item => item.SubjectSchedule.StartDateTime >= query.FromDate.Value);
        if (query.ToDate.HasValue) source = source.Where(item => item.SubjectSchedule.StartDateTime < query.ToDate.Value.AddDays(1));
        return source;
    }

    private static IQueryable<AttendanceProjection> AttendanceProjectionQuery(IQueryable<Attendance> source) =>
        source.Select(item => new AttendanceProjection(
            item.Id, item.StudentId, item.Student.UserId, item.Student.Nickname ?? item.StudentId.ToString(),
            item.Student.Major.Code, item.Student.Major.Name, item.Student.AcademicYear.Name,
            item.SubjectScheduleId, item.SubjectSchedule.SubjectTeachingId, item.SubjectSchedule.SubjectTeaching.Name,
            item.SubjectSchedule.SubjectTeaching.SubjectId,
            item.SubjectSchedule.SubjectTeaching.Subject.SubjectCode,
            item.SubjectSchedule.SubjectTeaching.Subject.Name,
            item.SubjectSchedule.SubjectTeaching.Subject.FacultyId,
            item.SubjectSchedule.SubjectTeaching.Subject.Faculty != null
                ? item.SubjectSchedule.SubjectTeaching.Subject.Faculty.Name
                : null,
            item.SubjectSchedule.RoomId, item.SubjectSchedule.Room != null ? item.SubjectSchedule.Room.Name : null,
            item.SubjectSchedule.TeacherId,
            item.SubjectSchedule.Teacher != null ? item.SubjectSchedule.Teacher.UserId : null,
            item.SubjectSchedule.StartDateTime, item.SubjectSchedule.EndDateTime,
            item.Status, item.Notes, item.CreatedById, item.CreationDate,
            item.IsFirstTypeWarning, item.IsSecondTypeWarning));

    private static AttendanceManagementItemDto MapAttendance(AttendanceProjection item, IReadOnlyDictionary<Guid, AcademicService.Application.DTOs.StudentAccess.SafeIdentityUserDto> users)
    {
        return new AttendanceManagementItemDto(item.AttendanceId, item.StudentId, item.StudentCode,
            users.GetValueOrDefault(item.StudentUserId)?.FullName, users.GetValueOrDefault(item.StudentUserId)?.ProfilePicUrl,
            item.MajorCode, item.MajorName, item.AcademicYearName,
            item.ScheduleId, item.ClassId, item.ClassName, item.SubjectId, item.SubjectCode, item.SubjectName,
            item.FacultyId, item.FacultyName, item.RoomId, item.RoomName,
            item.TeacherFacultyId, item.TeacherUserId.HasValue ? users.GetValueOrDefault(item.TeacherUserId.Value)?.FullName : null,
            item.StartDateTime, item.EndDateTime, item.Status, item.Notes, item.CreatedById,
            users.GetValueOrDefault(item.CreatedById)?.FullName, item.CreationDate, item.IsFirstTypeWarning, item.IsSecondTypeWarning);
    }

    public async Task<ManagementSubjectDetailDto?> GetSubjectAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Subjects.AsNoTracking()
            .Where(subject => subject.Id == id && !subject.IsDeleted)
            .Select(subject => new ManagementSubjectDetailDto(
                subject.Id, subject.FacultyId, subject.Faculty != null ? subject.Faculty.Code : null,
                subject.Faculty != null ? subject.Faculty.Name : null, subject.SubjectCode, subject.Name,
                subject.CreditPoint, subject.TotalHours, subject.Note, subject.IsActived,
                subject.SemesterSubjects.Count(item => !item.IsDeleted),
                subject.SubjectTeachings.Count(item => !item.IsDeleted),
                subject.SubjectDocuments.Count(),
                subject.SubjectSpecialNotes.Count(item => !item.IsDeleted)))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private sealed record StudentProjection(Guid StudentId, Guid UserId, string StudentCode,
        string MajorCode, string MajorName, string? FacultyName, string AcademicYearName,
        int? RawStudyStatus, bool IsGraduated, bool? HasIssue);

    private sealed record AttendanceProjection(
        Guid AttendanceId, Guid StudentId, Guid StudentUserId, string StudentCode,
        string MajorCode, string MajorName, string AcademicYearName,
        Guid ScheduleId, Guid ClassId, string ClassName,
        Guid SubjectId, string SubjectCode, string SubjectName,
        Guid? FacultyId, string? FacultyName, Guid? RoomId, string? RoomName,
        Guid? TeacherFacultyId, Guid? TeacherUserId,
        DateTime StartDateTime, DateTime EndDateTime, int Status, string Notes,
        Guid CreatedById, DateTime CreationDate, bool? IsFirstTypeWarning, bool? IsSecondTypeWarning);
}
