using IndustrialApp.Application.Ai;
using IndustrialApp.Application.Audit;
using IndustrialApp.Application.Security;
using IndustrialApp.Infrastructure.Ai;
using IndustrialApp.Infrastructure.Audit;
using IndustrialApp.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace IndustrialApp.Infrastructure;

public static class AiServiceCollectionExtensions
{
    public static IServiceCollection AddAiInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var aiOptions = CreateAiOptions(configuration);
        var endpoint = new Uri(aiOptions.Endpoint);

        services.AddSingleton<IOptions<AiOptions>>(Options.Create(aiOptions));

        services.AddKernel()
            .AddOllamaChatCompletion(
                modelId: aiOptions.ModelId,
                endpoint: endpoint);

        services.AddHttpClient(OllamaHealthService.HttpClientName, client =>
        {
            client.BaseAddress = endpoint;
            client.Timeout = TimeSpan.FromSeconds(2);
        });

        services.AddScoped<IAiChatService, SemanticKernelChatService>();
        services.AddScoped<IAiHealthService, OllamaHealthService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IPermissionService, DemoPermissionService>();

        return services;
    }

    private static AiOptions CreateAiOptions(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var modelId = configuration[$"{AiOptions.SectionName}:ModelId"];
        if (string.IsNullOrWhiteSpace(modelId))
        {
            throw new InvalidOperationException("Ai:ModelId is missing.");
        }

        var endpoint = configuration[$"{AiOptions.SectionName}:Endpoint"];
        if (string.IsNullOrWhiteSpace(endpoint))
        {
            throw new InvalidOperationException("Ai:Endpoint is missing.");
        }

        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out _))
        {
            throw new InvalidOperationException("Ai:Endpoint must be a valid absolute URI.");
        }

        return new AiOptions
        {
            ModelId = modelId,
            Endpoint = endpoint
        };
    }
}
