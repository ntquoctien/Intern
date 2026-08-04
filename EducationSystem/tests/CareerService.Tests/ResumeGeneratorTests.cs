using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using CareerService.Application;
using CareerService.Application.DTOs.Resume;
using CareerService.Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CareerService.Tests;

public sealed class ResumeGeneratorTests
{
    [Fact]
    public void ResponseSchema_UsesCompactSkillKeywords()
    {
        var schema = LlmResumeGeneratorService.BuildResponseSchema();
        var skillProperties = schema["properties"]!["skills"]!["properties"]!
            ["functionalSkills"]!["items"]!["properties"]!.AsObject();

        Assert.True(skillProperties.ContainsKey("keywords"));
        Assert.False(skillProperties.ContainsKey("description"));
        Assert.Contains("TUYỆT ĐỐI KHÔNG sao chép", LlmResumeGeneratorService.SystemInstruction);
        Assert.Contains("đúng 3 câu", LlmResumeGeneratorService.SystemInstruction);
        Assert.Contains("RÀNG BUỘC CHỐNG ẢO GIÁC", LlmResumeGeneratorService.SystemInstruction);
        var bulletSchema = schema["properties"]!["projects"]!["items"]!["properties"]!
            ["actionBulletPoints"]!.AsObject();
        Assert.Equal(2, bulletSchema["minItems"]!.GetValue<int>());
        Assert.Equal(3, bulletSchema["maxItems"]!.GetValue<int>());
    }

    [Fact]
    public void FactGuard_LocksMetadata_ButKeepsProfessionalRewritesAndTechKeywords()
    {
        var source = new ResumePromptPayloadDto
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
            JobDescription = "Phát triển REST API bằng ASP.NET Core và SQL Server.",
            Projects =
            [
                new HydratedProjectDto
                {
                    ProjectId = 7,
                    ProjectName = "EducationSystem",
                    TechStack = "C#, ASP.NET Core, SQL Server, REST API, Git",
                    MyRole = "Backend Developer",
                    MyContributions = "Tôi đã làm API quản lý sinh viên và kết nối SQL Server.",
                    ProjectDescription = "Hệ thống quản lý giáo dục"
                }
            ],
            MatchedSubjects =
            [
                new HydratedMatchedSubjectDto
                {
                    SubjectName = "Cơ sở dữ liệu",
                    CourseOutcomes =
                    [
                        new HydratedOutcomeDto
                        {
                            OutcomeCode = "CLO1",
                            Name = "Trình bày các khái niệm về SQL",
                            Description = "Giải thích mô hình ERD và truy vấn SQL Server",
                            ProgressionLevel = "R"
                        }
                    ]
                }
            ]
        };
        var generated = new OptimizedResumeResponseDto
        {
            Header = new StudentHeaderDto
            {
                FullName = "Tên bịa",
                StudentCode = "FAKE",
                MajorName = "Ngành bịa",
                Gpa = 10
            },
            ProfessionalSummary =
                "Backend Developer có nền tảng ASP.NET Core, SQL Server và REST API qua dự án EducationSystem.",
            Skills = new CategorizedSkillsDto
            {
                FunctionalSkills =
                [
                    new CompactSkillItemDto
                    {
                        SkillName = "Trình bày các khái niệm Backend",
                        Proficiency = "Khá tốt",
                        Keywords =
                        [
                            " ASP.NET Core ",
                            "asp.net core",
                            "SQL Server",
                            "Kubernetes"
                        ]
                    }
                ]
            },
            Projects =
            [
                new OptimizedProjectDto
                {
                    ProjectId = 7,
                    ProjectName = "Dự án bịa",
                    TechStack = "Kubernetes",
                    MyRole = "CEO",
                    ActionBulletPoints =
                    [
                        "Xây dựng REST API quản lý sinh viên bằng ASP.NET Core và SQL Server, chuẩn hóa luồng dữ liệu đầu ra."
                    ]
                }
            ]
        };

        var guarded = LlmResumeGeneratorService.ApplySourceOfTruth(source, generated);

