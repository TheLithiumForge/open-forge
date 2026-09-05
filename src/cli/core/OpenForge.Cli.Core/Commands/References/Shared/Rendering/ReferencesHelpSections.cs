using System.Globalization;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.References.Shared.Rendering;

internal static class ReferencesHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                "Syntax",
                "  open-forge references <source-reference> [--direction=in|out|both] "
                + "[--include=<source-reference>]... [--exclude=<source-reference>]... [global flags]"),
            new CliHelpSection(
                "Source and direction",
                "  source-reference accepts one source ID or exact .agents/... path. "
                + "--direction selects in, out, or both; default: both. Direction values are exact and case-sensitive."),
            new CliHelpSection(
                "Incoming filters",
                "  --include and --exclude each consume one scalar source reference per occurrence. "
                + "Includes form a union, exclusions win, and filters apply only to incoming work; they are invalid with out."),
            new CliHelpSection(
                "Global options",
                "  --workspace <path>, --json, --view=<compact|expanded>, --verbose, --help, and --version "
                + "retain their shared Shell meaning. --view is a no-op with --json."),
            new CliHelpSection(
                "Examples",
                "  open-forge references memory\n"
                + "  open-forge references memory --direction=in --include=directives --exclude=working/checkpoints\n"
                + $"  open-forge references {SourceLogicalPath.LoaderPath} --direction=out --json"),
            new CliHelpSection(
                "Related commands",
                "  open-forge find — discover sources by authored predicates.\n"
                + "  open-forge route list — list routed source identities.\n"
                + "  open-forge doctor — inspect unavailable or blocked source facts."),
            new CliHelpSection("Results and streams", ResultsAndStreams()),
            new CliHelpSection(
                "Notes",
                "  References is a deterministic, read-only one-hop report. It does not fetch URLs, load target bodies, "
                + "follow links, write files, build an index, or repair destinations."),
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
            lines.Add(
                $"  {definition.MachineName}: exit {definition.Disposition.ExitCode.ToString(CultureInfo.InvariantCulture)} "
                + $"and human {Stream(definition.Disposition.HumanOutputTarget)}.");
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
