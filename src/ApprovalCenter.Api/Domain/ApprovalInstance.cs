using System.ComponentModel.DataAnnotations.Schema;

namespace ApprovalCenter.Api.Domain;

[Table("ApprovalInstances")]
public class ApprovalInstance
{
    public Guid Id { get; set; }
    public string BusinessType { get; set; } = default!;
    public string BusinessId { get; set; } = default!;
    public Guid WorkflowDefinitionId { get; set; }
    public string Status { get; set; } = "Pending";
    public string Title { get; set; } = default!;
    public string? InitiatorUserId { get; set; }
    public string? InitiatorUserName { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.Now;
}
