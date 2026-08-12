using System.Net;
using System.Net.Http.Json;
using System.Net.Mail;
using System.Text;
using CommunicationService.Application.DTOs.InternshipVerification;
using CommunicationService.Application.Interfaces;
using CommunicationService.Application.Options;
using Microsoft.Extensions.Options;
using SharedKernel;

namespace CommunicationService.Application.Services;

public sealed class SmtpInternshipVerificationEmailSender(
    IOptions<InternshipEmailOptions> options,
    ILogger<SmtpInternshipVerificationEmailSender> logger) : IInternshipVerificationEmailSender
{
    public async Task SendAsync(
        string mentorEmail,
        string companyName,
        Uri verificationLink,
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        EnsureConfigured(settings);

        using var message = new MailMessage
        {
            From = new MailAddress(settings.FromAddress, settings.FromName, Encoding.UTF8),
            Subject = $"[Cao Đẳng Tây Đô] Xác nhận thông tin thực tập tại {companyName}",
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true,
            Body = BuildHtmlBody(companyName, verificationLink)
        };
        message.To.Add(new MailAddress(mentorEmail));

        using var client = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
        {
            EnableSsl = settings.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Timeout = checked(settings.TimeoutSeconds * 1000)
        };
        if (!string.IsNullOrWhiteSpace(settings.Username))
        {
            client.Credentials = new NetworkCredential(settings.Username, settings.Password);
        }

        try
        {
            await client.SendMailAsync(message, cancellationToken);
            logger.LogInformation(
                "Internship verification email sent to {MentorEmail} for {CompanyName}.",
                mentorEmail,
                companyName);
        }
        catch (Exception exception) when (
            exception is SmtpException or InvalidOperationException)
        {
            logger.LogError(
                exception,
                "Failed to send internship verification email to {MentorEmail}.",
                mentorEmail);
            throw;
        }
    }

    private static void EnsureConfigured(InternshipEmailOptions settings)
    {
        if (string.IsNullOrWhiteSpace(settings.SmtpHost) ||
            string.IsNullOrWhiteSpace(settings.FromAddress))
        {
            throw new InvalidOperationException(
                "InternshipEmail SMTP settings are missing. Configure SmtpHost and FromAddress.");
        }

        if (!string.IsNullOrWhiteSpace(settings.Username) &&
            string.IsNullOrWhiteSpace(settings.Password))
        {
            throw new InvalidOperationException(
                "InternshipEmail:Password is required when Username is configured.");
        }
    }

    private static string BuildHtmlBody(string companyName, Uri verificationLink)
    {
        var safeCompany = WebUtility.HtmlEncode(companyName);
        var safeLink = WebUtility.HtmlEncode(verificationLink.AbsoluteUri);
        return $$"""
            <!doctype html>
            <html lang="vi">
            <body style="margin:0;background:#f3f5f7;font-family:Arial,sans-serif;color:#243746">
              <div style="max-width:620px;margin:32px auto;background:#fff;border:1px solid #e1e6eb;border-radius:16px;overflow:hidden">
                <div style="background:#002140;padding:22px 28px;color:#fff">
                  <strong style="font-size:18px">Cao Đẳng Tây Đô</strong>
                  <div style="margin-top:4px;font-size:13px;opacity:.82">Cổng xác thực thực tập doanh nghiệp</div>
                </div>
                <div style="padding:28px">
                  <p>Kính gửi Quý doanh nghiệp <strong>{{safeCompany}}</strong>,</p>
                  <p>Một sinh viên Cao Đẳng Tây Đô đã khai báo quá trình thực tập tại doanh nghiệp và đề nghị người hướng dẫn xác nhận thông tin.</p>
                  <p style="margin:26px 0">
                    <a href="{{safeLink}}" style="display:inline-block;background:#002140;color:#fff;text-decoration:none;padding:13px 22px;border-radius:9px;font-weight:700">
                      Xác nhận thông tin thực tập
                    </a>
                  </p>
                  <p style="font-size:13px;color:#667788">Liên kết là mã bảo mật dùng một lần. Nếu nút không hoạt động, vui lòng mở đường dẫn:</p>
                  <p style="font-size:12px;word-break:break-all;color:#0b5e9b">{{safeLink}}</p>
                  <p style="font-size:12px;color:#81909d;margin-top:24px">Nếu Quý doanh nghiệp không liên quan đến yêu cầu này, vui lòng bỏ qua email.</p>
                </div>
              </div>
            </body>
            </html>
            """;
    }
}

public sealed class AcademicInternshipClient(
    IHttpClientFactory httpClientFactory) : IAcademicInternshipClient
{
    public async Task<AcademicStudentVerificationProfileDto?> GetStudentVerificationProfileAsync(
        Guid studentId,
        CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient("AcademicService");
        using var response = await httpClient.GetAsync(
            $"api/academic/internal/students/{studentId}/internship-verification-profile",
            cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<
            ApiResponse<AcademicStudentVerificationProfileDto>>(
            cancellationToken: cancellationToken);
        return payload is { Success: true } ? payload.Data : null;
    }

    public async Task<int> CreateApprovedInternshipAsync(
        CreateApprovedInternshipDto request,
        CancellationToken cancellationToken)
    {
        var httpClient = httpClientFactory.CreateClient("AcademicService");
        using var response = await httpClient.PostAsJsonAsync(
            "api/academic/internships/sync-approved",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<ApiResponse<int>>(
            cancellationToken: cancellationToken);
        return payload is { Success: true, Data: > 0 }
            ? payload.Data
            : throw new HttpRequestException("AcademicService returned an invalid internship response.");
    }
}
