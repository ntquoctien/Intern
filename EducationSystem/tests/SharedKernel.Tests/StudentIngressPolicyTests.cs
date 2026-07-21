using SharedKernel;
using Xunit;

namespace SharedKernel.Tests;

public sealed class StudentIngressPolicyTests
{
    [Theory]
    [InlineData("/api/auth/student/login")]
    [InlineData("/api/student/me/profile")]
    [InlineData("/api/student/me/exam-results")]
    [InlineData("/api/student/me/form-requests/11111111-1111-1111-1111-111111111111")]
    public void PublicStudentPaths_AreAllowed(string path) => Assert.True(StudentIngressPolicy.IsAllowedPath(path));

    [Theory]
    [InlineData("/api/academic/students")]
    [InlineData("/api/management/students")]
    [InlineData("/api/identity/users")]
    [InlineData("/api/internal/users/summaries")]
    [InlineData("/api/student/me/../management/students")]
    public void GenericOrTraversalPaths_AreRejected(string path) => Assert.False(StudentIngressPolicy.IsAllowedPath(path));

    [Fact]
    public void NginxIngress_HasFailClosedApiFallback_AndNoManagementProxy()
    {
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "EducationSystem.sln"))) root = root.Parent;
        Assert.NotNull(root);
        var configuration = File.ReadAllText(Path.Combine(root!.FullName, "deploy", "nginx", "student-portal.conf"));
        Assert.Contains("location ^~ /api/ { return 404; }", configuration);
        Assert.DoesNotContain("location /api/management", configuration);
        Assert.DoesNotContain("location /api/academic", configuration);
    }
}
