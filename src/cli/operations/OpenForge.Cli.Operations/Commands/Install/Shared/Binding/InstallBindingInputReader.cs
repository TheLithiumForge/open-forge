using System.CommandLine;
using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Request;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Binding;

internal static class InstallBindingInputReader
{
    internal static InstallBindingInput Read(
        ParseResult result,
        InstallSymbols symbols)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(symbols);

        var dryRun = result.GetValue(symbols.DryRun);
        return new InstallBindingInput(
            Force: result.GetValue(symbols.Force),
            Automatic: result.GetValue(symbols.Automatic),
            Mode: dryRun ? InstallMode.DryRun : InstallMode.Apply);
    }
}
