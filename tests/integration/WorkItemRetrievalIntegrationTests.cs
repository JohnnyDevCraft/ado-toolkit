using AdoToolkit.Commands;
using AdoToolkit.Config;
using AdoToolkit.Integrations;
using AdoToolkit.Models;
using AdoToolkit.Models.WorkItems;
using AdoToolkit.Services;

namespace AdoToolkit.Tests.Integration;

public sealed class WorkItemRetrievalIntegrationTests
{
    [Fact]
    public async Task Work_item_get_creates_artifacts_and_updates_index()
    {
        var home = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        var appPaths = new AppPaths(home);
        var configService = new AppConfigService(appPaths, new JsonFileStore(), new SchemaValidationService());
        var config = await configService.LoadOrCreateAsync();
        config.Pat = "pat";
        config.CurrentContext.Organization = "org-one";
        config.CurrentContext.Project = new ProjectRef { Id = "proj-1", Name = "Project One" };
        await configService.SaveAsync(config);

        var retrievalService = new WorkItemRetrievalService(new FakeWorkItemClient(), new WorkItemReferenceParser());
        var command = new WorkItemCommands(retrievalService, new WorkItemArtifactWriter(), new JsonFileStore(), new WorkItemIndexService(configService));

        var output = await command.GetAsync(config, 1001);

        Assert.True(File.Exists(output.JsonPath));
        Assert.True(File.Exists(output.MarkdownPath));

        var reloaded = await configService.LoadOrCreateAsync();
        var indexed = Assert.Single(reloaded.WorkItems);
        Assert.Equal(1001, indexed.Id);
        Assert.Equal("org-one", indexed.Organization);
        Assert.Equal("Project One", indexed.Project.Name);
        Assert.Equal(2, indexed.Artifacts.Count);
    }

    private sealed class FakeWorkItemClient : IAzureDevOpsWorkItemClient
    {
        public Task<NormalizedWorkItem> GetWorkItemAsync(string organization, string pat, int workItemId, CancellationToken cancellationToken = default)
        {
            var item = workItemId switch
            {
                1001 => new NormalizedWorkItem
                {
                    Id = 1001,
                    Organization = organization,
                    ProjectId = "proj-1",
                    ProjectName = "Project One",
                    Title = "Main work item",
                    WorkItemType = "Feature",
                    State = "Active",
                    ParentIds = [2001],
                    ChildIds = [3001],
                    RelatedIds = [4001],
                    DescriptionFields =
                    [
                        new DescriptionField
                        {
                            ReferenceName = "System.Description",
                            DisplayName = "Description",
                            Value = "References #4001"
                        }
                    ]
                },
                2001 => new NormalizedWorkItem
                {
                    Id = 2001,
                    Organization = organization,
                    ProjectId = "proj-1",
                    ProjectName = "Project One",
                    Title = "Parent",
                    WorkItemType = "Epic",
                    State = "Active"
                },
                3001 => new NormalizedWorkItem
                {
                    Id = 3001,
                    Organization = organization,
                    ProjectId = "proj-1",
                    ProjectName = "Project One",
                    Title = "Child",
                    WorkItemType = "Task",
                    State = "New"
                },
                4001 => new NormalizedWorkItem
                {
                    Id = 4001,
                    Organization = organization,
                    ProjectId = "proj-1",
                    ProjectName = "Project One",
                    Title = "Related",
                    WorkItemType = "Bug",
                    State = "Active"
                },
                _ => throw new InvalidOperationException("Unexpected work item ID")
            };

            return Task.FromResult(item);
        }

        public Task<IReadOnlyList<WorkItemComment>> GetCommentsAsync(string organization, string projectName, string pat, int workItemId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<WorkItemComment> comments =
            [
                new WorkItemComment
                {
                    CommentId = $"{workItemId}-c1",
                    AuthoredBy = "Tester",
                    RenderedText = workItemId == 1001 ? "Linked to #4001" : "No extra links"
                }
            ];
            return Task.FromResult(comments);
        }
    }
}
