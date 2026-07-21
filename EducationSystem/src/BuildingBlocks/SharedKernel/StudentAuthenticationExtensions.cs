using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace SharedKernel;

public static class StudentAuthenticationExtensions
{
    public static IServiceCollection AddStudentJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(StudentJwtOptions.SectionName);
        services.AddOptions<StudentJwtOptions>()
            .Bind(section)
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "StudentJwt:Issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "StudentJwt:Audience is required.")
            .Validate(options => Encoding.UTF8.GetByteCount(options.SigningKey) >= 32, "StudentJwt:SigningKey must contain at least 32 UTF-8 bytes.")
            .Validate(options => options.LifetimeMinutes is >= 1 and <= StudentJwtOptions.MaximumLifetimeMinutes,
                $"StudentJwt:LifetimeMinutes must be between 1 and {StudentJwtOptions.MaximumLifetimeMinutes}.")
            .ValidateOnStart();

        var options = section.Get<StudentJwtOptions>() ?? new StudentJwtOptions();
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentStudentAccessor, CurrentStudentAccessor>();
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.MapInboundClaims = false;
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = true,
                    ValidIssuer = options.Issuer,
                    ValidateAudience = true,
                    ValidAudience = options.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = JwtRegisteredClaimNames.Sub
                };
                jwt.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        var expired = context.AuthenticateFailure is SecurityTokenExpiredException;
                        var code = expired ? StudentErrorCodes.TokenExpired : StudentErrorCodes.Unauthorized;
                        var message = expired ? "Student access token has expired." : "A valid student access token is required.";
                        await context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(code, message));
                    }
                };
            });

        services.AddAuthorization(authorization =>
            authorization.AddPolicy(StudentClaimTypes.StudentPolicy, policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim(StudentClaimTypes.TokenType, StudentClaimTypes.StudentTokenType)
                    .RequireClaim(StudentClaimTypes.StudentId)
                    .RequireClaim(StudentClaimTypes.StudentCode)));

        return services;
    }
}

public sealed record CurrentStudent(Guid StudentId, Guid? UserId, string StudentCode);

public interface ICurrentStudentAccessor
{
    CurrentStudent GetRequiredStudent();
}

public sealed class CurrentStudentAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentStudentAccessor
{
    public CurrentStudent GetRequiredStudent()
    {
        var principal = httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true ||
            principal.FindFirstValue(StudentClaimTypes.TokenType) != StudentClaimTypes.StudentTokenType ||
            !Guid.TryParse(principal.FindFirstValue(StudentClaimTypes.StudentId), out var studentId))
        {
            throw new UnauthorizedAccessException("A valid student principal is required.");
        }

        var studentCode = principal.FindFirstValue(StudentClaimTypes.StudentCode);
        if (string.IsNullOrWhiteSpace(studentCode))
        {
            throw new UnauthorizedAccessException("The student code claim is missing.");
        }

        var subject = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return new CurrentStudent(studentId, Guid.TryParse(subject, out var userId) ? userId : null, studentCode);
    }
}
