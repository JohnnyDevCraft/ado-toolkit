using Spectre.Console;

namespace AdoToolkit.Presentation;

public sealed class PageLayout
{
    private readonly AppHeaderRenderer _headerRenderer;

    public PageLayout(AppHeaderRenderer headerRenderer)
    {
        _headerRenderer = headerRenderer;
    }

    public void Render(string title, string? subtitle = null)
    {
        _headerRenderer.Render();
        AnsiConsole.MarkupLine($"[bold]{Markup.Escape(title)}[/]");
        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            AnsiConsole.MarkupLine($"[grey]{Markup.Escape(subtitle)}[/]");
        }

        AnsiConsole.Write(new Rule().RuleStyle("grey"));
    }

    public string BuildFrameTitle(string title, string? subtitle = null)
    {
        return string.IsNullOrWhiteSpace(subtitle)
            ? title
            : $"{title} | {subtitle}";
    }
}
