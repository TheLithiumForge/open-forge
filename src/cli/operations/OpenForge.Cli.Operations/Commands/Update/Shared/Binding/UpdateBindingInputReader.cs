using System.CommandLine;
using OpenForge.Cli.Core.Commands.Update.Models.Binding;
using OpenForge.Cli.Core.Commands.Update.Models.Request;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Binding;

internal static class UpdateBindingInputReader
{
    internal static UpdateBindingInput Read(
        ParseResult result,
        UpdateSymbols symbols)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(symbols);
        return new UpdateBindingInput(
            Force: result.GetValue(symbols.Force),
            Prune: result.GetValue(symbols.Prune),
            Automatic: result.GetValue(symbols.Automatic),
            Mode: result.GetValue(symbols.DryRun)
                ? UpdateMode.DryRun
                : UpdateMode.Apply,
            AllowsInteractiveConfirmation: false);
    }
}
