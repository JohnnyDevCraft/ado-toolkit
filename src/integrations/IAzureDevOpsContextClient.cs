using AdoToolkit.Models;

namespace AdoToolkit.Integrations;

public interface IAzureDevOpsContextClient
{
    Task<IReadOnlyList<AdoOrganizationInfo>> ListOrganizationsAsync(string pat, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProjectRef>> ListProjectsAsync(string organization, string pat, CancellationToken cancellationToken = default);
}

