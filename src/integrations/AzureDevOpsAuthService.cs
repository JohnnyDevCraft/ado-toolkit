namespace AdoToolkit.Integrations;

public sealed class AzureDevOpsAuthService
{
    public void EnsurePatConfigured(string? pat)
    {
        if (string.IsNullOrWhiteSpace(pat))
        {
            throw new InvalidOperationException("A Personal Access Token is required for this command.");
        }
    }
}

