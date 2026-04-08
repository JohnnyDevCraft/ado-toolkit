using AdoToolkit.Models;
using AdoToolkit.Models.WorkItems;
using AdoToolkit.Integrations;

namespace AdoToolkit.Services;

public sealed class WorkItemRetrievalService
{
    private readonly IAzureDevOpsWorkItemClient _client;
    private readonly WorkItemReferenceParser _referenceParser;

    public WorkItemRetrievalService(IAzureDevOpsWorkItemClient client, WorkItemReferenceParser referenceParser)
    {
        _client = client;
        _referenceParser = referenceParser;
    }

    public async Task<RetrievalResult> RetrieveAsync(AppConfig config, int workItemId, CancellationToken cancellationToken = default)
    {
        ValidateConfig(config);

        var notes = new List<string>();
        var organization = config.CurrentContext.Organization!;
        var pat = config.Pat!;

        var root = await _client.GetWorkItemAsync(organization, pat, workItemId, cancellationToken);
        root.Comments.AddRange(await _client.GetCommentsAsync(organization, root.ProjectName, pat, root.Id, cancellationToken));

        var parents = await LoadWorkItemsAsync(root.ParentIds, organization, pat, notes, cancellationToken);
        var children = await LoadWorkItemsAsync(root.ChildIds, organization, pat, notes, cancellationToken);

        var referenceSources = new[] { root }.Concat(parents).Concat(children).ToList();
        var textualReferences = referenceSources.SelectMany(_referenceParser.ParseReferences).ToList();
        var relationReferences = referenceSources
            .SelectMany(source => source.RelatedIds.Select(relatedId => new WorkItemReference
            {
                SourceWorkItemId = source.Id,
                SourceKind = "relation",
                SourceLabel = "Related Link",
                ReferencedWorkItemId = relatedId,
                ReferenceText = $"Related work item {relatedId}",
                RelationshipType = "Related"
            }))
            .ToList();

        var excludedIds = new HashSet<int>([root.Id]);
        excludedIds.UnionWith(parents.Select(parent => parent.Id));
        excludedIds.UnionWith(children.Select(child => child.Id));

        var parsedReferences = textualReferences
            .Concat(relationReferences)
            .Where(reference => !excludedIds.Contains(reference.ReferencedWorkItemId))
            .GroupBy(reference => new
            {
                reference.SourceWorkItemId,
                reference.SourceKind,
                reference.SourceLabel,
                reference.SourceCommentId,
                reference.ReferencedWorkItemId,
                reference.RelationshipType
            })
            .Select(group => group.First())
            .ToList();

        var related = new List<RelatedWorkItem>();
        foreach (var referenceGroup in parsedReferences.GroupBy(reference => reference.ReferencedWorkItemId))
        {
            try
            {
                var relatedWorkItem = await _client.GetWorkItemAsync(organization, pat, referenceGroup.Key, cancellationToken);
                relatedWorkItem.Comments.AddRange(await _client.GetCommentsAsync(organization, relatedWorkItem.ProjectName, pat, relatedWorkItem.Id, cancellationToken));
                foreach (var reference in referenceGroup)
                {
                    reference.Retrieved = true;
                }

                related.Add(new RelatedWorkItem
                {
                    WorkItem = relatedWorkItem,
                    References = referenceGroup.ToList()
                });
            }
            catch (Exception ex)
            {
                foreach (var reference in referenceGroup)
                {
                    reference.Retrieved = false;
                }

                notes.Add($"Referenced work item {referenceGroup.Key} could not be loaded: {ex.Message}");
            }
        }

        return new RetrievalResult
        {
            RootWorkItem = root,
            Parents = parents,
            Children = children,
            RelatedWorkItems = related.OrderBy(item => item.WorkItem.Id).ToList(),
            Notes = notes
        };
    }

    private async Task<List<NormalizedWorkItem>> LoadWorkItemsAsync(IEnumerable<int> ids, string organization, string pat, List<string> notes, CancellationToken cancellationToken)
    {
        var results = new List<NormalizedWorkItem>();
        foreach (var id in ids.Distinct().OrderBy(value => value))
        {
            try
            {
                var workItem = await _client.GetWorkItemAsync(organization, pat, id, cancellationToken);
                workItem.Comments.AddRange(await _client.GetCommentsAsync(organization, workItem.ProjectName, pat, workItem.Id, cancellationToken));
                results.Add(workItem);
            }
            catch (Exception ex)
            {
                notes.Add($"Work item {id} could not be loaded: {ex.Message}");
            }
        }

        return results;
    }

    private static void ValidateConfig(AppConfig config)
    {
        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(config.Pat))
        {
            missing.Add("PAT");
        }

        if (string.IsNullOrWhiteSpace(config.CurrentContext.Organization))
        {
            missing.Add("organization");
        }

        if (config.CurrentContext.Project is null ||
            string.IsNullOrWhiteSpace(config.CurrentContext.Project.Id) ||
            string.IsNullOrWhiteSpace(config.CurrentContext.Project.Name))
        {
            missing.Add("default project");
        }

        if (string.IsNullOrWhiteSpace(config.Settings.OutputRootPath))
        {
            missing.Add("output path");
        }

        if (missing.Count > 0)
        {
            throw new InvalidOperationException($"Missing required configuration: {string.Join(", ", missing)}.");
        }
    }
}

