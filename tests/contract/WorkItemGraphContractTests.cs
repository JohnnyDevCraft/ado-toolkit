using System.Text.Json;
using AdoToolkit.Models.WorkItems;
using AdoToolkit.Services;

namespace AdoToolkit.Tests.Contract;

public sealed class WorkItemGraphContractTests
{
    [Fact]
    public void Retrieval_result_json_contains_required_sections()
    {
        var writer = new WorkItemArtifactWriter();
        var result = new RetrievalResult
        {
            RootWorkItem = new NormalizedWorkItem
            {
                Id = 123,
                ProjectId = "proj-1",
                ProjectName = "Project One",
                Organization = "org-one",
                Title = "Root Work Item",
                WorkItemType = "Feature",
                State = "Active"
            }
        };

        var json = writer.BuildJson(result);
        using var document = JsonDocument.Parse(json);

        Assert.True(document.RootElement.TryGetProperty("RootWorkItem", out _)
            || document.RootElement.TryGetProperty("rootWorkItem", out _));
        Assert.True(document.RootElement.TryGetProperty("Parents", out _)
            || document.RootElement.TryGetProperty("parents", out _));
        Assert.True(document.RootElement.TryGetProperty("Children", out _)
            || document.RootElement.TryGetProperty("children", out _));
        Assert.True(document.RootElement.TryGetProperty("RelatedWorkItems", out _)
            || document.RootElement.TryGetProperty("relatedWorkItems", out _));
    }
}

