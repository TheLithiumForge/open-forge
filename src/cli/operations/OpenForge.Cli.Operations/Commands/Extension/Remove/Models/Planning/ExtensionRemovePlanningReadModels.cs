using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Selection;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;

internal sealed record ExtensionRemoveRecoveryRead(
    ExtensionRemovePlanBuild? Boundary,
    IReadOnlyList<ExtensionRemoveFinding>? Findings = null);

internal sealed record ExtensionRemoveSelectionRead(
    ExtensionRemoveSelection? Selection,
    ExtensionRemovePlanBuild? Boundary);
