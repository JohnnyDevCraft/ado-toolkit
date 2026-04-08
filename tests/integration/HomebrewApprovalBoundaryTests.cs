namespace AdoToolkit.Tests.Integration;

public sealed class HomebrewApprovalBoundaryTests
{
    [Fact]
    public async Task Local_packaging_script_stays_local_only()
    {
        var path = "/Users/john/Source/repos/xelseor/ado-toolkit/scripts/package-homebrew.sh";
        var content = await File.ReadAllTextAsync(path);

        Assert.DoesNotContain("gh repo create", content);
        Assert.DoesNotContain("gh release create", content);
        Assert.DoesNotContain("git push", content);
        Assert.DoesNotContain("brew tap", content);
        Assert.Contains("dist/homebrew", content);
    }

    [Fact]
    public async Task Documentation_keeps_tap_repo_updates_as_explicit_maintainer_actions()
    {
        var docsPath = "/Users/john/Source/repos/xelseor/ado-toolkit/docs/homebrew-release.md";
        var readmePath = "/Users/john/Source/repos/xelseor/ado-toolkit/README.md";

        var docs = await File.ReadAllTextAsync(docsPath);
        var readme = await File.ReadAllTextAsync(readmePath);

        Assert.Contains("homebrew-ado-toolkit", docs);
        Assert.Contains("does not automatically create or mutate the tap repository", docs);
        Assert.Contains("Tap handoff", readme);
        Assert.Contains("tap updates remain a deliberate maintainer step", readme);
    }
}
