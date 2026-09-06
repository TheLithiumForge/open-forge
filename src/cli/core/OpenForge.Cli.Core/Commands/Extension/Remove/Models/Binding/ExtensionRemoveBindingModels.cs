using System.CommandLine;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Binding;

internal sealed record ExtensionRemoveSymbols(
    Command Command,
    Argument<string[]> StableIds,
    Option<bool> Prune,
    Option<bool> Automatic,
    Option<bool> DryRun);

internal sealed record ExtensionRemoveBindingInput(
    ExtensionRemoveMode Mode,
    bool Prune,
    bool Automatic);
