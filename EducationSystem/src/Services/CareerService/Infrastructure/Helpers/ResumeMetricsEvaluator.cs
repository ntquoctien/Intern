using System.Text;
using System.Text.RegularExpressions;
using CareerService.Application.DTOs.Resume;

namespace CareerService.Infrastructure.Helpers;

public static partial class ResumeMetricsEvaluator
{
    public static ResumeQualityMetricsDto Evaluate(
        ResumePromptPayloadDto source,
        OptimizedResumeResponseDto generated)
    {
        var sourceText = BuildSourceText(source);
        var generatedText = BuildGeneratedText(generated);
        var preservation = SourceCoverage(sourceText, generatedText);
        var alignment = SourceCoverage(source.JobDescription, generatedText);

        return new ResumeQualityMetricsDto
        {
            JobAlignmentScore = alignment,
            ContentPreservationScore = preservation,
            HasHallucinationWarning = alignment > 0.80 && preservation < 0.60
        };
    }

    public static double SourceCoverage(string sourceText, string candidateText)
    {
        var source = Tokenize(sourceText);
        if (source.Count == 0) return 0;
        var candidate = Tokenize(candidateText);
        var intersection = source.Count(candidate.Contains);
        return Math.Round((double)intersection / source.Count, 4);
    }

    internal static HashSet<string> Tokenize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        return TokenPattern()
            .Matches(text.Normalize(NormalizationForm.FormC).ToLowerInvariant())
            .Select(match => match.Value)
            .Where(token => token.Length > 1 && !StopWords.Contains(token))
            .ToHashSet(StringComparer.Ordinal);
    }

    internal static string BuildSourceText(ResumePromptPayloadDto source)
    {
        var parts = new List<string>
        {
            source.StudentInfo.FullName,
            source.StudentInfo.StudentCode,
            source.StudentInfo.MajorName,
            source.StudentInfo.FacultyName ?? string.Empty,
            source.StudentInfo.AcademicYear,
            source.StudentInfo.Gpa.HasValue ? "GPA" : string.Empty,
            source.StudentInfo.Gpa?.ToString(
                System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            source.TargetRole,
            source.CareerFocusTag ?? string.Empty,
            source.JobDescription,
            source.CurrentSummaryDraft ?? string.Empty
        };
        parts.AddRange(source.MatchedSubjects.SelectMany(subject =>
            new[]
            {
                subject.SubjectCode,
                subject.SubjectName,
                subject.Score.ToString(System.Globalization.CultureInfo.InvariantCulture)
            }.Concat(subject.CourseOutcomes.SelectMany(outcome =>
                new[]
                {
                    outcome.OutcomeCode,
                    outcome.Name,
                    outcome.Description,
                    outcome.ProgressionLevel
                }))));
        parts.AddRange(source.Projects.SelectMany(project => new[]
        {
            project.ProjectName,
            project.TechStack,
            project.ProjectDescription,
            project.MyRole,
            project.MyContributions,
            project.TeamSize.ToString(
                System.Globalization.CultureInfo.InvariantCulture),
            project.MappedCourseCode ?? string.Empty
        }));
        parts.AddRange(source.Internships.SelectMany(internship => new[]
        {
            internship.CompanyName,
            internship.Position,
            internship.StartDate.ToString("O"),
            internship.EndDate?.ToString("O") ?? string.Empty,
            internship.TaskDescription
        }));
        parts.AddRange(source.Certifications);
        parts.AddRange(source.AwardsAndActivities);
        return string.Join(' ', parts);
    }

    internal static string BuildGeneratedText(OptimizedResumeResponseDto generated)
    {
        var parts = new List<string>
        {
            generated.Header.FullName,
            generated.Header.StudentCode,
            generated.Header.MajorName,
            generated.Education.InstitutionName,
            generated.Education.MajorName,
            generated.Education.DegreeName,
            generated.Header.Gpa?.ToString(
                System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty,
            generated.Header.TargetRole,
            generated.ProfessionalSummary
        };
        parts.AddRange(AllSkills(generated.Skills).SelectMany(skill =>
            new[] { skill.SkillName, skill.Proficiency, skill.Description }));
        parts.AddRange(generated.Projects.SelectMany(project =>
            new[]
            {
                project.ProjectName,
                project.TechStack,
                project.MyRole
            }.Concat(project.ActionBulletPoints)));
        parts.AddRange(generated.Internships.SelectMany(internship =>
            new[]
            {
                internship.CompanyName,
                internship.Position,
                internship.DurationText
            }.Concat(internship.ActionBulletPoints)));
        parts.AddRange(generated.Certifications);
        parts.AddRange(generated.AwardsAndActivities);
        return string.Join(' ', parts);
    }

    private static IEnumerable<SkillItemDto> AllSkills(CategorizedSkillsDto skills) =>
        skills.KnowledgeDomain
            .Concat(skills.FunctionalSkills)
            .Concat(skills.InterpersonalSkills);

    private static readonly HashSet<string> StopWords =
    [
        "và", "là", "của", "có", "cho", "trong", "với", "được", "các", "một",
        "những", "để", "từ", "theo", "về", "trên", "tại", "đã", "khi", "and",
        "the", "for", "with", "from", "that", "this", "into", "are", "was"
    ];

    [GeneratedRegex(@"[\p{L}\p{N}][\p{L}\p{N}+#.\-]*", RegexOptions.CultureInvariant)]
    private static partial Regex TokenPattern();
}
