using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.List.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Reading;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.List.Shared.Rendering;

public sealed class LibraryListPresentationTests
{
    [Fact(DisplayName = "Library List JSON preserves the exact complete ordered wire graph and nullable facts"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    public void ExactWireGraph()
    {
        using var document = JsonDocument.Parse(Render(LibraryListResultFixture.Create()));
        var root = document.RootElement;
        LibraryReadPresentationAssertions.Envelope(root, "library list", "attention");
        var result = root.GetProperty("result");
        LibraryReadPresentationAssertions.Members(result, "record", "libraries", "inventory", "coverage", "findings");
        var record = result.GetProperty("record");
        LibraryReadPresentationAssertions.Members(record, "path", "state", "libraryCount");
        Assert.Equal(".agents/open-forge.libraries.json", record.GetProperty("path").GetString());
        Assert.Equal(1, record.GetProperty("libraryCount").GetInt32());
        var library = Assert.Single(result.GetProperty("libraries").EnumerateArray());
        LibraryReadPresentationAssertions.Members(library, "id", "sourceRoot", "sourceRootState", "paths");
        Assert.Equal("team-knowledge", library.GetProperty("id").GetString());
        var path = Assert.Single(library.GetProperty("paths").EnumerateArray());
        LibraryReadPresentationAssertions.Members(path, "sourcePath", "destinationPath", "expectedRelativeLink", "sourceId", "state", "observedRelativeLink");
        Assert.Equal("directives/review", path.GetProperty("sourceId").GetString());
        Assert.Equal("../../shared/team/.agents/directives/review.md", path.GetProperty("expectedRelativeLink").GetString());
        Assert.Equal("missing", path.GetProperty("state").GetString());
        Assert.Equal(JsonValueKind.Null, path.GetProperty("observedRelativeLink").ValueKind);
        Assert.Equal("not-requested", result.GetProperty("inventory").GetString());
        Assert.Equal("complete", result.GetProperty("coverage").GetString());
        var finding = Assert.Single(result.GetProperty("findings").EnumerateArray());
        LibraryReadPresentationAssertions.Members(finding, "code", "status", "libraryId", "path", "cause");
        Assert.Equal("library-list.link-missing", finding.GetProperty("code").GetString());
        Assert.Equal("attention", finding.GetProperty("status").GetString());
    }

    [Fact(DisplayName = "Library List JSON retains unknown counts and absent workspace instead of claiming zero"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    public void UnknownFactsRemainNull()
    {
        var seed = LibraryListResultFixture.Create(CliSemanticStatus.Incomplete);
        var result = seed with
        {
            Workspace = null,
            Result = seed.Result with { Record = seed.Result.Record with { LibraryCount = null }, Libraries = [] },
        };
        using var document = JsonDocument.Parse(Render(result));
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("workspace").ValueKind);
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("result").GetProperty("record").GetProperty("libraryCount").ValueKind);
        Assert.Empty(document.RootElement.GetProperty("result").GetProperty("libraries").EnumerateArray());
    }

    [Theory(DisplayName = "Library List executes every status exit and human or JSON stream coordinate"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    [InlineData((int)CliSemanticStatus.Complete, "complete", 0)]
    [InlineData((int)CliSemanticStatus.Attention, "attention", 2)]
    [InlineData((int)CliSemanticStatus.Incomplete, "incomplete", 3)]
    [InlineData((int)CliSemanticStatus.Invalid, "invalid", 4)]
    [InlineData((int)CliSemanticStatus.Blocked, "blocked", 5)]
    [InlineData((int)CliSemanticStatus.Failed, "failed", 1)]
    [InlineData((int)CliSemanticStatus.Interrupted, "interrupted", 130)]
    public Task StatusCoordinates(int status, string text, int exit)
        => LibraryReadPresentationAssertions.StreamsAsync(
            LibraryListResultFixture.Create((CliSemanticStatus)status),
            new CliRendererSet<LibraryListResult>(LibraryListPresentation.RenderHuman, LibraryListPresentation.RenderJson), text, exit);

    [Fact(DisplayName = "Library List views retain finding and identity facts while JSON ignores view and verbosity"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    public void SameFactsAcrossPresentations()
    {
        var result = LibraryListResultFixture.Create();
        var json = Render(result);
        foreach (var view in new[] { CliView.Compact, CliView.Expanded })
        {
            foreach (var verbosity in new[] { CliVerbosity.Normal, CliVerbosity.Verbose })
            {
                var human = LibraryListPresentation.RenderHuman(new(result, new(CliOutputFormat.Human, view, verbosity)));
                foreach (var fact in new[] { "team-knowledge", "shared/team", ".agents/directives/review.md", "directives/review", "missing", "attention", "complete" })
                {
                    Assert.Contains(fact, human, StringComparison.Ordinal);
                }

                Assert.Contains("library-list.link-missing", human, StringComparison.Ordinal);
                Assert.Equal(json, LibraryListPresentation.RenderJson(new(result, new(CliOutputFormat.Json, view, verbosity))));
            }
        }
    }

    private static string Render(LibraryListResult result)
        => LibraryListPresentation.RenderJson(new(result, new(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal)));
}
