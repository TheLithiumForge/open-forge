using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

public sealed class InstallImplicitSetupOwnershipIntegrationTests
{
    [Theory(DisplayName = "Implicit first setup blocks unavailable sharing policy before application confirmation"), InlineData("invalid"), InlineData("directory"), InlineData("unavailable")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task FreshImplicitSelectionRequiresReadablePolicy(string state)
    {
        using var workspace = InstallOperationWorkspace.Create("install-implicit-ownership");
        var unavailable = SeedUnknownOwnership(workspace, state == "unavailable" ? "invalid" : state);
        var presets = 0;
        var confirmations = 0;
        var before = workspace.SnapshotHashes();
        if (state == "unavailable")
            unavailable = new FileStream(workspace.Combine(InstallOperationWorkspace.OwnershipPath), FileMode.Open, FileAccess.Read, FileShare.None);
        try
        {
            var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(observe: (_, _) => confirmations++),
                workspace.LockStoreRoot, Selection(InstallPreset.Essentials, () => presets++));
            var result = await operation.ExecuteAsync(workspace.Request(automatic: false, allowsInteractiveConfirmation: true), TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Blocked, result.Status);
            Assert.Contains(result.Findings, finding => finding.Code == InstallFindingCode.LifecycleBlocked);
            Assert.Equal(1, presets);
            Assert.Equal(0, confirmations);
            Assert.NotNull(result.Input.Configuration);
            Assert.False(workspace.Exists(".agents/loader.md"));
            Assert.False(workspace.Exists(".agents/memory/working/_working.md"));
            Assert.False(workspace.Exists(".agents/guidance/_guidance.md"));
            Assert.Equal(InstallLifecycleOutcome.NotRequested, result.Facts.Lifecycle.Outcome);
            Assert.Empty(result.Facts.Effects);
        }
        finally { unavailable?.Dispose(); }
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Ordinary installed repeat bypasses setup ownership admission and retains the ordinary planner boundary")]
    [InlineData("invalid"), InlineData("directory"), InlineData("unavailable"), Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task InstalledRepeatDoesNotEnterSetup(string state)
    {
        using var workspace = InstallOperationWorkspace.Create("install-repeat-ownership");
        var initial = InstallOperationFactory.Create(InstallInteractionTestSupport.Unavailable(), workspace.LockStoreRoot);
        Assert.Equal(CliSemanticStatus.Complete, (await initial.ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken)).Status);
        if (state == "directory") File.Delete(workspace.Combine(InstallOperationWorkspace.OwnershipPath));
        var unavailable = SeedUnknownOwnership(workspace, state, installed: true);
        var presets = 0;
        var confirmations = 0;
        try
        {
            var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(observe: (_, _) => confirmations++),
                workspace.LockStoreRoot, Selection(InstallPreset.Essentials, () => presets++));
            var ordinary = await initial.ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken);
            var repeat = await operation.ExecuteAsync(workspace.Request(automatic: false, allowsInteractiveConfirmation: true), TestContext.Current.CancellationToken);
            Assert.Equal(ordinary.Status, repeat.Status);
            Assert.Equal(CliSemanticStatus.Blocked, repeat.Status);
            Assert.Contains(repeat.Findings, finding => finding.Code == InstallFindingCode.LifecycleBlocked);
            Assert.Equal(ordinary.Findings.Select(finding => (finding.Code, finding.Cause)), repeat.Findings.Select(finding => (finding.Code, finding.Cause)));
            Assert.Empty(repeat.Facts.Effects);
            Assert.Null(repeat.Input.Configuration);
            Assert.Equal(0, presets);
            Assert.Equal(0, confirmations);
        }
        finally
        {
            unavailable?.Dispose();
            if (state == "directory") Directory.Delete(workspace.Combine(InstallOperationWorkspace.OwnershipPath));
        }
    }

    [Theory(DisplayName = "Implicit selection still blocks actual source adoption when ownership cannot rule out competing claims")]
    [InlineData("invalid"), InlineData("directory"), InlineData("unavailable"), Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task AffectedAdoptionStillRequiresOwnership(string state)
    {
        using var workspace = InstallOperationWorkspace.Create("install-implicit-adoption-ownership");
        workspace.WriteText(".agents/guidance/note.md", "# Authored note\n\nKeep my note.\n");
        var unavailable = SeedUnknownOwnership(workspace, state);
        var presets = 0;
        var confirmations = 0;
        try
        {
            var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(observe: (_, _) => confirmations++),
                workspace.LockStoreRoot, Selection(InstallPreset.FullCore, () => presets++));
            var result = await operation.ExecuteAsync(workspace.Request(automatic: false, allowsInteractiveConfirmation: true), TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Blocked, result.Status);
            Assert.Contains(result.Findings, finding => finding.Code == InstallFindingCode.LifecycleBlocked);
            Assert.Empty(result.Facts.Effects);
            Assert.Equal(1, presets);
            Assert.Equal(0, confirmations);
            Assert.False(workspace.Exists(".agents/loader.md"));
            Assert.Equal("# Authored note\n\nKeep my note.\n", File.ReadAllText(workspace.Combine(".agents/guidance/note.md")));
            Assert.False(workspace.Exists(".agents/open-forge.json"));
            Assert.False(workspace.Exists(".gitignore"));
        }
        finally { unavailable?.Dispose(); }
    }

    [Fact(DisplayName = "A known Framework receipt with a missing loader forbids initial preset adoption")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task MissingManagedLoaderDoesNotBecomeFreshSetup()
    {
        using var workspace = InstallOperationWorkspace.Create("install-missing-owned-loader");
        var initial = InstallOperationFactory.Create(InstallInteractionTestSupport.Unavailable(), workspace.LockStoreRoot);
        Assert.Equal(CliSemanticStatus.Complete, (await initial.ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken)).Status);
        File.Delete(workspace.Combine(".agents/loader.md"));
        var before = workspace.SnapshotHashes();
        var presets = 0;
        var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(), workspace.LockStoreRoot,
            Selection(InstallPreset.Essentials, () => presets++));
        var result = await operation.ExecuteAsync(workspace.Request(force: true, automatic: false, allowsInteractiveConfirmation: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == InstallFindingCode.ManagedDivergence);
        Assert.Empty(result.Facts.Effects);
        Assert.Equal(0, presets);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static InstallSetupInteraction Selection(InstallPreset preset, Action observe)
        => new((_, policy, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            Assert.True(policy.Allowed);
            observe();
            return ValueTask.FromResult(CliPromptReply<InstallPreset>.Answered(preset));
        }, (_, _, _) => throw new InvalidOperationException("No Custom route prompt is expected."),
            (_, _, _) => throw new InvalidOperationException("No Custom action prompt is expected."));

    private static FileStream? SeedUnknownOwnership(InstallOperationWorkspace workspace, string state, bool installed = false)
    {
        var path = workspace.Combine(InstallOperationWorkspace.OwnershipPath);
        if (state == "directory")
        {
            if (installed) Directory.CreateDirectory(path);
            else workspace.CreateDirectory(InstallOperationWorkspace.OwnershipPath);
            return null;
        }
        if (installed) File.WriteAllText(path, "invalid receipt");
        else workspace.WriteText(InstallOperationWorkspace.OwnershipPath, "invalid receipt");
        return state == "unavailable" ? new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None) : null;
    }
}
