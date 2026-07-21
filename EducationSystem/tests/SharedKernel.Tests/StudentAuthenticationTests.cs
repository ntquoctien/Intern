using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedKernel;
using Xunit;

namespace SharedKernel.Tests;

public sealed class StudentAuthenticationTests
{
    private const string SigningKey = "test-only-signing-key-at-least-thirty-two-bytes";
    private static readonly Guid StudentId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid UserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

    [Fact]
    public void Token_ContainsOnlyRequiredStudentClaims_AndCapsLifetime()
    {
        var now = new DateTimeOffset(2026, 7, 21, 0, 0, 0, TimeSpan.Zero);
        var options = Options.Create(NewOptions(lifetimeMinutes: 30));
        var issuer = new StudentTokenIssuer(options, new FixedTimeProvider(now));

        var issued = issuer.Issue(StudentId, UserId, "SV000001");
        var token = new JwtSecurityTokenHandler().ReadJwtToken(issued.AccessToken);

        Assert.Equal(now.AddMinutes(30), issued.ExpiresAt);
        Assert.Equal(StudentId.ToString(), token.Claims.Single(x => x.Type == StudentClaimTypes.StudentId).Value);
        Assert.Equal("SV000001", token.Claims.Single(x => x.Type == StudentClaimTypes.StudentCode).Value);
        Assert.Equal(StudentClaimTypes.StudentTokenType, token.Claims.Single(x => x.Type == StudentClaimTypes.TokenType).Value);
        Assert.DoesNotContain(token.Claims, x => x.Type.Contains("password", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-jwt")]
    public void MissingOrMalformedToken_IsRejected(string? token)
    {
        var handler = new JwtSecurityTokenHandler();
        Assert.ThrowsAny<Exception>(() => handler.ValidateToken(token, ValidationParameters(), out _));
    }

    [Fact]
    public void WrongIssuer_IsRejected()
    {
        var token = CreateToken(issuer: "wrong-issuer");
        Assert.Throws<SecurityTokenInvalidIssuerException>(() =>
            new JwtSecurityTokenHandler().ValidateToken(token, ValidationParameters(), out _));
    }

    [Fact]
    public void WrongAudience_IsRejected()
    {
        var token = CreateToken(audience: "wrong-audience");
        Assert.Throws<SecurityTokenInvalidAudienceException>(() =>
            new JwtSecurityTokenHandler().ValidateToken(token, ValidationParameters(), out _));
    }

    [Fact]
    public void ExpiredToken_IsRejected()
    {
        var token = CreateToken(expires: DateTime.UtcNow.AddMinutes(-2));
        Assert.Throws<SecurityTokenExpiredException>(() =>
            new JwtSecurityTokenHandler().ValidateToken(token, ValidationParameters(), out _));
    }

    [Fact]
    public void WrongTokenType_IsRejectedByCurrentStudentAccessor()
    {
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(StudentClaimTypes.StudentId, StudentId.ToString()),
                new Claim(StudentClaimTypes.StudentCode, "SV000001"),
                new Claim(StudentClaimTypes.TokenType, "management")
            ], "test"))
        };
        var accessor = new CurrentStudentAccessor(new HttpContextAccessor { HttpContext = context });

        Assert.Throws<UnauthorizedAccessException>(() => accessor.GetRequiredStudent());
    }

    private static StudentJwtOptions NewOptions(int lifetimeMinutes = 15) => new()
    {
        Issuer = "test-issuer",
        Audience = "test-audience",
        SigningKey = SigningKey,
        LifetimeMinutes = lifetimeMinutes
    };

    private static TokenValidationParameters ValidationParameters() => new()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
        ValidateIssuer = true,
        ValidIssuer = "test-issuer",
        ValidateAudience = true,
        ValidAudience = "test-audience",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };

    private static string CreateToken(
        string issuer = "test-issuer",
        string audience = "test-audience",
        DateTime? expires = null)
    {
        var token = new JwtSecurityToken(
            issuer,
            audience,
            [new Claim(StudentClaimTypes.TokenType, StudentClaimTypes.StudentTokenType)],
            notBefore: DateTime.UtcNow.AddMinutes(-5),
            expires: expires ?? DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
                SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
