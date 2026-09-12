using System.Globalization;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Shared.Rendering;

internal static class ExtensionUpdatePresentation
{
    internal static CliHelpContent CreateHelp()
        => new(
        [
            new CliHelpSection(
                "Syntax",
                "  open-forge extension update [<stable-id>...] [--source <package-or-catalogue-path>] [--all] [--force] [--prune] [--automatic] [--dry-run] [global options]"),
            new CliHelpSection(
                "Selection and dependencies",
                "  Select exact managed stable IDs or --all from one reviewed source. Dependencies, including dependencies of dependencies, are resolved first."),
            new CliHelpSection(
                "Authority",
                "  Normal mode preserves changed, missing, retired, shared, and unknown content. --force replaces or restores current expected content; --prune deletes eligible retired content."),
            new CliHelpSection(
                "Results and streams",
                "  Dry-run writes nothing. Human and JSON output preserve the shared semantic status, stream, and exit mapping."),
        ]);

    internal static string RenderHuman(CliPresentationRequest<ExtensionUpdateResult> presentation)
        => ExtensionUpdateHumanRenderer.Render(presentation);

    internal static string? RenderDiagnostic(CliPresentationRequest<ExtensionUpdateResult> presentation)
        => $"status={CliStatusDefinitions.Read(presentation.Result.Status).MachineName}; findings={presentation.Result.Findings.Count.ToString(CultureInfo.InvariantCulture)}";

}
