using IndustrialApp.Application.Ai;
using IndustrialApp.Application.Audit;
using IndustrialApp.Application.Security;

namespace IndustrialApp.Application.Tests;

public sealed class AiAssistantServiceTests
{
    [Fact]
    public async Task AskAsync_WhenUserIsAuthorized_ReturnsAnswerAndWritesAudit()
    {
        var chatService = new FakeAiChatService("Answer from AI");
        var permissionService = new FakePermissionService(canUseAi: true);
        var auditLogService = new FakeAuditLogService();
        var service = new AiAssistantService(chatService, permissionService, auditLogService);

        var response = await service.AskAsync(
            new AskAiRequest("user-1", "How is machine A?"),
            CancellationToken.None);

        Assert.Equal("Answer from AI", response.Answer);
        Assert.Equal(1, chatService.CallCount);
        Assert.Single(auditLogService.Entries);
        Assert.Equal("user-1", auditLogService.Entries[0].UserId);
        Assert.Equal("How is machine A?", auditLogService.Entries[0].Question);
        Assert.Equal("Answer from AI", auditLogService.Entries[0].Answer);
    }

    [Fact]
    public async Task AskAsync_WhenUserIsUnauthorized_ThrowsAndDoesNotCallChat()
    {
        var chatService = new FakeAiChatService("Answer from AI");
        var permissionService = new FakePermissionService(canUseAi: false);
        var auditLogService = new FakeAuditLogService();
        var service = new AiAssistantService(chatService, permissionService, auditLogService);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.AskAsync(
            new AskAiRequest("user-1", "How is machine A?"),
            CancellationToken.None));

        Assert.Equal(0, chatService.CallCount);
        Assert.Empty(auditLogService.Entries);
    }

    [Fact]
    public async Task AskAsync_WhenQuestionIsBlank_ThrowsArgumentException()
    {
        var service = new AiAssistantService(
            new FakeAiChatService("Answer from AI"),
            new FakePermissionService(canUseAi: true),
            new FakeAuditLogService());

        await Assert.ThrowsAsync<ArgumentException>(() => service.AskAsync(
            new AskAiRequest("user-1", "   "),
            CancellationToken.None));
    }

    private sealed class FakeAiChatService : IAiChatService
    {
        private readonly string _answer;

        public FakeAiChatService(string answer)
        {
            _answer = answer;
        }

        public int CallCount { get; private set; }

        public Task<string> GetAnswerAsync(string question, CancellationToken cancellationToken)
        {
            CallCount++;
            return Task.FromResult(_answer);
        }
    }

    private sealed class FakePermissionService : IPermissionService
    {
        private readonly bool _canUseAi;

        public FakePermissionService(bool canUseAi)
        {
            _canUseAi = canUseAi;
        }

        public Task<bool> CanUseAiAsync(string userId, CancellationToken cancellationToken)
        {
            return Task.FromResult(_canUseAi);
        }
    }

    private sealed class FakeAuditLogService : IAuditLogService
    {
        public List<AuditEntry> Entries { get; } = [];

        public Task WriteAsync(string userId, string question, string answer, CancellationToken cancellationToken)
        {
            Entries.Add(new AuditEntry(userId, question, answer));
            return Task.CompletedTask;
        }
    }

    private sealed record AuditEntry(string UserId, string Question, string Answer);
}
