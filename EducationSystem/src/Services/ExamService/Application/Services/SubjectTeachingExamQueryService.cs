using ExamService.Application.DTOs.SubjectTeachingExams;
using ExamService.Application.Interfaces;
using ExamService.Infrastructure.Persistence;
using ExamService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace ExamService.Application.Services;

public sealed class SubjectTeachingExamQueryService : ISubjectTeachingExamQueryService
{
    private readonly ExamDbContext _dbContext;

    public SubjectTeachingExamQueryService(ExamDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<SubjectTeachingExamListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.SubjectTeachingExams.AsNoTracking();
        source = source.Where(x => !x.IsDeleted);
        if (filters?.SubjectTeachingId is { } subjectTeachingId)
        {
            source = source.Where(x => x.SubjectTeachingId == subjectTeachingId);
        }

        if (filters?.FromDate is { } fromDate)
        {
            source = source.Where(x => x.StartDate >= fromDate);
        }

        if (filters?.ToDate is { } toDate)
        {
            source = source.Where(x => x.StartDate <= toDate);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.Name.Contains(search) || x.Notes.Contains(search));
        }
        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SubjectTeachingExamListItemDto
                    {
                        Id = x.Id,
                        SubjectTeachingId = x.SubjectTeachingId,
                        QuestionSuiteId = x.QuestionSuiteId,
                        Name = x.Name,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        RoomId = x.RoomId,
                        TeacherId = x.TeacherId,
                        Notes = x.Notes,
                        Type = x.Type,
                        Count = x.Count,
                        NumOfEasy = x.NumOfEasy,
                        NumOfNormal = x.NumOfNormal,
                        NumOfHard = x.NumOfHard,
                        NumOfPractice = x.NumOfPractice,
                        Method = x.Method,
                        AllowNotifyStudent = x.AllowNotifyStudent,
                        IsDeleted = x.IsDeleted
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<SubjectTeachingExamListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<SubjectTeachingExamDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.SubjectTeachingExams
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new SubjectTeachingExamDetailDto
                    {
                        Id = x.Id,
                        SubjectTeachingId = x.SubjectTeachingId,
                        QuestionSuiteId = x.QuestionSuiteId,
                        Name = x.Name,
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        RoomId = x.RoomId,
                        TeacherId = x.TeacherId,
                        Notes = x.Notes,
                        Type = x.Type,
                        Count = x.Count,
                        NumOfEasy = x.NumOfEasy,
                        NumOfNormal = x.NumOfNormal,
                        NumOfHard = x.NumOfHard,
                        NumOfPractice = x.NumOfPractice,
                        Method = x.Method,
                        AllowNotifyStudent = x.AllowNotifyStudent,
                        IsDeleted = x.IsDeleted
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<SubjectTeachingExamLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        var source = _dbContext.SubjectTeachingExams.AsNoTracking();
        source = source.Where(x => !x.IsDeleted);
        return await source
            .OrderBy(x => x.Id)
            .Select(x => new SubjectTeachingExamLookupDto
                    {
                        Id = x.Id,
                        Name = x.Name
                    })
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<SubjectTeachingExam> ApplySorting(IQueryable<SubjectTeachingExam> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "name" => descending ? source.OrderByDescending(x => x.Name) : source.OrderBy(x => x.Name),
            "startdate" => descending ? source.OrderByDescending(x => x.StartDate) : source.OrderBy(x => x.StartDate),
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "subjectteachingid" => descending ? source.OrderByDescending(x => x.SubjectTeachingId) : source.OrderBy(x => x.SubjectTeachingId),
            "questionsuiteid" => descending ? source.OrderByDescending(x => x.QuestionSuiteId) : source.OrderBy(x => x.QuestionSuiteId),
            "enddate" => descending ? source.OrderByDescending(x => x.EndDate) : source.OrderBy(x => x.EndDate),
            "roomid" => descending ? source.OrderByDescending(x => x.RoomId) : source.OrderBy(x => x.RoomId),
            "teacherid" => descending ? source.OrderByDescending(x => x.TeacherId) : source.OrderBy(x => x.TeacherId),
            "notes" => descending ? source.OrderByDescending(x => x.Notes) : source.OrderBy(x => x.Notes),
            "type" => descending ? source.OrderByDescending(x => x.Type) : source.OrderBy(x => x.Type),
            "count" => descending ? source.OrderByDescending(x => x.Count) : source.OrderBy(x => x.Count),
            "numofeasy" => descending ? source.OrderByDescending(x => x.NumOfEasy) : source.OrderBy(x => x.NumOfEasy),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
