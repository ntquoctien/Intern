using ExamService.Application.DTOs.Questions;
using ExamService.Application.Interfaces;
using ExamService.Infrastructure.Persistence;
using ExamService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace ExamService.Application.Services;

public sealed class QuestionQueryService : IQuestionQueryService
{
    private readonly ExamDbContext _dbContext;

    public QuestionQueryService(ExamDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<QuestionListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.Questions.AsNoTracking();



        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.QuestionText.Contains(search) || (x.ImageUrl != null && x.ImageUrl.Contains(search)));
        }
        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new QuestionListItemDto
                    {
                        Id = x.Id,
                        QuestionSuiteId = x.QuestionSuiteId,
                        QuestionText = x.QuestionText,
                        Level = x.Level,
                        ImageUrl = x.ImageUrl
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<QuestionListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<QuestionDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Questions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new QuestionDetailDto
                    {
                        Id = x.Id,
                        QuestionSuiteId = x.QuestionSuiteId,
                        QuestionText = x.QuestionText,
                        Level = x.Level,
                        ImageUrl = x.ImageUrl
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<QuestionLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        var source = _dbContext.Questions.AsNoTracking();

        return await source
            .OrderBy(x => x.Id)
            .Select(x => new QuestionLookupDto
                    {
                        Id = x.Id
                    })
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<Question> ApplySorting(IQueryable<Question> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "questionsuiteid" => descending ? source.OrderByDescending(x => x.QuestionSuiteId) : source.OrderBy(x => x.QuestionSuiteId),
            "questiontext" => descending ? source.OrderByDescending(x => x.QuestionText) : source.OrderBy(x => x.QuestionText),
            "level" => descending ? source.OrderByDescending(x => x.Level) : source.OrderBy(x => x.Level),
            "imageurl" => descending ? source.OrderByDescending(x => x.ImageUrl) : source.OrderBy(x => x.ImageUrl),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
