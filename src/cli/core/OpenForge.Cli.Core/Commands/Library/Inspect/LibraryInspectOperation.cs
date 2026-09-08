using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Observation;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Record;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Source;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library.Inspect;

internal sealed class LibraryInspectOperation
{
    private readonly PhysicalPathResolver _resolver = new();

    internal async ValueTask<LibraryInspectResult> ExecuteAsync(
        LibraryInspectRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var read = await LibrariesRecordReader.ReadAsync(
                _resolver,
                request.Workspace,
                cancellationToken).ConfigureAwait(false);
            if (read.State != LibrariesRecordReadState.Complete)
            {
                return FromRecordBoundary(request, read);
            }

            var record = read.Record
                ?? throw new InvalidOperationException("A complete Library record read requires a record.");
            var selected = record.Libraries.FirstOrDefault(library =>
                string.Equals(library.Id.Value, request.LibraryId.Value, StringComparison.Ordinal));
            return selected is null
                ? UnknownId(request, LibraryRecordViewState.Complete)
                : await ObserveSelectedAsync(_resolver, request, selected, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Event(
                request,
                CliSemanticStatus.Interrupted,
                LibraryRecordViewState.Interrupted,
                LibraryInventoryViewState.Interrupted,
                LibraryCoverage.Interrupted,
                LibraryInspectFindingCode.Interrupted,
                "Library Inspect was interrupted.");
        }
        catch (Exception exception) when (exception is IOException
            or InvalidOperationException
            or UnauthorizedAccessException)
        {
            return Event(
                request,
                CliSemanticStatus.Failed,
                LibraryRecordViewState.Failed,
                LibraryInventoryViewState.Failed,
                LibraryCoverage.Failed,
                LibraryInspectFindingCode.OperationFailed,
                exception.Message);
        }
    }

    private static async ValueTask<LibraryInspectResult> ObserveSelectedAsync(
        PhysicalPathResolver resolver,
        LibraryInspectRequest request,
        LibraryRecord selected,
        CancellationToken cancellationToken)
    {
        var registered = selected.Paths
            .Select(sourcePath => Registered(selected, sourcePath))
            .ToArray();
        var source = LibrarySourceRootReader.Read(
            resolver,
            new LibrarySourceRootRequest
            {
                Workspace = request.Workspace,
                SourceRoot = selected.SourceRoot,
            },
            cancellationToken);
        if (source.State != LibrarySourceRootState.Available)
        {
            return FromSourceBoundary(request, selected, registered, source);
        }

        var inventoryRead = await LibraryInventoryReader.ReadAsync(
            resolver,
            source,
            cancellationToken).ConfigureAwait(false);
        var inventory = inventoryRead.Inventory;
        var eligibleEntries = inventory?.Entries ?? [];
        var eligible = eligibleEntries
            .Select(entry => Eligible(entry.SourcePath))
            .ToArray();
        var findings = new List<LibraryInspectFinding>();
        if (inventory is null || inventory.State != LibraryInventoryState.Complete)
        {
            findings.Add(new LibraryInspectFinding
            {
                Code = LibraryInspectFindingCode.InventoryIncomplete,
                Status = inventory?.State == LibraryInventoryState.Blocked
                    ? CliSemanticStatus.Blocked
                    : CliSemanticStatus.Incomplete,
                LibraryId = selected.Id.Value,
                Path = ".agents",
                Cause = inventory?.Cause
                    ?? inventoryRead.UnavailablePaths.FirstOrDefault()?.Cause
                    ?? "The complete Library source inventory could not be established.",
            });
        }

        var comparisons = ObserveComparisons(
            resolver,
            request,
            selected,
            registered,
            eligible,
            inventory?.State == LibraryInventoryState.Complete,
            findings,
            cancellationToken);
        var projection = SelectProjection(inventory, comparisons);
        var status = SelectStatus(findings);
        return new LibraryInspectResult
        {
            Status = status,
            Workspace = request.Workspace,
            Result = new LibraryInspectPayload
            {
                Record = new LibraryInspectRecordView
                {
                    Path = LibraryPathIdentity.RecordRelativePath,
                    State = LibraryRecordViewState.Complete,
                    Id = selected.Id.Value,
                    SourceRoot = selected.SourceRoot.Value,
                    RegisteredPaths = registered,
                },
                Source = new LibraryInspectSourceView
                {
                    RootState = LibrarySourceRootViewState.Available,
                    State = ReadInventoryState(inventory),
                    EligiblePaths = eligible,
                },
                Projection = new LibraryInspectProjectionView
                {
                    State = projection,
                    Comparisons = comparisons,
                },
                Findings = [.. findings],
            },
        };
    }

