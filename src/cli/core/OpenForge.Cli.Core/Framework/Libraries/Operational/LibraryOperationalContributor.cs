using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Record;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Source;
using OpenForge.Cli.Core.Framework.Lifecycle.Ownership;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Libraries.Operational;

internal interface ILibraryOperationalContributor
{
    ValueTask<LibraryStatusView> ReadStatusAsync(CliWorkspace workspace, CancellationToken cancellationToken);
    ValueTask<LibraryDoctorView> ReadDoctorAsync(CliWorkspace workspace, CancellationToken cancellationToken);
}

internal sealed class LibraryOperationalContributor : ILibraryOperationalContributor
{
    public async ValueTask<LibraryStatusView> ReadStatusAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        var resolver = new PhysicalPathResolver();
        try
        {
            var record = await LibrariesRecordReader.ReadAsync(
                resolver,
                workspace,
                cancellationToken).ConfigureAwait(false);
            var ownership = await new LifecycleOwnershipReader(resolver).ReadAsync(
                workspace,
                cancellationToken).ConfigureAwait(false);
            var sources = ImmutableArray.CreateBuilder<LibrarySourceRootObservation>();
            var mappings = ImmutableArray.CreateBuilder<LibraryMappingObservation>();
            if (record.State == LibrariesRecordReadState.Complete && record.Record is { } value)
            {
                foreach (var library in value.Libraries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!sources.Any(source => source.Request.SourceRoot == library.SourceRoot))
                    {
                        sources.Add(LibrarySourceRootReader.Read(
                            resolver,
                            new LibrarySourceRootRequest
                            {
                                Workspace = workspace,
                                SourceRoot = library.SourceRoot,
                            },
                            cancellationToken));
                    }
                    ObserveMappings(resolver, workspace, library, library.Paths, mappings, cancellationToken);
                }
            }

