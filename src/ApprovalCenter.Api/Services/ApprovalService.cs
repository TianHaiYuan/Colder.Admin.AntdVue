using ApprovalCenter.Api.Domain;
using ApprovalCenter.Api.Infrastructure;
using EFCore.Sharding;
using Microsoft.EntityFrameworkCore;

namespace ApprovalCenter.Api.Services;

public class SubmitApprovalRequest
{
	public string BusinessType { get; set; } = default!;
	public string BusinessId { get; set; } = default!;
	public string Title { get; set; } = default!;
	public string? InitiatorUserId { get; set; }
	public string? InitiatorUserName { get; set; }
}

public class HandleStepRequest
{
	public string? ApproverUserId { get; set; }
	public string? ApproverUserName { get; set; }
	public string? Remark { get; set; }
}

public class ApprovalService
{
	    private readonly IDbAccessor _db;
    private readonly IEventPublisher _publisher;

	    public ApprovalService(IDbAccessor db, IEventPublisher publisher)
    {
        _db = db;
        _publisher = publisher;
    }

    public async Task<ApprovalInstance> SubmitAsync(SubmitApprovalRequest request)
    {
	        var workflow = await _db.GetIQueryable<WorkflowDefinition>()
	            .Where(x => x.BusinessType == request.BusinessType && x.IsEnabled)
	            .OrderByDescending(x => x.CreateTime)
	            .FirstOrDefaultAsync();
	        if (workflow == null)
	            throw new InvalidOperationException($"Workflow not configured for {request.BusinessType}");

	        var steps = await _db.GetIQueryable<WorkflowStepDefinition>()
	            .Where(x => x.WorkflowDefinitionId == workflow.Id)
	            .OrderBy(x => x.StepOrder)
	            .ToListAsync();

	        var instance = new ApprovalInstance
	        {
	            Id = Guid.NewGuid(),
	            BusinessType = request.BusinessType,
	            BusinessId = request.BusinessId,
	            WorkflowDefinitionId = workflow.Id,
	            Title = request.Title,
	            InitiatorUserId = request.InitiatorUserId,
	            InitiatorUserName = request.InitiatorUserName,
	            Status = "Pending"
	        };

	        var stepInstances = steps.Select(s => new ApprovalStepInstance
	        {
	            Id = Guid.NewGuid(),
	            ApprovalInstanceId = instance.Id,
	            StepOrder = s.StepOrder,
	            StepName = s.StepName,
	            ApproverUserIds = s.ApproverRoleOrUserIds,
	            Status = s.StepOrder == 1 ? "Pending" : "Waiting"
	        }).ToList();

	        var txResult = await _db.RunTransactionAsync(async () =>
	        {
	            await _db.InsertAsync(instance);
	            await _db.InsertAsync(stepInstances);
	        });
	        if (!txResult.Success)
	            throw txResult.ex;

        _publisher.Publish("approval.submitted", new
        {
            EventType = "approval.submitted",
            instance.BusinessType,
            instance.BusinessId,
            ApprovalInstanceId = instance.Id,
            instance.Title,
            instance.InitiatorUserId,
            instance.InitiatorUserName
        });

        var firstStep = stepInstances.FirstOrDefault(x => x.StepOrder == 1);
        if (firstStep != null)
        {
            _publisher.Publish("approval.step.pending", new
            {
                EventType = "approval.step.pending",
                instance.BusinessType,
                instance.BusinessId,
                ApprovalInstanceId = instance.Id,
                StepOrder = firstStep.StepOrder,
                StepName = firstStep.StepName,
                ApproverUserIds = firstStep.ApproverUserIds,
                instance.Title,
                instance.InitiatorUserId,
                instance.InitiatorUserName
            });
        }

        return instance;
    }

