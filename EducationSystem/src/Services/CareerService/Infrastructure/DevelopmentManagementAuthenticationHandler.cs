using System.Security.Claims;
using System.Text.Encodings.Web;
using CareerService.Application;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace CareerService.Infrastructure;

public sealed class DevelopmentManagementAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> schemeOptions,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IWebHostEnvironment environment,
    IOptions<ManagementAuthOptions> managementOptions)
    : AuthenticationHandler<AuthenticationSchemeOptions>(schemeOptions, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!environment.IsDevelopment() || !managementOptions.Value.AllowDevelopmentBypass)
            return Task.FromResult(AuthenticateResult.NoResult());

        var identity = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, "development-system-admin"),
            new Claim(ClaimTypes.Name, "Development System Admin"),
            new Claim(ClaimTypes.Role, "SystemAdministrator")
        ], Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(
            new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }
}
