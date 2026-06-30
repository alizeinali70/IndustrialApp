using IndustrialApp.Application.Audit;
using IndustrialApp.Application.Security;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace IndustrialApp.Application.Ai;

public class AiAssistantService : IAiAssistantService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chat;
    private readonly IPermissionService _permissionService;
    private readonly IAuditLogService _auditLogService;

    public AiAssistantService(
        Kernel kernel,
        IChatCompletionService chat,
        IPermissionService permissionService,
        IAuditLogService auditLogService)
    {
        _kernel = kernel;
        _chat = chat;
        _permissionService = permissionService;
        _auditLogService = auditLogService;
    }

    public async Task<string> AskAsync(
        string userId,
        string question,
        CancellationToken cancellationToken)
    {
        if (!await _permissionService.CanUseAiAsync(userId, cancellationToken))
            throw new UnauthorizedAccessException("User is not allowed to use AI assistant.");

        var history = new ChatHistory();

        history.AddSystemMessage("""
                                 You are an AI assistant inside industrial production software.

                                 Rules:
                                 - Do not invent data.
                                 - Do not make production decisions.
                                 - Do not modify data.
                                 - If information is missing, say that it is missing.
                                 - Give practical, concise answers.
                                 - Ask for human confirmation before any risky action.
                                 """);

        history.AddUserMessage(question);

        string answer;

        try
        {
            var result = await _chat.GetChatMessageContentAsync(
                history,
                kernel: _kernel,
                cancellationToken: cancellationToken);

            answer = result.Content ?? string.Empty;
        }
        catch (Exception ex) when (IsOllamaConnectionProblem(ex))
        {
            throw new InvalidOperationException(
                "Ollama is not running. Please start Ollama and try again.",
                ex);
        }

        await _auditLogService.WriteAsync(
            userId,
            question,
            answer,
            cancellationToken);

        return answer;
    }

    private static bool IsOllamaConnectionProblem(Exception ex)
    {
        if (ex is HttpRequestException)
            return true;

        if (ex.InnerException is not null && IsOllamaConnectionProblem(ex.InnerException))
            return true;

        var message = ex.ToString();

        return message.Contains("localhost:11434", StringComparison.OrdinalIgnoreCase)
               || message.Contains("connection refused", StringComparison.OrdinalIgnoreCase)
               || message.Contains("actively refused", StringComparison.OrdinalIgnoreCase)
               || message.Contains("verweigert", StringComparison.OrdinalIgnoreCase)
               || message.Contains("verweigerte", StringComparison.OrdinalIgnoreCase)
               || message.Contains("No connection could be made", StringComparison.OrdinalIgnoreCase);
    }
}