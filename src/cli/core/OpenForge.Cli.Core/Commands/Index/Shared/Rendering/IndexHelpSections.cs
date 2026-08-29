using System.Globalization;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Rendering;

internal static class IndexHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                "Syntax",
                "  open-forge index [source-reference...] [--dry-run] [global flags]"),
            new CliHelpSection(
                "Selection",
                """
                  With no source operands, Index rebuilds the Loader and every reachable entrypoint region. An entrypoint selects its subtree and direct exposing parent; a routed leaf selects its direct exposing parent.
                """),
            new CliHelpSection(
                "Write policy",
                "  Omit --dry-run to apply bounded generated-interior changes. --dry-run previews the same complete preflighted plan and writes nothing. Repetition is idempotent."),
            new CliHelpSection(
                "Global options",
                "  --workspace <path>, --json, --view <compact|expanded>, --verbose, --help, and --version retain their shared Shell meaning. --view is a no-op with --json."),
            new CliHelpSection(
                "Examples",
                """
                  open-forge index
                  open-forge index memory --dry-run
                  open-forge index .agents/memory/_memory.md --json
                """),
            new CliHelpSection(
                "Related commands",
                """
                  open-forge doctor — inspect blocked topology, metadata, or generated-region facts.
                  open-forge cleanup — remove a reported retained recovery artifact after review.
                """),
            new CliHelpSection("Results and streams", ResultsAndStreams()),
            new CliHelpSection(
                "Notes",
                "  Index changes only valid bounded generated Entries interiors. It does not repair markers, format complete files, modify overwrites, search for another workspace, or create Git commits."),
        ]);

    private static string ResultsAndStreams()
    {
        var lines = new List<string>
        {
            "  Human complete, attention, and incomplete results use stdout; invalid, blocked, failed, and interrupted results use stderr.",
            string.Create(
                CultureInfo.InvariantCulture,
                $"  JSON writes one schema-version-{IndexDefinitions.SchemaVersion} envelope to stdout for every semantic status. Verbose diagnostics use bounded stderr."),
        };
        foreach (var status in Enum.GetValues<CliSemanticStatus>())
        {
            var definition = CliStatusDefinitions.Read(status);
            lines.Add(string.Create(
                CultureInfo.InvariantCulture,
                $"  {definition.MachineName}: exit {definition.Disposition.ExitCode} and human {Stream(definition.Disposition.HumanOutputTarget)}."));
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string Stream(CliOutputTarget target)
        => target switch
        {
            CliOutputTarget.StandardOutput => "stdout",
            CliOutputTarget.StandardError => "stderr",
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, "The output target is not defined."),
        };
}
