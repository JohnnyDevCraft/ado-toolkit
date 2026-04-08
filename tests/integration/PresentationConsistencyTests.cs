using AdoToolkit.Presentation;

namespace AdoToolkit.Tests.Integration;

public sealed class PresentationConsistencyTests
{
    [Fact]
    public void Header_renderer_exposes_stable_ascii_header()
    {
        var renderer = new AppHeaderRenderer();
        var lines = renderer.GetHeaderLines();

        Assert.Equal(6, lines.Count);
        Assert.Contains("____", lines[0], StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Bridge Azure DevOps context", lines[^1]);
    }

    [Fact]
    public void Console_output_service_formats_all_message_types()
    {
        var output = new ConsoleOutputService();

        Assert.Contains("[green]", output.BuildSuccessMarkup("done"));
        Assert.Contains("[blue]", output.BuildInfoMarkup("info"));
        Assert.Contains("[yellow]", output.BuildWarningMarkup("warn"));
        Assert.Contains("[red]", output.BuildErrorMarkup("err"));
    }

    [Fact]
    public void Page_layout_builds_consistent_frame_title()
    {
        var layout = new PageLayout(new AppHeaderRenderer());
        Assert.Equal("Pull Requests | Review stored threads", layout.BuildFrameTitle("Pull Requests", "Review stored threads"));
    }
}
