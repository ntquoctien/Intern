namespace SharedKernel;

public sealed class StudentJwtOptions
{
    public const string SectionName = "StudentJwt";
    public const int DefaultLifetimeMinutes = 15;
    public const int MaximumLifetimeMinutes = 30;

    public string Issuer { get; set; } = "EducationSystem.IdentityService";

    public string Audience { get; set; } = "EducationSystem.StudentPortal";

    public string SigningKey { get; set; } = string.Empty;

    public int LifetimeMinutes { get; set; } = DefaultLifetimeMinutes;

    public TimeSpan EffectiveLifetime => TimeSpan.FromMinutes(Math.Clamp(LifetimeMinutes, 1, MaximumLifetimeMinutes));
}

public static class StudentClaimTypes
{
    public const string StudentId = "studentId";
    public const string StudentCode = "studentCode";
    public const string TokenType = "tokenType";
    public const string StudentTokenType = "student";
    public const string StudentPolicy = "StudentOnly";
}
