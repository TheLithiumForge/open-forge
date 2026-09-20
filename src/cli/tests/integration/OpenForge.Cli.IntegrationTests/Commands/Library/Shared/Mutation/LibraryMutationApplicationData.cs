using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

internal static class LibraryMutationApplicationData
{
    internal const string ManagedPath = ".agents/framework-owned.md";
    private const string ManagedBody = "---\nopen-forge:\n  description: Unrelated Framework file\n  tags: [Framework]\n---\n# Unrelated Framework file\n";

    internal static void FrameworkFile(LibraryMutationWorkspace workspace)
        => workspace.Write(ManagedPath, ManagedBody);

    internal static LibraryRegistrationRead ReadRecord(LibraryMutationWorkspace workspace, bool exists, bool registered)
    {
        var path = workspace.Absolute(LibraryMutationWorkspace.RecordPath);
        return new LibraryRegistrationRead
        {
            State = exists ? LibraryRegistrationReadState.Complete : LibraryRegistrationReadState.Missing,
            Record = exists ? Record(registered) : null,
            Snapshot = exists ? FileStateSnapshot.File(path, path, File.ReadAllBytes(path)) : FileStateSnapshot.Missing(path),
            Cause = null,
        };
    }

    internal static LibraryRegistrationSet Record(bool registered)
        => LibraryRegistrationSet.Create([LibraryRegistration.Create(LibraryId.Create("team-knowledge"),
            WorkspaceRelativeDirectory.Create(LibraryMutationWorkspace.SourceRoot), LibraryDestinationRoot.Create("."),
            registered ? [SourceRelativeEligiblePath.Create(LibraryMutationWorkspace.Leaf)] : [])]);

    internal static LibraryInventoryRead Inventory(LibraryMutationWorkspace workspace)
    {
        var root = WorkspaceRelativeDirectory.Create(LibraryMutationWorkspace.SourceRoot);
        var sourceRoot = workspace.Absolute(LibraryMutationWorkspace.SourceRoot);
        return new LibraryInventoryRead
        {
            Source = new LibrarySourceRootObservation
            {
                Request = new LibrarySourceRootRequest { Workspace = workspace.Workspace, SourceRoot = root },
                State = LibrarySourceRootState.Available,
                LexicalSourceRoot = workspace.Absolute(LibraryMutationWorkspace.SourceRoot),
                PhysicalSourceRoot = workspace.Absolute(LibraryMutationWorkspace.SourceRoot),
                LexicallyContained = true,
                PhysicallyContained = true,
                Cause = null,
            },
            Inventory = LibraryInventory.Complete(root, sourceRoot, [EligibleSourceFile.Create(
                SourceRelativeEligiblePath.Create(LibraryMutationWorkspace.Leaf),
                workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/{LibraryMutationWorkspace.Leaf}"))]),
            ExcludedPaths = [],
            UnavailablePaths = [],
        };
    }

    internal static LibraryConsumerBoundaryFacts Boundary(LibraryMutationWorkspace workspace)
        => new()
        {
            Request = new LibraryConsumerBoundaryRequest
            {
                Workspace = workspace.Workspace,
                RequiredAncestorPaths = [CanonicalRelativePath.Create(".agents/directives")],
            },
            ConsumerRoot = Directory(workspace.Absolute(".agents")),
            Ancestors = [Directory(workspace.Absolute(".agents/directives"))],
            IsComplete = true,
        };

    internal static ImmutableArray<LibraryMappingObservation> Mapping(LibraryMutationWorkspace workspace, bool linked)
    {
        var mapping = LibraryMapping.Create(WorkspaceRelativeDirectory.Create(LibraryMutationWorkspace.SourceRoot), LibraryDestinationRoot.Create("."),
            SourceRelativeEligiblePath.Create(LibraryMutationWorkspace.Leaf));
        return [LibraryMappingObservation.Create(mapping, Leaf(workspace, linked),
            linked ? LibraryMappingObservationState.Current : LibraryMappingObservationState.Missing, cause: null)];
    }

    internal static RelativeFileLinkEffect Link(bool delete)
    {
        var path = CanonicalRelativePath.Create(LibraryMutationWorkspace.Leaf);
        var identity = RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../../shared/team-knowledge/.agents/directives/review.md");
        return delete ? RelativeFileLinkEffect.Delete(path, identity) : RelativeFileLinkEffect.Create(path, identity);
    }

    internal static NoFollowLeafObservation Leaf(LibraryMutationWorkspace workspace, bool linked)
        => linked ? NoFollowLeafObservation.CreateRelativeFileLink(workspace.Absolute(LibraryMutationWorkspace.Leaf), Link(delete: true).Link)
            : NoFollowLeafObservation.Missing(workspace.Absolute(LibraryMutationWorkspace.Leaf));

    internal static WorkspaceOwnershipRead WorkspaceOwnership(LibraryMutationWorkspace workspace)
    {
        var path = workspace.Absolute(LibraryMutationWorkspace.OwnershipPath);
        if (!File.Exists(path))
        {
            return WorkspaceOwnershipRead.Absent(path);
        }
        var bytes = File.ReadAllBytes(path);
        var decoded = OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization.WorkspaceOwnershipCodec.Read(bytes);
        return new(WorkspaceOwnershipReadState.Complete,
            decoded.Document ?? throw new InvalidOperationException("Expected test ownership."),
            path, FileStateSnapshot.File(path, path, bytes), null);
    }

    private static LibraryConsumerDirectoryObservation Directory(string path)
        => new(NoFollowLeafObservation.Directory(path), PhysicalPathResolution.Contained(path, path));
}
