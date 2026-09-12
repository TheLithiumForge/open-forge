using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Install;

internal static class InstallBinding
{
    internal static InstallSymbols CreateSymbols() => InstallSymbols.Create();

    internal static CliCommandBinding<InstallRequest, InstallResult> Close(
        InstallSymbols symbols,
        CliHelpContent help,
        CliOperation<InstallRequest, InstallResult> operation,
        CliRendererSet<InstallResult> renderers,
        CliDiagnosticRenderer<InstallResult>? diagnosticRenderer)
    {
        var binder = new InstallRequestBinder(symbols);
        var invalidResultFactory = new InstallWorkspaceResultFactory(symbols);
        return new CliCommandBinding<InstallRequest, InstallResult>(
            symbols.InstallCommand,
            new CliCommandBindingComponents<InstallRequest, InstallResult>
            {
                Help = help,
                WorkspaceRequirement = CliWorkspaceRequirement.Required,
                Binder = binder.Bind,
                InvalidResultFactory = invalidResultFactory.Create,
                Operation = operation,
                Renderers = renderers,
                DiagnosticRenderer = diagnosticRenderer,
            });
    }
}
