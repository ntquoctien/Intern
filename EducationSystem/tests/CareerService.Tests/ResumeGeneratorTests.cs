using System.Net;
using System.Text;
using CareerService.Application;
using CareerService.Application.DTOs.Resume;
using CareerService.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CareerService.Tests;

public sealed class ResumeGeneratorTests
{
    [Fact]
    public async Task VaultResponse_WithTrailingMarkdown_IsParsedWithoutRetry()
    {
        var handler = new VaultResumeHandler();
        var service = new LlmResumeGeneratorService(
            new HttpClient(handler),
            Options.Create(new ResumeLlmOptions
            {
                Provider = "Vault",
                BaseUrl = "https://vault.test/v1",
                Model = "test-model",
                ApiKey = "test-secret",
                MaxRetries = 2
            }),
            NullLogger<LlmResumeGeneratorService>.Instance);

        var result = await service.GenerateOptimizedResumeAsync(
            new ResumePromptPayloadDto
            {
                StudentInfo = new StudentInfoPromptDto
                {
                    FullName = "Nguyễn Văn A",
                    StudentCode = "SV001",
                    MajorName = "Công nghệ thông tin",
                    AcademicYear = "2022 - 2026",
                    Gpa = 8.2
                },
                TargetRole = "Backend Developer",
                JobDescription = "Phát triển REST API bằng C# và SQL Server."
            },
            CancellationToken.None);

        Assert.Equal(1, handler.RequestCount);
        Assert.Equal("Nguyễn Văn A", result.Header.FullName);
        Assert.Equal("SV001", result.Header.StudentCode);
        Assert.Equal("Backend Developer", result.Header.TargetRole);
    }

    [Fact]
    public async Task VaultResponse_WithProseBeforeFencedJson_IsParsedWithoutRetry()
    {
        var handler = new VaultResumeHandler(includeProsePrefix: true);
        var service = new LlmResumeGeneratorService(
            new HttpClient(handler),
            Options.Create(new ResumeLlmOptions
            {
                Provider = "Vault",
                BaseUrl = "https://vault.test/v1",
                Model = "test-model",
                ApiKey = "test-secret",
                MaxRetries = 2
            }),
            NullLogger<LlmResumeGeneratorService>.Instance);

        var result = await service.GenerateOptimizedResumeAsync(
            new ResumePromptPayloadDto
            {
                StudentInfo = new StudentInfoPromptDto
                {
                    FullName = "Nguyễn Văn A",
                    StudentCode = "SV001",
                    MajorName = "Công nghệ thông tin",
                    AcademicYear = "2022 - 2026",
                    Gpa = 8.2
                },
                TargetRole = "Backend Developer",
                JobDescription = "Phát triển REST API bằng C# và SQL Server."
            },
            CancellationToken.None);

        Assert.Equal(1, handler.RequestCount);
        Assert.Equal("Nguyễn Văn A", result.Header.FullName);
        Assert.Equal("SV001", result.Header.StudentCode);
        Assert.Equal("Backend Developer", result.Header.TargetRole);
    }

    private sealed class VaultResumeHandler(bool includeProsePrefix = false)
        : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            const string jsonObject =
                """
                {
                  "header": {},
                  "education": {},
                  "professionalSummary": "",
                  "skills": {
                    "knowledgeDomain": [],
                    "functionalSkills": [],
                    "interpersonalSkills": []
                  },
                  "projects": [],
                  "internships": [],
                  "certifications": [],
                  "awardsAndActivities": []
                }
                """;
            var resumeJson = includeProsePrefix
                ? $$"""
                Tôi sẽ phân tích dữ liệu nguồn trước khi biên soạn CV.

                ```json
                {{jsonObject}}
                ```
                """
                : $$"""
                {{jsonObject}}
                ---
                Nội dung trên đã được tối ưu.
                """;
            var body = System.Text.Json.JsonSerializer.Serialize(new
            {
                choices = new[]
                {
                    new { message = new { content = resumeJson } }
                }
            });
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
        }
    }
}
