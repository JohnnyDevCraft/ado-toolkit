using AdoToolkit.Config;
using AdoToolkit.Models;
using AdoToolkit.Services;

namespace AdoToolkit.Commands;

public sealed class WorkItemCommands
{
    private readonly WorkItemRetrievalService _retrievalService;
    private readonly WorkItemArtifactWriter _artifactWriter;
    private readonly JsonFileStore _store;
    private readonly WorkItemIndexService _indexService;

    public WorkItemCommands(
        WorkItemRetrievalService retrievalService,
        WorkItemArtifactWriter artifactWriter,
        JsonFileStore store,
        WorkItemIndexService indexService)
    {
        _retrievalService = retrievalService;
        _artifactWriter = artifactWriter;
        _store = store;
        _indexService = indexService;
    }

    public async Task<(string JsonPath, string MarkdownPath)> GetAsync(AppConfig config, int workItemId, string? outputOverride = null, CancellationToken cancellationToken = default)
    {
        var result = await _retrievalService.RetrieveAsync(config, workItemId, cancellationToken);
        var outputRoot = string.IsNullOrWhiteSpace(outputOverride) ? config.Settings.OutputRootPath : outputOverride!;
        Directory.CreateDirectory(outputRoot);

        var baseName = $"{AppPaths.Slugify(result.RootWorkItem.ProjectName, "project")}-{workItemId}-{AppPaths.Slugify(result.RootWorkItem.Title, "work-item")}";
        var jsonPath = Path.Combine(outputRoot, $"{baseName}.json");
        var markdownPath = Path.Combine(outputRoot, $"{baseName}.md");

        await File.WriteAllTextAsync(jsonPath, _artifactWriter.BuildJson(result), cancellationToken);
        await File.WriteAllTextAsync(markdownPath, _artifactWriter.BuildMarkdown(result), cancellationToken);
        await _indexService.IndexAsync(config, result, jsonPath, markdownPath, cancellationToken);

        return (jsonPath, markdownPath);
    }
}
