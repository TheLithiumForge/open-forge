using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Reading;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.List;

public sealed class LibraryListOrderingAndPrecedenceTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Library List preserves ordinal Library and canonical path order with destination-derived IDs"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    public async Task OrdinalIdentityOrder()
    {
        using var fixture = new LibraryReadWorkspace();
        fixture.Files.CreateDirectory("shared/alpha/.agents");
        fixture.Source();
        fixture.SourceFile(".agents/directives/Z.md");
        fixture.SourceFile(".agents/directives/a.md");
        fixture.CurrentLink(".agents/directives/Z.md");
        fixture.CurrentLink(".agents/directives/a.md");
        fixture.Files.WriteText(LibraryReadWorkspace.RecordPath, """
            {"schemaVersion":1,"libraries":[
              {"id":"alpha","sourceRoot":"shared/alpha","destinationRoot":".","paths":[]},
              {"id":"team-knowledge","sourceRoot":"shared/team","destinationRoot":".","paths":[".agents/directives/Z.md",".agents/directives/a.md"]}]}
            """);
        var before = fixture.Snapshot();
        var result = await fixture.ListAsync(TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(["alpha", "team-knowledge"], result.Result.Libraries.Select(library => library.Id));
        Assert.Equal([".agents/directives/Z.md", ".agents/directives/a.md"], result.Result.Libraries[1].Paths.Select(path => path.DestinationPath));
        Assert.Equal(["directives/Z", "directives/a"], result.Result.Libraries[1].Paths.Select(path => path.SourceId));
        Assert.Equal(2, result.Result.Record.LibraryCount);
        Assert.Equal(before, fixture.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Library List chooses invalid before blocked before incomplete before safe drift"), Trait("Feature", "library-read"), Trait("Evidence", "Integration")]
    [InlineData("incomplete", (int)CliSemanticStatus.Attention)]
    [InlineData("blocked", (int)CliSemanticStatus.Blocked)]
    [InlineData("invalid", (int)CliSemanticStatus.Invalid)]
    public static async Task MixedConditionPrecedence(string highest, int status)
    {
        using var fixture = new LibraryReadWorkspace();
        fixture.Source();
        fixture.SourceFile();
        var blocked = string.Empty;
        var invalid = string.Empty;
        if (highest is "blocked" or "invalid")
        {
            fixture.Files.CreateDirectory("shared/actual/.agents");
            fixture.Files.CreateDirectorySymbolicLink("shared/linked", "actual");
            blocked = ",{\"id\":\"c-blocked\",\"sourceRoot\":\"shared/linked\",\"destinationRoot\":\".\",\"paths\":[]}";
        }

        if (highest == "invalid")
        {
            fixture.Files.CreateFile("shared/invalid", "A source root must be a directory.");
            invalid = ",{\"id\":\"d-invalid\",\"sourceRoot\":\"shared/invalid\",\"destinationRoot\":\".\",\"paths\":[]}";
        }

        fixture.Files.WriteText(LibraryReadWorkspace.RecordPath, $$"""
            {"schemaVersion":1,"libraries":[
              {"id":"a-drift","sourceRoot":"shared/team","destinationRoot":".","paths":[".agents/directives/review.md"]},
              {"id":"b-unavailable","sourceRoot":"shared/missing","destinationRoot":".","paths":[]}{{blocked}}{{invalid}}]}
            """);
        var before = fixture.Snapshot();
        var result = await fixture.ListAsync(TestContext.Current.CancellationToken);

        Assert.Equal((CliSemanticStatus)status, result.Status);
        Assert.Contains(result.Result.Findings, finding => finding.Code == LibraryListFindingCode.LinkMissing);
        Assert.Contains(result.Result.Findings, finding => finding.Code == LibraryListFindingCode.SourceRootUnavailable);
        Assert.Equal(LibraryListInventoryState.NotRequested, result.Result.Inventory);
        Assert.Equal(result.Status switch
        {
            CliSemanticStatus.Attention => LibraryCoverage.Complete,
            CliSemanticStatus.Blocked => LibraryCoverage.Blocked,
            CliSemanticStatus.Invalid => LibraryCoverage.NotStarted,
            _ => throw new ArgumentOutOfRangeException(nameof(result.Status), result.Status, "Unexpected precedence status."),
        }, result.Result.Coverage);
        Assert.Equal(before, fixture.Snapshot());
        fixture.AssertNoPersistentState();
    }
}
