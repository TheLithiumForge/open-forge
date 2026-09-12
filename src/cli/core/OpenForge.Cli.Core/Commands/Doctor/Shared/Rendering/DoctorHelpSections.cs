using System.Globalization;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal static class DoctorHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection("Syntax", "  open-forge doctor [global flags]"),
            new CliHelpSection(
                "Diagnosis",
                "  Inspect six fixed workspace, recovery, route, reference, Framework lifecycle, and Extension lifecycle domains without changing them."),
            new CliHelpSection(
                "Global options",
                "  --workspace <path>, --json, --view=<compact|expanded>, --verbose, --help, and --version retain their shared Shell meaning. --view is a no-op with --json."),
            new CliHelpSection("Results and streams", ResultsAndStreams()),
            new CliHelpSection(
                "Notes",
                "  Doctor is deterministic, stateless, and read-only. It does not lock, repair, recover, dispatch an action, or modify the workspace."),
        ]);

    private static string ResultsAndStreams()
    {
        var lines = new List<string>
        {
            "  Human complete, attention, and incomplete results use stdout; invalid, blocked, failed, and interrupted results use stderr.",
            string.Create(CultureInfo.InvariantCulture, $"  JSON writes one schema-version-{DoctorDefinitions.SchemaVersion} envelope to stdout for every semantic status."),
        };
        foreach (var status in Enum.GetValues<CliSemanticStatus>())
        {
            var definition = CliStatusDefinitions.Read(status);
            lines.Add(string.Create(
                CultureInfo.InvariantCulture,
                $"  {definition.MachineName}: exit {definition.Disposition.ExitCode}."));
        }

        return string.Join(Environment.NewLine, lines);
    }
}
