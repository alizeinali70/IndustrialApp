namespace IndustrialApp.Application.Ai;

public interface IAiAssistantService
{
    Task<string> AskAsync(string userId, string question, CancellationToken cancellationToken);

}