using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.Result;
using System.Text;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Effects;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Content;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Update;

[Trait("Feature", "update-workspace-adoption"), Trait("Evidence", "Integration")]
public sealed class UpdateWorkspaceAdoptionIntegrationTests
{
    private const string SkillsCataloguePath = ".agents/skills/_skills.md";

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update previews and applies missing native Skill metadata and catalogues exactly once")]
    public async Task PreviewsAppliesAndRepeatsNativeSkillAdoption()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-adoption-apply");
        var payload = await UpdateWorkspaceAdoptionTestFixture.EstablishSyntheticEarlierPayloadAsync(workspace);

        const string partialBody = "# Partial Local Skill\n\nPreserve this body and [its reference](references/guide.md).\n";
        const string partialSkill = "---\nname: partial-skill\nlicense: MIT\ncustom-note: '  keep: these bytes  '\n---\n" + partialBody;
        const string missingBody = "# Frontmatterless Skill\n\nPreserve this body too.\n";
        var partial = UpdateWorkspaceAdoptionTestFixture.SeedSkill(
            workspace,
            "partial-skill",
            partialSkill,
            partialBody);
        var missing = UpdateWorkspaceAdoptionTestFixture.SeedSkill(
            workspace,
            "frontmatterless",
            missingBody,
            missingBody);
        var beforePreview = workspace.SnapshotHashes();

        var preview = await workspace.ExecuteAsync(workspace.Request(
            mode: UpdateMode.DryRun,
            force: false,
            prune: false,
            automatic: true,
            allowsInteractiveConfirmation: false));

        UpdateWorkspaceAdoptionTestFixture.AssertStatus(preview, CliSemanticStatus.Complete);
        Assert.Empty(preview.Findings);
        Assert.NotEmpty(preview.Effects);
        Assert.All(preview.Effects, effect => Assert.Equal(UpdatePhysicalEffectOutcome.Planned, effect.Outcome));
        Assert.Equal(beforePreview, workspace.SnapshotHashes());
        AssertExpectedAdoptionEffects(preview, partial, missing);

        var partialPlan = UpdateWorkspaceAdoptionTestFixture.AssertMigration(
            preview,
            partial.SkillPath,
            UpdateMigrationOutcome.Planned);
        Assert.Contains(WorkspaceAdoptionAction.MetadataCompleted, partialPlan.Actions);
        Assert.Contains("description", partialPlan.Fields);
        Assert.Contains(partialPlan.Derivation, value =>
            value.Field == "description" && value.Source == WorkspaceAdoptionDerivationSource.Heading);

        var missingPlan = UpdateWorkspaceAdoptionTestFixture.AssertMigration(
            preview,
            missing.SkillPath,
            UpdateMigrationOutcome.Planned);
        Assert.Contains(WorkspaceAdoptionAction.MetadataCompleted, missingPlan.Actions);
        Assert.Contains("name", missingPlan.Fields);
        Assert.Contains("description", missingPlan.Fields);
        Assert.Contains(missingPlan.Derivation, value =>
            value.Field == "name" && value.Source == WorkspaceAdoptionDerivationSource.DirectoryName);
        Assert.Contains(missingPlan.Derivation, value =>
            value.Field == "description" && value.Source == WorkspaceAdoptionDerivationSource.Heading);

        var applied = await workspace.ExecuteAsync(workspace.Request(
            mode: UpdateMode.Apply,
            force: false,
            prune: false,
            automatic: true,
            allowsInteractiveConfirmation: false));

        UpdateWorkspaceAdoptionTestFixture.AssertStatus(applied, CliSemanticStatus.Complete);
        Assert.Empty(applied.Findings);
        Assert.Equal(UpdateVerificationState.Verified, applied.Verification);
        Assert.All(applied.Effects, effect => Assert.Equal(UpdatePhysicalEffectOutcome.Verified, effect.Outcome));
        UpdateWorkspaceAdoptionTestFixture.AssertAppliedMigrationEffects(applied);
        Assert.Equal(payload.CurrentLoaderBytes, workspace.ReadBytes(UpdateIntegrationWorkspace.ManagedPath));
        AssertSkillPreservedWithCompletedMetadata(workspace, partial, partialBody);
        AssertSkillPreservedWithCompletedMetadata(workspace, missing, missingBody);
        AssertLocalAdoptionHasNoFrameworkProvenance(applied, partial, missing);

        var skillsCatalogue = workspace.ReadText(SkillsCataloguePath);
        Assert.Contains("partial-skill", skillsCatalogue, StringComparison.Ordinal);
        Assert.Contains("frontmatterless", skillsCatalogue, StringComparison.Ordinal);
        foreach (var skill in new[] { partial, missing })
        {
            Assert.True(workspace.Exists(skill.ReferencesCataloguePath));
            Assert.Contains("guide.md", workspace.ReadText(skill.ReferencesCataloguePath), StringComparison.Ordinal);
            Assert.Equal(skill.ReferenceBytes, workspace.ReadBytes(skill.ReferencePath));
            Assert.Equal(skill.SupportBytes, workspace.ReadBytes(skill.SupportPath));
            Assert.Equal(skill.OverwriteBytes, workspace.ReadBytes(skill.OverwritePath));
            Assert.Equal(skill.BinaryBytes, workspace.ReadBytes(skill.BinaryPath));
        }

        AssertFrameworkDoesNotOwnAdoptedFilesOrCatalogues(workspace, partial, missing);

        var beforeRepeat = workspace.SnapshotHashes();
        var repeated = await workspace.ExecuteAsync(workspace.Request(
            mode: UpdateMode.Apply,
            force: false,
            prune: false,
            automatic: true,
            allowsInteractiveConfirmation: false));

        UpdateWorkspaceAdoptionTestFixture.AssertStatus(repeated, CliSemanticStatus.Complete);
        Assert.Empty(repeated.Findings);
        Assert.Empty(repeated.Effects);
        Assert.Empty(repeated.Migrations);
        Assert.Equal(UpdateVerificationState.Verified, repeated.Verification);
        Assert.Equal(beforeRepeat, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update preserves Install-adopted reference catalogue ownership while adding a reference and Skill")]
    public async Task PreservesInstallAdoptedReferenceCatalogueOwnershipDuringUpdate()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-install-adopted-reference-catalogue");
        workspace.RegisterTestCleanupPath(".agents/skills");
        workspace.CreateDirectory(".agents/skills");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);

