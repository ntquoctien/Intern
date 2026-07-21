using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace SharedKernel;

public sealed record IssuedStudentToken(string AccessToken, DateTimeOffset ExpiresAt);

public interface IStudentTokenIssuer
{
    IssuedStudentToken Issue(Guid studentId, Guid? userId, string studentCode);
}

public sealed class StudentTokenIssuer(IOptions<StudentJwtOptions> options, TimeProvider timeProvider) : IStudentTokenIssuer
{
    public IssuedStudentToken Issue(Guid studentId, Guid? userId, string studentCode)
    {
        var jwtOptions = options.Value;
        var now = timeProvider.GetUtcNow();
        var expiresAt = now.Add(jwtOptions.EffectiveLifetime);
        var claims = new List<Claim>
        {
            new(StudentClaimTypes.StudentId, studentId.ToString()),
            new(StudentClaimTypes.StudentCode, studentCode),
            new(StudentClaimTypes.TokenType, StudentClaimTypes.StudentTokenType),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };
        if (userId.HasValue)
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));
        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new IssuedStudentToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
