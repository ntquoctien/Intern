using IdentityService.Application.DTOs.Users;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/management/users")]
public sealed class ManagementUsersController(IdentityDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ApiResponse<IReadOnlyList<ManagementUserDto>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await dbContext.Users.AsNoTracking()
            .Where(user => !user.IsDeleted)
            .OrderBy(user => user.UserName)
            .Select(user => new
            {
                user.Id, user.UserName, user.FullName, user.BirthDate, user.IdentificationDate,
                user.IdentificationNumber, user.UserInternalId, user.Mobile, user.ProfilePicUrl,
                user.Role, user.IsActived, user.LastEnforceAnnouncementRead
            })
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyList<ManagementUserDto>>.Ok(users.Select(user => new ManagementUserDto(
            user.Id, user.UserName, user.FullName, user.BirthDate, user.IdentificationDate,
            Mask(user.IdentificationNumber), user.UserInternalId, Mask(user.Mobile, 3),
            user.ProfilePicUrl, user.Role, user.IsActived, user.LastEnforceAnnouncementRead)).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ManagementUserDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.AsNoTracking().SingleOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);
        if (user is null) return NotFound(ApiResponse<ManagementUserDto>.Fail("User not found."));

        return Ok(ApiResponse<ManagementUserDto>.Ok(new ManagementUserDto(
            user.Id, user.UserName, user.FullName, user.BirthDate, user.IdentificationDate,
            Mask(user.IdentificationNumber), user.UserInternalId, Mask(user.Mobile, 3),
            user.ProfilePicUrl, user.Role, user.IsActived, user.LastEnforceAnnouncementRead)));
    }

    private static string? Mask(string? value, int visible = 4)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim();
        if (normalized.Length <= visible) return new string('*', normalized.Length);
        return new string('*', normalized.Length - visible) + normalized[^visible..];
    }
}
