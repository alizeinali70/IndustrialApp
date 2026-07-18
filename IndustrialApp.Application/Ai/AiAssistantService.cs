using IndustrialApp.Application.Audit;
using IndustrialApp.Application.Security;

namespace IndustrialApp.Application.Ai;

public sealed class AiAssistantService : IAiAssistantService
{
    private readonly IAiChatService _aiChatService;
    private readonly IPermissionService _permissionService;
    private readonly IAuditLogService _auditLogService;

    public AiAssistantService(
        IAiChatService aiChatService,
        IPermissionService permissionService,
        IAuditLogService auditLogService)
    {
        _aiChatService = aiChatService;
        _permissionService = permissionService;
        _auditLogService = auditLogService;
    }

    public async Task<AskAiResponse> AskAsync(AskAiRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Question))
        {
            throw new ArgumentException("Question is required.", nameof(request));
        }

        if (!await _permissionService.CanUseAiAsync(request.UserId, cancellationToken))
        {
            throw new UnauthorizedAccessException("User is not allowed to use AI assistant.");
        }

        var answer = await _aiChatService.GetAnswerAsync(request.Question, cancellationToken);

        await _auditLogService.WriteAsync(
            request.UserId,
            request.Question,
            answer,
            cancellationToken);

        return new AskAiResponse(answer);
    }
}