    private static LibraryPathComparison[] ObserveComparisons(
        PhysicalPathResolver resolver,
        LibraryInspectRequest request,
        LibraryRecord selected,
        IReadOnlyList<LibraryRegisteredPath> registered,
        IReadOnlyList<LibraryEligiblePath> eligible,
        bool inventoryComplete,
        ICollection<LibraryInspectFinding> findings,
        CancellationToken cancellationToken)
    {
        var registeredByPath = registered.ToDictionary(
            path => path.DestinationPath,
            StringComparer.Ordinal);
        var eligibleByPath = eligible.ToDictionary(
            path => path.DestinationPath,
            StringComparer.Ordinal);
        var paths = new SortedSet<string>(registeredByPath.Keys, StringComparer.Ordinal);
        paths.UnionWith(eligibleByPath.Keys);
        var comparisons = new List<LibraryPathComparison>(paths.Count);
        foreach (var path in paths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sourcePath = Framework.Libraries.Models.Identity.SourceRelativeEligiblePath.Create(path);
            var mapping = LibraryPathIdentity.Map(selected.SourceRoot, sourcePath);
            var observation = LibraryMappingObserver.Observe(
                resolver,
                new LibraryMappingObservationRequest
                {
                    Workspace = request.Workspace,
                    Mapping = mapping,
                },
                cancellationToken);
            registeredByPath.TryGetValue(path, out var registeredPath);
            var isEligible = eligibleByPath.ContainsKey(path);
            var relation = ReadRelation(
                observation.State,
                registeredPath is not null,
                isEligible,
                inventoryComplete);
            comparisons.Add(new LibraryPathComparison
            {
                SourcePath = path,
                DestinationPath = path,
                SourceId = SourceIdentity.DeriveId(path),
                Relation = relation,
                Registered = registeredPath,
                ObservedRelativeLink = observation.Leaf.RelativeFileLink?.RawRelativeTarget,
            });
            AddComparisonFinding(findings, selected, path, relation, observation.Cause);
        }

        return [.. comparisons];
    }

    private static LibraryComparisonRelation ReadRelation(
        LibraryMappingObservationState observation,
        bool registered,
        bool eligible,
        bool inventoryComplete)
    {
        if (observation == LibraryMappingObservationState.Blocked)
        {
            return LibraryComparisonRelation.Blocked;
        }

        if (observation == LibraryMappingObservationState.Unavailable)
        {
            return LibraryComparisonRelation.Unavailable;
        }

        if (registered && eligible)
        {
            return observation switch
            {
                LibraryMappingObservationState.Current => LibraryComparisonRelation.Current,
                LibraryMappingObservationState.Missing => LibraryComparisonRelation.Missing,
                LibraryMappingObservationState.Changed => LibraryComparisonRelation.Changed,
                _ => throw new ArgumentOutOfRangeException(nameof(observation), observation, "The mapping observation state is not defined."),
            };
        }

        if (eligible)
        {
            return LibraryComparisonRelation.Added;
        }

        return inventoryComplete
            ? LibraryComparisonRelation.Retired
            : LibraryComparisonRelation.Unavailable;
    }

    private static LibraryRegisteredPath Registered(
        LibraryRecord selected,
        Framework.Libraries.Models.Identity.SourceRelativeEligiblePath sourcePath)
    {
        var mapping = LibraryPathIdentity.Map(selected.SourceRoot, sourcePath);
        return new LibraryRegisteredPath
        {
            SourcePath = sourcePath.Value,
            DestinationPath = mapping.DestinationPath.Value,
            ExpectedRelativeLink = mapping.ExpectedRelativeLink.Value,
            SourceId = SourceIdentity.DeriveId(mapping.DestinationPath.Value),
        };
    }

    private static LibraryEligiblePath Eligible(
        Framework.Libraries.Models.Identity.SourceRelativeEligiblePath sourcePath)
        => new()
        {
            SourcePath = sourcePath.Value,
            DestinationPath = sourcePath.Value,
            SourceId = SourceIdentity.DeriveId(sourcePath.Value),
        };

    private static LibraryInspectResult FromRecordBoundary(
        LibraryInspectRequest request,
        LibrariesRecordRead read)
    {
        if (read.State == LibrariesRecordReadState.Missing)
        {
            return UnknownId(request, LibraryRecordViewState.Missing);
        }

        var (status, state, inventory, coverage, code) = read.State switch
        {
            LibrariesRecordReadState.Malformed => (
                CliSemanticStatus.Invalid,
                LibraryRecordViewState.Invalid,
                LibraryInventoryViewState.NotStarted,
                LibraryCoverage.NotStarted,
                LibraryInspectFindingCode.RecordInvalid),
            LibrariesRecordReadState.Unavailable => (
                CliSemanticStatus.Incomplete,
                LibraryRecordViewState.Unavailable,
                LibraryInventoryViewState.NotStarted,
                LibraryCoverage.Incomplete,
                LibraryInspectFindingCode.RecordUnavailable),
            LibrariesRecordReadState.Blocked => (
                CliSemanticStatus.Blocked,
                LibraryRecordViewState.Blocked,
                LibraryInventoryViewState.NotStarted,
                LibraryCoverage.Blocked,
                LibraryInspectFindingCode.RecordBlocked),
            _ => throw new ArgumentOutOfRangeException(
                nameof(read),
                read.State,
                "The Library record read state is not defined for a boundary result."),
        };
        return Event(
            request,
            status,
            state,
            inventory,
            coverage,
            code,
            read.Cause ?? "The Library record could not be established.");
    }

