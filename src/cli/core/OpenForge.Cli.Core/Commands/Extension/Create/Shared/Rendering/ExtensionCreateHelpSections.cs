using System.Globalization;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Extension.Create.Shared.Rendering;

internal static class ExtensionCreateHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                "Syntax",
                """
                  open-forge extension create [<stable-id>] [--path <catalogue-path>]
                    [--name <text>] [--description <text>] [--package-version <text>]
                    [--dependency <stable-id>]... [--automatic] [--dry-run] [global flags]
                """),
            new CliHelpSection(
                "Required input and interaction",
                """
                  Stable ID and --path are required before planning. A prompt-capable human request asks only for whichever fact is missing and permits local correction.
                  Fully explicit requests do not prompt. --automatic, --json, and redirected use never prompt and return invalid when either required fact is absent.
                """),
            new CliHelpSection(
                "Manifest",
                """
                  The default name splits the stable ID on hyphens and uppercases each first ASCII letter.
                  Description is `Open Forge Extension package <stable-id>.`; package version is `0.1.0`; dependencies default to none.
                  --name, --description, and --package-version are singleton nonblank overrides.
                  Repeat --dependency for valid distinct non-self IDs; dependencies are written in ordinal order and availability is not resolved.
                """),
            new CliHelpSection(
                "Catalogue and scaffold",
                """
                  --path selects one existing safely resolved ordinary catalogue directory; no marker is required and the parent is never created. Only <catalogue>/<stable-id>/ is inspected.
                  An absent destination can be created and an exact scaffold is a verified no-op.
                  Divergent, partial, additional, unknown, or colliding content blocks. The scaffold contains only extension.json and payload/.agents/.
                """),
            new CliHelpSection(
                "Modes and global options",
                """
                  Apply is the default. --dry-run previews the same exact plan without writes; --automatic suppresses interaction but adds no inferred input or authority.
                  --workspace is accepted as a no-op. --json, --view=<compact|expanded>, --verbose, --help, and --version retain their shared meaning; --view is a no-op with --json.
                """),
            new CliHelpSection(
                "Examples",
                """
                  open-forge extension create
                  open-forge extension create development-toolkit --path D:/packages/open-forge --automatic --dry-run
                  open-forge extension create development-toolkit --path D:/packages/open-forge --name "Development Toolkit" --description "Adds development workflows"
                  open-forge extension create development-toolkit --path D:/packages/open-forge --package-version 0.2.0 --dependency shared-prompts
                """),
            new CliHelpSection(
                "Results and streams",
                ResultsAndStreams()),
            new CliHelpSection(
                "Workspace and recovery boundary",
                """
                  Extension Create never installs the package, mutates a workspace, writes lifecycle or generated-navigation state, acquires .agents/open-forge.lock, or creates a recovery bundle.
                  It uses a separate create-only destination path with no Replace or Delete and never restores, rolls back, or compensates for retained partial state.
                """),
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
