using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Configuration;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

public sealed class InstallImplicitSetupOwnershipIntegrationTests
{
    [Theory(DisplayName = "Custom setup consumes one complete route answer and retains explicit rows")]
    [InlineData(false), InlineData(true), Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task CustomReturnsOneCompleteAnswer(bool installed)
    {
        using var workspace = InstallOperationWorkspace.Create("install-custom-route-answer");
        if (installed) workspace.WriteText(".agents/loader.md", "# Loader\n");
        var before = workspace.SnapshotHashes();
        var calls = 0;
        var interaction = new InstallSetupInteraction((_, _, _) => throw new InvalidOperationException("The preset is explicit."),
            (question, _, _) =>
            {
                calls++;
                Assert.Equal(installed, question.Installed);
                Assert.Equal(InstallConfigurationChoices.RouteIds, question.Routes.Select(row => row.Id));
                Assert.Equal(new InstallRouteLock("skills", "remove"), Assert.Single(question.Locks));
                Assert.Equal(InstallRouteAction.Remove, question.Routes.Single(row => row.Id == "skills").Action);
                Assert.Equal(installed ? InstallRouteAction.Add : InstallRouteAction.Remove, question.Routes.Single(row => row.Id == "guidance").Action);
                return ValueTask.FromResult(CliPromptReply<ImmutableArray<InstallRouteSelection>>.Answered(
                    question.Routes.Select(row => row.Id == "guidance" ? row with { Action = InstallRouteAction.GitIgnore } : row).Reverse().ToImmutableArray()));
            }, (question, _, _) => ValueTask.FromResult(CliPromptReply<string>.Answered(question.InitialForm)));
        var request = workspace.Request(automatic: false, allowsInteractiveConfirmation: true) with
        {
            Setup = new(installed, InstallPreset.Custom, [new("skills", InstallRouteAction.Remove)]),
        };
        var result = await new InstallSetupResolver(new PhysicalPathResolver(), interaction).ResolveAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(1, calls);
        Assert.Null(result.Boundary);
        var configuration = Assert.IsType<InstallConfiguration>(result.Configuration);
        Assert.Equal(InstallConfigurationChoices.RouteIds, configuration.Routes.Select(row => row.Id));
        Assert.Equal(InstallRouteAction.GitIgnore, configuration.Routes.Single(row => row.Id == "guidance").Action);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Custom setup rejects missing duplicate unknown changed fixed and undefined route answers")]
    [InlineData("missing"), InlineData("duplicate"), InlineData("unknown"), InlineData("fixed"), InlineData("undefined")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task CustomRejectsInvalidAnswer(string scenario)
    {
        using var workspace = InstallOperationWorkspace.Create("install-custom-invalid-answer");
        var before = workspace.SnapshotHashes();
        var interaction = new InstallSetupInteraction((_, _, _) => throw new InvalidOperationException("The preset is explicit."),
            (question, _, _) =>
            {
                var rows = question.Routes;
                rows = scenario switch
                {
                    "missing" => rows.RemoveAt(0),
                    "duplicate" => rows.SetItem(1, rows[0]),
                    "unknown" => rows.SetItem(1, new("unknown", InstallRouteAction.Add)),
                    "fixed" => rows.SetItem(0, rows[0] with { Action = InstallRouteAction.Remove }),
                    "undefined" => rows.SetItem(1, rows[1] with { Action = (InstallRouteAction)99 }),
                    _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
                };
                return ValueTask.FromResult(CliPromptReply<ImmutableArray<InstallRouteSelection>>.Answered(rows));
            }, (_, _, _) => throw new InvalidOperationException("Invalid routes cannot reach frontmatter selection."));
        var request = workspace.Request(automatic: false, allowsInteractiveConfirmation: true) with
        {
            Setup = new(false, InstallPreset.Custom, [new("directives", InstallRouteAction.Add)]),
        };
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await new InstallSetupResolver(new PhysicalPathResolver(), interaction).ResolveAsync(request, TestContext.Current.CancellationToken));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Custom cancellation and unavailable answers stop before frontmatter and writes")]
    [InlineData(true), InlineData(false), Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task CustomNonAnswerStops(bool cancelled)
    {
        using var workspace = InstallOperationWorkspace.Create("install-custom-cancelled");
        var before = workspace.SnapshotHashes();
        var interaction = new InstallSetupInteraction((_, _, _) => throw new InvalidOperationException("The preset is explicit."),
            (_, _, _) => ValueTask.FromResult(cancelled ? CliPromptReply<ImmutableArray<InstallRouteSelection>>.Cancelled()
                : CliPromptReply<ImmutableArray<InstallRouteSelection>>.Unavailable()),
            (_, _, _) => throw new InvalidOperationException("A non-answer cannot reach frontmatter selection."));
        var request = workspace.Request(automatic: false, allowsInteractiveConfirmation: true) with { Setup = new(false, InstallPreset.Custom, []) };
        var result = await new InstallSetupResolver(new PhysicalPathResolver(), interaction).ResolveAsync(request, TestContext.Current.CancellationToken);
        Assert.True(result.Cancelled);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

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
            (question, _, _) => ValueTask.FromResult(CliPromptReply<string>.Answered(question.InitialForm)));

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
