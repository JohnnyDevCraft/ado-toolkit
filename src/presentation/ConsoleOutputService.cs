using Spectre.Console;

namespace AdoToolkit.Presentation;

public sealed class ConsoleOutputService
{
    public string BuildSuccessMarkup(string message) => $"[green]{Markup.Escape(message)}[/]";

    public string BuildInfoMarkup(string message) => $"[blue]{Markup.Escape(message)}[/]";

    public string BuildWarningMarkup(string message) => $"[yellow]{Markup.Escape(message)}[/]";

    public string BuildErrorMarkup(string message) => $"[red]{Markup.Escape(message)}[/]";

    public void WriteSuccess(string message)
    {
        AnsiConsole.MarkupLine(BuildSuccessMarkup(message));
    }

    public void WriteInfo(string message)
    {
        AnsiConsole.MarkupLine(BuildInfoMarkup(message));
    }

    public void WriteWarning(string message)
    {
        AnsiConsole.MarkupLine(BuildWarningMarkup(message));
    }

    public void WriteError(string message)
    {
        AnsiConsole.MarkupLine(BuildErrorMarkup(message));
    }
}
