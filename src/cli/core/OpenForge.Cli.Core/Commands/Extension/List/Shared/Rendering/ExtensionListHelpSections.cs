using System.Globalization;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Extension.List.Shared.Rendering;

internal static class ExtensionListHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                heading: "Syntax",
                body: "  open-forge extension list [--installed] [--available] [--source <package-or-catalogue-path>] [global flags]"),
            new CliHelpSection(
                heading: "Sections",
                body: "  With neither filter, render Installed then Available. --installed or --available alone selects one section; together they select both. "
                + "Repeated section flags are idempotent."),
            new CliHelpSection(
                heading: "Source and workspace",
                body: "  Installed facts use only .agents/open-forge.lifecycle.json schema v1 in the exact selected workspace. "
                + "Available facts use the embedded catalogue or one exact disjoint local package/catalogue supplied by --source. No network, registry, cache, or fallback is used."),
            new CliHelpSection(
                heading: "Global options",
                body: "  --workspace <path>, --json, --view=<compact|expanded>, --verbose, --help, and --version retain their shared Shell meaning. "
                + "--view is a no-op with --json."),
            new CliHelpSection(
                heading: "Examples",
                body: """
                  open-forge extension list
                  open-forge extension list --installed --json
                  open-forge extension list --available --source D:/packages/open-forge
                """),
            new CliHelpSection(
                heading: "Results and streams",
                body: ResultsAndStreams()),
            new CliHelpSection(
                heading: "Notes",
                body: "  Extension List is deterministic and read-only. Availability never proves installation; an installed fact survives source unavailability. "
                + "The command writes no payload, lifecycle, generated navigation, backup, cache, temporary file, or diagnostic artifact."),
        ]);

    private static string ResultsAndStreams()
    {
        var lines = new List<string>
        {
            "  Human complete, attention, and incomplete results use stdout; invalid, blocked, failed, and interrupted results use stderr.",
            "  JSON writes one schema-version-1 envelope to stdout for every semantic status. Verbose diagnostics use bounded stderr.",
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
