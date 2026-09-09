using System.Text.Json;
using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.UnitTests.Commands.Library.Shared.Reading;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Inspect.Shared.Rendering;

public sealed class LibraryInspectPresentationTests
{
    [Fact(DisplayName = "Library Inspect JSON preserves exact member order and separate registered and observed facts"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    public void ExactWireGraph()
    {
        using var document = JsonDocument.Parse(Render(LibraryInspectResultFixture.Create()));
        var root = document.RootElement;
        LibraryReadPresentationAssertions.Envelope(root, "library inspect", "attention");
        var result = root.GetProperty("result");
        LibraryReadPresentationAssertions.Members(result, "record", "source", "projection", "findings");
        var record = result.GetProperty("record");
        LibraryReadPresentationAssertions.Members(record, "path", "state", "id", "sourceRoot", "destinationRoot", "registeredPaths");
        Assert.Equal("team-knowledge", record.GetProperty("id").GetString());
        var registered = Assert.Single(record.GetProperty("registeredPaths").EnumerateArray());
        LibraryReadPresentationAssertions.Members(registered, "sourcePath", "destinationPath", "expectedRelativeLink", "sourceId");
        Assert.Equal("directives/review", registered.GetProperty("sourceId").GetString());
        var source = result.GetProperty("source");
        LibraryReadPresentationAssertions.Members(source, "rootState", "state", "eligiblePaths");
        Assert.Equal("available", source.GetProperty("rootState").GetString());
        Assert.Equal("complete", source.GetProperty("state").GetString());
        var eligible = Assert.Single(source.GetProperty("eligiblePaths").EnumerateArray());
        LibraryReadPresentationAssertions.Members(eligible, "sourcePath", "destinationPath", "sourceId");
        var projection = result.GetProperty("projection");
        LibraryReadPresentationAssertions.Members(projection, "state", "comparisons");
        var comparison = Assert.Single(projection.GetProperty("comparisons").EnumerateArray());
        LibraryReadPresentationAssertions.Members(comparison, "sourcePath", "destinationPath", "sourceId", "relation", "registered", "observedRelativeLink");
        Assert.Equal("missing", comparison.GetProperty("relation").GetString());
        foreach (var property in registered.EnumerateObject())
        {
            Assert.Equal(property.Value.GetRawText(), comparison.GetProperty("registered").GetProperty(property.Name).GetRawText());
        }
        Assert.Equal(JsonValueKind.Null, comparison.GetProperty("observedRelativeLink").ValueKind);
        var finding = Assert.Single(result.GetProperty("findings").EnumerateArray());
        LibraryReadPresentationAssertions.Members(finding, "code", "status", "libraryId", "path", "cause");
        Assert.Equal("library-inspect.link-missing", finding.GetProperty("code").GetString());
    }

    [Fact(DisplayName = "Library Inspect stopped stages preserve null identity and all empty arrays"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    public void UnknownFactsRemainNull()
    {
        var seed = LibraryInspectResultFixture.Create(CliSemanticStatus.Invalid);
        var result = seed with
        {
            Workspace = null,
            Result = seed.Result with
            {
                Record = seed.Result.Record with { Id = null, SourceRoot = null, DestinationRoot = null, RegisteredPaths = [] },
                Source = seed.Result.Source with { EligiblePaths = [] },
                Projection = seed.Result.Projection with { Comparisons = [] },
            },
        };
        using var document = JsonDocument.Parse(Render(result));
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("workspace").ValueKind);
        var payload = document.RootElement.GetProperty("result");
        Assert.Equal(JsonValueKind.Null, payload.GetProperty("record").GetProperty("id").ValueKind);
        Assert.Equal(JsonValueKind.Null, payload.GetProperty("record").GetProperty("sourceRoot").ValueKind);
        Assert.Empty(payload.GetProperty("record").GetProperty("registeredPaths").EnumerateArray());
        Assert.Empty(payload.GetProperty("source").GetProperty("eligiblePaths").EnumerateArray());
        Assert.Empty(payload.GetProperty("projection").GetProperty("comparisons").EnumerateArray());
    }

    [Theory(DisplayName = "Library Inspect executes every status exit and human or JSON stream coordinate"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    [InlineData((int)CliSemanticStatus.Complete, "complete", 0)]
    [InlineData((int)CliSemanticStatus.Attention, "attention", 2)]
    [InlineData((int)CliSemanticStatus.Incomplete, "incomplete", 3)]
    [InlineData((int)CliSemanticStatus.Invalid, "invalid", 4)]
    [InlineData((int)CliSemanticStatus.Blocked, "blocked", 5)]
    [InlineData((int)CliSemanticStatus.Failed, "failed", 1)]
    [InlineData((int)CliSemanticStatus.Interrupted, "interrupted", 130)]
    public Task StatusCoordinates(int status, string text, int exit)
        => LibraryReadPresentationAssertions.StreamsAsync(
            LibraryInspectResultFixture.Create((CliSemanticStatus)status),
            new CliRendererSet<LibraryInspectResult>(LibraryInspectPresentation.RenderHuman, LibraryInspectPresentation.RenderJson), text, exit);

    [Fact(DisplayName = "Library Inspect compact expanded and JSON projections retain identity relation and finding facts"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    public void SameFactsAcrossPresentations()
    {
        var result = LibraryInspectResultFixture.Create();
        var json = Render(result);
        foreach (var view in new[] { CliView.Compact, CliView.Expanded })
        {
            foreach (var verbosity in new[] { CliVerbosity.Normal, CliVerbosity.Verbose })
            {
                var human = LibraryInspectPresentation.RenderHuman(new(result, new(CliOutputFormat.Human, view, verbosity)));
                foreach (var fact in new[] { "team-knowledge", "shared/team", ".agents/directives/review.md", "directives/review", "missing", "attention", "complete" })
                {
                    Assert.Contains(fact, human, StringComparison.Ordinal);
                }

                Assert.Contains("library-inspect.link-missing", human, StringComparison.Ordinal);
                Assert.Equal(json, LibraryInspectPresentation.RenderJson(new(result, new(CliOutputFormat.Json, view, verbosity))));
            }
        }
    }

    private static string Render(LibraryInspectResult result)
        => LibraryInspectPresentation.RenderJson(new(result, new(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal)));
}
