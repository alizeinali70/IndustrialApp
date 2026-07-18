namespace IndustrialApp.Application.Ai;

public interface IAiHealthService
{
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken);
}
