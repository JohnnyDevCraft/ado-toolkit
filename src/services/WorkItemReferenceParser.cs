using System.Text.RegularExpressions;
using AdoToolkit.Models.WorkItems;

namespace AdoToolkit.Services;

public sealed class WorkItemReferenceParser
{
    private static readonly Regex[] ReferencePatterns =
    [
        new(@"(?<!\w)#(?<id>\d{2,9})(?!\w)", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"\bwork\s*item\s*#?(?<id>\d{2,9})\b", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"\bwi\s*#?(?<id>\d{2,9})\b", RegexOptions.Compiled | RegexOptions.IgnoreCase),
        new(@"\bbug\s*#(?<id>\d{2,9})\b", RegexOptions.Compiled | RegexOptions.IgnoreCase)
    ];

    public IReadOnlyList<WorkItemReference> ParseReferences(NormalizedWorkItem sourceWorkItem)
    {
        var references = new List<WorkItemReference>();

        foreach (var descriptionField in sourceWorkItem.DescriptionFields)
        {
            references.AddRange(ParseText(sourceWorkItem.Id, "description", descriptionField.DisplayName, descriptionField.ReferenceName, descriptionField.Value));
        }

        foreach (var comment in sourceWorkItem.Comments)
        {
            references.AddRange(ParseText(sourceWorkItem.Id, "comment", $"Comment {comment.CommentId}", comment.CommentId, comment.RenderedText));
        }

        return references
            .GroupBy(reference => new
            {
                reference.SourceWorkItemId,
                reference.SourceKind,
                reference.SourceLabel,
                reference.SourceCommentId,
                reference.ReferencedWorkItemId
            })
            .Select(group => group.First())
            .ToList();
    }

    private static IEnumerable<WorkItemReference> ParseText(int sourceWorkItemId, string sourceKind, string sourceLabel, string? sourceCommentId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            yield break;
        }

        var seenIds = new HashSet<int>();

        foreach (var pattern in ReferencePatterns)
        {
            foreach (Match match in pattern.Matches(text))
            {
                if (!int.TryParse(match.Groups["id"].Value, out var referencedId) || !seenIds.Add(referencedId))
                {
                    continue;
                }

                yield return new WorkItemReference
                {
                    SourceWorkItemId = sourceWorkItemId,
                    SourceKind = sourceKind,
                    SourceLabel = sourceLabel,
                    SourceCommentId = sourceKind == "comment" ? sourceCommentId : null,
                    ReferencedWorkItemId = referencedId,
                    ReferenceText = match.Value
                };
            }
        }
    }
}

