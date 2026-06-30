namespace IndustrialApp.Application.Security;

public interface IPermissionService
{
    Task<bool> CanUseAiAsync(string userId, CancellationToken cancellationToken);
}