        const string installedBody = "# Install-adopted Skill\n\nKeep its native bytes intact.\n";
        const string installedContents =
            "---\nname: install-adopted-skill\ndescription: Existing complete metadata\nlicense: MIT\ncustom-note: preserve-me\n---\n"
            + installedBody;
        var installedSkill = UpdateWorkspaceAdoptionTestFixture.SeedSkill(
            workspace,
            "install-adopted-skill",
            installedContents,
            installedBody);
        var install = await InstallOperationFactory.Create(
                InstallInteractionTestSupport.Unavailable(),
                workspace.LockStoreRoot)
            .ExecuteAsync(
                new InstallRequest(
                    workspace.Workspace,
                    InstallMode.Apply,
                    force: false,
                    automatic: true,
                    allowsInteractiveConfirmation: false),
                TestContext.Current.CancellationToken);

        UpdateWorkspaceAdoptionTestFixture.AssertStatus(install, CliSemanticStatus.Complete);
        Assert.Empty(install.Findings);
        Assert.Equal(InstallManagementClassification.ManagedAdoption, install.Facts.Classification);
        Assert.Equal(installedSkill.OriginalSkillBytes, workspace.ReadBytes(installedSkill.SkillPath));
        Assert.Equal(installedSkill.ReferenceBytes, workspace.ReadBytes(installedSkill.ReferencePath));
        Assert.Equal(installedSkill.SupportBytes, workspace.ReadBytes(installedSkill.SupportPath));
        Assert.Equal(installedSkill.OverwriteBytes, workspace.ReadBytes(installedSkill.OverwritePath));
        Assert.Equal(installedSkill.BinaryBytes, workspace.ReadBytes(installedSkill.BinaryPath));
        Assert.True(workspace.Exists(installedSkill.ReferencesCataloguePath));
        Assert.Contains("guide.md", workspace.ReadText(installedSkill.ReferencesCataloguePath), StringComparison.Ordinal);
        var wholeFilePathsAfterInstall = AssertInstallAdoptionOwnership(workspace, installedSkill);

        const string secondReferencePath = ".agents/skills/install-adopted-skill/references/additional.md";
        var secondReferenceBytes = Encoding.UTF8.GetBytes("# Additional Reference\n\nPreserve this authored source.\n");
        workspace.WriteText(secondReferencePath, Encoding.UTF8.GetString(secondReferenceBytes));

        const string addedBody = "# Added Native Skill\n\nThis Skill is already complete.\n";
        const string addedContents =
            "---\nname: added-native-skill\ndescription: New complete metadata\nlicense: Apache-2.0\ncustom-note: keep-this-too\n---\n"
            + addedBody;
        var addedSkill = UpdateWorkspaceAdoptionTestFixture.SeedSkill(
            workspace,
            "added-native-skill",
            addedContents,
            addedBody);
        var beforePreview = workspace.SnapshotHashes();

        var preview = await workspace.ExecuteAsync(workspace.Request(
            mode: UpdateMode.DryRun,
            force: false,
            prune: false,
            automatic: true,
            allowsInteractiveConfirmation: false));

        UpdateWorkspaceAdoptionTestFixture.AssertStatus(preview, CliSemanticStatus.Complete);
        Assert.Empty(preview.Findings);
        Assert.NotEmpty(preview.Effects);
        Assert.All(preview.Effects, effect => Assert.Equal(UpdatePhysicalEffectOutcome.Planned, effect.Outcome));
        Assert.Equal(beforePreview, workspace.SnapshotHashes());

        var userCataloguePlan = UpdateWorkspaceAdoptionTestFixture.AssertMigration(
            preview,
            installedSkill.ReferencesCataloguePath,
            UpdateMigrationOutcome.Planned);
        Assert.Collection(
            userCataloguePlan.Actions,
            action => Assert.Equal(WorkspaceAdoptionAction.NavigationUpdated, action));
        Assert.True(
            userCataloguePlan.IsUserOwnedSource,
            "The Install-adopted references catalogue navigation migration must retain its user-source fact.");

        var managedSkillsCataloguePlan = UpdateWorkspaceAdoptionTestFixture.AssertMigration(
            preview,
            SkillsCataloguePath,
            UpdateMigrationOutcome.Planned);
        Assert.Collection(
            managedSkillsCataloguePlan.Actions,
            action => Assert.Equal(WorkspaceAdoptionAction.NavigationUpdated, action));
        Assert.False(
            managedSkillsCataloguePlan.IsUserOwnedSource,
            "The Framework-owned Skills catalogue navigation migration must not be marked as a user source.");

        var addedCataloguePlan = UpdateWorkspaceAdoptionTestFixture.AssertMigration(
            preview,
            addedSkill.ReferencesCataloguePath,
            UpdateMigrationOutcome.Planned);
        Assert.Collection(
            addedCataloguePlan.Actions,
            action => Assert.Equal(WorkspaceAdoptionAction.EntrypointCreated, action),
            action => Assert.Equal(WorkspaceAdoptionAction.NavigationUpdated, action));
        Assert.DoesNotContain(preview.Migrations, migration =>
            migration.Path == installedSkill.SkillPath || migration.Path == addedSkill.SkillPath);
        Assert.Equal(3, preview.Migrations.Count);

        var applied = await workspace.ExecuteAsync(workspace.Request(
            mode: UpdateMode.Apply,
            force: false,
            prune: false,
            automatic: true,
            allowsInteractiveConfirmation: false));

        UpdateWorkspaceAdoptionTestFixture.AssertStatus(applied, CliSemanticStatus.Complete);
        Assert.Empty(applied.Findings);
        Assert.Equal(UpdateVerificationState.Verified, applied.Verification);
        Assert.All(applied.Effects, effect => Assert.Equal(UpdatePhysicalEffectOutcome.Verified, effect.Outcome));
        UpdateWorkspaceAdoptionTestFixture.AssertAppliedMigrationEffects(applied);

