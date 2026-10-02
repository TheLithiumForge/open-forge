using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.Result;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Planning;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Presentation.Install;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

public sealed class InstallWorkspaceAdoptionIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task AlreadyMatchingCategoryBaseReceivesOwnershipAfterPreservation()
    {
        using var workspace = InstallOperationWorkspace.Create("install-exact-category-preservation");
        const string path = ".agents/guidance/_guidance.md";
        const string overwritePath = ".agents/guidance/_guidance.overwrite.md";
        var payload = Assert.IsType<FrameworkPayload>(EmbeddedFrameworkPayloadReader.Read().Payload);
        var asset = Assert.IsType<FrameworkPayloadAsset>(payload.Find(path));
        workspace.WriteText(path, System.Text.Encoding.UTF8.GetString(asset.Bytes.AsSpan()));
        var original = File.ReadAllBytes(workspace.Combine(path));
        var operation = CreateOperation(workspace);

        var applied = await operation.ExecuteAsync(workspace.Request(automatic: true), TestContext.Current.CancellationToken);

        Assert.True(applied.Status == CliSemanticStatus.Complete, DescribeResult(applied));
        Assert.Equal(original, File.ReadAllBytes(workspace.Combine(path)));
        Assert.DoesNotContain(applied.Facts.Effects, effect => effect.Path == path);
        Assert.True(workspace.Exists(overwritePath));
        var owner = Assert.IsType<FrameworkOwnership>((await WorkspaceOwnershipReader.ReadAsync(
            new PhysicalPathResolver(), workspace.Workspace, TestContext.Current.CancellationToken)).Document.Framework);
        Assert.Contains(path, owner.Paths);
        Assert.DoesNotContain(overwritePath, owner.Paths);
        var beforeRepeat = workspace.SnapshotHashes();
        var repeated = await operation.ExecuteAsync(workspace.Request(automatic: true), TestContext.Current.CancellationToken);
        Assert.True(repeated.Status == CliSemanticStatus.Complete, DescribeResult(repeated));
        Assert.Empty(repeated.Facts.Effects);
        Assert.Equal(beforeRepeat, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory, InlineData(false), InlineData(true), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task UnavailablePreservationDestinationBlocksWithoutChangingContent(bool foreignOwned)
    {
        using var workspace = InstallOperationWorkspace.Create("install-preservation-boundary");
        const string path = ".agents/guidance/_guidance.md";
        const string overwritePath = ".agents/guidance/_guidance.overwrite.md";
        workspace.WriteText(path, "# Local guidance\n\nKeep my context.\n");
        if (foreignOwned)
        {
            workspace.WriteText(overwritePath, "# Existing managed customization\n");
            workspace.WriteText(InstallOperationWorkspace.OwnershipPath,
                "{\"schemaVersion\":1,\"extensions\":[{\"id\":\"foreign\",\"paths\":[\"" + overwritePath + "\"]}]}\n");
        }
        else
        {
            workspace.WriteText(".agents/open-forge.json", "{\"removedFiles\":[\"" + overwritePath + "\"]}\n");
        }

        var before = workspace.SnapshotHashes();
        var result = await CreateOperation(workspace).ExecuteAsync(
            workspace.Request(automatic: true), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Empty(result.Facts.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Contains(result.Findings, finding => finding.Cause.Contains(overwritePath, StringComparison.Ordinal));
    }

    [Trait("Boundary", "OS")]
    [Theory, InlineData(false), InlineData(true), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task PlainCategoryCollisionPreservesContextAndExistingCompanion(bool hasCompanion)
    {
        using var workspace = InstallOperationWorkspace.Create("install-category-preservation");
        const string path = ".agents/guidance/_guidance.md";
        const string overwritePath = ".agents/guidance/_guidance.overwrite.md";
        const string authored = "# My notes\r\n\r\nKeep café and [guide](guide.md).\r\n";
        const string previousOverwrite = "# Local exception\r\n\r\nThis exception stays last.\r\n";
        workspace.WriteText(path, authored);
        workspace.WriteText(".agents/guidance/guide.md", "# Guide\n\nMy guidance.\n");
        if (hasCompanion)
        {
            workspace.WriteText(overwritePath, previousOverwrite);
        }

        var before = workspace.SnapshotHashes();
        var operation = CreateOperation(workspace);
        var preview = await operation.ExecuteAsync(
            workspace.Request(mode: InstallMode.DryRun, automatic: true), TestContext.Current.CancellationToken);
        Assert.True(preview.Status == CliSemanticStatus.Complete, DescribeResult(preview));
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Contains(preview.Facts.Migrations, migration => migration.Path == overwritePath
            && migration.Actions.Contains(WorkspaceAdoptionAction.ContentPreserved));

        var applied = await operation.ExecuteAsync(workspace.Request(automatic: true), TestContext.Current.CancellationToken);
        Assert.True(applied.Status == CliSemanticStatus.Complete, DescribeResult(applied));
        var preserved = File.ReadAllText(workspace.Combine(overwritePath));
        Assert.StartsWith(authored, preserved, StringComparison.Ordinal);
        if (hasCompanion)
        {
            Assert.EndsWith(previousOverwrite, preserved, StringComparison.Ordinal);
        }

        var installed = File.ReadAllText(workspace.Combine(path));
        Assert.Contains("## Axioms", installed, StringComparison.Ordinal);
        Assert.Contains("guide.md", installed, StringComparison.Ordinal);
        Assert.DoesNotContain("# My notes", installed, StringComparison.Ordinal);
        var ownership = await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(),
            workspace.Workspace, TestContext.Current.CancellationToken);
        Assert.Contains(path, ownership.Document.Framework!.Paths);
        Assert.DoesNotContain(overwritePath, ownership.Document.Framework.Paths);
        var after = workspace.SnapshotHashes();
        var repeated = await operation.ExecuteAsync(workspace.Request(automatic: true), TestContext.Current.CancellationToken);
        Assert.True(repeated.Status == CliSemanticStatus.Complete, DescribeResult(repeated));
        Assert.Empty(repeated.Facts.Effects);
        Assert.Equal(after, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory, InlineData("missing"), InlineData("partial"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task PlainInstallCompletesNativeSkillAndCreatesOnlyItsReferenceCatalogue(string fixture)
    {
        using var workspace = InstallOperationWorkspace.Create("install-native-skill-adoption");
        using var cleanup = new ActionOnDispose(() => DeleteOrdinaryFileIfPresent(
            workspace.Combine(".agents/skills/local-skill/references/_references.md")));
        const string skillPath = ".agents/skills/local-skill/SKILL.md";
        const string skillBody = "# Local Skill\n\nRead [Reference](references/guide.md).\n";
        const string referencesPath = ".agents/skills/local-skill/references/guide.md";
        var originalSkill = fixture == "missing"
            ? skillBody
            : "---\nname: local-skill\nlicense: MIT\ncustom: keep-me\n---\n" + skillBody;
        workspace.WriteText(skillPath, originalSkill);
        workspace.WriteText(referencesPath, "# Guide\n\nReference content.\n");
        var beforeDryRun = workspace.SnapshotHashes();
        var operation = CreateOperation(workspace);

        var dryRun = await operation.ExecuteAsync(
            workspace.Request(mode: InstallMode.DryRun, automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, dryRun.Status);
        Assert.Empty(dryRun.Findings);
        Assert.NotEmpty(dryRun.Facts.Effects);
        Assert.All(dryRun.Facts.Effects, effect => Assert.Equal(InstallEffectOutcome.Planned, effect.Outcome));
        Assert.Equal(beforeDryRun, workspace.SnapshotHashes());
        var plannedMigrations = await ReadMigrations(workspace);
        Assert.Contains(plannedMigrations, migration =>
            migration.Path == skillPath
            && migration.Actions.Contains(WorkspaceAdoptionAction.MetadataCompleted));
        Assert.Contains(plannedMigrations, migration =>
            migration.Path == ".agents/skills/local-skill/references/_references.md"
            && migration.Actions.Contains(WorkspaceAdoptionAction.EntrypointCreated));

        var applied = await operation.ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Empty(applied.Findings);
        Assert.Contains(applied.Facts.Effects, effect => effect.Path == skillPath);
        Assert.True(workspace.Exists(".agents/skills/local-skill/references/_references.md"));
        Assert.False(workspace.Exists(".agents/skills/_local-skill.md"));
        var skill = await workspace.ReadTextAsync(skillPath, TestContext.Current.CancellationToken);
        Assert.EndsWith(skillBody, skill, StringComparison.Ordinal);
        Assert.Contains(
            fixture == "missing" ? "name: \"local-skill\"" : "name: local-skill",
            skill,
            StringComparison.Ordinal);
        Assert.Contains("description:", skill, StringComparison.Ordinal);
        if (fixture == "partial")
        {
            Assert.Contains("license: MIT", skill, StringComparison.Ordinal);
            Assert.Contains("custom: keep-me", skill, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("open-forge:", skill, StringComparison.Ordinal);
        Assert.DoesNotContain("## Entries", skill, StringComparison.Ordinal);
        var references = await workspace.ReadTextAsync(
            ".agents/skills/local-skill/references/_references.md",
            TestContext.Current.CancellationToken);
        Assert.Contains("## Entries", references, StringComparison.Ordinal);
        Assert.Contains("guide.md", references, StringComparison.Ordinal);

        var owner = await WorkspaceOwnershipReader.ReadAsync(
            new PhysicalPathResolver(),
            workspace.Workspace,
            TestContext.Current.CancellationToken);
        var framework = Assert.IsType<FrameworkOwnership>(owner.Document.Framework);
        Assert.DoesNotContain(skillPath, framework.Paths);
        Assert.DoesNotContain(".agents/skills/local-skill/references/_references.md", framework.Paths);

        var beforeRepeat = workspace.SnapshotHashes();
        var repeated = await operation.ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
        Assert.Empty(repeated.Findings);
        Assert.Empty(repeated.Facts.Effects);
        Assert.Equal(beforeRepeat, workspace.SnapshotHashes());
        Assert.Empty(await ReadMigrations(workspace));
    }

    [Trait("Boundary", "OS")]
    [Theory, InlineData("missing"), InlineData("partial"), InlineData("valid"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task ManagedInstallAdoptsNativeSkillWithoutForceAndProjectsItsRoute(string fixture)
    {
        using var workspace = InstallOperationWorkspace.Create("install-managed-skill-adoption");
        using var cleanup = new ActionOnDispose(() =>
        {
            DeleteOrdinaryFileIfPresent(workspace.Combine(".agents/skills/local-skill/references/_references.md"));
        });
        // Register the empty target directory with TemporaryWorkspace before Install;
        // the Skill and reference are still added only after the first Install.
        workspace.CreateDirectory(".agents/skills");
        var operation = CreateOperation(workspace);
        var first = await operation.ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, first.Status);

        const string skillPath = ".agents/skills/local-skill/SKILL.md";
        const string skillBody = "# Local Skill\n\nRead [Reference](references/guide.md).\n";
        var skill = fixture switch
        {
            "missing" => skillBody,
            "partial" => "---\nname: local-skill\nlicense: MIT\ncustom: keep-me\n---\n" + skillBody,
            "valid" => "---\nname: local-skill\ndescription: A locally authored Skill.\nlicense: MIT\n---\n" + skillBody,
            _ => throw new ArgumentOutOfRangeException(nameof(fixture)),
        };
        const string referencesPath = ".agents/skills/local-skill/references/guide.md";
        workspace.CreateDirectory(".agents/skills/local-skill/references");
        workspace.WriteText(skillPath, skill);
        workspace.WriteText(referencesPath, "# Guide\n\nReference content.\n");
        var originalSkillBytes = File.ReadAllBytes(workspace.Combine(skillPath));
        var beforeDryRun = workspace.SnapshotHashes();

        var dryRun = await operation.ExecuteAsync(
            workspace.Request(mode: InstallMode.DryRun, automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, dryRun.Status);
        Assert.Empty(dryRun.Findings);
        Assert.Contains(dryRun.Facts.Effects, effect =>
            effect.Path == ".agents/skills/_skills.md"
            && effect.Kind == InstallEffectKind.GeneratedRegion
            && effect.Action == InstallEffectAction.Replace
            && effect.Outcome == InstallEffectOutcome.Planned);
        Assert.Equal(beforeDryRun, workspace.SnapshotHashes());
        Assert.Contains(await ReadMigrations(workspace), migration =>
            migration.Path == ".agents/skills/_skills.md"
            && migration.Actions.Contains(WorkspaceAdoptionAction.NavigationUpdated));
        if (fixture != "valid")
        {
            Assert.Contains(await ReadMigrations(workspace), migration =>
                migration.Path == skillPath
                && migration.Actions.Contains(WorkspaceAdoptionAction.MetadataCompleted));
        }

        var applied = await operation.ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Empty(applied.Findings);
        var summary = Assert.IsType<InstallOperationSummary>(applied.Summary);
        Assert.Equal(InstallManagementState.ManagedAdoption, summary.ManagementState);
        var appliedSkillBytes = File.ReadAllBytes(workspace.Combine(skillPath));
        if (fixture == "valid")
        {
            Assert.Equal(originalSkillBytes, appliedSkillBytes);
        }

        var appliedSkill = await workspace.ReadTextAsync(skillPath, TestContext.Current.CancellationToken);
        Assert.EndsWith(skillBody, appliedSkill, StringComparison.Ordinal);
        Assert.Contains(
            fixture == "missing" ? "name: \"local-skill\"" : "name: local-skill",
            appliedSkill,
            StringComparison.Ordinal);
        Assert.Contains("description:", appliedSkill, StringComparison.Ordinal);
        if (fixture != "missing")
        {
            Assert.Contains("license: MIT", appliedSkill, StringComparison.Ordinal);
        }

        if (fixture == "partial")
        {
            Assert.Contains("custom: keep-me", appliedSkill, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("open-forge:", appliedSkill, StringComparison.Ordinal);
        Assert.DoesNotContain("## Entries", appliedSkill, StringComparison.Ordinal);
        Assert.True(workspace.Exists(".agents/skills/local-skill/references/_references.md"));
        Assert.False(workspace.Exists(".agents/skills/_local-skill.md"));
        var framework = Assert.IsType<FrameworkOwnership>((await WorkspaceOwnershipReader.ReadAsync(
            new PhysicalPathResolver(),
            workspace.Workspace,
            TestContext.Current.CancellationToken)).Document.Framework);
        Assert.Contains(".agents/skills/_skills.md", framework.Paths);
        Assert.DoesNotContain(skillPath, framework.Paths);
        Assert.DoesNotContain(".agents/skills/local-skill/references/_references.md", framework.Paths);

        var beforeRepeat = workspace.SnapshotHashes();
        var repeated = await operation.ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
        Assert.Empty(repeated.Findings);
        Assert.Empty(repeated.Facts.Effects);
        Assert.Equal(beforeRepeat, workspace.SnapshotHashes());
        Assert.Empty(await ReadMigrations(workspace));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Plain Markdown remains unchanged while Install creates the required nested route entrypoint"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task NestedMarkdownKeepsItsMetadataAndInvalidUtf8OverwriteBytes()
    {
        using var workspace = InstallOperationWorkspace.Create("install-nested-markdown-adoption");
        using var cleanup = new ActionOnDispose(() =>
        {
            DeleteOrdinaryFileIfPresent(workspace.Combine(".agents/guidance/deep/_deep.md"));
            DeleteOrdinaryFileIfPresent(workspace.Combine(".agents/guidance/deep/local-note.overwrite.md"));
        });
        const string notePath = ".agents/guidance/deep/local-note.md";
        const string note = "# Local Note\n\nNo Open Forge metadata is authored here.\n";
        const string overwritePath = ".agents/guidance/deep/local-note.overwrite.md";
        byte[] overwriteBytes = [0xFF, 0xFE, 0x00, 0xC3, 0x28];
        workspace.WriteText(notePath, note);
        File.WriteAllBytes(workspace.Combine(overwritePath), overwriteBytes);

        var result = await CreateOperation(workspace).ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);

        Assert.True(
            result.Status == CliSemanticStatus.Complete,
            DescribeResult(result));
        Assert.Empty(result.Findings);
        Assert.Equal(note, await workspace.ReadTextAsync(notePath, TestContext.Current.CancellationToken));
        Assert.Equal(overwriteBytes, File.ReadAllBytes(workspace.Combine(overwritePath)));
        Assert.True(workspace.Exists(".agents/guidance/deep/_deep.md"));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Install reuses one recognized index entrypoint and preserves its authored body"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task UniqueIndexEntrypointIsReusedWithoutReplacingItsBody()
    {
        using var workspace = InstallOperationWorkspace.Create("install-index-host-reuse");
        const string hostPath = ".agents/guidance/index.md";
        const string body = "# Authored Guidance\n\nKeep this local body exactly.\n";
        workspace.WriteText(hostPath, body);
        var operation = CreateOperation(workspace);

        var result = await operation.ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);

        Assert.True(
            result.Status == CliSemanticStatus.Complete,
            DescribeResult(result));
        Assert.Empty(result.Findings);
        Assert.False(workspace.Exists(".agents/guidance/_guidance.md"));
        var adopted = await workspace.ReadTextAsync(hostPath, TestContext.Current.CancellationToken);
        Assert.Contains("Keep this local body exactly.", adopted, StringComparison.Ordinal);
        Assert.StartsWith(body, adopted, StringComparison.Ordinal);
        Assert.Contains(result.Facts.Effects, effect => effect.Path == hostPath);

        var beforeRepeat = workspace.SnapshotHashes();
        var repeated = await operation.ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
        Assert.Empty(repeated.Facts.Effects);
        Assert.Equal(beforeRepeat, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Malformed native Skill metadata blocks adoption without writes"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task MalformedSkillMetadataBlocksWriteFree()
    {
        using var workspace = InstallOperationWorkspace.Create("install-malformed-skill-adoption");
        const string path = ".agents/skills/local-skill/SKILL.md";
        workspace.WriteText(path, "---\nname: [broken\n---\n# Local Skill\n");
        var before = workspace.SnapshotHashes();

        var result = await CreateOperation(workspace).ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);

        Assert.True(
            result.Status == CliSemanticStatus.Blocked,
            DescribeResult(result));
        Assert.Empty(result.Facts.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(workspace.Exists(InstallOperationWorkspace.OwnershipPath));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "A competing Extension owner blocks required Skill metadata adoption"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task ForeignOwnedIncompleteSkillIsPreserved()
    {
        using var workspace = InstallOperationWorkspace.Create("install-foreign-owned-skill");
        const string skillPath = ".agents/skills/local-skill/SKILL.md";
        workspace.WriteText(skillPath, "# Local Skill\n\nBody remains local.\n");
        workspace.WriteText(
            InstallOperationWorkspace.OwnershipPath,
            "{\"schemaVersion\":1,\"extensions\":[{\"id\":\"foreign\",\"paths\":[\""
                + skillPath
                + "\"]}]}\n");
        var before = workspace.SnapshotHashes();

        var result = await CreateOperation(workspace).ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Empty(result.Facts.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal("# Local Skill\n\nBody remains local.\n",
            await workspace.ReadTextAsync(skillPath, TestContext.Current.CancellationToken));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Install keeps a removed Skill root absent"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task RemovedRootStaysAbsent()
    {
        using var workspace = InstallOperationWorkspace.Create("install-removed-skill-root-adoption");
        workspace.WriteText(".agents/open-forge.json", "{\"removedCategories\":[\"skills\"]}");

        var result = await CreateOperation(workspace).ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.False(Directory.Exists(workspace.Combine(".agents/skills")));
        Assert.False(workspace.Exists(".agents/skills/_skills.md"));
    }

    [Trait("Boundary", "OS")]
    [Theory, InlineData("valid"), InlineData("partial"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task RemovedDirectoryPreventsSkillAndReferenceAdoption(string fixture)
    {
        using var workspace = InstallOperationWorkspace.Create("install-removed-directory-adoption");
        using var cleanup = new ActionOnDispose(() =>
        {
            DeleteOrdinaryFileIfPresent(workspace.Combine(
                ".agents/skills/local-skill/references/_references.md"));
        });
        workspace.WriteText(
            ".agents/open-forge.json",
            "{\"removedDirectories\":[\".agents/skills/local-skill\"]}");
        const string skillPath = ".agents/skills/local-skill/SKILL.md";
        const string referencePath = ".agents/skills/local-skill/references/guide.md";
        var originalSkill = fixture == "valid"
            ? "---\nname: local-skill\ndescription: Keep this authored description.\nlicense: MIT\n---\n# Local Skill\n\nBody stays byte-for-byte.\n"
            : "---\nname: local-skill\nlicense: MIT\n---\n# Local Skill\n\nBody stays byte-for-byte.\n";
        workspace.WriteText(skillPath, originalSkill);
        workspace.WriteText(referencePath, "# Guide\n\nReference stays byte-for-byte.\n");
        var originalSkillBytes = File.ReadAllBytes(workspace.Combine(skillPath));
        var originalReferenceBytes = File.ReadAllBytes(workspace.Combine(referencePath));

        var result = await CreateOperation(workspace).ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(originalSkillBytes, File.ReadAllBytes(workspace.Combine(skillPath)));
        Assert.Equal(originalReferenceBytes, File.ReadAllBytes(workspace.Combine(referencePath)));
        Assert.False(workspace.Exists(
            ".agents/skills/local-skill/references/_references.md"));
        if (result.Status == CliSemanticStatus.Blocked)
        {
            Assert.Equal("partial", fixture);
            Assert.Empty(result.Facts.Effects);
        }
        else
        {
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Empty(await ReadMigrations(workspace));
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "An exact removed file does not remove sources beneath its path prefix"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task RemovedFileDoesNotExpandToItsPathPrefix()
    {
        using var workspace = InstallOperationWorkspace.Create("install-removed-file-exact-adoption");
        const string sourcePath = ".agents/guidance/deep/note.md/child.md";
        const string entrypointPath = ".agents/guidance/deep/note.md/_note.md.md";
        using var cleanup = new ActionOnDispose(() =>
        {
            DeleteOrdinaryFileIfPresent(workspace.Combine(entrypointPath));
            DeleteOrdinaryFileIfPresent(workspace.Combine(".agents/guidance/deep/_deep.md"));
        });
        workspace.WriteText(
            ".agents/open-forge.json",
            "{\"removedFiles\":[\".agents/guidance/deep/note.md\"]}");
        const string source = "# Child Note\n\nThe exact file exclusion must not cover this route.\n";
        workspace.WriteText(sourcePath, source);
        var originalBytes = File.ReadAllBytes(workspace.Combine(sourcePath));

        var result = await CreateOperation(workspace).ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(originalBytes, File.ReadAllBytes(workspace.Combine(sourcePath)));
        Assert.True(workspace.Exists(entrypointPath));
        Assert.Contains(result.Facts.Migrations, migration => migration.Path == entrypointPath
            && migration.Actions.Contains(WorkspaceAdoptionAction.EntrypointCreated));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Removed categories keep an existing Skill outside adoption"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task RemovedCategoryPreservesExistingSkillWithoutAdoption()
    {
        using var workspace = InstallOperationWorkspace.Create("install-removed-category-skill-adoption");
        using var cleanup = new ActionOnDispose(() =>
            DeleteOrdinaryFileIfPresent(workspace.Combine(
                ".agents/skills/local-skill/references/_references.md")));
        workspace.WriteText(
            ".agents/open-forge.json",
            "{\"removedCategories\":[\"skills\"]}");
        const string skillPath = ".agents/skills/local-skill/SKILL.md";
        const string referencePath = ".agents/skills/local-skill/references/guide.md";
        const string skill = "# Local Skill\n\nCategory exclusion preserves this body.\n";
        const string reference = "# Guide\n\nReference stays local.\n";
        workspace.WriteText(skillPath, skill);
        workspace.WriteText(referencePath, reference);
        var originalSkillBytes = File.ReadAllBytes(workspace.Combine(skillPath));
        var originalReferenceBytes = File.ReadAllBytes(workspace.Combine(referencePath));

        var result = await CreateOperation(workspace).ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(originalSkillBytes, File.ReadAllBytes(workspace.Combine(skillPath)));
        Assert.Equal(originalReferenceBytes, File.ReadAllBytes(workspace.Combine(referencePath)));
        Assert.False(workspace.Exists(
            ".agents/skills/local-skill/references/_references.md"));
        if (result.Status == CliSemanticStatus.Complete)
        {
            Assert.Empty(await ReadMigrations(workspace));
        }
        else
        {
            Assert.Equal(CliSemanticStatus.Blocked, result.Status);
            Assert.Empty(result.Facts.Effects);
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Install still blocks managed authored payload divergence even with force"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task ManagedAuthoredPayloadDivergenceRemainsBlocked()
    {
        using var workspace = InstallOperationWorkspace.Create("install-managed-authored-divergence");
        var operation = CreateOperation(workspace);
        var installed = await operation.ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, installed.Status);
        workspace.ReplaceInstalledText(".agents/loader.md", "# User-authored divergence\n");
        var before = workspace.SnapshotHashes();

        var result = await operation.ExecuteAsync(
            workspace.Request(force: true, automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == InstallFindingCode.ManagedDivergence);
        Assert.Empty(result.Facts.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Install preserves a CRLF managed Skills host outside its Entries region"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task CrLfHostOutsideEntriesSurvivesDryRunApplyAndRepeat()
    {
        using var workspace = InstallOperationWorkspace.Create("install-crlf-generated-host-adoption");
        const string hostPath = ".agents/skills/_skills.md";
        const string skillPath = ".agents/skills/local-skill/SKILL.md";
        const string referencePath = ".agents/skills/local-skill/references/guide.md";
        const string referencesCataloguePath = ".agents/skills/local-skill/references/_references.md";
        using var cleanup = new ActionOnDispose(() => DeleteOrdinaryFileIfPresent(
            workspace.Combine(referencesCataloguePath)));

        // Register the empty route subtree before the initial Install; the local
        // native Skill and its reference are added after the Framework is installed.
        workspace.CreateDirectory(".agents/skills");
        var operation = CreateOperation(workspace);
        var firstInstall = await operation.ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, firstInstall.Status);

        var installedHostText = await workspace.ReadTextAsync(
            hostPath,
            TestContext.Current.CancellationToken);
        Assert.DoesNotContain("\r", installedHostText, StringComparison.Ordinal);
        var crlfHostText = installedHostText.Replace("\n", "\r\n", StringComparison.Ordinal);
        Assert.Equal(installedHostText, crlfHostText.Replace("\r\n", "\n", StringComparison.Ordinal));
        workspace.ReplaceInstalledText(hostPath, crlfHostText);
        var originalHostBytes = File.ReadAllBytes(workspace.Combine(hostPath));
        var originalHostText = System.Text.Encoding.UTF8.GetString(originalHostBytes);
        var originalEntriesParts = ReadGeneratedEntriesParts(originalHostText);
        Assert.Contains("\r\n", originalEntriesParts.Section, StringComparison.Ordinal);

        const string skillText = "---\nname: local-skill\ndescription: A complete local Skill.\nlicense: MIT\n---\n# Local Skill\n\nRead [Guide](references/guide.md).\n";
        const string referenceText = "# Guide\n\nLocal supporting reference stays intact.\n";
        workspace.CreateDirectory(".agents/skills/local-skill/references");
        workspace.WriteText(skillPath, skillText);
        workspace.WriteText(referencePath, referenceText);
        var originalSkillBytes = File.ReadAllBytes(workspace.Combine(skillPath));
        var originalReferenceBytes = File.ReadAllBytes(workspace.Combine(referencePath));
        var beforeDryRun = workspace.SnapshotHashes();

        var dryRun = await operation.ExecuteAsync(
            workspace.Request(mode: InstallMode.DryRun, automatic: true),
            TestContext.Current.CancellationToken);

        Assert.True(
            dryRun.Status == CliSemanticStatus.Complete,
            DescribeResult(dryRun));
        Assert.Empty(dryRun.Findings);
        Assert.Contains(dryRun.Facts.Effects, effect =>
            effect.Path == hostPath
            && effect.Kind == InstallEffectKind.GeneratedRegion
            && effect.Outcome == InstallEffectOutcome.Planned);
        Assert.Contains(dryRun.Facts.Migrations, migration =>
            migration.Path == hostPath
            && migration.Actions.Contains(WorkspaceAdoptionAction.NavigationUpdated));
        Assert.Equal(beforeDryRun, workspace.SnapshotHashes());
        Assert.Equal(originalHostBytes, File.ReadAllBytes(workspace.Combine(hostPath)));

        var applied = await operation.ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);

        Assert.True(
            applied.Status == CliSemanticStatus.Complete,
            DescribeResult(applied));
        Assert.Empty(applied.Findings);
        var appliedHostText = await workspace.ReadTextAsync(
            hostPath,
            TestContext.Current.CancellationToken);
        var appliedEntriesParts = ReadGeneratedEntriesParts(appliedHostText);
        Assert.Equal(originalEntriesParts.Prefix, appliedEntriesParts.Prefix);
        Assert.Equal(originalEntriesParts.Suffix, appliedEntriesParts.Suffix);
        Assert.Contains("## Entries", appliedEntriesParts.Section, StringComparison.Ordinal);
        Assert.Contains("\r\n", appliedEntriesParts.Section, StringComparison.Ordinal);
        Assert.Contains("local-skill/SKILL.md", appliedEntriesParts.Section, StringComparison.Ordinal);
        Assert.Equal(originalSkillBytes, File.ReadAllBytes(workspace.Combine(skillPath)));
        Assert.Equal(originalReferenceBytes, File.ReadAllBytes(workspace.Combine(referencePath)));
        Assert.True(workspace.Exists(referencesCataloguePath));
        Assert.False(workspace.Exists(".agents/skills/_local-skill.md"));

        var owner = await WorkspaceOwnershipReader.ReadAsync(
            new PhysicalPathResolver(),
            workspace.Workspace,
            TestContext.Current.CancellationToken);
        var framework = Assert.IsType<FrameworkOwnership>(owner.Document.Framework);
        Assert.Contains(hostPath, framework.Paths);
        Assert.DoesNotContain(skillPath, framework.Paths);
        Assert.DoesNotContain(referencesCataloguePath, framework.Paths);
        Assert.Contains(framework.Regions, region =>
            region.Path == referencesCataloguePath && region.Region == "entries");

        var beforeRepeat = workspace.SnapshotHashes();
        var repeated = await operation.ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);

        Assert.True(
            repeated.Status == CliSemanticStatus.Complete,
            DescribeResult(repeated));
        Assert.Equal(
            InstallManagementClassification.TrustedExact,
            repeated.Facts.Classification);
        Assert.Empty(repeated.Facts.Effects);
        Assert.Empty(repeated.Facts.Migrations);
        Assert.Equal(beforeRepeat, workspace.SnapshotHashes());
        Assert.Equal(appliedHostText, await workspace.ReadTextAsync(
            hostPath,
            TestContext.Current.CancellationToken));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Managed loader divergence blocks Skill adoption without migration output"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task ManagedLoaderDivergenceBlocksPartialSkillWithoutTextOrJsonMigrations()
    {
        using var workspace = InstallOperationWorkspace.Create("install-loader-divergence-skill-adoption");
        using var cleanup = new ActionOnDispose(() => DeleteOrdinaryFileIfPresent(
            workspace.Combine(".agents/skills/local-skill/references/_references.md")));
        // Register this subtree before the initial Install so the test workspace
        // owns the local Skill added after installation.
        workspace.CreateDirectory(".agents/skills");
        var operation = CreateOperation(workspace);
        var installed = await operation.ExecuteAsync(
            workspace.Request(automatic: true),
            TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, installed.Status);

        const string loaderPath = ".agents/loader.md";
        var loaderText = await workspace.ReadTextAsync(loaderPath, TestContext.Current.CancellationToken);
        var loaderEntriesParts = ReadGeneratedEntriesParts(loaderText);
        var loaderWithAuthoredChange = string.Concat(
            loaderEntriesParts.Prefix,
            "\nAuthored loader paragraph changed outside the managed Entries section.\n\n",
            loaderEntriesParts.Section,
            loaderEntriesParts.Suffix);
        var changedLoaderParts = ReadGeneratedEntriesParts(loaderWithAuthoredChange);
        Assert.Equal(loaderEntriesParts.Section, changedLoaderParts.Section);
        workspace.ReplaceInstalledText(loaderPath, loaderWithAuthoredChange);

        const string skillPath = ".agents/skills/local-skill/SKILL.md";
        const string referencePath = ".agents/skills/local-skill/references/guide.md";
        workspace.CreateDirectory(".agents/skills/local-skill");
        workspace.CreateDirectory(".agents/skills/local-skill/references");
        const string partialSkillText = "---\nname: local-skill\nlicense: MIT\ncustom: preserve\n---\n# Local Skill\n\nRead [Guide](references/guide.md).\n";
        const string supportingReferenceText = "# Guide\n\nThe supporting source remains local.\n";
        workspace.WriteText(
            skillPath,
            partialSkillText);
        workspace.WriteText(referencePath, supportingReferenceText);
        var partialSkillBytes = File.ReadAllBytes(workspace.Combine(skillPath));
        var supportingReferenceBytes = File.ReadAllBytes(workspace.Combine(referencePath));
        var before = workspace.SnapshotHashes();

        var result = await operation.ExecuteAsync(
            workspace.Request(mode: InstallMode.DryRun, automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == InstallFindingCode.ManagedDivergence);
        Assert.Empty(result.Facts.Effects);
        Assert.Empty(result.Facts.Migrations);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(partialSkillBytes, File.ReadAllBytes(workspace.Combine(skillPath)));
        Assert.Equal(supportingReferenceBytes, File.ReadAllBytes(workspace.Combine(referencePath)));

        var text = CommandOutputRenderers<InstallResult>.Render(
            new CliPresentationRequest<InstallResult>(
                result,
                new(CliFormat.Text, CliDetail.Full, null)),
            InstallPresentation.Rendering);
        Assert.DoesNotContain("Planned migration", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(skillPath, text, StringComparison.Ordinal);

        var jsonText = CommandOutputRenderers<InstallResult>.Render(
            new CliPresentationRequest<InstallResult>(
                result,
                new(CliFormat.Json, CliDetail.Full, null)),
            InstallPresentation.Rendering);
        using var json = JsonDocument.Parse(jsonText);
        Assert.False(json.RootElement.GetProperty("data").TryGetProperty("migrations", out _));
    }

    private static (string Prefix, string Section, string Suffix) ReadGeneratedEntriesParts(string document)
    {
        var facts = new MarkdownDocumentParser().Parse(document);
        Assert.Equal(MarkdownGeneratedRegionState.Complete, facts.GeneratedRegion.State);
        var span = Assert.IsType<MarkdownTextSpan>(facts.GeneratedRegion.RegionSpan);
        return (
            document[..span.Start],
            document[span.Start..span.End],
            document[span.End..]);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "An existing native Skill at an exact payload path stays user owned"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task ExistingExactPayloadSkillIsPreservedWithoutForce()
    {
        using var workspace = InstallOperationWorkspace.Create("install-occupied-payload-skill-adoption");
        const string occupiedPath = ".agents/skills/open-forge-cli/SKILL.md";
        const string skillPath = ".agents/skills/local-skill/SKILL.md";
        const string authoredOccupant = "---\nname: open-forge-cli\ndescription: Authored existing occupant\nlicense: MIT\n---\n# Authored Existing Skill\n\nThis exact payload target belongs to the workspace.\n";
        var payloadRead = EmbeddedFrameworkPayloadReader.Read();
        var payload = Assert.IsType<FrameworkPayload>(payloadRead.Payload);
        var payloadAsset = payload.Find(occupiedPath)
            ?? throw new InvalidOperationException("The selected native Skill payload asset is unavailable.");
        Assert.Equal(occupiedPath, payloadAsset.Path);
        Assert.Contains(occupiedPath, InstallOperationWorkspace.EmbeddedPayloadPaths);
        Assert.True(SourceFormClassifier.TryClassify(payloadAsset.Path, out var payloadForm));
        Assert.Equal(SourceDocumentForm.Skill, payloadForm);
        Assert.False(SourceFormClassifier.IsEntrypoint(payloadForm));
        Assert.False(SourceFormClassifier.IsCompatibilityEntrypoint(payloadForm));

        var authoredOccupantBytes = System.Text.Encoding.UTF8.GetBytes(authoredOccupant);
        const string partialSkillText = "---\nname: local-skill\nlicense: MIT\ncustom: preserve\n---\n# Local Skill\n\nBody remains local.\n";
        var partialSkillBytes = System.Text.Encoding.UTF8.GetBytes(partialSkillText);
        workspace.WriteText(occupiedPath, authoredOccupant);
        workspace.CreateDirectory(".agents/skills/local-skill");
        workspace.WriteText(skillPath, partialSkillText);
        var before = workspace.SnapshotHashes();

        var result = await CreateOperation(workspace).ExecuteAsync(
            workspace.Request(mode: InstallMode.DryRun, automatic: true),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Findings);
        Assert.DoesNotContain(result.Facts.Effects, effect => effect.Path == occupiedPath);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(authoredOccupantBytes, File.ReadAllBytes(workspace.Combine(occupiedPath)));
        Assert.Equal(partialSkillBytes, File.ReadAllBytes(workspace.Combine(skillPath)));
        Assert.Equal(authoredOccupant, await workspace.ReadTextAsync(
            occupiedPath,
            TestContext.Current.CancellationToken));
        Assert.True(workspace.Exists(skillPath));
        var applied = await CreateOperation(workspace).ExecuteAsync(
            workspace.Request(automatic: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal(authoredOccupantBytes, File.ReadAllBytes(workspace.Combine(occupiedPath)));
        Assert.DoesNotContain("## Entries", File.ReadAllText(workspace.Combine(occupiedPath)), StringComparison.Ordinal);
        Assert.Contains("description:", File.ReadAllText(workspace.Combine(skillPath)), StringComparison.Ordinal);
        var after = workspace.SnapshotHashes();
        var repeated = await CreateOperation(workspace).ExecuteAsync(
            workspace.Request(automatic: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
        Assert.Empty(repeated.Facts.Effects);
        Assert.Equal(after, workspace.SnapshotHashes());

    }

    private static InstallOperation CreateOperation(InstallOperationWorkspace workspace)
        => InstallOperationFactory.Create(
            InstallInteractionTestSupport.Confirmation(),
            workspace.LockStoreRoot);

    private static string DescribeResult(InstallResult result)
    {
        var findings = result.Findings
            .Select(finding => $"{finding.Code}: {finding.Cause}; subject: {finding.Subject ?? "<none>"}")
            .ToArray();
        return findings.Length == 0
            ? $"Status: {result.Status}; no findings."
            : $"Status: {result.Status}{Environment.NewLine}{string.Join(Environment.NewLine, findings)}";
    }

    private static void DeleteOrdinaryFileIfPresent(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
        {
            throw new InvalidOperationException($"Refusing to remove a non-ordinary test file: {path}");
        }

        File.Delete(path);
    }

    private static void DeleteEmptyOrdinaryDirectoryIfPresent(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device))
            != FileAttributes.Directory)
        {
            throw new InvalidOperationException($"Refusing to remove a non-ordinary test directory: {path}");
        }

        Directory.Delete(path, recursive: false);
    }

    private sealed class ActionOnDispose : IDisposable
    {
        private readonly Action _action;

        internal ActionOnDispose(Action action)
        {
            _action = action;
        }

        public void Dispose() => _action();
    }

    private static async ValueTask<IReadOnlyList<InstallMigrationPlan>> ReadMigrations(
        InstallOperationWorkspace workspace)
    {
        var payloadRead = EmbeddedFrameworkPayloadReader.Read();
        var payload = Assert.IsType<FrameworkPayload>(payloadRead.Payload);
        var build = await new InstallIntendedStateBuilder(new PhysicalPathResolver()).BuildAsync(
            workspace.Request(mode: InstallMode.DryRun, automatic: true),
            payload,
            TestContext.Current.CancellationToken);
        Assert.Equal(InstallIntendedStateBuildState.Complete, build.State);
        return Assert.IsType<InstallIntendedState>(build.IntendedState).Migrations;
    }
}
