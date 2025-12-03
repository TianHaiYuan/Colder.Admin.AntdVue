namespace NotificationCenter.Api.Infrastructure;

public class NotificationOptions
{
	public bool EnableSignalR { get; set; } = true;
	public bool EnableDingTalk { get; set; } = false;
	public string[]? SignalREventTypes { get; set; } = new[] { "approval.step.pending" };
	public string[]? DingTalkEventTypes { get; set; } = new[] { "approval.step.pending" };
}

public class DingTalkOptions
{
	public string Webhook { get; set; } = string.Empty;
	public string Secret { get; set; } = string.Empty;
}
