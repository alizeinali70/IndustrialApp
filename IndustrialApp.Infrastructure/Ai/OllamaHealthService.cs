using IndustrialApp.Application.Ai;
using System.Net.Http;

namespace IndustrialApp.Infrastructure.Ai;

public sealed class OllamaHealthService : IAiHealthService
{
    public const string HttpClientName = "OllamaHealth";

    private readonly IHttpClientFactory _httpClientFactory;

    public OllamaHealthService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient(HttpClientName);
            using var response = await client.GetAsync(new Uri("api/tags", UriKind.Relative), cancellationToken);

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }
}
