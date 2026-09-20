using OpenForge.Cli.Core.Commands.Library.List.Shared.Serialization;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.List;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Reading;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.List;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class LibraryListBeforeOutputSnapshotTests
{
    private static readonly CommandOutputRenderers<LibraryListResult> Renderers = CommandOutputRenderers<LibraryListResult>.From(LibraryListPresentation.Rendering);

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Library list output preserves empty, current and drifted registered links")]
    [InlineData("none-registered", (int)CliSemanticStatus.Complete)]
    [InlineData("one-current", (int)CliSemanticStatus.Complete)]
    [InlineData("link-missing", (int)CliSemanticStatus.Attention)]
    [InlineData("link-changed", (int)CliSemanticStatus.Attention)]
    [InlineData("no-ownership-record", (int)CliSemanticStatus.Complete)]
    public async Task RegisteredLinks(string situation, int status)
    {
        using var fixture = new LibraryReadWorkspace();
        if (situation == "none-registered")
        {
            fixture.Files.WriteText(LibraryReadWorkspace.RecordPath, "{\"schemaVersion\":1,\"libraries\":[]}");
        }
        else if (situation != "no-ownership-record")
        {
            fixture.Source();
            fixture.SourceFile();
            fixture.Record(LibraryReadWorkspace.ReviewPath);
            if (situation == "one-current") fixture.CurrentLink();
            if (situation == "link-changed") fixture.Files.WriteText(LibraryReadWorkspace.ReviewPath, "local occupant\n");
        }

        var before = fixture.Snapshot();
        var result = await fixture.ListAsync(TestContext.Current.CancellationToken);
        Assert.Equal((CliSemanticStatus)status, result.Status);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
        Renderers.MatchDetails(result, situation, testName: $"{nameof(RegisteredLinks)}_{situation}");
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Library list output preserves source, ownership and link boundary findings")]
    [InlineData("source-folder-missing", "missing-source", (int)CliSemanticStatus.Attention)]
    [InlineData("record-invalid", "malformed-record", (int)CliSemanticStatus.Incomplete)]
    [InlineData("link-blocked", "destination-parent-link", (int)CliSemanticStatus.Blocked)]
    public async Task ObservationBoundary(string situation, string seed, int status)
    {
        using var fixture = new LibraryReadWorkspace();
        LibraryReadBoundary.Arrange(fixture, seed);
        var before = fixture.Snapshot();
        var result = await fixture.ListAsync(TestContext.Current.CancellationToken);
        Assert.Equal((CliSemanticStatus)status, result.Status);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
        Renderers.MatchDetails(result, situation, testName: $"{nameof(ObservationBoundary)}_{situation}");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library list output preserves an existing unreadable ownership record")]
    public async Task RecordUnreadable()
    {
        using var fixture = new LibraryReadWorkspace();
        fixture.Record();
        var before = fixture.Snapshot();
        LibraryListResult result;
        using (var held = File.Open(fixture.Files.Combine(LibraryReadWorkspace.RecordPath), FileMode.Open, FileAccess.Read, FileShare.None))
        {
            result = await fixture.ListAsync(TestContext.Current.CancellationToken);
        }

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(before, fixture.Snapshot());
        Renderers.MatchDetails(result, "record-unreadable");
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library list output preserves invalid operands without effects")]
    public async Task InvalidInput()
    {
        using var fixture = new LibraryReadWorkspace();
        var before = fixture.Snapshot();
        await new ReadCommandOutputCapture(fixture.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = "invalid-input",
            Arguments = ["library", "list", "unexpected-operand"],
            ExitCode = 4,
            ShellDiagnostic = true,
        }, testName: nameof(InvalidInput));
        Assert.Equal(before, fixture.Snapshot());
    }
}
