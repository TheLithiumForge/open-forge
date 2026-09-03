using System.Globalization;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Rendering;

internal static class RouteMoveHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                "Syntax",
                "  open-forge route move <source-reference> <destination-target> [--dry-run] [global flags]"),
            new CliHelpSection(
                "Source",
                "  <source-reference> selects one ordinary unmanaged routed Markdown leaf by ID, base path, or overwrite path, or one complete unmanaged category by its recognized entrypoint path."),
            new CliHelpSection(
                "Destination",
                "  <destination-target> is one exact workspace-relative .agents path below an existing routable parent. Leaf and category destination forms must match the selected subject."),
            new CliHelpSection(
                "Write policy",
                "  Omit --dry-run to apply the complete locked and revalidated move, reference, and generated-navigation plan. --dry-run previews that same plan without writing files."),
            new CliHelpSection(
                "Examples",
                "  open-forge route move guidance/old-guide .agents/archive/new-guide.md\n"
                + "  open-forge route move .agents/guidance/topics/_topics.md .agents/archive/topics/_topics.md --dry-run"),
            new CliHelpSection(
                "Notes",
                "  Route Move never moves lifecycle-managed content, initializes a missing parent route, overwrites a destination, prompts, or invokes Index as a subprocess.\n"
                + ResultsAndStreams()),
        ]);

    private static string ResultsAndStreams()
    {
        var lines = new List<string>
        {
            "  Human complete, attention, and incomplete results use stdout; invalid, blocked, failed, and interrupted results use stderr.",
            string.Create(
                CultureInfo.InvariantCulture,
                $"  JSON writes one schema-version-{RouteMoveDefinitions.SchemaVersion} envelope to stdout for every semantic status. Verbose diagnostics use bounded stderr."),
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
