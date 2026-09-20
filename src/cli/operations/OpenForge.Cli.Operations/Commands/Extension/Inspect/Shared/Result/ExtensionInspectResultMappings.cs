using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal static class ExtensionInspectResultMappings
{
    internal static ExtensionInspectSourceKind ReadSourceKind(ExtensionSourceKind kind)
        => kind switch
        {
            ExtensionSourceKind.EmbeddedCatalogue => ExtensionInspectSourceKind.EmbeddedCatalogue,
            ExtensionSourceKind.Package => ExtensionInspectSourceKind.Package,
            ExtensionSourceKind.Catalogue => ExtensionInspectSourceKind.Catalogue,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Extension source kind is not defined."),
        };

    internal static ExtensionInspectSourceState ReadSourceState(ExtensionSourceReadState state)
        => state switch
        {
            ExtensionSourceReadState.Complete => ExtensionInspectSourceState.Available,
            ExtensionSourceReadState.Missing => ExtensionInspectSourceState.Missing,
            ExtensionSourceReadState.Invalid => ExtensionInspectSourceState.Invalid,
            ExtensionSourceReadState.Blocked => ExtensionInspectSourceState.Blocked,
            ExtensionSourceReadState.Unavailable => ExtensionInspectSourceState.Unavailable,
            ExtensionSourceReadState.Cancelled => ExtensionInspectSourceState.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Extension source state is not defined."),
        };

    internal static ExtensionInspectLifecycleReadState ReadLifecycleState(WorkspaceOwnershipRead ownership)
        => ownership.State switch
        {
            WorkspaceOwnershipReadState.Complete => ExtensionInspectLifecycleReadState.Complete,
            WorkspaceOwnershipReadState.Absent => ExtensionInspectLifecycleReadState.Missing,
            WorkspaceOwnershipReadState.Invalid => ExtensionInspectLifecycleReadState.Invalid,
            WorkspaceOwnershipReadState.Unavailable => ExtensionInspectLifecycleReadState.Unavailable,
            _ => throw new ArgumentOutOfRangeException(nameof(ownership)),
        };

    internal static ExtensionInspectAvailableState ReadAvailableState(
        ExtensionSourceReadState state)
        => state switch
        {
            ExtensionSourceReadState.Complete => ExtensionInspectAvailableState.Present,
            ExtensionSourceReadState.Missing => ExtensionInspectAvailableState.Unavailable,
            ExtensionSourceReadState.Invalid => ExtensionInspectAvailableState.Invalid,
            ExtensionSourceReadState.Blocked => ExtensionInspectAvailableState.Blocked,
            ExtensionSourceReadState.Unavailable => ExtensionInspectAvailableState.Unavailable,
            ExtensionSourceReadState.Cancelled => ExtensionInspectAvailableState.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Extension source state is not defined."),
        };

    internal static ExtensionInspectPackageFileState ReadPackageFileState(
        ExtensionPackageFileReadState state)
        => state switch
        {
            ExtensionPackageFileReadState.Available => ExtensionInspectPackageFileState.Available,
            ExtensionPackageFileReadState.Missing => ExtensionInspectPackageFileState.Missing,
            ExtensionPackageFileReadState.Invalid => ExtensionInspectPackageFileState.Invalid,
            ExtensionPackageFileReadState.Blocked => ExtensionInspectPackageFileState.Blocked,
            ExtensionPackageFileReadState.Unavailable => ExtensionInspectPackageFileState.Unavailable,
            ExtensionPackageFileReadState.Cancelled => ExtensionInspectPackageFileState.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Extension package-file state is not defined."),
        };

    internal static ExtensionInspectDeclaredPathState ReadDeclaredPathState(
        ExtensionPackageFileReadState state)
        => state switch
        {
            ExtensionPackageFileReadState.Available => ExtensionInspectDeclaredPathState.Available,
            ExtensionPackageFileReadState.Missing => ExtensionInspectDeclaredPathState.Missing,
            ExtensionPackageFileReadState.Invalid => ExtensionInspectDeclaredPathState.Invalid,
            ExtensionPackageFileReadState.Blocked => ExtensionInspectDeclaredPathState.Blocked,
            ExtensionPackageFileReadState.Unavailable => ExtensionInspectDeclaredPathState.Unavailable,
            ExtensionPackageFileReadState.Cancelled => ExtensionInspectDeclaredPathState.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Extension package-file state is not defined."),
        };

    internal static ExtensionInspectComparisonSide Side(
        ExtensionInspectComparisonSideState state,
        IReadOnlyList<ExtensionInspectFingerprintFact> facts)
        => new()
        {
            State = state,
            Fingerprints = facts,
        };
}
