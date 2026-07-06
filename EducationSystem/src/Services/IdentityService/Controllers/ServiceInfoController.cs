using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/identity/info")]
public sealed class ServiceInfoController : ControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponse<object>> Get()
    {
        var data = new
        {
            service = "Identity Service",
            schema = "identity",
            domain = "Identity and access foundation",
            databaseConnected = false,
            rabbitMqEnabled = false,
            grpcEnabled = false,
            apiGatewayEnabled = false,
            responsibilities = new[]
            {
                "Users",
                "UserDevices",
                "PasswordResets",
                "AuditLogs",
                "Settings"
            }
        };

        return Ok(ApiResponse<object>.Ok(data, "Identity Service information"));
    }
}
