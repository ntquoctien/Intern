using CommunicationService.Application.DTOs.FormRequests;
using CommunicationService.Application.Interfaces;
using CommunicationService.Infrastructure.Persistence;
using CommunicationService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace CommunicationService.Application.Services;

public sealed class FormRequestQueryService : IFormRequestQueryService
{
    private readonly CommunicationDbContext _dbContext;

    public FormRequestQueryService(CommunicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<FormRequestListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.FormRequests.AsNoTracking();
        source = source.Where(x => !x.IsDeleted);
        if (filters?.StudentId is { } studentId)
        {
            source = source.Where(x => x.StudentId == studentId);
        }

        if (filters?.Status is { } status)
        {
            source = source.Where(x => x.Status == status);
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
            source = source.Where(x => x.ApprovalName.Contains(search) || x.Note.Contains(search));
        }
        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new FormRequestListItemDto
                    {
                        Id = x.Id,
                        CreationDate = x.CreationDate,
                        UpdateDate = x.UpdateDate,
                        StudentId = x.StudentId,
                        FormTemplateId = x.FormTemplateId,
                        ApprovalId = x.ApprovalId,
                        ApprovalName = x.ApprovalName,
                        Note = x.Note,
                        Status = x.Status,
                        IsDeleted = x.IsDeleted
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<FormRequestListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<FormRequestDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FormRequests
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new FormRequestDetailDto
                    {
                        Id = x.Id,
                        CreationDate = x.CreationDate,
                        UpdateDate = x.UpdateDate,
                        StudentId = x.StudentId,
                        FormTemplateId = x.FormTemplateId,
                        ApprovalId = x.ApprovalId,
                        ApprovalName = x.ApprovalName,
                        Note = x.Note,
                        Status = x.Status,
                        IsDeleted = x.IsDeleted
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<FormRequest> ApplySorting(IQueryable<FormRequest> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "creationdate" => descending ? source.OrderByDescending(x => x.CreationDate) : source.OrderBy(x => x.CreationDate),
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "updatedate" => descending ? source.OrderByDescending(x => x.UpdateDate) : source.OrderBy(x => x.UpdateDate),
            "studentid" => descending ? source.OrderByDescending(x => x.StudentId) : source.OrderBy(x => x.StudentId),
            "formtemplateid" => descending ? source.OrderByDescending(x => x.FormTemplateId) : source.OrderBy(x => x.FormTemplateId),
            "approvalid" => descending ? source.OrderByDescending(x => x.ApprovalId) : source.OrderBy(x => x.ApprovalId),
            "approvalname" => descending ? source.OrderByDescending(x => x.ApprovalName) : source.OrderBy(x => x.ApprovalName),
            "note" => descending ? source.OrderByDescending(x => x.Note) : source.OrderBy(x => x.Note),
            "status" => descending ? source.OrderByDescending(x => x.Status) : source.OrderBy(x => x.Status),
            "isdeleted" => descending ? source.OrderByDescending(x => x.IsDeleted) : source.OrderBy(x => x.IsDeleted),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
