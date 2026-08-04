using System.Net;
using System.Text.RegularExpressions;
using CareerService.Application;
using CareerService.Application.DTOs.Resume;
using SharedKernel;

namespace CareerService.Infrastructure;

public sealed class ResumeContextHydrationService(
    IVectorMatchClient vectorMatchClient,
    IAcademicResumeClient academicResumeClient,
    ILogger<ResumeContextHydrationService> logger) : IResumeContextHydrationService
{
    private static readonly Regex HtmlTagPattern =
        new("<[^>]*>", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex ActiveContentPattern =
        new(
            @"<(script|style)\b[^>]*>.*?</\1>",
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant |
            RegexOptions.IgnoreCase |
            RegexOptions.Singleline);
    private static readonly Regex WhitespacePattern =
        new(@"\s+", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex EmailPattern =
        new(
            @"\b[\w.!#$%&'*+/=?^`{|}~-]+@[\w.-]+\.[A-Za-z]{2,}\b",
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant |
            RegexOptions.IgnoreCase);
    private static readonly Regex UrlPattern =
        new(
            @"\b(?:https?://|www\.|github\.com/)\S+",
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant |
            RegexOptions.IgnoreCase);
    private static readonly Regex PhonePattern =
        new(
            @"(?<!\d)(?:\+?\d[\d\s().-]{7,}\d)(?!\d)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public async Task<ResumePromptPayloadDto> HydrateResumeContextAsync(
        PrepareResumePayloadRequestDto request,
        Guid authenticatedStudentId,
        Guid? authenticatedUserId,
        string bearerToken,
        CancellationToken cancellationToken)
    {
        if (request.StudentId == Guid.Empty ||
            request.StudentId != authenticatedStudentId)
            throw new DownstreamApiException(
                "RESUME_CONTEXT_NOT_FOUND",
                "Resume context was not found.",
                StatusCodes.Status404NotFound);
        if ((request.SelectedSubjectIds?.Count ?? 0) > 100)
            throw new DownstreamApiException(
                "TOO_MANY_SELECTED_SUBJECTS",
                "At most 100 subjects may be selected.",
                StatusCodes.Status400BadRequest);
        if ((request.SelectedInternshipIds?.Count ?? 0) > 100)
            throw new DownstreamApiException(
                "TOO_MANY_SELECTED_INTERNSHIPS",
                "At most 100 internships may be selected.",
                StatusCodes.Status400BadRequest);
        if ((request.UiProjects?.Count ?? 0) > 20)
            throw new DownstreamApiException(
                "TOO_MANY_UI_PROJECTS",
                "At most 20 UI projects may be supplied.",
                StatusCodes.Status400BadRequest);
        if (string.IsNullOrWhiteSpace(bearerToken))
            throw new DownstreamApiException(
                "STUDENT_TOKEN_REQUIRED",
                "A student bearer token is required.",
                StatusCodes.Status401Unauthorized);
        if ((request.Certifications?.Count ?? 0) > 20)
            throw new DownstreamApiException(
                "TOO_MANY_CERTIFICATIONS",
                "At most 20 certifications may be supplied.",
                StatusCodes.Status400BadRequest);
        if ((request.AwardsAndActivities?.Count ?? 0) > 20)
            throw new DownstreamApiException(
                "TOO_MANY_AWARDS_AND_ACTIVITIES",
                "At most 20 awards or activities may be supplied.",
                StatusCodes.Status400BadRequest);

        var selectedIds = (request.SelectedSubjectIds ?? [])
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();
        var vectorRequest = new VectorMatchRequestContract(
            request.StudentId,
            CleanPromptText(request.JobDescription, 10_000),
            selectedIds.Count == 0 ? null : selectedIds,
            request.TopK,
            request.SimilarityThreshold);

        var vectorTask = vectorMatchClient.MatchAsync(
            vectorRequest, cancellationToken);
        var academicTask = academicResumeClient.GetContextAsync(
            request.StudentId, bearerToken, cancellationToken);
        try
        {
            await Task.WhenAll(vectorTask, academicTask);
        }
        catch
        {
            // Await the individual tasks below so AcademicService failures still
            // propagate while a VectorMatchService outage can use raw data.
        }

        var academic = await academicTask;
        VectorMatchResponseContract? vector = null;
        try
        {
            vector = await vectorTask;
        }
        catch (Exception exception) when (
            IsVectorMatchUnavailable(exception, cancellationToken))
        {
            logger.LogWarning(
                exception,
                "VectorMatchService is unavailable for student {StudentId}; " +
                "using score-based resume context.",
                request.StudentId);
        }

        EnsureOwnership(request.StudentId, authenticatedUserId, vector, academic);

        var selectedSet = selectedIds.ToHashSet();
        var courses = academic.EligibleCourses
            .Where(course =>
                selectedSet.Count == 0 || selectedSet.Contains(course.SubjectId))
            .ToDictionary(
                course => course.SubjectCode.Trim(),
                StringComparer.OrdinalIgnoreCase);
        var matchedBySubject = (vector?.TopMatchedOutcomes ?? [])
            .Where(match => courses.ContainsKey(match.SubjectCode.Trim()))
            .GroupBy(
                match => match.SubjectCode.Trim(),
                StringComparer.OrdinalIgnoreCase);

        return new ResumePromptPayloadDto
        {
            StudentInfo = new StudentInfoPromptDto
            {
                StudentId = academic.Student.StudentId,
                FullName = Clean(academic.Student.FullName, 200),
                StudentCode = Clean(academic.Student.UserName, 100),
                MajorName = Clean(academic.Student.MajorName, 255),
                Gpa = academic.Gpa.HasValue
                    ? Convert.ToDouble(academic.Gpa.Value)
                    : null
            },
            CareerFocusTag = NullIfBlank(CleanPromptText(request.CareerFocusTag, 200)),
            TargetRole = Clean(request.TargetRole, 200),
            JobDescription = CleanPromptText(request.JobDescription, 10_000),
            CurrentSummaryDraft = NullIfBlank(
                CleanPromptText(request.CurrentSummaryDraft, 3_000)),
            MatchedSubjects = vector is null
                ? HydrateScoreFallbackSubjects(courses.Values, selectedSet, request.TopK)
                : matchedBySubject
                    .Select(group => HydrateSubject(courses[group.Key], group))
                    .Where(subject => subject.CourseOutcomes.Count > 0)
                    .OrderByDescending(subject =>
                        subject.CourseOutcomes.Max(outcome => outcome.SimilarityScore))
                    .ThenBy(subject => subject.SubjectCode)
                    .ToList(),
            Projects = HydrateProjects(request.UiProjects, academic.Projects),
            Internships = HydrateInternships(
                request.SelectedInternshipIds,
                academic.ApprovedInternships),
            Certifications = HydrateTextList(request.Certifications, 20, 255),
            AwardsAndActivities = HydrateTextList(request.AwardsAndActivities, 20, 500),
            IsFallbackMode = vector?.IsFallback ?? true
        };
    }

    private static bool IsVectorMatchUnavailable(
        Exception exception,
        CancellationToken cancellationToken) =>
        exception is HttpRequestException or DownstreamApiException ||
        exception is TaskCanceledException && !cancellationToken.IsCancellationRequested;

    private static List<HydratedMatchedSubjectDto> HydrateScoreFallbackSubjects(
        IEnumerable<AcademicResumeCourseContract> courses,
        IReadOnlySet<Guid> selectedSubjectIds,
        int topK)
    {
        var fallbackCourses = courses
            .Where(course => selectedSubjectIds.Count == 0 ||
                             selectedSubjectIds.Contains(course.SubjectId))
            .OrderByDescending(course => course.Score)
            .ThenBy(course => course.SubjectCode);

        if (selectedSubjectIds.Count == 0)
            fallbackCourses = fallbackCourses.Take(Math.Clamp(topK, 1, 100))
                .OrderByDescending(course => course.Score)
                .ThenBy(course => course.SubjectCode);

        return fallbackCourses
            .Select(course => new HydratedMatchedSubjectDto
            {
                SubjectId = course.SubjectId,
                SubjectCode = Clean(course.SubjectCode, 100),
                SubjectName = Clean(course.SubjectName, 255),
                CreditPoint = Math.Max(course.CreditPoint, 0),
                Score = Convert.ToDouble(course.Score),
                CourseOutcomes = course.CourseOutcomes
                    .Select(outcome => new HydratedOutcomeDto
                    {
                        OutcomeCode = Clean(outcome.Name, 100),
                        Name = Clean(outcome.Name, 200),
                        Description = CleanPromptText(outcome.Description, 4_000),
                        SimilarityScore = 0,
                        ProgressionLevel = string.Empty
                    })
                    .Where(outcome => outcome.Name.Length > 0 ||
                                      outcome.Description.Length > 0)
                    .ToList()
            })
            .ToList();
    }

    private static List<HydratedProjectDto> HydrateProjects(
        IReadOnlyList<UiProjectOverrideDto>? uiProjects,
        IReadOnlyList<AcademicResumeProjectContract> academicProjects)
    {
        // A non-null list is the user's explicit selection. In particular, an
        // empty list means "include no projects" and must not expand back to
        // every project in AcademicService.
        if (uiProjects is not null)
        {
            var ownedProjectIds = academicProjects
                .Select(project => project.ProjectId)
                .ToHashSet();
            return uiProjects
                .Select((project, index) => new HydratedProjectDto
                {
                    ProjectId = project.ProjectId is > 0 &&
                                ownedProjectIds.Contains(project.ProjectId.Value)
                        ? project.ProjectId.Value
                        : -(index + 1),
                    ProjectName = CleanPromptText(project.ProjectName, 255),
                    TechStack = CleanPromptText(project.TechStack, 500),
                    ProjectDescription = string.Empty,
                    MyRole = CleanPromptText(project.MyRole, 200),
                    MyContributions =
                        CleanPromptText(project.MyContributions, 4_000),
                    TeamSize = Math.Clamp(project.TeamSize, 1, 100)
                })
                .ToList();
        }

        return academicProjects
            .Take(20)
            .Select(project => new HydratedProjectDto
            {
                ProjectId = project.ProjectId,
                ProjectName = Clean(project.ProjectName, 255),
                TechStack = Clean(project.TechStack, 500),
                ProjectDescription =
                    CleanPromptText(project.ProjectDescription, 2_000),
                MyRole = Clean(project.MyRole, 200),
                MyContributions =
                    CleanPromptText(project.MyContributions, 4_000),
                TeamSize = Math.Clamp(project.TeamSize, 1, 100),
                MappedCourseCode = NullIfBlank(
                    Clean(project.MappedCourseCode, 100))
            })
            .ToList();
    }

    private static List<HydratedInternshipDto> HydrateInternships(
        IReadOnlyList<int>? selectedInternshipIds,
        IReadOnlyList<AcademicResumeInternshipContract> approvedInternships)
    {
        if (selectedInternshipIds is not { Count: > 0 })
            return [];

        var approvedById = approvedInternships
            .GroupBy(internship => internship.InternshipId)
            .ToDictionary(group => group.Key, group => group.First());
        return selectedInternshipIds
            .Where(id => id > 0)
            .Distinct()
            .Where(approvedById.ContainsKey)
            .Select(id => approvedById[id])
            .Select(internship => new HydratedInternshipDto
            {
                InternshipId = internship.InternshipId,
                CompanyName = Clean(internship.CompanyName, 255),
                Position = Clean(internship.Position, 200),
                StartDate = internship.StartDate.ToDateTime(TimeOnly.MinValue),
                EndDate = internship.EndDate?.ToDateTime(TimeOnly.MinValue),
                TaskDescription =
                    CleanPromptText(internship.TaskDescription, 4_000)
            })
            .ToList();
    }

    private static List<string> HydrateTextList(
        IReadOnlyList<string>? values,
        int maximumItems,
        int maximumLength)
    {
        if (values is not { Count: > 0 })
            return [];

        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => CleanPromptText(value, maximumLength))
            .Where(value => value.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(maximumItems)
            .ToList();
    }

    private static void EnsureOwnership(
        Guid requestedStudentId,
        Guid? authenticatedUserId,
        VectorMatchResponseContract? vector,
        AcademicResumeContextContract academic)
    {
        if (vector is not null && vector.StudentId != requestedStudentId ||
            academic.Student.StudentId != requestedStudentId ||
            authenticatedUserId.HasValue &&
            academic.Student.UserId != authenticatedUserId.Value)
            throw new DownstreamApiException(
                "RESUME_CONTEXT_NOT_FOUND",
                "Resume context was not found.",
                StatusCodes.Status404NotFound);
    }

    private static HydratedMatchedSubjectDto HydrateSubject(
        AcademicResumeCourseContract course,
        IEnumerable<VectorMatchedOutcomeContract> matches)
    {
        var outcomes = matches
            .OrderByDescending(match => match.SimilarityScore)
            .GroupBy(
                match => $"{match.CloId}:{match.CriteriaId}:{match.CloCode}",
                StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Select(match => new HydratedOutcomeDto
            {
                OutcomeCode = Clean(match.CloCode, 100),
                Name = Clean(match.CloCode, 200),
                Description = Clean(match.Description, 4_000),
                SimilarityScore = Math.Clamp(match.SimilarityScore, 0f, 1f),
                ProgressionLevel = NormalizeProgression(match.ProgressionLevel)
            })
            .ToList();

        return new HydratedMatchedSubjectDto
        {
            SubjectId = course.SubjectId,
            SubjectCode = Clean(course.SubjectCode, 100),
            SubjectName = Clean(course.SubjectName, 255),
            CreditPoint = Math.Max(course.CreditPoint, 0),
            Score = Convert.ToDouble(course.Score),
            CourseOutcomes = outcomes
        };
    }

    private static string Clean(string? value, int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var decoded = WebUtility.HtmlDecode(value);
        var withoutActiveContent = ActiveContentPattern.Replace(decoded, " ");
        var withoutTags = HtmlTagPattern.Replace(withoutActiveContent, " ");
        var withoutControls = new string(
            withoutTags.Where(character =>
                    !char.IsControl(character) ||
                    character is '\r' or '\n' or '\t')
                .ToArray());
        var normalized = WhitespacePattern.Replace(withoutControls, " ").Trim();
        return normalized.Length <= maximumLength
            ? normalized
            : normalized[..maximumLength].TrimEnd();
    }

    private static string CleanPromptText(string? value, int maximumLength)
    {
        var cleaned = Clean(value, maximumLength);
        cleaned = EmailPattern.Replace(cleaned, " ");
        cleaned = UrlPattern.Replace(cleaned, " ");
        cleaned = PhonePattern.Replace(cleaned, " ");
        return Clean(cleaned, maximumLength);
    }

    private static string NormalizeProgression(string? value)
    {
        var normalized = value?.Trim().ToUpperInvariant();
        return normalized is "E" or "R" or "D" ? normalized : string.Empty;
    }

    private static string? NullIfBlank(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;
}
