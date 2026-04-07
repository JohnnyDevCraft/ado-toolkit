namespace AdoToolkit.Models;

public sealed class AppSettings
{
    public string StorageRootPath { get; set; } = string.Empty;

    public string OutputRootPath { get; set; } = string.Empty;

    public string PromptRootPath { get; set; } = string.Empty;

    public string HeaderStyle { get; set; } = "default";

    public ArtifactPointer? LastViewedArtifact { get; set; }
}

