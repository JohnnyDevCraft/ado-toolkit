namespace AdoToolkit.Models;

public sealed class AppConfig
{
    public string SchemaVersion { get; set; } = "1.0.0";

    public string? Pat { get; set; }

    public CurrentContext CurrentContext { get; set; } = new();

    public AppSettings Settings { get; set; } = new();

    public List<WorkItemEntry> WorkItems { get; set; } = [];

    public List<PullRequestEntry> PullRequests { get; set; } = [];
}

