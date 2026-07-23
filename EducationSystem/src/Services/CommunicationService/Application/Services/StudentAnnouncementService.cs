using System.Text.RegularExpressions;
using CommunicationService.Application.DTOs.StudentAccess;
using CommunicationService.Application.Interfaces;
using CommunicationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CommunicationService.Application.Services;

public sealed partial class StudentAnnouncementService(CommunicationDbContext dbContext) : IStudentAnnouncementService
{
    private static readonly string[] AllowedRoutes =
    [
        "/student/schedule", "/student/subjects", "/student/exam-results",
        "/student/announcements", "/student/documents", "/student/tuition",
        "/student/form-requests"
    ];

    public async Task<IReadOnlyList<StudentAnnouncementDto>> GetAllAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var userIdText = userId.ToString();
        var candidates = await dbContext.UserAnnouncements.AsNoTracking()
            .Where(item => !item.IsDeleted && item.UserIds.Contains(userIdText))
            .OrderByDescending(item => item.CreationDate)
            .Select(item => new
            {
                item.Id, item.Type, item.UserIds, item.Message, item.CreationDate,
                item.Status, item.EnforceRead, item.NotificationType, item.DeepLink
            })
            .ToListAsync(cancellationToken);

        return candidates
            .Where(item => RecipientIds().Matches(item.UserIds)
                .Select(match => match.Value)
                .Any(value => Guid.TryParse(value, out var parsed) && parsed == userId))
            .Select(item => new StudentAnnouncementDto(
                item.Id,
                item.Type,
                item.Message,
                item.CreationDate,
                item.Status,
                item.EnforceRead,
                item.NotificationType,
                AllowedRoutes.Any(route => string.Equals(item.DeepLink, route, StringComparison.OrdinalIgnoreCase))
                    ? item.DeepLink
                    : null))
            .ToList();
    }

    [GeneratedRegex(@"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}")]
    private static partial Regex RecipientIds();
}