        var appliedUserCatalogueMigration = UpdateWorkspaceAdoptionTestFixture.AssertMigration(
            applied,
            installedSkill.ReferencesCataloguePath,
            UpdateMigrationOutcome.Applied);
        Assert.Collection(
            appliedUserCatalogueMigration.Actions,
            action => Assert.Equal(WorkspaceAdoptionAction.NavigationUpdated, action));
        Assert.True(appliedUserCatalogueMigration.IsUserOwnedSource);
        var appliedManagedSkillsCatalogueMigration = UpdateWorkspaceAdoptionTestFixture.AssertMigration(
            applied,
            SkillsCataloguePath,
            UpdateMigrationOutcome.Applied);
        Assert.Collection(
            appliedManagedSkillsCatalogueMigration.Actions,
            action => Assert.Equal(WorkspaceAdoptionAction.NavigationUpdated, action));
        Assert.False(appliedManagedSkillsCatalogueMigration.IsUserOwnedSource);
        var appliedAddedCatalogueMigration = UpdateWorkspaceAdoptionTestFixture.AssertMigration(
            applied,
            addedSkill.ReferencesCataloguePath,
            UpdateMigrationOutcome.Applied);
        Assert.Collection(
            appliedAddedCatalogueMigration.Actions,
            action => Assert.Equal(WorkspaceAdoptionAction.EntrypointCreated, action),
            action => Assert.Equal(WorkspaceAdoptionAction.NavigationUpdated, action));
        Assert.Equal(3, applied.Migrations.Count);

        Assert.Equal(installedSkill.OriginalSkillBytes, workspace.ReadBytes(installedSkill.SkillPath));
        Assert.Equal(installedSkill.ReferenceBytes, workspace.ReadBytes(installedSkill.ReferencePath));
        Assert.Equal(installedSkill.SupportBytes, workspace.ReadBytes(installedSkill.SupportPath));
        Assert.Equal(installedSkill.OverwriteBytes, workspace.ReadBytes(installedSkill.OverwritePath));
        Assert.Equal(installedSkill.BinaryBytes, workspace.ReadBytes(installedSkill.BinaryPath));
        Assert.Equal(secondReferenceBytes, workspace.ReadBytes(secondReferencePath));
        Assert.Contains("guide.md", workspace.ReadText(installedSkill.ReferencesCataloguePath), StringComparison.Ordinal);
        Assert.Contains("additional.md", workspace.ReadText(installedSkill.ReferencesCataloguePath), StringComparison.Ordinal);
        var skillsCatalogue = workspace.ReadText(SkillsCataloguePath);
        Assert.Contains("install-adopted-skill", skillsCatalogue, StringComparison.Ordinal);
        Assert.Contains("added-native-skill", skillsCatalogue, StringComparison.Ordinal);

        Assert.Equal(addedSkill.OriginalSkillBytes, workspace.ReadBytes(addedSkill.SkillPath));
        Assert.Equal(addedSkill.ReferenceBytes, workspace.ReadBytes(addedSkill.ReferencePath));
        Assert.Equal(addedSkill.SupportBytes, workspace.ReadBytes(addedSkill.SupportPath));
        Assert.Equal(addedSkill.OverwriteBytes, workspace.ReadBytes(addedSkill.OverwritePath));
        Assert.Equal(addedSkill.BinaryBytes, workspace.ReadBytes(addedSkill.BinaryPath));
        Assert.True(workspace.Exists(addedSkill.ReferencesCataloguePath));
        Assert.Contains("guide.md", workspace.ReadText(addedSkill.ReferencesCataloguePath), StringComparison.Ordinal);
        var wholeFilePathsAfterUpdate = AssertInstallAdoptionOwnership(workspace, installedSkill, addedSkill);
        Assert.Equal(wholeFilePathsAfterInstall, wholeFilePathsAfterUpdate);

        var beforeRepeat = workspace.SnapshotHashes();
        var repeated = await workspace.ExecuteAsync(workspace.Request(
            mode: UpdateMode.Apply,
            force: false,
            prune: false,
            automatic: true,
            allowsInteractiveConfirmation: false));

        UpdateWorkspaceAdoptionTestFixture.AssertStatus(repeated, CliSemanticStatus.Complete);
        Assert.Empty(repeated.Findings);
        Assert.Equal(UpdateVerificationState.Verified, repeated.Verification);
        Assert.Empty(repeated.Effects);
        Assert.Empty(repeated.Migrations);
        Assert.Equal(beforeRepeat, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory, InlineData(false), InlineData(true)]
    public async Task NoSkillAndCompleteNativeSkillControlsAvoidMetadataMigration(bool includeCompleteSkill)
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-adoption-control");
        var payload = await UpdateWorkspaceAdoptionTestFixture.EstablishSyntheticEarlierPayloadAsync(workspace);
        UpdateWorkspaceAdoptionTestFixture.SkillSeed? skill = null;
        if (includeCompleteSkill)
        {
            const string body = "# Complete Native Skill\n\nAlready complete and unchanged.\n";
            const string contents = "---\nname: complete-skill\ndescription: Existing description\nlicense: Apache-2.0\ncustom-note: keep-me\n---\n" + body;
            skill = UpdateWorkspaceAdoptionTestFixture.SeedSkill(
                workspace,
                "complete-skill",
                contents,
                body);
        }

        var originalSkillBytes = skill?.OriginalSkillBytes;
        var result = await workspace.ExecuteAsync(workspace.Request(
            mode: UpdateMode.Apply,
            force: false,
            prune: false,
            automatic: true,
            allowsInteractiveConfirmation: false));

        UpdateWorkspaceAdoptionTestFixture.AssertStatus(result, CliSemanticStatus.Complete);
        Assert.Empty(result.Findings);
        Assert.Equal(UpdateVerificationState.Verified, result.Verification);
        Assert.Equal(payload.CurrentLoaderBytes, workspace.ReadBytes(UpdateIntegrationWorkspace.ManagedPath));
        Assert.DoesNotContain(result.Migrations, migration =>
            migration.Actions.Contains(WorkspaceAdoptionAction.MetadataCompleted));
        if (skill is null)
        {
            Assert.Empty(result.Migrations);
            Assert.DoesNotContain(result.Effects, effect => effect.Path.StartsWith(".agents/skills/", StringComparison.Ordinal));
        }
        else
        {
            Assert.DoesNotContain(result.Migrations, migration => migration.Path == skill.SkillPath);
            Assert.Equal(originalSkillBytes, workspace.ReadBytes(skill.SkillPath));
        }

