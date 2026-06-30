using IndustrialApp.Application.Audit;
using Microsoft.Extensions.Logging;

namespace IndustrialApp.Infrastructure.Audit;

public partial class AuditLogService : IAuditLogService
{
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(ILogger<AuditLogService> logger)
    {
        _logger = logger;
    }

    public Task WriteAsync(
        string userId,
        string question,
        string answer,
        CancellationToken cancellationToken)
    {
        LogAiAudit(userId, question, answer);

        return Task.CompletedTask;
    }

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "AI audit. UserId: {UserId}, Question: {Question}, Answer: {Answer}")]
    private partial void LogAiAudit(
        string userId,
        string question,
        string answer);
}