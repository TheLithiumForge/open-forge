using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Request;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Mutation;

internal static class LibraryMutationPlanningData
{
    internal const string Id = "team-knowledge";
    internal const string SourceRoot = "shared/team-knowledge";
    internal const string Leaf = ".agents/directives/review.md";
    internal const string RecordPath = ".agents/open-forge.lock.json";
    internal static string Root { get; } = Path.GetFullPath(
        Path.Combine(Path.GetTempPath(), "open-forge-library-planning-unit"));
    internal static CliWorkspace Workspace { get; } = new(Root, Root, CliWorkspaceSelectionMethod.CurrentDirectory);
    internal static string Absolute(string path)
        => Path.GetFullPath(Path.Combine(Root, path.Replace('/', Path.DirectorySeparatorChar)));

    internal static LibraryAttachPlanningInput Attach(params string[] current)
        => new()
        {
            Request = new LibraryAttachRequest
            {
                AllowPrompt = false,
                Automatic = true,
                DestinationRoot = LibraryDestinationRoot.Create("."),
                Workspace = Workspace,
                LibraryId = LibraryId.Create(Id),
                SourceRoot = WorkspaceRelativeDirectory.Create(SourceRoot),
                Mode = LibraryMode.Apply,
            },
            Record = MissingRecord(),
            Source = Inventory(current),
            Mappings = [.. current.Select(path => Mapping(path, LibraryMappingObservationState.Missing))],
            ConsumerBoundary = Boundary(),
            Ownership = WorkspaceOwnership(),
            GeneratedRegionChanges = [],
        };

    internal static LibrarySyncPlanningInput Sync(string[] current, string[] registered)
        => new()
        {
            Request = new LibrarySyncRequest { AllowPrompt = false, Automatic = true, Workspace = Workspace, LibraryId = LibraryId.Create(Id), Mode = LibraryMode.Apply },
            Record = Record(registered),
            Source = Inventory(current),
            Mappings = [.. current.Union(registered).Order(StringComparer.Ordinal)
                .Select(path => Mapping(path, registered.Contains(path) ? LibraryMappingObservationState.Current : LibraryMappingObservationState.Missing))],
            ConsumerBoundary = Boundary(),
            Ownership = RegisteredOwnership(registered),
            GeneratedRegionChanges = [],
        };

    internal static LibraryDetachPlanningInput Detach(params string[] registered)
        => new()
        {
            Request = new LibraryDetachRequest { AllowPrompt = false, Automatic = true, Workspace = Workspace, LibraryId = LibraryId.Create(Id), Mode = LibraryMode.Apply },
            Record = Record(registered),
            Mappings = [.. registered.Select(path => Mapping(path, LibraryMappingObservationState.Current))],
            ConsumerBoundary = Boundary(),
            Ownership = RegisteredOwnership(registered),
            GeneratedRegionChanges = [],
        };

    internal static LibraryRegistrationRead MissingRecord()
        => new() { State = LibraryRegistrationReadState.Missing, Record = null, Snapshot = FileStateSnapshot.Missing(Absolute(RecordPath)), Cause = null };

    internal static LibraryRegistrationRead Record(params string[] paths)
        => new()
        {
            State = LibraryRegistrationReadState.Complete,
            Record = LibraryRegistrationSet.Create([LibraryRegistration.Create(LibraryId.Create(Id), WorkspaceRelativeDirectory.Create(SourceRoot), LibraryDestinationRoot.Create("."),
                [.. paths.Order(StringComparer.Ordinal).Select(SourceRelativeEligiblePath.Create)])]),
            Snapshot = FileStateSnapshot.File(Absolute(RecordPath), Absolute(RecordPath), RecordBytes(paths)),
            Cause = null,
        };

    internal static byte[] RecordBytes(params string[] paths)
        => OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization.WorkspaceOwnershipCodec.Write(
            new WorkspaceOwnershipDocument(1, null, [],
                [new LibraryOwnership(Id, SourceRoot, ".", [.. paths.Order(StringComparer.Ordinal)])]));

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
        var sourceRoot = Absolute(SourceRoot);
        return new LibraryInventoryRead
        {
            Source = new LibrarySourceRootObservation
            {
                Request = new LibrarySourceRootRequest { Workspace = Workspace, SourceRoot = root },
                State = LibrarySourceRootState.Available,
                LexicalSourceRoot = Absolute(SourceRoot),
                PhysicalSourceRoot = Absolute(SourceRoot),
                LexicallyContained = true,
                PhysicallyContained = true,
                Cause = null,
            },
            Inventory = LibraryInventory.Complete(root, sourceRoot,
                [.. paths.Order(StringComparer.Ordinal).Select(path => EligibleSourceFile.Create(
                    SourceRelativeEligiblePath.Create(path), Absolute($"{SourceRoot}/{path}")))]),
            ExcludedPaths = [],
            UnavailablePaths = [],
        };
    }

    internal static LibraryMappingObservation Mapping(string path, LibraryMappingObservationState state)
    {
        var mapping = LibraryMapping.Create(WorkspaceRelativeDirectory.Create(SourceRoot), LibraryDestinationRoot.Create("."), SourceRelativeEligiblePath.Create(path));
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

    internal static WorkspaceOwnershipRead Ownership(params OwnedPath[] claims)
        => new(WorkspaceOwnershipReadState.Complete,
            new WorkspaceOwnershipDocument(1,
                claims.Any(claim => claim.Manager == OwnedPathManager.Framework)
                    ? new FrameworkOwnership(new OwnedSource("open-forge", null),
                        [.. claims.Where(claim => claim.Manager == OwnedPathManager.Framework).Select(claim => claim.Path)], [])
                    : null,
                [.. claims.Where(claim => claim.Manager == OwnedPathManager.Extension).GroupBy(claim => claim.Owner)
                    .Select(group => new ExtensionOwnership(group.Key, null, null, [], [.. group.Select(claim => claim.Path)], []))], []),
            Absolute(".agents/open-forge.lock.json"), null, null);

    internal static WorkspaceOwnershipRead RegisteredOwnership(params string[] paths)
    {
        var snapshot = FileStateSnapshot.File(Absolute(RecordPath), Absolute(RecordPath), RecordBytes(paths));
        return new(WorkspaceOwnershipReadState.Complete,
            new WorkspaceOwnershipDocument(1, null, [],
                [new LibraryOwnership(Id, SourceRoot, ".", [.. paths.Order(StringComparer.Ordinal)])]),
            Absolute(RecordPath), snapshot, null);
    }

    internal static WorkspaceOwnershipRead WorkspaceOwnership()
        => WorkspaceOwnershipRead.Absent(Absolute(".agents/open-forge.lock.json"));
}
