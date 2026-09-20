using OpenForge.Cli.Core.Commands.Library.List.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Models.Result.Coordinates.Observation;
using OpenForge.Cli.Core.Presentation.Library.List;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.List.Shared.Rendering;

public sealed class LibraryListTextViewTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library List native minimal text output contains rows and omits source inventory prose")]
    public void MinimalText()
    {
        var text = Render(LibraryListResultFixture.Create(), CliDetail.Minimal);
        Assert.Contains("team-knowledge", text, StringComparison.Ordinal);
        Assert.Contains("shared/team -> .", text, StringComparison.Ordinal);
        Assert.Contains(".agents/directives/review.md", text, StringComparison.Ordinal);
        Assert.Contains("1 missing", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Source inventory", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Selected by:", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library List native detail adds link targets only at full depth")]
    public void ViewsPreserveKnownAndUnknownFacts()
    {
        var compact = Render(LibraryListResultFixture.Create(), CliDetail.Minimal);
        var standard = Render(LibraryListResultFixture.Create(), CliDetail.Standard);
        var full = Render(LibraryListResultFixture.Create(), CliDetail.Full);
        Assert.DoesNotContain("Expected target:", compact, StringComparison.Ordinal);
        Assert.DoesNotContain("Expected target:", standard, StringComparison.Ordinal);
        Assert.Contains("Expected target:", full, StringComparison.Ordinal);
        Assert.DoesNotContain("Source inventory", full, StringComparison.Ordinal);

        var incomplete = LibraryListResultFixture.Create(CliSemanticStatus.Incomplete);
        var unavailable = RenderOutput(incomplete, CliDetail.Minimal);
        Assert.Equal(CliSemanticStatus.Incomplete, unavailable.Status);
        Assert.Equal(CliOutputTarget.StandardOutput, unavailable.PrimaryTarget);

        var empty = incomplete with
        {
            Workspace = null,
            Status = CliSemanticStatus.Complete,
            Result = incomplete.Result with
            {
                Record = incomplete.Result.Record with { State = LibraryRecordViewState.Complete, LibraryCount = 0 },
                Findings = [],
            },
        };
        Assert.Equal("No Libraries are registered.\nNext: open-forge library attach <id> <source-folder>\n",
            Render(empty, CliDetail.Minimal));
    }

    private static string Render(LibraryListResult result, CliDetail detail)
        => RenderOutput(result, detail).PrimaryContent;

    private static CliRenderedOutput RenderOutput(LibraryListResult result, CliDetail detail)
        => CliRenderingStage.Render(
            new CliPresentationRequest<LibraryListResult>(result, new(CliFormat.Text, detail, null)),
            LibraryListPresentation.Rendering);
}
