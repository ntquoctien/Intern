using System.Text.Json;
using System.Data;
using CommunicationService.Application.DTOs.InternshipVerification;
using CommunicationService.Application.Interfaces;
using CommunicationService.Application.Options;
using CommunicationService.Infrastructure.Persistence;
using CommunicationService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CommunicationService.Application.Services;

public sealed class InternshipVerificationService(
    CommunicationDbContext dbContext,
    IInternshipVerificationEmailSender emailSender,
    IAcademicInternshipClient academicClient,
    TimeProvider timeProvider,
    IOptions<InternshipEmailOptions> emailOptions) : IInternshipVerificationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<InternshipRequestCreatedDto> SubmitAsync(
        Guid studentId,
        SubmitInternshipRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.EndDate.HasValue && request.EndDate.Value < request.StartDate)
        {
            throw new ArgumentException("EndDate must be greater than or equal to StartDate.");
        }

        var portalBaseUrl = emailOptions.Value.PortalBaseUrl.TrimEnd('/');
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var token = Guid.NewGuid();
        var verificationLink = new Uri(
            $"{portalBaseUrl}/verify-internship?token={Uri.EscapeDataString(token.ToString())}",
            UriKind.Absolute);
        var student = await academicClient.GetStudentVerificationProfileAsync(
            studentId,
            cancellationToken) ?? throw new InvalidOperationException(
                "The student verification profile could not be loaded.");
        var document = new InternshipVerificationDocument(
            new InternshipDeclaration(
                request.CompanyName.Trim(),
                request.Position.Trim(),
                request.MentorEmail.Trim(),
                request.StartDate,
                request.EndDate,
                NullIfWhiteSpace(request.TaskDescription)),
            null,
            new InternshipStudentSnapshot(student.StudentName, student.StudentCode));

        var entity = new FormRequest
        {
            Id = Guid.NewGuid(),
            StudentId = studentId,
            CreationDate = now,
            UpdateDate = now,
            ApprovalName = string.Empty,
            Note = $"{document.Declaration.CompanyName} · {document.Declaration.Position}",
            Status = FormRequestWorkflowStatuses.Pending,
            EmployerToken = token,
            EmployerVerifiedStatus = 0,
            VerificationData = JsonSerializer.Serialize(document, JsonOptions),
            IsDeleted = false
        };

        dbContext.FormRequests.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        await emailSender.SendAsync(
            document.Declaration.MentorEmail,
            document.Declaration.CompanyName,
            verificationLink,
            cancellationToken);

        return new InternshipRequestCreatedDto(entity.Id, "PendingEmployerVerification", now);
    }

    public async Task<InternshipVerificationContextDto?> GetVerificationContextAsync(
        Guid token,
        CancellationToken cancellationToken)
    {
        var json = await dbContext.FormRequests.AsNoTracking()
            .Where(item =>
                item.EmployerToken == token &&
                item.EmployerVerifiedStatus == 0 &&
                !item.IsDeleted)
            .Select(item => item.VerificationData)
            .SingleOrDefaultAsync(cancellationToken);
        if (json is null)
        {
            return null;
        }

        var document = DeserializeDocument(json);
        var student = document.Student;
        return new InternshipVerificationContextDto(
            document.Declaration.CompanyName,
            student?.StudentName ?? "Sinh viên Cao Đắng Tây Đô",
            student?.StudentCode ?? string.Empty,
            document.Declaration.Position,
            document.Declaration.StartDate,
            document.Declaration.EndDate,
            document.Declaration.TaskDescription);
    }

    public async Task<InternshipVerificationResultDto?> VerifyByEmployerAsync(
        VerifyInternshipByEmployerDto request,
        CancellationToken cancellationToken)
    {
        if (request.Score is null or < 0 or > 10)
        {
            throw new ArgumentException("Score must be between 0 and 10.");
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        var entity = await dbContext.FormRequests
            .SingleOrDefaultAsync(
                item => item.EmployerToken == request.Token &&
                    item.EmployerVerifiedStatus == 0 &&
                    !item.IsDeleted,
                cancellationToken);
        if (entity is null)
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        var document = DeserializeDocument(entity.VerificationData);
        var verifiedAt = timeProvider.GetUtcNow().UtcDateTime;
        entity.EmployerVerifiedStatus = request.IsInformationCorrect ? 1 : 2;
        entity.VerificationData = JsonSerializer.Serialize(
            document with
            {
                EmployerAssessment = new EmployerAssessment(
                    request.IsInformationCorrect,
                    request.Score!.Value,
                    NullIfWhiteSpace(request.EvaluationNotes),
                    verifiedAt)
            },
            JsonOptions);
        entity.UpdateDate = verifiedAt;

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new InternshipVerificationResultDto(entity.Id, entity.EmployerVerifiedStatus, verifiedAt);
    }

    public async Task<InternshipApprovalResultDto?> ApproveAsync(
        Guid requestId,
        Guid? administratorId,
        string administratorName,
        CancellationToken cancellationToken)
    {
        var entity = await dbContext.FormRequests
            .SingleOrDefaultAsync(
                item => item.Id == requestId && !item.IsDeleted,
                cancellationToken);
        if (entity is null)
        {
            return null;
        }

        if (entity.EmployerVerifiedStatus != 1)
        {
            throw new InvalidOperationException("The employer must confirm the internship before school approval.");
        }

        var document = DeserializeDocument(entity.VerificationData);
        var declaration = document.Declaration;
        var internshipId = await academicClient.CreateApprovedInternshipAsync(
            new CreateApprovedInternshipDto(
                entity.Id,
                entity.StudentId,
                declaration.CompanyName,
                declaration.Position,
                declaration.StartDate,
                declaration.EndDate,
                declaration.TaskDescription),
            cancellationToken);

        var approvedAt = timeProvider.GetUtcNow().UtcDateTime;
        entity.Status = FormRequestWorkflowStatuses.Approved;
        entity.ApprovalId = administratorId;
        entity.ApprovalName = string.IsNullOrWhiteSpace(administratorName) ? "Administrator" : administratorName.Trim();
        entity.UpdateDate = approvedAt;
        await dbContext.SaveChangesAsync(cancellationToken);

        return new InternshipApprovalResultDto(entity.Id, internshipId, approvedAt);
    }

    private static InternshipVerificationDocument DeserializeDocument(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException("The internship declaration is missing.");
        }

        try
        {
            return JsonSerializer.Deserialize<InternshipVerificationDocument>(json, JsonOptions)
                ?? throw new InvalidOperationException("The internship declaration is invalid.");
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("The internship declaration is invalid.", exception);
        }
    }

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
