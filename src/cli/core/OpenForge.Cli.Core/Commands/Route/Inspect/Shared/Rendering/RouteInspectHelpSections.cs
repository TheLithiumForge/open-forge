using System.Globalization;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;

internal static class RouteInspectHelpSections
{
    internal static CliHelpContent CreateInspect()
    {
        return new CliHelpContent(
        [
            new CliHelpSection(
                "Syntax",
                "  open-forge route inspect <source-reference> [global options]"),
            new CliHelpSection(
                "Source references",
                "  Use one source-id or one exact .agents/... or ./.agents/... path. "
                + "Supply a source reference to run this command."),
            new CliHelpSection(
                "Inherited global options",
                "  --workspace <path>, --json, --view <compact|expanded>, --verbose, --help, and --version.\n"
                + "  You can also use --view=<compact|expanded> or --view:<compact|expanded>.\n"
                + "  --view selects detail in text and JSON."),
            new CliHelpSection("Results and streams", BuildResultsBody()),
            new CliHelpSection(
                "Examples",
                "  open-forge route inspect memory/working/checkpoints\n"
                + "  open-forge route inspect .agents/memory/_memory.md --view compact\n"
                + "  open-forge route inspect memory/working/checkpoints --json --view expanded\n"
                + "  open-forge route inspect --help\n"
                + "  open-forge route inspect --version"),
            new CliHelpSection(
                "Related commands",
                "  route list — list routed sources and descendants.\n"
                + "  context — use open-forge context [source-reference...] to read selected source content.\n"
                + "  doctor — diagnose workspace conditions without changing them."),
            new CliHelpSection(
                "Notes",
                "  Inspect reports one route profile without authored source bodies, ordinary links, "
                + "diagnosis, recommendations, or mutation. JSON always writes one complete result "
                + "document to stdout; verbose diagnostics are bounded and use stderr."),
        ]);
    }

    private static string BuildResultsBody()
    {
        var lines = new List<string>
        {
            "  Human complete, attention, and incomplete results use stdout; invalid, blocked, failed, and interrupted results use stderr.",
            "  Expanded JSON writes one schema-version-1 result to stdout for every status. Verbose diagnostics use stderr.",
        };
        foreach (var status in Enum.GetValues<CliSemanticStatus>())
        {
            var definition = CliStatusDefinitions.Read(status);
            lines.Add(
                $"  {definition.MachineName}: exit {definition.Disposition.ExitCode.ToString(CultureInfo.InvariantCulture)}.");
        }

        return string.Join(Environment.NewLine, lines);
    }
}
