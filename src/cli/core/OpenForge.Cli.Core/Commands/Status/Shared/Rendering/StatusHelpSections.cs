using System.Globalization;
using OpenForge.Cli.Core.Commands.Status;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Rendering;

internal static class StatusHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                "Syntax",
                "  open-forge status [global flags]"),
            new CliHelpSection(
                "Inspection",
                "  Inspect the selected workspace's installation, startup and continuity context, generated navigation, lifecycle, managed targets, and exact recovery candidates."),
            new CliHelpSection(
                "Global options",
                "  --workspace <path>, --json, --view=<compact|expanded>, --verbose, --help, and --version retain their shared Shell meaning. --view is a no-op with --json."),
            new CliHelpSection(
                "Related commands",
                """
                  open-forge doctor — diagnose unavailable or attention-requiring operational facts.
                  open-forge context — return startup or selected authored context.
                """),
            new CliHelpSection("Results and streams", ResultsAndStreams()),
            new CliHelpSection(
                "Notes",
                "  Status is deterministic, stateless, and read-only. It does not acquire a workspace lock, repair lifecycle state, restore recovery data, or modify the workspace."),
        ]);

    private static string ResultsAndStreams()
    {
        var lines = new List<string>
        {
            "  Human complete, attention, and incomplete results use stdout; invalid, blocked, failed, and interrupted results use stderr.",
            string.Create(
                CultureInfo.InvariantCulture,
                $"  JSON writes one schema-version-{StatusDefinitions.SchemaVersion} envelope to stdout for every semantic status. Verbose diagnostics use bounded stderr."),
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
