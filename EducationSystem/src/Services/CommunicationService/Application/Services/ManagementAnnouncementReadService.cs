using System.Text.Json;
using CommunicationService.Application.DTOs.Management;
using CommunicationService.Application.Interfaces;
using CommunicationService.Infrastructure.Persistence;
using CommunicationService.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommunicationService.Application.Services;

public sealed class ManagementAnnouncementReadService(CommunicationDbContext dbContext) : IManagementAnnouncementReadService
{
    public async Task<ManagementAnnouncementPageDto> GetPageAsync(ManagementAnnouncementQueryDto query, CancellationToken cancellationToken)
    {
        var source = ApplyFilters(dbContext.UserAnnouncements.AsNoTracking().Where(item => !item.IsDeleted), query);
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var total = await source.CountAsync(cancellationToken);
        var statusCounts = await source.GroupBy(item => item.Status)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Key, item => item.Count, cancellationToken);
        var enforceCount = await source.CountAsync(item => item.EnforceRead, cancellationToken);
        var rows = await source.OrderByDescending(item => item.CreationDate)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new ManagementAnnouncementPageDto(rows.Select(Map).ToList(), pageNumber, pageSize,
            total, statusCounts, enforceCount);
    }

    public async Task<ManagementAnnouncementItemDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await dbContext.UserAnnouncements.AsNoTracking()
            .SingleOrDefaultAsync(value => value.Id == id && !value.IsDeleted, cancellationToken);
        return item is null ? null : Map(item);
    }

    private static IQueryable<UserAnnouncement> ApplyFilters(IQueryable<UserAnnouncement> source,
        ManagementAnnouncementQueryDto query)
    {
        if (!string.IsNullOrWhiteSpace(query.Search)) { var search = query.Search.Trim(); source = source.Where(item => item.Message.Contains(search)); }
        if (!string.IsNullOrWhiteSpace(query.Recipient)) { var recipient = query.Recipient.Trim(); source = source.Where(item => item.UserIds.Contains(recipient)); }
        if (query.Type.HasValue) source = source.Where(item => item.Type == query.Type);
        if (query.NotificationType.HasValue) source = source.Where(item => item.NotificationType == query.NotificationType);
        if (query.Status.HasValue) source = source.Where(item => item.Status == query.Status);
        if (query.EnforceRead.HasValue) source = source.Where(item => item.EnforceRead == query.EnforceRead);
        if (query.FromDate.HasValue) source = source.Where(item => item.CreationDate >= query.FromDate.Value);
        if (query.ToDate.HasValue) source = source.Where(item => item.CreationDate < query.ToDate.Value.AddDays(1));
        return source;
    }

    private static ManagementAnnouncementItemDto Map(UserAnnouncement item)
    {
        var count = RecipientCount(item.UserIds);
        return new ManagementAnnouncementItemDto(item.Id, item.Type, item.NotificationType,
            item.Status, item.Message, item.CreationDate, item.EnforceRead,
            AllowlistedLink(item.DeepLink), string.IsNullOrWhiteSpace(item.DeepLinkParam) ? null : item.DeepLinkParam,
            item.EntityObjectId, count, count == 0 ? "Không xác định người nhận" : $"{count:N0} người nhận");
    }

    private static int RecipientCount(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return 0;
        try
        {
            using var document = JsonDocument.Parse(raw);
            if (document.RootElement.ValueKind == JsonValueKind.Array) return document.RootElement.GetArrayLength();
            if (document.RootElement.ValueKind == JsonValueKind.String) return 1;
        }
        catch (JsonException) { }
        return raw.Split([',', ';', '|'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;
    }

    private static string? AllowlistedLink(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.StartsWith('/')) return null;
        return value.StartsWith("/student/", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/management/", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/exam/", StringComparison.OrdinalIgnoreCase)
            ? value : null;
    }
}
