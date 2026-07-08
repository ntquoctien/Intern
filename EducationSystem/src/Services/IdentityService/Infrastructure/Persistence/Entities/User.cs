using System;
using System.Collections.Generic;

namespace IdentityService.Infrastructure.Persistence.Entities;

public partial class User
{
    public Guid Id { get; set; }

    public string UserName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public byte[] PasswordSalt { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public DateTime? BirthDate { get; set; }

    public DateTime? IdentificationDate { get; set; }

    public string? IdentificationNumber { get; set; }

    public string UserInternalId { get; set; } = null!;

    public string? Mobile { get; set; }

    public string? ProfilePicUrl { get; set; }

    public int Role { get; set; }

    public bool IsActived { get; set; }

    public DateTime? LastEnforceAnnouncementRead { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public virtual ICollection<UserDevice> UserDevices { get; set; } = new List<UserDevice>();
}
