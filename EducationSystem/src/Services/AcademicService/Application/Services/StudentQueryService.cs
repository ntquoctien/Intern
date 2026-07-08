using AcademicService.Application.DTOs.Students;
using AcademicService.Application.Interfaces;
using AcademicService.Infrastructure.Persistence;
using AcademicService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace AcademicService.Application.Services;

public sealed class StudentQueryService : IStudentQueryService
{
    private readonly AcademicDbContext _dbContext;

    public StudentQueryService(AcademicDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<StudentListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.Students.AsNoTracking();
        source = source.Where(x => !x.IsDeleted);
        if (filters?.UserId is { } userId)
        {
            source = source.Where(x => x.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => (x.Nickname != null && x.Nickname.Contains(search)) || (x.PlaceOfBirth != null && x.PlaceOfBirth.Contains(search)) || (x.Hometown != null && x.Hometown.Contains(search)) || (x.PermanentAddress != null && x.PermanentAddress.Contains(search)) || (x.ContactAddress != null && x.ContactAddress.Contains(search)) || (x.Ethnicity != null && x.Ethnicity.Contains(search)) || (x.Religion != null && x.Religion.Contains(search)) || (x.EducationLevel != null && x.EducationLevel.Contains(search)));
        }
        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new StudentListItemDto
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        AcademicYearId = x.AcademicYearId,
                        MajorId = x.MajorId,
                        RelativeUserId = x.RelativeUserId,
                        StudyStatus = x.StudyStatus,
                        Gender = x.Gender,
                        Nickname = x.Nickname,
                        PlaceOfBirth = x.PlaceOfBirth,
                        Hometown = x.Hometown,
                        PermanentAddress = x.PermanentAddress,
                        ContactAddress = x.ContactAddress,
                        Ethnicity = x.Ethnicity,
                        Religion = x.Religion,
                        EducationLevel = x.EducationLevel,
                        FatherName = x.FatherName,
                        FatherOccupation = x.FatherOccupation,
                        MotherName = x.MotherName,
                        MotherOccupation = x.MotherOccupation,
                        SpouseName = x.SpouseName,
                        SpouseOccupation = x.SpouseOccupation,
                        PolicySubject = x.PolicySubject,
                        PreviousOccupation = x.PreviousOccupation,
                        PostGraduationWorkplace = x.PostGraduationWorkplace,
                        CommunistPartyJoinDate = x.CommunistPartyJoinDate,
                        OfficialPartyJoinDate = x.OfficialPartyJoinDate,
                        YouthUnionJoinDate = x.YouthUnionJoinDate,
                        IsGraduated = x.IsGraduated,
                        HasIssue = x.HasIssue,
                        IssueDescription = x.IssueDescription,
                        LibraryId = x.LibraryId,
                        IsDeleted = x.IsDeleted
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<StudentListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<StudentDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Students
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new StudentDetailDto
                    {
                        Id = x.Id,
                        UserId = x.UserId,
                        AcademicYearId = x.AcademicYearId,
                        MajorId = x.MajorId,
                        RelativeUserId = x.RelativeUserId,
                        StudyStatus = x.StudyStatus,
                        Gender = x.Gender,
                        Nickname = x.Nickname,
                        PlaceOfBirth = x.PlaceOfBirth,
                        Hometown = x.Hometown,
                        PermanentAddress = x.PermanentAddress,
                        ContactAddress = x.ContactAddress,
                        Ethnicity = x.Ethnicity,
                        Religion = x.Religion,
                        EducationLevel = x.EducationLevel,
                        FatherName = x.FatherName,
                        FatherOccupation = x.FatherOccupation,
                        MotherName = x.MotherName,
                        MotherOccupation = x.MotherOccupation,
                        SpouseName = x.SpouseName,
                        SpouseOccupation = x.SpouseOccupation,
                        PolicySubject = x.PolicySubject,
                        PreviousOccupation = x.PreviousOccupation,
                        PostGraduationWorkplace = x.PostGraduationWorkplace,
                        CommunistPartyJoinDate = x.CommunistPartyJoinDate,
                        OfficialPartyJoinDate = x.OfficialPartyJoinDate,
                        YouthUnionJoinDate = x.YouthUnionJoinDate,
                        IsGraduated = x.IsGraduated,
                        HasIssue = x.HasIssue,
                        IssueDescription = x.IssueDescription,
                        LibraryId = x.LibraryId,
                        IsDeleted = x.IsDeleted
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<StudentLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        var source = _dbContext.Students.AsNoTracking();
        source = source.Where(x => !x.IsDeleted);
        return await source
            .OrderBy(x => x.Id)
            .Select(x => new StudentLookupDto
                    {
                        Id = x.Id,
                        Nickname = x.Nickname
                    })
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<Student> ApplySorting(IQueryable<Student> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "userid" => descending ? source.OrderByDescending(x => x.UserId) : source.OrderBy(x => x.UserId),
            "academicyearid" => descending ? source.OrderByDescending(x => x.AcademicYearId) : source.OrderBy(x => x.AcademicYearId),
            "majorid" => descending ? source.OrderByDescending(x => x.MajorId) : source.OrderBy(x => x.MajorId),
            "relativeuserid" => descending ? source.OrderByDescending(x => x.RelativeUserId) : source.OrderBy(x => x.RelativeUserId),
            "studystatus" => descending ? source.OrderByDescending(x => x.StudyStatus) : source.OrderBy(x => x.StudyStatus),
            "gender" => descending ? source.OrderByDescending(x => x.Gender) : source.OrderBy(x => x.Gender),
            "nickname" => descending ? source.OrderByDescending(x => x.Nickname) : source.OrderBy(x => x.Nickname),
            "placeofbirth" => descending ? source.OrderByDescending(x => x.PlaceOfBirth) : source.OrderBy(x => x.PlaceOfBirth),
            "hometown" => descending ? source.OrderByDescending(x => x.Hometown) : source.OrderBy(x => x.Hometown),
            "permanentaddress" => descending ? source.OrderByDescending(x => x.PermanentAddress) : source.OrderBy(x => x.PermanentAddress),
            "contactaddress" => descending ? source.OrderByDescending(x => x.ContactAddress) : source.OrderBy(x => x.ContactAddress),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
