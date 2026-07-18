using IndustrialApp.Application.Ai;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IndustrialApp.Api.Pages.AiAssistant;

public sealed class IndexModel : PageModel
{
    private readonly IAiAssistantService _aiAssistantService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public IndexModel(
        IAiAssistantService aiAssistantService,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _aiAssistantService = aiAssistantService;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [BindProperty]
    public string Question { get; set; } = string.Empty;

    public string? Answer { get; private set; }

    public string? ErrorMessage { get; private set; }

    public bool OllamaAvailable { get; private set; }

    public string ModelId => _configuration["Ai:ModelId"] ?? "Unknown model";

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        OllamaAvailable = await GetOllamaAvailabilityAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnGetStatusAsync(CancellationToken cancellationToken)
    {
        var isAvailable = await GetOllamaAvailabilityAsync(cancellationToken);

        return new JsonResult(new
        {
            ollamaAvailable = isAvailable,
            statusText = isAvailable ? "Local AI ready" : "Ollama is offline",
            ariaLabel = isAvailable ? "Ollama is running" : "Ollama is not running"
        });
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        OllamaAvailable = await GetOllamaAvailabilityAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(Question))
        {
            ErrorMessage = "Please enter a question.";
            OllamaAvailable = await GetOllamaAvailabilityAsync(cancellationToken);
            return Page();
        }

        try
        {
            var userId = User.Identity?.Name ?? "local-dev-user";

            Answer = await _aiAssistantService.AskAsync(
                userId,
                Question,
                cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (HttpRequestException ex)
        {
            ErrorMessage = ex.Message;
        }


        return Page();
    }

    private async Task<bool> GetOllamaAvailabilityAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var client = _httpClientFactory.CreateClient("OllamaHealth");
            using var response = await client.GetAsync(
                new Uri(client.BaseAddress!, "api/tags"),
                cancellationToken);

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