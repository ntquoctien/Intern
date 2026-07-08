using System;
using System.Collections.Generic;

namespace CommunicationService.Infrastructure.Persistence.Entities;

public partial class UserAnnouncement
{
    public Guid Id { get; set; }

    public int Type { get; set; }

    public string UserIds { get; set; } = null!;

    public string Message { get; set; } = null!;

    public DateTime CreationDate { get; set; }

    public int Status { get; set; }

    public string DeepLink { get; set; } = null!;

    public string DeepLinkParam { get; set; } = null!;

    public Guid EntityObjectId { get; set; }

    public bool EnforceRead { get; set; }

    public int? NotificationType { get; set; }

    public bool IsDeleted { get; set; }
}
