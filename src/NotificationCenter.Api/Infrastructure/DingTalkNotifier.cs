using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace NotificationCenter.Api.Infrastructure;

public class DingTalkNotifier
{
	private readonly HttpClient _httpClient;
	private readonly DingTalkOptions _options;

	public DingTalkNotifier(IOptions<DingTalkOptions> options)
	{
		_options = options.Value;
		_httpClient = new HttpClient();
	}

	public async Task SendAsync(string message)
	{
		if (string.IsNullOrWhiteSpace(_options.Webhook))
			return;

		var payload = new
		{
			msgtype = "text",
			text = new { content = message }
		};

		var json = JsonSerializer.Serialize(payload);
		using var request = new HttpRequestMessage(HttpMethod.Post, _options.Webhook)
		{
			Content = new StringContent(json, Encoding.UTF8, "application/json")
		};
		request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

		try
		{
			await _httpClient.SendAsync(request);
		}
		catch
		{
			// 忽略钉钉发送异常，避免影响主流程
		}
	}
}
