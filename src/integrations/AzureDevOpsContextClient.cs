using System.Text.Json;
using AdoToolkit.Models;

namespace AdoToolkit.Integrations;

public sealed class AzureDevOpsContextClient : IAzureDevOpsContextClient
{
    private readonly AzureDevOpsHttpClientFactory _clientFactory;
    private readonly AzureDevOpsAuthService _authService;

    public AzureDevOpsContextClient(AzureDevOpsHttpClientFactory clientFactory, AzureDevOpsAuthService authService)
    {
        _clientFactory = clientFactory;
        _authService = authService;
    }

    public async Task<IReadOnlyList<AdoOrganizationInfo>> ListOrganizationsAsync(string pat, CancellationToken cancellationToken = default)
    {
        _authService.EnsurePatConfigured(pat);

        using var client = _clientFactory.Create(pat);
        using var profileResponse = await client.GetAsync("https://app.vssps.visualstudio.com/_apis/profile/profiles/me?api-version=7.1", cancellationToken);
        await EnsureSuccessAsync(profileResponse, "Unable to resolve the authenticated Azure DevOps profile.");

        await using var profileStream = await profileResponse.Content.ReadAsStreamAsync(cancellationToken);
        using var profileDocument = await JsonDocument.ParseAsync(profileStream, cancellationToken: cancellationToken);

        var memberId = profileDocument.RootElement.TryGetProperty("id", out var idElement)
            ? idElement.GetString()
            : null;

        if (string.IsNullOrWhiteSpace(memberId))
        {
            return [];
        }

        using var accountsResponse = await client.GetAsync($"https://app.vssps.visualstudio.com/_apis/accounts?memberId={Uri.EscapeDataString(memberId)}&api-version=7.1", cancellationToken);
        await EnsureSuccessAsync(accountsResponse, "Unable to list Azure DevOps organizations for the current user.");

        await using var accountsStream = await accountsResponse.Content.ReadAsStreamAsync(cancellationToken);
        using var accountsDocument = await JsonDocument.ParseAsync(accountsStream, cancellationToken: cancellationToken);

        if (!accountsDocument.RootElement.TryGetProperty("value", out var valueElement) || valueElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return valueElement
            .EnumerateArray()
            .Select(account => new AdoOrganizationInfo
            {
                Id = account.TryGetProperty("accountId", out var accountId) ? accountId.GetString() : null,
                Name = account.TryGetProperty("accountName", out var accountName) ? accountName.GetString() ?? string.Empty : string.Empty
            })
            .Where(organization => !string.IsNullOrWhiteSpace(organization.Name))
            .OrderBy(organization => organization.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<IReadOnlyList<ProjectRef>> ListProjectsAsync(string organization, string pat, CancellationToken cancellationToken = default)
    {
        _authService.EnsurePatConfigured(pat);
        if (string.IsNullOrWhiteSpace(organization))
        {
            throw new InvalidOperationException("An Azure DevOps organization is required to list projects.");
        }

        using var client = _clientFactory.Create(pat);
        using var response = await client.GetAsync($"https://dev.azure.com/{Uri.EscapeDataString(organization)}/_apis/projects?api-version=7.1", cancellationToken);
        await EnsureSuccessAsync(response, $"Unable to list Azure DevOps projects for organization '{organization}'.");

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        if (!document.RootElement.TryGetProperty("value", out var valueElement) || valueElement.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return valueElement
            .EnumerateArray()
            .Select(project => new ProjectRef
            {
                Id = project.TryGetProperty("id", out var idElement) ? idElement.GetString() : null,
                Name = project.TryGetProperty("name", out var nameElement) ? nameElement.GetString() : null
            })
            .Where(project => !string.IsNullOrWhiteSpace(project.Name))
            .OrderBy(project => project.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
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
}

