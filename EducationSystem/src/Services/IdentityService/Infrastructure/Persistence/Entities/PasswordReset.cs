using System;
using System.Collections.Generic;

namespace IdentityService.Infrastructure.Persistence.Entities;

public partial class PasswordReset
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTime CreationDate { get; set; }

    public bool IsDeactive { get; set; }
}
