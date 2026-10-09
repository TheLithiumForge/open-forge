using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Distribution;
using System.Text;
using OpenForge.Cli.Core.Presentation.Install;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

public sealed class InstallChangeOutputSnapshotTests
{
    private static readonly CommandOutputRenderers<InstallResult> Renderers = CommandOutputRenderers<InstallResult>.From(InstallPresentation.Rendering);

    [Fact(DisplayName = "Configure adding Guidance and private Archived Memory describes five edits without replacements")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
    public async Task ConfigureGuidanceAndArchivedMemory()
    {
        using var workspace = InstallOperationWorkspace.Create("install-configure-change-output");
        var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(), workspace.LockStoreRoot);
        var installed = await operation.ExecuteAsync(workspace.Request() with
        { Setup = new(false, InstallPreset.Essentials, []) }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, installed.Status);
        var before = workspace.SnapshotHashes();
        var request = workspace.Request(InstallMode.DryRun) with
        {
            Setup = new(true, InstallPreset.Custom,
                [new("guidance", InstallRouteAction.Add), new("memory/archived", InstallRouteAction.GitIgnore)]),
        };
        var result = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(2, result.Facts.Effects.Count(effect => effect.Kind == InstallEffectKind.Directory));
        Assert.Equal(2, result.Facts.Effects.Count(effect => effect.Action == InstallEffectAction.Create && effect.Kind == InstallEffectKind.File));
        Assert.Equal(5, result.Facts.Effects.Count(effect => effect.Action == InstallEffectAction.Replace));
        Assert.All(result.Facts.Effects.Where(effect => effect.Kind == InstallEffectKind.File && effect.Action == InstallEffectAction.Replace),
            effect => Assert.Equal(InstallEffectContentChange.PreservedContent, effect.ContentChange));
        InstallConfirmationFacts? confirmation = null;
        var cancelled = await InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(accepted: false,
            observe: (_, facts) => confirmation = facts), workspace.LockStoreRoot)
            .ExecuteAsync(workspace.Request(automatic: false, allowsInteractiveConfirmation: true) with { Setup = request.Setup },
                TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Interrupted, cancelled.Status);
        Assert.Equal(0, Assert.IsType<InstallConfirmationFacts>(confirmation).ReplacementCount);
        Assert.Equal(before, workspace.SnapshotHashes());
        Renderers.MatchDetails(result, "configure-guidance-archived");
    }

    [Fact(DisplayName = "Install preview distinguishes host preservation from whole-file replacement")]
    [Trait("Feature", "install-presentation"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
    public async Task ExistingContentPreview()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var replace in new[] { false, true })
        {
            using var workspace = InstallOperationWorkspace.Create("install-existing-change-output");
            if (replace)
            {
                const string path = ".agents/memory/_memory.md";
                var payload = EmbeddedFrameworkPayloadReader.Read().Payload
                    ?? throw new InvalidOperationException("The embedded Framework payload is unavailable.");
                var asset = payload.Find(path) ?? throw new InvalidOperationException("The Memory entrypoint is unavailable.");
                var text = Encoding.UTF8.GetString(asset.Bytes.AsSpan());
                workspace.WriteText(path, text.Replace("crystallized/_crystallized.md", "authored.md", StringComparison.Ordinal));
            }
            else
                workspace.WriteText("AGENTS.md", "# Authored content\n\nKeep this text.\n");
            var before = workspace.SnapshotHashes();
            var result = await InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(), workspace.LockStoreRoot)
                .ExecuteAsync(workspace.Request(InstallMode.DryRun, force: replace), TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal(before, workspace.SnapshotHashes());
            if (replace)
                Assert.Contains(result.Facts.Effects, effect => effect.Kind == InstallEffectKind.File && effect.Action == InstallEffectAction.Replace
                    && effect.ContentChange == InstallEffectContentChange.WholeFile);
            Renderers.MatchDetails(result, replace ? "force-occupant" : "existing-host", snapshotCollector: snapshots);
        }
        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }

    [Fact(DisplayName = "Configure metadata preview names conversions without replacement authority")]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
    public async Task ConfigureMetadataPreview()
    {
        using var workspace = InstallOperationWorkspace.Create("install-conversion-change-output");
        var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(), workspace.LockStoreRoot);
        Assert.Equal(CliSemanticStatus.Complete,
            (await operation.ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken)).Status);
        var before = workspace.SnapshotHashes();
        var setup = new InstallSetupInput(true, null, []) { Frontmatter = FrontmatterForm.Scoped };
        var preview = await operation.ExecuteAsync(workspace.Request(InstallMode.DryRun) with { Setup = setup }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Contains(preview.Facts.Effects, effect => effect.ContentChange == InstallEffectContentChange.FrontmatterConversion);
        Assert.All(preview.Facts.Effects.Where(effect => effect.ContentChange == InstallEffectContentChange.FrontmatterConversion),
            effect => { Assert.Equal(InstallEffectKind.File, effect.Kind); Assert.Equal(InstallEffectAction.Replace, effect.Action); });
        InstallConfirmationFacts? confirmation = null;
        var cancelled = await InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(accepted: false,
            observe: (_, facts) => confirmation = facts), workspace.LockStoreRoot)
            .ExecuteAsync(workspace.Request(automatic: false, allowsInteractiveConfirmation: true) with { Setup = setup }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Interrupted, cancelled.Status);
        Assert.Equal(0, Assert.IsType<InstallConfirmationFacts>(confirmation).ReplacementCount);
        Assert.Equal(before, workspace.SnapshotHashes());
        Renderers.MatchDetails(preview, "configure-metadata");
    }
}
