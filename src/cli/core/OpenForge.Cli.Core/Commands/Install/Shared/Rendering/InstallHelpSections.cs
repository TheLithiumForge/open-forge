using System.Globalization;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Rendering;

internal static class InstallHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                heading: "Syntax",
                body: "  open-forge install [--force] [--automatic] [--dry-run] [global flags]"),
            new CliHelpSection(
                heading: "Establishment",
                body: "  Install establishes the complete embedded Framework in one exact workspace or verifies an exact managed installation as a no-op. Managed divergence is preserved and directs to open-forge update."),
            new CliHelpSection(
                heading: "Write policy",
                body: "  --force may replace only one eligible initial occupant; it never updates or adopts managed state. --automatic suppresses confirmation without adding force. --dry-run previews the same complete preflighted plan and writes nothing. Repetition is idempotent."),
            new CliHelpSection(
                heading: "Confirmation",
                body: "  A prompt-capable human apply that would write asks once after preflight. Dry-run, verified no-op, --automatic, JSON, and non-prompt-capable requests never prompt; a non-prompt-capable human write requires --automatic."),
            new CliHelpSection(
                heading: "Global options",
                body: "  --workspace <path>, --json, --view <compact|expanded>, --verbose, --help, and --version retain their shared Shell meaning. --view is a no-op with --json."),
            new CliHelpSection(
                heading: "Examples",
                body: "  open-forge install\n"
                + "  open-forge install --automatic --dry-run --json\n"
                + "  open-forge install --force\n"
                + "  open-forge install --force --automatic --dry-run"),
            new CliHelpSection(
                heading: "Related commands",
                body: "  open-forge update — reconcile an existing managed Framework installation.\n"
                + "  open-forge doctor — inspect blocked or unavailable lifecycle and safety facts.\n"
                + "  open-forge cleanup — remove a reported retained recovery artifact after review."),
            new CliHelpSection(heading: "Results and streams", body: ResultsAndStreams()),
            new CliHelpSection(
                heading: "Notes",
                body: "  Install uses only the Framework payload embedded in the running CLI. It does not discover another workspace, fetch content, manipulate Git, repair markers, reconcile managed divergence, or roll back target effects."),
        ]);

    private static string ResultsAndStreams()
    {
        var lines = new List<string>
        {
            "  Human complete, attention, and incomplete results use stdout; invalid, blocked, failed, and interrupted results use stderr.",
            string.Create(
                CultureInfo.InvariantCulture,
                $"  JSON writes one schema-version-{InstallDefinitions.SchemaVersion} envelope to stdout for every semantic status. Verbose diagnostics use bounded stderr."),
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
            _ => throw new ArgumentOutOfRangeException(
                nameof(target),
                target,
                "The output target is not defined."),
        };
}
