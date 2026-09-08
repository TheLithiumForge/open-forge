using System.Collections.Immutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

internal static class LibraryMutationApplicationData
{
    internal const string ManagedPath = ".agents/framework-owned.md";
    private const string ManagedBody = "---\nopen-forge:\n  description: Unrelated Framework file\n  tags: [Framework]\n---\n# Unrelated Framework file\n";

    internal static void Lifecycle(LibraryMutationWorkspace workspace)
    {
        workspace.Write(ManagedPath, ManagedBody);
        var fingerprint = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(ManagedBody)));
        workspace.Write(".agents/open-forge.lifecycle.json", $$$"""
            {"schemaVersion":1,"fingerprintPolicy":"open-forge-markdown-v1","workspacePath":"{{{JsonEncodedText.Encode(workspace.Path)}}}",
             "framework":{"coverage":"complete","source":{"id":"open-forge","version":"1.0.0","inventoryFingerprint":"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"},
              "targets":[{"path":".agents/framework-owned.md","sourceAssetPath":".agents/framework-owned.md","region":null,
                "baselineFingerprint":"{{{fingerprint}}}","fingerprintKind":"exact-bytes"}],"generatedRegions":[]},
             "extensions":{"coverage":"complete","packages":[],"paths":[]}}
            """);
    }

    internal static LibrariesRecordRead ReadRecord(LibraryMutationWorkspace workspace, bool exists, bool registered)
    {
        var path = workspace.Absolute(LibraryMutationWorkspace.RecordPath);
        return new LibrariesRecordRead
        {
            State = exists ? LibrariesRecordReadState.Complete : LibrariesRecordReadState.Missing,
            Record = exists ? Record(registered) : null,
            Snapshot = exists ? FileStateSnapshot.File(path, path, File.ReadAllBytes(path)) : FileStateSnapshot.Missing(path),
            Cause = null,
        };
    }

    internal static LibrariesRecord Record(bool registered)
        => LibrariesRecord.Create([LibraryRecord.Create(LibraryId.Create("team-knowledge"),
            WorkspaceRelativeDirectory.Create(LibraryMutationWorkspace.SourceRoot),
            registered ? [SourceRelativeEligiblePath.Create(LibraryMutationWorkspace.Leaf)] : [])]);

    internal static LibraryInventoryRead Inventory(LibraryMutationWorkspace workspace)
    {
        var root = WorkspaceRelativeDirectory.Create(LibraryMutationWorkspace.SourceRoot);
        var agents = workspace.Absolute($"{LibraryMutationWorkspace.SourceRoot}/.agents");
        return new LibraryInventoryRead
        {
            Source = new LibrarySourceRootObservation
            {
                Request = new LibrarySourceRootRequest { Workspace = workspace.Workspace, SourceRoot = root },
                State = LibrarySourceRootState.Available,
                LexicalSourceRoot = workspace.Absolute(LibraryMutationWorkspace.SourceRoot),
                PhysicalSourceRoot = workspace.Absolute(LibraryMutationWorkspace.SourceRoot),
                PhysicalAgentsDirectory = agents,
                LexicallyContained = true,
                PhysicallyContained = true,
                PhysicallyDisjoint = true,
                Condition = LibrarySourceRootCondition.None,
                Cause = null,
            },
            Inventory = LibraryInventory.Complete(root, agents, [EligibleSourceFile.Create(
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
        var mapping = LibraryMapping.Create(WorkspaceRelativeDirectory.Create(LibraryMutationWorkspace.SourceRoot),
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

    internal static LifecycleOwnershipReadResult Ownership(LibraryMutationWorkspace workspace)
        => new(
            framework: new(LifecycleOwnershipSection.Framework, LifecycleOwnershipReadState.Trusted, null),
            extensions: new(LifecycleOwnershipSection.Extensions, LifecycleOwnershipReadState.Trusted, null),
            claims: [new LifecycleOwnershipClaim(ManagedPath, LifecycleOwnershipManager.Framework, "open-forge")],
            lifecycleFileExpectation: FileStateSnapshot.File(workspace.Absolute(".agents/open-forge.lifecycle.json"),
                workspace.Absolute(".agents/open-forge.lifecycle.json"), File.ReadAllBytes(workspace.Absolute(".agents/open-forge.lifecycle.json"))).Expectation, findings: []);

    private static LibraryConsumerDirectoryObservation Directory(string path)
        => new(NoFollowLeafObservation.Directory(path), PhysicalPathResolution.Contained(path, path));
}
