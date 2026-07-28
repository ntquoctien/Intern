using System;
using System.Collections.Generic;

namespace CommunicationService.Infrastructure.Persistence.Entities;

public partial class FormRequest
{
    public Guid Id { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime UpdateDate { get; set; }

    public Guid StudentId { get; set; }

    public Guid? FormTemplateId { get; set; }

    public Guid? ApprovalId { get; set; }

    public string ApprovalName { get; set; } = null!;

    public string Note { get; set; } = null!;

    public int Status { get; set; }

    public Guid? EmployerToken { get; set; }

    public int EmployerVerifiedStatus { get; set; }

    public string? VerificationData { get; set; }

    public bool IsDeleted { get; set; }

    public virtual FormTemplate? FormTemplate { get; set; }
}
