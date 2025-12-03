using ApprovalCenter.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace ApprovalCenter.Api.Infrastructure;

public class ApprovalDbContext(DbContextOptions<ApprovalDbContext> options) : DbContext(options)
{
    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<WorkflowStepDefinition> WorkflowStepDefinitions => Set<WorkflowStepDefinition>();
    public DbSet<ApprovalInstance> ApprovalInstances => Set<ApprovalInstance>();
    public DbSet<ApprovalStepInstance> ApprovalStepInstances => Set<ApprovalStepInstance>();
}
