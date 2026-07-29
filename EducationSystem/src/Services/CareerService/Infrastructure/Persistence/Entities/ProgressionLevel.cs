using System;
using System.Collections.Generic;

namespace CareerService.Infrastructure.Persistence.Entities;

public partial class ProgressionLevel
{
    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string VietnameseName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public byte Rank { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CloPloMapping> CloPloMappings { get; set; } = new List<CloPloMapping>();
}
