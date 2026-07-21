using AcademicService.Application.Services;
using AcademicService.Infrastructure.Persistence;
using AcademicStudent = AcademicService.Infrastructure.Persistence.Entities.Student;
using IdentityService.Application.StudentAccess;
using IdentityService.Infrastructure.Persistence;
using IdentityUser = IdentityService.Infrastructure.Persistence.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedKernel;
using Xunit;

namespace StudentAccess.Tests;

public sealed class StudentLoginTests
{
    [Fact]
    public async Task AcademicResolver_RequiresExactlyOneLiveExactNickname()
    {
        await using var db = NewAcademicDb();
        db.Students.AddRange(
            NewStudent("SV000001"),
            NewStudent("SV000001"),
            NewStudent("SV000002", deleted: true));
        await db.SaveChangesAsync();
        var resolver = new StudentIdentityResolver(db);

        Assert.Equal(2, (await resolver.ResolveByNicknameAsync("SV000001")).MatchCount);
        Assert.Equal(0, (await resolver.ResolveByNicknameAsync("sv000001")).MatchCount);
        Assert.Equal(0, (await resolver.ResolveByNicknameAsync("SV000002")).MatchCount);
    }

    [Fact]
    public async Task Login_ReturnsNotFoundAndAmbiguousWithoutIssuingToken()
    {
        await using var db = NewIdentityDb();
        var issuer = new FakeTokenIssuer();
        var client = new FakeAcademicClient
        {
            Resolution = new AcademicStudentResolution(0, null)
        };
        var service = CreateService(db, client, issuer);

        Assert.Equal(StudentErrorCodes.NotFound, (await service.LoginAsync("missing")).ErrorCode);
        client.Resolution = new AcademicStudentResolution(2, null);
        Assert.Equal(StudentErrorCodes.CodeAmbiguous, (await service.LoginAsync("duplicate")).ErrorCode);
        Assert.Equal(0, issuer.IssueCount);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task Login_RejectsInactiveOrDeletedUser(bool active, bool deleted)
    {
        await using var db = NewIdentityDb();
        var userId = Guid.NewGuid();
        db.Users.Add(NewUser(userId, active, deleted));
        await db.SaveChangesAsync();
        var candidate = NewCandidate(userId);
        var service = CreateService(db, new FakeAcademicClient
        {
            Resolution = new AcademicStudentResolution(1, candidate)
        }, new FakeTokenIssuer());

        var result = await service.LoginAsync("SV000001");
        Assert.Equal(StudentErrorCodes.AccountUnavailable, result.ErrorCode);
    }

    [Fact]
    public async Task Login_RejectsMissingLinkedUser()
    {
        await using var db = NewIdentityDb();
        var service = CreateService(db, new FakeAcademicClient
        {
            Resolution = new AcademicStudentResolution(1, NewCandidate(Guid.NewGuid()))
        }, new FakeTokenIssuer());

        Assert.Equal(StudentErrorCodes.AccountUnavailable, (await service.LoginAsync("SV000001")).ErrorCode);
    }

    [Theory]
    [InlineData(true, null, true)]
    [InlineData(false, null, false)]
    [InlineData(false, 0, true)]
    public async Task AcademicFlags_DoNotBlockEligibleLogin(bool graduated, int? studyStatus, bool hasIssue)
    {
        await using var db = NewIdentityDb();
        var userId = Guid.NewGuid();
        db.Users.Add(NewUser(userId, active: true, deleted: false));
        await db.SaveChangesAsync();
        var candidate = NewCandidate(userId) with
        {
            IsGraduated = graduated,
            StudyStatus = studyStatus,
            HasIssue = hasIssue
        };
        var issuer = new FakeTokenIssuer();
        var service = CreateService(db, new FakeAcademicClient
        {
            Resolution = new AcademicStudentResolution(1, candidate)
        }, issuer);

        var result = await service.LoginAsync("SV000001");
        Assert.True(result.Success);
        Assert.Equal(1, issuer.IssueCount);
    }

    private static StudentLoginService CreateService(
        IdentityDbContext db,
        IAcademicStudentIdentityClient client,
        IStudentTokenIssuer issuer) =>
        new(db, client, issuer, Options.Create(new StudentLoginOptions()));

    private static AcademicStudentCandidate NewCandidate(Guid userId) =>
        new(Guid.NewGuid(), userId, "SV000001", null, false, null);

    private static IdentityUser NewUser(Guid id, bool active, bool deleted) => new()
    {
        Id = id,
        UserName = "student",
        UserInternalId = "SV000001",
        FullName = "Test Student",
        PasswordHash = "not-returned",
        PasswordSalt = [0],
        Role = 0,
        IsActived = active,
        IsDeleted = deleted
    };

    private static AcademicStudent NewStudent(string nickname, bool deleted = false) => new()
    {
        Id = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        AcademicYearId = Guid.NewGuid(),
        MajorId = Guid.NewGuid(),
        Nickname = nickname,
        IssueDescription = string.Empty,
        IsDeleted = deleted
    };

    private static IdentityDbContext NewIdentityDb() => new(
        new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static AcademicDbContext NewAcademicDb() => new(
        new DbContextOptionsBuilder<AcademicDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private sealed class FakeAcademicClient : IAcademicStudentIdentityClient
    {
        public AcademicStudentResolution Resolution { get; set; } = new(0, null);

        public Task<AcademicStudentResolution> ResolveByNicknameAsync(string code, CancellationToken cancellationToken) =>
            Task.FromResult(Resolution);

        public Task<AcademicStudentResolution> ResolveByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
            Task.FromResult(Resolution);
    }

    private sealed class FakeTokenIssuer : IStudentTokenIssuer
    {
        public int IssueCount { get; private set; }

        public IssuedStudentToken Issue(Guid studentId, Guid? userId, string studentCode)
        {
            IssueCount++;
            return new IssuedStudentToken("test-token", DateTimeOffset.UtcNow.AddMinutes(15));
        }
    }
}
