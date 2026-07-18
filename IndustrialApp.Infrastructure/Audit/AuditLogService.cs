using IndustrialApp.Application.Audit;
using Microsoft.Extensions.Logging;

namespace IndustrialApp.Infrastructure.Audit;

public partial class AuditLogService : IAuditLogService
{
    private const int PreviewLimit = 120;

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
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(question);
        ArgumentNullException.ThrowIfNull(answer);

        if (!_logger.IsEnabled(LogLevel.Information))
        {
            return Task.CompletedTask;
        }

        var questionPreview = CreatePreview(question);
        var answerPreview = CreatePreview(answer);

        LogAiAudit(
            userId,
            questionPreview,
            question.Length,
            answerPreview,
            answer.Length);

        return Task.CompletedTask;
    }

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "AI audit. UserId: {UserId}, QuestionPreview: {QuestionPreview}, QuestionLength: {QuestionLength}, AnswerPreview: {AnswerPreview}, AnswerLength: {AnswerLength}")]
    private partial void LogAiAudit(
        string userId,
        string questionPreview,
        int questionLength,
        string answerPreview,
        int answerLength);

    private static string CreatePreview(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (value.Length <= PreviewLimit)
        {
            return value;
        }

        return $"{value[..PreviewLimit]}...";
    }
}