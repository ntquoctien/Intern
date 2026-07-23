using IdentityService.Application.DTOs.Management;
using IdentityService.Application.Interfaces;
using IdentityService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IdentityService.Application.Services;

public sealed class ManagementSystemReadService(IdentityDbContext dbContext) : IManagementSystemReadService
{
    private static readonly HashSet<string> SafeValueKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "Site:UniversityName", "System:TimeZone", "Email:From", "Email:SmtpHost",
        "Email:SmtpPort", "Storage:UploadPath", "Feature:EnableNotification"
    };

    public async Task<ManagementSystemOverviewDto> GetOverviewAsync(CancellationToken cancellationToken)
    {
        var rawSettings = await dbContext.Settings.AsNoTracking().Where(item => !item.IsDeleted)
            .OrderBy(item => item.Key).ToListAsync(cancellationToken);
        var settings = rawSettings.Select(item => new SafeSettingDto(item.Id, item.Key,
            SafeValueKeys.Contains(item.Key) ? item.Value : "••••••••", !SafeValueKeys.Contains(item.Key))).ToList();
        var devices = await dbContext.UserDevices.AsNoTracking().GroupBy(item => item.DeviceType)
            .Select(group => new DeviceTypeCountDto(group.Key, group.Count())).ToListAsync(cancellationToken);
        var audits = await dbContext.AuditLogs.AsNoTracking().Where(item => !item.IsDeleted)
            .OrderByDescending(item => item.CreationDate).Take(10)
            .Select(item => new SafeAuditDto(item.Id, item.Action, item.CreationDate,
                item.RecordEntity, item.RecordDesc)).ToListAsync(cancellationToken);
        return new ManagementSystemOverviewDto(settings, settings.Count,
            await dbContext.AuditLogs.CountAsync(item => !item.IsDeleted, cancellationToken),
            await dbContext.UserDevices.CountAsync(cancellationToken),
            await dbContext.PasswordResets.CountAsync(cancellationToken),
            devices, audits);
    }
}
