using OpenForge.Cli.Core.Commands.Library.Attach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

internal static class LibraryMutationCompletionData
{
    internal static LibraryExecutionEvidence Empty()
        => new()
        {
            Permission = null,
            Directories = [],
            Links = [],
            GeneratedRegions = [],
            Record = null,
            RecoveryPreparation = null,
            RecoveryCleanup = null,
            Cancellation = null,
            UnexpectedFailure = null,
            SourceEffectScope = null,
            RecordPublicationOrder = LibraryRecordPublicationOrder.NotObserved,
        };

    internal static LibraryExecutionEvidence Verified(RelativeFileLinkEffect link, PlannedFileChange record, FileStateSnapshot before)
    {
        var path = LibraryMutationPlanningData.Absolute(LibraryMutationPlanningData.Leaf);
        var missing = NoFollowLeafObservation.Missing(path);
        var linked = NoFollowLeafObservation.CreateRelativeFileLink(path, link.Link);
        var recordAfter = record.Kind == PlannedFileChangeKind.Delete
            ? FileStateSnapshot.Missing(record.LogicalPath)
            : FileStateSnapshot.File(record.LogicalPath, record.LogicalPath, record.IntendedBytes.AsSpan());
        return Empty() with
        {
            Links = [RelativeFileLinkReceipt.Verified(link,
                link.Kind == RelativeFileLinkEffectKind.Delete ? linked : missing,
                link.Kind == RelativeFileLinkEffectKind.Delete ? missing : linked)],
            Record = FileChangeReceipt.Verified(record, before, recordAfter),
            RecoveryCleanup = RecoveryBundleDeletionResult.Deleted(),
            RecordPublicationOrder = LibraryRecordPublicationOrder.Last,
            SourceEffectScope = new LibrarySourceEffectScopeFacts
            {
                ProtectedSourceRoots = [WorkspaceRelativeDirectory.Create(LibraryMutationPlanningData.SourceRoot)],
                AttemptedMutationTargets = [CanonicalRelativePath.Create(LibraryMutationPlanningData.Leaf),
                    CanonicalRelativePath.Create(LibraryMutationPlanningData.RecordPath)],
                IsComplete = true,
            },
        };
    }

    private static RelativeFileLinkEffect Link(bool delete)
    {
        var path = CanonicalRelativePath.Create(LibraryMutationPlanningData.Leaf);
        var identity = RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../../shared/team-knowledge/.agents/directives/review.md");
        return delete ? RelativeFileLinkEffect.Delete(path, identity) : RelativeFileLinkEffect.Create(path, identity);
    }

    internal static LibraryAttachCompletionInput Attach()
    {
        var observations = LibraryMutationPlanningData.Attach(LibraryMutationPlanningData.Leaf);
        var before = Assert.IsType<FileStateSnapshot>(LibraryMutationPlanningData.MissingRecord().Snapshot);
        var record = PlannedFileChange.Create(before.Expectation, LibraryMutationPlanningData.RecordBytes(LibraryMutationPlanningData.Leaf));
        var link = Link(delete: false);
        var plan = new LibraryAttachPlan
        {
            Permissions = null,
            Input = observations,
            State = LibraryPlanState.Complete,
            Directories = [],
            Links = [link],
            GeneratedRegions = [],
            OwnershipChange = record,
            IntendedRecord = LibraryMutationPlanningData.Record(LibraryMutationPlanningData.Leaf).Record,
            Findings = [],
        };
        return new LibraryAttachCompletionInput
        {
            Request = observations.Request,
            Plan = plan,
            Observations = observations,
            Execution = Verified(link, record, before),
        };
    }

    internal static LibrarySyncCompletionInput Sync()
    {
        var observations = LibraryMutationPlanningData.Sync([LibraryMutationPlanningData.Leaf], []);
        var before = Assert.IsType<FileStateSnapshot>(LibraryMutationPlanningData.Record().Snapshot);
        var record = PlannedFileChange.Replace(before.Expectation, LibraryMutationPlanningData.RecordBytes(LibraryMutationPlanningData.Leaf));
        var link = Link(delete: false);
        var plan = new LibrarySyncPlan
        {
            Permissions = null,
            Input = observations,
            State = LibraryPlanState.Complete,
            Directories = [],
            Links = [link],
            GeneratedRegions = [],
            OwnershipChange = record,
            IntendedRecord = LibraryMutationPlanningData.Record(LibraryMutationPlanningData.Leaf).Record,
            Findings = [],
        };
        return new LibrarySyncCompletionInput
        {
            Request = observations.Request,
            Plan = plan,
            Observations = observations,
            Execution = Verified(link, record, before),
        };
    }

    internal static LibraryDetachCompletionInput Detach()
    {
        var observations = LibraryMutationPlanningData.Detach(LibraryMutationPlanningData.Leaf);
        var before = Assert.IsType<FileStateSnapshot>(LibraryMutationPlanningData.Record(LibraryMutationPlanningData.Leaf).Snapshot);
        var record = PlannedFileChange.Replace(before.Expectation, System.Text.Encoding.UTF8.GetBytes("{\"schemaVersion\":1,\"framework\":null,\"extensions\":[],\"libraries\":[]}"));
        var link = Link(delete: true);
        var plan = new LibraryDetachPlan
        {
            Permissions = null,
            Input = observations,
            State = LibraryPlanState.Complete,
            Directories = [],
            Links = [link],
            GeneratedRegions = [],
            OwnershipChange = record,
            IntendedRecord = null,
            Findings = [],
        };
        return new LibraryDetachCompletionInput
        {
            Request = observations.Request,
            Plan = plan,
            Observations = observations,
            Execution = Verified(link, record, before),
        };
    }
}
