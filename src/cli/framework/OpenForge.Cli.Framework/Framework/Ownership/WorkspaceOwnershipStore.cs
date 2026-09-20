using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;

namespace OpenForge.Cli.Core.Framework.Ownership;

internal sealed class WorkspaceOwnershipStore
{
    internal OwnershipWritePlanResult PlanFrameworkOwnership(
        WorkspaceOwnershipRead current,
        FrameworkOwnership intended)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(intended);

        var snapshot = current.Snapshot;
        if (snapshot is null)
        {
            return OwnershipWritePlanResult.Skipped(
                current.Cause
                    ?? "The workspace ownership lock could not be read, so its write was skipped.");
        }

        var basis = current.State == WorkspaceOwnershipReadState.Complete
            ? current.Document
            : WorkspaceOwnershipDocument.Empty;
        var candidate = basis with { Framework = intended };
        var bytes = WorkspaceOwnershipCodec.Write(candidate);
        if (snapshot.HasBytes && snapshot.Bytes.AsSpan().SequenceEqual(bytes))
        {
            return OwnershipWritePlanResult.Unchanged();
        }

        var change = snapshot.Kind == FileExpectationKind.Missing
            ? PlannedFileChange.Create(snapshot.Expectation, bytes)
            : PlannedFileChange.Replace(snapshot.Expectation, bytes);
        return OwnershipWritePlanResult.Planned(change);
    }

    internal OwnershipWritePlanResult PlanExtensionOwnership(
        WorkspaceOwnershipRead current,
        ImmutableArray<ExtensionOwnership> intended)
    {
        ArgumentNullException.ThrowIfNull(current);

        var snapshot = current.Snapshot;
        if (snapshot is null)
        {
            return OwnershipWritePlanResult.Skipped(
                current.Cause
                    ?? "The workspace ownership lock could not be read, so its write was skipped.");
        }

        var basis = current.State == WorkspaceOwnershipReadState.Complete
            ? current.Document
            : WorkspaceOwnershipDocument.Empty;
        var candidate = basis with { Extensions = intended };
        var bytes = WorkspaceOwnershipCodec.Write(candidate);
        if (snapshot.HasBytes && snapshot.Bytes.AsSpan().SequenceEqual(bytes))
        {
            return OwnershipWritePlanResult.Unchanged();
        }

        var change = snapshot.Kind == FileExpectationKind.Missing
            ? PlannedFileChange.Create(snapshot.Expectation, bytes)
            : PlannedFileChange.Replace(snapshot.Expectation, bytes);
        return OwnershipWritePlanResult.Planned(change);
    }

    internal OwnershipWritePlanResult PlanLibraryOwnership(
        WorkspaceOwnershipRead current,
        ImmutableArray<LibraryOwnership> intended)
    {
        ArgumentNullException.ThrowIfNull(current);

        var snapshot = current.Snapshot;
        if (snapshot is null)
        {
            return OwnershipWritePlanResult.Skipped(
                current.Cause
                    ?? "The workspace ownership lock could not be read, so its write was skipped.");
        }

        var basis = current.State == WorkspaceOwnershipReadState.Complete
            ? current.Document
            : WorkspaceOwnershipDocument.Empty;
        var candidate = basis with { Libraries = intended };
        var bytes = WorkspaceOwnershipCodec.Write(candidate);
        if (snapshot.HasBytes && snapshot.Bytes.AsSpan().SequenceEqual(bytes))
        {
            return OwnershipWritePlanResult.Unchanged();
        }

        var change = snapshot.Kind == FileExpectationKind.Missing
            ? PlannedFileChange.Create(snapshot.Expectation, bytes)
            : PlannedFileChange.Replace(snapshot.Expectation, bytes);
        return OwnershipWritePlanResult.Planned(change);
    }
}
