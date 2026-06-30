namespace IndustrialApp.Application.Audit;

public interface IAuditLogService
{
    Task WriteAsync(string userId, string question, string answer, CancellationToken cancellationToken);

}