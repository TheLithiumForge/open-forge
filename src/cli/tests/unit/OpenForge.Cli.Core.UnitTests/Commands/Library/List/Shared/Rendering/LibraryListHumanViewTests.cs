using OpenForge.Cli.Core.Commands.Library.List.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.List.Shared.Rendering;

public sealed class LibraryListHumanViewTests
{
    [Fact(DisplayName = "Library List compact wording matches the reviewed small fixture snapshot"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    public void CompactSnapshot()
    {
        var result = LibraryListResultFixture.Create();
        var text = LibraryListPresentation.RenderHuman(new(result, new(CliOutputFormat.Human, CliView.Compact, CliVerbosity.Normal)));
        var expected = $"""
            1 Library registered.
            Status: requires attention
            Workspace: {result.Workspace?.LexicalRoot}
            Selected by: --workspace
            Record: complete (.agents/open-forge.libraries.json)
            Registered Libraries: 1
            Source inventory: not scanned (library list checks registered links only)
            Checks: complete
            REQUIRES ATTENTION: The registered destination is absent. [library-list.link-missing]
              .agents/directives/review.md
              Library: team-knowledge

            Library: team-knowledge
              Source root: shared/team (available)
              Destination root: .
              .agents/directives/review.md -> .agents/directives/review.md; missing; ID directives/review
            """;
        Assert.Equal(expected.ReplaceLineEndings(), text);
    }

    [Fact(DisplayName = "Library List views retain exact mappings and unknown counts without implying an empty registration"), Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
    public void ViewsPreserveKnownAndUnknownFacts()
    {
        var result = LibraryListResultFixture.Create();
        var compact = LibraryListPresentation.RenderHuman(new(result, new(CliOutputFormat.Human, CliView.Compact, CliVerbosity.Normal)));
        var expanded = LibraryListPresentation.RenderHuman(new(result, new(CliOutputFormat.Human, CliView.Expanded, CliVerbosity.Normal)));
        Assert.StartsWith("1 Library registered.", compact, StringComparison.Ordinal);
        foreach (var text in new[] { compact, expanded })
        {
            Assert.Contains("Source inventory: not scanned", text, StringComparison.Ordinal);
            Assert.Contains("Destination root: .", text, StringComparison.Ordinal);
            Assert.Contains(".agents/directives/review.md -> .agents/directives/review.md; missing; ID directives/review", text, StringComparison.Ordinal);
            Assert.Contains("REQUIRES ATTENTION: The registered destination is absent. [library-list.link-missing]", text, StringComparison.Ordinal);
        }
        Assert.DoesNotContain("Expected target:", compact, StringComparison.Ordinal);
        Assert.Contains("Expected target: ../../shared/team/.agents/directives/review.md", expanded, StringComparison.Ordinal);
        var unknown = LibraryListResultFixture.Create(CliSemanticStatus.Incomplete);
        var unavailable = LibraryListPresentation.RenderHuman(new(unknown, new(CliOutputFormat.Human, CliView.Compact, CliVerbosity.Normal)));
        Assert.Contains("Registered Libraries: unavailable", unavailable, StringComparison.Ordinal);
        Assert.DoesNotContain("No Libraries are registered", unavailable, StringComparison.Ordinal);
        var empty = unknown with { Result = unknown.Result with { Record = unknown.Result.Record with { State = LibraryRecordViewState.Complete, LibraryCount = 0 } } };
        Assert.StartsWith("No Libraries are registered.", LibraryListPresentation.RenderHuman(new(empty, new(CliOutputFormat.Human, CliView.Compact, CliVerbosity.Normal))), StringComparison.Ordinal);
    }
}
