using ExamService.Application.DTOs.ExamQuestionAnswers;
using ExamService.Application.Interfaces;
using ExamService.Infrastructure.Persistence;
using ExamService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace ExamService.Application.Services;

public sealed class ExamQuestionAnswerQueryService : IExamQuestionAnswerQueryService
{
    private readonly ExamDbContext _dbContext;

    public ExamQuestionAnswerQueryService(ExamDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ExamQuestionAnswerListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.ExamQuestionAnswers.AsNoTracking();



        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.AnswerText.Contains(search));
        }
        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ExamQuestionAnswerListItemDto
                    {
                        Id = x.Id,
                        ExamQuestionSelectionId = x.ExamQuestionSelectionId,
                        AnswerText = x.AnswerText,
                        IsAnswer = x.IsAnswer
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<ExamQuestionAnswerListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<ExamQuestionAnswerDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ExamQuestionAnswers
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ExamQuestionAnswerDetailDto
                    {
                        Id = x.Id,
                        ExamQuestionSelectionId = x.ExamQuestionSelectionId,
                        AnswerText = x.AnswerText,
                        IsAnswer = x.IsAnswer
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<ExamQuestionAnswer> ApplySorting(IQueryable<ExamQuestionAnswer> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "examquestionselectionid" => descending ? source.OrderByDescending(x => x.ExamQuestionSelectionId) : source.OrderBy(x => x.ExamQuestionSelectionId),
            "answertext" => descending ? source.OrderByDescending(x => x.AnswerText) : source.OrderBy(x => x.AnswerText),
            "isanswer" => descending ? source.OrderByDescending(x => x.IsAnswer) : source.OrderBy(x => x.IsAnswer),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