        var repeated = await workspace.ExecuteAsync(workspace.Request(
            mode: UpdateMode.Apply,
            force: false,
            prune: false,
            automatic: true,
            allowsInteractiveConfirmation: false));
        UpdateWorkspaceAdoptionTestFixture.AssertStatus(repeated, CliSemanticStatus.Complete);
        Assert.Empty(repeated.Effects);
        Assert.Empty(repeated.Migrations);
    }

    [Trait("Boundary", "Interaction")]
    [Fact(DisplayName = "Cancelling Update leaves adoption bytes intact and migrations planned")]
    public async Task CancellationPreservesPlannedAdoptionWithoutWrites()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-adoption-cancel");
        await UpdateWorkspaceAdoptionTestFixture.EstablishSyntheticEarlierPayloadAsync(workspace);
        const string body = "# Cancelled Skill\n\nThe original body stays here.\n";
        var skill = UpdateWorkspaceAdoptionTestFixture.SeedSkill(
            workspace,
            "cancelled-skill",
            body,
            body);
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(
            workspace.Request(automatic: false, allowsInteractiveConfirmation: true),
            canPrompt: true,
            input: "n\n");

        UpdateWorkspaceAdoptionTestFixture.AssertStatus(result, CliSemanticStatus.Interrupted);
        Assert.DoesNotContain(result.Effects, effect => effect.Outcome == UpdatePhysicalEffectOutcome.Verified);
        Assert.All(result.Migrations, migration => Assert.Equal(UpdateMigrationOutcome.Planned, migration.Outcome));
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(skill.OriginalSkillBytes, workspace.ReadBytes(skill.SkillPath));
    }

    private static void AssertExpectedAdoptionEffects(
        UpdateResult result,
        UpdateWorkspaceAdoptionTestFixture.SkillSeed partial,
        UpdateWorkspaceAdoptionTestFixture.SkillSeed missing)
    {
        var paths = result.Effects.Select(effect => effect.Path).ToArray();
        Assert.Contains(UpdateIntegrationWorkspace.ManagedPath, paths);
        Assert.Contains(SkillsCataloguePath, paths);
        foreach (var skill in new[] { partial, missing })
        {
            Assert.Contains(skill.SkillPath, paths);
            Assert.Contains(skill.ReferencesCataloguePath, paths);
            Assert.Contains(result.Migrations, migration =>
                migration.Path == skill.ReferencesCataloguePath
                && migration.Actions.Contains(WorkspaceAdoptionAction.EntrypointCreated));
        }

        Assert.Contains(result.Migrations, migration =>
            migration.Path == SkillsCataloguePath
            && migration.Actions.Contains(WorkspaceAdoptionAction.NavigationUpdated));
    }

    private static void AssertSkillPreservedWithCompletedMetadata(
        UpdateIntegrationWorkspace workspace,
        UpdateWorkspaceAdoptionTestFixture.SkillSeed skill,
        string originalBody)
    {
        var updated = Encoding.UTF8.GetString(workspace.ReadBytes(skill.SkillPath));
        Assert.Contains("description:", updated, StringComparison.Ordinal);
        Assert.EndsWith(originalBody, updated, StringComparison.Ordinal);
        if (skill.Slug == "partial-skill")
        {
            Assert.Contains("name: partial-skill\n", updated, StringComparison.Ordinal);
            Assert.Contains("license: MIT\n", updated, StringComparison.Ordinal);
            Assert.Contains("custom-note: '  keep: these bytes  '\n", updated, StringComparison.Ordinal);
            return;
        }

        Assert.Contains("name: \"frontmatterless\"", updated, StringComparison.Ordinal);
        Assert.Equal(originalBody, updated[updated.IndexOf(originalBody, StringComparison.Ordinal)..]);
    }

    private static void AssertLocalAdoptionHasNoFrameworkProvenance(
        UpdateResult result,
        UpdateWorkspaceAdoptionTestFixture.SkillSeed partial,
        UpdateWorkspaceAdoptionTestFixture.SkillSeed missing)
    {
        var localPaths = new[]
        {
            SkillsCataloguePath,
            partial.SkillPath,
            partial.ReferencesCataloguePath,
            missing.SkillPath,
            missing.ReferencesCataloguePath,
        };
        foreach (var path in localPaths)
        {
            var effect = Assert.Single(result.Effects, item => item.Path == path);
            Assert.NotEmpty(effect.Changes);
            Assert.All(effect.Changes, change => Assert.Null(change.SourceAssetPath));
        }
    }

    private static void AssertFrameworkDoesNotOwnAdoptedFilesOrCatalogues(
        UpdateIntegrationWorkspace workspace,
        UpdateWorkspaceAdoptionTestFixture.SkillSeed partial,
        UpdateWorkspaceAdoptionTestFixture.SkillSeed missing)
    {
        var ownership = JsonNode.Parse(workspace.ReadText(UpdateIntegrationWorkspace.OwnershipPath))!.AsObject();
        var framework = ownership["framework"]!.AsObject();
        var paths = framework["paths"]!.AsArray()
            .Select(node => node!.GetValue<string>())
            .ToArray();
        var adoptedPaths = new[]
        {
            partial.SkillPath,
            partial.ReferencesCataloguePath,
            missing.SkillPath,
            missing.ReferencesCataloguePath,
        };
        Assert.Contains(SkillsCataloguePath, paths);
        foreach (var path in adoptedPaths)
        {
            Assert.DoesNotContain(path, paths);
        }

        var generatedPaths = new[] { SkillsCataloguePath }.Concat(adoptedPaths).ToArray();
        var regions = framework["regions"]!.AsArray()
            .Select(node => (Path: node!["path"]!.GetValue<string>(), Region: node["region"]!.GetValue<string>()))
            .Where(region => generatedPaths.Contains(region.Path, StringComparer.Ordinal))
            .ToArray();
        Assert.All(regions, region => Assert.Equal("entries", region.Region));
    }

    private static string[] AssertInstallAdoptionOwnership(
        UpdateIntegrationWorkspace workspace,
        UpdateWorkspaceAdoptionTestFixture.SkillSeed installAdoptedSkill,
        params UpdateWorkspaceAdoptionTestFixture.SkillSeed[] additionalSkills)
    {
        var ownership = JsonNode.Parse(workspace.ReadText(UpdateIntegrationWorkspace.OwnershipPath))!.AsObject();
        var framework = ownership["framework"]!.AsObject();
        var wholeFilePaths = framework["paths"]!.AsArray()
            .Select(node => node!.GetValue<string>())
            .ToArray();
        Assert.Contains(SkillsCataloguePath, wholeFilePaths);

        var userSkills = new[] { installAdoptedSkill }.Concat(additionalSkills).ToArray();
        foreach (var skill in userSkills)
        {
            foreach (var userSourcePath in new[]
                     {
                         skill.SkillPath,
                         skill.ReferencePath,
                         skill.ReferencesCataloguePath,
                         skill.SupportPath,
                         skill.OverwritePath,
                         skill.BinaryPath,
                     })
            {
                Assert.DoesNotContain(userSourcePath, wholeFilePaths);
            }
        }

        var generatedRegions = framework["regions"]!.AsArray()
            .Select(node => (Path: node!["path"]!.GetValue<string>(), Region: node["region"]!.GetValue<string>()))
            .ToArray();
        Assert.Contains(generatedRegions, region =>
            region.Path == SkillsCataloguePath && region.Region == "entries");
        Assert.Contains(generatedRegions, region =>
            region.Path == installAdoptedSkill.ReferencesCataloguePath && region.Region == "entries");

        return wholeFilePaths.OrderBy(path => path, StringComparer.Ordinal).ToArray();
    }
}

