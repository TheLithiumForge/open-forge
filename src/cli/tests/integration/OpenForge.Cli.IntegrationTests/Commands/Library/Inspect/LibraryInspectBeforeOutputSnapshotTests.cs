using OpenForge.Cli.Core.Commands.Library.Inspect.Shared.Serialization;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.Inspect;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Reading;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport.Filesystem;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Inspect;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class LibraryInspectBeforeOutputSnapshotTests
{
    private static readonly CommandOutputRenderers<LibraryInspectResult> Renderers = CommandOutputRenderers<LibraryInspectResult>.From(LibraryInspectPresentation.Rendering);

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library inspect output preserves an inaccessible source inventory without effects")]
    public async Task SourceUnreadable()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("This source enumeration boundary uses an owned Windows directory ACL.");
            return;
        }
        using var fixture = new LibraryReadWorkspace();
        fixture.Source();
        fixture.SourceFile();
        fixture.Record(LibraryReadWorkspace.ReviewPath);
        fixture.CurrentLink();
        var before = fixture.Snapshot();
        LibraryInspectResult result;
        using (var denied = WindowsDirectoryEnumerationDenial.Create(fixture.Path,
                   fixture.Files.Combine(LibraryReadWorkspace.SourceRoot + "/.agents")))
        {
            result = await fixture.InspectAsync(TestContext.Current.CancellationToken);
        }
        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(before, fixture.Snapshot());
        Renderers.MatchDetails(result, "source-unreadable");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library inspect output preserves a blocked destination mapping without effects")]
    public async Task BlockedMapping()
    {
        using var fixture = new LibraryReadWorkspace();
        LibraryReadBoundary.Arrange(fixture, "destination-parent-link");
        var before = fixture.Snapshot();
        var result = await fixture.InspectAsync(TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
        Renderers.MatchDetails(result, "blocked-mapping");
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Library inspect output preserves complete source and registered link comparisons")]
    [InlineData("current", (int)CliSemanticStatus.Complete)]
    [InlineData("added-source-files", (int)CliSemanticStatus.Attention)]
    [InlineData("retired-source-files", (int)CliSemanticStatus.Attention)]
    [InlineData("missing-links", (int)CliSemanticStatus.Attention)]
    [InlineData("changed-links", (int)CliSemanticStatus.Attention)]
    [InlineData("empty-source", (int)CliSemanticStatus.Complete)]
    public async Task SourceComparison(string situation, int status)
    {
        using var fixture = new LibraryReadWorkspace();
        fixture.Source();
        if (situation == "empty-source")
        {
            fixture.Record();
        }
        else
        {
            if (situation != "retired-source-files") fixture.SourceFile();
            fixture.Record(LibraryReadWorkspace.ReviewPath);
            if (situation == "changed-links") fixture.Files.WriteText(LibraryReadWorkspace.ReviewPath, "changed local occupant\n");
            else if (situation != "missing-links") fixture.CurrentLink();
            if (situation == "added-source-files") fixture.SourceFile(".agents/directives/added.md");
        }

        var before = fixture.Snapshot();
        var result = await fixture.InspectAsync(TestContext.Current.CancellationToken);
        Assert.Equal((CliSemanticStatus)status, result.Status);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
        Renderers.MatchDetails(result, situation, testName: $"{nameof(SourceComparison)}_{situation}");
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Library inspect output preserves missing ownership, unknown IDs and invalid IDs")]
    [InlineData("unknown-id", 4)]
    [InlineData("no-ownership-record", 0)]
    [InlineData("invalid-id", 4)]
    public async Task Selection(string situation, int exitCode)
    {
        using var fixture = new LibraryReadWorkspace();
        if (situation != "no-ownership-record") fixture.Record();
        var id = situation switch
        {
            "invalid-id" => "INVALID ID",
            "unknown-id" => "unknown",
            _ => "team-knowledge",
        };
        var before = fixture.Snapshot();
        await new ReadCommandOutputCapture(fixture.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = situation,
            Arguments = ["library", "inspect", id],
            ExitCode = exitCode,
        }, testName: $"{nameof(Selection)}_{situation}");
        Assert.Equal(before, fixture.Snapshot());
    }
}
