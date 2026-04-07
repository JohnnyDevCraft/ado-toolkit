using Spectre.Console;

namespace AdoToolkit.Presentation;

public sealed class AppHeaderRenderer
{
    public void Render()
    {
        AnsiConsole.Write(new FigletText("ADO Toolkit").Color(Color.Aqua));
        AnsiConsole.MarkupLine("[grey]Bridge Azure DevOps context into developer and AI workflows.[/]");
        AnsiConsole.WriteLine();
    }
}
