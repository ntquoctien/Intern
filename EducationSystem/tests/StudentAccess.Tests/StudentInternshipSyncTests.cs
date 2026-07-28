using AcademicService.Application.DTOs.Internships;
using AcademicService.Application.DTOs.StudentAccess;
using AcademicService.Application.Interfaces;
using AcademicService.Application.Services;
using AcademicService.Infrastructure.Persistence;
using AcademicService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace StudentAccess.Tests;

public sealed class StudentInternshipSyncTests
{
    [Fact]
    public async Task SyncApproved_IsIdempotentByFormRequest_AndAppearsInStudentHistory()
    {
        await using var db = new AcademicDbContext(
            new DbContextOptionsBuilder<AcademicDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);
        var student = new Student
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AcademicYearId = Guid.NewGuid(),
            MajorId = Guid.NewGuid(),
            Nickname = "23DTH118",
            IssueDescription = string.Empty
        };
        db.Students.Add(student);
        await db.SaveChangesAsync();
        var service = new StudentInternshipService(db, new FakeIdentityUserClient());
        var request = new SyncApprovedInternshipDto(
            Guid.NewGuid(),
            student.Id,
            "TDU Software",
            "Intern .NET Developer",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 3, 31),
            "Developed ASP.NET Core APIs.");

        var firstId = await service.SyncApprovedAsync(request, default);
        var retriedId = await service.SyncApprovedAsync(request, default);
        var history = await service.GetByStudentAsync(student.Id, default);
        var verificationProfile = await service.GetVerificationProfileAsync(student.Id, default);

        Assert.NotNull(firstId);
        Assert.Equal(firstId, retriedId);
        var internship = Assert.Single(history);
        Assert.Equal(request.FormRequestId, internship.FormRequestId);
        Assert.Equal("TDU Software", internship.CompanyName);
        Assert.NotNull(verificationProfile);
        Assert.Equal("23DTH118", verificationProfile!.StudentCode);
        Assert.Equal("Nguyễn Trọng Nghĩa", verificationProfile.StudentName);
    }

    private sealed class FakeIdentityUserClient : IIdentityUserClient
    {
        public Task<SafeIdentityUserDto?> GetAsync(
            Guid userId,
            CancellationToken cancellationToken) =>
            Task.FromResult<SafeIdentityUserDto?>(
                new(userId, "Nguyễn Trọng Nghĩa", "trongnghia", null, true));

        public Task<IReadOnlyDictionary<Guid, SafeIdentityUserDto>> GetManyAsync(
            IEnumerable<Guid> userIds,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyDictionary<Guid, SafeIdentityUserDto>>(
                new Dictionary<Guid, SafeIdentityUserDto>());
    }
}