    private static LibraryInspectResult UnknownId(
        LibraryInspectRequest request,
        LibraryRecordViewState state)
        => new()
        {
            Status = CliSemanticStatus.Invalid,
            Workspace = request.Workspace,
            Result = new LibraryInspectPayload
            {
                Record = new LibraryInspectRecordView
                {
                    Path = LibraryPathIdentity.RecordRelativePath,
                    State = state,
                    Id = request.LibraryId.Value,
                    SourceRoot = null,
                    RegisteredPaths = [],
                },
                Source = new LibraryInspectSourceView
                {
                    RootState = LibrarySourceRootViewState.NotStarted,
                    State = LibraryInventoryViewState.NotStarted,
                    EligiblePaths = [],
                },
                Projection = new LibraryInspectProjectionView
                {
                    State = LibraryCoverage.NotStarted,
                    Comparisons = [],
                },
                Findings =
                [
                    new LibraryInspectFinding
                    {
                        Code = LibraryInspectFindingCode.UnknownId,
                        Status = CliSemanticStatus.Invalid,
                        LibraryId = request.LibraryId.Value,
                        Path = LibraryPathIdentity.RecordRelativePath,
                        Cause = "The selected Library record does not contain the supplied ID.",
                    },
                ],
            },
        };

    private static LibraryInspectResult FromSourceBoundary(
        LibraryInspectRequest request,
        LibraryRecord selected,
        LibraryRegisteredPath[] registered,
        LibrarySourceRootObservation source)
    {
        var (status, root, inventory, projection, code) = source.State switch
        {
            LibrarySourceRootState.Invalid => (
                CliSemanticStatus.Invalid,
                LibrarySourceRootViewState.Invalid,
                LibraryInventoryViewState.Invalid,
                LibraryCoverage.NotStarted,
                LibraryInspectFindingCode.SourceRootInvalid),
            LibrarySourceRootState.Blocked => (
                CliSemanticStatus.Blocked,
                LibrarySourceRootViewState.Blocked,
                LibraryInventoryViewState.Blocked,
                LibraryCoverage.Blocked,
                LibraryInspectFindingCode.SourceRootBlocked),
            LibrarySourceRootState.Missing => (
                CliSemanticStatus.Incomplete,
                LibrarySourceRootViewState.Missing,
                LibraryInventoryViewState.Incomplete,
                LibraryCoverage.Incomplete,
                LibraryInspectFindingCode.SourceRootUnavailable),
            LibrarySourceRootState.Inaccessible or LibrarySourceRootState.Unavailable => (
                CliSemanticStatus.Incomplete,
                LibrarySourceRootViewState.Unavailable,
                LibraryInventoryViewState.Incomplete,
                LibraryCoverage.Incomplete,
                LibraryInspectFindingCode.SourceRootUnavailable),
            _ => throw new ArgumentOutOfRangeException(nameof(source), source.State, "The Library source-root state is not defined."),
        };
        return new LibraryInspectResult
        {
            Status = status,
            Workspace = request.Workspace,
            Result = new LibraryInspectPayload
            {
                Record = new LibraryInspectRecordView
                {
                    Path = LibraryPathIdentity.RecordRelativePath,
                    State = LibraryRecordViewState.Complete,
                    Id = selected.Id.Value,
                    SourceRoot = selected.SourceRoot.Value,
                    RegisteredPaths = registered,
                },
                Source = new LibraryInspectSourceView
                {
                    RootState = root,
                    State = inventory,
                    EligiblePaths = [],
                },
                Projection = new LibraryInspectProjectionView
                {
                    State = projection,
                    Comparisons = [],
                },
                Findings =
                [
                    new LibraryInspectFinding
                    {
                        Code = code,
                        Status = status,
                        LibraryId = selected.Id.Value,
                        Path = selected.SourceRoot.Value,
                        Cause = source.Cause ?? "The Library source root could not be established.",
                    },
                ],
            },
        };
    }

