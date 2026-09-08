using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Reading;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.List;

public sealed class LibraryListBoundaryTests
{
    [Theory(DisplayName = "Library List maps strict record source and unsafe observation boundaries without empty substitution"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    [InlineData("malformed-record", (int)CliSemanticStatus.Invalid, (int)LibraryListFindingCode.InvalidRecord)]
    [InlineData("duplicate-id", (int)CliSemanticStatus.Blocked, (int)LibraryListFindingCode.RecordBlocked)]
    [InlineData("record-link", (int)CliSemanticStatus.Blocked, (int)LibraryListFindingCode.RecordBlocked)]
    [InlineData("missing-source", (int)CliSemanticStatus.Incomplete, (int)LibraryListFindingCode.SourceRootUnavailable)]
    [InlineData("source-file", (int)CliSemanticStatus.Invalid, (int)LibraryListFindingCode.SourceRootInvalid)]
    [InlineData("missing-agents", (int)CliSemanticStatus.Invalid, (int)LibraryListFindingCode.SourceRootInvalid)]
    [InlineData("source-link", (int)CliSemanticStatus.Blocked, (int)LibraryListFindingCode.SourceRootBlocked)]
    [InlineData("consumer-overlap", (int)CliSemanticStatus.Blocked, (int)LibraryListFindingCode.SourceRootBlocked)]
    [InlineData("destination-parent-link", (int)CliSemanticStatus.Blocked, (int)LibraryListFindingCode.LinkBlocked)]
    public static async Task BoundaryStatus(string scenario, int status, int code)
    {
        using var fixture = new LibraryReadWorkspace();
        LibraryReadBoundary.Arrange(fixture, scenario);
        var before = fixture.Snapshot();
        var result = await fixture.ListAsync(TestContext.Current.CancellationToken);

        Assert.Equal((CliSemanticStatus)status, result.Status);
        Assert.Contains(result.Result.Findings, finding => finding.Code == (LibraryListFindingCode)code);
        Assert.Equal(LibraryListInventoryState.NotRequested, result.Result.Inventory);
        if (scenario is "malformed-record" or "duplicate-id" or "record-link")
        {
            Assert.Null(result.Result.Record.LibraryCount);
            Assert.Empty(result.Result.Libraries);
        }

        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }

    [Fact(DisplayName = "Library List preserves unavailable record coverage when an existing record cannot be read"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    public async Task UnavailableRecordIsNotMissing()
    {
        using var fixture = new LibraryReadWorkspace();
        fixture.Record();
        var before = fixture.Snapshot();
        using (var held = new FileStream(fixture.Files.Combine(LibraryReadWorkspace.RecordPath), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            var result = await fixture.ListAsync(TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
            Assert.Equal(LibraryRecordViewState.Unavailable, result.Result.Record.State);
            Assert.Null(result.Result.Record.LibraryCount);
            Assert.Equal(LibraryCoverage.Incomplete, result.Result.Coverage);
            Assert.Contains(result.Result.Findings, finding => finding.Code == LibraryListFindingCode.RecordUnavailable);
        }

        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }

    [Fact(DisplayName = "Library List caller interruption precedes malformed record work and preserves all state"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    public async Task CancellationAtIngress()
    {
        using var fixture = new LibraryReadWorkspace();
        LibraryReadBoundary.Arrange(fixture, "malformed-record");
        var before = fixture.Snapshot();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var result = await fixture.ListAsync(cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains(result.Result.Findings, finding => finding.Code == LibraryListFindingCode.Interrupted);
        Assert.Empty(result.Result.Libraries);
        Assert.Null(result.Result.Record.LibraryCount);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }

    [Fact(DisplayName = "Library List never traverses an inaccessible unregistered source subtree"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    public async Task SourceInventoryIsNeverRequested()
    {
        if (!OperatingSystem.IsLinux())
        {
            throw new PlatformNotSupportedException("Required permission evidence targets Linux.");
        }
        using var fixture = new LibraryReadWorkspace();
        fixture.Source();
        fixture.SourceFile();
        fixture.Record(LibraryReadWorkspace.ReviewPath);
        fixture.CurrentLink();
        var inaccessible = fixture.Files.CreateDirectory("shared/team/.agents/unregistered");
        var before = fixture.Snapshot();
        var mode = File.GetUnixFileMode(inaccessible);
        try
        {
            File.SetUnixFileMode(inaccessible, UnixFileMode.None);
            var result = await fixture.ListAsync(TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal(LibraryListInventoryState.NotRequested, result.Result.Inventory);
            Assert.Single(Assert.Single(result.Result.Libraries).Paths);
        }
        finally
        {
            File.SetUnixFileMode(inaccessible, mode);
        }

        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }
}
