using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

internal static class LibraryMutationPlanningData
{
    internal const string Id = "team-knowledge";
    internal const string SourceRoot = "shared/team-knowledge";
    internal const string Leaf = ".agents/directives/review.md";
    internal const string RecordPath = ".agents/open-forge.libraries.json";
    internal static string Root { get; } = Path.GetFullPath("library-planning-unit");
    internal static CliWorkspace Workspace { get; } = new(Root, Root, CliWorkspaceSelectionMethod.CurrentDirectory);
    internal static string Absolute(string path) => Path.Combine(Root, path);

    internal static LibraryAttachPlanningInput Attach(params string[] current)
        => new()
        {
            Request = new LibraryAttachRequest
            {
                Workspace = Workspace,
                LibraryId = LibraryId.Create(Id),
                SourceRoot = WorkspaceRelativeDirectory.Create(SourceRoot),
                Mode = LibraryMode.Apply,
            },
            Record = MissingRecord(),
            Source = Inventory(current),
            Mappings = [.. current.Select(path => Mapping(path, LibraryMappingObservationState.Missing))],
            ConsumerBoundary = Boundary(),
            Ownership = Ownership(),
            GeneratedRegionChanges = [],
        };

    internal static LibrarySyncPlanningInput Sync(string[] current, string[] registered)
        => new()
        {
            Request = new LibrarySyncRequest { Workspace = Workspace, LibraryId = LibraryId.Create(Id), Mode = LibraryMode.Apply },
            Record = Record(registered),
            Source = Inventory(current),
            Mappings = [.. current.Union(registered).Order(StringComparer.Ordinal)
                .Select(path => Mapping(path, registered.Contains(path) ? LibraryMappingObservationState.Current : LibraryMappingObservationState.Missing))],
            ConsumerBoundary = Boundary(),
            Ownership = Ownership(),
            GeneratedRegionChanges = [],
        };

    internal static LibraryDetachPlanningInput Detach(params string[] registered)
        => new()
        {
            Request = new LibraryDetachRequest { Workspace = Workspace, LibraryId = LibraryId.Create(Id), Mode = LibraryMode.Apply },
            Record = Record(registered),
            Mappings = [.. registered.Select(path => Mapping(path, LibraryMappingObservationState.Current))],
            ConsumerBoundary = Boundary(),
            Ownership = Ownership(),
            GeneratedRegionChanges = [],
        };

    internal static LibrariesRecordRead MissingRecord()
        => new() { State = LibrariesRecordReadState.Missing, Record = null, Snapshot = FileStateSnapshot.Missing(Absolute(RecordPath)), Cause = null };

    internal static LibrariesRecordRead Record(params string[] paths)
        => new()
        {
            State = LibrariesRecordReadState.Complete,
            Record = LibrariesRecord.Create([LibraryRecord.Create(LibraryId.Create(Id), WorkspaceRelativeDirectory.Create(SourceRoot),
                [.. paths.Order(StringComparer.Ordinal).Select(SourceRelativeEligiblePath.Create)])]),
            Snapshot = FileStateSnapshot.File(Absolute(RecordPath), Absolute(RecordPath), RecordBytes(paths)),
            Cause = null,
        };

    internal static byte[] RecordBytes(params string[] paths)
    {
        var values = string.Join(",", paths.Order(StringComparer.Ordinal).Select(path => $"\"{path}\""));
        return Encoding.UTF8.GetBytes($$"""
            {"schemaVersion":1,"libraries":[{"id":"team-knowledge","sourceRoot":"shared/team-knowledge","paths":[{{values}}]}]}
            """);
    }

    internal static LibraryConsumerBoundaryFacts Boundary()
        => new()
        {
            Request = new LibraryConsumerBoundaryRequest
            {
                Workspace = Workspace,
                RequiredAncestorPaths = [CanonicalRelativePath.Create(".agents/directives")],
            },
            ConsumerRoot = DirectoryObservation(".agents"),
            Ancestors = [DirectoryObservation(".agents/directives")],
            IsComplete = true,
        };

    internal static LibraryConsumerDirectoryObservation DirectoryObservation(string path, bool missing = false)
    {
        var absolute = Absolute(path);
        return missing
            ? new(NoFollowLeafObservation.Missing(absolute), PhysicalPathResolution.Classified(PhysicalPathState.Missing, absolute))
            : new(NoFollowLeafObservation.Directory(absolute), PhysicalPathResolution.Contained(absolute, absolute));
    }

    internal static LibraryInventoryRead Inventory(params string[] paths)
    {
        var root = WorkspaceRelativeDirectory.Create(SourceRoot);
        var agents = Absolute($"{SourceRoot}/.agents");
        return new LibraryInventoryRead
        {
            Source = new LibrarySourceRootObservation
            {
                Request = new LibrarySourceRootRequest { Workspace = Workspace, SourceRoot = root },
                State = LibrarySourceRootState.Available,
                LexicalSourceRoot = Absolute(SourceRoot),
                PhysicalSourceRoot = Absolute(SourceRoot),
                PhysicalAgentsDirectory = agents,
                LexicallyContained = true,
                PhysicallyContained = true,
                PhysicallyDisjoint = true,
                Condition = LibrarySourceRootCondition.None,
                Cause = null,
            },
            Inventory = LibraryInventory.Complete(root, agents,
                [.. paths.Order(StringComparer.Ordinal).Select(path => EligibleSourceFile.Create(
                    SourceRelativeEligiblePath.Create(path), Absolute($"{SourceRoot}/{path}")))]),
            ExcludedPaths = [],
            UnavailablePaths = [],
        };
    }

    internal static LibraryMappingObservation Mapping(string path, LibraryMappingObservationState state)
    {
        var mapping = LibraryMapping.Create(WorkspaceRelativeDirectory.Create(SourceRoot), SourceRelativeEligiblePath.Create(path));
        var leaf = state switch
        {
            LibraryMappingObservationState.Current => NoFollowLeafObservation.CreateRelativeFileLink(Absolute(path),
                RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, mapping.ExpectedRelativeLink.Value)),
            LibraryMappingObservationState.Missing => NoFollowLeafObservation.Missing(Absolute(path)),
            LibraryMappingObservationState.Changed => NoFollowLeafObservation.OrdinaryFile(Absolute(path)),
            LibraryMappingObservationState.Blocked => NoFollowLeafObservation.Classified(Absolute(path), NoFollowLeafState.ReparsePoint),
            LibraryMappingObservationState.Unavailable => NoFollowLeafObservation.Missing(Absolute(path)),
            _ => throw new ArgumentOutOfRangeException(nameof(state)),
        };
        var cause = state is LibraryMappingObservationState.Blocked or LibraryMappingObservationState.Unavailable ? "Observation unavailable or unsafe." : null;
        return LibraryMappingObservation.Create(mapping, leaf, state, cause);
    }

    internal static LifecycleOwnershipReadResult Ownership(params LifecycleOwnershipClaim[] claims)
        => new(
            framework: new(LifecycleOwnershipSection.Framework, LifecycleOwnershipReadState.Trusted, null),
            extensions: new(LifecycleOwnershipSection.Extensions, LifecycleOwnershipReadState.Trusted, null),
            claims: claims,
            lifecycleFileExpectation: FileExpectation.File(Absolute(".agents/open-forge.lifecycle.json"), Absolute(".agents/open-forge.lifecycle.json"), new string('a', 64)),
            findings: []);
}
