using IndustrialApp.Application.Ai;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IndustrialApp.Api.Pages.AiAssistant;

public sealed class IndexModel : PageModel
{
    private readonly IAiAssistantService _aiAssistantService;

    public IndexModel(IAiAssistantService aiAssistantService)
    {
        _aiAssistantService = aiAssistantService;
    }

    [BindProperty]
    public string Question { get; set; } = string.Empty;

    public string? Answer { get; private set; }

    public string? ErrorMessage { get; private set; }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(Question))
        {
            ErrorMessage = "Please enter a question.";
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
}