using System.Text;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.Inspect;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Models;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Selection;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Rendering;

public sealed class RouteInspectHumanViewTests
{
    [Fact(DisplayName = "All-files presentation reports an unmeasured answer without scan labels"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void AllFilesHasNoScanLabels()
    {
        var scope = RouteInspectReportSelector.MatchingScope(RouteInspectMatchingFilesScope.AllFiles);
        Assert.Equal("all-files", scope.Name);
        Assert.Null(scope.Description);
        var files = new RouteInspectMatchingFilesData
        {
            Scope = scope.Name,
            Complete = true,
            Count = null,
            Paths = [],
            PathLimit = 100,
            Truncated = false,
            Reason = null,
            Note = "No effective applyTo restriction. Every file applies. No scan was run.",
        };
        var builder = new StringBuilder();
        RouteInspectMatchingFilesTextRenderer.Render(builder, files);
        Assert.Equal("\nMatching files: all files\nNo effective applyTo restriction. Every file applies. No scan was run.\n", builder.ToString());
    }

    [Theory(DisplayName = "Matching-files limitations preserve each finite reason and scope"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    [InlineData((int)RouteInspectMatchingFilesReason.SourceUnavailable, "source-unavailable", "The source could not be resolved for matching-file inspection.")]
    [InlineData((int)RouteInspectMatchingFilesReason.ConditionUnavailable, "condition-unavailable", "The source's effective applyTo conditions could not be established.")]
    [InlineData((int)RouteInspectMatchingFilesReason.GitUnavailable, "git-unavailable", "Git could not provide the workspace file inventory.")]
    [InlineData((int)RouteInspectMatchingFilesReason.ScanTimeout, "scan-timeout", "The matching-file scan did not finish within 30 seconds.")]
    [InlineData((int)RouteInspectMatchingFilesReason.ScanFailed, "scan-failed", "The matching-file scan could not be completed.")]
    [InlineData((int)RouteInspectMatchingFilesReason.FilesUnavailable, "files-unavailable", "Required file information or .gitignore content could not be read or decoded.")]
    [InlineData((int)RouteInspectMatchingFilesReason.UnsafePath, "unsafe-path", "The matching-file scan encountered a path outside the workspace boundary.")]
    [InlineData((int)RouteInspectMatchingFilesReason.Cancelled, "cancelled", "The matching-file scan was cancelled.")]
    public void MatchingLimitationsAreExact(int reason, string name, string sentence)
    {
        foreach (var scope in new RouteInspectMatchingFilesScope?[] { null, RouteInspectMatchingFilesScope.GitTrackedAndUntracked, RouteInspectMatchingFilesScope.WorkspaceFiles })
        {
            var files = Assert.IsType<RouteInspectMatchingFilesData>(
                RouteInspectReportSelector.ProjectMatchingFiles(RouteInspectMatchingFiles.Unavailable(scope, (RouteInspectMatchingFilesReason)reason)));
            Assert.Equal(name, files.Reason);
            var builder = new StringBuilder();
            RouteInspectMatchingFilesTextRenderer.Render(builder, files);
            var scopeText = scope switch
            {
                null => "unavailable",
                RouteInspectMatchingFilesScope.GitTrackedAndUntracked => "existing Git tracked and untracked files, excluding ignored untracked files",
                RouteInspectMatchingFilesScope.WorkspaceFiles => "workspace files filtered by .gitignore using Open Forge rules",
                _ => throw new ArgumentOutOfRangeException(nameof(scope)),
            };
            var expected = $"""

                Matching files: unavailable
                Scan scope: {scopeText}
                Scan completeness: incomplete
                {sentence}

                """;
            Assert.Equal(expected.ReplaceLineEndings("\n"), builder.ToString().ReplaceLineEndings("\n"));
            Assert.Null(files.Count);
            Assert.Empty(files.Paths);
            Assert.False(files.Complete);
            Assert.False(files.Truncated);
        }
    }

    [Fact(DisplayName = "Matching-files limitations remain visible when source selection has no profile"),
     Trait("Feature", "route-inspect"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void MatchingFilesSurviveMissingProfile()
    {
        var basis = RouteInspectPresentationTestData.InvalidResult();
        var result = RouteInspectResult.Create(basis.Status, basis.Workspace, basis.Selection, null, null,
            basis.Observations, basis.Conditions, basis.Next,
            matchingFiles: RouteInspectMatchingFiles.Unavailable(null, RouteInspectMatchingFilesReason.SourceUnavailable));
        Assert.Contains("Matching files: unavailable", RenderText(result, CliDetail.Minimal), StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Route Inspect views show incomplete conditions before the profile and preserve each JSON level"), Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    [InlineData((int)CliDetail.Minimal)]
    [InlineData((int)CliDetail.Standard)]
    public void ConditionsPrecedeProfile(int view)
    {
        var result = RouteInspectPresentationTestData.IncompleteResult();
        var output = RenderText(result, (CliDetail)view);

        Assert.Contains("This file could not be measured", output, StringComparison.Ordinal);
        Assert.True(output.IndexOf("This file could not be measured", StringComparison.Ordinal)
            < output.IndexOf("Where this source belongs", StringComparison.Ordinal));
        Assert.Contains("Next: open-forge doctor", output, StringComparison.Ordinal);
        Assert.Equal(2, output.Split("This file could not be measured", StringSplitOptions.None).Length - 1);

        var json = RenderJson(result, CliDetail.Standard);
        Assert.Equal(json, RenderJson(result, CliDetail.Standard));
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Route Inspect human views preserve complete long subjects and the operation Next command"), Trait("Feature", "route-inspect"), Trait("Evidence", "Unit")]
    [InlineData((int)CliDetail.Minimal)]
    [InlineData((int)CliDetail.Standard)]
    public void FullSubjectAndNext(int view)
    {
        var basis = RouteInspectPresentationTestData.InvalidResult();
        var subject = new string('x', 9000) + "-end";
        var result = RouteInspectResult.Create(
            basis.Status, basis.Workspace, basis.Selection, null, null, [],
            [new RouteInspectCondition(RouteInspectConditionCode.InvalidSourceReference, CliSemanticStatus.Invalid, subject, "The source reference was rejected.")],
            basis.Next);
        var output = RenderText(result, (CliDetail)view);

        Assert.Contains(subject, output, StringComparison.Ordinal);
        Assert.Contains("Next: open-forge route inspect --help", output, StringComparison.Ordinal);
    }

    private static string RenderText(RouteInspectResult result, CliDetail detail)
        => CliRenderingStage.Render(
            RouteInspectPresentationTestData.Presentation(result, detail),
            RouteInspectPresentation.Rendering).PrimaryContent;

    private static string RenderJson(RouteInspectResult result, CliDetail detail)
        => CliRenderingStage.Render(
            RouteInspectPresentationTestData.Presentation(result, detail, CliFormat.Json),
            RouteInspectPresentation.Rendering).PrimaryContent;
}