[Trait("Feature", "update-workspace-adoption"), Trait("Evidence", "Integration")]
public sealed class UpdateWorkspaceAdoptionBoundaryIntegrationTests
{
    private const string SkillsCataloguePath = ".agents/skills/_skills.md";

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Current Install continues to block the synthetic earlier-loader fixture")]
    public async Task CurrentInstallStillBlocksWithoutForceOrEffects()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-adoption-install-control");
        await UpdateWorkspaceAdoptionTestFixture.EstablishSyntheticEarlierPayloadAsync(workspace);
        var before = workspace.SnapshotHashes();

        var result = await InstallOperationFactory.Create(
                InstallInteractionTestSupport.Unavailable(),
                workspace.LockStoreRoot)
            .ExecuteAsync(
                new InstallRequest(
                    workspace.Workspace,
                    InstallMode.Apply,
                    force: false,
                    automatic: true,
                    allowsInteractiveConfirmation: false),
                TestContext.Current.CancellationToken);

        Assert.Equal(InstallMode.Apply, result.Mode);
        Assert.False(result.Force);
        UpdateWorkspaceAdoptionTestFixture.AssertStatus(result, CliSemanticStatus.Blocked);
        Assert.Equal(
            InstallManagementClassification.ManagedDivergence,
            result.Facts.Classification);
        Assert.Contains(result.Findings, finding => finding.Code == InstallFindingCode.ManagedDivergence);
        Assert.Empty(result.Facts.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Selection")]
    [Theory, InlineData("directory"), InlineData("category")]
    public async Task RemovedDirectoryOrCategoryPreservesValidSkillAndCatalogues(string exclusion)
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-adoption-exclusion");
        var payload = await UpdateWorkspaceAdoptionTestFixture.EstablishSyntheticEarlierPayloadAsync(workspace);
        const string body = "# Excluded Complete Skill\n\nNo adoption should touch this.\n";
        var skill = UpdateWorkspaceAdoptionTestFixture.SeedSkill(
            workspace,
            "excluded-skill",
            "---\nname: excluded-skill\ndescription: Existing complete metadata\nlicense: MIT\n---\n" + body,
            body);
        var settings = exclusion == "directory"
            ? $$"""{"frontmatter":"scoped","removedDirectories":["{{skill.DirectoryPath}}"]}"""
            : "{\"frontmatter\":\"scoped\",\"removedCategories\":[\"skills\"]}";
        workspace.ReplaceText(".agents/open-forge.json", settings);
        var originalSkill = workspace.ReadBytes(skill.SkillPath);
        var originalReference = workspace.ReadBytes(skill.ReferencePath);
        var originalSkillsCatalogue = workspace.ReadBytes(".agents/skills/_skills.md");

        var result = await workspace.ExecuteAsync(workspace.Request(
            mode: UpdateMode.Apply,
            force: false,
            prune: false,
            automatic: true,
            allowsInteractiveConfirmation: false));

        UpdateWorkspaceAdoptionTestFixture.AssertStatus(result, CliSemanticStatus.Complete);
        Assert.Equal(payload.CurrentLoaderBytes, workspace.ReadBytes(UpdateIntegrationWorkspace.ManagedPath));
        var excludedUnit = exclusion == "directory" ? skill.DirectoryPath : ".agents/skills";
        Assert.DoesNotContain(result.Effects, effect =>
            string.Equals(effect.Path, excludedUnit, StringComparison.Ordinal)
            || effect.Path.StartsWith(excludedUnit + "/", StringComparison.Ordinal));
        Assert.Empty(result.Migrations);
        Assert.Equal(originalSkill, workspace.ReadBytes(skill.SkillPath));
        Assert.Equal(originalReference, workspace.ReadBytes(skill.ReferencePath));
        if (exclusion == "category")
        {
            Assert.Equal(originalSkillsCatalogue, workspace.ReadBytes(".agents/skills/_skills.md"));
        }
        else
        {
            Assert.Contains(result.Effects, effect => effect.Path == SkillsCataloguePath);
            Assert.Contains(skill.Slug, workspace.ReadText(".agents/skills/_skills.md"), StringComparison.Ordinal);
        }
        Assert.Equal(settings, workspace.ReadText(".agents/open-forge.json"));
        Assert.False(workspace.Exists(skill.ReferencesCataloguePath));
    }

    [Theory, InlineData("missing-native-frontmatter"), InlineData("partial-name-license-missing-description"), Trait("Boundary", "Selection")]
    public async Task ExcludedSkillWithIncompleteMetadataRemainsVisibleToStrictProjection(string metadataCase)
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-adoption-excluded-incomplete-skill");
        await UpdateWorkspaceAdoptionTestFixture.EstablishSyntheticEarlierPayloadAsync(workspace);
        const string body = "# Incomplete Excluded Skill\n\nPreserve its body and [reference](references/guide.md).\n";
        var slug = metadataCase == "missing-native-frontmatter"
            ? "excluded-frontmatterless"
            : "excluded-partial-metadata";
        var skillContents = metadataCase switch
        {
            "missing-native-frontmatter" => body,
            "partial-name-license-missing-description" =>
                "---\nname: excluded-partial-metadata\nlicense: MIT\n---\n" + body,
            _ => throw new ArgumentOutOfRangeException(nameof(metadataCase), metadataCase, "Unknown exclusion fixture case."),
        };
        var skill = UpdateWorkspaceAdoptionTestFixture.SeedSkill(workspace, slug, skillContents, body);
        var settings = $$"""{"frontmatter":"scoped","removedDirectories":["{{skill.DirectoryPath}}"]}""";
        workspace.ReplaceText(".agents/open-forge.json", settings);
        var originalSkill = workspace.ReadBytes(skill.SkillPath);
        var originalReference = workspace.ReadBytes(skill.ReferencePath);
        var originalSupport = workspace.ReadBytes(skill.SupportPath);
        var originalOverwrite = workspace.ReadBytes(skill.OverwritePath);
        var originalBinary = workspace.ReadBytes(skill.BinaryPath);
        var originalSettings = workspace.ReadBytes(".agents/open-forge.json");
        var originalOwnership = workspace.ReadBytes(UpdateIntegrationWorkspace.OwnershipPath);
        var originalSkillsCatalogue = workspace.ReadBytes(SkillsCataloguePath);
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(workspace.Request(
            mode: UpdateMode.Apply,
            force: false,
            prune: false,
            automatic: true,
            allowsInteractiveConfirmation: false));

