using AcademicService.Application.DTOs.StudentAccess;
using AcademicService.Application.Interfaces;
using AcademicService.Application.Services;
using AcademicService.Infrastructure.Persistence;
using AcademicService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace StudentAccess.Tests;

public sealed class AcademicOwnershipTests
{
    [Fact]
    public async Task StudentA_CannotReadStudentBExclusiveAcademicRows()
    {
        await using var db = NewDb();
        var faculty = new Faculty { Id = Guid.NewGuid(), Code = "CNTT", Name = "Công nghệ thông tin" };
        var major = new Major { Id = Guid.NewGuid(), FacultyId = faculty.Id, Faculty = faculty, Code = "KTPM", Name = "Kỹ thuật phần mềm" };
        var year = new AcademicYear
        {
            Id = Guid.NewGuid(), Name = "2026", Year = 2026,
            StartDate = new DateTime(2026, 1, 1), EndDate = new DateTime(2026, 12, 31)
        };
        var studentA = NewStudent(major, year, "SVA");
        var studentB = NewStudent(major, year, "SVB");
        var subject = new Subject
        {
            Id = Guid.NewGuid(), Faculty = faculty, FacultyId = faculty.Id,
            SubjectCode = "SE101", Name = "Kiến trúc phần mềm", CreditPoint = 3,
            Note = string.Empty, IsActived = true
        };
        var room = new Room { Id = Guid.NewGuid(), Name = "A101", NumberOfSeats = 40 };
        var teaching = new SubjectTeaching
        {
            Id = Guid.NewGuid(), Subject = subject, SubjectId = subject.Id, Name = "SE101.01",
            StartDate = new DateTime(2026, 7, 1), EndDate = new DateTime(2026, 9, 1),
            TotalSessions = 15, RoomIdDefault = room.Id, RoomIdDefaultNavigation = room
        };
        var enrollmentB = new SubjectStudent
        {
            Id = Guid.NewGuid(), Student = studentB, StudentId = studentB.Id,
            SubjectTeaching = teaching, SubjectTeachingId = teaching.Id
        };
        var schedule = new SubjectSchedule
        {
            Id = Guid.NewGuid(), SubjectTeaching = teaching, SubjectTeachingId = teaching.Id,
            Room = room, RoomId = room.Id, StartDateTime = new DateTime(2026, 7, 22, 8, 0, 0),
            EndDateTime = new DateTime(2026, 7, 22, 10, 0, 0), Note = string.Empty
        };
        var attendanceB = new Attendance
        {
            Id = Guid.NewGuid(), Student = studentB, StudentId = studentB.Id,
            SubjectSchedule = schedule, SubjectScheduleId = schedule.Id,
            CreatedById = Guid.NewGuid(), Status = 3, Notes = string.Empty,
            CreationDate = new DateTime(2026, 7, 22)
        };
        db.AddRange(faculty, major, year, studentA, studentB, subject, room, teaching, enrollmentB, schedule, attendanceB);
        await db.SaveChangesAsync();
        var service = new StudentSelfService(db, new FakeIdentityClient(), TimeProvider.System);

        Assert.NotNull(await service.GetProfileAsync(studentA.Id, "SVA", default));
        Assert.Null(await service.GetProfileAsync(Guid.NewGuid(), "SVB", default));
        Assert.Empty(await service.GetSubjectsAsync(studentA.Id, default));
        Assert.Empty(await service.GetScheduleAsync(studentA.Id, default));
        Assert.Empty(await service.GetAttendanceAsync(studentA.Id, default));
        Assert.Single(await service.GetSubjectsAsync(studentB.Id, default));
        Assert.Single(await service.GetScheduleAsync(studentB.Id, default));
        Assert.Single(await service.GetAttendanceAsync(studentB.Id, default));
    }

    private static Student NewStudent(Major major, AcademicYear year, string code) => new()
    {
        Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Major = major, MajorId = major.Id,
        AcademicYear = year, AcademicYearId = year.Id, Nickname = code, IssueDescription = string.Empty
    };

    private static AcademicDbContext NewDb() => new(
        new DbContextOptionsBuilder<AcademicDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private sealed class FakeIdentityClient : IIdentityUserClient
    {
        public Task<SafeIdentityUserDto?> GetAsync(Guid userId, CancellationToken cancellationToken) =>
            Task.FromResult<SafeIdentityUserDto?>(new(userId, "Sinh viên", "student", null, true));

        public Task<IReadOnlyDictionary<Guid, SafeIdentityUserDto>> GetManyAsync(
            IEnumerable<Guid> userIds,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyDictionary<Guid, SafeIdentityUserDto>>(
                new Dictionary<Guid, SafeIdentityUserDto>());
    }
}
