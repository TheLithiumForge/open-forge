using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Selection;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Presentation.Extension.Update;
using OpenForge.Cli.Core.Presentation.Extension.Update.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Extension.Update;

public sealed class ExtensionUpdateReportSelectorTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension Update failed mixed effects keep not-started text and count only verified files"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void FailedMixedEffectsKeepNotStartedTextAndCountOnlyVerifiedFiles()
    {
        var result = Result(
            ExtensionUpdateMode.Apply,
            WorkspacePermissionResult.NotEvaluated,
            [ExtensionUpdateEffectOutcome.Verified, ExtensionUpdateEffectOutcome.NotStarted],
            [new ExtensionUpdateFinding(
                ExtensionUpdateFindingCode.WriteFailed,
                "The target could not be written.",
                ".agents/toolkit.md")]);

        var selected = Select(result, CliDetail.Full);
        var rows = selected.Report.Data.TextRows.ToDictionary(row => row.Path, StringComparer.Ordinal);

        Assert.Contains("replaced", rows[".agents/aaa.md"].Text, StringComparison.Ordinal);
        Assert.Equal("not started", rows[".agents/toolkit.md"].Text);
        Assert.Equal(1, selected.Report.Counts.Single(count => count.Name == "filesReplaced").Value);

        using var json = JsonDocument.Parse(Json(selected));
        var effects = json.RootElement.GetProperty("effects").EnumerateArray().ToArray();
        Assert.Equal("replaced", effects[0].GetProperty("action").GetString());
        Assert.Equal("done", effects[0].GetProperty("outcome").GetString());
        Assert.Equal("replaced", effects[1].GetProperty("action").GetString());
        Assert.Equal("not-started", effects[1].GetProperty("outcome").GetString());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension Update verified saved grants name every required path at minimal and higher detail"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void VerifiedSavedGrantsNameEveryRequiredPathAtMinimalAndHigherDetail()
    {
        var result = Result(
            ExtensionUpdateMode.Apply,
            new WorkspacePermissionResult(
                [".agents/open-forge.json", ".agents/settings.json"],
                [".agents/open-forge.json", ".agents/settings.json"],
                WorkspacePermissionDecision.Approved,
                WorkspacePermissionAction.Create,
                WorkspacePermissionOutcome.Verified),
            [ExtensionUpdateEffectOutcome.NotStarted],
            [new ExtensionUpdateFinding(
                ExtensionUpdateFindingCode.WriteFailed,
                "The target could not be written.",
                ".agents/toolkit.md")]);

        foreach (var detail in new[] { CliDetail.Minimal, CliDetail.Standard, CliDetail.Full })
        {
            var selected = Select(result, detail);
            var details = selected.Report.Data.TextDetails;

            Assert.Contains(
                "Saved a grant for .agents/open-forge.json to .agents/open-forge.json",
                details);
            Assert.Contains(
                "Saved a grant for .agents/settings.json to .agents/open-forge.json",
                details);
        }
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension Update preview keeps planned wording and does not claim a saved grant"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void PreviewKeepsPlannedWordingAndDoesNotClaimSavedGrant()
    {
        var result = Result(
            ExtensionUpdateMode.DryRun,
            new WorkspacePermissionResult(
                [".agents/open-forge.json"],
                [".agents/open-forge.json"],
                WorkspacePermissionDecision.Required,
                WorkspacePermissionAction.Create,
                WorkspacePermissionOutcome.Planned),
            [ExtensionUpdateEffectOutcome.Planned]);

        var selected = Select(result, CliDetail.Minimal);

        Assert.Contains("replace", selected.Report.Data.TextRows.Single().Text, StringComparison.Ordinal);
        Assert.Contains(
            "Would save a grant for .agents/open-forge.json to .agents/open-forge.json",
            selected.Report.Data.TextDetails);
        Assert.DoesNotContain(
            "Saved a grant for .agents/open-forge.json to .agents/open-forge.json",
            selected.Report.Data.TextDetails);
        Assert.Equal(1, selected.Report.Counts.Single(count => count.Name == "filesReplaced").Value);
        Assert.Equal(CliSemanticStatus.Complete, selected.Report.Status);
        Assert.Equal(CliHeadlineKind.Preview, selected.Report.Headline.Kind);
        Assert.Equal(
            "Would update the toolkit Extension to 2.0.0.",
            selected.Report.Headline.Sentence);
    }

    [Fact(DisplayName = "Extension Update retained-file warning preview uses future tense while apply keeps completed wording"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void RetainedFileWarningPreviewUsesFutureTenseWhileApplyKeepsCompletedWording()
    {
        var finding = new ExtensionUpdateFinding(
            ExtensionUpdateFindingCode.ManagedDivergence,
            "The retired file was kept.",
            ".agents/retired.md");

        var preview = Select(
            Result(
                ExtensionUpdateMode.DryRun,
                WorkspacePermissionResult.NotEvaluated,
                [ExtensionUpdateEffectOutcome.Planned],
                [finding],
                includeRetainedFile: true),
            CliDetail.Full);
        var applied = Select(
            Result(
                ExtensionUpdateMode.Apply,
                WorkspacePermissionResult.NotEvaluated,
                [ExtensionUpdateEffectOutcome.Verified],
                [finding],
                includeRetainedFile: true),
            CliDetail.Full);

        Assert.Equal(CliSemanticStatus.Attention, preview.Report.Status);
        Assert.Equal(CliHeadlineKind.Warnings, preview.Report.Headline.Kind);
        Assert.Equal(
            "Would update the toolkit Extension to 2.0.0; 1 file from an earlier version would be kept.",
            preview.Report.Headline.Sentence);
        Assert.Contains(
            preview.Report.Effects,
            effect => effect.Path == ".agents/retired.md");
        Assert.Contains(
            preview.Report.Findings,
            reportFinding => reportFinding.Subject.Path == ".agents/retired.md");

        Assert.Equal(CliSemanticStatus.Attention, applied.Report.Status);
        Assert.Equal(CliHeadlineKind.Warnings, applied.Report.Headline.Kind);
        Assert.Equal(
            "Updated the toolkit Extension to 2.0.0. 1 file from an earlier version was kept.",
            applied.Report.Headline.Sentence);
    }

    private static CliSelectedReport<ExtensionUpdateData> Select(
        ExtensionUpdateResult result,
        CliDetail detail)
    {
        var selection = new CliSelection(detail);
        var rendering = ExtensionUpdatePresentation.Rendering;
        return rendering.SelectText!(
            CliReportTrimmer.Trim(rendering.Selector(result, selection), selection, rendering.Shape));
    }

    private static string Json(CliSelectedReport<ExtensionUpdateData> selected)
        => CliJsonRenderer.Render(selected, ExtensionUpdatePresentation.Rendering.DataJsonTypeInfo);

    private static ExtensionUpdateResult Result(
        ExtensionUpdateMode mode,
        WorkspacePermissionResult permissions,
        IReadOnlyList<ExtensionUpdateEffectOutcome> outcomes,
        IReadOnlyList<ExtensionUpdateFinding>? findings = null,
        bool includeRetainedFile = false)
    {
        var paths = outcomes
            .Select((_, index) => index == 0 ? ".agents/aaa.md" : ".agents/toolkit.md")
            .ToArray();
        var comparisonPaths = paths
            .Concat(includeRetainedFile ? [".agents/retired.md"] : [])
            .ToArray();
        var package = new ExtensionUpdatePackage(
            "toolkit",
            selectedRoot: true,
            dependencies: [],
            fromVersion: "1.0.0",
            toVersion: "2.0.0");
        var comparisons = comparisonPaths
            .Select((path, index) => new ExtensionUpdateComparison(
                path,
                package.Id,
                ExtensionUpdateComparisonTargetKind.PackageFile,
                region: null,
                sourceAssetPath: $"content/{path}",
                ExtensionUpdateComparisonFingerprintKind.OpenForgeMarkdownV1,
                currentFingerprint: new string((char)('a' + index), 64),
                intendedFingerprint: new string((char)('b' + index), 64),
                ExtensionUpdateComparisonCurrentState.Changed,
                includeRetainedFile && path == ".agents/retired.md"
                    ? ExtensionUpdateComparisonIntendedState.Retired
                    : ExtensionUpdateComparisonIntendedState.Changed,
                includeRetainedFile && path == ".agents/retired.md"
                    ? ExtensionUpdateRetirementEligibility.Eligible
                    : ExtensionUpdateRetirementEligibility.NotApplicable))
            .ToArray();
        var effects = paths
            .Select((path, index) => new ExtensionUpdateEffect(
                path,
                package.Id,
                ExtensionUpdateEffectKind.PackageFile,
                ExtensionUpdateEffectAction.Replace,
                [new ExtensionUpdateLogicalChange(
                    ExtensionUpdateComparisonTargetKind.PackageFile,
                    ExtensionUpdateChangeAction.Replace,
                    region: null,
                    sourceAssetPath: $"content/{path}")],
                outcomes[index],
                ExtensionUpdateEffectResidual.None))
            .ToArray();
        var verification = mode == ExtensionUpdateMode.DryRun
            ? ExtensionUpdateVerificationState.Planned
            : ExtensionUpdateVerificationState.NotRequested;
        var facts = new ExtensionUpdateResultFacts
        {
            Selection = new ExtensionUpdateSelection(
                ExtensionUpdateSelectionKind.ExplicitIds,
                [package.Id]),
            Source = new ExtensionUpdateSource(
                ExtensionUpdateSourceKind.Catalogue,
                "/source",
                "catalogue-identity",
                packageCount: 1),
            Packages = [package],
            Comparisons = comparisons,
            GeneratedNavigation = new ExtensionUpdateGeneratedNavigation([]),
            Effects = effects,
            Permissions = permissions,
            Lifecycle = new ExtensionUpdateLifecycle(
                ExtensionUpdateLifecycleTrust.Trusted,
                ExtensionUpdateLifecycleCoverage.Complete,
                ExtensionUpdateLifecycleAction.Publish,
                mode == ExtensionUpdateMode.DryRun
                    ? ExtensionUpdateLifecycleOutcome.Planned
                    : ExtensionUpdateLifecycleOutcome.NotStarted),
            Recovery = new ExtensionUpdateRecovery(
                ExtensionUpdateRecoveryState.NotCreated,
                [],
                residualPath: null),
            Verification = new ExtensionUpdateVerification(verification, verification, verification),
        };

        return new ExtensionUpdateResult(new ExtensionUpdateResultFormation
        {
            Workspace = new CliWorkspace(
                "extension-update-selector-workspace",
                "extension-update-selector-workspace",
                CliWorkspaceSelectionMethod.CurrentDirectory),
            Mode = mode,
            Force = false,
            Prune = false,
            Automatic = true,
            Facts = facts,
            Findings = findings ?? [],
        });
    }
}
