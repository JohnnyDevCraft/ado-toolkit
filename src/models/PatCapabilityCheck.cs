namespace AdoToolkit.Models;

public sealed class PatCapabilityCheck
{
    public required string Name { get; init; }

    public required string Operation { get; init; }

    public bool Passed { get; init; }

    public string? FailureDetail { get; init; }
}
