using IndustrialApp.Application.Audit;
using IndustrialApp.Application.Security;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace IndustrialApp.Application.Ai;

public class AiAssistantService: IAiAssistantService
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

        var result = await _chat.GetChatMessageContentAsync(
            history,
            kernel: _kernel,
            cancellationToken: cancellationToken);

        var answer = result.Content ?? string.Empty;

        await _auditLogService.WriteAsync(
            userId,
            question,
            answer,
            cancellationToken);

        return answer;
    }
}