        UpdateWorkspaceAdoptionTestFixture.AssertStatus(result, CliSemanticStatus.Blocked);
        Assert.Contains(result.Findings, finding => finding.Code == UpdateFindingCode.GeneratedRegionUnsafe);
        Assert.Empty(result.Effects);
        Assert.Empty(result.Migrations);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(originalSkill, workspace.ReadBytes(skill.SkillPath));
        var preservedSkillText = Encoding.UTF8.GetString(workspace.ReadBytes(skill.SkillPath));
        Assert.EndsWith(body, preservedSkillText, StringComparison.Ordinal);
        if (metadataCase == "partial-name-license-missing-description")
        {
            Assert.Contains("license: MIT\n", preservedSkillText, StringComparison.Ordinal);
            Assert.DoesNotContain("description:", preservedSkillText, StringComparison.Ordinal);
        }
        else
        {
            Assert.DoesNotContain("---", preservedSkillText, StringComparison.Ordinal);
        }

        Assert.Equal(originalReference, workspace.ReadBytes(skill.ReferencePath));
        Assert.Equal(originalSupport, workspace.ReadBytes(skill.SupportPath));
        Assert.Equal(originalOverwrite, workspace.ReadBytes(skill.OverwritePath));
        Assert.Equal(originalBinary, workspace.ReadBytes(skill.BinaryPath));
        Assert.Equal(originalSettings, workspace.ReadBytes(".agents/open-forge.json"));
        Assert.Equal(originalOwnership, workspace.ReadBytes(UpdateIntegrationWorkspace.OwnershipPath));
        Assert.Equal(originalSkillsCatalogue, workspace.ReadBytes(SkillsCataloguePath));
        Assert.False(workspace.Exists(skill.ReferencesCataloguePath));
        Assert.False(workspace.Exists($"{skill.DirectoryPath}/_skills.md"));
    }

    [Trait("Boundary", "Selection")]
    [Fact(DisplayName = "An exact file exclusion does not cover a sibling Skill or nested references")]
    public async Task RemovedFileExcludesOnlyTheExactNeighborPath()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-adoption-exact-file-exclusion");
        await UpdateWorkspaceAdoptionTestFixture.EstablishSyntheticEarlierPayloadAsync(workspace);
        const string body = "# Exact File Boundary Skill\n\nThe authored body is unchanged.\n";
        var skill = UpdateWorkspaceAdoptionTestFixture.SeedSkill(
            workspace,
            "exact-file-skill",
            body,
            body);
        const string neighborPath = ".agents/skills/exact-neighbor.md";
        const string neighborContents = "---\nopen-forge:\n  description: Excluded neighbor\n  tags: [Workspace]\n---\n\n# Neighbor\n";
        workspace.WriteText(neighborPath, neighborContents);
        const string settings = "{\"frontmatter\":\"scoped\",\"removedFiles\":[\".agents/skills/exact-neighbor.md\"]}";
        workspace.ReplaceText(".agents/open-forge.json", settings);

        var result = await workspace.ExecuteAsync(workspace.Request(
            mode: UpdateMode.Apply,
            force: false,
            prune: false,
            automatic: true,
            allowsInteractiveConfirmation: false));

        UpdateWorkspaceAdoptionTestFixture.AssertStatus(result, CliSemanticStatus.Complete);
        Assert.Contains(result.Effects, effect => effect.Path == skill.SkillPath);
        Assert.Contains(result.Effects, effect => effect.Path == skill.ReferencesCataloguePath);
        Assert.Contains(result.Effects, effect => effect.Path == ".agents/skills/_skills.md");
        Assert.DoesNotContain(result.Effects, effect => effect.Path == neighborPath);
        Assert.Contains(result.Migrations, migration =>
            migration.Path == skill.SkillPath
            && migration.Actions.Contains(WorkspaceAdoptionAction.MetadataCompleted));
        Assert.Equal(Encoding.UTF8.GetBytes(neighborContents), workspace.ReadBytes(neighborPath));
        Assert.EndsWith(
            body,
            Encoding.UTF8.GetString(workspace.ReadBytes(skill.SkillPath)),
            StringComparison.Ordinal);
        Assert.Equal(skill.ReferenceBytes, workspace.ReadBytes(skill.ReferencePath));
        Assert.Contains("guide.md", workspace.ReadText(skill.ReferencesCataloguePath), StringComparison.Ordinal);
    }

    [Trait("Boundary", "Ownership")]
    [Theory, InlineData("no-framework"), InlineData("malformed-lock"), InlineData("foreign-owner")]
    public async Task UnknownOrForeignFrameworkOwnershipBlocksAllAdoptionEffects(string boundary)
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-adoption-ownership-boundary");
        await UpdateWorkspaceAdoptionTestFixture.EstablishSyntheticEarlierPayloadAsync(workspace);
        const string body = "# Ownership Boundary Skill\n\nKeep this local.\n";
        var skill = UpdateWorkspaceAdoptionTestFixture.SeedSkill(
            workspace,
            "ownership-boundary",
            body,
            body);
        if (boundary == "no-framework")
        {
            UpdateWorkspaceAdoptionTestFixture.RemoveFrameworkOwnership(workspace);
        }
        else if (boundary == "malformed-lock")
        {
            workspace.SeedMalformedLifecycle();
        }
        else
        {
            UpdateWorkspaceAdoptionTestFixture.AddForeignOwner(workspace, skill.SkillPath);
        }

        var before = workspace.SnapshotHashes();
        var result = await workspace.ExecuteAsync(workspace.Request(
            mode: UpdateMode.Apply,
            force: false,
            prune: false,
            automatic: true,
            allowsInteractiveConfirmation: false));

        UpdateWorkspaceAdoptionTestFixture.AssertStatus(result, CliSemanticStatus.Blocked);
        Assert.NotEmpty(result.Findings);
        Assert.Empty(result.Effects);
        Assert.Empty(result.Migrations);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "A reparse-point Skill parent blocks Update adoption without effects")]
    public async Task ReparsePointSkillParentBlocksAdoptionWriteFree()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-adoption-reparse-skill");
        await UpdateWorkspaceAdoptionTestFixture.EstablishSyntheticEarlierPayloadAsync(workspace);
        const string targetDirectory = "skill-reparse-target";
        workspace.RegisterTestCleanupPath(targetDirectory);
        workspace.CreateDirectory(targetDirectory);
        const string targetSkillPath = "skill-reparse-target/SKILL.md";
        const string targetSkill = "---\nname: reparse-skill\nlicense: MIT\n---\n# Reparse Skill\n";
        workspace.WriteText(targetSkillPath, targetSkill);

        const string linkRelativePath = ".agents/skills/reparse-skill";
        var linkPath = Path.Combine(workspace.PhysicalPath, ".agents", "skills", "reparse-skill");
        var targetPath = Path.Combine(workspace.PhysicalPath, "skill-reparse-target");
        Directory.CreateSymbolicLink(linkPath, targetPath);
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(workspace.Request(
            mode: UpdateMode.Apply,
            force: false,
            prune: false,
            automatic: true,
            allowsInteractiveConfirmation: false));

        UpdateWorkspaceAdoptionTestFixture.AssertStatus(result, CliSemanticStatus.Blocked);
        Assert.Contains(result.Findings, finding => finding.Code == UpdateFindingCode.TargetUnsafe);
        Assert.Empty(result.Effects);
        Assert.Empty(result.Migrations);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(Encoding.UTF8.GetBytes(targetSkill), workspace.ReadBytes(targetSkillPath));
        Assert.True(Directory.Exists(linkPath));
        Assert.Equal(targetPath, new DirectoryInfo(linkPath).LinkTarget);
        Assert.Equal(linkRelativePath, Path.GetRelativePath(workspace.PhysicalPath, linkPath).Replace('\\', '/'));
    }

    [Trait("Boundary", "Parsing")]
    [Fact(DisplayName = "Malformed native Skill metadata blocks Update adoption before effects")]
    public async Task MalformedNativeSkillRemainsWriteFree()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-adoption-malformed-skill");
        await UpdateWorkspaceAdoptionTestFixture.EstablishSyntheticEarlierPayloadAsync(workspace);
        workspace.CreateDirectory(".agents/skills/malformed-skill");
        const string path = ".agents/skills/malformed-skill/SKILL.md";
        workspace.WriteText(path, "---\nname: [broken\n---\n# Malformed Skill\n");
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(workspace.Request(
            mode: UpdateMode.Apply,
            force: false,
            prune: false,
            automatic: true,
            allowsInteractiveConfirmation: false));

        UpdateWorkspaceAdoptionTestFixture.AssertStatus(result, CliSemanticStatus.Blocked);
        Assert.NotEmpty(result.Findings);
        Assert.Empty(result.Effects);
        Assert.Empty(result.Migrations);
        Assert.Equal(before, workspace.SnapshotHashes());
    }
}

