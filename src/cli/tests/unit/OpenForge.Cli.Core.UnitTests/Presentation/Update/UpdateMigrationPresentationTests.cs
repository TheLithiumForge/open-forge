using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.Result;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Update;
using OpenForge.Cli.Core.Presentation.Update.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Update;

public sealed class UpdateMigrationPresentationTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Update migration JSON keeps the schema 3 row shape at minimal and full detail"), Trait("Feature", "update-migration-presentation"), Trait("Evidence", "Unit")]
    public void MigrationJsonKeepsExactShapeAtMinimalAndFullDetail()
    {
        var result = Result(
            UpdateMode.DryRun,
            [
                Migration(
                    ".agents/skills/local/SKILL.md",
                    UpdateMigrationOutcome.Planned,
                    [WorkspaceAdoptionAction.MetadataCompleted, WorkspaceAdoptionAction.EntriesSectionAdded],
                    ["open-forge.description"],
                    [new WorkspaceAdoptionDerivation(
                        "open-forge.description",
                        WorkspaceAdoptionDerivationSource.ExistingTitle)],
                    isUserOwnedSource: true),
            ],
            [Effect(".agents/skills/local/SKILL.md", UpdatePhysicalEffectOutcome.Planned)]);

        foreach (var detail in new[] { CliDetail.Minimal, CliDetail.Standard, CliDetail.Full })
        {
            using var json = JsonDocument.Parse(Json(result, detail));
            var root = json.RootElement;
            Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
            var data = root.GetProperty("data");
            Assert.Equal("migrations", data.EnumerateObject().Last().Name);
            var row = Assert.Single(data.GetProperty("migrations").EnumerateArray());
            Assert.Equal(
                new[] { "path", "actions", "fields", "derivation", "outcome" },
                row.EnumerateObject().Select(property => property.Name));
            Assert.Equal(".agents/skills/local/SKILL.md", row.GetProperty("path").GetString());
            Assert.Equal(
                new[] { "metadata-completed", "entries-section-added" },
                row.GetProperty("actions").EnumerateArray().Select(value => value.GetString()));
            Assert.Equal(
                new[] { "open-forge.description" },
                row.GetProperty("fields").EnumerateArray().Select(value => value.GetString()));
            Assert.Equal("planned", row.GetProperty("outcome").GetString());
            var derivation = Assert.Single(row.GetProperty("derivation").EnumerateArray());
            Assert.Equal(new[] { "field", "source" }, derivation.EnumerateObject().Select(property => property.Name));
            Assert.Equal("open-forge.description", derivation.GetProperty("field").GetString());
            Assert.Equal("existing-title", derivation.GetProperty("source").GetString());
        }
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Update omits migration JSON and keeps no-op migration facts empty"), Trait("Feature", "update-migration-presentation"), Trait("Evidence", "Unit")]
    public void NoOpOmitsMigrationsFromJsonAndFacts()
    {
        var result = Result(UpdateMode.Apply);

        Assert.Empty(result.Migrations);
        foreach (var detail in new[] { CliDetail.Minimal, CliDetail.Standard, CliDetail.Full })
        {
            using var json = JsonDocument.Parse(Json(result, detail));
            Assert.False(json.RootElement.GetProperty("data").TryGetProperty("migrations", out _));
        }
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Update migration text names paths and uses verified outcomes in minimal standard and full views"), Trait("Feature", "update-migration-presentation"), Trait("Evidence", "Unit")]
    public void MigrationTextKeepsPathSpecificVerifiedAndPendingWording()
    {
        const string appliedPath = ".agents/skills/ready/SKILL.md";
        const string pendingPath = ".agents/skills/pending/SKILL.md";
        var result = Result(
            UpdateMode.Apply,
            [
                Migration(
                    appliedPath,
                    UpdateMigrationOutcome.Applied,
                    [WorkspaceAdoptionAction.MetadataCompleted],
                    ["open-forge.description"],
                    [new WorkspaceAdoptionDerivation(
                        "open-forge.description",
                        WorkspaceAdoptionDerivationSource.ExistingTitle)],
                    isUserOwnedSource: true),
                Migration(
                    pendingPath,
                    UpdateMigrationOutcome.Planned,
                    [WorkspaceAdoptionAction.EntriesSectionAdded],
                    isUserOwnedSource: true),
            ],
            [
                Effect(appliedPath, UpdatePhysicalEffectOutcome.Verified),
                Effect(pendingPath, UpdatePhysicalEffectOutcome.CompletionUnknown),
            ],
            [new UpdateFinding(UpdateFindingCode.WriteFailed, pendingPath, "The target write failed.")],
            UpdateVerificationState.Unknown);

        foreach (var detail in new[] { CliDetail.Minimal, CliDetail.Standard, CliDetail.Full })
        {
            var selected = Select(result, detail);
            var text = CliTextRenderer.Render(
                selected,
                CliTextStyle.Plain,
                UpdatePresentation.Rendering.DataTextRenderer).Content;

            Assert.Contains("Migrated: metadata updated", text, StringComparison.Ordinal);
            Assert.Contains("Planned migration: Entries section added", text, StringComparison.Ordinal);
            Assert.Contains(appliedPath, text, StringComparison.Ordinal);
            Assert.Contains(pendingPath, text, StringComparison.Ordinal);
            Assert.Contains("Migrated 1 source.", text, StringComparison.Ordinal);
            Assert.Contains("Planned migration for 1 source.", text, StringComparison.Ordinal);
        }

        using var fullJson = JsonDocument.Parse(Json(result, CliDetail.Full));
        foreach (var effect in fullJson.RootElement.GetProperty("data").GetProperty("effects").EnumerateArray())
        {
            Assert.False(effect.TryGetProperty("sourceAssetPath", out _));
            Assert.False(effect.TryGetProperty("source", out _));
            Assert.Equal("unknown", effect.GetProperty("relation").GetProperty("current").GetString());
            Assert.Equal("unknown", effect.GetProperty("relation").GetProperty("shipped").GetString());
        }
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Update keeps pending migration paths visible when an incomplete result did not verify them"), Trait("Feature", "update-migration-presentation"), Trait("Evidence", "Unit")]
    public void IncompleteMigrationUsesPlannedWording()
    {
        const string path = ".agents/skills/pending/SKILL.md";
        var result = Result(
            UpdateMode.Apply,
            [Migration(
                path,
                UpdateMigrationOutcome.Planned,
                [WorkspaceAdoptionAction.MetadataCompleted],
                isUserOwnedSource: true)],
            [Effect(path, UpdatePhysicalEffectOutcome.NotStarted)],
            [new UpdateFinding(UpdateFindingCode.ProjectionUnavailable, path, "The intended projection was unavailable.")],
            UpdateVerificationState.Unknown);
        var selected = Select(result, CliDetail.Minimal);
        var text = CliTextRenderer.Render(
            selected,
            CliTextStyle.Plain,
            UpdatePresentation.Rendering.DataTextRenderer).Content;

        Assert.Equal(CliSemanticStatus.Incomplete, selected.Report.Status);
        Assert.Single(selected.Report.Findings);
        Assert.Contains("Planned migration: metadata updated", text, StringComparison.Ordinal);
        Assert.Contains("Planned migration for 1 source.", text, StringComparison.Ordinal);
        Assert.Contains(path, text, StringComparison.Ordinal);
        Assert.DoesNotContain("Migrated:", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "A verified informational Update migration remains complete without findings"), Trait("Feature", "update-migration-presentation"), Trait("Evidence", "Unit")]
    public void VerifiedInformationalMigrationHasNoWarningStatus()
    {
        const string path = ".agents/skills/local/SKILL.md";
        var result = Result(
            UpdateMode.Apply,
            [Migration(
                path,
                UpdateMigrationOutcome.Applied,
                [WorkspaceAdoptionAction.MetadataCompleted],
                isUserOwnedSource: true)],
            [Effect(path, UpdatePhysicalEffectOutcome.Verified)]);
        var selected = Select(result, CliDetail.Full);
        var text = CliTextRenderer.Render(
            selected,
            CliTextStyle.Plain,
            UpdatePresentation.Rendering.DataTextRenderer).Content;

        Assert.Equal(CliSemanticStatus.Complete, selected.Report.Status);
        Assert.Empty(selected.Report.Findings);
        Assert.Single(selected.Report.Effects);
        Assert.Contains("Migrated: metadata updated", text, StringComparison.Ordinal);
        Assert.Equal(1, text.Split("Migrated 1 source.", StringSplitOptions.None).Length - 1);
        Assert.DoesNotContain("warning", text, StringComparison.OrdinalIgnoreCase);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "A navigation migration keeps its existing Framework section effect"), Trait("Feature", "update-migration-presentation"), Trait("Evidence", "Unit")]
    public void NavigationMigrationDoesNotReclassifyItsFrameworkHostAsUserOwned()
    {
        const string path = ".agents/loader.md";
        var result = Result(
            UpdateMode.DryRun,
            [Migration(
                path,
                UpdateMigrationOutcome.Planned,
                [WorkspaceAdoptionAction.NavigationUpdated],
                isUserOwnedSource: false)],
            [new UpdatePhysicalEffect(
                path,
                UpdatePhysicalEffectAction.Replace,
                [new UpdateLogicalChange(
                    UpdateComparisonTargetKind.GeneratedRegion,
                    UpdateLogicalChangeAction.Replace,
                    "entries",
                    sourceAssetPath: null)],
                UpdatePhysicalEffectOutcome.Planned,
                UpdatePhysicalEffectResidual.None)],
            source: new UpdateSource
            {
                Id = "framework",
                Version = "1.0.0",
                InventoryFingerprint = new string('a', 64),
                AssetCount = 1,
            },
            generatedNavigation: new UpdateGeneratedNavigation
            {
                Coverage = UpdateGeneratedNavigationCoverage.Complete,
                Regions = [new UpdateGeneratedNavigationRegion
                {
                    Path = path,
                    State = UpdateGeneratedNavigationRegionState.Changed,
                }],
            });
        var selected = Select(result, CliDetail.Full);
        var dataEffect = Assert.Single(selected.Report.Data.Effects!);

        Assert.Equal(CliEffectKind.Section, Assert.Single(selected.Report.Effects).Kind);
        Assert.Equal("framework", dataEffect.Source?.Id);
        Assert.Null(dataEffect.SourceAssetPath);
        Assert.Equal(1, selected.Report.Counts.Single(count => count.Name == "sectionsUpdated").Value);
        using var json = JsonDocument.Parse(Json(result, CliDetail.Full));
        Assert.Equal(
            "navigation-updated",
            Assert.Single(json.RootElement.GetProperty("data").GetProperty("migrations").EnumerateArray())
                .GetProperty("actions")[0]
                .GetString());
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "A navigation-only user catalogue migration has no Framework file provenance or payload count"), Trait("Feature", "update-migration-presentation"), Trait("Evidence", "Unit")]
    public void NavigationOnlyUserCatalogueMigrationDoesNotClaimFrameworkPayload()
    {
        const string path = ".agents/skills/local/references/_references.md";
        var result = Result(
            UpdateMode.DryRun,
            [Migration(
                path,
                UpdateMigrationOutcome.Planned,
                [WorkspaceAdoptionAction.NavigationUpdated],
                isUserOwnedSource: true)],
            [new UpdatePhysicalEffect(
                path,
                UpdatePhysicalEffectAction.Replace,
                [new UpdateLogicalChange(
                    UpdateComparisonTargetKind.GeneratedRegion,
                    UpdateLogicalChangeAction.Replace,
                    "entries",
                    sourceAssetPath: null)],
                UpdatePhysicalEffectOutcome.Planned,
                UpdatePhysicalEffectResidual.None)],
            source: new UpdateSource
            {
                Id = "framework",
                Version = "1.0.0",
                InventoryFingerprint = new string('b', 64),
                AssetCount = 1,
            },
            generatedNavigation: new UpdateGeneratedNavigation
            {
                Coverage = UpdateGeneratedNavigationCoverage.Complete,
                Regions = [new UpdateGeneratedNavigationRegion
                {
                    Path = path,
                    State = UpdateGeneratedNavigationRegionState.Changed,
                }],
            });
        var selected = Select(result, CliDetail.Full);
        var reportEffect = Assert.Single(selected.Report.Effects);
        var dataEffect = Assert.Single(selected.Report.Data.Effects!);

        Assert.Equal(CliEffectKind.File, reportEffect.Kind);
        Assert.Equal(0, selected.Report.Data.ChangedFiles);
        Assert.Equal(0, selected.Report.Counts.Single(count => count.Name == "filesReplaced").Value);
        Assert.Equal(0, selected.Report.Counts.Single(count => count.Name == "filesCreated").Value);
        Assert.Equal(1, selected.Report.Counts.Single(count => count.Name == "sectionsUpdated").Value);
        Assert.Null(dataEffect.Source);
        Assert.Null(dataEffect.SourceAssetPath);
        Assert.Null(dataEffect.Before);
        Assert.Null(dataEffect.After);
        Assert.Equal("unknown", dataEffect.Relation.Current);
        Assert.Equal("unknown", dataEffect.Relation.Shipped);

        using var json = JsonDocument.Parse(Json(result, CliDetail.Full));
        var data = json.RootElement.GetProperty("data");
        var migration = Assert.Single(data.GetProperty("migrations").EnumerateArray());
        Assert.Equal(
            new[] { "path", "actions", "fields", "derivation", "outcome" },
            migration.EnumerateObject().Select(property => property.Name));
        Assert.Equal("navigation-updated", migration.GetProperty("actions")[0].GetString());
        var effect = Assert.Single(data.GetProperty("effects").EnumerateArray());
        Assert.False(effect.TryGetProperty("source", out _));
        Assert.False(effect.TryGetProperty("sourceAssetPath", out _));

        var text = CliTextRenderer.Render(
            selected,
            CliTextStyle.Plain,
            UpdatePresentation.Rendering.DataTextRenderer).Content;
        Assert.Contains(path, text, StringComparison.Ordinal);
        Assert.Contains("Planned migration: navigation updated", text, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "A navigation-only Framework Skills catalogue keeps its payload attribution"), Trait("Feature", "update-migration-presentation"), Trait("Evidence", "Unit")]
    public void NavigationOnlyFrameworkSkillsCatalogueKeepsPayloadAttribution()
    {
        const string path = ".agents/skills/_skills.md";

        var result = Result(
            UpdateMode.DryRun,
            [Migration(
                path,
                UpdateMigrationOutcome.Planned,
                [WorkspaceAdoptionAction.NavigationUpdated],
                isUserOwnedSource: false)],
            [new UpdatePhysicalEffect(
                path,
                UpdatePhysicalEffectAction.Replace,
                [new UpdateLogicalChange(
                    UpdateComparisonTargetKind.GeneratedRegion,
                    UpdateLogicalChangeAction.Replace,
                    "entries",
                    sourceAssetPath: null)],
                UpdatePhysicalEffectOutcome.Planned,
                UpdatePhysicalEffectResidual.None)],
            source: new UpdateSource
            {
                Id = "framework",
                Version = "1.0.0",
                InventoryFingerprint = new string('c', 64),
                AssetCount = 1,
            },
            generatedNavigation: new UpdateGeneratedNavigation
            {
                Coverage = UpdateGeneratedNavigationCoverage.Complete,
                Regions = [new UpdateGeneratedNavigationRegion
                {
                    Path = path,
                    State = UpdateGeneratedNavigationRegionState.Changed,
                }],
            });
        var selected = Select(result, CliDetail.Full);
        var dataEffect = Assert.Single(selected.Report.Data.Effects!);

        Assert.Equal(CliEffectKind.Section, Assert.Single(selected.Report.Effects).Kind);
        Assert.Equal(1, selected.Report.Data.ChangedFiles);
        Assert.Equal(0, selected.Report.Counts.Single(count => count.Name == "filesReplaced").Value);
        Assert.Equal(1, selected.Report.Counts.Single(count => count.Name == "sectionsUpdated").Value);
        Assert.Equal("framework", dataEffect.Source?.Id);
        Assert.Null(dataEffect.SourceAssetPath);

        using var json = JsonDocument.Parse(Json(result, CliDetail.Full));
        var migration = Assert.Single(json.RootElement.GetProperty("data").GetProperty("migrations").EnumerateArray());
        Assert.Equal("navigation-updated", migration.GetProperty("actions")[0].GetString());
        Assert.False(migration.TryGetProperty("isUserOwnedSource", out _));
    }

    private static UpdateMigration Migration(
        string path,
        UpdateMigrationOutcome outcome,
        IReadOnlyList<WorkspaceAdoptionAction> actions,
        IReadOnlyList<string>? fields = null,
        IReadOnlyList<WorkspaceAdoptionDerivation>? derivation = null,
        bool isUserOwnedSource = false)
        => new(path, actions, fields ?? [], derivation ?? [], outcome)
        {
            IsUserOwnedSource = isUserOwnedSource,
        };

    private static UpdatePhysicalEffect Effect(string path, UpdatePhysicalEffectOutcome outcome)
        => new(
            path,
            UpdatePhysicalEffectAction.Replace,
            [new UpdateLogicalChange(
                UpdateComparisonTargetKind.File,
                UpdateLogicalChangeAction.Replace,
                region: null,
                sourceAssetPath: null)],
            outcome,
            outcome == UpdatePhysicalEffectOutcome.CompletionUnknown
                ? UpdatePhysicalEffectResidual.Unknown
                : UpdatePhysicalEffectResidual.None);

    private static UpdateResult Result(
        UpdateMode mode,
        IReadOnlyList<UpdateMigration>? migrations = null,
        IReadOnlyList<UpdatePhysicalEffect>? effects = null,
        IReadOnlyList<UpdateFinding>? findings = null,
        UpdateVerificationState verification = UpdateVerificationState.NotRequested,
        UpdateSource? source = null,
        UpdateGeneratedNavigation? generatedNavigation = null)
        => new(new UpdateResultFormation
        {
            Workspace = null,
            Mode = mode,
            Force = false,
            Prune = false,
            Automatic = true,
            Source = source,
            Comparisons = [],
            GeneratedNavigation = generatedNavigation,
            Effects = effects ?? [],
            Lifecycle = new UpdateLifecycle
            {
                Trust = UpdateLifecycleTrust.NotRequested,
                Coverage = UpdateLifecycleCoverage.NotRequested,
                Action = UpdateLifecycleAction.None,
                Outcome = UpdateLifecycleOutcome.NotRequested,
            },
            Recovery = new UpdateRecovery
            {
                State = UpdateRecoveryState.NotRequired,
                ProtectedPaths = [],
                ResidualPath = null,
            },
            Verification = verification,
            Findings = findings ?? [],
            Migrations = migrations ?? [],
        });

    private static string Json(UpdateResult result, CliDetail detail)
        => CliJsonRenderer.Render(Select(result, detail), UpdatePresentation.Rendering.DataJsonTypeInfo);

    private static CliSelectedReport<UpdateData> Select(UpdateResult result, CliDetail detail)
    {
        var selection = new CliSelection(detail, null);
        var rendering = UpdatePresentation.Rendering;
        var selected = CliReportTrimmer.Trim(rendering.Selector(result, selection), selection, rendering.Shape);
        return rendering.SelectText!(selected);
    }
}
