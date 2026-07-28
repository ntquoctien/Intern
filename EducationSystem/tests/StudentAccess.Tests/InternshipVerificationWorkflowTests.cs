using CommunicationService.Application.DTOs.InternshipVerification;
using CommunicationService.Application.Interfaces;
using CommunicationService.Application.Options;
using CommunicationService.Application.Services;
using CommunicationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Xunit;

namespace StudentAccess.Tests;

public sealed class InternshipVerificationWorkflowTests
{
    [Fact]
    public async Task Submit_CreatesPendingRequestAndQueuesTokenizedMentorLink()
    {
        await using var db = NewDb();
        var email = new CapturingEmailSender();
        var service = NewService(db, email, new FakeAcademicClient());
        var studentId = Guid.NewGuid();

        var result = await service.SubmitAsync(
            studentId,
            new SubmitInternshipRequestDto(
                "TDU Software",
                "Backend Intern",
                "mentor@example.com",
                new DateOnly(2026, 1, 1),
                new DateOnly(2026, 3, 31),
                "Build APIs"),
            default);

        var entity = await db.FormRequests.SingleAsync();
        Assert.Equal(result.RequestId, entity.Id);
        Assert.Equal(studentId, entity.StudentId);
        Assert.NotEqual(Guid.Empty, entity.EmployerToken);
        Assert.Equal(0, entity.EmployerVerifiedStatus);
        Assert.Equal(FormRequestWorkflowStatuses.Pending, entity.Status);
        Assert.Contains(entity.EmployerToken!.Value.ToString(), email.VerificationLink!.Query);
    }

    [Fact]
    public async Task EmployerToken_CanBeUsedOnlyOnce_AndStoresAssessment()
    {
        await using var db = NewDb();
        var service = NewService(db, new CapturingEmailSender(), new FakeAcademicClient());
        await service.SubmitAsync(
            Guid.NewGuid(),
            ValidSubmission(),
            default);
        var token = (await db.FormRequests.SingleAsync()).EmployerToken!.Value;
        var context = await service.GetVerificationContextAsync(token, default);

        var first = await service.VerifyByEmployerAsync(
            new VerifyInternshipByEmployerDto(token, true, 8.5m, "Information confirmed."),
            default);
        var replay = await service.VerifyByEmployerAsync(
            new VerifyInternshipByEmployerDto(token, true, 8.5m, null),
            default);
        var usedContext = await service.GetVerificationContextAsync(token, default);

        Assert.NotNull(context);
        Assert.Equal("Nguyễn Trọng Nghĩa", context!.StudentName);
        Assert.Equal("23DTH118", context.StudentCode);
        Assert.NotNull(first);
        Assert.Equal(1, first!.EmployerVerifiedStatus);
        Assert.Null(replay);
        Assert.Null(usedContext);
        var verificationData = (await db.FormRequests.SingleAsync()).VerificationData;
        Assert.Contains("Information confirmed.", verificationData);
        Assert.Contains("8.5", verificationData);
    }

    [Fact]
    public async Task SchoolApproval_PersistsOfficialInternshipAfterEmployerConfirmation()
    {
        await using var db = NewDb();
        var academic = new FakeAcademicClient();
        var service = NewService(db, new CapturingEmailSender(), academic);
        var created = await service.SubmitAsync(Guid.NewGuid(), ValidSubmission(), default);
        var token = (await db.FormRequests.SingleAsync()).EmployerToken!.Value;
        await service.VerifyByEmployerAsync(
            new VerifyInternshipByEmployerDto(token, true, 9m, null),
            default);

        var approved = await service.ApproveAsync(
            created.RequestId,
            Guid.NewGuid(),
            "Academic Administrator",
            default);

        Assert.NotNull(approved);
        Assert.Equal(42, approved!.InternshipId);
        Assert.NotNull(academic.LastRequest);
        var entity = await db.FormRequests.SingleAsync();
        Assert.Equal(FormRequestWorkflowStatuses.Approved, entity.Status);
        Assert.Equal("Academic Administrator", entity.ApprovalName);
    }

    private static SubmitInternshipRequestDto ValidSubmission() =>
        new(
            "TDU Software",
            "Frontend Intern",
            "mentor@example.com",
            new DateOnly(2026, 1, 1),
            null,
            "Build the student portal.");

    private static CommunicationDbContext NewDb() =>
        new(
            new DbContextOptionsBuilder<CommunicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(warnings =>
                    warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options);

    private static InternshipVerificationService NewService(
        CommunicationDbContext db,
        IInternshipVerificationEmailSender emailSender,
        IAcademicInternshipClient academicClient) =>
        new(
            db,
            emailSender,
            academicClient,
            TimeProvider.System,
            Options.Create(new InternshipEmailOptions
            {
                PortalBaseUrl = "https://portal.tdu.edu.vn",
                SmtpHost = "smtp.example.com",
                FromAddress = "no-reply@tdu.edu.vn"
            }));

    private sealed class CapturingEmailSender : IInternshipVerificationEmailSender
    {
        public Uri? VerificationLink { get; private set; }

        public Task SendAsync(
            string mentorEmail,
            string companyName,
            Uri verificationLink,
            CancellationToken cancellationToken)
        {
            VerificationLink = verificationLink;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAcademicClient : IAcademicInternshipClient
    {
        public CreateApprovedInternshipDto? LastRequest { get; private set; }

        public Task<AcademicStudentVerificationProfileDto?> GetStudentVerificationProfileAsync(
            Guid studentId,
            CancellationToken cancellationToken) =>
            Task.FromResult<AcademicStudentVerificationProfileDto?>(
                new(studentId, "23DTH118", "Nguyễn Trọng Nghĩa"));

        public Task<int> CreateApprovedInternshipAsync(
            CreateApprovedInternshipDto request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(42);
        }
    }
}