            return new LibraryStatusView
            {
                State = ReadState(record, sources, inventories: null, mappings),
                Ownership = ownership,
                LinkCapability = ReadLinkCapability(),
                Record = record,
                Sources = sources.ToImmutable(),
                Mappings = mappings.ToImmutable(),
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return InterruptedStatus(workspace);
        }
    }

    public async ValueTask<LibraryDoctorView> ReadDoctorAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        var resolver = new PhysicalPathResolver();
        try
        {
            var record = await LibrariesRecordReader.ReadAsync(
                resolver,
                workspace,
                cancellationToken).ConfigureAwait(false);
            var ownership = await new LifecycleOwnershipReader(resolver).ReadAsync(
                workspace,
                cancellationToken).ConfigureAwait(false);
            var inventories = ImmutableArray.CreateBuilder<LibraryInventoryRead>();
            var mappings = ImmutableArray.CreateBuilder<LibraryMappingObservation>();
            if (record.State == LibrariesRecordReadState.Complete && record.Record is { } value)
            {
                foreach (var library in value.Libraries)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var inventory = inventories.FirstOrDefault(observed => observed.Source.Request.SourceRoot == library.SourceRoot);
                    if (inventory is null)
                    {
                        var source = LibrarySourceRootReader.Read(
                            resolver,
                            new LibrarySourceRootRequest
                            {
                                Workspace = workspace,
                                SourceRoot = library.SourceRoot,
                            },
                            cancellationToken);
                        inventory = source.State == LibrarySourceRootState.Available
                            ? await LibraryInventoryReader.ReadAsync(
                                resolver,
                                source,
                                cancellationToken).ConfigureAwait(false)
                            : new LibraryInventoryRead
                            {
                                Source = source,
                                Inventory = null,
                                ExcludedPaths = [],
                                UnavailablePaths = [],
                            };
                        inventories.Add(inventory);
                    }
                    var paths = library.Paths
                        .Concat(inventory.Inventory?.Entries.Select(entry => entry.SourcePath) ?? [])
                        .DistinctBy(path => path.Value, StringComparer.Ordinal)
                        .OrderBy(path => path.Value, StringComparer.Ordinal);
                    ObserveMappings(resolver, workspace, library, paths, mappings, cancellationToken);
                }
            }

            return new LibraryDoctorView
            {
                State = ReadState(record, sources: null, inventories, mappings),
                Ownership = ownership,
                LinkCapability = ReadLinkCapability(),
                Record = record,
                Inventories = inventories.ToImmutable(),
                Mappings = mappings.ToImmutable(),
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return InterruptedDoctor(workspace);
        }
    }

    private static void ObserveMappings(
        PhysicalPathResolver resolver,
        CliWorkspace workspace,
        LibraryRecord library,
        IEnumerable<SourceRelativeEligiblePath> paths,
        ImmutableArray<LibraryMappingObservation>.Builder mappings,
        CancellationToken cancellationToken)
    {
        var observedMappings = mappings.Select(observation => observation.Mapping).ToHashSet();
        foreach (var path in paths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var mapping = LibraryPathIdentity.Map(library.SourceRoot, library.DestinationRoot, path);
            if (!observedMappings.Add(mapping))
            {
                continue;
            }
            mappings.Add(LibraryMappingObserver.Observe(
                resolver,
                new LibraryMappingObservationRequest
                {
                    Workspace = workspace,
                    Mapping = mapping,
                },
                cancellationToken));
        }
    }

    private static OperationalViewState ReadState(
        LibrariesRecordRead record,
        IEnumerable<LibrarySourceRootObservation>? sources,
        IEnumerable<LibraryInventoryRead>? inventories,
        IEnumerable<LibraryMappingObservation> mappings)
    {
        if (record.State is LibrariesRecordReadState.Malformed or LibrariesRecordReadState.Blocked
            || sources?.Any(source => source.State is LibrarySourceRootState.Invalid or LibrarySourceRootState.Blocked) == true
            || inventories?.Any(read => read.Inventory?.State == LibraryInventoryState.Blocked
                || read.Source.State == LibrarySourceRootState.Blocked
                || read.Source.State == LibrarySourceRootState.Invalid) == true
            || mappings.Any(mapping => mapping.State == LibraryMappingObservationState.Blocked))
        {
            return OperationalViewState.Blocked;
        }

        if (record.State == LibrariesRecordReadState.Unavailable
            || sources?.Any(source => source.State is LibrarySourceRootState.Missing
                or LibrarySourceRootState.Inaccessible
                or LibrarySourceRootState.Unavailable) == true
            || inventories?.Any(read => read.Inventory is null
                || read.Inventory.State is LibraryInventoryState.Incomplete or LibraryInventoryState.Unavailable) == true
            || mappings.Any(mapping => mapping.State == LibraryMappingObservationState.Unavailable))
        {
            return OperationalViewState.Incomplete;
        }

        return OperationalViewState.Complete;
    }

    private static LibraryLinkCapabilityFact ReadLinkCapability()
        => OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS()
            ? new LibraryLinkCapabilityFact(
                LibraryLinkCapabilityState.Supported,
                "The current platform exposes managed file-symbolic-link support.")
            : new LibraryLinkCapabilityFact(
                LibraryLinkCapabilityState.Unsupported,
                "The current platform does not expose a supported Library link implementation.");

    private static LibraryStatusView InterruptedStatus(CliWorkspace workspace)
        => new()
        {
            State = OperationalViewState.Interrupted,
            Ownership = null,
            LinkCapability = null,
            Record = InterruptedRecord(workspace),
            Sources = [],
            Mappings = [],
        };

    private static LibraryDoctorView InterruptedDoctor(CliWorkspace workspace)
        => new()
        {
            State = OperationalViewState.Interrupted,
            Ownership = null,
            LinkCapability = null,
            Record = InterruptedRecord(workspace),
            Inventories = [],
            Mappings = [],
        };

    private static LibrariesRecordRead InterruptedRecord(CliWorkspace workspace)
        => new()
        {
            State = LibrariesRecordReadState.Unavailable,
            Record = null,
            Snapshot = null,
            Cause = $"Library observation for '{workspace.LexicalRoot}' was interrupted.",
        };
}
