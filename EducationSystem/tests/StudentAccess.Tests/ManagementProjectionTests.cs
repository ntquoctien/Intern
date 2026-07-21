using AcademicService.Application.DTOs.StudentAccess;
using AcademicService.Application.Interfaces;
using AcademicService.Application.Services;
using AcademicService.Infrastructure.Persistence;
using AcademicService.Infrastructure.Persistence.Entities;
using ExamService.Application.Services;
using ExamService.Infrastructure.Persistence;
using ExamService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace StudentAccess.Tests;

public sealed class ManagementProjectionTests
{
    [Fact]
    public async Task AcademicManagementProjections_HandleOptionalRelationsAndSharedSubjects()
    {
        await using var db = new AcademicDbContext(new DbContextOptionsBuilder<AcademicDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var faculty = new Faculty { Id = Guid.NewGuid(), Code = "IT", Name = "CNTT" };
        var majorA = new Major { Id = Guid.NewGuid(), Faculty = faculty, FacultyId = faculty.Id, Code = "SE", Name = "KTPM" };
        var majorB = new Major { Id = Guid.NewGuid(), Faculty = faculty, FacultyId = faculty.Id, Code = "CS", Name = "KHMT" };
        var year = new AcademicYear { Id = Guid.NewGuid(), Name = "2026", Year = 2026, StartDate = new(2026, 1, 1), EndDate = new(2026, 12, 31) };
        var subject = new Subject { Id = Guid.NewGuid(), Faculty = faculty, FacultyId = faculty.Id, SubjectCode = "CS101", Name = "Nhập môn", CreditPoint = 3, Note = "", IsActived = true };
        var planA = NewPlan(majorA, year, 1); var planB = NewPlan(majorB, year, 1);
        var planSubjectA = NewPlanSubject(planA, subject); var planSubjectB = NewPlanSubject(planB, subject);
        var student = new Student { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Major = majorA, MajorId = majorA.Id, AcademicYear = year, AcademicYearId = year.Id, Nickname = "SV000001", IssueDescription = "" };
        var teacher = new TeacherFaculty { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Faculty = faculty, FacultyId = faculty.Id };
        var teaching = new SubjectTeaching { Id = Guid.NewGuid(), Subject = subject, SubjectId = subject.Id, Name = "CS101.01", StartDate = new(2026, 7, 1), EndDate = new(2026, 9, 1), TotalSessions = 12 };
        var teacherLink = new SubjectTeachingTeacher { Id = Guid.NewGuid(), SubjectTeaching = teaching, SubjectTeachingId = teaching.Id, Teacher = teacher, TeacherId = teacher.Id };
        var enrollment = new SubjectStudent { Id = Guid.NewGuid(), Student = student, StudentId = student.Id, SubjectTeaching = teaching, SubjectTeachingId = teaching.Id };
        var schedule = new SubjectSchedule { Id = Guid.NewGuid(), SubjectTeaching = teaching, SubjectTeachingId = teaching.Id, StartDateTime = new(2026, 7, 20, 8, 0, 0), EndDateTime = new(2026, 7, 20, 10, 0, 0), Note = "" };
        var attendance = new Attendance { Id = Guid.NewGuid(), Student = student, StudentId = student.Id, SubjectSchedule = schedule, SubjectScheduleId = schedule.Id, CreatedById = Guid.NewGuid(), Status = 4, Notes = "", CreationDate = new(2026, 7, 20) };
        db.AddRange(faculty, majorA, majorB, year, subject, planA, planB, planSubjectA, planSubjectB, student, teacher, teaching, teacherLink, enrollment, schedule, attendance);
        await db.SaveChangesAsync();
        var service = new ManagementReadService(db, new MissingIdentityClient());

        Assert.Equal(1, (await service.GetDashboardAsync(default)).Students);
        Assert.Single(await service.GetStudentsAsync(default));
        Assert.NotNull(await service.GetStudentAsync(student.Id, default));
        Assert.Single(await service.GetTeachersAsync(default));
        Assert.Equal(2, (await service.GetPlansAsync(default)).Count);
        var projectedClass = Assert.Single(await service.GetClassesAsync(default));
        Assert.Equal("CNTT", projectedClass.FacultyName);
        Assert.Empty(projectedClass.TeacherNames);
        Assert.Null(Assert.Single(await service.GetScheduleAsync(default)).RoomName);
        Assert.Equal(4, Assert.Single(await service.GetAttendanceAsync(default)).RawStatus);
    }

    [Fact]
    public async Task ExamManagementProjections_PreserveRawStatisticsAndQuestionHierarchy()
    {
        await using var db = new ExamDbContext(new DbContextOptionsBuilder<ExamDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var suite = new QuestionSuite { Id = Guid.NewGuid(), SubjectId = Guid.NewGuid(), Name = "Bộ câu hỏi A", CreationTime = new(2026, 7, 1) };
        var question = new Question { Id = Guid.NewGuid(), QuestionSuite = suite, QuestionSuiteId = suite.Id, QuestionText = "Câu hỏi?", Level = 2 };
        var answer = new QuestionAnswer { Id = Guid.NewGuid(), Question = question, QuestionId = question.Id, AnswerText = "Đáp án", IsAnswer = true };
        var exam = new SubjectTeachingExam { Id = Guid.NewGuid(), SubjectTeachingId = Guid.NewGuid(), QuestionSuite = suite, QuestionSuiteId = suite.Id, Name = "Thi", StartDate = new(2026, 7, 20), EndDate = new(2026, 7, 20, 2, 0, 0), Notes = "" };
        var result = new ExamResult { Id = Guid.NewGuid(), StudentId = Guid.NewGuid(), SubjectTeachingExam = exam, SubjectTeachingExamId = exam.Id, Result = 7.25f, CombinedResult = null };
        db.AddRange(suite, question, answer, exam, result);
        await db.SaveChangesAsync();
        var service = new ManagementExamReadService(db);

        var summary = await service.GetResultSummaryAsync(default);
        Assert.Equal(1, summary.RecordCount);
        Assert.Equal(7.25f, summary.MinimumRawResult);
        Assert.Single(await service.GetResultsAsync(default));
        Assert.Single(await service.GetSuitesAsync(default));
        var detail = await service.GetSuiteAsync(suite.Id, default);
        Assert.Single(Assert.Single(detail!.Questions).Answers);
    }

    private static SemesterPlan NewPlan(Major major, AcademicYear year, int semester) => new() { Id = Guid.NewGuid(), Major = major, MajorId = major.Id, AcademicYear = year, AcademicYearId = year.Id, Semester = semester, StartDate = new(2026, 1, 1), EndDate = new(2026, 5, 1) };
    private static SemesterSubject NewPlanSubject(SemesterPlan plan, Subject subject) => new() { Id = Guid.NewGuid(), SemesterPlan = plan, SemesterPlanId = plan.Id, Subject = subject, SubjectId = subject.Id, SubjectCode = subject.SubjectCode, SubjectName = subject.Name, CreditPoint = subject.CreditPoint };

    private sealed class MissingIdentityClient : IIdentityUserClient
    {
        public Task<SafeIdentityUserDto?> GetAsync(Guid userId, CancellationToken cancellationToken) => Task.FromResult<SafeIdentityUserDto?>(null);
        public Task<IReadOnlyDictionary<Guid, SafeIdentityUserDto>> GetManyAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyDictionary<Guid, SafeIdentityUserDto>>(new Dictionary<Guid, SafeIdentityUserDto>());
    }
}
