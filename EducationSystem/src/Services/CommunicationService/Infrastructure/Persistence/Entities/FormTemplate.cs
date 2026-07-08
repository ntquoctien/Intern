using System;
using System.Collections.Generic;

namespace CommunicationService.Infrastructure.Persistence.Entities;

public partial class FormTemplate
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? DocumentUrl { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<FormRequest> FormRequests { get; set; } = new List<FormRequest>();
}
