using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Move;

public sealed class RouteMovePresentationTests
{
    [Fact(DisplayName = "Route Move JSON preserves every nested object order"), Trait("Feature", "route-move"), Trait("Evidence", "Unit")]
    public void JsonPreservesEveryNestedObjectOrder()
    {
        using var document = JsonDocument.Parse(RouteMoveJsonRenderer.Render(
            Presentation(Result(CliSemanticStatus.Attention), CliOutputFormat.Json, CliView.Compact, CliVerbosity.Normal)));
        var root = document.RootElement;
        var result = root.GetProperty("result");
        AssertPropertyOrder(root.GetProperty("workspace"), "path", "selectedBy");
        AssertPropertyOrder(result.GetProperty("source"), "requested", "selectedBy", "id", "path", "form");
        AssertPropertyOrder(result.GetProperty("destination"), "requested", "id", "path", "parentId", "parentPath");
        var subject = result.GetProperty("subject");
        AssertPropertyOrder(subject, "kind", "layers", "items");
        Assert.All(subject.GetProperty("layers").EnumerateArray(), layer => AssertPropertyOrder(layer, "layer", "sourcePath", "destinationPath"));
        AssertPropertyOrder(result.GetProperty("ownership"), "state", "framework", "extensions", "claims");
        var references = result.GetProperty("references");
        AssertPropertyOrder(references, "coverage", "scannedSourceCount", "inspectedSourceCount", "occurrenceCount", "rewrites");
        var rewrite = Assert.Single(references.GetProperty("rewrites").EnumerateArray());
        AssertPropertyOrder(rewrite, "sourcePath", "destinationSourcePath", "layer", "location", "before", "expected", "oldTarget", "expectedTarget");
        AssertPropertyOrder(rewrite.GetProperty("location"), "line", "column", "byteOffset", "byteLength");
        AssertPropertyOrder(rewrite.GetProperty("oldTarget"), "id", "path");
        AssertPropertyOrder(rewrite.GetProperty("expectedTarget"), "id", "path");
        var generated = result.GetProperty("generatedNavigation");
        AssertPropertyOrder(generated, "coverage", "regions");
        AssertPropertyOrder(Assert.Single(generated.GetProperty("regions").EnumerateArray()), "path", "reasons", "state");
        AssertPropertyOrder(result.GetProperty("plan"), "completeness", "safety");
        Assert.All(result.GetProperty("effects").EnumerateArray(), effect =>
        {
            AssertPropertyOrder(effect, "path", "kind", "action", "before", "expected", "outcome", "residual");
            AssertPropertyOrder(effect.GetProperty("before"), "kind", "contentSha256");
            AssertPropertyOrder(effect.GetProperty("expected"), "kind", "contentSha256");
        });
        AssertPropertyOrder(result.GetProperty("recovery"), "state", "protectedPaths", "residualPath");
        AssertPropertyOrder(Assert.Single(result.GetProperty("findings").EnumerateArray()), "code", "status", "target", "cause");
        AssertPropertyOrder(root.GetProperty("next"), "command", "reason");
    }

    private const string RecoveryPath = "/recovery/route-move-final.zip";

