using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Binding;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;

namespace OpenForge.Cli.Core.Commands.Install;

internal sealed class InstallRequestBinder(InstallSymbols symbols)
{
    private readonly InstallSymbols _symbols = symbols;

    internal CliBindResult<InstallRequest, InstallResult> Bind(
        CliBindingParse parse,
        CliInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(parse);
        ArgumentNullException.ThrowIfNull(invocation);

        var input = InstallBindingInputReader.Read(parse.Result, _symbols);
        if (invocation.Workspace is not { } workspace)
        {
            return CliBindResult<InstallRequest, InstallResult>.Invalid(
                InstallResult.Invalid(
                    input,
                    workspace: null,
                    findings:
                    [
                        new InstallFinding(
                            InstallFindingCode.WorkspaceUnavailable,
                            "The selected workspace is unavailable."),
                    ]));
        }

        var allowsInteractiveConfirmation = invocation.Presentation.Format == CliOutputFormat.Human
            && !input.Automatic
            && input.Mode == InstallMode.Apply;
        return CliBindResult<InstallRequest, InstallResult>.Bound(
            new InstallRequest(
                workspace: workspace,
                mode: input.Mode,
                force: input.Force,
                automatic: input.Automatic,
                allowsInteractiveConfirmation: allowsInteractiveConfirmation));
    }
}
