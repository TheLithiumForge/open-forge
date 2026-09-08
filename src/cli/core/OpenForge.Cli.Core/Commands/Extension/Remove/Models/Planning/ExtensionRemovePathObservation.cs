using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;

internal sealed record ExtensionRemovePathObservation
{
    internal required string Path { get; init; }

    internal required IReadOnlyList<string> SelectedOwnerIds { get; init; }

    internal required IReadOnlyList<string> RemainingOwnerIds { get; init; }

    internal required ExtensionRemovePathClassification Classification { get; init; }

    internal ExtensionRemoveLibraryBoundary? LibraryBoundary { get; init; }

    internal FileStateSnapshot? Snapshot { get; init; }

    internal ExtensionRemoveFinding? Boundary { get; init; }

    internal ExtensionRemovePathPlan ToPathPlan(ExtensionRemoveChangedContentPolicy policy)
        => new(
            Path,
            Classification,
            SelectedOwnerIds,
            RemainingOwnerIds,
            Classification switch
            {
                ExtensionRemovePathClassification.Shared => ExtensionRemovePathAction.RetainShared,
                ExtensionRemovePathClassification.UnchangedFinalOwner => ExtensionRemovePathAction.Delete,
                ExtensionRemovePathClassification.ChangedFinalOwner
                    when policy == ExtensionRemoveChangedContentPolicy.Delete => ExtensionRemovePathAction.Delete,
                ExtensionRemovePathClassification.ChangedFinalOwner => ExtensionRemovePathAction.KeepAsUnmanaged,
                ExtensionRemovePathClassification.Missing => ExtensionRemovePathAction.ReleaseOwnership,
                _ => throw new ArgumentOutOfRangeException(message: "The Extension Remove path classification is not defined.", innerException: null),
            })
        { LibraryBoundary = LibraryBoundary };

    internal static ExtensionRemovePathObservation Shared(
        string path,
        IReadOnlyList<string> selected,
        IReadOnlyList<string> remaining)
        => Create(path, selected, remaining, ExtensionRemovePathClassification.Shared, snapshot: null);

    internal static ExtensionRemovePathObservation Missing(string path, IReadOnlyList<string> selected)
        => Create(path, selected, [], ExtensionRemovePathClassification.Missing, snapshot: null);

    internal static ExtensionRemovePathObservation Existing(
        string path,
        IReadOnlyList<string> selected,
        ExtensionRemovePathClassification classification,
        FileStateSnapshot snapshot)
        => Create(path, selected, [], classification, snapshot);

    internal static ExtensionRemovePathObservation Stop(
        string path,
        ExtensionRemoveFindingCode code,
        string cause)
        => new()
        {
            Path = path,
            SelectedOwnerIds = [],
            RemainingOwnerIds = [],
            Classification = ExtensionRemovePathClassification.Missing,
            Snapshot = null,
            Boundary = new ExtensionRemoveFinding(code, cause, path),
        };

    private static ExtensionRemovePathObservation Create(
        string path,
        IReadOnlyList<string> selected,
        IReadOnlyList<string> remaining,
        ExtensionRemovePathClassification classification,
        FileStateSnapshot? snapshot)
        => new()
        {
            Path = path,
            SelectedOwnerIds = selected,
            RemainingOwnerIds = remaining,
            Classification = classification,
            Snapshot = snapshot,
            Boundary = null,
        };
}
