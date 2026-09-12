using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;
using OpenForge.Cli.Core.Commands.Extension.Inspect;
using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Inspect;

public sealed class ExtensionInspectContractTests
{
    [Fact(DisplayName = "Extension Inspect comparison state mapping is exhaustive over modes and side availability"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Unit")]
    public void ComparisonStateMappingIsExhaustive()
    {
        var available = ExtensionInspectComparisonSideState.Available;
        var unavailable = ExtensionInspectComparisonSideState.Unavailable;

        Assert.Equal(
            ExtensionInspectComparisonState.NotStarted,
            ExtensionInspectComparisonProjector.ReadComparisonState(
                ExtensionInspectComparisonMode.None,
                available,
                available,
                available));
        Assert.Equal(
            ExtensionInspectComparisonState.Complete,
            ExtensionInspectComparisonProjector.ReadComparisonState(
                ExtensionInspectComparisonMode.ThreeWay,
                available,
                available,
                available));
        Assert.Equal(
            ExtensionInspectComparisonState.Incomplete,
            ExtensionInspectComparisonProjector.ReadComparisonState(
                ExtensionInspectComparisonMode.ThreeWay,
                unavailable,
                available,
                available));
        Assert.Equal(
            ExtensionInspectComparisonState.Incomplete,
            ExtensionInspectComparisonProjector.ReadComparisonState(
                ExtensionInspectComparisonMode.ThreeWay,
                available,
                unavailable,
                available));
        Assert.Equal(
            ExtensionInspectComparisonState.Incomplete,
            ExtensionInspectComparisonProjector.ReadComparisonState(
                ExtensionInspectComparisonMode.ThreeWay,
                available,
                available,
                unavailable));
        Assert.Equal(
            ExtensionInspectComparisonState.Complete,
            ExtensionInspectComparisonProjector.ReadComparisonState(
                ExtensionInspectComparisonMode.InstalledOnly,
                available,
                available,
                unavailable));
        Assert.Equal(
            ExtensionInspectComparisonState.Incomplete,
            ExtensionInspectComparisonProjector.ReadComparisonState(
                ExtensionInspectComparisonMode.InstalledOnly,
                unavailable,
                available,
                available));
        Assert.Equal(
            ExtensionInspectComparisonState.Incomplete,
            ExtensionInspectComparisonProjector.ReadComparisonState(
                ExtensionInspectComparisonMode.InstalledOnly,
                available,
                unavailable,
                available));
        Assert.Equal(
            ExtensionInspectComparisonState.Complete,
            ExtensionInspectComparisonProjector.ReadComparisonState(
                ExtensionInspectComparisonMode.AvailableOnly,
                unavailable,
                unavailable,
                available));
        Assert.Equal(
            ExtensionInspectComparisonState.Incomplete,
            ExtensionInspectComparisonProjector.ReadComparisonState(
                ExtensionInspectComparisonMode.AvailableOnly,
                available,
                available,
                unavailable));

        var undefined = (ExtensionInspectComparisonMode)int.MaxValue;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => ExtensionInspectComparisonProjector.ReadComparisonState(
                undefined,
                available,
                available,
                available));
        Assert.Equal("mode", exception.ParamName);
        Assert.Equal(undefined, exception.ActualValue);
    }

    [Fact(DisplayName = "Extension Inspect source failure mapping is exhaustive over failure kinds and source states"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Unit")]
    public void SourceFailureFindingMappingIsExhaustive()
    {
        var fallbackFailureKinds = new[]
        {
            ExtensionSourceFailureKind.None,
            ExtensionSourceFailureKind.Unavailable,
            ExtensionSourceFailureKind.Invalid,
            ExtensionSourceFailureKind.PackageUnavailable,
        };
        foreach (var failureKind in fallbackFailureKinds)
        {
            Assert.Equal(
                ExtensionInspectFindingCode.SourceUnavailable,
                ExtensionInspectPackageLifecycleBuilder.ReadSourceFailureFindingCode(
                    failureKind,
                    ExtensionSourceReadState.Missing));
            Assert.Equal(
                ExtensionInspectFindingCode.SourceUnavailable,
                ExtensionInspectPackageLifecycleBuilder.ReadSourceFailureFindingCode(
                    failureKind,
                    ExtensionSourceReadState.Unavailable));
            Assert.Equal(
                ExtensionInspectFindingCode.SourceInvalid,
                ExtensionInspectPackageLifecycleBuilder.ReadSourceFailureFindingCode(
                    failureKind,
                    ExtensionSourceReadState.Invalid));
        }

        var expected = new (ExtensionSourceFailureKind FailureKind, ExtensionInspectFindingCode Code)[]
        {
            (ExtensionSourceFailureKind.Overlap, ExtensionInspectFindingCode.SourceOverlap),
            (ExtensionSourceFailureKind.Ambiguous, ExtensionInspectFindingCode.SourceAmbiguous),
            (ExtensionSourceFailureKind.DependencyIncomplete, ExtensionInspectFindingCode.DependencyIncomplete),
            (ExtensionSourceFailureKind.DependencyCycle, ExtensionInspectFindingCode.DependencyCycle),
            (ExtensionSourceFailureKind.DependencyConflict, ExtensionInspectFindingCode.DependencyConflict),
            (ExtensionSourceFailureKind.PackageInvalid, ExtensionInspectFindingCode.PackageInvalid),
            (ExtensionSourceFailureKind.IdentityAmbiguous, ExtensionInspectFindingCode.IdentityAmbiguous),
        };
        foreach (var (failureKind, code) in expected)
        {
            Assert.Equal(
                code,
                ExtensionInspectPackageLifecycleBuilder.ReadSourceFailureFindingCode(
                    failureKind,
                    ExtensionSourceReadState.Invalid));
        }

        var undefined = (ExtensionSourceFailureKind)int.MaxValue;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => ExtensionInspectPackageLifecycleBuilder.ReadSourceFailureFindingCode(
                undefined,
                ExtensionSourceReadState.Invalid));
        Assert.Equal("failureKind", exception.ParamName);
        Assert.Equal(undefined, exception.ActualValue);
    }

    [Fact(DisplayName = "Extension Inspect maps all 31 finding codes to the frozen wire code and status"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Unit")]
    public void FindingVocabularyAndStatusesAreExhaustive()
    {
        var expected = new (string Code, CliSemanticStatus Status)[]
        {
            ("extension-inspect.invalid-input", CliSemanticStatus.Invalid),
            ("extension-inspect.invalid-stable-id", CliSemanticStatus.Invalid),
            ("extension-inspect.workspace-unavailable", CliSemanticStatus.Blocked),
            ("extension-inspect.workspace-unsafe", CliSemanticStatus.Blocked),
            ("extension-inspect.source-unavailable", CliSemanticStatus.Attention),
            ("extension-inspect.source-invalid", CliSemanticStatus.Incomplete),
            ("extension-inspect.source-overlap", CliSemanticStatus.Blocked),
            ("extension-inspect.source-ambiguous", CliSemanticStatus.Blocked),
            ("extension-inspect.identity-ambiguous", CliSemanticStatus.Blocked),
            ("extension-inspect.lifecycle-unavailable", CliSemanticStatus.Incomplete),
            ("extension-inspect.lifecycle-invalid", CliSemanticStatus.Incomplete),
            ("extension-inspect.lifecycle-blocked", CliSemanticStatus.Blocked),
            ("extension-inspect.package-unavailable", CliSemanticStatus.Incomplete),
            ("extension-inspect.package-invalid", CliSemanticStatus.Incomplete),
            ("extension-inspect.dependency-incomplete", CliSemanticStatus.Incomplete),
            ("extension-inspect.dependency-cycle", CliSemanticStatus.Blocked),
            ("extension-inspect.dependency-conflict", CliSemanticStatus.Blocked),
            ("extension-inspect.path-unavailable", CliSemanticStatus.Incomplete),
            ("extension-inspect.path-invalid", CliSemanticStatus.Blocked),
            ("extension-inspect.ownership-conflict", CliSemanticStatus.Blocked),
            ("extension-inspect.fingerprint-unavailable", CliSemanticStatus.Incomplete),
            ("extension-inspect.fingerprint-fallback", CliSemanticStatus.Incomplete),
            ("extension-inspect.generated-boundary-invalid", CliSemanticStatus.Incomplete),
            ("extension-inspect.dependency-changed", CliSemanticStatus.Attention),
            ("extension-inspect.path-changed", CliSemanticStatus.Attention),
            ("extension-inspect.path-current-diverged", CliSemanticStatus.Attention),
            ("extension-inspect.path-missing", CliSemanticStatus.Attention),
            ("extension-inspect.path-new", CliSemanticStatus.Attention),
            ("extension-inspect.path-retired", CliSemanticStatus.Attention),
            ("extension-inspect.operation-failed", CliSemanticStatus.Failed),
            ("extension-inspect.interrupted", CliSemanticStatus.Interrupted),
        };

        var codes = Enum.GetValues<ExtensionInspectFindingCode>();
        Assert.Equal(expected.Length, codes.Length);
        for (var index = 0; index < expected.Length; index++)
        {
            Assert.Equal(index, (int)codes[index]);
            Assert.Equal(expected[index].Code, ExtensionInspectDefinitions.ReadFindingCode(codes[index]));
            Assert.Equal(expected[index].Status, ExtensionInspectDefinitions.ReadFindingStatus(codes[index]));
        }
    }

    [Fact(DisplayName = "Extension Inspect next actions follow the exhaustive status and recommendation table"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Unit")]
    public void NextActionsAreExactAndBounded()
    {
        Assert.Null(ExtensionInspectDefinitions.ReadNext(CliSemanticStatus.Complete, actionable: false, subjectId: "toolkit"));
        Assert.Null(ExtensionInspectDefinitions.ReadNext(CliSemanticStatus.Attention, actionable: false, subjectId: "toolkit"));

        var update = ExtensionInspectDefinitions.ReadNext(
            CliSemanticStatus.Attention,
            actionable: true,
            subjectId: "toolkit");
        Assert.NotNull(update);
        Assert.Equal("open-forge extension update toolkit", update!.Command);
        Assert.Equal(
            "Apply the trusted current-source change for this stable ID with the explicit update command.",
            update.Reason);

        var expected = new (CliSemanticStatus Status, string Command, string Reason)[]
        {
            (CliSemanticStatus.Incomplete, "open-forge doctor", "Inspect unavailable lifecycle, source, dependency, path, or fingerprint facts before relying on this result."),
            (CliSemanticStatus.Invalid, "open-forge extension inspect --help", "Correct the named Extension Inspect input, then rerun the request."),
            (CliSemanticStatus.Blocked, "open-forge doctor", "Inspect the blocked workspace, source, identity, or ownership boundary before rerunning Extension Inspect."),
            (CliSemanticStatus.Failed, "open-forge extension inspect --verbose", "Report the failure and retry Extension Inspect with bounded diagnostics."),
            (CliSemanticStatus.Interrupted, "open-forge extension inspect", "Rerun the same Extension Inspect request."),
        };

        foreach (var value in expected)
        {
            var action = ExtensionInspectDefinitions.ReadNext(value.Status, actionable: false, subjectId: "toolkit");
            Assert.NotNull(action);
            Assert.Equal(value.Command, action!.Command);
            Assert.Equal(value.Reason, action.Reason);
        }
    }

    [Fact(DisplayName = "Extension Inspect JSON emits the complete ordered graph with null and empty values"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Unit")]
    public void JsonGraphHasFrozenOrderAndPresence()
    {
        var result = ExtensionInspectResultBuilder.Invalid(
            workspace: null,
            supplied: "bad ID",
            cause: "The request is invalid.");
        var json = ExtensionInspectJsonRenderer.Render(
            new CliPresentationRequest<ExtensionInspectResult>(
                result,
                new CliPresentation(CliOutputFormat.Json, CliView.Expanded, CliVerbosity.Normal)));

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.Equal(
            ["schemaVersion", "command", "status", "workspace", "result", "next"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal(JsonValueKind.Null, root.GetProperty("workspace").ValueKind);

        var commandResult = root.GetProperty("result");
        Assert.Equal(
            ["subject", "source", "lifecycle", "installed", "available", "dependencies", "pathFacts", "comparison", "generated", "findings", "counts"],
            commandResult.EnumerateObject().Select(property => property.Name));
        Assert.Equal(JsonValueKind.Array, commandResult.GetProperty("findings").ValueKind);
        Assert.Equal(JsonValueKind.Array, commandResult.GetProperty("subject").GetProperty("candidates").ValueKind);
        Assert.Equal(JsonValueKind.Array, commandResult.GetProperty("dependencies").GetProperty("declared").ValueKind);
        Assert.Equal(JsonValueKind.Array, commandResult.GetProperty("dependencies").GetProperty("resolved").ValueKind);
        Assert.Equal(JsonValueKind.Array, commandResult.GetProperty("dependencies").GetProperty("order").ValueKind);
        Assert.Equal(JsonValueKind.Array, commandResult.GetProperty("pathFacts").GetProperty("declared").ValueKind);
        Assert.Equal(JsonValueKind.Array, commandResult.GetProperty("pathFacts").GetProperty("current").ValueKind);
        Assert.Equal(JsonValueKind.Array, commandResult.GetProperty("comparison").GetProperty("paths").ValueKind);
        Assert.Equal(JsonValueKind.Array, commandResult.GetProperty("generated").GetProperty("regions").ValueKind);
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("extension inspect", root.GetProperty("command").GetString());
        Assert.Equal("invalid", root.GetProperty("status").GetString());
        Assert.Equal("extension-inspect.invalid-input", commandResult.GetProperty("findings")[0].GetProperty("code").GetString());
    }

    [Fact(DisplayName = "Extension Inspect human views are projections of the same typed result"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Unit")]
    public void HumanViewsRetainStatusAndFinding()
    {
        var result = ExtensionInspectResultBuilder.InvalidStableId(
            workspace: null,
            supplied: "Toolkit",
            cause: "The supplied ID is not lowercase.");
        var compact = ExtensionInspectHumanRenderer.Render(
            new CliPresentationRequest<ExtensionInspectResult>(
                result,
                new CliPresentation(CliOutputFormat.Human, CliView.Compact, CliVerbosity.Normal)));
        var expanded = ExtensionInspectHumanRenderer.Render(
            new CliPresentationRequest<ExtensionInspectResult>(
                result,
                new CliPresentation(CliOutputFormat.Human, CliView.Expanded, CliVerbosity.Normal)));

        Assert.Contains("extension-inspect.invalid-stable-id", compact, StringComparison.Ordinal);
        Assert.Contains("Status: invalid", compact, StringComparison.Ordinal);
        Assert.Contains("extension-inspect.invalid-stable-id", expanded, StringComparison.Ordinal);
        Assert.Contains("Status: invalid", expanded, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Extension Inspect validates findings and applies category-specific tie-breaks"), Trait("Feature", "extension-inspect"), Trait("Evidence", "Unit")]
    public void FindingValidationFailsClosedAndOrdersPathEvidenceByPath()
    {
        var ordered = ExtensionInspectFindingPolicy.Normalize(
        [
            Finding(
                ExtensionInspectFindingCode.PathUnavailable,
                CliSemanticStatus.Incomplete,
                subject: "a-package",
                packageId: "a-package",
                path: ".agents/z.md",
                cause: "The later path is unavailable."),
            Finding(
                ExtensionInspectFindingCode.PathUnavailable,
                CliSemanticStatus.Incomplete,
                subject: "z-package",
                packageId: "z-package",
                path: ".agents/a.md",
                cause: "The earlier path is unavailable."),
        ]);

        Assert.Equal([".agents/a.md", ".agents/z.md"], ordered.Select(finding => finding.Path));

        var duplicate = ExtensionInspectFindingPolicy.Normalize(
        [
            Finding(
                ExtensionInspectFindingCode.FingerprintFallback,
                CliSemanticStatus.Incomplete,
                path: ".agents/toolkit.md",
                cause: "Current bytes use fallback."),
            Finding(
                ExtensionInspectFindingCode.FingerprintFallback,
                CliSemanticStatus.Incomplete,
                path: ".agents/toolkit.md",
                cause: "Intended bytes use fallback."),
        ]);
        var mismatched = ExtensionInspectFindingPolicy.Normalize(
        [
            Finding(
                ExtensionInspectFindingCode.PathUnavailable,
                CliSemanticStatus.Attention,
                path: ".agents/toolkit.md",
                cause: "The status does not match the code."),
        ]);
        var unknown = ExtensionInspectFindingPolicy.Normalize(
        [
            Finding(
                (ExtensionInspectFindingCode)99,
                CliSemanticStatus.Failed,
                cause: "The code is outside the frozen vocabulary."),
        ]);

        Assert.All(
            new[] { duplicate, mismatched, unknown },
            findings => Assert.Equal(
                ExtensionInspectFindingCode.OperationFailed,
                Assert.Single(findings).Code));
    }

    private static ExtensionInspectFinding Finding(
        ExtensionInspectFindingCode code,
        CliSemanticStatus status,
        string? subject = null,
        string? packageId = null,
        string? path = null,
        string cause = "A finding was formed.")
        => new()
        {
            Code = code,
            Status = status,
            Subject = subject,
            PackageId = packageId,
            Dependency = null,
            Path = path,
            Cause = cause,
            Location = null,
            Candidates = [],
        };
}
