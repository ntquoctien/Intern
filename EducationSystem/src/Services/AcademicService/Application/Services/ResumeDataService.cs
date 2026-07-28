using AcademicService.Application.DTOs.Resume;
using AcademicService.Application.Interfaces;
using AcademicService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AcademicService.Application.Services;

public sealed class ResumeDataService(
    AcademicDbContext dbContext,
    IIdentityUserClient identityUserClient) : IResumeDataService
{
    public async Task<ResumeContextDto?> GetContextDataAsync(
        Guid studentId,
        CancellationToken cancellationToken)
    {
        var student = await dbContext.Students.AsNoTracking()
            .Where(item => item.Id == studentId && !item.IsDeleted)
            .Select(item => new
            {
                item.Id,
                item.UserId,
                MajorCode = item.Major.Code,
                MajorName = item.Major.Name,
                FacultyName = item.Major.Faculty != null ? item.Major.Faculty.Name : null,
                AcademicYear = item.AcademicYear.Name
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (student is null)
        {
            return null;
        }

        var identity = await identityUserClient.GetAsync(student.UserId, cancellationToken);
        if (identity is null)
        {
            return null;
        }

        var evaluationRows = await dbContext.StudentEvaluations.AsNoTracking()
            .Where(evaluation =>
                evaluation.StudentId == studentId &&
                !evaluation.IsDeleted &&
                evaluation.TotalScore.HasValue &&
                evaluation.TotalScore >= 0 &&
                evaluation.TotalScore <= 10 &&
                evaluation.SubjectTeaching != null &&
                !evaluation.SubjectTeaching.IsDeleted &&
                !evaluation.SubjectTeaching.Subject.IsDeleted)
            .Select(evaluation => new
            {
                SubjectId = evaluation.SubjectTeaching!.SubjectId,
                evaluation.SubjectTeaching.Subject.SubjectCode,
                SubjectName = evaluation.SubjectTeaching.Subject.Name,
                evaluation.SubjectTeaching.Subject.CreditPoint,
                Score = evaluation.TotalScore!.Value,
                Outcomes = evaluation.StudentEvaluationDetails
                    .Where(detail => !detail.IsDeleted)
                    .Select(detail => new
                    {
                        Name = detail.EvaluationName ??
                            (detail.EvaluationCriteria != null
                                ? detail.EvaluationCriteria.Name
                                : null),
                        Description = detail.EvaluationCriteria != null
                            ? detail.EvaluationCriteria.Description
                            : null
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        var groupedEvaluations = evaluationRows
            .GroupBy(row => new
            {
                row.SubjectId,
                row.SubjectCode,
                row.SubjectName,
                row.CreditPoint
            })
            .Select(group => new
            {
                group.Key,
                Score = decimal.Round(group.Average(row => row.Score), 2),
                Outcomes = group
                    .SelectMany(row => row.Outcomes)
                    .Where(outcome => !string.IsNullOrWhiteSpace(outcome.Name))
                    .GroupBy(outcome => outcome.Name!, StringComparer.OrdinalIgnoreCase)
                    .Select(outcome => new ResumeCourseOutcomeDto(
                        outcome.Key,
                        outcome.Select(item => item.Description).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))))
                    .OrderBy(outcome => outcome.Name)
                    .ToList()
            })
            .ToList();

        var totalCredits = groupedEvaluations.Sum(item => item.Key.CreditPoint);
        var gpa = groupedEvaluations.Count == 0
            ? (decimal?)null
            : decimal.Round(
                totalCredits > 0
                    ? groupedEvaluations.Sum(item => item.Score * item.Key.CreditPoint) / totalCredits
                    : groupedEvaluations.Average(item => item.Score),
                2);

        var eligibleCourses = groupedEvaluations
            .Where(item => item.Score >= 7.0m)
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Key.SubjectCode)
            .Select(item => new ResumeCourseDto(
                item.Key.SubjectId,
                item.Key.SubjectCode,
                item.Key.SubjectName,
                item.Key.CreditPoint,
                item.Score,
                item.Outcomes))
            .ToList();

        var projects = await (
            from project in dbContext.StudentProjects.AsNoTracking()
            join subject in dbContext.Subjects.AsNoTracking()
                on project.MappedCourseId equals subject.Id into mappedSubjects
            from subject in mappedSubjects.DefaultIfEmpty()
            where project.StudentId == studentId
            orderby project.ProjectId descending
            select new ResumeProjectDto(
                project.ProjectId,
                project.ProjectName,
                project.TechStack,
                project.ProjectDescription,
                project.SourceCodeUrl,
                project.TeamSize,
                project.MyRole,
                project.MyContributions,
                project.MappedCourseId,
                subject != null ? subject.SubjectCode : null,
                subject != null ? subject.Name : null))
            .ToListAsync(cancellationToken);

        var internships = await dbContext.StudentInternships.AsNoTracking()
            .Where(item => item.StudentId == studentId)
            .OrderByDescending(item => item.StartDate)
            .Select(item => new ResumeInternshipDto(
                item.InternshipId,
                item.CompanyName,
                item.Position,
                item.StartDate,
                item.EndDate,
                item.TaskDescription))
            .ToListAsync(cancellationToken);

        return new ResumeContextDto(
            new ResumeStudentDto(
                student.Id,
                student.UserId,
                identity.FullName,
                identity.UserName,
                student.MajorCode,
                student.MajorName,
                student.FacultyName,
                student.AcademicYear),
            gpa,
            eligibleCourses,
            projects,
            internships);
    }
}
