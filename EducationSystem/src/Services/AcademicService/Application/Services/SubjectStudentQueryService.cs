using AcademicService.Application.DTOs.SubjectStudents;
using AcademicService.Application.Interfaces;
using AcademicService.Infrastructure.Persistence;
using AcademicService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace AcademicService.Application.Services;

public sealed class SubjectStudentQueryService : ISubjectStudentQueryService
{
    private readonly AcademicDbContext _dbContext;

    public SubjectStudentQueryService(AcademicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<SubjectStudentListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.SubjectStudents.AsNoTracking();

        if (filters?.StudentId is { } studentId)
        {
            source = source.Where(x => x.StudentId == studentId);
        }

        if (filters?.SubjectTeachingId is { } subjectTeachingId)
        {
            source = source.Where(x => x.SubjectTeachingId == subjectTeachingId);
        }

        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SubjectStudentListItemDto
                    {
                        Id = x.Id,
                        SubjectTeachingId = x.SubjectTeachingId,
                        StudentId = x.StudentId
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<SubjectStudentListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<SubjectStudentDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SubjectStudents
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new SubjectStudentDetailDto
                    {
                        Id = x.Id,
                        SubjectTeachingId = x.SubjectTeachingId,
                        StudentId = x.StudentId
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<SubjectStudent> ApplySorting(IQueryable<SubjectStudent> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "subjectteachingid" => descending ? source.OrderByDescending(x => x.SubjectTeachingId) : source.OrderBy(x => x.SubjectTeachingId),
            "studentid" => descending ? source.OrderByDescending(x => x.StudentId) : source.OrderBy(x => x.StudentId),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
