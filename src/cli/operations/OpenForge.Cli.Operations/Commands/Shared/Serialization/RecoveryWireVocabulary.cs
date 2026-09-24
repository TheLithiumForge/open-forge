using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;

namespace OpenForge.Cli.Core.Commands.Shared.Serialization;

/// <summary>
/// Owns the wire names for recovery facts. Cleanup, Doctor, Repair, and Status
/// all report the same bundles, so they report them with the same words.
/// </summary>
internal static class RecoveryWireVocabulary
{
    internal static string Producer(RecoveryBundleProducer value)
        => value switch
        {
            RecoveryBundleProducer.Framework => "framework",
            RecoveryBundleProducer.Extension => "extension",
            RecoveryBundleProducer.Index => "index",
            RecoveryBundleProducer.Route => "route",
            RecoveryBundleProducer.Repair => "repair",
            RecoveryBundleProducer.Library => "library",
            RecoveryBundleProducer.Workspace => "workspace",
            _ => Undefined(nameof(value), value),
        };

    internal static string Operation(RecoveryBundleOperation value)
        => value switch
        {
            RecoveryBundleOperation.Install => "install",
            RecoveryBundleOperation.Index => "index",
            RecoveryBundleOperation.Create => "create",
            RecoveryBundleOperation.Init => "init",
            RecoveryBundleOperation.Move => "move",
            RecoveryBundleOperation.Update => "update",
            RecoveryBundleOperation.Remove => "remove",
            RecoveryBundleOperation.Repair => "repair",
            RecoveryBundleOperation.Attach => "attach",
            RecoveryBundleOperation.Sync => "sync",
            RecoveryBundleOperation.Detach => "detach",
            _ => Undefined(nameof(value), value),
        };

    internal static string CandidateKind(RecoveryBundleCandidateKind value)
        => value switch
        {
            RecoveryBundleCandidateKind.Final => "final",
            RecoveryBundleCandidateKind.Draft => "draft",
            _ => Undefined(nameof(value), value),
        };

    internal static string Integrity(RecoveryBundleIntegrity value)
        => value switch
        {
            RecoveryBundleIntegrity.Verified => "verified",
            RecoveryBundleIntegrity.Malformed => "malformed",
            RecoveryBundleIntegrity.Unsupported => "unsupported",
            RecoveryBundleIntegrity.Unavailable => "unavailable",
            RecoveryBundleIntegrity.Incomplete => "incomplete",
            _ => Undefined(nameof(value), value),
        };

    internal static string TargetComparison(RecoveryBundleTargetComparisonState value)
        => value switch
        {
            RecoveryBundleTargetComparisonState.Prior => "prior",
            RecoveryBundleTargetComparisonState.Intended => "intended",
            RecoveryBundleTargetComparisonState.Third => "third",
            RecoveryBundleTargetComparisonState.Unavailable => "unavailable",
            RecoveryBundleTargetComparisonState.Blocked => "blocked",
            _ => Undefined(nameof(value), value),
        };

    private static string Undefined<T>(string name, T value)
        where T : struct, Enum
        => throw new ArgumentOutOfRangeException(
            name,
            value,
            "The recovery wire value is not defined.");
}
