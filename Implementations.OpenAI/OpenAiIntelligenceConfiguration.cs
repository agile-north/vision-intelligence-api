namespace Implementations.OpenAI;

public class OpenAiIntelligenceConfiguration
{
    public Uri BaseAddress { get; set; } = null!;
    public int MaxTokens { get; set; }
    public string? ApiKey { get; set; }
}