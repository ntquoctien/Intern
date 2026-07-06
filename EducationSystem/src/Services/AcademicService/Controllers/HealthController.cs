using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace AcademicService.Controllers;

[ApiController]
[Route("api/academic/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponse<object>> Get()
    {
        var data = new
        {
            service = "Academic Service",
            status = "Healthy",
            timestamp = DateTime.UtcNow
        };

        return Ok(ApiResponse<object>.Ok(data, "Academic Service is healthy"));
    }
}
