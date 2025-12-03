using System.ComponentModel.DataAnnotations.Schema;

namespace ApprovalCenter.Api.Domain;

[Table("ApprovalStepInstances")]
public class ApprovalStepInstance
{
    public Guid Id { get; set; }
    public Guid ApprovalInstanceId { get; set; }
    public int StepOrder { get; set; }
    public string StepName { get; set; } = default!;
    public string ApproverUserIds { get; set; } = default!;
    public string Status { get; set; } = "Pending";
    public string? Remark { get; set; }
    public DateTime? ActionTime { get; set; }
}
