using ExamService.Application.DTOs.QuestionSuites;
using ExamService.Application.Interfaces;
using ExamService.Infrastructure.Persistence;
using ExamService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace ExamService.Application.Services;

public sealed class QuestionSuiteQueryService : IQuestionSuiteQueryService
{
    private readonly ExamDbContext _dbContext;

    public QuestionSuiteQueryService(ExamDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<QuestionSuiteListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.QuestionSuites.AsNoTracking();



        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.Name.Contains(search));
        }
        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new QuestionSuiteListItemDto
                    {
                        Id = x.Id,
                        SubjectId = x.SubjectId,
                        Name = x.Name,
                        UpdatedById = x.UpdatedById,
                        CreationTime = x.CreationTime
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<QuestionSuiteListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<QuestionSuiteDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.QuestionSuites
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new QuestionSuiteDetailDto
                    {
                        Id = x.Id,
                        SubjectId = x.SubjectId,
                        Name = x.Name,
                        UpdatedById = x.UpdatedById,
                        CreationTime = x.CreationTime
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<QuestionSuiteLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        var source = _dbContext.QuestionSuites.AsNoTracking();

        return await source
            .OrderBy(x => x.Id)
            .Select(x => new QuestionSuiteLookupDto
                    {
                        Id = x.Id,
                        Name = x.Name
                    })
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<QuestionSuite> ApplySorting(IQueryable<QuestionSuite> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "name" => descending ? source.OrderByDescending(x => x.Name) : source.OrderBy(x => x.Name),
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "subjectid" => descending ? source.OrderByDescending(x => x.SubjectId) : source.OrderBy(x => x.SubjectId),
            "updatedbyid" => descending ? source.OrderByDescending(x => x.UpdatedById) : source.OrderBy(x => x.UpdatedById),
            "creationtime" => descending ? source.OrderByDescending(x => x.CreationTime) : source.OrderBy(x => x.CreationTime),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
