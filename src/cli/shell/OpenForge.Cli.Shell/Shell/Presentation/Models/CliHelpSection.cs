namespace OpenForge.Cli.Core.Shell.Presentation.Models;

internal sealed record CliHelpSection
{
    internal CliHelpSection(string heading, string body, bool preserveLines = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(heading);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        Heading = heading;
        Body = body;
        PreserveLines = preserveLines;
    }

    internal CliHelpSection(string heading, IReadOnlyList<CliHelpSection> children)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(heading);
        ArgumentNullException.ThrowIfNull(children);
        Heading = heading;
        Body = string.Empty;
        Children = children;
    }

    internal CliHelpSection(string heading, IReadOnlyList<(string Label, string? Description)> rows)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(heading);
        ArgumentNullException.ThrowIfNull(rows);
        Heading = heading;
        Body = string.Empty;
        Rows = rows;
    }

    internal string Heading { get; }

    internal string Body { get; }

    internal bool PreserveLines { get; }

    internal IReadOnlyList<CliHelpSection> Children { get; } = [];

    internal IReadOnlyList<(string Label, string? Description)> Rows { get; init; } = [];

    internal int? MaximumNameWidth { get; init; }
}
