using System.Text.Json;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Configuration;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

public sealed class InstallFrontmatterIntegrationTests
{
    [Fact(DisplayName = "A fresh resolved root selection persists its setting and delivers root metadata"), Trait("Feature", "install-frontmatter"), Trait("Evidence", "Integration")]
    public async Task FreshDefaultPersistsRootAndDeliversRoot()
    {
        using var workspace = InstallOperationWorkspace.Create("install-frontmatter-root");
        var request = await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Root);
        var before = workspace.SnapshotHashes();
        var preview = await InstallFrontmatterFixture.Operation(workspace).ExecuteAsync(
            await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Root, dryRun: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        var result = await InstallFrontmatterFixture.Operation(workspace).ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(preview.Facts.Effects.Select(effect => effect.Path), result.Facts.Effects.Select(effect => effect.Path));
        Assert.Equal("root", Assert.IsType<InstallFrontmatter>(result.Frontmatter).Form);
        var settings = await WorkspaceSettingsReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, TestContext.Current.CancellationToken);
        Assert.Equal(FrontmatterForm.Root, settings.Document.DeclaredFrontmatter);
        Assert.DoesNotContain("open-forge:", File.ReadAllText(workspace.Combine(".agents/directives/_directives.md")), StringComparison.Ordinal);
        Assert.Single(result.Facts.Effects, effect => effect.Path == ".agents/open-forge.json");
        Assert.Equal(InstallOperationWorkspace.EmbeddedPayloadPaths.Count, result.Facts.Footprint?.PayloadFiles);
    }

    [Fact(DisplayName = "A fresh scoped selection persists Scoped and retains canonical delivered bytes"), Trait("Feature", "install-frontmatter"), Trait("Evidence", "Integration")]
    public async Task FreshScopedPersistsScopedAndDeliversCanonicalBytes()
    {
        using var workspace = InstallOperationWorkspace.Create("install-frontmatter-scoped");
        await InstallFrontmatterFixture.SeedScopedAsync(workspace);
        var settings = await WorkspaceSettingsReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, TestContext.Current.CancellationToken);
        Assert.Equal(FrontmatterForm.Scoped, settings.Document.DeclaredFrontmatter);
        var payload = Assert.IsType<Core.Framework.Distribution.Models.FrameworkPayload>(EmbeddedFrameworkPayloadReader.Read().Payload);
        foreach (var asset in payload.Assets.Where(asset => asset.Path.StartsWith(".agents/", StringComparison.Ordinal)))
            Assert.Equal(asset.Bytes.ToArray(), File.ReadAllBytes(workspace.Combine(asset.Path)));
    }

    [Fact(DisplayName = "An existing missing frontmatter key retains Scoped without a settings effect"), Trait("Feature", "install-frontmatter"), Trait("Evidence", "Integration")]
    public async Task ExistingMissingKeyStaysScoped()
    {
        using var workspace = InstallOperationWorkspace.Create("install-frontmatter-legacy");
        var operation = InstallFrontmatterFixture.Operation(workspace);
        Assert.Equal(CliSemanticStatus.Complete, (await operation.ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken)).Status);
        Assert.False(workspace.Exists(".agents/open-forge.json"));
        var before = workspace.SnapshotHashes();
        var result = await operation.ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Facts.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Repeated root Install compares rendered identity and has no effects"), Trait("Feature", "install-frontmatter"), Trait("Evidence", "Integration")]
    public async Task RepeatedRootInstallIsNoOp()
    {
        using var workspace = InstallOperationWorkspace.Create("install-frontmatter-repeat");
        var operation = InstallFrontmatterFixture.Operation(workspace);
        Assert.Equal(CliSemanticStatus.Complete, (await operation.ExecuteAsync(await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Root), TestContext.Current.CancellationToken)).Status);
        var before = workspace.SnapshotHashes();
        var result = await operation.ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Facts.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Form-only configuration preserves exclusions, ignore bytes, sharing and ownership"), Trait("Feature", "install-frontmatter"), Trait("Evidence", "Integration")]
    public async Task FormOnlyConfigurePreservesRoutes()
    {
        using var workspace = InstallOperationWorkspace.Create("install-frontmatter-form-only");
        workspace.WriteText(".agents/open-forge.json", """{"removedCategories":["templates"],"foreign":7}""");
        workspace.WriteText(".gitignore", "# authored ignore\n*.local\n");
        await InstallFrontmatterFixture.SeedScopedAsync(workspace);
        var ownership = File.ReadAllBytes(workspace.Combine(InstallOperationWorkspace.OwnershipPath));
        var ignore = File.ReadAllBytes(workspace.Combine(".gitignore"));
        using var heldIgnore = new FileStream(workspace.Combine(".gitignore"), FileMode.Open, FileAccess.Read, FileShare.None);
        var result = await InstallFrontmatterFixture.Operation(workspace).ExecuteAsync(
            await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Root, installed: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(ownership, File.ReadAllBytes(workspace.Combine(InstallOperationWorkspace.OwnershipPath)));
        Assert.DoesNotContain(result.Facts.Effects, effect => effect.Path == ".gitignore" || effect.Path == InstallOperationWorkspace.OwnershipPath);
        using var settings = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(".agents/open-forge.json")));
        Assert.Equal("templates", settings.RootElement.GetProperty("removedCategories")[0].GetString());
        Assert.Equal(7, settings.RootElement.GetProperty("foreign").GetInt32());
        Assert.False(workspace.Exists(".agents/templates"));
        heldIgnore.Dispose();
        Assert.Equal(ignore, File.ReadAllBytes(workspace.Combine(".gitignore")));
    }

    [Fact(DisplayName = "Settings changed after confirmation prevent every planned frontmatter effect"), Trait("Feature", "install-frontmatter"), Trait("Evidence", "Integration")]
    public async Task StaleSettingsPreventEffects()
    {
        using var workspace = InstallOperationWorkspace.Create("install-frontmatter-stale");
        await InstallFrontmatterFixture.SeedScopedAsync(workspace);
        var request = await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Root, installed: true);
        var before = File.ReadAllBytes(workspace.Combine(".agents/directives/_directives.md"));
        var operation = InstallFrontmatterFixture.Operation(workspace, InstallInteractionTestSupport.Confirmation(observe: (_, _) =>
            workspace.ReplaceInstalledText(".agents/open-forge.json", """{"frontmatter":"scoped","concurrent":true}""")));
        var result = await operation.ExecuteAsync(new InstallRequest(workspace.Workspace, InstallMode.Apply, false, false, true)
        { Frontmatter = request.Frontmatter }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.All(result.Facts.Effects, effect => Assert.Equal(InstallEffectOutcome.NotStarted, effect.Outcome));
        Assert.Equal(before, File.ReadAllBytes(workspace.Combine(".agents/directives/_directives.md")));
    }

    [Fact(DisplayName = "Persisting explicit Scoped on an exact legacy workspace retains a settings-only plan"), Trait("Feature", "install-frontmatter"), Trait("Evidence", "Integration")]
    public async Task SettingsOnlyPlanSurvivesExactAdmission()
    {
        using var workspace = InstallOperationWorkspace.Create("install-frontmatter-settings-only");
        var operation = InstallFrontmatterFixture.Operation(workspace);
        Assert.Equal(CliSemanticStatus.Complete, (await operation.ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken)).Status);
        var selection = (await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Scoped, installed: true)).Frontmatter;
        var result = await operation.ExecuteAsync(workspace.Request() with { Frontmatter = selection }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(".agents/open-forge.json", Assert.Single(result.Facts.Effects).Path);
    }

    [Fact(DisplayName = "Changing Root back to Scoped restores canonical payload bytes"), Trait("Feature", "install-frontmatter"), Trait("Evidence", "Integration")]
    public async Task ConfigureBackToScopedUsesCanonicalBytes()
    {
        using var workspace = InstallOperationWorkspace.Create("install-frontmatter-reverse");
        var operation = InstallFrontmatterFixture.Operation(workspace);
        Assert.Equal(CliSemanticStatus.Complete, (await operation.ExecuteAsync(await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Root), TestContext.Current.CancellationToken)).Status);
        var result = await operation.ExecuteAsync(await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Scoped, installed: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal("root", result.Frontmatter?.PreviousForm);
        var payload = Assert.IsType<Core.Framework.Distribution.Models.FrameworkPayload>(EmbeddedFrameworkPayloadReader.Read().Payload);
        foreach (var asset in payload.Assets.Where(asset => asset.Path.StartsWith(".agents/", StringComparison.Ordinal)))
            Assert.Equal(asset.Bytes.ToArray(), File.ReadAllBytes(workspace.Combine(asset.Path)));
    }

    [Fact(DisplayName = "Form-only configuration preserves missing payload files and their receipts"), Trait("Feature", "install-frontmatter"), Trait("Evidence", "Integration")]
    public async Task FormOnlyDoesNotRestoreMissingPayload()
    {
        using var workspace = InstallOperationWorkspace.Create("install-frontmatter-missing-payload");
        await InstallFrontmatterFixture.SeedScopedAsync(workspace);
        const string path = ".agents/guidance/_guidance.md";
        File.Delete(workspace.Combine(path));
        var ownership = File.ReadAllBytes(workspace.Combine(InstallOperationWorkspace.OwnershipPath));
        var result = await InstallFrontmatterFixture.Operation(workspace).ExecuteAsync(
            await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Root, installed: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.False(workspace.Exists(path));
        Assert.DoesNotContain(result.Facts.Effects, effect => effect.Path == path);
        Assert.Equal(ownership, File.ReadAllBytes(workspace.Combine(InstallOperationWorkspace.OwnershipPath)));
    }
}
