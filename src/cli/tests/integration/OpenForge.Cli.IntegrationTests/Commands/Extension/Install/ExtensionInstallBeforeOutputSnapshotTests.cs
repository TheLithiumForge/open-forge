using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport.Snapshots;
using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Install;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install.Shared.Permissions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class ExtensionInstallBeforeOutputSnapshotTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension install output preserves an earlier created file when a later replacement is denied")]
    public async Task PartialWriteFailure()
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This deterministic replacement failure requires Windows file sharing.");
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-output-partial");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-install-output-partial-source");
        source.AddPackage("toolkit", [], (".agents/aaa.md", "# Earlier\n"), (".agents/toolkit.md", "# Toolkit\n"));
        workspace.CreateOccupant(".agents/toolkit.md", "prior local content\n");
        var sourceBefore = source.Snapshot();
        var scripted = ScriptedCliTerminal.Lines([], canPrompt: false);
        var prompts = new CliPrompts(scripted.Terminal);
        ExtensionInstallResult result;
        using (var held = File.Open(workspace.Combine(".agents/toolkit.md"), FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            result = await ExtensionInstallOperationFactory.Create(
                    ExtensionInteractionTestFactory.ForInstall(prompts), workspace.LockStoreRoot)
                .ExecuteAsync(new ExtensionInstallRequest(workspace.Workspace, ExtensionInstallMode.Apply, ["toolkit"], false, source.Path,
                    force: true, automatic: true, allowInteraction: false), TestContext.Current.CancellationToken);
        }
        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Equal("# Earlier\n", workspace.ReadText(".agents/aaa.md"));
        Assert.Equal("prior local content\n", workspace.ReadText(".agents/toolkit.md"));
        Assert.Equal(sourceBefore, source.Snapshot());
        Assert.NotNull(result.Recovery.ResidualPath);
        AssertRecovery(result);
        Renderers.MatchDetails(result, "write-failed-partial", result.Recovery.ResidualPath, source.Path,
            testName: nameof(PartialWriteFailure));
    }

    private static readonly CommandOutputRenderers<ExtensionInstallResult> Renderers = CommandOutputRenderers<ExtensionInstallResult>.From(ExtensionInstallPresentation.Rendering);

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Extension install output preserves package selection, content safety and dependency effects")]
    [InlineData("single-package", (int)CliSemanticStatus.Complete)]
    [InlineData("with-dependencies", (int)CliSemanticStatus.Complete)]
    [InlineData("select-from-source-prompt", (int)CliSemanticStatus.Complete)]
    [InlineData("no-selection-non-interactive", (int)CliSemanticStatus.Invalid)]
    [InlineData("existing-file-without-force", (int)CliSemanticStatus.Blocked)]
    [InlineData("with-force", (int)CliSemanticStatus.Complete)]
    [InlineData("already-installed", (int)CliSemanticStatus.Complete)]
    [InlineData("changed-since-install", (int)CliSemanticStatus.Blocked)]
    [InlineData("removed-extension", (int)CliSemanticStatus.Blocked)]
    [InlineData("path-excluded", (int)CliSemanticStatus.Complete)]
    [InlineData("no-content-directory", (int)CliSemanticStatus.Attention)]
    [InlineData("dry-run", (int)CliSemanticStatus.Complete)]
    [InlineData("source-unreadable", (int)CliSemanticStatus.Incomplete)]
    [InlineData("lock-held", (int)CliSemanticStatus.Blocked)]
    [InlineData("cancelled", (int)CliSemanticStatus.Interrupted)]
    public async Task PackageInstallation(string situation, int status)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-output");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-install-output-source");
        source.AddPackage("toolkit", situation == "with-dependencies" ? ["base"] : [],
            situation == "no-content-directory" ? [] : [(".agents/toolkit.md", "# Toolkit\n")]);
        if (situation == "removed-extension")
        {
            await File.WriteAllTextAsync(
                workspace.Combine(".agents/open-forge.json"),
                "{\"schemaVersion\":1,\"removedExtensions\":[\"toolkit\"]}",
                TestContext.Current.CancellationToken);
        }
        else if (situation == "path-excluded")
        {
            await File.WriteAllTextAsync(
                workspace.Combine(".agents/open-forge.json"),
                "{\"schemaVersion\":1,\"removedFiles\":[\".agents/toolkit.md\"]}",
                TestContext.Current.CancellationToken);
        }
        if (situation is "with-dependencies" or "select-from-source-prompt" or "no-selection-non-interactive")
            source.AddPackage("base", [], (".agents/base.md", "# Base\n"));
        var prompted = situation == "select-from-source-prompt";
        var scripted = ScriptedCliTerminal.Lines(
            prompted ? ["2", "yes"] : [],
            canPrompt: prompted);
        var prompts = new CliPrompts(scripted.Terminal);
        var operation = ExtensionInstallOperationFactory.Create(
            ExtensionInteractionTestFactory.ForInstall(prompts), workspace.LockStoreRoot);
        var request = new ExtensionInstallRequest(workspace.Workspace,
            situation == "dry-run" ? ExtensionInstallMode.DryRun : ExtensionInstallMode.Apply,
            situation is "select-from-source-prompt" or "no-selection-non-interactive" ? [] : ["toolkit"],
            all: false, source.Path, force: situation == "with-force", automatic: !prompted, allowInteraction: prompted);
        if (situation is "already-installed" or "changed-since-install")
        {
            var initial = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Complete, initial.Status);
            if (situation == "changed-since-install") workspace.ReplaceText(".agents/toolkit.md", "changed installed content\n");
        }
        if (situation is "existing-file-without-force" or "with-force") workspace.CreateOccupant(".agents/toolkit.md", "prior local content\n");
        var before = workspace.Snapshot();
        var sourceBefore = source.Snapshot();
        using var cancellation = new CancellationTokenSource();
        if (situation == "cancelled") cancellation.Cancel();
        ExtensionInstallResult result;
        using (var held = situation == "lock-held" ? workspace.HoldLock() : null)
        using (var unreadable = situation == "source-unreadable"
                   ? File.Open(Path.Combine(source.PackagePath("toolkit"), "extension.json"), FileMode.Open, FileAccess.Read, FileShare.None) : null)
        {
            result = await operation.ExecuteAsync(request, cancellation.Token);
        }
        Assert.Equal((CliSemanticStatus)status, result.Status);
        if (situation is "single-package" or "with-dependencies" or "select-from-source-prompt" or "with-force")
            Assert.Equal("# Toolkit\n", workspace.ReadText(".agents/toolkit.md"));
        else if (situation == "removed-extension")
            Assert.Equal(before, workspace.Snapshot());
        else if (situation == "path-excluded")
        {
            Assert.False(File.Exists(workspace.Combine(".agents/toolkit.md")));
            Assert.Contains(result.Findings, finding => finding.Code == ExtensionInstallFindingCode.PathExcluded);
            Assert.DoesNotContain(result.Findings, finding => finding.Code == ExtensionInstallFindingCode.PackageContentMissing);
            Assert.DoesNotContain(
                ".agents/toolkit.md",
                workspace.ReadExtensionOwnership().EnumerateArray()
                    .SelectMany(extension => extension.GetProperty("paths").EnumerateArray())
                    .Select(value => value.GetString()));
        }
        else if (situation != "no-content-directory") Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(sourceBefore, source.Snapshot());
        if (prompted) Assert.NotEmpty(scripted.Output.ToString());
        AssertRecovery(result);
        Renderers.MatchDetails(result, situation, result.Recovery.ResidualPath, source.Path,
            testName: $"{nameof(PackageInstallation)}_{situation}");
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Extension install output preserves exact external path permission decisions")]
    [InlineData("permission-required-non-interactive", (int)CliSemanticStatus.Blocked)]
    [InlineData("permission-prompt-always", (int)CliSemanticStatus.Complete)]
    [InlineData("permission-prompt-once", (int)CliSemanticStatus.Complete)]
    [InlineData("allow-path-flag", (int)CliSemanticStatus.Complete)]
    public async Task PathPermission(string situation, int status)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-output-permission");
        await workspace.SeedFrameworkAsync();
        using var source = PermissionFixture.CreatePackage();
        var prompted = situation is "permission-prompt-always" or "permission-prompt-once";
        string[] promptedInput = situation == "permission-prompt-always" ? ["always", "yes"] : ["once", "yes"];
        IEnumerable<string?> lines = prompted ? promptedInput : [];
        var scripted = ScriptedCliTerminal.Lines(lines, canPrompt: prompted);
        var prompts = new CliPrompts(scripted.Terminal);
        var before = workspace.Snapshot();
        var sourceBefore = source.SnapshotHashes();
        try
        {
            var result = await ExtensionInstallOperationFactory.Create(
                    ExtensionInteractionTestFactory.ForInstall(prompts), workspace.LockStoreRoot)
                .ExecuteAsync(new ExtensionInstallRequest(workspace.Workspace, ExtensionInstallMode.Apply, ["team"], false, source.Path,
                    force: false, automatic: !prompted, allowInteraction: prompted,
                    allowPath: situation == "allow-path-flag" ? [PermissionFixture.ExternalPath] : []), TestContext.Current.CancellationToken);
            Assert.Equal((CliSemanticStatus)status, result.Status);
            if (situation == "permission-required-non-interactive") Assert.Equal(before, workspace.Snapshot());
            else Assert.Equal("content bytes\n", File.ReadAllText(workspace.Combine(PermissionFixture.ExternalPath)));
            Assert.Equal(sourceBefore, source.SnapshotHashes());
            AssertRecovery(result);
            Renderers.MatchDetails(result, situation, result.Recovery.ResidualPath, source.Path,
                testName: $"{nameof(PathPermission)}_{situation}");
        }
        finally
        {
            PermissionFixture.DeleteExternalOutput(workspace);
        }
    }

    private static void AssertRecovery(ExtensionInstallResult result)
    {
        if (result.Recovery.ResidualPath is { } path) Assert.True(File.Exists(path));
    }

    [Theory, InlineData("core-templates", "templates"), InlineData("only-guidance", "guidance")]
    [Trait("Feature", "extension-install"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
    public async Task EntirelyExcludedPackageReportsItsCategoryAndRecordedInstallation(string id, string category)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-excluded-wording");
        using var source = ExtensionInstallCatalogue.Create("extension-install-excluded-wording-source");
        var setup = await workspace.RunAsync(["install", "--preset", "essentials", "--automatic"]);
        Assert.Equal(0, setup.ExitCode);
        string? sourcePath = null;
        if (id == "only-guidance")
        {
            source.AddPackage(id, [], (".agents/guidance/note.md", "A package note.\n"));
            sourcePath = source.Path;
            var settings = JsonNode.Parse(workspace.ReadText(".agents/open-forge.json"))!.AsObject();
            settings["removedCategories"] = new JsonArray(category);
            workspace.ReplaceText(".agents/open-forge.json", settings.ToJsonString());
        }

        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        var operation = CreateOperation(workspace, ScriptedCliTerminal.Lines([], canPrompt: false));
        var before = workspace.Snapshot();
        var preview = await operation.ExecuteAsync(Request(workspace, id: id, sourcePath: sourcePath, mode: ExtensionInstallMode.DryRun), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(before, workspace.Snapshot());
        Renderers.MatchDetails(preview, id + ".preview", extensionSourcePath: sourcePath, snapshotCollector: snapshots);

        var installed = await operation.ExecuteAsync(Request(workspace, id: id, sourcePath: sourcePath, mode: ExtensionInstallMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, installed.Status);
        var receipt = Assert.Single(workspace.ReadExtensionOwnership().EnumerateArray());
        Assert.Equal(id, receipt.GetProperty("id").GetString());
        Assert.Empty(receipt.GetProperty("paths").EnumerateArray());
        if (category == "templates") Assert.False(Directory.Exists(workspace.Combine(".agents/templates")));
        if (id == "only-guidance") Assert.False(File.Exists(workspace.Combine(".agents/guidance/note.md")));
        Renderers.MatchDetails(installed, id + ".installed", extensionSourcePath: sourcePath, snapshotCollector: snapshots);

        var recorded = workspace.Snapshot();
        var repeated = await operation.ExecuteAsync(Request(workspace, id: id, sourcePath: sourcePath, mode: ExtensionInstallMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
        Assert.Equal(recorded, workspace.Snapshot());
        Renderers.MatchDetails(repeated, id + ".already-recorded", extensionSourcePath: sourcePath, snapshotCollector: snapshots);
        CommandOutputSnapshot.MatchDetailSnapshot(snapshots, testName: $"{nameof(EntirelyExcludedPackageReportsItsCategoryAndRecordedInstallation)}.{id}");
    }

    [Theory, InlineData("planning"), InlineData("development-toolkit")]
    [Trait("Feature", "extension-install"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
    public async Task DependencyCountsUsePackageSingularAndPlural(string id)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-dependency-wording");
        await workspace.SeedFrameworkAsync();
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        var operation = CreateOperation(workspace, ScriptedCliTerminal.Lines([], canPrompt: false));
        var before = workspace.Snapshot();
        var preview = await operation.ExecuteAsync(Request(workspace, id: id, sourcePath: null, mode: ExtensionInstallMode.DryRun), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(before, workspace.Snapshot());
        Renderers.MatchDetails(preview, id + ".preview", snapshotCollector: snapshots);
        var applied = await operation.ExecuteAsync(Request(workspace, id: id, sourcePath: null, mode: ExtensionInstallMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.NotEmpty(workspace.ReadExtensionOwnership().EnumerateArray());
        Renderers.MatchDetails(applied, id + ".installed", snapshotCollector: snapshots);
        CommandOutputSnapshot.MatchDetailSnapshot(snapshots, testName: $"{nameof(DependencyCountsUsePackageSingularAndPlural)}.{id}");
    }

    [Fact, Trait("Feature", "extension-install"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
    public async Task CancelledReviewedPlanHasNoCompletedEntriesUpdate()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-cancel-wording");
        await workspace.SeedFrameworkAsync();
        var before = workspace.Snapshot();
        var scripted = ScriptedCliTerminal.Lines(["no"]);
        var result = await CreateOperation(workspace, scripted).ExecuteAsync(new ExtensionInstallRequest(
            ImplicitWorkspace(workspace), ExtensionInstallMode.Apply, ["development"], all: false, sourcePath: null,
            force: false, automatic: false, allowInteraction: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains("Would update the Entries section", scripted.Output.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("Updated the Entries section", scripted.Output.ToString(), StringComparison.Ordinal);
        Assert.Equal(before, workspace.Snapshot());
        Renderers.MatchDetails(result, "cancelled-plan");
    }

    private static ExtensionInstallOperation CreateOperation(ExtensionInstallIntegrationWorkspace workspace, ScriptedCliTerminal scripted)
        => ExtensionInstallOperationFactory.Create(ExtensionInteractionTestFactory.ForInstall(new CliPrompts(scripted.Terminal)), workspace.LockStoreRoot);

    [Fact(DisplayName = "Planning on Essentials reports skipped starters in its preview and receipt")]
    [Trait("Feature", "extension-install"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
    public async Task PartiallyExcludedPlanningReportsStarters()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("planning-partial-exclusion");
        Assert.Equal(0, (await workspace.RunAsync(["install", "--preset", "essentials", "--automatic"])).ExitCode);
        var operation = CreateOperation(workspace, ScriptedCliTerminal.Lines([], canPrompt: false));
        var before = workspace.Snapshot();
        var preview = await operation.ExecuteAsync(Request(workspace, "planning", null, ExtensionInstallMode.DryRun), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        var excluded = Assert.Single(preview.ExcludedPackages, package => package.Id == "planning");
        Assert.Equal("planning", excluded.Id);
        Assert.Equal("Planning", excluded.Name);
        Assert.False(excluded.EntirePayloadExcluded);
        Assert.Equal(["templates"], excluded.Categories);
        Assert.Equal(before, workspace.Snapshot());
        Renderers.MatchDetails(preview, "preview", testName: nameof(PartiallyExcludedPlanningReportsStarters) + ".preview");
        var applied = await operation.ExecuteAsync(Request(workspace, "planning", null, ExtensionInstallMode.Apply), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal(excluded.FileCount, Assert.Single(applied.ExcludedPackages, package => package.Id == "planning").FileCount);
        Assert.False(Directory.Exists(workspace.Combine(".agents/templates")));
        var planning = Assert.Single(workspace.ReadExtensionOwnership().EnumerateArray(), package => package.GetProperty("id").GetString() == "planning");
        Assert.NotEmpty(planning.GetProperty("paths").EnumerateArray());
        Assert.DoesNotContain(planning.GetProperty("paths").EnumerateArray(), path => path.GetString() is { } value && value.StartsWith(".agents/templates/", StringComparison.Ordinal));
        Renderers.MatchDetails(applied, "applied", testName: nameof(PartiallyExcludedPlanningReportsStarters) + ".applied");
    }

    private static ExtensionInstallRequest Request(ExtensionInstallIntegrationWorkspace workspace, string id, string? sourcePath, ExtensionInstallMode mode)
        => new(ImplicitWorkspace(workspace), mode, [id], all: false, sourcePath: sourcePath,
            force: false, automatic: mode == ExtensionInstallMode.Apply, allowInteraction: false);

    private static CliWorkspace ImplicitWorkspace(ExtensionInstallIntegrationWorkspace workspace)
        => new(lexicalRoot: workspace.Path, physicalRoot: workspace.Path, selectedBy: CliWorkspaceSelectionMethod.CurrentDirectory);
}
