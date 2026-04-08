using Spectre.Console;

namespace AdoToolkit.Presentation;

public sealed class SpectreSetupInteraction : ISetupInteraction
{
    public string PromptSecret(string prompt)
    {
        return AnsiConsole.Prompt(
            new TextPrompt<string>(prompt)
                .Secret());
    }

    public string PromptText(string prompt)
    {
        return AnsiConsole.Prompt(new TextPrompt<string>(prompt));
    }

    public void WaitForAcknowledgement(string message)
    {
        AnsiConsole.MarkupLine($"[grey]{Markup.Escape(message)}[/]");
        Console.ReadLine();
    }
}
