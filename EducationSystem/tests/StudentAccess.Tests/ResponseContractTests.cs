using System.Reflection;
using Xunit;

namespace StudentAccess.Tests;

public sealed class ResponseContractTests
{
    [Fact]
    public void StudentAndManagementContracts_ExcludeProhibitedSensitiveFields()
    {
        var assemblies = new[]
        {
            typeof(AcademicService.Application.DTOs.StudentAccess.StudentProfileDto).Assembly,
            typeof(IdentityService.Application.StudentAccess.StudentSessionDto).Assembly,
            typeof(ExamService.Application.DTOs.StudentAccess.StudentExamResultDto).Assembly,
            typeof(CommunicationService.Application.DTOs.StudentAccess.StudentFormRequestDto).Assembly
        }.Distinct();
        var prohibited = new[]
        {
            "password", "salt", "identification", "address", "family", "father",
            "mother", "spouse", "mobile", "push", "deviceToken"
        };

        var exposed = assemblies.SelectMany(assembly => assembly.GetExportedTypes())
            .Where(type => type.Namespace?.Contains("StudentAccess") == true || type.Namespace?.Contains("DTOs.Management") == true)
            .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(property => $"{type.FullName}.{property.Name}"))
            .Where(name => prohibited.Any(word => name.Contains(word, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        Assert.Empty(exposed);
    }
}
