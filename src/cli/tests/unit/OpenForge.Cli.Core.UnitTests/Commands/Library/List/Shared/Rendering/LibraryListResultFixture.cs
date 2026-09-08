using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Reading;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.List.Shared.Rendering;

internal static class LibraryListResultFixture
{
    internal static LibraryListResult Create(CliSemanticStatus status = CliSemanticStatus.Attention)
    {
        var seed = Drift();
        if (status == CliSemanticStatus.Attention)
        {
            return seed;
        }

        if (status == CliSemanticStatus.Complete)
        {
            var library = seed.Result.Libraries[0];
            var path = library.Paths[0] with { State = LibraryLinkViewState.Current, ObservedRelativeLink = library.Paths[0].ExpectedRelativeLink };
            return seed with { Status = status, Result = seed.Result with { Libraries = [library with { Paths = [path] }], Findings = [] } };
        }

        var (record, coverage, finding) = status switch
        {
            CliSemanticStatus.Incomplete => (LibraryRecordViewState.Unavailable, LibraryCoverage.Incomplete, LibraryListFindingCode.RecordUnavailable),
            CliSemanticStatus.Invalid => (LibraryRecordViewState.Invalid, LibraryCoverage.NotStarted, LibraryListFindingCode.InvalidRecord),
            CliSemanticStatus.Blocked => (LibraryRecordViewState.Blocked, LibraryCoverage.Blocked, LibraryListFindingCode.RecordBlocked),
            CliSemanticStatus.Failed => (LibraryRecordViewState.Failed, LibraryCoverage.Failed, LibraryListFindingCode.OperationFailed),
            CliSemanticStatus.Interrupted => (LibraryRecordViewState.Interrupted, LibraryCoverage.Interrupted, LibraryListFindingCode.Interrupted),
            _ => throw new ArgumentOutOfRangeException(nameof(status)),
        };
        return seed with
        {
            Status = status,
            Result = seed.Result with
            {
                Record = seed.Result.Record with { State = record, LibraryCount = null },
                Coverage = coverage,
                Libraries = [],
                Findings = [seed.Result.Findings[0] with { Code = finding, Status = status, LibraryId = null, Path = ".agents/open-forge.libraries.json", Cause = "Record observation stopped." }],
            },
        };
    }

    private static LibraryListResult Drift()
        => new()
        {
            Status = CliSemanticStatus.Attention,
            Workspace = LibraryReadInputs.Workspace,
            Result = new LibraryListPayload
            {
                Record = new LibraryListRecordView { Path = ".agents/open-forge.libraries.json", State = LibraryRecordViewState.Complete, LibraryCount = 1 },
                Libraries =
                [
                    new LibraryListView
                    {
                        Id = "team-knowledge",
                        SourceRoot = "shared/team",
                        SourceRootState = LibrarySourceRootViewState.Available,
                        Paths =
                        [
                            new LibraryListRegisteredPath
                            {
                                SourcePath = ".agents/directives/review.md",
                                DestinationPath = ".agents/directives/review.md",
                                ExpectedRelativeLink = "../../shared/team/.agents/directives/review.md",
                                SourceId = "directives/review",
                                State = LibraryLinkViewState.Missing,
                                ObservedRelativeLink = null,
                            },
                        ],
                    },
                ],
                Inventory = LibraryListInventoryState.NotRequested,
                Coverage = LibraryCoverage.Complete,
                Findings =
                [
                    new LibraryListFinding
                    {
                        Code = LibraryListFindingCode.LinkMissing,
                        Status = CliSemanticStatus.Attention,
                        LibraryId = "team-knowledge",
                        Path = ".agents/directives/review.md",
                        Cause = "The registered destination is absent.",
                    },
                ],
            },
        };
}