internal static class UpdateWorkspaceAdoptionTestFixture
{
    private const string SyntheticEarlierPayloadSentence = "Earlier Task70 loader fixture sentence.";
    private const string ReferenceContents = "# Reference Guide\n\nKeep these exact reference bytes.\n";
    private const string SupportContents = "Support-file bytes stay as authored.\n";
    private const string OverwriteContents = "---\nopen-forge:\n  description: Local Skill override\n  tags: [Workspace]\n---\n\n# Local override\n";
    private static readonly UTF8Encoding StrictUtf8NoBom = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    internal sealed record SkillSeed(
        string Slug,
        string DirectoryPath,
        string SkillPath,
        string ReferencePath,
        string ReferencesCataloguePath,
        string SupportPath,
        string OverwritePath,
        string BinaryPath,
        string Body,
        byte[] OriginalSkillBytes,
        byte[] ReferenceBytes,
        byte[] SupportBytes,
        byte[] OverwriteBytes,
        byte[] BinaryBytes);

    internal sealed record SyntheticPayload(byte[] CurrentLoaderBytes, byte[] EarlierLoaderBytes);

    internal static async Task<SyntheticPayload> EstablishSyntheticEarlierPayloadAsync(
        UpdateIntegrationWorkspace workspace)
    {
        workspace.RegisterTestCleanupPath(".agents/skills");
        workspace.CreateDirectory(".agents/skills");
        var payload = CreateSyntheticEarlierPayload();
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        workspace.ReplaceText(
            UpdateIntegrationWorkspace.ManagedPath,
            StrictUtf8NoBom.GetString(payload.EarlierLoaderBytes));
        workspace.SeedPreviousInventoryIdentity();
        return payload;
    }

    internal static SkillSeed SeedSkill(
        UpdateIntegrationWorkspace workspace,
        string slug,
        string skillContents,
        string body)
    {
        var directory = $".agents/skills/{slug}";
        var references = $"{directory}/references";
        var support = $"{directory}/support";
        var assets = $"{directory}/assets";
        workspace.CreateDirectory(references);
        workspace.CreateDirectory(support);
        workspace.CreateDirectory(assets);

        var skillPath = $"{directory}/SKILL.md";
        var referencePath = $"{references}/guide.md";
        var referencesCataloguePath = $"{references}/_references.md";
        var supportPath = $"{support}/readme.txt";
        var overwritePath = SourceOverwritePath.ReadAdjacentPath(skillPath);
        var binaryPath = $"{assets}/fixture.bin";
        var binaryBytes = new byte[] { 0, 1, 0x7f, 0x80, 0xff };

        workspace.WriteText(skillPath, skillContents);
        workspace.WriteText(referencePath, ReferenceContents);
        workspace.WriteText(supportPath, SupportContents);
        workspace.WriteText(overwritePath, OverwriteContents);
        File.WriteAllBytes(
            Path.Combine(workspace.PhysicalPath, ".agents", "skills", slug, "assets", "fixture.bin"),
            binaryBytes);

        return new SkillSeed(
            slug,
            directory,
            skillPath,
            referencePath,
            referencesCataloguePath,
            supportPath,
            overwritePath,
            binaryPath,
            body,
            Encoding.UTF8.GetBytes(skillContents),
            Encoding.UTF8.GetBytes(ReferenceContents),
            Encoding.UTF8.GetBytes(SupportContents),
            Encoding.UTF8.GetBytes(OverwriteContents),
            binaryBytes);
    }

