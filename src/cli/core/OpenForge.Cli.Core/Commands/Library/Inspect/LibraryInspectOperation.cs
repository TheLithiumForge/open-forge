using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Inspect.Shared.Comparison;
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
            return FromSourceBoundary(request, selected, LibraryInspectComparisonReader.RegisteredPaths(selected), source);
        }

        var inventoryRead = await LibraryInventoryReader.ReadAsync(
            resolver,
            source,
            cancellationToken).ConfigureAwait(false);
        var inventory = inventoryRead.Inventory;
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
                Path = selected.SourceRoot.Value,
                Cause = inventory?.Cause
                    ?? inventoryRead.UnavailablePaths.FirstOrDefault()?.Cause
                    ?? "The complete Library source inventory could not be established.",
            });
        }

        var comparison = new LibraryInspectComparisonReader(resolver, request.Workspace).Read(selected, inventory, cancellationToken);
        findings.AddRange(comparison.Findings);
        var projection = SelectProjection(inventory, comparison.Comparisons);
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
                    DestinationRoot = selected.DestinationRoot.Value,
                    RegisteredPaths = comparison.RegisteredPaths,
                },
                Source = new LibraryInspectSourceView
                {
                    RootState = LibrarySourceRootViewState.Available,
                    State = ReadInventoryState(inventory),
                    EligiblePaths = comparison.EligiblePaths,
                },
                Projection = new LibraryInspectProjectionView
                {
                    State = projection,
                    Comparisons = comparison.Comparisons,
                },
                Findings = [.. findings],
            },
        };
    }

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
                    DestinationRoot = null,
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
                    DestinationRoot = selected.DestinationRoot.Value,
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
                    DestinationRoot = null,
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
