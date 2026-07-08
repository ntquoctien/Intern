using AcademicService.Application.DTOs.StudentEvaluations;
using AcademicService.Application.Interfaces;
using AcademicService.Infrastructure.Persistence;
using AcademicService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace AcademicService.Application.Services;

public sealed class StudentEvaluationQueryService : IStudentEvaluationQueryService
{
    private readonly AcademicDbContext _dbContext;

    public StudentEvaluationQueryService(AcademicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<StudentEvaluationListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.StudentEvaluations.AsNoTracking();
        source = source.Where(x => !x.IsDeleted);
        if (filters?.StudentId is { } studentId)
        {
            source = source.Where(x => x.StudentId == studentId);
        }

        if (filters?.SubjectTeachingId is { } subjectTeachingId)
        {
            source = source.Where(x => x.SubjectTeachingId == subjectTeachingId);
        }

        if (filters?.SubjectTeachingExamId is { } subjectTeachingExamId)
        {
            source = source.Where(x => x.SubjectTeachingExamId == subjectTeachingExamId);
        }

        if (filters?.FromDate is { } fromDate)
        {
            source = source.Where(x => x.CreationDate >= fromDate);
        }

        if (filters?.ToDate is { } toDate)
        {
            source = source.Where(x => x.CreationDate <= toDate);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => (x.TeacherName != null && x.TeacherName.Contains(search)) || (x.Comment != null && x.Comment.Contains(search)));
        }
        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new StudentEvaluationListItemDto
                    {
                        Id = x.Id,
                        StudentId = x.StudentId,
                        SubjectTeachingId = x.SubjectTeachingId,
                        SemesterPlanId = x.SemesterPlanId,
                        SubjectTeachingExamId = x.SubjectTeachingExamId,
                        QuestionId = x.QuestionId,
                        TeacherId = x.TeacherId,
                        TeacherName = x.TeacherName,
                        Type = x.Type,
                        Comment = x.Comment,
                        TotalScore = x.TotalScore,
                        CreationDate = x.CreationDate,
                        UpdatedDate = x.UpdatedDate,
                        IsDeleted = x.IsDeleted
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<StudentEvaluationListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<StudentEvaluationDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StudentEvaluations
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new StudentEvaluationDetailDto
                    {
                        Id = x.Id,
                        StudentId = x.StudentId,
                        SubjectTeachingId = x.SubjectTeachingId,
                        SemesterPlanId = x.SemesterPlanId,
                        SubjectTeachingExamId = x.SubjectTeachingExamId,
                        QuestionId = x.QuestionId,
                        TeacherId = x.TeacherId,
                        TeacherName = x.TeacherName,
                        Type = x.Type,
                        Comment = x.Comment,
                        TotalScore = x.TotalScore,
                        CreationDate = x.CreationDate,
                        UpdatedDate = x.UpdatedDate,
                        IsDeleted = x.IsDeleted
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<StudentEvaluation> ApplySorting(IQueryable<StudentEvaluation> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "creationdate" => descending ? source.OrderByDescending(x => x.CreationDate) : source.OrderBy(x => x.CreationDate),
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "studentid" => descending ? source.OrderByDescending(x => x.StudentId) : source.OrderBy(x => x.StudentId),
            "subjectteachingid" => descending ? source.OrderByDescending(x => x.SubjectTeachingId) : source.OrderBy(x => x.SubjectTeachingId),
            "semesterplanid" => descending ? source.OrderByDescending(x => x.SemesterPlanId) : source.OrderBy(x => x.SemesterPlanId),
            "subjectteachingexamid" => descending ? source.OrderByDescending(x => x.SubjectTeachingExamId) : source.OrderBy(x => x.SubjectTeachingExamId),
            "questionid" => descending ? source.OrderByDescending(x => x.QuestionId) : source.OrderBy(x => x.QuestionId),
            "teacherid" => descending ? source.OrderByDescending(x => x.TeacherId) : source.OrderBy(x => x.TeacherId),
            "teachername" => descending ? source.OrderByDescending(x => x.TeacherName) : source.OrderBy(x => x.TeacherName),
            "type" => descending ? source.OrderByDescending(x => x.Type) : source.OrderBy(x => x.Type),
            "comment" => descending ? source.OrderByDescending(x => x.Comment) : source.OrderBy(x => x.Comment),
            "totalscore" => descending ? source.OrderByDescending(x => x.TotalScore) : source.OrderBy(x => x.TotalScore),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
