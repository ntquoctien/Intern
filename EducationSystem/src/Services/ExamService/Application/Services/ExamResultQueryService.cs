using ExamService.Application.DTOs.ExamResults;
using ExamService.Application.Interfaces;
using ExamService.Infrastructure.Persistence;
using ExamService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace ExamService.Application.Services;

public sealed class ExamResultQueryService : IExamResultQueryService
{
    private readonly ExamDbContext _dbContext;

    public ExamResultQueryService(ExamDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ExamResultListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.ExamResults.AsNoTracking();
        source = source.Where(x => !x.IsDeleted);
        if (filters?.StudentId is { } studentId)
        {
            source = source.Where(x => x.StudentId == studentId);
        }

        if (filters?.SubjectTeachingExamId is { } subjectTeachingExamId)
        {
            source = source.Where(x => x.SubjectTeachingExamId == subjectTeachingExamId);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => (x.Notes != null && x.Notes.Contains(search)) || (x.ExamResultDesc != null && x.ExamResultDesc.Contains(search)) || (x.ExamResultDetail != null && x.ExamResultDetail.Contains(search)));
        }
        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ExamResultListItemDto
                    {
                        Id = x.Id,
                        SubjectTeachingExamId = x.SubjectTeachingExamId,
                        StudentId = x.StudentId,
                        ExamAttemptId = x.ExamAttemptId,
                        Result = x.Result,
                        CombinedResult = x.CombinedResult,
                        Notes = x.Notes,
                        ExamResultDesc = x.ExamResultDesc,
                        ExamResultDetail = x.ExamResultDetail,
                        IsDeleted = x.IsDeleted
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<ExamResultListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<ExamResultDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ExamResults
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new ExamResultDetailDto
                    {
                        Id = x.Id,
                        SubjectTeachingExamId = x.SubjectTeachingExamId,
                        StudentId = x.StudentId,
                        ExamAttemptId = x.ExamAttemptId,
                        Result = x.Result,
                        CombinedResult = x.CombinedResult,
                        Notes = x.Notes,
                        ExamResultDesc = x.ExamResultDesc,
                        ExamResultDetail = x.ExamResultDetail,
                        IsDeleted = x.IsDeleted
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<ExamResult> ApplySorting(IQueryable<ExamResult> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "subjectteachingexamid" => descending ? source.OrderByDescending(x => x.SubjectTeachingExamId) : source.OrderBy(x => x.SubjectTeachingExamId),
            "studentid" => descending ? source.OrderByDescending(x => x.StudentId) : source.OrderBy(x => x.StudentId),
            "examattemptid" => descending ? source.OrderByDescending(x => x.ExamAttemptId) : source.OrderBy(x => x.ExamAttemptId),
            "result" => descending ? source.OrderByDescending(x => x.Result) : source.OrderBy(x => x.Result),
            "combinedresult" => descending ? source.OrderByDescending(x => x.CombinedResult) : source.OrderBy(x => x.CombinedResult),
            "notes" => descending ? source.OrderByDescending(x => x.Notes) : source.OrderBy(x => x.Notes),
            "examresultdesc" => descending ? source.OrderByDescending(x => x.ExamResultDesc) : source.OrderBy(x => x.ExamResultDesc),
            "examresultdetail" => descending ? source.OrderByDescending(x => x.ExamResultDetail) : source.OrderBy(x => x.ExamResultDetail),
            "isdeleted" => descending ? source.OrderByDescending(x => x.IsDeleted) : source.OrderBy(x => x.IsDeleted),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
