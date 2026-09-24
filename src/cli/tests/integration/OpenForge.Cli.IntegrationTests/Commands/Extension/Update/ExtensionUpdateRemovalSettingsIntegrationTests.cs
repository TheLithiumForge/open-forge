using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Update;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Interaction;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Update;

public sealed class ExtensionUpdateRemovalSettingsIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Update blocks an explicitly excluded target and bulk update skips it"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task ExcludedIdentityBlocksExplicitUpdateAndSkipsBulkUpdate()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-update-excluded-identity");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-update-excluded-identity-source");
        source.AddPackage("alpha", [], (".agents/alpha.md", Document("Alpha v1")));
        source.AddPackage("beta", [], (".agents/beta.md", Document("Beta v1")));
        await InstallAllAsync(workspace, source);
        await File.WriteAllTextAsync(
            workspace.Combine(".agents/open-forge.json"),
            "{\"schemaVersion\":1,\"removedExtensions\":[\"alpha\"]}",
            TestContext.Current.CancellationToken);
        source.ReplacePayload("alpha", ".agents/alpha.md", Document("Alpha v2"));
        source.ReplacePayload("beta", ".agents/beta.md", Document("Beta v2"));
        var beforeExplicit = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var explicitRun = await workspace.RunAsync(
        [
            "extension", "update", "alpha",
            "--source", source.Path,
            "--automatic", "--prune", "--format", "json",
        ]);

        Assert.Equal(5, explicitRun.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, explicitRun.Status);
        using (var document = JsonDocument.Parse(explicitRun.StandardOutput))
        {
            Assert.Contains(document.RootElement.GetProperty("findings").EnumerateArray(), finding =>
                finding.GetProperty("code").GetString() == "extension-update.removed-extension");
            Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
        }
        Assert.Equal(beforeExplicit, workspace.Snapshot());

        var bulkRun = await workspace.RunAsync(
        [
            "extension", "update", "--all",
            "--source", source.Path,
            "--automatic", "--prune", "--format", "json",
        ]);

        Assert.Equal(2, bulkRun.ExitCode);
        Assert.Equal(CliSemanticStatus.Attention, bulkRun.Status);
        using var bulkDocument = JsonDocument.Parse(bulkRun.StandardOutput);
        Assert.Contains(bulkDocument.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "extension-update.bulk-excluded"
            && finding.GetProperty("subject").GetProperty("id").GetString() == "alpha");
        var effects = bulkDocument.RootElement.GetProperty("effects").EnumerateArray().ToArray();
        Assert.Contains(effects, effect => effect.GetProperty("path").GetString() == ".agents/beta.md");
        Assert.DoesNotContain(effects, effect => effect.GetProperty("path").GetString() == ".agents/alpha.md");
        Assert.Equal(Document("Alpha v1"), workspace.ReadText(".agents/alpha.md"));
        Assert.Equal(Document("Beta v2"), workspace.ReadText(".agents/beta.md"));
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Update preserves excluded files and old claims under prune and ignores future paths"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task ExcludedFilesAndDirectoriesAreRetainedUnderPrune()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-update-excluded-paths-prune");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-update-excluded-paths-prune-source");
        const string excludedFile = ".agents/guidance/exact.md";
        const string excludedDirectory = ".agents/guidance/retained";
        const string oldDirectoryFile = $"{excludedDirectory}/old.md";
        const string futureDirectoryFile = $"{excludedDirectory}/future.md";
        const string activeFile = ".agents/guidance/active.md";
        source.AddPackage(
            "toolkit",
            [],
            (excludedFile, Document("Exact v1")),
            (oldDirectoryFile, Document("Old v1")),
            (activeFile, Document("Active v1")));
        await InstallAllAsync(workspace, source);
        const string localExact = "Local exact bytes\n";
        const string localOld = "Local retained bytes\n";
        workspace.ReplaceText(excludedFile, localExact);
        workspace.ReplaceText(oldDirectoryFile, localOld);
        await File.WriteAllTextAsync(
            workspace.Combine(".agents/open-forge.json"),
            $$"""{"schemaVersion":1,"removedFiles":["{{excludedFile}}"],"removedDirectories":["{{excludedDirectory}}"]}""",
            TestContext.Current.CancellationToken);
        source.ReplacePayload("toolkit", excludedFile, Document("Exact v2"));
        source.RemovePayload("toolkit", oldDirectoryFile);
        source.AddPayload("toolkit", futureDirectoryFile, Document("Future"));
        source.ReplacePayload("toolkit", activeFile, Document("Active v2"));
        source.SetVersion("toolkit", "2.0.0");
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--automatic", "--prune", "--detail", "full", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var excludedFindings = document.RootElement.GetProperty("findings").EnumerateArray()
            .Where(finding => finding.GetProperty("code").GetString() == "extension-update.path-excluded")
            .ToArray();
        Assert.Equal(3, excludedFindings.Length);
        Assert.All(excludedFindings, finding =>
            Assert.Equal("info", finding.GetProperty("severity").GetString()));
        var effects = document.RootElement.GetProperty("effects").EnumerateArray().ToArray();
        Assert.Contains(effects, effect => effect.GetProperty("path").GetString() == activeFile);
        Assert.DoesNotContain(effects, effect => effect.GetProperty("path").GetString() == excludedFile);
        Assert.DoesNotContain(effects, effect => effect.GetProperty("path").GetString() == oldDirectoryFile);
        Assert.DoesNotContain(effects, effect => effect.GetProperty("path").GetString() == futureDirectoryFile);
        Assert.Equal(localExact, workspace.ReadText(excludedFile));
        Assert.Equal(localOld, workspace.ReadText(oldDirectoryFile));
        Assert.False(File.Exists(workspace.Combine(futureDirectoryFile)));
        Assert.Equal(Document("Active v2"), workspace.ReadText(activeFile));
        using var ownership = JsonDocument.Parse(workspace.ReadText(".agents/open-forge.lock.json"));
        Assert.Equal(
            [activeFile, excludedFile, oldDirectoryFile],
            Assert.Single(ownership.RootElement.GetProperty("extensions").EnumerateArray())
                .GetProperty("paths").EnumerateArray().Select(value => value.GetString()));
        Assert.DoesNotContain(futureDirectoryFile, ownership.RootElement.GetProperty("extensions")[0]
            .GetProperty("paths").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Unlisting an excluded path allows an ordinary Extension Update to restore it"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task UnlistedPathIsRestoredByOrdinaryUpdate()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-update-restore-unlisted-path");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-update-restore-unlisted-path-source");
        const string excludedFile = ".agents/guidance/restore.md";
        const string activeFile = ".agents/guidance/active.md";
        source.AddPackage("toolkit", [], (excludedFile, Document("Restore v1")), (activeFile, Document("Active v1")));
        await File.WriteAllTextAsync(
            workspace.Combine(".agents/open-forge.json"),
            $$"""{"schemaVersion":1,"removedFiles":["{{excludedFile}}"]}""",
            TestContext.Current.CancellationToken);
        var install = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);
        Assert.Equal(CliSemanticStatus.Complete, install.Status);
        Assert.False(File.Exists(workspace.Combine(excludedFile)));
        using (var ownership = JsonDocument.Parse(workspace.ReadText(".agents/open-forge.lock.json")))
        {
            Assert.DoesNotContain(excludedFile, Assert.Single(ownership.RootElement.GetProperty("extensions").EnumerateArray())
                .GetProperty("paths").EnumerateArray().Select(value => value.GetString()));
        }

        await File.WriteAllTextAsync(
            workspace.Combine(".agents/open-forge.json"),
            "{\"schemaVersion\":1}",
            TestContext.Current.CancellationToken);
        source.ReplacePayload("toolkit", excludedFile, Document("Restore v2"));
        source.ReplacePayload("toolkit", activeFile, Document("Active v2"));
        var beforeSource = source.Snapshot();

        var update = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, update.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, update.Status);
        Assert.Equal(Document("Restore v2"), workspace.ReadText(excludedFile));
        Assert.Equal(Document("Active v2"), workspace.ReadText(activeFile));
        using var finalOwnership = JsonDocument.Parse(workspace.ReadText(".agents/open-forge.lock.json"));
        Assert.Contains(excludedFile, Assert.Single(finalOwnership.RootElement.GetProperty("extensions").EnumerateArray())
            .GetProperty("paths").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Update revalidates removal settings under its lease before effects"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task ChangedRemovalSettingsUnderLeaseBlocksTheReviewedUpdate()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-update-settings-change-under-lease");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-update-settings-change-under-lease-source");
        source.AddPackage("toolkit", [], (".agents/toolkit.md", Document("Toolkit v1")));
        const string originalSettings = "{\"schemaVersion\":1}";
        const string changedSettings = "{\"schemaVersion\":1,\"removedExtensions\":[\"toolkit\"]}";
        await File.WriteAllTextAsync(
            workspace.Combine(".agents/open-forge.json"),
            originalSettings,
            TestContext.Current.CancellationToken);
        var install = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);
        Assert.Equal(CliSemanticStatus.Complete, install.Status);
        var previousContent = workspace.ReadText(".agents/toolkit.md");
        var previousOwnership = workspace.ReadText(".agents/open-forge.lock.json");
        source.ReplacePayload("toolkit", ".agents/toolkit.md", Document("Toolkit v2"));
        source.SetVersion("toolkit", "2.0.0");
        var beforeSource = source.Snapshot();

        var scripted = ScriptedCliTerminal.Lines(["yes"], canPrompt: true);
        var prompts = new CliPrompts(scripted.Terminal);
        var interaction = ExtensionInteractionTestFactory.ForUpdate(prompts);
        var confirm = interaction.Apply;
        interaction = interaction with
        {
            Apply = async (preview, question, policy, cancellationToken) =>
            {
                var reply = await confirm(preview, question, policy, cancellationToken);
                Assert.Equal(
                    originalSettings,
                    await File.ReadAllTextAsync(
                        workspace.Combine(".agents/open-forge.json"),
                        cancellationToken));
                await File.WriteAllTextAsync(
                    workspace.Combine(".agents/open-forge.json"),
                    changedSettings,
                    cancellationToken);
                return reply;
            },
        };

        var result = await ExtensionUpdateOperationFactory.Create(interaction, workspace.LockStoreRoot)
            .ExecuteAsync(
                new ExtensionUpdateRequest(
                    workspace.Workspace,
                    ExtensionUpdateMode.Apply,
                    ["toolkit"],
                    all: false,
                    sourcePath: source.Path,
                    force: false,
                    prune: false,
                    automatic: false,
                    allowInteraction: true),
                TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.True(result.Findings.Any(finding =>
            finding.Code == ExtensionUpdateFindingCode.TargetChanged
            && finding.Cause.Contains("Workspace settings", StringComparison.Ordinal)),
            string.Join(Environment.NewLine, result.Findings.Select(finding => $"{finding.Code}: {finding.Cause}")));
        Assert.Equal(previousContent, workspace.ReadText(".agents/toolkit.md"));
        Assert.Equal(previousOwnership, workspace.ReadText(".agents/open-forge.lock.json"));
        Assert.Equal(changedSettings, workspace.ReadText(".agents/open-forge.json"));
        Assert.Null(result.Recovery.ResidualPath);
        Assert.Equal(beforeSource, source.Snapshot());
    }

    private static async Task InstallAllAsync(
        ExtensionInstallIntegrationWorkspace workspace,
        ExtensionInstallCatalogue source)
    {
        var run = await workspace.RunAsync(
        [
            "extension", "install", "--all",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);
        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
    }

    private static string Document(string heading)
        => OpenForgeDocumentSeed.Metadata(heading, ["Extension"], $"# {heading}\n");
}
