// using System.Net.Http.Headers;
// using FarmingApi.Core;

// namespace EventHubChatting.Core;

// public class ServiceHttpClient
// {
// 	private readonly HttpClient _httpClient;

// 	public ServiceHttpClient(string path)
// 	{
// 		_httpClient = new HttpClient();
// 		var environment = _getEnv();
// 		_httpClient.BaseAddress = new Uri($"https://{path}-{environment}.eventhub.one");
// 		// _addToken();
// 	}

// 	private static string _getEnv()
// 	{
// 		var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
// 		return env switch
// 		{
// 			"Production" => "prod",
// 			"Uat" => "uat",
// 			_ => "dev"
// 		};
// 	}

// 	// private void _addToken()
// 	// {
// 	// 	var id = Environment.GetEnvironmentVariable("SERVICE_ID") ?? "93bfb7c4-dfde-487a-a5fb-0d01f77a64c5";
// 	// 	// var token = AuthenticationExtension.GenerateToken(Guid.Parse(id), "Service");
// 	// 	_httpClient.DefaultRequestHeaders.Authorization =
// 	// 		new AuthenticationHeaderValue("Bearer", token);
// 	// }

// 	public async Task<HttpResponseMessage> GetAsync(string path)
// 	{
// 		return await _httpClient.GetAsync(path);
// 	}

// 	public async Task<HttpResponseMessage> PostAsync<T>(string path, T body)
// 	{
// 		return await _httpClient.PostAsJsonAsync(path, body);
// 	}

// 	public async Task<HttpResponseMessage> PutAsync<T>(string path, T body)
// 	{
// 		return await _httpClient.PutAsJsonAsync(path, body);
// 	}

// 	public async Task<HttpResponseMessage> DeleteAsync(string path)
// 	{
// 		return await _httpClient.DeleteAsync(path);
// 	}
// }