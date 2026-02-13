namespace AuthorizationDemo.Setup;

public class ObservabilityConfig
{
    public const string TAG_NAME = "Observability";
    public bool Enabled { get; set; } = false;
    public string ExporterUrl { get; set; } = "";
}