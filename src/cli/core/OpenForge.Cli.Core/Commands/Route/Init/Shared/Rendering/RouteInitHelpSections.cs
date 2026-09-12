using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Help;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Rendering;

internal static class RouteInitHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                "Syntax",
                "  open-forge route init <route-target> [--framework] [--description <text>] [--responsibility <text>] [--tag=<tag>]... [--dry-run] [global flags]"),
            new CliHelpSection(
                "Target",
                "  <route-target> selects one exact route ID or .agents entrypoint path. A missing exact-path target must use the canonical entrypoint filename. Route Init creates missing entrypoints in that exact chain and never creates the Loader."),
            new CliHelpSection(
                "Scaffold mode",
                "  Generic mode uses the fixed draft scaffold. --framework uses the trusted embedded Framework topology and managed entrypoint assets. Repetition of --framework is idempotent."),
            new CliHelpSection(
                "Metadata",
                "  --description <text>, --responsibility <text>, and ordered repeated --tag=<tag> values apply only to a missing generic final target. Description and responsibility are singletons; empty or duplicate tags are invalid."),
            new CliHelpSection(
                "Write policy",
                "  Omit --dry-run to apply the complete preflighted plan. --dry-run previews the same directories, entrypoints, generated-region effects, and bounded changes without writing files. Repetition is idempotent."),
            new CliHelpSection(
                "Global options",
                "  --workspace <path>, --json, --view <compact|expanded>, --verbose, --help, and --version retain their shared Shell meaning. --view is a no-op with --json."),
            new CliHelpSection(
                "Examples",
                "  open-forge route init memory/project-alpha/documents\n"
                + "  open-forge route init memory/project-alpha/documents --description \"Project documents\" --tag=Memory\n"
                + "  open-forge route init \"memory/Mobile App/crystallized/documents\" --framework --dry-run\n"
                + "  open-forge route init .agents/memory/project-alpha/documents/_documents.md --json"),
            new CliHelpSection(
                "Related commands",
                "  open-forge route update — author an existing entrypoint.\n"
                + "  open-forge doctor — inspect blocked or unavailable route and safety facts.\n"
                + "  open-forge cleanup — remove a reported retained recovery artifact after review."),
            new CliHelpSection("Results and streams", CliResultHelp.ResultsAndStreams(RouteInitDefinitions.SchemaVersion)),
            new CliHelpSection(
                "Notes",
                "  Route Init does not instantiate a Template, infer route meaning, rewrite authored content, normalize compatibility filenames, repair malformed generated regions, manipulate Git, or create commits."),
        ]);
}
