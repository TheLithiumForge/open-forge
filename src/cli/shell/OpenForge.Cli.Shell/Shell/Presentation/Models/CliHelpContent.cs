namespace OpenForge.Cli.Core.Shell.Presentation.Models;

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
