using System.Globalization;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Rendering;

internal static class UpdateHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                heading: "Syntax",
                body: "  open-forge update [--force] [--prune] [--automatic] [--dry-run] [global flags]"),
            new CliHelpSection(
                heading: "Reconciliation",
                body: "  Update reconciles one trusted managed Framework installation with the complete payload embedded in the running CLI. A verified current installation is a write-free no-op."),
            new CliHelpSection(
                heading: "Authority",
                body: "  --force may replace changed or restore missing current managed content. "
                + "--prune may delete eligible retired managed content. The two authorities are independent; "
                + "--automatic grants neither. Safe source changes and new targets do not require force."),
            new CliHelpSection(
                heading: "Execution",
                body: "  --automatic suppresses confirmation without adding force or prune. "
                + "--dry-run previews the same complete preflighted plan and writes nothing. "
                + "A prompt-capable human apply that would write asks once after preflight; "
                + "JSON and redirected execution never prompt."),
            new CliHelpSection(
                heading: "Global options",
                body: "  --workspace <path>, --json, --view <compact|expanded>, --verbose, --help, and --version retain their shared Shell meaning. --view is a no-op with --json."),
            new CliHelpSection(
                heading: "Examples",
                body: "  open-forge update\n"
                + "  open-forge update --automatic --dry-run --json\n"
                + "  open-forge update --force\n"
                + "  open-forge update --force --prune --automatic"),
            new CliHelpSection(
                heading: "Related commands",
                body: "  open-forge doctor — inspect blocked or unavailable lifecycle and safety facts.\n"
                + "  open-forge cleanup — remove a reported retained recovery artifact after review."),
            new CliHelpSection(heading: "Results and streams", body: ResultsAndStreams()),
            new CliHelpSection(
                heading: "Notes",
                body: "  Update uses only trusted local lifecycle state and the Framework payload embedded in the running CLI. "
                + "It does not fetch content, adopt unmanaged files, manipulate Git, restore targets automatically, "
                + "or remove recovery artifacts owned by Cleanup."),
        ]);

    private static string ResultsAndStreams()
    {
        var lines = new List<string>
        {
            "  Human complete, attention, and incomplete results use stdout; invalid, blocked, failed, and interrupted results use stderr.",
            string.Create(
                CultureInfo.InvariantCulture,
                $"  JSON writes one schema-version-{UpdateDefinitions.SchemaVersion} envelope to stdout for every semantic status. Verbose diagnostics use bounded stderr."),
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
