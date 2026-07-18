using IndustrialApp.Application.Security;

namespace IndustrialApp.Infrastructure.Security;

public sealed class DemoPermissionService : IPermissionService
{
    public Task<bool> CanUseAiAsync(string userId, CancellationToken cancellationToken)
    {
        return Task.FromResult(!string.IsNullOrWhiteSpace(userId));
    }
}
