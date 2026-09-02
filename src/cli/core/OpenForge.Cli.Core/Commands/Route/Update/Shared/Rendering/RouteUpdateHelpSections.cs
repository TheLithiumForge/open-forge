using System.Globalization;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Rendering;

internal static class RouteUpdateHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                "Syntax",
                "  open-forge route update <source-reference> [--description <text>] [--responsibility <text>] [--tag=<tag>]... [--template <template-reference>] [--dry-run] [global flags]"),
            new CliHelpSection(
                "Target",
                """
                  <source-reference> selects one existing ordinary routed Markdown source by exact ID, base path, or adjacent overwrite path.
                  Route Update preserves its ID, base path, document form, and overwrite ownership.
                """),
            new CliHelpSection(
                "Metadata",
                """
                  Supply at least one metadata or Template operation. Description, responsibility, and Template values are singletons and reject repetition.
                  Repeated --tag=<tag> values form one ordered replacement list; empty or duplicate tags are invalid.
                  An exact empty responsibility removes that field, while whitespace-only text is invalid.
                """),
            new CliHelpSection(
                "Template",
                """
                  --template <template-reference> copies only the body of one exact routed Markdown source tagged Template when the target body is empty or whitespace.
                  The target's authored body content is protected; target metadata and identity remain unchanged.
                """),
            new CliHelpSection(
                "Write policy",
                """
                  Omit --dry-run to apply the complete locked and revalidated target and generated-navigation plan.
                  --dry-run previews that same plan without writing files. A verified no-op writes nothing and creates no recovery bundle.
                """),
            new CliHelpSection(
                "Examples",
                "  open-forge route update memory/project-alpha/overview --description \"Project overview\"\n"
                + "  open-forge route update .agents/memory/project-alpha/overview.md --responsibility \"\" --tag=Docs --tag=Memory\n"
                + "  open-forge route update memory/project-alpha/overview --template templates/route --dry-run"),
            new CliHelpSection(
                "Notes",
                "  Global workspace, JSON, view, verbosity, help, and version options retain their shared meaning.\n"
                + ResultsAndStreams()),
        ]);

    private static string ResultsAndStreams()
    {
        var lines = new List<string>
        {
            "  Human complete, attention, and incomplete results use stdout; invalid, blocked, failed, and interrupted results use stderr.",
            string.Create(
                CultureInfo.InvariantCulture,
                $"  JSON writes one schema-version-{RouteUpdateDefinitions.SchemaVersion} envelope to stdout for every semantic status. Verbose diagnostics use bounded stderr."),
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
