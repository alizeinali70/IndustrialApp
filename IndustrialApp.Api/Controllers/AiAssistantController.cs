using IndustrialApp.Application.Ai;
using IndustrialApp.Domain.Ai;
using Microsoft.AspNetCore.Mvc;

namespace IndustrialApp.Api.Controllers;

[ApiController]
[Route("api/ai-assistant")]
public sealed class AiAssistantController : ControllerBase
{
    private readonly IAiAssistantService _aiAssistantService;

    public AiAssistantController(IAiAssistantService aiAssistantService)
    {
        _aiAssistantService = aiAssistantService;
    }

    [HttpPost("ask")]
    public async Task<ActionResult<string>> AskAsync(
        [FromBody] AiRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        if (string.IsNullOrWhiteSpace(request.Question))
            return BadRequest("Question is required.");

        var userId = User.Identity?.Name ?? "local-dev-user";

        var answer = await _aiAssistantService.AskAsync(
            userId,
            request.Question,
            cancellationToken);

        return Ok(answer);
    }
}