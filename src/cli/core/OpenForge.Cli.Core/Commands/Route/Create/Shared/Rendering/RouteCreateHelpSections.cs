using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Rendering;

internal static class RouteCreateHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                "Syntax",
                "  open-forge route create <file-target> --description <text> --tag=<tag>... [--responsibility <text>] [--template <template-reference>] [--dry-run] [global flags]"),
            new CliHelpSection(
                "Target",
                "  <file-target> selects one ordinary Markdown source ID or exact .agents path below exactly one existing routable parent. Use route init when the parent route is missing."),
            new CliHelpSection(
                "Metadata",
                "  --description <text> and at least one ordered --tag=<tag> are required. --responsibility <text> is optional. Singleton values reject repetition; exact duplicate tags are invalid."),
            new CliHelpSection(
                "Template",
                "  --template <template-reference> copies only the body of one exact routed Markdown source tagged Template. The destination keeps its explicit metadata and no continuing Template relationship."),
            new CliHelpSection(
                "Write policy",
                "  Omit --dry-run to apply the complete destination and generated-navigation plan. --dry-run previews that same plan without writing files."),
            new CliHelpSection(
                "Examples",
                """
                  open-forge route create memory/project-alpha/overview --description "Project overview" --tag=Docs
                  open-forge route create .agents/memory/project-alpha/overview.md --description "Project overview" --tag=Docs --template templates/route --dry-run
                """),
            new CliHelpSection(
                "Notes",
                "  Route Create does not initialize parent routes, overwrite differing content, create lifecycle ownership, infer metadata, or invoke Index as a subprocess."),
        ]);
}
