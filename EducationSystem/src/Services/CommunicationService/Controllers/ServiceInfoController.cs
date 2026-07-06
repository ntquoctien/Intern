using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace CommunicationService.Controllers;

[ApiController]
[Route("api/communication/info")]
public sealed class ServiceInfoController : ControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponse<object>> Get()
    {
        var data = new
        {
            service = "Communication Service",
            schema = "communication",
            domain = "Communication and request management",
            databaseConnected = false,
            rabbitMqEnabled = false,
            grpcEnabled = false,
            apiGatewayEnabled = false,
            responsibilities = new[]
            {
                "FormTemplates",
                "FormRequests",
                "UserAnnouncements"
            }
        };

        return Ok(ApiResponse<object>.Ok(data, "Communication Service information"));
    }
}