        Assert.Equal("Nguyễn Văn A", guarded.Header.FullName);
        Assert.Equal("SV001", guarded.Header.StudentCode);
        Assert.Equal(
            3,
            System.Text.RegularExpressions.Regex.Matches(
                guarded.ProfessionalSummary,
                @"[.!?]+(?=\s|$)").Count);
        Assert.Equal("EducationSystem", guarded.Projects.Single().ProjectName);
        Assert.Equal("C#, ASP.NET Core, SQL Server, REST API, Git", guarded.Projects.Single().TechStack);
        Assert.Contains(guarded.Projects.Single().ActionBulletPoints,
            bullet => bullet.StartsWith("Xây dựng REST API", StringComparison.Ordinal));
        var skill = guarded.Skills.FunctionalSkills.Single();
        Assert.DoesNotContain("Trình bày", skill.SkillName, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("ASP.NET Core", skill.Keywords);
        Assert.Single(skill.Keywords, keyword =>
            keyword.Equals("ASP.NET Core", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain("Kubernetes", skill.Keywords);
    }

    [Fact]
    public void FactGuard_DoesNotFallbackToRawContribution()
    {
        const string raw = "Tôi đã làm API quản lý sinh viên và kết nối SQL Server.";
        var source = new ResumePromptPayloadDto
        {
            StudentInfo = new StudentInfoPromptDto(),
            Projects =
            [
                new HydratedProjectDto
                {
                    ProjectId = 1,
                    ProjectName = "EducationSystem",
                    TechStack = "SQL Server",
                    MyRole = "Backend Developer",
                    MyContributions = raw
                }
            ]
        };
        var generated = new OptimizedResumeResponseDto
        {
            Projects =
            [
                new OptimizedProjectDto
                {
                    ProjectId = 1,
                    ActionBulletPoints = [raw]
                }
            ]
        };

        var guarded = LlmResumeGeneratorService.ApplySourceOfTruth(source, generated);

        Assert.NotEqual(raw, guarded.Projects.Single().ActionBulletPoints.Single());
        Assert.StartsWith("Triển khai", guarded.Projects.Single().ActionBulletPoints.Single());
    }

    [Fact]
    public void FactGuard_KeepsLowOverlapRewrite_ButRejectsInventedMetricsAndCompanies()
    {
        var source = new ResumePromptPayloadDto
        {
            StudentInfo = new StudentInfoPromptDto(),
            Projects =
            [
                new HydratedProjectDto
                {
                    ProjectId = 1,
                    ProjectName = "EducationSystem",
                    TechStack = "ASP.NET Core, SQL Server",
                    MyRole = "Backend Developer",
                    MyContributions = "Phụ trách API quản lý sinh viên.",
                    ProjectDescription = "Nền tảng quản lý giáo dục."
                }
            ]
        };
        const string professionalRewrite =
            "Thiết kế luồng xử lý nghiệp vụ rõ ràng, nâng cao khả năng bảo trì mã nguồn.";
        var generated = new OptimizedResumeResponseDto
        {
            Projects =
            [
                new OptimizedProjectDto
                {
                    ProjectId = 1,
                    ActionBulletPoints =
                    [
                        professionalRewrite,
                        "Tối ưu hóa truy vấn giúp tăng 99% hiệu năng xử lý.",
                        "Triển khai giải pháp cho Công ty Không Có."
                    ]
                }
            ]
        };

        var guarded = LlmResumeGeneratorService.ApplySourceOfTruth(source, generated);
        var bullets = guarded.Projects.Single().ActionBulletPoints;

        Assert.Contains(professionalRewrite, bullets);
        Assert.DoesNotContain(bullets, bullet => bullet.Contains("99%"));
        Assert.DoesNotContain(bullets, bullet => bullet.Contains("Công ty Không Có"));
    }

    [Fact]
    public void FactGuard_KeepsNaturalThreeSentenceSummaryWithoutTokenCoverageFallback()
    {
        var source = new ResumePromptPayloadDto
        {
            StudentInfo = new StudentInfoPromptDto(),
            TargetRole = "Backend Developer"
        };
        const string summary =
            "Backend Developer định hướng kiến tạo các dịch vụ web ổn định và dễ mở rộng. " +
            "Nổi bật với tư duy tổ chức mã nguồn mạch lạc và khả năng giải quyết yêu cầu nghiệp vụ. " +
            "Mong muốn phối hợp trong môi trường Agile để tạo ra sản phẩm thực tế có giá trị.";
        var generated = new OptimizedResumeResponseDto
        {
            ProfessionalSummary = summary
        };

        var guarded = LlmResumeGeneratorService.ApplySourceOfTruth(source, generated);

        Assert.Equal(summary, guarded.ProfessionalSummary);
    }

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

    [Fact]
    public async Task VaultResponse_AsServerSentEvents_IsParsedWithoutRetry()
    {
        var handler = new VaultResumeHandler(useStreamingResponse: true);
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
    public async Task GptSol_UsesVaultResponsesApi()
    {
        var handler = new VaultResponsesHandler();
        var service = new LlmResumeGeneratorService(
            new HttpClient(handler),
            Options.Create(new ResumeLlmOptions
            {
                Provider = "Vault",
                BaseUrl = "https://vault.test/v1",
                Model = "gpt-5.6-sol",
                ApiKey = "test-secret",
                MaxRetries = 0
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
        Assert.Equal("https://vault.test/v1/responses", handler.RequestUri);
        Assert.Equal("gpt-5.6-sol", handler.RequestModel);
        Assert.True(handler.HasInstructions);
        Assert.True(handler.HasInput);
        Assert.True(handler.IsStreaming);
        Assert.Equal("SV001", result.Header.StudentCode);
    }

    private sealed class VaultResponsesHandler : HttpMessageHandler
    {
        public int RequestCount { get; private set; }
        public string? RequestUri { get; private set; }
        public string? RequestModel { get; private set; }
        public bool HasInstructions { get; private set; }
        public bool HasInput { get; private set; }
        public bool IsStreaming { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestCount++;
            RequestUri = request.RequestUri?.ToString();
            var requestJson = await request.Content!.ReadAsStringAsync(cancellationToken);
            using var requestDocument = JsonDocument.Parse(requestJson);
            RequestModel = requestDocument.RootElement.GetProperty("model").GetString();
            HasInstructions = requestDocument.RootElement.TryGetProperty("instructions", out _);
            HasInput = requestDocument.RootElement.TryGetProperty("input", out _);
            IsStreaming = requestDocument.RootElement.GetProperty("stream").GetBoolean();

            const string resumeJson =
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
            var split = resumeJson.Length / 2;
            var firstEvent = JsonSerializer.Serialize(new
            {
                type = "response.output_text.delta",
                delta = resumeJson[..split]
            });
            var secondEvent = JsonSerializer.Serialize(new
            {
                type = "response.output_text.delta",
                delta = resumeJson[split..]
            });
            var body =
                $"event: response.output_text.delta\ndata: {firstEvent}\n\n" +
                $"event: response.output_text.delta\ndata: {secondEvent}\n\n" +
                "event: response.completed\ndata: {\"type\":\"response.completed\"}\n\n";
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "text/event-stream")
            };
        }
    }

    private sealed class VaultResumeHandler(
        bool includeProsePrefix = false,
        bool useStreamingResponse = false)
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
            string body;
            string mediaType;
            if (useStreamingResponse)
            {
                var split = resumeJson.Length / 2;
                body =
                    $"data: {{\"choices\":[{{\"delta\":{{\"content\":{System.Text.Json.JsonSerializer.Serialize(resumeJson[..split])}}}}}]}}\n\n" +
                    $"data: {{\"choices\":[{{\"delta\":{{\"content\":{System.Text.Json.JsonSerializer.Serialize(resumeJson[split..])}}}}}]}}\n\n" +
                    "data: [DONE]\n\n";
                mediaType = "text/event-stream";
            }
            else
            {
                body = System.Text.Json.JsonSerializer.Serialize(new
                {
                    choices = new[]
                    {
                        new { message = new { content = resumeJson } }
                    }
                });
                mediaType = "application/json";
            }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, mediaType)
            });
        }
    }
}
