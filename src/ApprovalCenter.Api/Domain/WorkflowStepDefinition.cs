using System.ComponentModel.DataAnnotations.Schema;

namespace ApprovalCenter.Api.Domain;

[Table("WorkflowStepDefinitions")]
public class WorkflowStepDefinition
{
    public Guid Id { get; set; }
    public Guid WorkflowDefinitionId { get; set; }
    public int StepOrder { get; set; }
    public string StepName { get; set; } = default!;
    public string ApproverRoleOrUserIds { get; set; } = default!;
}
