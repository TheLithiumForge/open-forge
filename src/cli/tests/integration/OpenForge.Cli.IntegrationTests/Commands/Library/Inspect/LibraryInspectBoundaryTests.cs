using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Reading;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Inspect;

public sealed class LibraryInspectBoundaryTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Library Inspect maps invalid unavailable and unsafe boundaries while observing unavailable ownership without a gate"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    [InlineData("malformed-record", (int)CliSemanticStatus.Incomplete, (int)LibraryInspectFindingCode.RecordInvalid)]
    [InlineData("duplicate-id", (int)CliSemanticStatus.Incomplete, (int)LibraryInspectFindingCode.RecordInvalid)]
    [InlineData("record-link", (int)CliSemanticStatus.Complete, (int)LibraryInspectFindingCode.OwnershipObservation)]
    [InlineData("missing-source", (int)CliSemanticStatus.Incomplete, (int)LibraryInspectFindingCode.SourceRootUnavailable)]
    [InlineData("source-file", (int)CliSemanticStatus.Invalid, (int)LibraryInspectFindingCode.SourceRootInvalid)]
    [InlineData("source-link", (int)CliSemanticStatus.Blocked, (int)LibraryInspectFindingCode.SourceRootBlocked)]
    [InlineData("destination-parent-link", (int)CliSemanticStatus.Blocked, (int)LibraryInspectFindingCode.LinkBlocked)]
    public static async Task BoundaryStatus(string scenario, int status, int code)
    {
        using var fixture = new LibraryReadWorkspace();
        LibraryReadBoundary.Arrange(fixture, scenario);
        var before = fixture.Snapshot();
        var result = await fixture.InspectAsync(TestContext.Current.CancellationToken);

        Assert.Equal((CliSemanticStatus)status, result.Status);
        Assert.Contains(result.Result.Findings, finding => finding.Code == (LibraryInspectFindingCode)code);
        Assert.Equal(scenario is "record-link",
            result.Result.Projection.State == LibraryCoverage.Complete);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Library Inspect retains an unknown supplied ID without selecting a nearby record"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    [InlineData("absent")]
    [InlineData("strict-empty")]
    [InlineData("other-id")]
    public static async Task UnknownSuppliedIdIsInvalid(string record)
    {
        using var fixture = new LibraryReadWorkspace();
        if (record == "strict-empty")
        {
            fixture.Files.WriteText(LibraryReadWorkspace.RecordPath, "{\"schemaVersion\":1,\"libraries\":[]}");
        }
        else if (record == "other-id")
        {
            fixture.Record();
        }

        var before = fixture.Snapshot();
        var result = await fixture.InspectAsync(TestContext.Current.CancellationToken, "team-knowledge-unknown");

        Assert.Equal(record == "absent" ? CliSemanticStatus.Complete : CliSemanticStatus.Invalid, result.Status);
        Assert.Equal("team-knowledge-unknown", result.Result.Record.Id);
        Assert.Null(result.Result.Record.SourceRoot);
        Assert.Empty(result.Result.Record.RegisteredPaths);
        Assert.Equal(LibraryInventoryViewState.NotStarted, result.Result.Source.State);
        Assert.Empty(result.Result.Source.EligiblePaths);
        Assert.Empty(result.Result.Projection.Comparisons);
        Assert.Contains(result.Result.Findings, finding => finding.Code ==
            (record == "absent" ? LibraryInspectFindingCode.OwnershipObservation : LibraryInspectFindingCode.UnknownId));
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Library Inspect treats an unreadable existing record as an ownership observation rather than unknown ID"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    public async Task UnavailableRecordIsNotUnknownId()
    {
        using var fixture = new LibraryReadWorkspace();
        fixture.Record();
        var before = fixture.Snapshot();
        using (var held = new FileStream(fixture.Files.Combine(LibraryReadWorkspace.RecordPath), FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            var result = await fixture.InspectAsync(TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal(LibraryRecordViewState.Unavailable, result.Result.Record.State);
            Assert.Equal(LibraryInventoryViewState.NotStarted, result.Result.Source.State);
            Assert.Contains(result.Result.Findings, finding => finding.Code == LibraryInspectFindingCode.OwnershipObservation);
            Assert.DoesNotContain(result.Result.Findings, finding => finding.Code == LibraryInspectFindingCode.UnknownId);
        }

        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Library Inspect interruption precedes record work and retains no inventory claim"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    public async Task CancellationAtIngress()
    {
        using var fixture = new LibraryReadWorkspace();
        LibraryReadBoundary.Arrange(fixture, "malformed-record");
        var before = fixture.Snapshot();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var result = await fixture.InspectAsync(cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains(result.Result.Findings, finding => finding.Code == LibraryInspectFindingCode.Interrupted);
        Assert.Empty(result.Result.Source.EligiblePaths);
        Assert.Empty(result.Result.Projection.Comparisons);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }
}