    [Fact(DisplayName = "Route Move attention human views expose every compact and expanded change fact"),
     Trait("Feature", "route-move"), Trait("Evidence", "UnitBehavior")]
    public void AttentionHumanViewsExposeEveryChangeFact()
    {
        var result = Result(CliSemanticStatus.Attention);

        var expanded = RouteMoveHumanRenderer.Render(
            Presentation(result, CliOutputFormat.Human, CliView.Expanded, CliVerbosity.Normal));
        var compact = RouteMoveHumanRenderer.Render(
            Presentation(result, CliOutputFormat.Human, CliView.Compact, CliVerbosity.Normal));

        foreach (var text in new[] { expanded, compact })
        {
            Assert.Contains("requires attention", text, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(RouteMoveTestData.DestinationPath, text, StringComparison.Ordinal);
            Assert.Contains("README.md", text, StringComparison.Ordinal);
            Assert.Contains(RouteMoveTestData.SourcePath, text, StringComparison.Ordinal);
            Assert.Contains("old%20guide.md", text, StringComparison.Ordinal);
            Assert.Contains("new%20guide.md", text, StringComparison.Ordinal);
            Assert.Contains(".agents/guidance/_guidance.md", text, StringComparison.Ordinal);
            Assert.Contains(RecoveryPath, text, StringComparison.Ordinal);
            Assert.Contains("open-forge cleanup", text, StringComparison.Ordinal);
        }

        var verbose = RouteMoveHumanRenderer.Render(
            Presentation(result, CliOutputFormat.Human, CliView.Expanded, CliVerbosity.Verbose));
        Assert.Equal(expanded, verbose);
    }

    [Fact(DisplayName = "Route Move attention JSON exposes the exact ordered schema-v1 graph"),
     Trait("Feature", "route-move"), Trait("Evidence", "UnitBehavior")]
    public void AttentionJsonExposesExactOrderedSchemaV1Graph()
    {
        var json = RouteMoveJsonRenderer.Render(
            Presentation(
                Result(CliSemanticStatus.Attention),
                CliOutputFormat.Json,
                CliView.Compact,
                CliVerbosity.Normal));
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var result = root.GetProperty("result");

        AssertPropertyOrder(root, "schemaVersion", "command", "status", "workspace", "result", "next");
        AssertPropertyOrder(
            result,
            "mode",
            "source",
            "destination",
            "subject",
            "ownership",
            "references",
            "generatedNavigation",
            "plan",
            "effects",
            "unchangedPaths",
            "recovery",
            "verification",
            "findings");
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("route move", root.GetProperty("command").GetString());
        Assert.Equal("attention", root.GetProperty("status").GetString());
        Assert.Equal("leaf", result.GetProperty("subject").GetProperty("kind").GetString());
        Assert.Empty(result.GetProperty("subject").GetProperty("items").EnumerateArray());
        Assert.Equal(
            ["moved-file", "reference-source", "moved-file"],
            result.GetProperty("effects").EnumerateArray()
                .Select(effect => effect.GetProperty("kind").GetString()));
        Assert.Equal(
            [RouteMoveTestData.DestinationPath, "README.md", RouteMoveTestData.SourcePath],
            result.GetProperty("effects").EnumerateArray()
                .Select(effect => effect.GetProperty("path").GetString()));
        Assert.Equal("retained", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal(RecoveryPath, result.GetProperty("recovery").GetProperty("residualPath").GetString());
        Assert.Equal("verified", result.GetProperty("verification").GetString());
        Assert.Equal(
            "route-move.recovery-artifact-retained",
            Assert.Single(result.GetProperty("findings").EnumerateArray()).GetProperty("code").GetString());
        Assert.Equal("open-forge cleanup", root.GetProperty("next").GetProperty("command").GetString());
    }

    [Fact(DisplayName = "Route Move category JSON retains every item field in exact order"),
     Trait("Feature", "route-move"), Trait("Evidence", "UnitBehavior")]
    public void CategoryJsonRetainsEveryItemFieldInExactOrder()
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
                        Layer = null,
                        SourceId = null,
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
                ],
            },
        };
        var result = new RouteMoveResult(formation, CliSemanticStatus.Complete, next: null);

        var json = RouteMoveJsonRenderer.Render(
            Presentation(
                result,
                CliOutputFormat.Json,
                CliView.Expanded,
                CliVerbosity.Normal));
        using var document = JsonDocument.Parse(json);
        var items = document.RootElement.GetProperty("result")
            .GetProperty("subject")
            .GetProperty("items")
            .EnumerateArray()
            .ToArray();

        Assert.Equal(2, items.Length);
        Assert.All(
            items,
            item => AssertPropertyOrder(
                item,
                "kind",
                "layer",
                "sourceId",
                "sourcePath",
                "destinationPath"));
        Assert.Equal("directory", items[0].GetProperty("kind").GetString());
        Assert.Equal(JsonValueKind.Null, items[0].GetProperty("layer").ValueKind);
        Assert.Equal("entrypoint", items[1].GetProperty("kind").GetString());
        Assert.Equal("base", items[1].GetProperty("layer").GetString());
    }

    [Theory(DisplayName = "Route Move failed and interrupted results render their exact status and next action"),
     InlineData((int)CliSemanticStatus.Failed, "failed", "open-forge route move --verbose"),
     InlineData((int)CliSemanticStatus.Interrupted, "interrupted", "open-forge route move"),
     Trait("Feature", "route-move"), Trait("Evidence", "UnitBehavior")]
    public void FailedAndInterruptedResultsRenderExactStatusAndNextAction(
        int statusValue,
        string expectedStatus,
        string expectedCommand)
    {
        var result = Result((CliSemanticStatus)statusValue);
        var humanPresentation = Presentation(
            result,
            CliOutputFormat.Human,
            CliView.Compact,
            CliVerbosity.Normal);
        var jsonPresentation = Presentation(
            result,
            CliOutputFormat.Json,
            CliView.Compact,
            CliVerbosity.Normal);

        var human = RouteMoveHumanRenderer.Render(humanPresentation);
        var json = RouteMoveJsonRenderer.Render(jsonPresentation);
        using var document = JsonDocument.Parse(json);

        Assert.Contains(expectedStatus, human, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(expectedCommand, human, StringComparison.Ordinal);
        Assert.Equal(expectedStatus, document.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            expectedCommand,
            document.RootElement.GetProperty("next").GetProperty("command").GetString());
    }

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
        var next = status switch
        {
            CliSemanticStatus.Attention => new CliNextAction(
                "open-forge cleanup",
                "Review and remove the reported recovery artifact after confirming the verified Route Move result."),
            CliSemanticStatus.Failed => new CliNextAction(
                "open-forge route move --verbose",
                "Report the failure and retry the same Route Move request with bounded diagnostics."),
            CliSemanticStatus.Interrupted => new CliNextAction(
                "open-forge route move",
                "Rerun the same Route Move request."),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported presentation status."),
        };

        return new RouteMoveResult(formation, status, next);
    }

    private static System.Collections.Immutable.ImmutableArray<RouteMoveEffect> Effects(
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

    private static CliPresentationRequest<RouteMoveResult> Presentation(
        RouteMoveResult result,
        CliOutputFormat format,
        CliView view,
        CliVerbosity verbosity)
        => new(
            result,
            new CliPresentation(format, view, verbosity));

    private static void AssertPropertyOrder(
        JsonElement element,
        params string[] expected)
        => Assert.Equal(
            expected,
            element.EnumerateObject().Select(property => property.Name));
}
