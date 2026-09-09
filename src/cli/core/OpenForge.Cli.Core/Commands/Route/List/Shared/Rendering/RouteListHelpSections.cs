using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;

internal static class RouteListHelpSections
{
    internal static CliHelpContent CreateList()
    {
        return new CliHelpContent(
        [
            new CliHelpSection(
                "Syntax",
                "  open-forge route list [source-reference] [--depth=<non-negative-integer|all>] [global flags]"),
            new CliHelpSection(
                "Source references",
                "  source-reference is either an exact source ID such as memory or an exact .agents/... or "
                + "./.agents/... path such as .agents/memory/_memory.md. IDs use the current workspace and are not "
                + "fuzzy or path guesses."),
            new CliHelpSection(
                "Depth",
                "  The default depth is 1. Use --depth=0 for selected roots only, a non-negative Int32 for a "
                + "bounded descendant depth, or --depth=all for the complete routed descendant closure. "
                + "The equals form is required for --depth."),
            new CliHelpSection(
                "Inherited global options",
                "  This leaf inherits --workspace <path>, --json, --view=<compact|expanded>, --verbose, --help, "
                + "and --version from the root. --view changes only human density and is a no-op with --json."),
            new CliHelpSection(
                "Exit and streams",
                BuildExitAndStreamBody()),
            new CliHelpSection(
                "Examples",
                "  open-forge route list\n"
                + "  open-forge route list memory\n"
                + "  open-forge route list .agents/memory/_memory.md\n"
                + "  open-forge route list memory --depth=all\n"
                + "  open-forge route list --depth=2 --json"),
            new CliHelpSection(
                "Related commands",
                "  route inspect — available; inspect one source's route behavior.\n"
                + "  find — find Markdown sources by authored tags and structural headings.\n"
                + "  context — return ordered startup and selected source content."),
        ]);
    }

    private static string BuildExitAndStreamBody()
    {
        var lines = new List<string>
        {
            "  Human complete, attention, and incomplete results use stdout; invalid, blocked, failed, and interrupted results use stderr.",
            "  JSON always writes one result envelope to stdout. Verbose diagnostics, when requested, use stderr and never include route rows or file content.",
        };
        foreach (var status in Enum.GetValues<CliSemanticStatus>())
        {
            var definition = CliStatusDefinitions.Read(status);
            lines.Add($"  {definition.MachineName}: exit {definition.Disposition.ExitCode.ToString(System.Globalization.CultureInfo.InvariantCulture)}.");
        }

        return string.Join(Environment.NewLine, lines);
    }
}
