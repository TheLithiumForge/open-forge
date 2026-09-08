using OpenForge.Cli.Core.Commands.Library.List.Models.Request;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Record;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Source;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Library.List;

internal sealed class LibraryListOperation
{
    private readonly PhysicalPathResolver _resolver = new();

    internal async ValueTask<LibraryListResult> ExecuteAsync(
        LibraryListRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var record = await LibrariesRecordReader.ReadAsync(
                _resolver,
                request.Workspace,
                cancellationToken).ConfigureAwait(false);
            return record.State == LibrariesRecordReadState.Complete
                ? ObserveCompleteRecord(_resolver, request, record, cancellationToken)
                : FromRecordBoundary(request, record);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Event(
                request,
                CliSemanticStatus.Interrupted,
                LibraryRecordViewState.Interrupted,
                LibraryCoverage.Interrupted,
                LibraryListFindingCode.Interrupted,
                "Library List was interrupted.");
        }
        catch (Exception exception) when (exception is IOException
            or InvalidOperationException
            or UnauthorizedAccessException)
        {
            return Event(
                request,
                CliSemanticStatus.Failed,
                LibraryRecordViewState.Failed,
                LibraryCoverage.Failed,
                LibraryListFindingCode.OperationFailed,
                exception.Message);
        }
    }

    private static LibraryListResult ObserveCompleteRecord(
        PhysicalPathResolver resolver,
        LibraryListRequest request,
        LibrariesRecordRead read,
        CancellationToken cancellationToken)
    {
        var record = read.Record
            ?? throw new InvalidOperationException("A complete Library record read requires a record.");
        var libraries = new List<LibraryListView>(record.Libraries.Length);
        var findings = new List<LibraryListFinding>();
        foreach (var library in record.Libraries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = LibrarySourceRootReader.Read(
                resolver,
                new LibrarySourceRootRequest
                {
                    Workspace = request.Workspace,
                    SourceRoot = library.SourceRoot,
                },
                cancellationToken);
            var sourceState = ReadSourceState(source.State);
            AddSourceFinding(findings, library, source);

            var paths = new List<LibraryListRegisteredPath>(library.Paths.Length);
            foreach (var sourcePath in library.Paths)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var mapping = LibraryPathIdentity.Map(library.SourceRoot, sourcePath);
                var observation = LibraryMappingObserver.Observe(
                    resolver,
                    new LibraryMappingObservationRequest
                    {
                        Workspace = request.Workspace,
                        Mapping = mapping,
                    },
                    cancellationToken);
                paths.Add(ProjectPath(observation));
                AddMappingFinding(findings, library, observation);
            }

            libraries.Add(new LibraryListView
            {
                Id = library.Id.Value,
                SourceRoot = library.SourceRoot.Value,
                SourceRootState = sourceState,
                Paths = [.. paths],
            });
        }

        var status = SelectStatus(findings);
        return new LibraryListResult
        {
            Status = status,
            Workspace = request.Workspace,
            Result = new LibraryListPayload
            {
                Record = new LibraryListRecordView
                {
                    Path = LibraryPathIdentity.RecordRelativePath,
                    State = LibraryRecordViewState.Complete,
                    LibraryCount = record.Libraries.Length,
                },
                Libraries = [.. libraries],
                Inventory = LibraryListInventoryState.NotRequested,
                Coverage = status switch
                {
                    CliSemanticStatus.Invalid => LibraryCoverage.NotStarted,
                    CliSemanticStatus.Blocked => LibraryCoverage.Blocked,
                    CliSemanticStatus.Incomplete => LibraryCoverage.Incomplete,
                    CliSemanticStatus.Attention or CliSemanticStatus.Complete => LibraryCoverage.Complete,
                    _ => throw new ArgumentOutOfRangeException(nameof(read), status, "The Library List status is not defined."),
                },
                Findings = [.. findings],
            },
        };
    }

    private static LibraryListResult FromRecordBoundary(
        LibraryListRequest request,
        LibrariesRecordRead read)
    {
        if (read.State == LibrariesRecordReadState.Missing)
        {
            return new LibraryListResult
            {
                Status = CliSemanticStatus.Complete,
                Workspace = request.Workspace,
                Result = new LibraryListPayload
                {
                    Record = new LibraryListRecordView
                    {
                        Path = LibraryPathIdentity.RecordRelativePath,
                        State = LibraryRecordViewState.Missing,
                        LibraryCount = 0,
                    },
                    Libraries = [],
                    Inventory = LibraryListInventoryState.NotRequested,
                    Coverage = LibraryCoverage.Complete,
                    Findings = [],
                },
            };
        }

        var (status, recordState, coverage, code) = read.State switch
        {
            LibrariesRecordReadState.Malformed => (
                CliSemanticStatus.Invalid,
                LibraryRecordViewState.Invalid,
                LibraryCoverage.NotStarted,
                LibraryListFindingCode.InvalidRecord),
            LibrariesRecordReadState.Unavailable => (
                CliSemanticStatus.Incomplete,
                LibraryRecordViewState.Unavailable,
                LibraryCoverage.Incomplete,
                LibraryListFindingCode.RecordUnavailable),
            LibrariesRecordReadState.Blocked => (
                CliSemanticStatus.Blocked,
                LibraryRecordViewState.Blocked,
                LibraryCoverage.Blocked,
                LibraryListFindingCode.RecordBlocked),
            _ => throw new ArgumentOutOfRangeException(
                nameof(read),
                read.State,
                "The Library record read state is not defined for a boundary result."),
        };
        return Event(
            request,
            status,
            recordState,
            coverage,
            code,
            read.Cause ?? "The Library record could not be established.");
    }

    private static LibraryListRegisteredPath ProjectPath(
        LibraryMappingObservation observation)
        => new()
        {
            SourcePath = observation.Mapping.SourcePath.Value,
            DestinationPath = observation.Mapping.DestinationPath.Value,
            ExpectedRelativeLink = observation.Mapping.ExpectedRelativeLink.Value,
            SourceId = SourceIdentity.DeriveId(observation.Mapping.DestinationPath.Value),
            State = observation.State switch
            {
                LibraryMappingObservationState.Current => LibraryLinkViewState.Current,
                LibraryMappingObservationState.Missing => LibraryLinkViewState.Missing,
                LibraryMappingObservationState.Changed => LibraryLinkViewState.Changed,
                LibraryMappingObservationState.Blocked => LibraryLinkViewState.Blocked,
                LibraryMappingObservationState.Unavailable => LibraryLinkViewState.Unavailable,
                _ => throw new ArgumentOutOfRangeException(nameof(observation), observation.State, "The mapping observation state is not defined."),
            },
            ObservedRelativeLink = observation.Leaf.RelativeFileLink?.RawRelativeTarget,
        };

    private static LibrarySourceRootViewState ReadSourceState(
        LibrarySourceRootState state)
        => state switch
        {
            LibrarySourceRootState.Available => LibrarySourceRootViewState.Available,
            LibrarySourceRootState.Missing => LibrarySourceRootViewState.Missing,
            LibrarySourceRootState.Invalid => LibrarySourceRootViewState.Invalid,
            LibrarySourceRootState.Inaccessible or LibrarySourceRootState.Unavailable => LibrarySourceRootViewState.Unavailable,
            LibrarySourceRootState.Blocked => LibrarySourceRootViewState.Blocked,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Library source-root state is not defined."),
        };

    private static void AddSourceFinding(
        List<LibraryListFinding> findings,
        LibraryRecord library,
        LibrarySourceRootObservation source)
    {
        (LibraryListFindingCode Code, CliSemanticStatus Status)? mapped = source.State switch
        {
            LibrarySourceRootState.Available => null,
            LibrarySourceRootState.Invalid => (LibraryListFindingCode.SourceRootInvalid, CliSemanticStatus.Invalid),
            LibrarySourceRootState.Blocked => (LibraryListFindingCode.SourceRootBlocked, CliSemanticStatus.Blocked),
            LibrarySourceRootState.Missing
                or LibrarySourceRootState.Inaccessible
                or LibrarySourceRootState.Unavailable => (LibraryListFindingCode.SourceRootUnavailable, CliSemanticStatus.Incomplete),
            _ => throw new ArgumentOutOfRangeException(nameof(source), source.State, "The Library source-root state is not defined."),
        };
        if (mapped is not { } finding)
        {
            return;
        }

        findings.Add(new LibraryListFinding
        {
            Code = finding.Code,
            Status = finding.Status,
            LibraryId = library.Id.Value,
            Path = library.SourceRoot.Value,
            Cause = source.Cause ?? "The Library source root could not be established.",
        });
    }

    private static void AddMappingFinding(
        List<LibraryListFinding> findings,
        LibraryRecord library,
        LibraryMappingObservation observation)
    {
        (LibraryListFindingCode Code, CliSemanticStatus Status, string Cause)? mapped = observation.State switch
        {
            LibraryMappingObservationState.Current => null,
            LibraryMappingObservationState.Missing => (LibraryListFindingCode.LinkMissing, CliSemanticStatus.Attention, "The registered destination is absent."),
            LibraryMappingObservationState.Changed => (LibraryListFindingCode.LinkChanged, CliSemanticStatus.Attention, "The registered destination differs from its expected relative link."),
            LibraryMappingObservationState.Unavailable => (LibraryListFindingCode.LinkUnavailable, CliSemanticStatus.Incomplete, observation.Cause ?? "The registered destination is unavailable."),
            LibraryMappingObservationState.Blocked => (LibraryListFindingCode.LinkBlocked, CliSemanticStatus.Blocked, observation.Cause ?? "The registered destination is unsafe."),
            _ => throw new ArgumentOutOfRangeException(nameof(observation), observation.State, "The mapping observation state is not defined."),
        };
        if (mapped is not { } finding)
        {
            return;
        }

        findings.Add(new LibraryListFinding
        {
            Code = finding.Code,
            Status = finding.Status,
            LibraryId = library.Id.Value,
            Path = observation.Mapping.DestinationPath.Value,
            Cause = finding.Cause,
        });
    }

    private static CliSemanticStatus SelectStatus(
        IEnumerable<LibraryListFinding> findings)
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

    private static LibraryListResult Event(
        LibraryListRequest request,
        CliSemanticStatus status,
        LibraryRecordViewState recordState,
        LibraryCoverage coverage,
        LibraryListFindingCode code,
        string cause)
        => new()
        {
            Status = status,
            Workspace = request.Workspace,
            Result = new LibraryListPayload
            {
                Record = new LibraryListRecordView
                {
                    Path = LibraryPathIdentity.RecordRelativePath,
                    State = recordState,
                    LibraryCount = null,
                },
                Libraries = [],
                Inventory = status is CliSemanticStatus.Interrupted or CliSemanticStatus.Failed
                    ? LibraryListInventoryState.NotStarted
                    : LibraryListInventoryState.NotRequested,
                Coverage = coverage,
                Findings =
                [
                    new LibraryListFinding
                    {
                        Code = code,
                        Status = status,
                        LibraryId = null,
                        Path = LibraryPathIdentity.RecordRelativePath,
                        Cause = cause,
                    },
                ],
            },
        };
}
