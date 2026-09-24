using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Interaction;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Interaction;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

public sealed class ExtensionInstallRemovalSettingsIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Install blocks excluded selected and required package IDs before all effects"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    [InlineData("toolkit", "toolkit")]
    [InlineData("toolkit", "base")]
    public async Task ExcludedIdentityBlocksTheFullInstall(string selectedId, string excludedId)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create($"extension-install-excluded-id-{selectedId}-{excludedId}");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create($"extension-install-excluded-id-source-{selectedId}-{excludedId}");
        source.AddPackage("base", [], (".agents/base.md", Document("Base")));
        source.AddPackage("toolkit", ["base"], (".agents/toolkit.md", Document("Toolkit")));
        await File.WriteAllTextAsync(
            workspace.Combine(".agents/open-forge.json"),
            $$"""{"schemaVersion":1,"removedExtensions":["{{excludedId}}"]}""",
            TestContext.Current.CancellationToken);
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "install", selectedId,
            "--source", source.Path,
            "--automatic", "--force", "--format", "json",
        ]);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Contains(document.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "extension-install.removed-extension");
        Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Install leaves excluded files and directories out of writes and ownership"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task ExcludedPayloadPathsAreNotWrittenOrClaimed()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-excluded-payload-paths");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-install-excluded-payload-paths-source");
        const string excludedFile = ".agents/guidance/skip.md";
        const string excludedDirectory = ".agents/guidance/private";
        const string allowedFile = ".agents/guidance/active.md";
        source.AddPackage(
            "toolkit",
            [],
            (excludedFile, Document("Skip")),
            ($"{excludedDirectory}/future.md", Document("Future")),
            (allowedFile, Document("Active")));
        await File.WriteAllTextAsync(
            workspace.Combine(".agents/open-forge.json"),
            $$"""{"schemaVersion":1,"removedFiles":["{{excludedFile}}"],"removedDirectories":["{{excludedDirectory}}"]}""",
            TestContext.Current.CancellationToken);
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic", "--detail", "full", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var effects = document.RootElement.GetProperty("effects").EnumerateArray().ToArray();
        Assert.Contains(effects, effect => effect.GetProperty("path").GetString() == allowedFile);
        Assert.DoesNotContain(effects, effect => effect.GetProperty("path").GetString() == excludedFile);
        Assert.DoesNotContain(effects, effect => effect.GetProperty("path").GetString() == $"{excludedDirectory}/future.md");
        Assert.False(File.Exists(workspace.Combine(excludedFile)));
        Assert.False(File.Exists(workspace.Combine($"{excludedDirectory}/future.md")));
        using var ownership = JsonDocument.Parse(workspace.ReadText(".agents/open-forge.lock.json"));
        Assert.Equal(
            [allowedFile],
            Assert.Single(ownership.RootElement.GetProperty("extensions").EnumerateArray())
                .GetProperty("paths").EnumerateArray().Select(value => value.GetString()));
        var excludedFindings = document.RootElement.GetProperty("findings").EnumerateArray()
            .Where(finding => finding.GetProperty("code").GetString() == "extension-install.path-excluded")
            .ToArray();
        Assert.Equal(2, excludedFindings.Length);
        Assert.All(excludedFindings, finding =>
            Assert.Equal("info", finding.GetProperty("severity").GetString()));
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Install does not create a missing excluded ancestor for remaining content"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task MissingExcludedAncestorBlocksAllEffects()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-excluded-missing-parent");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-install-excluded-missing-parent-source");
        source.AddPackage("toolkit", [], (".agents/blocked-parent/child.md", Document("Child")));
        await File.WriteAllTextAsync(
            workspace.Combine(".agents/open-forge.json"),
            "{\"schemaVersion\":1,\"removedFiles\":[\".agents/blocked-parent\"]}",
            TestContext.Current.CancellationToken);
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        Assert.Contains(document.RootElement.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "extension-install.excluded-ancestor");
        Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
        Assert.False(Directory.Exists(workspace.Combine(".agents/blocked-parent")));
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Install revalidates removal settings under its lease before effects"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task ChangedRemovalSettingsUnderLeaseBlocksTheReviewedInstall()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-settings-change-under-lease");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-install-settings-change-under-lease-source");
        source.AddPackage("toolkit", [], (".agents/toolkit.md", Document("Toolkit")));
        const string originalSettings = "{\"schemaVersion\":1}";
        const string changedSettings = "{\"schemaVersion\":1,\"removedExtensions\":[\"toolkit\"]}";
        await File.WriteAllTextAsync(
            workspace.Combine(".agents/open-forge.json"),
            originalSettings,
            TestContext.Current.CancellationToken);
        var beforeOwnership = workspace.ReadText(ExtensionInstallIntegrationWorkspace.OwnershipPath);
        var beforeSource = source.Snapshot();

        var scripted = ScriptedCliTerminal.Lines(["yes"], canPrompt: true);
        var prompts = new CliPrompts(scripted.Terminal);
        var interaction = ExtensionInteractionTestFactory.ForInstall(prompts);
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

        var result = await ExtensionInstallOperationFactory.Create(interaction, workspace.LockStoreRoot)
            .ExecuteAsync(
                new ExtensionInstallRequest(
                    workspace.Workspace,
                    ExtensionInstallMode.Apply,
                    ["toolkit"],
                    all: false,
                    sourcePath: source.Path,
                    force: false,
                    automatic: false,
                    allowInteraction: true),
                TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding =>
            finding.Code == ExtensionInstallFindingCode.TargetChanged
            && finding.Cause.Contains("Workspace removal settings changed", StringComparison.Ordinal));
        Assert.False(File.Exists(workspace.Combine(".agents/toolkit.md")));
        Assert.Equal(changedSettings, workspace.ReadText(".agents/open-forge.json"));
        Assert.Equal(beforeOwnership, workspace.ReadText(ExtensionInstallIntegrationWorkspace.OwnershipPath));
        Assert.Null(result.Recovery.ResidualPath);
        Assert.Equal(beforeSource, source.Snapshot());
    }

    private static string Document(string heading)
        => OpenForgeDocumentSeed.Metadata(heading, ["Extension"], $"# {heading}\n");
}
