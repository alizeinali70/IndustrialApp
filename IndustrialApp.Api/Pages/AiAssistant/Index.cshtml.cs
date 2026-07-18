using IndustrialApp.Application.Ai;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace IndustrialApp.Api.Pages.AiAssistant;

public sealed class IndexModel : PageModel
{
    private readonly IAiAssistantService _aiAssistantService;
    private readonly IAiHealthService _aiHealthService;
    private readonly AiOptions _aiOptions;

    public IndexModel(
        IAiAssistantService aiAssistantService,
        IAiHealthService aiHealthService,
        IOptions<AiOptions> aiOptions)
    {
        ArgumentNullException.ThrowIfNull(aiAssistantService);
        ArgumentNullException.ThrowIfNull(aiHealthService);
        ArgumentNullException.ThrowIfNull(aiOptions);

        _aiAssistantService = aiAssistantService;
        _aiHealthService = aiHealthService;
        _aiOptions = aiOptions.Value;
    }

    [BindProperty]
    public AskQuestionInput Input { get; set; } = new();

    public string? Answer { get; private set; }

    public string? ErrorMessage { get; private set; }

    public bool OllamaAvailable { get; private set; }

    public string ModelId => _aiOptions.ModelId;

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        OllamaAvailable = await _aiHealthService.IsAvailableAsync(cancellationToken);
    }

    public async Task<IActionResult> OnGetStatusAsync(CancellationToken cancellationToken)
    {
        var isAvailable = await _aiHealthService.IsAvailableAsync(cancellationToken);

        return new JsonResult(new
        {
            ollamaAvailable = isAvailable,
            statusText = isAvailable ? "Local AI ready" : "Ollama is offline",
            ariaLabel = isAvailable ? "Ollama is running" : "Ollama is not running"
        });
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        OllamaAvailable = await _aiHealthService.IsAvailableAsync(cancellationToken);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var userId = User.Identity?.Name ?? "local-dev-user";
            var response = await _aiAssistantService.AskAsync(
                new AskAiRequest(userId, Input.Question),
                cancellationToken);

            Answer = response.Answer;
        }
        catch (AiServiceUnavailableException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (UnauthorizedAccessException ex)
        {
            ErrorMessage = ex.Message;
        }

        return Page();
    }
}