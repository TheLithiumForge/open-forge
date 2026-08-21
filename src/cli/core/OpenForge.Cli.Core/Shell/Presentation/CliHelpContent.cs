namespace OpenForge.Cli.Core.Shell.Presentation;

internal sealed record CliHelpSection
{
    internal CliHelpSection(string heading, string body)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(heading);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        Heading = heading;
        Body = body;
    }

    internal string Heading { get; }

    internal string Body { get; }
}

internal sealed class CliHelpContent
{
    internal static readonly CliHelpContent Empty = new([]);

    internal CliHelpContent(IEnumerable<CliHelpSection> sections)
    {
        ArgumentNullException.ThrowIfNull(sections);
        Sections = Array.AsReadOnly(sections.ToArray());
    }

    internal IReadOnlyList<CliHelpSection> Sections { get; }
}
