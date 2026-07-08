using AcademicService.Application.DTOs.Subjects;
using AcademicService.Application.Interfaces;
using AcademicService.Infrastructure.Persistence;
using AcademicService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace AcademicService.Application.Services;

public sealed class SubjectQueryService : ISubjectQueryService
{
    private readonly AcademicDbContext _dbContext;

    public SubjectQueryService(AcademicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<SubjectListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.Subjects.AsNoTracking();
        source = source.Where(x => !x.IsDeleted);


        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.SubjectCode.Contains(search) || x.Name.Contains(search) || x.Note.Contains(search));
        }
        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SubjectListItemDto
                    {
                        Id = x.Id,
                        FacultyId = x.FacultyId,
                        SubjectCode = x.SubjectCode,
                        Name = x.Name,
                        CreditPoint = x.CreditPoint,
                        TotalHours = x.TotalHours,
                        Note = x.Note,
                        IsActived = x.IsActived,
                        IsDeleted = x.IsDeleted
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<SubjectListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<SubjectDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Subjects
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new SubjectDetailDto
                    {
                        Id = x.Id,
                        FacultyId = x.FacultyId,
                        SubjectCode = x.SubjectCode,
                        Name = x.Name,
                        CreditPoint = x.CreditPoint,
                        TotalHours = x.TotalHours,
                        Note = x.Note,
                        IsActived = x.IsActived,
                        IsDeleted = x.IsDeleted
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<SubjectLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        var source = _dbContext.Subjects.AsNoTracking();
        source = source.Where(x => !x.IsDeleted);
        return await source
            .OrderBy(x => x.Id)
            .Select(x => new SubjectLookupDto
                    {
                        Id = x.Id,
                        SubjectCode = x.SubjectCode,
                        Name = x.Name
                    })
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<Subject> ApplySorting(IQueryable<Subject> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "name" => descending ? source.OrderByDescending(x => x.Name) : source.OrderBy(x => x.Name),
            "subjectcode" => descending ? source.OrderByDescending(x => x.SubjectCode) : source.OrderBy(x => x.SubjectCode),
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "facultyid" => descending ? source.OrderByDescending(x => x.FacultyId) : source.OrderBy(x => x.FacultyId),
            "creditpoint" => descending ? source.OrderByDescending(x => x.CreditPoint) : source.OrderBy(x => x.CreditPoint),
            "totalhours" => descending ? source.OrderByDescending(x => x.TotalHours) : source.OrderBy(x => x.TotalHours),
            "note" => descending ? source.OrderByDescending(x => x.Note) : source.OrderBy(x => x.Note),
            "isactived" => descending ? source.OrderByDescending(x => x.IsActived) : source.OrderBy(x => x.IsActived),
            "isdeleted" => descending ? source.OrderByDescending(x => x.IsDeleted) : source.OrderBy(x => x.IsDeleted),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
