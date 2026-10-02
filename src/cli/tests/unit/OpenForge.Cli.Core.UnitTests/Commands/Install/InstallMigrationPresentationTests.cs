using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.Result;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Result;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Presentation.Install;
using OpenForge.Cli.Core.Presentation.Install.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Install;

public sealed class InstallMigrationPresentationTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install migration JSON is additive and keeps the exact schema 3 row shape at every detail level"), Trait("Feature", "install-migration-presentation"), Trait("Evidence", "Unit")]
    public void MigrationJsonKeepsExactShapeAtEveryDetailLevel()
    {
        var result = Result(
            InstallMode.DryRun,
            Facts(
                [Migration(
                    ".agents/skills/local/SKILL.md",
                    InstallMigrationOutcome.Planned,
                    [WorkspaceAdoptionAction.MetadataCompleted, WorkspaceAdoptionAction.EntriesSectionAdded],
                    ["open-forge.description"],
                    [new WorkspaceAdoptionDerivation("open-forge.description", WorkspaceAdoptionDerivationSource.ExistingTitle)])],
                classification: InstallManagementClassification.ManagedAdoption,
                preview: true));

        foreach (var detail in new[] { CliDetail.Minimal, CliDetail.Full })
        {
            using var json = JsonDocument.Parse(Json(result, detail));
            var root = json.RootElement;
            Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
            var migrations = root.GetProperty("data").GetProperty("migrations");
            Assert.Equal("migrations", root.GetProperty("data").EnumerateObject().Last().Name);
            var row = Assert.Single(migrations.EnumerateArray());
            Assert.Equal(
                new[] { "path", "actions", "fields", "derivation", "outcome" },
                row.EnumerateObject().Select(property => property.Name));
            Assert.Equal(".agents/skills/local/SKILL.md", row.GetProperty("path").GetString());
            Assert.Equal(
                new[] { "metadata-completed", "entries-section-added" },
                row.GetProperty("actions").EnumerateArray().Select(value => value.GetString()));
            Assert.Equal(new[] { "open-forge.description" }, row.GetProperty("fields").EnumerateArray().Select(value => value.GetString()));
            Assert.Equal("planned", row.GetProperty("outcome").GetString());
            var derivation = Assert.Single(row.GetProperty("derivation").EnumerateArray());
            Assert.Equal(new[] { "field", "source" }, derivation.EnumerateObject().Select(property => property.Name));
            Assert.Equal("open-forge.description", derivation.GetProperty("field").GetString());
            Assert.Equal("existing-title", derivation.GetProperty("source").GetString());
        }

        var dryRunText = Text(result, CliDetail.Standard);
        Assert.Contains("Planned migration", dryRunText, StringComparison.Ordinal);
        Assert.Contains(".agents/skills/local/SKILL.md", dryRunText, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install omits migration JSON and keeps internal migration facts empty when no migration was planned"), Trait("Feature", "install-migration-presentation"), Trait("Evidence", "Unit")]
    public void NoOpOmitsMigrationsFromJsonAndFacts()
    {
        var facts = InstallResultFactsFactory.NoOp(NoOpPlan());
        Assert.Empty(facts.Migrations);

        using var json = JsonDocument.Parse(Json(Result(InstallMode.Apply, facts), CliDetail.Full));
        Assert.False(json.RootElement.GetProperty("data").TryGetProperty("migrations", out _));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Install build without a plan or file effects omits intended migrations"), Trait("Feature", "install-migration-presentation"), Trait("Evidence", "Unit")]
    public void BuildWithoutPlanOrFileEffectsOmitsMigrations()
    {
        var plan = NoOpPlan();
        var intended = plan.IntendedState with
        {
            Migrations = [MigrationPlan(".agents/skills/local/SKILL.md")],
        };
        var facts = InstallResultFactsFactory.FromBuild(new InstallPlanBuild
        {
            ManagementState = InstallManagementState.ManagedAdoption,
            Plan = null,
            Findings = [],
            Evidence = new InstallPlanningEvidence
            {
                Payload = null,
                IntendedState = intended,
            },
        });

        Assert.Empty(facts.Migrations);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Install plan stages omit migrations without a matching file effect and keep backed migrations planned"), Trait("Feature", "install-migration-presentation"), Trait("Evidence", "Unit")]
    public void PlanStagesRequireMatchingFileEffectsForMigrations()
    {
        const string path = ".agents/skills/local/SKILL.md";
        var plan = PlanWithMigrations([MigrationPlan(path)], []);

        Assert.Empty(InstallResultFactsFactory.DryRun(plan).Migrations);
        Assert.Empty(InstallResultFactsFactory.DryRun(plan with
        {
            Effects = [EffectIdentity(path, InstallEffectKind.Directory)],
        }).Migrations);
        Assert.Empty(InstallResultFactsFactory.DryRun(plan with
        {
            Effects = [EffectIdentity(".agents/skills/other/SKILL.md", InstallEffectKind.File)],
        }).Migrations);

        var preview = InstallResultFactsFactory.DryRun(plan with
        {
            Effects = [EffectIdentity(path, InstallEffectKind.File)],
        });
        var unstarted = InstallResultFactsFactory.PlanBoundary(plan with
        {
            Effects = [EffectIdentity(path, InstallEffectKind.File)],
        });

        Assert.Equal(InstallMigrationOutcome.Planned, Assert.Single(preview.Migrations).Outcome);
        Assert.Equal(InstallMigrationOutcome.Planned, Assert.Single(unstarted.Migrations).Outcome);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Install migration facts are copied into a stable read-only collection"), Trait("Feature", "install-migration-presentation"), Trait("Evidence", "Unit")]
    public void MigrationFactsMaterializeTheirInput()
    {
        var migrations = new List<InstallMigration>
        {
            Migration(".agents/skills/local/SKILL.md", InstallMigrationOutcome.Planned, [WorkspaceAdoptionAction.MetadataCompleted]),
        };
        var facts = Facts(migrations);

        migrations.Clear();

        Assert.Single(facts.Migrations);
        Assert.IsAssignableFrom<IReadOnlyList<InstallMigration>>(facts.Migrations);
        Assert.Throws<NotSupportedException>(() => ((IList<InstallMigration>)facts.Migrations).Add(
            Migration(".agents/skills/other/SKILL.md", InstallMigrationOutcome.Planned, [WorkspaceAdoptionAction.MetadataCompleted])));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Install migration outcome is applied only for a verified non-directory effect at the same path"), Trait("Feature", "install-migration-presentation"), Trait("Evidence", "Unit")]
    public void MigrationOutcomeRequiresMatchingVerifiedFileEffect()
    {
        const string path = ".agents/skills/local/SKILL.md";
        foreach (var outcome in new[]
        {
            InstallEffectOutcome.Planned,
            InstallEffectOutcome.NotStarted,
            InstallEffectOutcome.VerificationFailed,
            InstallEffectOutcome.CompletionUnknown,
        })
        {
            Assert.Equal(
                InstallMigrationOutcome.Planned,
                InstallResultFactsFactory.MigrationOutcomeForPath(path, [Effect(path, InstallEffectKind.File, outcome)]));
        }

        Assert.Equal(
            InstallMigrationOutcome.Planned,
            InstallResultFactsFactory.MigrationOutcomeForPath(path, [Effect(path, InstallEffectKind.Directory, InstallEffectOutcome.Verified)]));
        Assert.Equal(
            InstallMigrationOutcome.Planned,
            InstallResultFactsFactory.MigrationOutcomeForPath(path, [Effect(".agents/skills/other/SKILL.md", InstallEffectKind.File, InstallEffectOutcome.Verified)]));
        Assert.Equal(
            InstallMigrationOutcome.Applied,
            InstallResultFactsFactory.MigrationOutcomeForPath(path, [Effect(path, InstallEffectKind.File, InstallEffectOutcome.Verified)]));
        Assert.Equal(
            InstallMigrationOutcome.Planned,
            InstallResultFactsFactory.MigrationOutcomeForPath(path,
            [
                Effect(path, InstallEffectKind.File, InstallEffectOutcome.Verified),
                Effect(path, InstallEffectKind.GeneratedRegion, InstallEffectOutcome.CompletionUnknown),
            ]));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Install migration text distinguishes verified and pending paths after mixed effects"), Trait("Feature", "install-migration-presentation"), Trait("Evidence", "Unit")]
    public void MixedMigrationResultsKeepPathSpecificVerbs()
    {
        const string appliedPath = ".agents/skills/ready/SKILL.md";
        const string pendingPath = ".agents/skills/pending/SKILL.md";
        var facts = Facts(
            [
                Migration(appliedPath, InstallMigrationOutcome.Applied, [WorkspaceAdoptionAction.MetadataCompleted]),
                Migration(pendingPath, InstallMigrationOutcome.Planned, [WorkspaceAdoptionAction.EntriesSectionAdded]),
            ],
            [
                Effect(appliedPath, InstallEffectKind.File, InstallEffectOutcome.Verified),
                Effect(pendingPath, InstallEffectKind.File, InstallEffectOutcome.CompletionUnknown),
            ],
            InstallManagementClassification.ManagedAdoption);
        var result = Result(
            InstallMode.Apply,
            facts,
            [new InstallFinding(InstallFindingCode.WriteFailed, "A target write failed.", pendingPath)]);

        foreach (var detail in new[] { CliDetail.Minimal, CliDetail.Standard, CliDetail.Full })
        {
            var text = Text(result, detail);
            Assert.Contains("Migrated: metadata updated", text, StringComparison.Ordinal);
            Assert.Contains("Planned migration: Entries section added", text, StringComparison.Ordinal);
            Assert.Contains(appliedPath, text, StringComparison.Ordinal);
            Assert.Contains(pendingPath, text, StringComparison.Ordinal);
            Assert.Contains("Migrated 1 source.", text, StringComparison.Ordinal);
            Assert.Contains("Planned migration for 1 source.", text, StringComparison.Ordinal);
        }
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Successful managed adoption preserves effects and remains a completed informational result"), Trait("Feature", "install-migration-presentation"), Trait("Evidence", "Unit")]
    public void ManagedAdoptionClassificationDoesNotSuppressEffectsOrAddWarnings()
    {
        const string path = ".agents/skills/local/SKILL.md";
        var result = Result(
            InstallMode.Apply,
            Facts(
                [Migration(path, InstallMigrationOutcome.Applied, [WorkspaceAdoptionAction.MetadataCompleted])],
                [Effect(path, InstallEffectKind.File, InstallEffectOutcome.Verified)],
                InstallManagementClassification.ManagedAdoption,
                payloadFiles: 0));
        var selected = Select(result, CliDetail.Full);

        Assert.Equal(CliSemanticStatus.Complete, selected.Report.Status);
        Assert.Empty(selected.Report.Findings);
        Assert.Single(selected.Report.Effects);
        Assert.Equal((decimal?)1m, selected.Report.Counts.Single(count => count.Name == "filesReplaced").Value);
        Assert.Equal("managed-adoption", selected.Report.Data.Classification);

        using var json = JsonDocument.Parse(CliJsonRenderer.Render(selected, InstallPresentation.Rendering.DataJsonTypeInfo));
        Assert.Equal("managed-adoption", json.RootElement.GetProperty("data").GetProperty("classification").GetString());
        Assert.Equal(1, json.RootElement.GetProperty("effects").GetArrayLength());
        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, InstallPresentation.Rendering.DataTextRenderer).Content;
        Assert.Contains("Migrated: metadata updated", text, StringComparison.Ordinal);
        Assert.DoesNotContain("warning", text, StringComparison.OrdinalIgnoreCase);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Install payload footprint excludes workspace-owned migration targets"), Trait("Feature", "install-migration-presentation"), Trait("Evidence", "Unit")]
    public void PayloadFootprintExcludesUserOwnedPaths()
    {
        var intended = new InstallIntendedState
        {
            TargetBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal)
            {
                [".agents/skills/local/SKILL.md"] = [1],
                [".agents/directives/csharp/_csharp.md"] = [2],
            },
            ManagedBlockBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal),
            GeneratedRegionPaths = new HashSet<string>(StringComparer.Ordinal),
            ProjectionInputs = [],
            Migrations = [],
            UserOwnedPaths = new HashSet<string>([".agents/skills/local/SKILL.md"], StringComparer.Ordinal),
        };

        Assert.Equal(1, InstallResultFactsFactory.PayloadFileCount(intended));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Install effects do not describe user-owned migration files as bundled payload assets"), Trait("Feature", "install-migration-presentation"), Trait("Evidence", "Unit")]
    public void UserOwnedEffectHasNoBundledSourceAsset()
    {
        const string path = ".agents/skills/local/SKILL.md";
        var identity = new InstallEffectIdentity
        {
            Path = path,
            Kind = InstallEffectKind.File,
            Action = InstallEffectAction.Replace,
            SourceAssetPath = "framework/skills/local/SKILL.md",
        };

        Assert.Null(InstallResultFactsFactory.SourceAssetPathFor(
            identity,
            new HashSet<string>([path], StringComparer.Ordinal)));
        Assert.Equal(
            identity.SourceAssetPath,
            InstallResultFactsFactory.SourceAssetPathFor(identity, new HashSet<string>(StringComparer.Ordinal)));
    }

    private static InstallMigration Migration(
        string path,
        InstallMigrationOutcome outcome,
        IReadOnlyList<WorkspaceAdoptionAction> actions,
        IReadOnlyList<string>? fields = null,
        IReadOnlyList<WorkspaceAdoptionDerivation>? derivation = null)
        => new(
            path,
            actions,
            fields ?? [],
            derivation ?? [],
            outcome);

    private static InstallMigrationPlan MigrationPlan(string path)
        => new()
        {
            Path = path,
            Actions = [WorkspaceAdoptionAction.MetadataCompleted],
            Fields = ["open-forge.description"],
            Derivation = [new WorkspaceAdoptionDerivation(
                "open-forge.description",
                WorkspaceAdoptionDerivationSource.ExistingTitle)],
        };

    private static InstallPlan PlanWithMigrations(
        IReadOnlyList<InstallMigrationPlan> migrations,
        IReadOnlyList<InstallEffectIdentity> effects)
    {
        var plan = NoOpPlan();
        return plan with
        {
            ManagementState = InstallManagementState.ManagedAdoption,
            IntendedState = plan.IntendedState with { Migrations = migrations },
            Effects = effects,
        };
    }

    private static InstallEffectIdentity EffectIdentity(string path, InstallEffectKind kind)
        => new()
        {
            Path = path,
            Kind = kind,
            Action = kind == InstallEffectKind.Directory
                ? InstallEffectAction.Create
                : InstallEffectAction.Replace,
            SourceAssetPath = null,
        };

    private static InstallResultFacts Facts(
        IReadOnlyList<InstallMigration> migrations,
        IReadOnlyList<InstallEffect>? effects = null,
        InstallManagementClassification? classification = null,
        int payloadFiles = 1,
        bool preview = false)
        => new(new InstallResultFactsInput
        {
            Source = null,
            Classification = classification,
            Footprint = new InstallFootprint(payloadFiles, 0, 0),
            Effects = effects ?? [],
            Migrations = migrations,
            Lifecycle = new InstallLifecycle(
                InstallLifecycleAction.Publish,
                preview ? InstallLifecycleOutcome.Planned : InstallLifecycleOutcome.Verified),
            Recovery = new InstallRecovery(InstallResultRecoveryState.NotRequired, null),
            Verification = new InstallVerification(preview
                ? InstallResultVerificationState.NotRequested
                : InstallResultVerificationState.Verified),
        });

    private static InstallPlan NoOpPlan()
    {
        var payload = FrameworkPayload.Create(
        [
            FrameworkPayloadAsset.Create("AGENTS.md", [1]),
            FrameworkPayloadAsset.Create("CLAUDE.md", [2]),
            FrameworkPayloadAsset.Create(".agents/loader.md", [3]),
        ]);
        return new InstallPlan
        {
            Request = new InstallRequest(Workspace(), InstallMode.Apply, force: false, automatic: true, allowsInteractiveConfirmation: false),
            Payload = payload,
            IntendedState = new InstallIntendedState
            {
                TargetBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal),
                ManagedBlockBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal),
                GeneratedRegionPaths = new HashSet<string>(StringComparer.Ordinal),
                ProjectionInputs = [],
                Migrations = [],
                UserOwnedPaths = new HashSet<string>(StringComparer.Ordinal),
            },
            ManagementState = InstallManagementState.TrustedExact,
            DirectoryCreations = [],
            TargetEffects = [],
            OwnershipEffect = null,
            Effects = [],
            Findings = [],
        };
    }

    private static InstallResult Result(
        InstallMode mode,
        InstallResultFacts facts,
        IReadOnlyList<InstallFinding>? findings = null)
        => new(
            Workspace(),
            new InstallBindingInput(Force: false, Automatic: true, Mode: mode),
            findings ?? [],
            facts: facts);

    private static InstallEffect Effect(
        string path,
        InstallEffectKind kind,
        InstallEffectOutcome outcome)
        => new(new InstallEffectInput
        {
            Path = path,
            Kind = kind,
            Action = kind == InstallEffectKind.Directory
                ? InstallEffectAction.Create
                : InstallEffectAction.Replace,
            SourceAssetPath = null,
            Outcome = outcome,
            Residual = InstallEffectResidual.None,
        });

    private static string Json(InstallResult result, CliDetail detail)
    {
        var selected = Select(result, detail);
        return CliJsonRenderer.Render(selected, InstallPresentation.Rendering.DataJsonTypeInfo);
    }

    private static string Text(InstallResult result, CliDetail detail)
    {
        var selected = Select(result, detail);
        return CliTextRenderer.Render(selected, CliTextStyle.Plain, InstallPresentation.Rendering.DataTextRenderer).Content;
    }

    private static CliSelectedReport<InstallData> Select(InstallResult result, CliDetail detail)
    {
        var selection = new CliSelection(detail, null);
        var rendering = InstallPresentation.Rendering;
        var selected = CliReportTrimmer.Trim(rendering.Selector(result, selection), selection, rendering.Shape);
        return rendering.SelectText!(selected);
    }

    private static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "install-migration-presentation"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.CurrentDirectory);
    }
}
