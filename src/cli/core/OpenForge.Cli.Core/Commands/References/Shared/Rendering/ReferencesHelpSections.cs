using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Help;

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
            new CliHelpSection("Results and streams", CliResultHelp.ResultsAndStreams(schemaVersion: 1)),
            new CliHelpSection(
                "Notes",
                "  References is a deterministic, read-only one-hop report. It does not fetch URLs, load target bodies, "
                + "follow links, write files, build an index, or repair destinations."),
        ]);
}
