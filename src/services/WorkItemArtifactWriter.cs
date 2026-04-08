using System.Text;
using System.Text.Json;
using AdoToolkit.Models.WorkItems;

namespace AdoToolkit.Services;

public sealed class WorkItemArtifactWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public string BuildMarkdown(RetrievalResult result)
    {
        var builder = new StringBuilder();
        var root = result.RootWorkItem;

        builder.AppendLine($"# {root.Title}");
        builder.AppendLine();
        builder.AppendLine($"- Work Item ID: `{root.Id}`");
        builder.AppendLine($"- Type: `{root.WorkItemType}`");
        builder.AppendLine($"- State: `{root.State}`");
        builder.AppendLine($"- Organization: `{root.Organization}`");
        builder.AppendLine($"- Project: `{root.ProjectName}` (`{root.ProjectId}`)");
        if (!string.IsNullOrWhiteSpace(root.Url))
        {
            builder.AppendLine($"- Azure DevOps URL: {root.Url}");
        }

        builder.AppendLine();
        builder.AppendLine("## Root Work Item");
        builder.AppendLine();
        AppendWorkItemSection(builder, root, false, null);

        builder.AppendLine("## Parent Work Items");
        builder.AppendLine();
        if (result.Parents.Count == 0)
        {
            builder.AppendLine("_No parent work items found._");
            builder.AppendLine();
        }
        else
        {
            foreach (var parent in result.Parents.OrderBy(item => item.Id))
            {
                AppendWorkItemSection(builder, parent, false, null);
            }
        }

        builder.AppendLine("## Child Work Items");
        builder.AppendLine();
        if (result.Children.Count == 0)
        {
            builder.AppendLine("_No child work items found._");
            builder.AppendLine();
        }
        else
        {
            foreach (var child in result.Children.OrderBy(item => item.Id))
            {
                AppendWorkItemSection(builder, child, false, null);
            }
        }

        builder.AppendLine("## Related Work Items");
        builder.AppendLine();
        if (result.RelatedWorkItems.Count == 0)
        {
            builder.AppendLine("_No related work items discovered from description or comment references._");
            builder.AppendLine();
        }
        else
        {
            foreach (var related in result.RelatedWorkItems.OrderBy(item => item.WorkItem.Id))
            {
                AppendWorkItemSection(builder, related.WorkItem, true, related.References);
            }
        }

        builder.AppendLine("## Notes");
        builder.AppendLine();
        if (result.Notes.Count == 0)
        {
            builder.AppendLine("_No retrieval warnings were recorded._");
        }
        else
        {
            foreach (var note in result.Notes)
            {
                builder.AppendLine($"- {note}");
            }
        }

        return builder.ToString();
    }

    public string BuildJson(RetrievalResult result)
    {
        return JsonSerializer.Serialize(result, JsonOptions);
    }

    private static void AppendWorkItemSection(StringBuilder builder, NormalizedWorkItem workItem, bool includeReferenceSources, IReadOnlyCollection<WorkItemReference>? references)
    {
        builder.AppendLine($"### {workItem.Title} (`{workItem.Id}`)");
        builder.AppendLine();
        builder.AppendLine($"- Type: `{workItem.WorkItemType}`");
        builder.AppendLine($"- State: `{workItem.State}`");
        builder.AppendLine($"- Assigned To: {ValueOrFallback(workItem.AssignedTo)}");
        builder.AppendLine($"- Tags: {ValueOrFallback(workItem.Tags)}");
        builder.AppendLine($"- Area Path: {ValueOrFallback(workItem.AreaPath)}");
        builder.AppendLine($"- Iteration Path: {ValueOrFallback(workItem.IterationPath)}");
        if (!string.IsNullOrWhiteSpace(workItem.Url))
        {
            builder.AppendLine($"- Azure DevOps URL: {workItem.Url}");
        }

        builder.AppendLine();
        builder.AppendLine("#### Description Fields");
        builder.AppendLine();
        if (workItem.DescriptionFields.Count == 0)
        {
            builder.AppendLine("_No description fields were available._");
            builder.AppendLine();
        }
        else
        {
            foreach (var descriptionField in workItem.DescriptionFields)
            {
                builder.AppendLine($"##### {descriptionField.DisplayName} (`{descriptionField.ReferenceName}`)");
                builder.AppendLine();
                builder.AppendLine(descriptionField.Value);
                builder.AppendLine();
            }
        }

        builder.AppendLine("#### Comments");
        builder.AppendLine();
        if (workItem.Comments.Count == 0)
        {
            builder.AppendLine("_No comments were retrieved._");
            builder.AppendLine();
        }
        else
        {
            foreach (var comment in workItem.Comments)
            {
                builder.AppendLine($"- Comment `{comment.CommentId}` by {ValueOrFallback(comment.AuthoredBy)} on {comment.AuthoredAt?.ToString("u") ?? "unknown date"}");
                builder.AppendLine();
                builder.AppendLine(comment.RenderedText);
                builder.AppendLine();
            }
        }

        if (includeReferenceSources && references is not null)
        {
            builder.AppendLine("#### Reference Sources");
            builder.AppendLine();
            foreach (var reference in references)
            {
                var sourceKindLabel = reference.SourceKind == "relation" ? "relation" : reference.SourceKind;
                var relationshipDetails = string.IsNullOrWhiteSpace(reference.RelationshipType) ? string.Empty : $" ({reference.RelationshipType})";
                builder.AppendLine($"- Found in work item `{reference.SourceWorkItemId}` {sourceKindLabel}{relationshipDetails} `{reference.SourceLabel}` via `{reference.ReferenceText}`");
            }

            builder.AppendLine();
        }
    }

    private static string ValueOrFallback(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "_Not set_" : value;
    }
}

