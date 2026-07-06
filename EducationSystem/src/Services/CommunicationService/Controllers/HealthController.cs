using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace CommunicationService.Controllers;

[ApiController]
[Route("api/communication/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponse<object>> Get()
    {
        var data = new
        {
            service = "Communication Service",
            status = "Healthy",
            timestamp = DateTime.UtcNow
        };

        return Ok(ApiResponse<object>.Ok(data, "Communication Service is healthy"));
    }
}
