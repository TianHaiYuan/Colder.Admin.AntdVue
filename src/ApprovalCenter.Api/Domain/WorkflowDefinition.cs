using System.ComponentModel.DataAnnotations.Schema;

namespace ApprovalCenter.Api.Domain;

[Table("WorkflowDefinitions")]
public class WorkflowDefinition
{
    public Guid Id { get; set; }
    public string BusinessType { get; set; } = default!;
    public string Name { get; set; } = default!;
    public bool IsEnabled { get; set; } = true;
    public DateTime CreateTime { get; set; } = DateTime.Now;
}
