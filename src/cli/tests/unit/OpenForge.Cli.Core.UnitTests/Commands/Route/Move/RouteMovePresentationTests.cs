using System.Collections.Immutable;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Presentation.Route.Move;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Move;

public sealed class RouteMovePresentationTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Move native JSON exposes the accepted ordered change data"),
     Trait("Feature", "route-move"), Trait("Evidence", "UnitContract")]
    public void NativeJsonExposesOrderedChangeData()
    {
        var output = Render(
            Result(CliSemanticStatus.Attention),
            CliFormat.Json,
            CliDetail.Standard);
        using var document = JsonDocument.Parse(output.PrimaryContent);
        var root = document.RootElement;
        var data = root.GetProperty("data");

        AssertPropertyOrder(
            root,
            "schemaVersion",
            "command",
            "status",
            "detail",
            "filter",
            "workspace",
            "summary",
            "findings",
            "effects",
            "counts",
            "limitations",
            "data",
            "recovery",
            "next");
        AssertPropertyOrder(data, "mode", "subject", "source", "destination", "moved", "rewrittenLinks");
        AssertPropertyOrder(data.GetProperty("source"), "id", "path");
        AssertPropertyOrder(data.GetProperty("destination"), "id", "path");
        AssertPropertyOrder(Assert.Single(data.GetProperty("moved").EnumerateArray()), "from", "to");
        var link = Assert.Single(data.GetProperty("rewrittenLinks").EnumerateArray());
        AssertPropertyOrder(link, "path", "location", "from", "to");
        AssertPropertyOrder(link.GetProperty("from"), "id", "path");
        AssertPropertyOrder(link.GetProperty("to"), "id", "path");
        Assert.Equal("completed-with-warnings", root.GetProperty("status").GetString());
        Assert.Equal("file", data.GetProperty("subject").GetString());
        Assert.Equal("retained", root.GetProperty("recovery").GetProperty("disposition").GetString());
        Assert.Equal("open-forge cleanup", root.GetProperty("next").GetProperty("command").GetString());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Move native text retains changes at the accepted detail levels"),
     Trait("Feature", "route-move"), Trait("Evidence", "UnitBehavior")]
    public void NativeTextRetainsChangesAtDetailLevels()
    {
        var result = Result(CliSemanticStatus.Attention);
        var minimal = Render(result, CliFormat.Text, CliDetail.Minimal).PrimaryContent;
        var standard = Render(result, CliFormat.Text, CliDetail.Standard).PrimaryContent;

        Assert.StartsWith("Moved ", minimal, StringComparison.Ordinal);
        Assert.Contains(RouteMoveTestData.DestinationPath, minimal, StringComparison.Ordinal);
        Assert.Contains("Entry updated in .agents/guidance/_guidance.md", minimal, StringComparison.Ordinal);
        Assert.Contains("Rewrote 1 link that pointed at the old path:", minimal, StringComparison.Ordinal);
        Assert.Contains("README.md:3:2", minimal, StringComparison.Ordinal);
        Assert.DoesNotContain("old%20guide.md", minimal, StringComparison.Ordinal);
        Assert.Contains("Workspace:", standard, StringComparison.Ordinal);
        Assert.Contains(
            $"{RouteMoveTestData.SourcePath} -> {RouteMoveTestData.DestinationPath}",
            standard,
            StringComparison.Ordinal);
        Assert.Contains("Recovery bundle was retained", standard, StringComparison.Ordinal);
        Assert.Contains("open-forge cleanup", standard, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route Move category JSON lists each moved member in the native data"),
     Trait("Feature", "route-move"), Trait("Evidence", "UnitBehavior")]
    public void CategoryJsonListsEachMovedMember()
    {
        var formation = RouteMoveTestData.Formation(RouteMoveMode.DryRun) with
        {
            Subject = new RouteMoveSubject
            {
                Kind = RouteMoveSubjectKind.Category,
                Layers =
                [
                    new RouteMoveSubjectLayer
                    {
                        Layer = RouteMoveLayerKind.Base,
                        SourcePath = ".agents/guidance/topics/_topics.md",
                        DestinationPath = ".agents/archive/topics/_topics.md",
                    },
                ],
                Items =
                [
                    new RouteMoveSubjectItem
                    {
                        Kind = RouteMoveItemKind.Directory,
                        SourcePath = ".agents/guidance/topics",
                        DestinationPath = ".agents/archive/topics",
                    },
                    new RouteMoveSubjectItem
                    {
                        Kind = RouteMoveItemKind.Entrypoint,
                        Layer = RouteMoveLayerKind.Base,
                        SourceId = "guidance/topics",
                        SourcePath = ".agents/guidance/topics/_topics.md",
                        DestinationPath = ".agents/archive/topics/_topics.md",
                    },
                    new RouteMoveSubjectItem
                    {
                        Kind = RouteMoveItemKind.Resource,
                        SourcePath = ".agents/guidance/topics/image.bin",
                        DestinationPath = ".agents/archive/topics/image.bin",
                    },
                ],
            },
        };
        var result = new RouteMoveResult(formation, CliSemanticStatus.Complete, next: null);

        var output = Render(result, CliFormat.Json, CliDetail.Standard);
        using var document = JsonDocument.Parse(output.PrimaryContent);
        var data = document.RootElement.GetProperty("data");
        var moved = data.GetProperty("moved").EnumerateArray().ToArray();

        Assert.Equal("route", data.GetProperty("subject").GetString());
        Assert.Equal(
            [
                ".agents/guidance/topics/_topics.md",
                ".agents/guidance/topics/image.bin",
            ],
            moved.Select(item => item.GetProperty("from").GetString()));
        Assert.Equal(
            [
                ".agents/archive/topics/_topics.md",
                ".agents/archive/topics/image.bin",
            ],
            moved.Select(item => item.GetProperty("to").GetString()));
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Route Move native output preserves failed and cancelled status actions")]
    [InlineData((int)CliSemanticStatus.Failed, "Route move stopped after 0 of 3 changes.", "open-forge route move --detail debug")]
    [InlineData((int)CliSemanticStatus.Interrupted, "Route move was cancelled. Nothing was changed.", "open-forge route move")]
    [Trait("Feature", "route-move")]
    [Trait("Evidence", "UnitBehavior")]
    public void FailedAndInterruptedResultsUseNativeStatusActions(
        int statusValue,
        string expectedHeadline,
        string expectedCommand)
    {
        var result = Result((CliSemanticStatus)statusValue);
        var text = Render(result, CliFormat.Text, CliDetail.Minimal);
        var json = Render(result, CliFormat.Json, CliDetail.Minimal);
        using var document = JsonDocument.Parse(json.PrimaryContent);

        Assert.StartsWith(expectedHeadline, text.PrimaryContent, StringComparison.Ordinal);
        Assert.Contains($"Next: {expectedCommand}", text.PrimaryContent, StringComparison.Ordinal);
        Assert.Equal(
            statusValue == (int)CliSemanticStatus.Interrupted ? "cancelled" : "failed",
            document.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            expectedCommand,
            document.RootElement.GetProperty("next").GetProperty("command").GetString());
    }

    private const string RecoveryPath = "/recovery/route-move-final.zip";

    private static RouteMoveResult Result(CliSemanticStatus status)
    {
        var effects = status == CliSemanticStatus.Interrupted
            ? Effects(RouteMoveEffectOutcome.NotStarted, RouteMoveEffectResidual.None)
            : status == CliSemanticStatus.Failed
                ? Effects(RouteMoveEffectOutcome.VerificationFailed, RouteMoveEffectResidual.Retained)
                : Effects(RouteMoveEffectOutcome.Verified, RouteMoveEffectResidual.None);
        var finding = status switch
        {
            CliSemanticStatus.Attention => RouteMoveTestData.Finding(
                RouteMoveFindingCode.RecoveryArtifactRetained,
                status),
            CliSemanticStatus.Failed => RouteMoveTestData.Finding(
                RouteMoveFindingCode.VerificationFailed,
                status),
            CliSemanticStatus.Interrupted => RouteMoveTestData.Finding(
                RouteMoveFindingCode.Interrupted,
                status),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported presentation status."),
        };
        var recovery = status == CliSemanticStatus.Interrupted
            ? new RouteMoveRecovery
            {
                State = RouteMoveRecoveryState.NotCreated,
                ProtectedPaths = ["README.md", RouteMoveTestData.SourcePath],
                ResidualPath = null,
            }
            : new RouteMoveRecovery
            {
                State = RouteMoveRecoveryState.Retained,
                ProtectedPaths = ["README.md", RouteMoveTestData.SourcePath],
                ResidualPath = RecoveryPath,
            };
        var formation = RouteMoveTestData.Formation() with
        {
            References = new RouteMoveReferences
            {
                Coverage = RouteMoveCoverage.Complete,
                ScannedSourceCount = 3,
                InspectedSourceCount = 3,
                OccurrenceCount = 1,
                Rewrites =
                [
                    new RouteMoveReferenceRewrite
                    {
                        SourcePath = "README.md",
                        DestinationSourcePath = "README.md",
                        Layer = null,
                        Location = new SourceLocation(3, 2, 24, 14),
                        Before = "old%20guide.md",
                        Expected = "new%20guide.md",
                        OldTarget = new RouteMoveReferenceTarget
                        {
                            Id = RouteMoveTestData.SourceId,
                            Path = RouteMoveTestData.SourcePath,
                        },
                        ExpectedTarget = new RouteMoveReferenceTarget
                        {
                            Id = RouteMoveTestData.DestinationId,
                            Path = RouteMoveTestData.DestinationPath,
                        },
                    },
                ],
            },
            GeneratedNavigation = new RouteMoveGeneratedNavigation
            {
                Coverage = RouteMoveCoverage.Complete,
                Regions =
                [
                    new RouteMoveGeneratedRegion
                    {
                        Path = ".agents/guidance/_guidance.md",
                        Reasons = [RouteMoveGeneratedReason.OldParent, RouteMoveGeneratedReason.NewParent],
                        State = RouteMoveGeneratedState.Changed,
                    },
                ],
            },
            Effects = effects,
            UnchangedPaths = [],
            Recovery = recovery,
            Verification = status switch
            {
                CliSemanticStatus.Attention => RouteMoveVerificationState.Verified,
                CliSemanticStatus.Failed => RouteMoveVerificationState.Failed,
                CliSemanticStatus.Interrupted => RouteMoveVerificationState.NotRequested,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported presentation status."),
            },
            Findings = [finding],
        };

        return new RouteMoveResult(formation, status, next: null);
    }

    private static ImmutableArray<RouteMoveEffect> Effects(
        RouteMoveEffectOutcome outcome,
        RouteMoveEffectResidual residual)
        =>
        [
            Effect(
                RouteMoveTestData.DestinationPath,
                RouteMoveEffectKind.MovedFile,
                RouteMoveEffectAction.Create,
                outcome,
                residual),
            Effect(
                "README.md",
                RouteMoveEffectKind.ReferenceSource,
                RouteMoveEffectAction.Replace,
                outcome,
                residual),
            Effect(
                RouteMoveTestData.SourcePath,
                RouteMoveEffectKind.MovedFile,
                RouteMoveEffectAction.Delete,
                outcome,
                residual),
        ];

    private static RouteMoveEffect Effect(
        string path,
        RouteMoveEffectKind kind,
        RouteMoveEffectAction action,
        RouteMoveEffectOutcome outcome,
        RouteMoveEffectResidual residual)
        => new()
        {
            Path = path,
            Kind = kind,
            Action = action,
            Before = action == RouteMoveEffectAction.Create
                ? new RouteMovePathState(RouteMovePathStateKind.Missing, null)
                : new RouteMovePathState(RouteMovePathStateKind.File, new string('a', 64)),
            Expected = action == RouteMoveEffectAction.Delete
                ? new RouteMovePathState(RouteMovePathStateKind.Missing, null)
                : new RouteMovePathState(RouteMovePathStateKind.File, new string('b', 64)),
            Outcome = outcome,
            Residual = residual,
        };

    private static CliRenderedOutput Render(
        RouteMoveResult result,
        CliFormat format,
        CliDetail detail)
        => CliRenderingStage.Render(
            new CliPresentationRequest<RouteMoveResult>(
                result,
                new CliPresentation(format, detail, null)),
            RouteMovePresentation.Rendering);

    private static void AssertPropertyOrder(
        JsonElement element,
        params string[] expected)
        => Assert.Equal(
            expected,
            element.EnumerateObject().Select(property => property.Name));
}
