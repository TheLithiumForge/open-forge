using System.Globalization;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Rendering;

internal static class ExtensionInstallPresentation
{
    internal static CliHelpContent CreateHelp()
        => new(
        [
            new CliHelpSection(
                "Syntax",
                "  open-forge extension install [<stable-id>...] [--source <package-or-catalogue-path>] [--all] [--force] [--automatic] [--dry-run] [global options]"),
            new CliHelpSection(
                "Selection and dependencies",
                "  Select exact stable IDs, --all, or the sole package in a one-package source. Dependencies are mandatory and installed first."),
            new CliHelpSection(
                "Interaction and automatic mode",
                "  Interactive text requests can offer a list of packages to choose from. --automatic and non-interactive requests do not choose packages or enable --force."),
            new CliHelpSection(
                "Initial force",
                "  --force replaces only eligible existing content during initial installation. Use extension update for changes to managed content."),
            new CliHelpSection(
                "Results and streams",
                "  Dry-run writes nothing. Human and JSON output preserve the shared semantic status, stream, and exit mapping."),
        ]);

    internal static string RenderHuman(CliPresentationRequest<ExtensionInstallResult> presentation)
        => ExtensionInstallHumanRenderer.Render(presentation);

    internal static string? RenderDiagnostic(CliPresentationRequest<ExtensionInstallResult> presentation)
        => $"status={CliStatusDefinitions.Read(presentation.Result.Status).MachineName}; findings={presentation.Result.Findings.Count.ToString(CultureInfo.InvariantCulture)}";

}
