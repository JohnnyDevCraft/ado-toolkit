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
        var memberId = await GetAuthenticatedProfileIdAsync(client, cancellationToken);

        if (string.IsNullOrWhiteSpace(memberId))
        {
            return [];
        }

        return await GetOrganizationsAsync(client, memberId, cancellationToken);
    }

    public async Task<AuthenticationCheckResult> ValidatePatAsync(string pat, CancellationToken cancellationToken = default)
    {
        _authService.EnsurePatConfigured(pat);

        using var client = _clientFactory.Create(pat);
        var checks = new List<PatCapabilityCheck>();

        string? memberId;
        try
        {
            memberId = await GetAuthenticatedProfileIdAsync(client, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return Failure(
                AuthenticationFailureCategory.ConnectivityFailure,
                "PAT test failed because Azure DevOps could not be reached.",
                "Check your network connection and Azure DevOps availability, then try setup again.",
                checks);
        }
        catch (TaskCanceledException)
        {
            return Failure(
                AuthenticationFailureCategory.ConnectivityFailure,
                "PAT test timed out while contacting Azure DevOps.",
                "Check your network connection and try setup again.",
                checks);
        }
        catch (InvalidOperationException ex) when (TryMapFailure(ex, "authenticated profile lookup", checks, out var result))
        {
            return result;
        }

        checks.Add(new PatCapabilityCheck
        {
            Name = "Authenticated profile lookup",
            Operation = "Resolve the authenticated Azure DevOps profile",
            Passed = true
        });

        if (string.IsNullOrWhiteSpace(memberId))
        {
            return Failure(
                AuthenticationFailureCategory.ServiceFailure,
                "PAT test failed because Azure DevOps did not return a usable profile.",
                "Try setup again. If the issue persists, Azure DevOps may be returning an unexpected response.",
                checks);
        }

        IReadOnlyList<AdoOrganizationInfo> organizations;
        try
        {
            organizations = await GetOrganizationsAsync(client, memberId, cancellationToken);
        }
        catch (HttpRequestException)
        {
            checks.Add(new PatCapabilityCheck
            {
                Name = "Organization discovery",
                Operation = "List accessible Azure DevOps organizations",
                Passed = false,
                FailureDetail = "Azure DevOps could not be reached during organization discovery."
            });

            return Failure(
                AuthenticationFailureCategory.ConnectivityFailure,
                "PAT test failed because Azure DevOps could not be reached during organization discovery.",
                "Check your network connection and Azure DevOps availability, then try setup again.",
                checks);
        }
        catch (TaskCanceledException)
        {
            checks.Add(new PatCapabilityCheck
            {
                Name = "Organization discovery",
                Operation = "List accessible Azure DevOps organizations",
                Passed = false,
                FailureDetail = "The organization discovery request timed out."
            });

            return Failure(
                AuthenticationFailureCategory.ConnectivityFailure,
                "PAT test timed out while listing Azure DevOps organizations.",
                "Check your network connection and try setup again.",
                checks);
        }
        catch (InvalidOperationException ex) when (TryMapFailure(ex, "organization discovery", checks, out var result))
        {
            return result;
        }

        checks.Add(new PatCapabilityCheck
        {
            Name = "Organization discovery",
            Operation = "List accessible Azure DevOps organizations",
            Passed = true
        });

        if (organizations.Count == 0)
        {
            checks.Add(new PatCapabilityCheck
            {
                Name = "Toolkit access",
                Operation = "Verify the PAT can reach at least one Azure DevOps organization",
                Passed = false,
                FailureDetail = "The PAT authenticated, but no accessible organizations were available."
            });

            return Failure(
                AuthenticationFailureCategory.InsufficientPermissions,
                "PAT test failed because the token does not expose an accessible Azure DevOps organization for this toolkit.",
                "Use a PAT that can access the Azure DevOps organization you want to work with, then try setup again.",
                checks);
        }

        checks.Add(new PatCapabilityCheck
        {
            Name = "Toolkit access",
            Operation = "Verify the PAT can reach at least one Azure DevOps organization",
            Passed = true
        });

        return new AuthenticationCheckResult
        {
            IsSuccess = true,
            FailureCategory = AuthenticationFailureCategory.None,
            SummaryMessage = "PAT test succeeded. The token is valid and can be used by this toolkit.",
            Guidance = "Press enter to save the PAT and continue setup.",
            CapabilityChecks = checks,
            Organizations = organizations
        };
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

    private static async Task<string?> GetAuthenticatedProfileIdAsync(HttpClient client, CancellationToken cancellationToken)
    {
        using var profileResponse = await client.GetAsync("https://app.vssps.visualstudio.com/_apis/profile/profiles/me?api-version=7.1", cancellationToken);
        await EnsureSuccessAsync(profileResponse, "Unable to resolve the authenticated Azure DevOps profile.");

        await using var profileStream = await profileResponse.Content.ReadAsStreamAsync(cancellationToken);
        using var profileDocument = await JsonDocument.ParseAsync(profileStream, cancellationToken: cancellationToken);

        return profileDocument.RootElement.TryGetProperty("id", out var idElement)
            ? idElement.GetString()
            : null;
    }

    private static async Task<IReadOnlyList<AdoOrganizationInfo>> GetOrganizationsAsync(HttpClient client, string memberId, CancellationToken cancellationToken)
    {
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

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string message)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        throw new InvalidOperationException($"{message} HTTP {(int)response.StatusCode}: {body}");
    }

    private static bool TryMapFailure(
        InvalidOperationException exception,
        string operationName,
        List<PatCapabilityCheck> checks,
        out AuthenticationCheckResult result)
    {
        var details = exception.Message;
        var statusCode = TryExtractStatusCode(details);
        if (statusCode is null)
        {
            result = Failure(
                AuthenticationFailureCategory.ServiceFailure,
                $"PAT test failed during {operationName}.",
                "Try setup again. If the issue persists, Azure DevOps may be returning an unexpected response.",
                checks);
            return true;
        }

        var capabilityDetail = BuildFailureDetail(statusCode.Value, operationName);
        checks.Add(new PatCapabilityCheck
        {
            Name = ToTitle(operationName),
            Operation = $"Verify {operationName}",
            Passed = false,
            FailureDetail = capabilityDetail
        });

        var category = statusCode.Value switch
        {
            401 => AuthenticationFailureCategory.InvalidCredentials,
            403 => AuthenticationFailureCategory.InsufficientPermissions,
            >= 500 => AuthenticationFailureCategory.ServiceFailure,
            _ => AuthenticationFailureCategory.ServiceFailure
        };

        var summary = category switch
        {
            AuthenticationFailureCategory.InvalidCredentials => $"PAT test failed during {operationName} because Azure DevOps rejected the credential.",
            AuthenticationFailureCategory.InsufficientPermissions => $"PAT test failed during {operationName} because the token does not have the permissions required by this toolkit.",
            AuthenticationFailureCategory.ServiceFailure => $"PAT test failed during {operationName} because Azure DevOps returned a service error.",
            _ => $"PAT test failed during {operationName}."
        };

        var guidance = category switch
        {
            AuthenticationFailureCategory.InvalidCredentials => "Check whether the PAT value is correct, active, and not expired, then try setup again.",
            AuthenticationFailureCategory.InsufficientPermissions => "Use a PAT with the Azure DevOps permissions required for setup and toolkit access, then try setup again.",
            AuthenticationFailureCategory.ServiceFailure => "Try setup again in a moment. If the problem continues, Azure DevOps may be unavailable.",
            _ => "Try setup again."
        };

        result = Failure(category, summary, guidance, checks);
        return true;
    }

    private static int? TryExtractStatusCode(string message)
    {
        const string marker = "HTTP ";
        var index = message.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return null;
        }

        var segment = message[(index + marker.Length)..];
        var digits = new string(segment.TakeWhile(char.IsDigit).ToArray());
        return int.TryParse(digits, out var statusCode) ? statusCode : null;
    }

    private static string BuildFailureDetail(int statusCode, string operationName)
    {
        return statusCode switch
        {
            401 => $"Azure DevOps returned 401 during {operationName}.",
            403 => $"Azure DevOps returned 403 during {operationName}.",
            >= 500 => $"Azure DevOps returned {statusCode} during {operationName}.",
            _ => $"Azure DevOps returned HTTP {statusCode} during {operationName}."
        };
    }

    private static string ToTitle(string text)
    {
        return string.Concat(text[0].ToString().ToUpperInvariant(), text.AsSpan(1));
    }

    private static AuthenticationCheckResult Failure(
        AuthenticationFailureCategory category,
        string summary,
        string guidance,
        IReadOnlyList<PatCapabilityCheck> checks)
    {
        return new AuthenticationCheckResult
        {
            IsSuccess = false,
            FailureCategory = category,
            SummaryMessage = summary,
            Guidance = guidance,
            CapabilityChecks = checks.ToList()
        };
    }
}
