namespace AdoToolkit.Tests.Integration;

public sealed class HomebrewReleaseWorkflowTests
{
    [Fact]
    public async Task Release_workflow_defines_tagged_build_and_formula_generation_flow()
    {
        var path = "/Users/john/Source/repos/xelseor/ado-toolkit/.github/workflows/release.yml";
        var content = await File.ReadAllTextAsync(path);

        Assert.Contains("tags:", content);
        Assert.Contains("- \"v*\"", content);
        Assert.Contains("build-macos:", content);
        Assert.Contains("homebrew-formula:", content);
        Assert.Contains("publish-release:", content);
        Assert.Contains("osx-arm64", content);
        Assert.Contains("osx-x64", content);
        Assert.Contains("./scripts/package-homebrew.sh --skip-publish", content);
        Assert.Contains("ado-toolkit.rb", content);
        Assert.Contains("softprops/action-gh-release@v2", content);
    }
}
