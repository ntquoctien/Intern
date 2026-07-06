using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace ExamService.Controllers;

[ApiController]
[Route("api/exam/info")]
public sealed class ServiceInfoController : ControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponse<object>> Get()
    {
        var data = new
        {
            service = "Exam Service",
            schema = "exam",
            domain = "Exam and assessment management",
            databaseConnected = false,
            rabbitMqEnabled = false,
            grpcEnabled = false,
            apiGatewayEnabled = false,
            responsibilities = new[]
            {
                "QuestionSuites",
                "Questions",
                "QuestionAnswers",
                "SubjectTeachingExams",
                "ExamAttempts",
                "ExamQuestionSelections",
                "ExamQuestionAnswers",
                "ExamResults"
            }
        };

        return Ok(ApiResponse<object>.Ok(data, "Exam Service information"));
    }
}
