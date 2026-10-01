using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Source;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Planning;

internal sealed class UpdateProjectionInputReader(
    PhysicalPathResolver physicalPathResolver,
    WorkspaceOwnershipDocument ownershipDocument)
{
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;
    private readonly UpdateComparisonReader _targetReader = new(physicalPathResolver);
    private readonly LibraryRegistrationSet _registrations =
        LibraryRegistrationReader.ReadRegistrations(ownershipDocument);

    internal async ValueTask<UpdateTargetRead> ReadAsync(
        CliWorkspace workspace,
        SourceLayer layer,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(layer);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var mappings = FindMappings(layer.CanonicalPath);
            if (mappings.Count == 0)
            {
                return await ReadOrdinaryAsync(workspace, layer, cancellationToken)
                    .ConfigureAwait(false);
            }

            if (mappings.Count > 1)
            {
                return Blocked(
                    layer.CanonicalPath,
                    "Multiple registered Library mappings claim the same Update projection destination.");
            }

            return await ReadRegisteredAsync(
                    workspace,
                    layer,
                    mappings[0],
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled(layer.CanonicalPath);
        }
    }

    private async ValueTask<UpdateTargetRead> ReadOrdinaryAsync(
        CliWorkspace workspace,
        SourceLayer layer,
        CancellationToken cancellationToken)
    {
        var read = await _targetReader
            .ReadAsync(workspace, layer.CanonicalPath, cancellationToken)
            .ConfigureAwait(false);
        if (read.State != UpdateTargetReadState.Available)
        {
            if (Reclassify(layer.CanonicalPath, read) is { } boundary)
            {
                return boundary;
            }

            throw new InvalidOperationException(
                "A non-available Update projection input requires a classified boundary.");
        }

        if (read.Snapshot is not { Kind: FileExpectationKind.File } snapshot)
        {
            return Blocked(
                layer.CanonicalPath,
                "The Update projection input is not an available ordinary file.");
        }

        if (snapshot.PhysicalPath is not { } physicalPath)
        {
            return Blocked(
                layer.CanonicalPath,
                "The Update projection input has no verified physical identity.");
        }

        return PhysicalIdentityTracker.PathComparer.Equals(physicalPath, layer.PhysicalPath)
            ? read
            : Blocked(
                layer.CanonicalPath,
                "The Update projection input changed physical identity from the source catalogue.");
    }

    private async ValueTask<UpdateTargetRead> ReadRegisteredAsync(
        CliWorkspace workspace,
        SourceLayer layer,
        (LibraryRegistration Registration, LibraryMapping Mapping) registered,
        CancellationToken cancellationToken)
    {
        var mappingObservation = ObserveMapping(workspace, registered.Mapping, cancellationToken);
        if (ReadMappingBoundary(layer.CanonicalPath, mappingObservation) is { } mappingBoundary)
        {
            return mappingBoundary;
        }

        var sourceRoot = LibrarySourceRootReader.Read(
            _physicalPathResolver,
            new LibrarySourceRootRequest
            {
                Workspace = workspace,
                SourceRoot = registered.Registration.SourceRoot,
            },
            cancellationToken);
        if (ReadSourceRootBoundary(layer.CanonicalPath, sourceRoot) is { } sourceRootBoundary)
        {
            return sourceRootBoundary;
        }

        var physicalSourceRoot = sourceRoot.PhysicalSourceRoot
            ?? throw new InvalidOperationException(
                "An available Library source root requires a physical path.");
        var sourcePath = $"{registered.Registration.SourceRoot.Value}/{registered.Mapping.SourcePath.Value}";
        var sourceRead = await _targetReader
            .ReadAsync(workspace, sourcePath, cancellationToken)
            .ConfigureAwait(false);
        if (Reclassify(layer.CanonicalPath, sourceRead) is { } sourceBoundary)
        {
            return sourceBoundary;
        }

        var sourceSnapshot = sourceRead.Snapshot
            ?? throw new InvalidOperationException(
                "An available Library source read requires an exact snapshot.");
        if (sourceSnapshot.Kind != FileExpectationKind.File)
        {
            return Blocked(
                layer.CanonicalPath,
                "The registered Library source is not an available ordinary file.");
        }

        if (sourceSnapshot.PhysicalPath is not { } sourcePhysicalPath)
        {
            return Blocked(
                layer.CanonicalPath,
                "The registered Library source has no verified physical identity.");
        }

        if (!PhysicalContainment.Contains(physicalSourceRoot, sourcePhysicalPath))
        {
            return Blocked(
                layer.CanonicalPath,
                "The registered Library source escaped its verified physical source root.");
        }

        if (!PhysicalIdentityTracker.PathComparer.Equals(sourcePhysicalPath, layer.PhysicalPath))
        {
            return Blocked(
                layer.CanonicalPath,
                "The registered Library source changed physical identity from the source catalogue.");
        }

        var confirmedSourceRoot = LibrarySourceRootReader.Read(
            _physicalPathResolver,
            new LibrarySourceRootRequest
            {
                Workspace = workspace,
                SourceRoot = registered.Registration.SourceRoot,
            },
            cancellationToken);
        if (ReadSourceRootBoundary(layer.CanonicalPath, confirmedSourceRoot) is { } confirmedRootBoundary)
        {
            return confirmedRootBoundary;
        }

        var confirmedPhysicalSourceRoot = confirmedSourceRoot.PhysicalSourceRoot
            ?? throw new InvalidOperationException(
                "An available Library source root requires a physical path.");
        if (!PhysicalIdentityTracker.PathComparer.Equals(
                physicalSourceRoot,
                confirmedPhysicalSourceRoot))
        {
            return Blocked(
                layer.CanonicalPath,
                "The Library source root changed physical identity during inspection.");
        }

        var confirmedMapping = ObserveMapping(workspace, registered.Mapping, cancellationToken);
        if (ReadMappingBoundary(layer.CanonicalPath, confirmedMapping) is { } confirmedMappingBoundary)
        {
            return confirmedMappingBoundary;
        }

        var destinationPath = Path.GetFullPath(Path.Combine(
            workspace.LexicalRoot,
            layer.CanonicalPath.Replace('/', Path.DirectorySeparatorChar)));
        var destinationResolution = _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            destinationPath);
        if (destinationResolution.State != PhysicalPathState.Contained)
        {
            return FromResolution(layer.CanonicalPath, destinationResolution);
        }

        var destinationPhysicalPath = destinationResolution.GetContainedPhysicalPath();
        if (!PhysicalIdentityTracker.PathComparer.Equals(destinationPhysicalPath, sourcePhysicalPath)
            || !PhysicalIdentityTracker.PathComparer.Equals(destinationPhysicalPath, layer.PhysicalPath))
        {
            return Blocked(
                layer.CanonicalPath,
                "The registered Library destination changed physical identity during inspection.");
        }

        return new UpdateTargetRead(
            layer.CanonicalPath,
            UpdateTargetReadState.Available,
            FileStateSnapshot.File(
                destinationPath,
                sourcePhysicalPath,
                sourceSnapshot.Bytes.AsSpan()),
            Cause: null);
    }

    private LibraryMappingObservation ObserveMapping(
        CliWorkspace workspace,
        LibraryMapping mapping,
        CancellationToken cancellationToken)
        => LibraryMappingObserver.Observe(
            _physicalPathResolver,
            new LibraryMappingObservationRequest
            {
                Workspace = workspace,
                Mapping = mapping,
            },
            cancellationToken);

    private IReadOnlyList<(LibraryRegistration Registration, LibraryMapping Mapping)> FindMappings(
        string canonicalPath)
    {
        var key = PortableWorkspacePath.CreatePortableKey(canonicalPath);
        return _registrations.Libraries
            .SelectMany(registration => LibraryPathIdentity.Mappings(registration)
                .Select(mapping => (Registration: registration, Mapping: mapping)))
            .Where(value => PortableWorkspacePath.CreatePortableKey(value.Mapping.DestinationPath.Value) == key)
            .ToArray();
    }

    private static UpdateTargetRead? ReadMappingBoundary(
        string relativePath,
        LibraryMappingObservation observation)
        => observation.State switch
        {
            LibraryMappingObservationState.Current => null,
            LibraryMappingObservationState.Unavailable => Unavailable(
                relativePath,
                observation.Cause ?? "The registered Library destination is unavailable."),
            _ => Blocked(
                relativePath,
                observation.Cause ?? "The registered Library destination is not current."),
        };

    private static UpdateTargetRead? ReadSourceRootBoundary(
        string relativePath,
        LibrarySourceRootObservation observation)
    {
        if (observation.State == LibrarySourceRootState.Available
            && observation.PhysicallyContained == true
            && observation.PhysicalSourceRoot is not null)
        {
            return null;
        }

        var cause = observation.Cause
            ?? "The registered Library source root is not available and physically contained.";
        return observation.State is LibrarySourceRootState.Inaccessible
            or LibrarySourceRootState.Unavailable
            ? Unavailable(relativePath, cause)
            : Blocked(relativePath, cause);
    }

    private static UpdateTargetRead? Reclassify(
        string relativePath,
        UpdateTargetRead read)
        => read.State switch
        {
            UpdateTargetReadState.Available => null,
            UpdateTargetReadState.Cancelled => Cancelled(relativePath),
            UpdateTargetReadState.Unavailable => Unavailable(
                relativePath,
                read.Cause ?? "The current Update projection input is unavailable."),
            _ => Blocked(
                relativePath,
                read.Cause ?? "The current Update projection input is missing or unsafe."),
        };

    private static UpdateTargetRead FromResolution(
        string relativePath,
        PhysicalPathResolution resolution)
    {
        var cause = resolution.Failure?.DirectCause
            ?? "The registered Library destination is not physically contained.";
        return resolution.State is PhysicalPathState.Inaccessible
            or PhysicalPathState.InputOutputFailure
            ? Unavailable(relativePath, cause)
            : Blocked(relativePath, cause);
    }

    private static UpdateTargetRead Blocked(string relativePath, string cause)
        => new(relativePath, UpdateTargetReadState.Blocked, Snapshot: null, cause);

    private static UpdateTargetRead Unavailable(string relativePath, string cause)
        => new(relativePath, UpdateTargetReadState.Unavailable, Snapshot: null, cause);

    private static UpdateTargetRead Cancelled(string relativePath)
        => new(relativePath, UpdateTargetReadState.Cancelled, Snapshot: null, Cause: null);
}
