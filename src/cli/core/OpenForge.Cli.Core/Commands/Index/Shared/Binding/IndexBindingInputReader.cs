using System.CommandLine;
using OpenForge.Cli.Core.Commands.Index.Models.Binding;
using OpenForge.Cli.Core.Commands.Index.Models.Request;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Binding;

internal static class IndexBindingInputReader
{
    internal static IndexBindingInput Read(ParseResult result, IndexSymbols symbols)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(symbols);
        var sources = result.GetValue(symbols.Sources) ?? [];
        var mode = result.GetValue(symbols.DryRun)
            ? IndexMode.DryRun
            : IndexMode.Apply;
        return new IndexBindingInput(Array.AsReadOnly(sources.ToArray()), mode);
    }
}
