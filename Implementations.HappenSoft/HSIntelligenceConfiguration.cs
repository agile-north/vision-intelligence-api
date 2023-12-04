namespace Implementations.HappenSoft;
public class HSIntelligenceConfiguration
{
    public bool Enabled { get; set; } = true;
    public Uri BaseAddress { get; set; } = null!;
    public string? ApiKey { get; set; }
}