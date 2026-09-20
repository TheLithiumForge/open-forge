using System.CommandLine;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Models.Binding;

internal sealed record ExtensionUpdateSymbols(
    Command Command,
    Argument<string[]> StableIds,
    Option<string?> Source,
    Option<bool> All,
    Option<bool> Force,
    Option<bool> Prune,
    Option<bool> Automatic,
    Option<bool> DryRun,
    Option<string[]> AllowPath);

internal sealed record ExtensionUpdateBindingInput(
    ExtensionUpdateMode Mode,
    bool Force,
    bool Prune,
    bool Automatic);
