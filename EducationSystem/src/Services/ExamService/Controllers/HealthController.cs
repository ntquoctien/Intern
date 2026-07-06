using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace ExamService.Controllers;

[ApiController]
[Route("api/exam/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponse<object>> Get()
    {
        var data = new
        {
            service = "Exam Service",
            status = "Healthy",
            timestamp = DateTime.UtcNow
        };

        return Ok(ApiResponse<object>.Ok(data, "Exam Service is healthy"));
    }
}
