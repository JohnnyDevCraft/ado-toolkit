namespace AdoToolkit.Models;

public sealed class AuthenticationCheckResult
{
    public bool IsSuccess { get; init; }

    public AuthenticationFailureCategory FailureCategory { get; init; }

    public required string SummaryMessage { get; init; }

    public required string Guidance { get; init; }

    public IReadOnlyList<PatCapabilityCheck> CapabilityChecks { get; init; } = [];

    public IReadOnlyList<AdoOrganizationInfo> Organizations { get; init; } = [];
}
