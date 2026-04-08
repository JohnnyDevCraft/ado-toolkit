using Spectre.Console;

namespace AdoToolkit.Presentation;

public sealed class AppHeaderRenderer
{
    public IReadOnlyList<string> GetHeaderLines()
    {
        return
        [
            "    _    ____   ___    _____           _ _    _ _   ",
            "   / \\  |  _ \\ / _ \\  |_   _|__   ___ | | | _(_) |_ ",
            "  / _ \\ | | | | | | |   | |/ _ \\ / _ \\| | |/ / | __|",
            " / ___ \\| |_| | |_| |   | | (_) | (_) | |   <| | |_ ",
            "/_/   \\_\\____/ \\___/    |_|\\___/ \\___/|_|_|\\_\\_|\\__|",
            "Bridge Azure DevOps context into developer and AI workflows."
        ];
    }

    public void Render()
    {
        foreach (var line in GetHeaderLines().Take(5))
        {
            AnsiConsole.MarkupLine($"[aqua]{Markup.Escape(line)}[/]");
        }

        AnsiConsole.MarkupLine($"[grey]{Markup.Escape(GetHeaderLines().Last())}[/]");
        AnsiConsole.WriteLine();
    }
}
