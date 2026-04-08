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
            "Bridge Azure DevOps context into developer and AI workflows.",
            "Designed By: JohnnyDevCraft | Xelseor LLC 2026",
            "Version: 0.1.1 | Build Date: 04/08/2026"
        ];
    }

    public void Render()
    {
        foreach (var line in GetHeaderLines().Take(5))
        {
            AnsiConsole.MarkupLine($"[aqua]{Markup.Escape(line)}[/]");
        }

        foreach (var line in GetHeaderLines().Skip(5))
        {
            AnsiConsole.MarkupLine($"[grey]{Markup.Escape(line)}[/]");
        }

        AnsiConsole.WriteLine();
    }
}
