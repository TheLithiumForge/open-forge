using OpenForge.Cli.Core.Commands.Library.Inspect.Models.Result;
using OpenForge.Cli.Core.Presentation.Library.Inspect;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Library.Inspect.Shared.Rendering;

public sealed class LibraryInspectHumanViewTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library Inspect native minimal current output is one sentence")]
    public void CurrentMinimalIsOneLine()
    {
        var result = LibraryInspectResultFixture.Create(CliSemanticStatus.Complete);
        var text = Render(result, CliDetail.Minimal);

        Assert.StartsWith("team-knowledge is current: 1 file from shared/team is linked under the destination folder.\n", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library Inspect native minimal attention output names every differing file")]
    public void AttentionMinimalListsDifferences()
    {
        var text = Render(LibraryInspectResultFixture.Create(), CliDetail.Minimal);

        Assert.Contains("team-knowledge needs a sync: 1 file differs between shared/team and the destination folder.", text, StringComparison.Ordinal);
        Assert.Contains(".agents/directives/review.md  missing", text, StringComparison.Ordinal);
        Assert.Contains("Next: open-forge library sync team-knowledge --dry-run", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Library Inspect native standard and full output add workspace, targets and inventory")]
    public void DetailAddsFactsByLevel()
    {
        var result = LibraryInspectResultFixture.Create();
        var minimal = Render(result, CliDetail.Minimal);
        var standard = Render(result, CliDetail.Standard);
        var full = Render(result, CliDetail.Full);

        Assert.Contains("Workspace:", standard, StringComparison.Ordinal);
        Assert.Contains(".agents/directives/review.md  missing", standard, StringComparison.Ordinal);
        Assert.DoesNotContain("Expected target:", standard, StringComparison.Ordinal);
        Assert.Contains("Expected target:", full, StringComparison.Ordinal);
        Assert.Contains("Observed target:", full, StringComparison.Ordinal);
        Assert.Contains("Source inventory: 1 eligible file, 0 excluded items.", full, StringComparison.Ordinal);
    }

    private static string Render(LibraryInspectResult result, CliDetail detail)
        => CliRenderingStage.Render(
            new CliPresentationRequest<LibraryInspectResult>(result, new(CliFormat.Text, detail, null)),
            LibraryInspectPresentation.Rendering).PrimaryContent;
}
