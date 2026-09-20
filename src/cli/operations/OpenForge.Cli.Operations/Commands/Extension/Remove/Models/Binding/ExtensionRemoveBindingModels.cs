using System.CommandLine;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Binding;

internal sealed record ExtensionRemoveSymbols(
    Command Command,
    Argument<string[]> StableIds,
    Option<bool> Automatic,
    Option<bool> DryRun,
    Option<string[]> AllowPath);

internal sealed record ExtensionRemoveBindingInput(
    ExtensionRemoveMode Mode,
    bool Automatic);
