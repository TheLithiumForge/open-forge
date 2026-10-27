using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Context;
using OpenForge.Cli.Core.Commands.Context.Models.Request;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Shared.Graph;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Configuration;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Content;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Configuration;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Diagnostics;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

[Trait("Feature", "install-outside-preservation"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class InstallOutsideLifecycleIntegrationTests
{
    private const string Outside = "docs/team";
    private const string CustomRoot = "knowledge/main";
    private const string IndependentFile = "CONTRIBUTING.md";
    private static readonly string Anchor = Outside + "/" + SourceFormClassifier.IndexFileName;
    private const string OutsideText = "# Authored compatibility catalogue\r\n\r\n## Entries\r\n\r\n- none - No entries - #Empty\r\n";
    private const string IndependentText = "# Independent contribution rules\n\nRetain these bytes.\n";

    [Theory(DisplayName = "Install configure and owned form conversion preserve outside rows registrations and exact unlisted state")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(WorkspaceLayout.DefaultMainRoot, true)]
    [InlineData(CustomRoot, false), InlineData(CustomRoot, true)]
    public async Task PreservesOutsideAcrossLifecycle(string mainRoot, bool listed)
    {
        using var workspace = InstallOperationWorkspace.Create(nameof(PreservesOutsideAcrossLifecycle), mainRoot);
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, $$"""
            {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},
             "{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{mainRoot}}",
             "{{WorkspaceSettingsDefinitions.OutsideRoutesProperty}}":["{{Outside}}"],
             "{{WorkspaceSettingsDefinitions.RootsProperty}}":["{{IndependentFile}}"],
             "independent":{"retain":"opaque"}
            }
            """);
        workspace.WriteText(Anchor, OutsideText);
        workspace.WriteText(IndependentFile, IndependentText);
        var operation = InstallFrontmatterFixture.Operation(workspace);
        var installed = await operation.ExecuteAsync(await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Scoped),
            TestContext.Current.CancellationToken);
        Assert.True(installed.Status == CliSemanticStatus.Complete, InstallFixtureDiagnostics.Failure(installed));
        var loader = mainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
        var text = await workspace.ReadTextAsync(loader, TestContext.Current.CancellationToken);
        var document = new MarkdownDocumentParser().Parse(text);
        var content = Assert.IsType<Core.Framework.Documents.Markdown.Models.MarkdownTextSpan>(document.GeneratedRegion.EntriesBlock?.Span);
        var rows = AuthoredRows(mainRoot, listed);
        var existing = SourceGeneratedEntriesParser.Parse(document);
        Assert.Equal(SourceGeneratedEntriesState.Complete, existing.State);
        var originalRows = existing.Entries.Select(entry => text[entry.Span.Start..entry.Span.End]).ToArray();
        var mixed = string.Join('\n', new[] { rows[0] }.Concat(originalRows).Concat(rows.Skip(1))) + "\n";
        workspace.ReplaceInstalledText(loader, text[..content.Start] + mixed + text[content.End..]);
        var seeded = File.ReadAllBytes(workspace.Combine(loader));
        var identity = new FrameworkContentIdentity();
        Assert.Equal(identity.ReadSourceFingerprint(Encoding.UTF8.GetBytes(text)).Sha256, identity.ReadSourceFingerprint(seeded).Sha256);
        Assert.Equal(existing.Entries.Count + rows.Length, SourceGeneratedEntriesParser.Parse(
            new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(seeded))).Entries.Count);
        var startup = await StartupAsync(workspace, listed, mainRoot);
        var unchanged = Snapshot(workspace);
        var unchangedDirectories = Directories(workspace);
        foreach (var mode in new[] { InstallMode.DryRun, InstallMode.Apply })
        {
            var refused = await operation.ExecuteAsync(workspace.Request(mode), TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Blocked, refused.Status);
            Assert.Contains(refused.Findings, finding => finding.Code == InstallFindingCode.ManagedDivergence);
            AssertSnapshot(unchanged, workspace);
            Assert.Equal(unchangedDirectories, Directories(workspace));
        }

        foreach (var configure in new[] { true, false })
        {
            var request = workspace.Request() with
            {
                Configuration = configure ? new(true, InstallPreset.FullCore, InstallConfigurationChoices.Defaults(InstallPreset.FullCore)) : null,
            };
            var before = Snapshot(workspace);
            var directories = Directories(workspace);
            var preview = await operation.ExecuteAsync(workspace.Request(InstallMode.DryRun) with { Configuration = request.Configuration },
                TestContext.Current.CancellationToken);
            Assert.True(preview.Status == CliSemanticStatus.Complete, InstallFixtureDiagnostics.Failure(preview));
            AssertSnapshot(before, workspace);
            Assert.Equal(directories, Directories(workspace));
            var result = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
            Assert.True(result.Status == CliSemanticStatus.Complete, InstallFixtureDiagnostics.Failure(result));
            AssertPreserved(workspace, loader, rows, listed, originalRows);
            Assert.Equal(startup, await StartupAsync(workspace, listed, mainRoot));
            var completed = workspace.SnapshotHashes();
            var repeat = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
            Assert.True(repeat.Status == CliSemanticStatus.Complete, InstallFixtureDiagnostics.Failure(repeat));
            Assert.Empty(repeat.Facts.Effects);
            Assert.Equal(completed, workspace.SnapshotHashes());
        }

        var conversion = await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Root, installed: true);
        var conversionBefore = Snapshot(workspace);
        var conversionDirectories = Directories(workspace);
        var conversionPreview = await operation.ExecuteAsync(workspace.Request(InstallMode.DryRun) with { Frontmatter = conversion.Frontmatter },
            TestContext.Current.CancellationToken);
        Assert.True(conversionPreview.Status == CliSemanticStatus.Complete, InstallFixtureDiagnostics.Failure(conversionPreview));
        AssertSnapshot(conversionBefore, workspace);
        Assert.Equal(conversionDirectories, Directories(workspace));
        var converted = await operation.ExecuteAsync(conversion, TestContext.Current.CancellationToken);
        Assert.True(converted.Status == CliSemanticStatus.Complete, InstallFixtureDiagnostics.Failure(converted));
        AssertPreserved(workspace, loader, rows, listed, originalRows);
        Assert.Equal(startup, await StartupAsync(workspace, listed, mainRoot));
        var final = workspace.SnapshotHashes();
        var finalRepeat = await operation.ExecuteAsync(await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Root, installed: true),
            TestContext.Current.CancellationToken);
        Assert.True(finalRepeat.Status == CliSemanticStatus.Complete, InstallFixtureDiagnostics.Failure(finalRepeat));
        Assert.Empty(finalRepeat.Facts.Effects);
        Assert.Equal(final, workspace.SnapshotHashes());
    }

    private static string[] AuthoredRows(string mainRoot, bool listed)
    {
        var prefix = Path.GetRelativePath(mainRoot, Outside).Replace('\\', '/');
        var file = Path.GetRelativePath(mainRoot, IndependentFile).Replace('\\', '/');
        var rows = new List<string>();
        if (listed)
        {
            rows.Add($"- [First authored team]({prefix}/{SourceFormClassifier.IndexFileName}) - #{SourceLoadingClosureResolver.LoadNowTag} #First - applies to `src/**`");
            rows.Add($"- [Second authored team]({prefix}/./{SourceFormClassifier.IndexFileName}) - #{SourceLoadingClosureResolver.KeepInMindTag} #Second - applies to `tests/**`");
            rows.Add($"- [First authored team]({prefix}/{SourceFormClassifier.IndexFileName}) - #{SourceLoadingClosureResolver.LoadNowTag} #First - applies to `src/**`");
            rows.Add($"- [Stale outside target]({prefix}/missing.md) - #Independent");
            var canonical = Path.GetRelativePath(mainRoot, WorkspaceRoutePath.CanonicalEntrypointPath(Outside)).Replace('\\', '/');
            rows.Add($"- [Stale canonical entrypoint]({canonical}) - #Independent");
        }
        rows.Add($"- [Independent file include]({file}) - #Independent");
        return rows.ToArray();
    }

    private static void AssertPreserved(InstallOperationWorkspace workspace, string loader, string[] rows, bool listed, string[] primaryRows)
    {
        Assert.Equal(Encoding.UTF8.GetBytes(OutsideText), File.ReadAllBytes(workspace.Combine(Anchor)));
        Assert.Equal(Encoding.UTF8.GetBytes(IndependentText), File.ReadAllBytes(workspace.Combine(IndependentFile)));
        Assert.False(workspace.Exists(WorkspaceRoutePath.CanonicalEntrypointPath(Outside)));
        Assert.False(workspace.Exists(Outside + "/missing.md"));
        using var settings = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath)));
        Assert.Equal(Outside, Assert.Single(settings.RootElement.GetProperty(WorkspaceSettingsDefinitions.OutsideRoutesProperty).EnumerateArray()).GetString());
        Assert.Equal(IndependentFile, Assert.Single(settings.RootElement.GetProperty(WorkspaceSettingsDefinitions.RootsProperty).EnumerateArray()).GetString());
        Assert.Equal("opaque", settings.RootElement.GetProperty("independent").GetProperty("retain").GetString());
        var text = File.ReadAllText(workspace.Combine(loader));
        var parsed = new MarkdownDocumentParser().Parse(text);
        var entries = SourceGeneratedEntriesParser.Parse(parsed);
        Assert.Equal(SourceGeneratedEntriesState.Complete, entries.State);
        var retained = entries.Entries.Where(entry => entry.Description is "First authored team" or "Second authored team" or "Stale outside target"
                or "Stale canonical entrypoint" or "Independent file include")
            .Select(entry => text[entry.Span.Start..entry.Span.End]).ToArray();
        Assert.Equal(rows, retained);
        Assert.Equal(listed ? 6 : 1, retained.Length);
        Assert.Equal(primaryRows, entries.Entries.Where(entry => !rows.Contains(text[entry.Span.Start..entry.Span.End], StringComparer.Ordinal))
            .Select(entry => text[entry.Span.Start..entry.Span.End]));
    }

    private static Dictionary<string, byte[]> Snapshot(InstallOperationWorkspace workspace)
        => Directory.EnumerateFiles(workspace.PhysicalPath, "*", SearchOption.AllDirectories)
            .ToDictionary(path => Path.GetRelativePath(workspace.PhysicalPath, path), File.ReadAllBytes, StringComparer.Ordinal);

    private static async Task<string[]> StartupAsync(InstallOperationWorkspace workspace, bool listed, string mainRoot)
    {
        var request = new ContextRequest(workspace.Workspace, [], false, new ContextContentSelection([], []), ContextLinkExpansion.None, null, CliDetail.Minimal)
        {
            WorkingPaths = ["src/Planned.cs"],
            WorkingPathsSupplied = true,
        };
        var context = await ContextOperationFactory.Create().ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.True(context.Status is CliSemanticStatus.Complete or CliSemanticStatus.Incomplete);
        Assert.Equal(listed, context.Sources.Any(source => source.Path == Anchor));
        if (listed)
        {
            var source = Assert.Single(context.Sources, source => source.Path == Anchor);
            Assert.Equal(ContextMetadataState.Missing, source.Metadata.State);
            Assert.Null(source.Metadata.Description);
            Assert.Empty(source.Metadata.Tags);
            var graph = await new ContextGraphBuilder().BuildAsync(workspace.Workspace, TestContext.Current.CancellationToken);
            var logical = Assert.IsType<SourceLogicalSource>(graph.Catalogue.FindByPath(Anchor));
            var row = Assert.IsType<SourceGeneratedEntry>(logical.IncludedEntry);
            Assert.Equal("First authored team", row.Description);
            Assert.Equal(new[] { SourceLoadingClosureResolver.LoadNowTag, "First" }, row.Tags);
            Assert.Equal(mainRoot + "/" + WorkspaceLayoutNames.LoaderFileName, logical.IncludedParentPath);
            Assert.Equal(ContextApplicabilityState.Matched, source.Applicability?.State);
            Assert.Contains(source.InclusionReasons, reason => reason.Kind == ContextInclusionReasonKind.LoadNow
                && reason.Source?.Path == mainRoot + "/" + WorkspaceLayoutNames.LoaderFileName);
        }
        // Mixed primary and authored slots may move. Their row orders are checked separately.
        return context.Sources.Select(source => source.Path).Order(StringComparer.Ordinal).ToArray();
    }

    private static string[] Directories(InstallOperationWorkspace workspace)
        => Directory.GetDirectories(workspace.PhysicalPath, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray();

    private static void AssertSnapshot(Dictionary<string, byte[]> before, InstallOperationWorkspace workspace)
    {
        var after = Snapshot(workspace);
        Assert.Equal(before.Keys.Order(StringComparer.Ordinal), after.Keys.Order(StringComparer.Ordinal));
        foreach (var pair in before) Assert.Equal(pair.Value, after[pair.Key]);
    }
}
