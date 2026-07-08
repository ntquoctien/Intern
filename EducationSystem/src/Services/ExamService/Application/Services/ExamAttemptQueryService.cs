using ExamService.Application.DTOs.ExamAttempts;
using ExamService.Application.Interfaces;
using ExamService.Infrastructure.Persistence;
using ExamService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace ExamService.Application.Services;

public sealed class ExamAttemptQueryService : IExamAttemptQueryService
{
    private readonly ExamDbContext _dbContext;

    public ExamAttemptQueryService(ExamDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ExamAttemptListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.ExamAttempts.AsNoTracking();
        source = source.Where(x => !x.IsDeleted);
        if (filters?.StudentId is { } studentId)
        {
            source = source.Where(x => x.StudentId == studentId);
        }

        if (filters?.SubjectTeachingExamId is { } subjectTeachingExamId)
        {
            source = source.Where(x => x.SubjectTeachingExamId == subjectTeachingExamId);
        }

        if (filters?.FromDate is { } fromDate)
        {
            source = source.Where(x => x.SubmitDate >= fromDate);
        }

        if (filters?.ToDate is { } toDate)
        {
            source = source.Where(x => x.SubmitDate <= toDate);
        }

        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ExamAttemptListItemDto
                    {
                        Id = x.Id,
                        StudentId = x.StudentId,
                        SubjectTeachingExamId = x.SubjectTeachingExamId,
                        DraftDate = x.DraftDate,
                        SubmitDate = x.SubmitDate,
                        IsDeleted = x.IsDeleted
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<ExamAttemptListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<ExamAttemptDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ExamAttempts
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new ExamAttemptDetailDto
                    {
                        Id = x.Id,
                        StudentId = x.StudentId,
                        SubjectTeachingExamId = x.SubjectTeachingExamId,
                        DraftDate = x.DraftDate,
                        SubmitDate = x.SubmitDate,
                        IsDeleted = x.IsDeleted
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<ExamAttempt> ApplySorting(IQueryable<ExamAttempt> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "studentid" => descending ? source.OrderByDescending(x => x.StudentId) : source.OrderBy(x => x.StudentId),
            "subjectteachingexamid" => descending ? source.OrderByDescending(x => x.SubjectTeachingExamId) : source.OrderBy(x => x.SubjectTeachingExamId),
            "draftdate" => descending ? source.OrderByDescending(x => x.DraftDate) : source.OrderBy(x => x.DraftDate),
            "submitdate" => descending ? source.OrderByDescending(x => x.SubmitDate) : source.OrderBy(x => x.SubmitDate),
            "isdeleted" => descending ? source.OrderByDescending(x => x.IsDeleted) : source.OrderBy(x => x.IsDeleted),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
