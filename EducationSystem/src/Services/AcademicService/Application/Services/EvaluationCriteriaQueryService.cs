using AcademicService.Application.DTOs.EvaluationCriterias;
using AcademicService.Application.Interfaces;
using AcademicService.Infrastructure.Persistence;
using AcademicService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace AcademicService.Application.Services;

public sealed class EvaluationCriteriaQueryService : IEvaluationCriteriaQueryService
{
    private readonly AcademicDbContext _dbContext;

    public EvaluationCriteriaQueryService(AcademicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<EvaluationCriteriaListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.EvaluationCriterias.AsNoTracking();



        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.Name.Contains(search) || x.Description.Contains(search));
        }
        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new EvaluationCriteriaListItemDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        Type = x.Type,
                        Score = x.Score,
                        ParentId = x.ParentId,
                        QuestionId = x.QuestionId
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<EvaluationCriteriaListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<EvaluationCriteriaDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.EvaluationCriterias
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new EvaluationCriteriaDetailDto
                    {
                        Id = x.Id,
                        Name = x.Name,
                        Description = x.Description,
                        Type = x.Type,
                        Score = x.Score,
                        ParentId = x.ParentId,
                        QuestionId = x.QuestionId
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<EvaluationCriteriaLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        var source = _dbContext.EvaluationCriterias.AsNoTracking();

        return await source
            .OrderBy(x => x.Id)
            .Select(x => new EvaluationCriteriaLookupDto
                    {
                        Id = x.Id,
                        Name = x.Name
                    })
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<EvaluationCriteria> ApplySorting(IQueryable<EvaluationCriteria> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "name" => descending ? source.OrderByDescending(x => x.Name) : source.OrderBy(x => x.Name),
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "description" => descending ? source.OrderByDescending(x => x.Description) : source.OrderBy(x => x.Description),
            "type" => descending ? source.OrderByDescending(x => x.Type) : source.OrderBy(x => x.Type),
            "score" => descending ? source.OrderByDescending(x => x.Score) : source.OrderBy(x => x.Score),
            "parentid" => descending ? source.OrderByDescending(x => x.ParentId) : source.OrderBy(x => x.ParentId),
            "questionid" => descending ? source.OrderByDescending(x => x.QuestionId) : source.OrderBy(x => x.QuestionId),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
