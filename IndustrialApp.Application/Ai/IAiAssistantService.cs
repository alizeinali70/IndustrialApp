namespace IndustrialApp.Application.Ai;

public interface IAiAssistantService
{
    Task<AskAiResponse> AskAsync(AskAiRequest request, CancellationToken cancellationToken);
}