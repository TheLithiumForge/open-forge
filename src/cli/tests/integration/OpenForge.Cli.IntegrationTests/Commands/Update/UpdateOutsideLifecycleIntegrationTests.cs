using System.IO.Compression;
using System.Text;
using OpenForge.Cli.Core.Commands.Install.Shared.Configuration;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Shared.Planning;
using OpenForge.Cli.Core.Commands.Update.Shared.Recovery;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Install;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Configuration;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Diagnostics;
using OpenForge.Cli.IntegrationTests.Commands.Update.Shared.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Update;

[Trait("Feature", "update-outside-preservation"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class UpdateOutsideLifecycleIntegrationTests
{
    private const string CustomRoot = "knowledge/main";
    private const string Outside = "docs/team";
    private static readonly string Anchor = Outside + "/" + SourceFormClassifier.IndexFileName;
    private const string IndependentFile = "CONTRIBUTING.md";
    private const string OutsideText = "# Independent compatibility catalogue\r\n\r\n## Entries\r\n\r\n- none - No entries - #Empty\r\n";
    private const string IndependentText = "# Independent contribution rules\n";

    [Theory(DisplayName = "Root Update retains outside policy unlisted state settings and literal recovery input")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(WorkspaceLayout.DefaultMainRoot, true)]
    [InlineData(CustomRoot, false), InlineData(CustomRoot, true)]
    public async Task PreservesOutsideRowsAndRecovery(string mainRoot, bool listed)
    {
        using var workspace = InstallOperationWorkspace.Create(nameof(PreservesOutsideRowsAndRecovery), mainRoot);
        var (rows, primaryRows) = await SeedAsync(workspace, mainRoot, listed);
        var loader = mainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
        var settings = File.ReadAllBytes(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath));
        var originalLoader = File.ReadAllBytes(workspace.Combine(loader));
        var operation = UpdateOperationFactory.Create(UpdateInteractionTestSupport.Unavailable(), workspace.LockStoreRoot);
        var before = workspace.SnapshotHashes();
        var directories = Directories(workspace);
        var preview = await operation.ExecuteAsync(Request(workspace, UpdateMode.DryRun), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(directories, Directories(workspace));

        var build = await UpdatePlanBuilder.Create().BuildExecutionAsync(Request(workspace), TestContext.Current.CancellationToken);
        var execution = Assert.IsType<UpdatePlanExecution>(build.Execution);
        var acquired = await new WorkspaceLockManager(workspace.LockStoreRoot).AcquireAsync(
            new(workspace.Workspace, UpdateDefinitions.CommandIdentity, Guid.NewGuid()), TestContext.Current.CancellationToken);
        await using (var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease))
        {
            var prepared = await UpdateRecoveryOperation.PrepareAsync(execution, lease.Request.OperationId, TestContext.Current.CancellationToken);
            Assert.Equal(RecoveryBundlePreparationState.Prepared, prepared.State);
            var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
            try
            {
                var entry = Assert.Single(preparation.Entries, entry => entry.TargetPath == loader);
                using var archive = ZipFile.OpenRead(preparation.BundlePath);
                using var stream = Assert.IsType<ZipArchiveEntry>(archive.GetEntry(Assert.IsType<string>(entry.PriorPayload))).Open();
                using var bytes = new MemoryStream();
                await stream.CopyToAsync(bytes, TestContext.Current.CancellationToken);
                Assert.Equal(originalLoader, bytes.ToArray());
                Assert.DoesNotContain(preparation.Entries, entry => entry.TargetPath.StartsWith(Outside + "/", StringComparison.Ordinal));
                Assert.Equal(before, workspace.SnapshotHashes());
            }
            finally
            {
                File.Delete(preparation.BundlePath);
            }
        }

        var applied = await operation.ExecuteAsync(Request(workspace), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal(settings, File.ReadAllBytes(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath)));
        AssertPreserved(workspace, loader, rows, primaryRows);
        var completed = workspace.SnapshotHashes();
        var repeat = await operation.ExecuteAsync(Request(workspace), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, repeat.Status);
        Assert.Empty(repeat.Effects);
        Assert.Equal(completed, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Root Update refuses settings Loader or outside physical drift before content effects")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, 0), InlineData(WorkspaceLayout.DefaultMainRoot, 1), InlineData(WorkspaceLayout.DefaultMainRoot, 2)]
    [InlineData(CustomRoot, 0), InlineData(CustomRoot, 1), InlineData(CustomRoot, 2)]
    public async Task RefusesRetainedInputDrift(string mainRoot, int change)
    {
        using var workspace = InstallOperationWorkspace.Create(nameof(RefusesRetainedInputDrift), mainRoot);
        await SeedAsync(workspace, mainRoot, listed: true);
        var loader = mainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
        var changed = change switch
        {
            0 => WorkspaceSettingsDefinitions.RelativePath,
            1 => loader,
            2 => Outside,
            _ => throw new ArgumentOutOfRangeException(nameof(change)),
        };
        var before = workspace.SnapshotHashes();
        var observed = false;
        var operation = UpdateOperationFactory.Create(UpdateInteractionTestSupport.Confirmation(observe: (_, _) =>
        {
            observed = true;
            if (change == 2)
            {
                File.Delete(workspace.Combine(Anchor));
                Directory.Delete(workspace.Combine(Outside));
                File.WriteAllText(workspace.Combine(Outside), "Concurrent outside boundary\n");
            }
            else
            {
                workspace.ReplaceInstalledText(changed, change == 0 ? Settings(mainRoot, registered: false) : "# Concurrent Loader input\n");
            }
        }), workspace.LockStoreRoot);
        try
        {
            var result = await operation.ExecuteAsync(new(workspace.Workspace, UpdateMode.Apply, false, false, false, true),
                TestContext.Current.CancellationToken);
            Assert.True(observed);
            Assert.Equal(CliSemanticStatus.Blocked, result.Status);
            var after = workspace.SnapshotHashes();
            Assert.Equal(before.Where(pair => pair.Key != changed && !pair.Key.StartsWith(changed + "/", StringComparison.Ordinal)),
                after.Where(pair => pair.Key != changed && !pair.Key.StartsWith(changed + "/", StringComparison.Ordinal)));
        }
        finally
        {
            if (change == 2 && observed)
            {
                File.Delete(workspace.Combine(Outside));
                Directory.CreateDirectory(workspace.Combine(Outside));
                File.WriteAllText(workspace.Combine(Anchor), OutsideText);
            }
        }
    }

    private static async Task<(string[] Rows, string[] PrimaryRows)> SeedAsync(InstallOperationWorkspace workspace, string mainRoot, bool listed)
    {
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, Settings(mainRoot, registered: true));
        workspace.WriteText(Anchor, OutsideText);
        workspace.WriteText(IndependentFile, IndependentText);
        var installed = await InstallFrontmatterFixture.Operation(workspace).ExecuteAsync(
            await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Scoped), TestContext.Current.CancellationToken);
        Assert.True(installed.Status == CliSemanticStatus.Complete, InstallFixtureDiagnostics.Failure(installed));
        var loader = mainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
        var text = File.ReadAllText(workspace.Combine(loader));
        var document = new MarkdownDocumentParser().Parse(text);
        var span = Assert.IsType<Core.Framework.Documents.Markdown.Models.MarkdownTextSpan>(document.GeneratedRegion.EntriesBlock?.Span);
        var prefix = Path.GetRelativePath(mainRoot, Outside).Replace('\\', '/');
        var file = Path.GetRelativePath(mainRoot, IndependentFile).Replace('\\', '/');
        var rows = new List<string>();
        if (listed)
        {
            rows.Add($"- [First outside row]({prefix}/{SourceFormClassifier.IndexFileName}) - #{SourceLoadingClosureResolver.LoadNowTag} #First - applies to `src/**`");
            rows.Add($"- [Second outside row]({prefix}/./{SourceFormClassifier.IndexFileName}) - #{SourceLoadingClosureResolver.KeepInMindTag} #Second - applies to `tests/**`");
            rows.Add(rows[0]);
            rows.Add($"- [Stale outside row]({prefix}/missing.md) - #Independent");
            var canonical = Path.GetRelativePath(mainRoot, WorkspaceRoutePath.CanonicalEntrypointPath(Outside)).Replace('\\', '/');
            rows.Add($"- [Stale canonical row]({canonical}) - #Independent");
        }
        rows.Add($"- [Independent include]({file}) - #Independent");
        var primary = SourceGeneratedEntriesParser.Parse(document).Entries.Select(entry => text[entry.Span.Start..entry.Span.End]).ToArray();
        var mixed = string.Join('\n', new[] { rows[0] }.Concat(primary).Concat(rows.Skip(1))) + "\n";
        workspace.ReplaceInstalledText(loader, text[..span.Start] + mixed + text[span.End..]);
        return (rows.ToArray(), primary);
    }

    private static string Settings(string mainRoot, bool registered)
        => $$"""
            {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},
             "{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{mainRoot}}",
             "{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":[{{(registered ? $"\"{Outside}\"" : string.Empty)}}],
             "{{WorkspaceSettingsDefinitions.RootsProperty}}":["{{IndependentFile}}"],
             "independent":{"retain":"opaque"}
            }
            """;

    private static UpdateRequest Request(InstallOperationWorkspace workspace, UpdateMode mode = UpdateMode.Apply)
        => new(workspace.Workspace, mode, false, false, true, false);

    private static void AssertPreserved(InstallOperationWorkspace workspace, string loader, string[] rows, string[] primaryRows)
    {
        Assert.Equal(Encoding.UTF8.GetBytes(OutsideText), File.ReadAllBytes(workspace.Combine(Anchor)));
        Assert.Equal(Encoding.UTF8.GetBytes(IndependentText), File.ReadAllBytes(workspace.Combine(IndependentFile)));
        Assert.False(workspace.Exists(WorkspaceRoutePath.CanonicalEntrypointPath(Outside)));
        var text = File.ReadAllText(workspace.Combine(loader));
        var parsed = SourceGeneratedEntriesParser.Parse(new MarkdownDocumentParser().Parse(text));
        Assert.Equal(SourceGeneratedEntriesState.Complete, parsed.State);
        var retained = parsed.Entries.Where(entry => entry.Description is "First outside row" or "Second outside row" or "Stale outside row"
                or "Stale canonical row" or "Independent include")
            .Select(entry => text[entry.Span.Start..entry.Span.End]);
        Assert.Equal(rows, retained);
        Assert.Equal(primaryRows, parsed.Entries.Where(entry => !rows.Contains(text[entry.Span.Start..entry.Span.End], StringComparer.Ordinal))
            .Select(entry => text[entry.Span.Start..entry.Span.End]));
    }

    private static string[] Directories(InstallOperationWorkspace workspace)
        => Directory.GetDirectories(workspace.PhysicalPath, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray();
}
