using ApprovalCenter.Api.Domain;
using ApprovalCenter.Api.Services;
using EFCore.Sharding;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApprovalCenter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApprovalsController(ApprovalService service, IDbAccessor db) : ControllerBase
{
    [HttpPost("submit")]
    public async Task<ActionResult<ApprovalInstance>> Submit([FromBody] SubmitApprovalRequest request)
    {
        var instance = await service.SubmitAsync(request);
        return Ok(instance);
    }

    [HttpGet("{businessType}/{businessId}")]
    public async Task<ActionResult<object>> GetByBusiness(string businessType, string businessId)
    {
	        var instance = await db.GetIQueryable<ApprovalInstance>()
	            .FirstOrDefaultAsync(x => x.BusinessType == businessType && x.BusinessId == businessId);
			if (instance == null)
			{
				return Ok(new { instance = (ApprovalInstance?)null, steps = Array.Empty<ApprovalStepInstance>() });
			}

			var steps = await db.GetIQueryable<ApprovalStepInstance>()
			    .Where(x => x.ApprovalInstanceId == instance.Id)
			    .OrderBy(x => x.StepOrder)
			    .ToListAsync();

			return Ok(new { instance, steps });
    }

	[HttpPost("{instanceId:guid}/steps/{stepOrder:int}/approve")]
	public async Task<ActionResult<ApprovalInstance>> Approve(Guid instanceId, int stepOrder, [FromBody] HandleStepRequest request)
	{
			try
			{
				var result = await service.ApproveStepAsync(instanceId, stepOrder, request);
				return Ok(result);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
	}

	[HttpPost("{instanceId:guid}/steps/{stepOrder:int}/reject")]
	public async Task<ActionResult<ApprovalInstance>> Reject(Guid instanceId, int stepOrder, [FromBody] HandleStepRequest request)
	{
			try
			{
				var result = await service.RejectStepAsync(instanceId, stepOrder, request);
				return Ok(result);
			}
			catch (InvalidOperationException ex)
			{
				return BadRequest(new { message = ex.Message });
			}
	}
}
