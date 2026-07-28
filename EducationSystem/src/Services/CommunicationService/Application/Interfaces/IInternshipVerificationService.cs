using CommunicationService.Application.DTOs.InternshipVerification;

namespace CommunicationService.Application.Interfaces;

public interface IInternshipVerificationService
{
    Task<InternshipRequestCreatedDto> SubmitAsync(
        Guid studentId,
        SubmitInternshipRequestDto request,
        CancellationToken cancellationToken);

    Task<InternshipVerificationResultDto?> VerifyByEmployerAsync(
        VerifyInternshipByEmployerDto request,
        CancellationToken cancellationToken);

    Task<InternshipVerificationContextDto?> GetVerificationContextAsync(
        Guid token,
        CancellationToken cancellationToken);

    Task<InternshipApprovalResultDto?> ApproveAsync(
        Guid requestId,
        Guid? administratorId,
        string administratorName,
        CancellationToken cancellationToken);
}

public interface IInternshipVerificationEmailSender
{
    Task SendAsync(
        string mentorEmail,
        string companyName,
        Uri verificationLink,
        CancellationToken cancellationToken);
}

public interface IAcademicInternshipClient
{
    Task<AcademicStudentVerificationProfileDto?> GetStudentVerificationProfileAsync(
        Guid studentId,
        CancellationToken cancellationToken);

    Task<int> CreateApprovedInternshipAsync(
        CreateApprovedInternshipDto request,
        CancellationToken cancellationToken);
}
