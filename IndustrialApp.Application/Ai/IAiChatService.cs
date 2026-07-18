namespace IndustrialApp.Application.Ai;

public interface IAiChatService
{
    Task<string> GetAnswerAsync(string question, CancellationToken cancellationToken);
}
