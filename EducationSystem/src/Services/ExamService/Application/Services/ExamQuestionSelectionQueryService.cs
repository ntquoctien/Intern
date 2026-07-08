using ExamService.Application.DTOs.ExamQuestionSelections;
using ExamService.Application.Interfaces;
using ExamService.Infrastructure.Persistence;
using ExamService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace ExamService.Application.Services;

public sealed class ExamQuestionSelectionQueryService : IExamQuestionSelectionQueryService
{
    private readonly ExamDbContext _dbContext;

    public ExamQuestionSelectionQueryService(ExamDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ExamQuestionSelectionListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.ExamQuestionSelections.AsNoTracking();

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
            source = source.Where(x => x.QuestionText.Contains(search) || x.ImageUrl.Contains(search));
        }
        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ExamQuestionSelectionListItemDto
                    {
                        Id = x.Id,
                        ExamAttemptId = x.ExamAttemptId,
                        AnswerId = x.AnswerId,
                        QuestionText = x.QuestionText,
                        ImageUrl = x.ImageUrl,
                        Order = x.Order,
                        IsFlag = x.IsFlag,
                        CreationDate = x.CreationDate,
                        SubmitDate = x.SubmitDate
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<ExamQuestionSelectionListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<ExamQuestionSelectionDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ExamQuestionSelections
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ExamQuestionSelectionDetailDto
                    {
                        Id = x.Id,
                        ExamAttemptId = x.ExamAttemptId,
                        AnswerId = x.AnswerId,
                        QuestionText = x.QuestionText,
                        ImageUrl = x.ImageUrl,
                        Order = x.Order,
                        IsFlag = x.IsFlag,
                        CreationDate = x.CreationDate,
                        SubmitDate = x.SubmitDate
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<ExamQuestionSelection> ApplySorting(IQueryable<ExamQuestionSelection> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "creationdate" => descending ? source.OrderByDescending(x => x.CreationDate) : source.OrderBy(x => x.CreationDate),
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "examattemptid" => descending ? source.OrderByDescending(x => x.ExamAttemptId) : source.OrderBy(x => x.ExamAttemptId),
            "answerid" => descending ? source.OrderByDescending(x => x.AnswerId) : source.OrderBy(x => x.AnswerId),
            "questiontext" => descending ? source.OrderByDescending(x => x.QuestionText) : source.OrderBy(x => x.QuestionText),
            "imageurl" => descending ? source.OrderByDescending(x => x.ImageUrl) : source.OrderBy(x => x.ImageUrl),
            "order" => descending ? source.OrderByDescending(x => x.Order) : source.OrderBy(x => x.Order),
            "isflag" => descending ? source.OrderByDescending(x => x.IsFlag) : source.OrderBy(x => x.IsFlag),
            "submitdate" => descending ? source.OrderByDescending(x => x.SubmitDate) : source.OrderBy(x => x.SubmitDate),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
