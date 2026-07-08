using AcademicService.Application.DTOs.Majors;
using AcademicService.Application.Interfaces;
using AcademicService.Infrastructure.Persistence;
using AcademicService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace AcademicService.Application.Services;

public sealed class MajorQueryService : IMajorQueryService
{
    private readonly AcademicDbContext _dbContext;

    public MajorQueryService(AcademicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<MajorListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.Majors.AsNoTracking();
        source = source.Where(x => !x.IsDeleted);


        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.Name.Contains(search) || x.Code.Contains(search));
        }
        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new MajorListItemDto
                    {
                        Id = x.Id,
                        FacultyId = x.FacultyId,
                        Name = x.Name,
                        Code = x.Code,
                        TrainingType = x.TrainingType,
                        IsDeleted = x.IsDeleted
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<MajorListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<MajorDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Majors
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new MajorDetailDto
                    {
                        Id = x.Id,
                        FacultyId = x.FacultyId,
                        Name = x.Name,
                        Code = x.Code,
                        TrainingType = x.TrainingType,
                        IsDeleted = x.IsDeleted
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<MajorLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        var source = _dbContext.Majors.AsNoTracking();
        source = source.Where(x => !x.IsDeleted);
        return await source
            .OrderBy(x => x.Id)
            .Select(x => new MajorLookupDto
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Name = x.Name
                    })
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<Major> ApplySorting(IQueryable<Major> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "name" => descending ? source.OrderByDescending(x => x.Name) : source.OrderBy(x => x.Name),
            "code" => descending ? source.OrderByDescending(x => x.Code) : source.OrderBy(x => x.Code),
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "facultyid" => descending ? source.OrderByDescending(x => x.FacultyId) : source.OrderBy(x => x.FacultyId),
            "trainingtype" => descending ? source.OrderByDescending(x => x.TrainingType) : source.OrderBy(x => x.TrainingType),
            "isdeleted" => descending ? source.OrderByDescending(x => x.IsDeleted) : source.OrderBy(x => x.IsDeleted),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
