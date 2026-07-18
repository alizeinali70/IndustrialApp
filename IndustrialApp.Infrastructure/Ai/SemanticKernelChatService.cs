using IndustrialApp.Application.Ai;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace IndustrialApp.Infrastructure.Ai;

public sealed class SemanticKernelChatService : IAiChatService
{
    private const string SystemPrompt = """
                                        You are an AI assistant inside industrial production software.

                                        Rules:
                                        - Do not invent data.
                                        - Do not make production decisions.
                                        - Do not modify data.
                                        - If information is missing, say that it is missing.
                                        - Give practical, concise answers.
                                        - Ask for human confirmation before any risky action.
                                        """;

    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletionService;

    public SemanticKernelChatService(Kernel kernel, IChatCompletionService chatCompletionService)
    {
        _kernel = kernel;
        _chatCompletionService = chatCompletionService;
    }

    public async Task<string> GetAnswerAsync(string question, CancellationToken cancellationToken)
    {
        var history = new ChatHistory();
        history.AddSystemMessage(SystemPrompt);
        history.AddUserMessage(question);

        try
        {
            var result = await _chatCompletionService.GetChatMessageContentAsync(
                history,
                kernel: _kernel,
                cancellationToken: cancellationToken);

            return result.Content ?? string.Empty;
        }
        catch (Exception ex) when (IsOllamaConnectionProblem(ex))
        {
            throw new AiServiceUnavailableException(
                "Ollama is not running. Please start Ollama and try again.",
                ex);
        }
    }

    private static bool IsOllamaConnectionProblem(Exception ex)
    {
        if (ex is HttpRequestException)
        {
            return true;
        }

        if (ex.InnerException is not null && IsOllamaConnectionProblem(ex.InnerException))
        {
            return true;
        }

        var message = ex.ToString();

        return message.Contains("localhost:11434", StringComparison.OrdinalIgnoreCase)
               || message.Contains("connection refused", StringComparison.OrdinalIgnoreCase)
               || message.Contains("actively refused", StringComparison.OrdinalIgnoreCase)
               || message.Contains("verweigert", StringComparison.OrdinalIgnoreCase)
               || message.Contains("verweigerte", StringComparison.OrdinalIgnoreCase)
               || message.Contains("No connection could be made", StringComparison.OrdinalIgnoreCase);
    }
}
