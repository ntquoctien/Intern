using System;
using System.Collections.Generic;

namespace IdentityService.Infrastructure.Persistence.Entities;

public partial class Setting
{
    public Guid Id { get; set; }

    public string Key { get; set; } = null!;

    public string Value { get; set; } = null!;

    public bool IsDeleted { get; set; }
}
