using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Reading;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Inspect.Shared.Rendering;

internal static class LibraryInspectResultFixture
{
    internal static LibraryInspectResult Create(CliSemanticStatus status = CliSemanticStatus.Attention)
    {
        var seed = Drift();
        if (status == CliSemanticStatus.Attention)
        {
            return seed;
        }

        if (status == CliSemanticStatus.Complete)
        {
            var comparison = seed.Result.Projection.Comparisons[0] with
            {
                Relation = LibraryComparisonRelation.Current,
                ObservedRelativeLink = seed.Result.Record.RegisteredPaths[0].ExpectedRelativeLink,
            };
            return seed with
            {
                Status = status,
                Result = seed.Result with { Projection = seed.Result.Projection with { Comparisons = [comparison] }, Findings = [] },
            };
        }

        var (record, finding) = status switch
        {
            CliSemanticStatus.Incomplete => (LibraryRecordViewState.Unavailable, LibraryInspectFindingCode.RecordUnavailable),
            CliSemanticStatus.Invalid => (LibraryRecordViewState.Invalid, LibraryInspectFindingCode.RecordInvalid),
            CliSemanticStatus.Blocked => (LibraryRecordViewState.Blocked, LibraryInspectFindingCode.RecordBlocked),
            CliSemanticStatus.Failed => (LibraryRecordViewState.Failed, LibraryInspectFindingCode.OperationFailed),
            CliSemanticStatus.Interrupted => (LibraryRecordViewState.Interrupted, LibraryInspectFindingCode.Interrupted),
            _ => throw new ArgumentOutOfRangeException(nameof(status)),
        };
        return seed with
        {
            Status = status,
            Result = seed.Result with
            {
                Record = seed.Result.Record with { State = record, SourceRoot = null, RegisteredPaths = [] },
                Source = new() { RootState = LibrarySourceRootViewState.NotStarted, State = LibraryInventoryViewState.NotStarted, EligiblePaths = [] },
                Projection = new() { State = LibraryCoverage.NotStarted, Comparisons = [] },
                Findings = [seed.Result.Findings[0] with { Code = finding, Status = status, Path = ".agents/open-forge.lock.json", Cause = "Record observation stopped." }],
            },
        };
    }

    private static LibraryInspectResult Drift()
    {
        var registered = new LibraryRegisteredPath
        {
            SourcePath = ".agents/directives/review.md",
            DestinationPath = ".agents/directives/review.md",
            ExpectedRelativeLink = "../../shared/team/.agents/directives/review.md",
            SourceId = "directives/review",
        };
        return new LibraryInspectResult
        {
            Status = CliSemanticStatus.Attention,
            Workspace = LibraryReadInputs.Workspace,
            Result = new LibraryInspectPayload
            {
                Record = new LibraryInspectRecordView
                {
                    Path = ".agents/open-forge.lock.json",
                    State = LibraryRecordViewState.Complete,
                    Id = "team-knowledge",
                    SourceRoot = "shared/team",
                    DestinationRoot = null,
                    RegisteredPaths = [registered],
                },
                Source = new LibraryInspectSourceView
                {
                    RootState = LibrarySourceRootViewState.Available,
                    State = LibraryInventoryViewState.Complete,
                    EligiblePaths =
                    [
                        new()
                        {
                            SourcePath = ".agents/directives/review.md",
                            DestinationPath = ".agents/directives/review.md",
                            SourceId = "directives/review",
                        },
                    ],
                },
                Projection = new LibraryInspectProjectionView
                {
                    State = LibraryCoverage.Complete,
                    Comparisons =
                    [
                        new LibraryPathComparison
                        {
                            SourcePath = ".agents/directives/review.md",
                            DestinationPath = ".agents/directives/review.md",
                            SourceId = "directives/review",
                            Relation = LibraryComparisonRelation.Missing,
                            Registered = registered,
                            ObservedRelativeLink = null,
                        },
                    ],
                },
                Findings =
                [
                    new LibraryInspectFinding
                    {
                        Code = LibraryInspectFindingCode.LinkMissing,
                        Status = CliSemanticStatus.Attention,
                        LibraryId = "team-knowledge",
                        Path = ".agents/directives/review.md",
                        Cause = "The eligible registered destination is absent.",
                    },
                ],
            },
        };
    }
}
