using IdentityService.Application.DTOs.Users;
using IdentityService.Application.Interfaces;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace IdentityService.Application.Services;

public sealed class UserQueryService : IUserQueryService
{
    private readonly IdentityDbContext _dbContext;

    public UserQueryService(IdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<UserListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default)
    {
        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;
        var source = _dbContext.Users.AsNoTracking();
        source = source.Where(x => !x.IsDeleted);


        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            source = source.Where(x => x.UserName.Contains(search) || x.FullName.Contains(search) || (x.IdentificationNumber != null && x.IdentificationNumber.Contains(search)) || x.UserInternalId.Contains(search) || (x.Mobile != null && x.Mobile.Contains(search)) || (x.ProfilePicUrl != null && x.ProfilePicUrl.Contains(search)));
        }
        var totalItems = await source.CountAsync(cancellationToken);
        var items = await ApplySorting(source, query)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserListItemDto
                    {
                        Id = x.Id,
                        UserName = x.UserName,
                        FullName = x.FullName,
                        BirthDate = x.BirthDate,
                        IdentificationDate = x.IdentificationDate,
                        IdentificationNumber = x.IdentificationNumber,
                        UserInternalId = x.UserInternalId,
                        Mobile = x.Mobile,
                        ProfilePicUrl = x.ProfilePicUrl,
                        Role = x.Role,
                        IsActived = x.IsActived,
                        LastEnforceAnnouncementRead = x.LastEnforceAnnouncementRead,
                        IsDeleted = x.IsDeleted
                    })
            .ToListAsync(cancellationToken);

        return new PagedResult<UserListItemDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    public async Task<UserDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(x => x.Id == id && !x.IsDeleted)
            .Select(x => new UserDetailDto
                    {
                        Id = x.Id,
                        UserName = x.UserName,
                        FullName = x.FullName,
                        BirthDate = x.BirthDate,
                        IdentificationDate = x.IdentificationDate,
                        IdentificationNumber = x.IdentificationNumber,
                        UserInternalId = x.UserInternalId,
                        Mobile = x.Mobile,
                        ProfilePicUrl = x.ProfilePicUrl,
                        Role = x.Role,
                        IsActived = x.IsActived,
                        LastEnforceAnnouncementRead = x.LastEnforceAnnouncementRead,
                        IsDeleted = x.IsDeleted
                    })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<UserLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default)
    {
        var source = _dbContext.Users.AsNoTracking();
        source = source.Where(x => !x.IsDeleted);
        return await source
            .OrderBy(x => x.Id)
            .Select(x => new UserLookupDto
                    {
                        Id = x.Id,
                        UserName = x.UserName,
                        FullName = x.FullName,
                        UserInternalId = x.UserInternalId
                    })
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<User> ApplySorting(IQueryable<User> source, QueryParameters query)
    {
        var descending = string.Equals(query.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (query.SortBy?.Trim().ToLowerInvariant()) switch
        {
            "username" => descending ? source.OrderByDescending(x => x.UserName) : source.OrderBy(x => x.UserName),
            "fullname" => descending ? source.OrderByDescending(x => x.FullName) : source.OrderBy(x => x.FullName),
            "id" => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id),
            "birthdate" => descending ? source.OrderByDescending(x => x.BirthDate) : source.OrderBy(x => x.BirthDate),
            "identificationdate" => descending ? source.OrderByDescending(x => x.IdentificationDate) : source.OrderBy(x => x.IdentificationDate),
            "identificationnumber" => descending ? source.OrderByDescending(x => x.IdentificationNumber) : source.OrderBy(x => x.IdentificationNumber),
            "userinternalid" => descending ? source.OrderByDescending(x => x.UserInternalId) : source.OrderBy(x => x.UserInternalId),
            "mobile" => descending ? source.OrderByDescending(x => x.Mobile) : source.OrderBy(x => x.Mobile),
            "profilepicurl" => descending ? source.OrderByDescending(x => x.ProfilePicUrl) : source.OrderBy(x => x.ProfilePicUrl),
            "role" => descending ? source.OrderByDescending(x => x.Role) : source.OrderBy(x => x.Role),
            "isactived" => descending ? source.OrderByDescending(x => x.IsActived) : source.OrderBy(x => x.IsActived),
            "lastenforceannouncementread" => descending ? source.OrderByDescending(x => x.LastEnforceAnnouncementRead) : source.OrderBy(x => x.LastEnforceAnnouncementRead),
            _ => descending ? source.OrderByDescending(x => x.Id) : source.OrderBy(x => x.Id)
        };
    }

}
