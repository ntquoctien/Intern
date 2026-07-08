using ExamService.Application.DTOs.QuestionAnswers;
using ExamService.Application.Interfaces;
using ExamService.Infrastructure.Persistence;
using ExamService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace ExamService.Application.Services;

public sealed class QuestionAnswerQueryService : IQuestionAnswerQueryService
{
    private readonly ExamDbContext _dbContext;

    public QuestionAnswerQueryService(ExamDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<QuestionAnswerListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.QuestionAnswers.AsNoTracking();



        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.AnswerText.Contains(search) || (x.ImageUrl != null && x.ImageUrl.Contains(search)));
        }
        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new QuestionAnswerListItemDto
                    {
                        Id = x.Id,
                        QuestionId = x.QuestionId,
                        AnswerText = x.AnswerText,
                        ImageUrl = x.ImageUrl,
                        IsAnswer = x.IsAnswer
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<QuestionAnswerListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<QuestionAnswerDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.QuestionAnswers
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new QuestionAnswerDetailDto
                    {
                        Id = x.Id,
                        QuestionId = x.QuestionId,
                        AnswerText = x.AnswerText,
                        ImageUrl = x.ImageUrl,
                        IsAnswer = x.IsAnswer
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<QuestionAnswer> ApplySorting(IQueryable<QuestionAnswer> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "questionid" => descending ? source.OrderByDescending(x => x.QuestionId) : source.OrderBy(x => x.QuestionId),
            "answertext" => descending ? source.OrderByDescending(x => x.AnswerText) : source.OrderBy(x => x.AnswerText),
            "imageurl" => descending ? source.OrderByDescending(x => x.ImageUrl) : source.OrderBy(x => x.ImageUrl),
            "isanswer" => descending ? source.OrderByDescending(x => x.IsAnswer) : source.OrderBy(x => x.IsAnswer),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
