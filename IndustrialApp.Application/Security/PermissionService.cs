namespace IndustrialApp.Application.Security;

public class PermissionService: IPermissionService
{
    public Task<bool> CanUseAiAsync(string userId, CancellationToken cancellationToken)
    {
        return Task.FromResult(!string.IsNullOrWhiteSpace(userId));
    }
}