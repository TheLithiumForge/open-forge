using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

public sealed class InstallPresetEstablishmentOwnershipIntegrationTests
{
    [Theory(DisplayName = "First preset establishment owns preserved canonical bases and safely installed replacements"), InlineData(false), InlineData(true)]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task PreservedCanonicalBaseRemainsManaged(bool matchesPayload)
    {
        using var workspace = InstallOperationWorkspace.Create("install-preset-establishment-receipts");
        const string path = ".agents/patterns/_patterns.md";
        const string companion = ".agents/patterns/_patterns.overwrite.md";
        var payload = EmbeddedFrameworkPayloadReader.Read().Payload ?? throw new InvalidOperationException("Payload is unavailable.");
        var asset = payload.Find(path) ?? throw new InvalidOperationException("Patterns payload is unavailable.");
        var original = matchesPayload ? Encoding.UTF8.GetString(asset.Bytes.AsSpan()) : "# Local patterns\n\nKeep my pattern notes.\n";
        workspace.WriteText(path, original);
        var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Unavailable(), workspace.LockStoreRoot);
        var request = workspace.Request() with { Setup = new(false, InstallPreset.Essentials, []) };

        var applied = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        var preserved = File.ReadAllText(workspace.Combine(companion));
        if (matchesPayload)
        {
            var generatedHeading = original.IndexOf("## Entries", StringComparison.Ordinal);
            Assert.True(generatedHeading >= 0);
            Assert.StartsWith(original[..generatedHeading], preserved, StringComparison.Ordinal);
            Assert.DoesNotContain("## Entries", preserved, StringComparison.Ordinal);
            Assert.Equal(Encoding.UTF8.GetBytes(original), File.ReadAllBytes(workspace.Combine(path)));
        }
        else Assert.StartsWith(original, preserved, StringComparison.Ordinal);
        Assert.Equal(matchesPayload ? InstallResultRecoveryState.NotRequired : InstallResultRecoveryState.Removed, applied.Facts.Recovery.State);
        Assert.Equal(!matchesPayload, applied.Facts.Effects.Any(effect => effect.Path == path));
        var framework = await ReadFrameworkAsync(workspace);
        Assert.Contains(path, framework.Paths);
        Assert.Contains(new OwnedRegion(path, "entries"), framework.Regions);
        Assert.DoesNotContain(companion, framework.Paths);
        Assert.DoesNotContain(".agents/open-forge.json", framework.Paths);
        Assert.DoesNotContain(".gitignore", framework.Paths);
        var current = File.ReadAllText(workspace.Combine(path));
        Assert.Contains("# Patterns", current, StringComparison.Ordinal);
        workspace.ReplaceInstalledText(path, current.Replace("# Patterns", "# Authored patterns", StringComparison.Ordinal));
        var blocked = await operation.ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, blocked.Status);
        Assert.Contains(blocked.Findings, finding => finding.Code == InstallFindingCode.ManagedDivergence);
        await AssertUpdateCanPlanAsync(workspace, path);
    }

    [Fact(DisplayName = "A reachable first-preset force replacement establishes a provider-region receipt and Update boundary")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task EligibleManagedHostForceKeepsOwnership()
    {
        using var workspace = InstallOperationWorkspace.Create("install-preset-force-host");
        var payload = EmbeddedFrameworkPayloadReader.Read().Payload ?? throw new InvalidOperationException("Payload is unavailable.");
        var agent = payload.Find("AGENTS.md") ?? throw new InvalidOperationException("Agent payload is unavailable.");
        var original = Encoding.UTF8.GetString(agent.Bytes.AsSpan());
        workspace.WriteText("AGENTS.md", original.Replace("Before starting a task", "Before starting another task", StringComparison.Ordinal));
        var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Unavailable(), workspace.LockStoreRoot);
        var request = workspace.Request() with { Setup = new(false, InstallPreset.Essentials, []) };
        var before = workspace.SnapshotHashes();
        var blocked = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, blocked.Status);
        Assert.Contains(blocked.Findings, finding => finding.Code == InstallFindingCode.TargetOccupied);
        Assert.Empty(blocked.Facts.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());

        var applied = await operation.ExecuteAsync(workspace.Request(force: true) with { Setup = request.Setup }, TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal(InstallResultRecoveryState.Removed, applied.Facts.Recovery.State);
        Assert.Contains(applied.Facts.Effects, effect => effect.Path == "AGENTS.md"
            && effect.Kind == InstallEffectKind.ManagedRegion && effect.Action == InstallEffectAction.Replace);
        var framework = await ReadFrameworkAsync(workspace);
        Assert.Contains(new OwnedRegion("AGENTS.md", "open-forge"), framework.Regions);
        Assert.DoesNotContain("AGENTS.md", framework.Paths);
        Assert.DoesNotContain(".agents/open-forge.json", framework.Paths);
        Assert.DoesNotContain(".gitignore", framework.Paths);
        workspace.ReplaceInstalledText("AGENTS.md", File.ReadAllText(workspace.Combine("AGENTS.md"))
            .Replace("Before starting a task", "Before starting another task", StringComparison.Ordinal));
        var repeat = await operation.ExecuteAsync(workspace.Request(force: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, repeat.Status);
        Assert.Contains(repeat.Findings, finding => finding.Code == InstallFindingCode.ManagedDivergence);
        await AssertUpdateCanPlanAsync(workspace, "AGENTS.md");
    }

    private static async Task<FrameworkOwnership> ReadFrameworkAsync(InstallOperationWorkspace workspace)
        => Assert.IsType<FrameworkOwnership>((await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(),
            workspace.Workspace, TestContext.Current.CancellationToken)).Document.Framework);

    private static async Task AssertUpdateCanPlanAsync(InstallOperationWorkspace workspace, string path)
    {
        var update = await CliHostCapture.RunAsync(["update", "--force", "--automatic", "--dry-run", "--format", "json", "--detail", "full"], workspace.PhysicalPath);
        Assert.True(update.ExitCode == 0, update.Output + update.Error);
        using var document = JsonDocument.Parse(update.Output);
        Assert.Contains(document.RootElement.GetProperty("effects").EnumerateArray(), effect => effect.GetProperty("path").GetString() == path);
    }
}
