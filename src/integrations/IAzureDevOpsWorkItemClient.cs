using AdoToolkit.Models.WorkItems;

namespace AdoToolkit.Integrations;

public interface IAzureDevOpsWorkItemClient
{
    Task<NormalizedWorkItem> GetWorkItemAsync(string organization, string pat, int workItemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkItemComment>> GetCommentsAsync(string organization, string projectName, string pat, int workItemId, CancellationToken cancellationToken = default);
}

