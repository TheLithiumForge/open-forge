using OpenForge.Cli.Core.Commands.Extension.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Selection;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning;

internal sealed class ExtensionUpdateResultFactsInput
{
    internal required ExtensionUpdateSelection Selection { get; init; }

    internal required ExtensionUpdateSource Source { get; init; }

    internal required IReadOnlyList<ExtensionUpdatePackage> Packages { get; init; }

    internal required IReadOnlyList<ExtensionUpdateComparison> Comparisons { get; init; }

    internal required ExtensionUpdateTopology Topology { get; init; }

    internal required IReadOnlyList<ExtensionUpdateEffect> Effects { get; init; }

    internal required ExtensionUpdateLifecycleAction LifecycleAction { get; init; }

    internal required ExtensionUpdateLifecycleOutcome LifecycleOutcome { get; init; }

    internal required ExtensionUpdateRecoveryState RecoveryState { get; init; }

    internal required ExtensionUpdateVerificationState Verification { get; init; }
}
