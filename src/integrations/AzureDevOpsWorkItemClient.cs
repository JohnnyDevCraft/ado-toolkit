using System.Net;
using System.Text;
using System.Text.Json;
using AdoToolkit.Models.WorkItems;
using AdoToolkit.Services;

namespace AdoToolkit.Integrations;

public sealed class AzureDevOpsWorkItemClient : IAzureDevOpsWorkItemClient
{
    private static readonly HashSet<string> ExcludedDescriptionFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "System.Title",
        "System.State",
        "System.Tags",
        "System.AreaPath",
        "System.IterationPath",
        "System.WorkItemType",
        "System.AssignedTo",
        "System.TeamProject",
        "System.ProjectId",
        "System.CreatedBy",
        "System.ChangedBy",
        "System.Reason",
        "System.History"
    };

    private static readonly string[] DescriptionFieldPriority =
    [
        "System.Description",
        "Microsoft.VSTS.Common.AcceptanceCriteria",
        "Microsoft.VSTS.TCM.ReproSteps",
        "Custom.Description",
        "Custom.AcceptanceCriteria",
        "Custom.ReproSteps",
        "Custom.ProblemStatement",
        "Custom.StepsToReproduce",
        "Custom.Background",
        "Custom.Notes"
    ];

    private readonly AzureDevOpsHttpClientFactory _clientFactory;

    public AzureDevOpsWorkItemClient(AzureDevOpsHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<NormalizedWorkItem> GetWorkItemAsync(string organization, string pat, int workItemId, CancellationToken cancellationToken = default)
    {
        using var client = _clientFactory.Create(pat);
        using var response = await client.GetAsync($"https://dev.azure.com/{Uri.EscapeDataString(organization)}/_apis/wit/workitems/{workItemId}?$expand=relations&api-version=7.1", cancellationToken);
        await EnsureSuccessAsync(response, $"Unable to load work item {workItemId}.");

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        return ParseWorkItem(document.RootElement, organization);
    }

    public async Task<IReadOnlyList<WorkItemComment>> GetCommentsAsync(string organization, string projectName, string pat, int workItemId, CancellationToken cancellationToken = default)
    {
        using var client = _clientFactory.Create(pat);
        using var response = await client.GetAsync($"https://dev.azure.com/{Uri.EscapeDataString(organization)}/{Uri.EscapeDataString(projectName)}/_apis/wit/workItems/{workItemId}/comments?api-version=7.1-preview.4", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return [];
        }

        await EnsureSuccessAsync(response, $"Unable to load comments for work item {workItemId}.");
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty("comments", out var commentsElement) || commentsElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var comments = new List<WorkItemComment>();
        foreach (var commentElement in commentsElement.EnumerateArray())
        {
            comments.Add(new WorkItemComment
            {
                CommentId = commentElement.TryGetProperty("id", out var idElement) ? idElement.ToString() : string.Empty,
                AuthoredBy = TryReadIdentity(commentElement, "createdBy"),
                AuthoredAt = commentElement.TryGetProperty("createdDate", out var createdDate) &&
                             createdDate.ValueKind == JsonValueKind.String &&
                             DateTimeOffset.TryParse(createdDate.GetString(), out var parsedDate)
                    ? parsedDate
                    : null,
                RenderedText = ExtractCommentText(commentElement)
            });
        }

        return comments;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string message)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException($"{message} HTTP {(int)response.StatusCode}: {body}");
    }

    private static NormalizedWorkItem ParseWorkItem(JsonElement root, string organization)
    {
        var fields = root.GetProperty("fields");
        var projectName = GetFieldString(fields, "System.TeamProject") ?? string.Empty;
        var projectId = GetFieldString(fields, "System.ProjectId") ?? string.Empty;

        return new NormalizedWorkItem
        {
            Id = root.GetProperty("id").GetInt32(),
            ProjectId = projectId,
            ProjectName = projectName,
            Organization = organization,
            Title = GetFieldString(fields, "System.Title") ?? $"Work Item {root.GetProperty("id").GetInt32()}",
            WorkItemType = GetFieldString(fields, "System.WorkItemType") ?? string.Empty,
            State = GetFieldString(fields, "System.State") ?? string.Empty,
            AssignedTo = ReadAssignedTo(fields),
            Tags = GetFieldString(fields, "System.Tags"),
            AreaPath = GetFieldString(fields, "System.AreaPath"),
            IterationPath = GetFieldString(fields, "System.IterationPath"),
            Url = root.TryGetProperty("url", out var urlElement) ? urlElement.GetString() : null,
            DescriptionFields = ExtractDescriptionFields(fields),
            ParentIds = ExtractRelationIds(root, "Parent"),
            ChildIds = ExtractRelationIds(root, "Child"),
            RelatedIds = ExtractRelationIds(root, "Related")
        };
    }

    private static List<DescriptionField> ExtractDescriptionFields(JsonElement fields)
    {
        var descriptions = new List<DescriptionField>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var priorityField in DescriptionFieldPriority)
        {
            if (TryGetStringField(fields, priorityField, out var value) && ShouldCaptureDescriptionField(priorityField, value))
            {
                descriptions.Add(new DescriptionField
                {
                    ReferenceName = priorityField,
                    DisplayName = DisplayNameForField(priorityField),
                    Value = HtmlMarkdownConverter.ConvertDescriptionField(value!)
                });
                seen.Add(priorityField);
            }
        }

        foreach (var property in fields.EnumerateObject())
        {
            if (seen.Contains(property.Name) || property.Value.ValueKind != JsonValueKind.String)
            {
                continue;
            }

            var value = property.Value.GetString();
            if (!ShouldCaptureDescriptionField(property.Name, value))
            {
                continue;
            }

            descriptions.Add(new DescriptionField
            {
                ReferenceName = property.Name,
                DisplayName = DisplayNameForField(property.Name),
                Value = HtmlMarkdownConverter.ConvertDescriptionField(value!)
            });
        }

        return descriptions;
    }

    private static bool ShouldCaptureDescriptionField(string fieldName, string? value)
    {
        if (ExcludedDescriptionFields.Contains(fieldName) || string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return LooksLikeDescriptionField(fieldName) ||
               HtmlMarkdownConverter.LooksLikeHtml(value) ||
               value.Contains('\n') ||
               value.Length >= 120;
    }

    private static bool LooksLikeDescriptionField(string fieldName)
    {
        return fieldName.Contains("description", StringComparison.OrdinalIgnoreCase)
               || fieldName.Contains("acceptancecriteria", StringComparison.OrdinalIgnoreCase)
               || fieldName.Contains("reprosteps", StringComparison.OrdinalIgnoreCase)
               || fieldName.Contains("problem", StringComparison.OrdinalIgnoreCase)
               || fieldName.Contains("background", StringComparison.OrdinalIgnoreCase)
               || fieldName.Contains("summary", StringComparison.OrdinalIgnoreCase)
               || fieldName.Contains("notes", StringComparison.OrdinalIgnoreCase)
               || fieldName.Contains("details", StringComparison.OrdinalIgnoreCase)
               || fieldName.Contains("steps", StringComparison.OrdinalIgnoreCase)
               || fieldName.Contains("criteria", StringComparison.OrdinalIgnoreCase);
    }

    private static List<int> ExtractRelationIds(JsonElement root, string relationName)
    {
        var ids = new List<int>();
        if (!root.TryGetProperty("relations", out var relations) || relations.ValueKind != JsonValueKind.Array)
        {
            return ids;
        }

        foreach (var relation in relations.EnumerateArray())
        {
            if (!relation.TryGetProperty("rel", out var relElement))
            {
                continue;
            }

            var rel = relElement.GetString() ?? string.Empty;
            if (!MatchesRelation(rel, relationName) || !relation.TryGetProperty("url", out var urlElement))
            {
                continue;
            }

            var relationUrl = urlElement.GetString();
            var lastSegment = relationUrl?.Split('/').LastOrDefault();
            if (int.TryParse(lastSegment, out var id))
            {
                ids.Add(id);
            }
        }

        return ids;
    }

    private static bool MatchesRelation(string relationValue, string relationName)
    {
        return relationName switch
        {
            "Parent" => relationValue.Contains("Hierarchy-Reverse", StringComparison.OrdinalIgnoreCase)
                        || relationValue.Contains("Parent", StringComparison.OrdinalIgnoreCase),
            "Child" => relationValue.Contains("Hierarchy-Forward", StringComparison.OrdinalIgnoreCase)
                       || relationValue.Contains("Child", StringComparison.OrdinalIgnoreCase),
            "Related" => relationValue.Contains("Related", StringComparison.OrdinalIgnoreCase),
            _ => relationValue.Contains(relationName, StringComparison.OrdinalIgnoreCase)
        };
    }

    private static string? GetFieldString(JsonElement fields, string fieldName)
    {
        return TryGetStringField(fields, fieldName, out var value) ? value : null;
    }

    private static bool TryGetStringField(JsonElement fields, string fieldName, out string? value)
    {
        value = null;
        if (!fields.TryGetProperty(fieldName, out var field))
        {
            return false;
        }

        value = field.ValueKind switch
        {
            JsonValueKind.String => field.GetString(),
            JsonValueKind.Number => field.ToString(),
            _ => null
        };

        return value is not null;
    }

    private static string? ReadAssignedTo(JsonElement fields)
    {
        if (!fields.TryGetProperty("System.AssignedTo", out var assignedTo))
        {
            return null;
        }

        return assignedTo.ValueKind switch
        {
            JsonValueKind.String => assignedTo.GetString(),
            JsonValueKind.Object => TryReadIdentity(assignedTo),
            _ => null
        };
    }

    private static string? TryReadIdentity(JsonElement parent, string propertyName)
    {
        return parent.TryGetProperty(propertyName, out var property) ? TryReadIdentity(property) : null;
    }

    private static string? TryReadIdentity(JsonElement identityObject)
    {
        if (identityObject.ValueKind == JsonValueKind.String)
        {
            return identityObject.GetString();
        }

        if (identityObject.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (identityObject.TryGetProperty("displayName", out var displayName))
        {
            return displayName.GetString();
        }

        return identityObject.TryGetProperty("name", out var name) ? name.GetString() : null;
    }

    private static string DisplayNameForField(string referenceName)
    {
        var lastSegment = referenceName.Split('.').Last();
        var builder = new StringBuilder();

        for (var index = 0; index < lastSegment.Length; index++)
        {
            var current = lastSegment[index];
            if (index > 0 && char.IsUpper(current) && !char.IsUpper(lastSegment[index - 1]))
            {
                builder.Append(' ');
            }

            builder.Append(current);
        }

        return builder.ToString();
    }

    private static string ExtractCommentText(JsonElement commentElement)
    {
        if (commentElement.TryGetProperty("renderedText", out var renderedText) && renderedText.ValueKind == JsonValueKind.String)
        {
            return renderedText.GetString() ?? string.Empty;
        }

        if (commentElement.TryGetProperty("text", out var text) && text.ValueKind == JsonValueKind.String)
        {
            return text.GetString() ?? string.Empty;
        }

        return string.Empty;
    }
}

