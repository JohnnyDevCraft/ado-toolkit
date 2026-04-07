using Spectre.Console;

namespace AdoToolkit.Presentation;

public sealed class ConsoleOutputService
{
    public void WriteSuccess(string message)
    {
        AnsiConsole.MarkupLine($"[green]{Markup.Escape(message)}[/]");
    }

    public void WriteInfo(string message)
    {
        AnsiConsole.MarkupLine($"[blue]{Markup.Escape(message)}[/]");
    }

    public void WriteWarning(string message)
    {
        AnsiConsole.MarkupLine($"[yellow]{Markup.Escape(message)}[/]");
    }

    public void WriteError(string message)
    {
        AnsiConsole.MarkupLine($"[red]{Markup.Escape(message)}[/]");
    }
}

