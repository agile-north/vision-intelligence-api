namespace Implementations.GoogleVertexAI;

public class GoogleVertexAiIntelligenceConfiguration
{
    public bool Enabled { get; set; } = false;
    public string ProjectId { get; set; } = null!;
    public string AccessKey { get; set; } = null!;
    public Uri BaseAddress { get; set; } = null!;
}