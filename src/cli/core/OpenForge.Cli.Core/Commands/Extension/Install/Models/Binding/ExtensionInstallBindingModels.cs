using System.CommandLine;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Models.Binding;

internal sealed record ExtensionInstallSymbols(
    Command Command,
    Argument<string[]> StableIds,
    Option<string?> Source,
    Option<bool> All,
    Option<bool> Force,
    Option<bool> Automatic,
    Option<bool> DryRun);

internal sealed record ExtensionInstallBindingInput(
    ExtensionInstallMode Mode,
    bool Force,
    bool Automatic);
