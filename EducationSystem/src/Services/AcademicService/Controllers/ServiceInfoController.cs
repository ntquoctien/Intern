using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace AcademicService.Controllers;

[ApiController]
[Route("api/academic/info")]
public sealed class ServiceInfoController : ControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponse<object>> Get()
    {
        var data = new
        {
            service = "Academic Service",
            schema = "academic",
            domain = "Academic management",
            databaseConnected = false,
            rabbitMqEnabled = false,
            grpcEnabled = false,
            apiGatewayEnabled = false,
            responsibilities = new[]
            {
                "Faculties",
                "Majors",
                "AcademicYears",
                "Rooms",
                "Students",
                "TeacherFaculties",
                "Subjects",
                "SubjectDocuments",
                "SubjectTeachings",
                "SubjectTeachingTeachers",
                "SubjectStudents",
                "SubjectSchedules",
                "SubjectSpecialNotes",
                "Attendances",
                "SemesterPlans",
                "SemesterSubjects",
                "SemesterTuitions",
                "EvaluationCriterias",
                "StudentEvaluations",
                "StudentEvaluationDetails"
            }
        };

        return Ok(ApiResponse<object>.Ok(data, "Academic Service information"));
    }
}
