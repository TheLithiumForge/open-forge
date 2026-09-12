using System.Globalization;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Rendering;

internal static class ExtensionRemovePresentation
{
    internal static CliHelpContent CreateHelp()
        => new(
        [
            new CliHelpSection(
                "Syntax",
                "  open-forge extension remove [<stable-id>...] [--prune] [--automatic] [--dry-run] [global options]"),
            new CliHelpSection(
                "Selection and dependencies",
                """
                  Select exact managed stable IDs, or choose from the interactive package list.
                  A dependency cannot be removed while a retained package needs it. Unused dependencies remain registered.
                """),
            new CliHelpSection(
                "Ownership and changed content",
                """
                  Shared paths remain owned by retained packages.
                  When the last owner is removed, unchanged files may be deleted. Changed files stay unmanaged
                  unless this request includes --prune.
                """),
            new CliHelpSection(
                "Results and streams",
                """
                  Dry-run writes nothing.
                  Human and JSON output preserve the shared semantic status, stream, recovery, and next-action mapping.
                """),
        ]);

    internal static string RenderHuman(CliPresentationRequest<ExtensionRemoveResult> presentation)
        => ExtensionRemoveHumanRenderer.Render(presentation);

    internal static string? RenderDiagnostic(
        CliPresentationRequest<ExtensionRemoveResult> presentation)
    {
        var status = CliStatusDefinitions.Read(presentation.Result.Status).MachineName;
        var findings = presentation.Result.Findings.Count.ToString(CultureInfo.InvariantCulture);
        return $"status={status}; findings={findings}";
    }

}
