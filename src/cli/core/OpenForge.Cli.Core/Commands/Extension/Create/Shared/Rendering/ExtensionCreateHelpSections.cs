using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Help;

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
                    [--dependency <stable-id>]... [--automatic] [--dry-run] [global options]
                """),
            new CliHelpSection(
                "Required input and interaction",
                """
                  Stable ID and --path are required to plan creation. Interactive text requests ask for missing values and let you correct invalid input.
                  Supplying both required values avoids prompts. --automatic, --json, and redirected requests return invalid if either value is missing.
                """),
            new CliHelpSection(
                "Manifest",
                """
                  The default name splits the stable ID on hyphens and uppercases each first ASCII letter.
                  Description is `Open Forge Extension package <stable-id>.`; package version is `0.1.0`; dependencies default to none.
                  --name, --description, and --package-version are nonblank values, each supplied once.
                  Repeat --dependency for valid distinct non-self IDs; dependencies are written in ordinal order and availability is not resolved.
                """),
            new CliHelpSection(
                "Catalogue and scaffold",
                """
                  --path selects an existing local catalogue directory. It needs no marker, and the command never creates this parent. Only <catalogue>/<stable-id>/ is inspected.
                  A missing destination is created. An identical existing scaffold needs no writes.
                  Divergent, partial, additional, unknown, or colliding content blocks. The scaffold contains only extension.json and content/.agents/.
                """),
            new CliHelpSection(
                "Modes and global options",
                """
                  Apply is the default. --dry-run previews the same exact plan without writes; --automatic suppresses interaction but adds no inferred input or authority.
                  --workspace is accepted as a no-op. --json, --view <compact|expanded>, --verbose, --help, and --version retain their shared meaning; --view is ignored with --json.
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
                CliResultHelp.ResultsAndStreams(schemaVersion: 1)),
            new CliHelpSection(
                "Workspace and recovery boundary",
                """
                  Extension Create never installs the package, mutates a workspace, writes lifecycle or generated-navigation state, acquires a workspace mutation lock, or creates a recovery bundle.
                  It uses a separate create-only destination path with no Replace or Delete and never restores, rolls back, or compensates for retained partial state.
                """),
        ]);
}