    internal static void AssertStatus(UpdateResult result, CliSemanticStatus expected)
    {
        var findings = string.Join("; ", result.Findings.Select(DescribeFinding));
        Assert.True(
            result.Status == expected,
            $"Expected Update status '{expected}', received '{result.Status}'. Findings: {findings}");
    }

    internal static void AssertStatus(InstallResult result, CliSemanticStatus expected)
    {
        var findings = string.Join("; ", result.Findings.Select(DescribeFinding));
        Assert.True(
            result.Status == expected,
            $"Expected Install status '{expected}', received '{result.Status}'. Findings: {findings}");
    }

    private static string DescribeFinding(UpdateFinding finding)
    {
        var target = finding.Target ?? "(none)";
        return $"code={finding.Code}; target={target}; cause={finding.Cause}";
    }

    private static string DescribeFinding(InstallFinding finding)
    {
        var target = finding.Subject ?? "(none)";
        return $"code={finding.Code}; target={target}; cause={finding.Cause}";
    }

    internal static UpdateMigration AssertMigration(
        UpdateResult result,
        string path,
        UpdateMigrationOutcome outcome)
    {
        var migration = Assert.Single(result.Migrations, item => item.Path == path);
        Assert.Equal(outcome, migration.Outcome);
        return migration;
    }

    internal static void AssertAppliedMigrationEffects(UpdateResult result)
    {
        foreach (var migration in result.Migrations)
        {
            Assert.Equal(UpdateMigrationOutcome.Applied, migration.Outcome);
            var effect = Assert.Single(result.Effects, item => item.Path == migration.Path);
            Assert.Equal(UpdatePhysicalEffectKind.File, effect.Kind);
            Assert.Equal(UpdatePhysicalEffectOutcome.Verified, effect.Outcome);
        }
    }

    internal static void RemoveFrameworkOwnership(UpdateIntegrationWorkspace workspace)
    {
        var ownership = JsonNode.Parse(workspace.ReadText(UpdateIntegrationWorkspace.OwnershipPath))!.AsObject();
        ownership["framework"] = null;
        workspace.ReplaceText(UpdateIntegrationWorkspace.OwnershipPath, ownership.ToJsonString());
    }

    internal static void AddForeignOwner(UpdateIntegrationWorkspace workspace, string path)
    {
        var ownership = JsonNode.Parse(workspace.ReadText(UpdateIntegrationWorkspace.OwnershipPath))!.AsObject();
        JsonArray extensions;
        if (ownership["extensions"] is JsonArray existingExtensions)
        {
            extensions = existingExtensions;
        }
        else
        {
            extensions = [];
            ownership["extensions"] = extensions;
        }

        var foreignOwner = (JsonNode)new JsonObject
        {
            ["id"] = "foreign-test-owner",
            ["paths"] = new JsonArray(JsonValue.Create(path)),
        };
        ((IList<JsonNode?>)extensions).Add(foreignOwner);
        workspace.ReplaceText(UpdateIntegrationWorkspace.OwnershipPath, ownership.ToJsonString());
    }

    private static SyntheticPayload CreateSyntheticEarlierPayload()
    {
        var payloadRead = EmbeddedFrameworkPayloadReader.Read();
        Assert.Equal(FrameworkPayloadReadState.Available, payloadRead.State);
        var currentPayload = Assert.IsType<FrameworkPayload>(payloadRead.Payload);
        var currentLoader = Assert.IsType<FrameworkPayloadAsset>(
            currentPayload.Find(FrameworkPayloadAsset.LoaderPath));
        // This is a synthetic older-payload fixture: clone the current payload and add one authored
        // loader sentence outside Entries. It is not an installation from an actual older binary.
        var currentText = StrictUtf8NoBom.GetString(currentLoader.Bytes.ToArray());
        var parsedCurrent = new MarkdownDocumentParser().Parse(currentText);
        Assert.Equal(MarkdownGeneratedRegionState.Complete, parsedCurrent.GeneratedRegion.State);
        var currentRegion = parsedCurrent.GeneratedRegion.RegionSpan
            ?? throw new InvalidOperationException("The current loader fixture requires a complete Entries region.");

        var insertion = $"\n{SyntheticEarlierPayloadSentence}\n\n";
        var earlierText = currentText.Insert(currentRegion.Start, insertion);
        var parsedEarlier = new MarkdownDocumentParser().Parse(earlierText);
        Assert.Equal(MarkdownGeneratedRegionState.Complete, parsedEarlier.GeneratedRegion.State);
        var earlierRegion = parsedEarlier.GeneratedRegion.RegionSpan
            ?? throw new InvalidOperationException("The earlier loader fixture requires a complete Entries region.");
        Assert.Contains(SyntheticEarlierPayloadSentence, earlierText[..earlierRegion.Start], StringComparison.Ordinal);
        Assert.Equal(
            currentText[currentRegion.Start..currentRegion.End],
            earlierText[earlierRegion.Start..earlierRegion.End]);
        Assert.Equal(currentText, earlierText.Remove(currentRegion.Start, insertion.Length));

        var earlierAssets = currentPayload.Assets
            .Select(asset => asset.Path == FrameworkPayloadAsset.LoaderPath
                ? FrameworkPayloadAsset.Create(asset.Path, StrictUtf8NoBom.GetBytes(earlierText))
                : asset)
            .ToArray();
        var earlierPayload = FrameworkPayload.Create(earlierAssets);
        Assert.NotEqual(currentPayload.InventoryFingerprint, earlierPayload.InventoryFingerprint);
        Assert.Equal(
            [FrameworkPayloadAsset.LoaderPath],
            currentPayload.Assets
                .Zip(earlierPayload.Assets)
                .Where(pair => pair.First.Sha256 != pair.Second.Sha256)
                .Select(pair => pair.First.Path));
        var earlierLoader = Assert.IsType<FrameworkPayloadAsset>(
            earlierPayload.Find(FrameworkPayloadAsset.LoaderPath));
        return new SyntheticPayload(currentLoader.Bytes.ToArray(), earlierLoader.Bytes.ToArray());
    }
}
