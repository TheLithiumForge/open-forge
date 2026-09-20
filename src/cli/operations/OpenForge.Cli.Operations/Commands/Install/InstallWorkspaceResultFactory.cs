using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Binding;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Install;

internal sealed class InstallWorkspaceResultFactory(InstallSymbols symbols)
{
    private readonly InstallSymbols _symbols = symbols;

    internal InstallResult Create(CliInvalidBindingInput input)
    {
        var parsed = InstallBindingInputReader.Read(input.BindingParse.Result, _symbols);
        var cause = input.InvalidInput.Diagnostics.Count == 1
            ? input.InvalidInput.Diagnostics[0]
            : string.Join(" ", input.InvalidInput.Diagnostics);
        return InstallResult.Invalid(
            parsed,
            workspace: null,
            findings:
            [
                new InstallFinding(
                    InstallFindingCode.WorkspaceUnavailable,
                    cause,
                    input.GlobalInput.WorkspaceValue ?? input.ProcessEnvironment.CurrentDirectory),
            ]);
    }
}
