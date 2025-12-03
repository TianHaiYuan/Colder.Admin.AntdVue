using Microsoft.AspNetCore.SignalR;

namespace NotificationCenter.Api.Infrastructure;

public class QueryStringUserIdProvider : IUserIdProvider
{
	public string? GetUserId(HubConnectionContext connection)
	{
		var httpContext = connection.GetHttpContext();
		return httpContext?.Request.Query["userId"].ToString();
	}
}