    private static LibraryInventoryViewState ReadInventoryState(
        LibraryInventory? inventory)
        => inventory?.State switch
        {
            LibraryInventoryState.Complete => LibraryInventoryViewState.Complete,
            LibraryInventoryState.Incomplete or LibraryInventoryState.Unavailable => LibraryInventoryViewState.Incomplete,
            LibraryInventoryState.Blocked => LibraryInventoryViewState.Blocked,
            null => LibraryInventoryViewState.Incomplete,
            _ => throw new ArgumentOutOfRangeException(nameof(inventory), inventory.State, "The Library inventory state is not defined."),
        };

    private static LibraryCoverage SelectProjection(
        LibraryInventory? inventory,
        IEnumerable<LibraryPathComparison> comparisons)
    {
        if (inventory?.State == LibraryInventoryState.Blocked
            || comparisons.Any(comparison => comparison.Relation == LibraryComparisonRelation.Blocked))
        {
            return LibraryCoverage.Blocked;
        }

        return inventory?.State != LibraryInventoryState.Complete
            || comparisons.Any(comparison => comparison.Relation == LibraryComparisonRelation.Unavailable)
            ? LibraryCoverage.Incomplete
            : LibraryCoverage.Complete;
    }

    private static void AddComparisonFinding(
        ICollection<LibraryInspectFinding> findings,
        LibraryRecord selected,
        string path,
        LibraryComparisonRelation relation,
        string? cause)
    {
        (LibraryInspectFindingCode Code, CliSemanticStatus Status, string Cause)? mapped = relation switch
        {
            LibraryComparisonRelation.Current => null,
            LibraryComparisonRelation.Added => (LibraryInspectFindingCode.PathAdded, CliSemanticStatus.Attention, "The eligible source path is not registered."),
            LibraryComparisonRelation.Retired => (LibraryInspectFindingCode.PathRetired, CliSemanticStatus.Attention, "The registered path is absent from the complete source inventory."),
            LibraryComparisonRelation.Missing => (LibraryInspectFindingCode.LinkMissing, CliSemanticStatus.Attention, "The eligible registered destination is absent."),
            LibraryComparisonRelation.Changed => (LibraryInspectFindingCode.LinkChanged, CliSemanticStatus.Attention, "The eligible registered destination differs from its expected relative link."),
            LibraryComparisonRelation.Blocked => (LibraryInspectFindingCode.LinkBlocked, CliSemanticStatus.Blocked, cause ?? "The Library destination is unsafe."),
            LibraryComparisonRelation.Unavailable => (LibraryInspectFindingCode.InventoryIncomplete, CliSemanticStatus.Incomplete, cause ?? "The required Library path facts are incomplete."),
            LibraryComparisonRelation.NotStarted => null,
            _ => throw new ArgumentOutOfRangeException(nameof(relation), relation, "The Library comparison relation is not defined."),
        };
        if (mapped is not { } finding)
        {
            return;
        }

        findings.Add(new LibraryInspectFinding
        {
            Code = finding.Code,
            Status = finding.Status,
            LibraryId = selected.Id.Value,
            Path = path,
            Cause = finding.Cause,
        });
    }

    private static CliSemanticStatus SelectStatus(
        IEnumerable<LibraryInspectFinding> findings)
    {
        var statuses = findings.Select(finding => finding.Status).ToArray();
        if (statuses.Contains(CliSemanticStatus.Invalid))
        {
            return CliSemanticStatus.Invalid;
        }

        if (statuses.Contains(CliSemanticStatus.Blocked))
        {
            return CliSemanticStatus.Blocked;
        }

        if (statuses.Contains(CliSemanticStatus.Incomplete))
        {
            return CliSemanticStatus.Incomplete;
        }

        return statuses.Contains(CliSemanticStatus.Attention)
            ? CliSemanticStatus.Attention
            : CliSemanticStatus.Complete;
    }

    private static LibraryInspectResult Event(
        LibraryInspectRequest request,
        CliSemanticStatus status,
        LibraryRecordViewState recordState,
        LibraryInventoryViewState inventory,
        LibraryCoverage projection,
        LibraryInspectFindingCode code,
        string cause)
        => new()
        {
            Status = status,
            Workspace = request.Workspace,
            Result = new LibraryInspectPayload
            {
                Record = new LibraryInspectRecordView
                {
                    Path = LibraryPathIdentity.RecordRelativePath,
                    State = recordState,
                    Id = request.LibraryId.Value,
                    SourceRoot = null,
                    RegisteredPaths = [],
                },
                Source = new LibraryInspectSourceView
                {
                    RootState = LibrarySourceRootViewState.NotStarted,
                    State = inventory,
                    EligiblePaths = [],
                },
                Projection = new LibraryInspectProjectionView
                {
                    State = projection,
                    Comparisons = [],
                },
                Findings =
                [
                    new LibraryInspectFinding
                    {
                        Code = code,
                        Status = status,
                        LibraryId = request.LibraryId.Value,
                        Path = LibraryPathIdentity.RecordRelativePath,
                        Cause = cause,
                    },
                ],
            },
        };
}