		public async Task<ApprovalInstance> ApproveStepAsync(Guid instanceId, int stepOrder, HandleStepRequest request)
		{
			ApprovalInstance? instance = null;
			ApprovalStepInstance? step = null;
			ApprovalStepInstance? nextStep = null;

			var txResult = await _db.RunTransactionAsync(async () =>
			{
				instance = await _db.GetIQueryable<ApprovalInstance>()
				    .FirstOrDefaultAsync(x => x.Id == instanceId)
				    ?? throw new InvalidOperationException("Approval instance not found");

				step = await _db.GetIQueryable<ApprovalStepInstance>()
				    .FirstOrDefaultAsync(x => x.ApprovalInstanceId == instanceId && x.StepOrder == stepOrder)
				    ?? throw new InvalidOperationException("Approval step not found");

				if (step.Status != "Pending")
					throw new InvalidOperationException("当前步骤不是待审批状态，无法再次审批");

				// 审批权限校验：必须传入当前审批人 Id，且在配置的 ApproverUserIds 列表中
				if (string.IsNullOrWhiteSpace(request.ApproverUserId))
					throw new InvalidOperationException("审批操作必须提供当前审批人标识(ApproverUserId)");
				var currentUserId = request.ApproverUserId!;
				var allowedIds = (step.ApproverUserIds ?? string.Empty)
				    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				if (allowedIds.Length > 0 && !allowedIds.Contains(currentUserId, StringComparer.OrdinalIgnoreCase))
					throw new InvalidOperationException("当前用户不是本步骤审批人，无权执行此操作");

				step.Status = "Approved";
				step.ActionTime = DateTime.Now;
				if (!string.IsNullOrWhiteSpace(request.Remark))
					step.Remark = request.Remark;

				// 如果 ApproverUserId 传入，则覆盖为真实审批人，便于后续查询
				if (!string.IsNullOrWhiteSpace(request.ApproverUserId))
					step.ApproverUserIds = request.ApproverUserId!;

				// 查找下一步
				nextStep = await _db.GetIQueryable<ApprovalStepInstance>()
				    .Where(x => x.ApprovalInstanceId == instanceId && x.StepOrder > stepOrder)
				    .OrderBy(x => x.StepOrder)
				    .FirstOrDefaultAsync();

				if (nextStep != null)
				{
					nextStep.Status = "Pending";
					instance.Status = "Pending";
				}
				else
				{
					// 已是最后一步
					instance.Status = "Approved";
				}

				await _db.UpdateAsync(step);
				if (nextStep != null)
					await _db.UpdateAsync(nextStep);
				await _db.UpdateAsync(instance);
			});

			if (!txResult.Success)
				throw txResult.ex;

			var resultInstance = instance!;

			// 当前步骤通过事件
			_publisher.Publish("approval.step.approved", new
			{
				EventType = "approval.step.approved",
				resultInstance.BusinessType,
				resultInstance.BusinessId,
				ApprovalInstanceId = resultInstance.Id,
				step!.StepOrder,
				step.StepName,
				step.Remark,
				request.ApproverUserId,
				request.ApproverUserName,
				resultInstance.Title,
				resultInstance.InitiatorUserId,
				resultInstance.InitiatorUserName
			});

			// 下一步待审事件
			if (nextStep != null)
			{
				_publisher.Publish("approval.step.pending", new
				{
					EventType = "approval.step.pending",
					resultInstance.BusinessType,
					resultInstance.BusinessId,
					ApprovalInstanceId = resultInstance.Id,
					nextStep.StepOrder,
					nextStep.StepName,
					nextStep.ApproverUserIds,
					resultInstance.Title,
					resultInstance.InitiatorUserId,
					resultInstance.InitiatorUserName
				});
			}
			else
			{
				// 审批全部完成事件
				_publisher.Publish("approval.completed", new
				{
					EventType = "approval.completed",
					resultInstance.BusinessType,
					resultInstance.BusinessId,
					ApprovalInstanceId = resultInstance.Id,
					resultInstance.Status,
					resultInstance.Title,
					resultInstance.InitiatorUserId,
					resultInstance.InitiatorUserName
				});
			}

			return resultInstance;
		}

		public async Task<ApprovalInstance> RejectStepAsync(Guid instanceId, int stepOrder, HandleStepRequest request)
		{
			ApprovalInstance? instance = null;
			ApprovalStepInstance? step = null;
			List<ApprovalStepInstance> laterSteps = new();

			var txResult = await _db.RunTransactionAsync(async () =>
			{
				instance = await _db.GetIQueryable<ApprovalInstance>()
				    .FirstOrDefaultAsync(x => x.Id == instanceId)
				    ?? throw new InvalidOperationException("Approval instance not found");

				step = await _db.GetIQueryable<ApprovalStepInstance>()
				    .FirstOrDefaultAsync(x => x.ApprovalInstanceId == instanceId && x.StepOrder == stepOrder)
				    ?? throw new InvalidOperationException("Approval step not found");

				if (step.Status != "Pending")
					throw new InvalidOperationException("当前步骤不是待审批状态，无法驳回");

				// 审批权限校验
				if (string.IsNullOrWhiteSpace(request.ApproverUserId))
					throw new InvalidOperationException("审批操作必须提供当前审批人标识(ApproverUserId)");
				var currentUserId = request.ApproverUserId!;
				var allowedIds = (step.ApproverUserIds ?? string.Empty)
				    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
				if (allowedIds.Length > 0 && !allowedIds.Contains(currentUserId, StringComparer.OrdinalIgnoreCase))
					throw new InvalidOperationException("当前用户不是本步骤审批人，无权执行此操作");

				step.Status = "Rejected";
				step.ActionTime = DateTime.Now;
				if (!string.IsNullOrWhiteSpace(request.Remark))
					step.Remark = request.Remark;
				if (!string.IsNullOrWhiteSpace(request.ApproverUserId))
					step.ApproverUserIds = request.ApproverUserId!;

				// 后续所有步骤标记为已取消
				laterSteps = await _db.GetIQueryable<ApprovalStepInstance>()
				    .Where(x => x.ApprovalInstanceId == instanceId && x.StepOrder > stepOrder)
				    .ToListAsync();
				foreach (var s in laterSteps)
				{
					s.Status = "Cancelled";
				}

				instance.Status = "Rejected";

				await _db.UpdateAsync(step);
				if (laterSteps.Count > 0)
					await _db.UpdateAsync(laterSteps);
				await _db.UpdateAsync(instance);
			});

			if (!txResult.Success)
				throw txResult.ex;

			var resultInstance = instance!;

			_publisher.Publish("approval.step.rejected", new
			{
				EventType = "approval.step.rejected",
				resultInstance.BusinessType,
				resultInstance.BusinessId,
				ApprovalInstanceId = resultInstance.Id,
				step!.StepOrder,
				step.StepName,
				step.Remark,
				request.ApproverUserId,
				request.ApproverUserName,
				resultInstance.Title,
				resultInstance.InitiatorUserId,
				resultInstance.InitiatorUserName
			});

			_publisher.Publish("approval.completed", new
			{
				EventType = "approval.completed",
				resultInstance.BusinessType,
				resultInstance.BusinessId,
				ApprovalInstanceId = resultInstance.Id,
				resultInstance.Status,
				resultInstance.Title,
				resultInstance.InitiatorUserId,
				resultInstance.InitiatorUserName
			});

			return resultInstance;
		}
}
