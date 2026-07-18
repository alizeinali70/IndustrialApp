namespace IndustrialApp.Application.Ai;

public sealed class AiOptions
{
    public const string SectionName = "Ai";

    public string ModelId { get; init; } = string.Empty;

    public string Endpoint { get; init; } = string.Empty;
}
