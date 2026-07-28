using System.ComponentModel.DataAnnotations;

namespace CommunicationService.Application.Options;

public sealed class InternshipEmailOptions
{
    public const string SectionName = "InternshipEmail";

    [Required]
    public string PortalBaseUrl { get; set; } = string.Empty;

    [Required]
    public string SmtpHost { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int SmtpPort { get; set; } = 587;

    public bool EnableSsl { get; set; } = true;

    [Required, EmailAddress]
    public string FromAddress { get; set; } = string.Empty;

    public string FromName { get; set; } = "Cổng sinh viên Đại học Tây Đô";

    public string? Username { get; set; }

    public string? Password { get; set; }

    [Range(1, 120)]
    public int TimeoutSeconds { get; set; } = 30;
}
