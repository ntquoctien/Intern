using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/identity/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponse<object>> Get()
    {
        var data = new
        {
            service = "Identity Service",
            status = "Healthy",
            timestamp = DateTime.UtcNow
        };

        return Ok(ApiResponse<object>.Ok(data, "Identity Service is healthy"));
    }
}
