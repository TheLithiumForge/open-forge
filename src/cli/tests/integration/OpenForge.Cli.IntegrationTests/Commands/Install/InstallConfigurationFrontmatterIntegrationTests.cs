using System.Text;
using System.IO.Compression;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Shared.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Extensions.Embedded;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Configuration;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

public sealed class InstallConfigurationFrontmatterIntegrationTests
{
    private const string Directives = ".agents/directives/_directives.md";
    private const string Scoped = ".agents/memory/team/working/_working.md";
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Configure converts owned Framework, scoped copies and bundled Extension files together"), InlineData(false), InlineData(true)]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Integration")]
    public async Task ConfigureConvertsFrameworkScopedCopiesAndExtensions(bool withPreset)
    {
        using var workspace = InstallOperationWorkspace.Create("install-convert-owned");
        await InstallFrontmatterFixture.SeedScopedAsync(workspace);
        using var sources = new InstallFrontmatterSources(workspace);
        var extensionPaths = await SeedAdditionalClaimsAsync(workspace, sources);
        var ownershipBefore = File.ReadAllBytes(workspace.Combine(InstallOperationWorkspace.OwnershipPath));
        var request = await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Root, installed: true);
        if (withPreset) request = request with { Configuration = new(true, InstallPreset.FullCore, InstallConfigurationChoices.Defaults(InstallPreset.FullCore)) };
        var result = await InstallFrontmatterFixture.Operation(workspace).ExecuteAsync(request, Token);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal("scoped", result.Frontmatter?.PreviousForm);
        Assert.Empty(Assert.IsType<InstallFrontmatter>(result.Frontmatter).Kept);
        foreach (var path in extensionPaths.Append(Scoped).Append(Directives))
        {
            Assert.DoesNotContain("open-forge:", File.ReadAllText(workspace.Combine(path)), StringComparison.Ordinal);
            Assert.Equal(InstallEffectKind.File, Assert.Single(result.Facts.Effects, effect => effect.Path == path).Kind);
        }
        Assert.Equal(ownershipBefore, File.ReadAllBytes(workspace.Combine(InstallOperationWorkspace.OwnershipPath)));
    }

    [Fact(DisplayName = "Configure retains edited whole files byte for byte and reports them as kept"), Trait("Feature", "install-frontmatter"), Trait("Evidence", "Integration")]
    public async Task ConfigureKeepsEditedFilesAndReportsThem()
    {
        using var workspace = InstallOperationWorkspace.Create("install-convert-edited");
        await InstallFrontmatterFixture.SeedScopedAsync(workspace);
        var edited = File.ReadAllText(workspace.Combine(Directives)) + "\nAuthored instruction.\n";
        workspace.ReplaceInstalledText(Directives, edited);
        var result = await InstallFrontmatterFixture.Operation(workspace).ExecuteAsync(
            await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Root, installed: true), Token);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var kept = Assert.Single(Assert.IsType<InstallFrontmatter>(result.Frontmatter).Kept);
        Assert.Equal(Directives, kept.Path);
        Assert.Equal(InstallFrontmatterKeptReason.Edited, kept.Reason);
        Assert.Equal(edited, File.ReadAllText(workspace.Combine(Directives)));
        Assert.DoesNotContain(result.Facts.Effects, effect => effect.Path == Directives);
    }

    [Fact(DisplayName = "Configure never rewrites unowned, excluded, Library, overwrite or region-only metadata"), Trait("Feature", "install-frontmatter"), Trait("Evidence", "Integration")]
    public async Task ConfigureNeverTouchesUserLibraryOrOverwriteFiles()
    {
        using var workspace = InstallOperationWorkspace.Create("install-convert-protected");
        workspace.WriteText(".agents/open-forge.json", """{"removedCategories":["maps"]}""");
        await InstallFrontmatterFixture.SeedScopedAsync(workspace);
        using var sources = new InstallFrontmatterSources(workspace);
        var paths = new[] { ".agents/directives/user.md", ".agents/directives/_directives.overwrite.md", ".agents/maps/_maps.md", ".agents/directives/library/note.md", ".agents/directives/region.md" };
        const string content = "---\nopen-forge:\n  description: Keep this authored source\n  tags: [Directive]\n---\n\n# Keep\n\nAuthored body.\n";
        foreach (var path in paths) sources.Add(path, content);
        var read = await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, Token);
        var framework = Assert.IsType<FrameworkOwnership>(read.Document.Framework);
        File.WriteAllBytes(workspace.Combine(InstallOperationWorkspace.OwnershipPath), WorkspaceOwnershipCodec.Write(read.Document with
        {
            Framework = framework with { Regions = framework.Regions.Add(new(paths[^1], "entries")) },
            Libraries = [new LibraryOwnership("notes", "vendor/notes", ".agents/directives/library", ["note.md"])],
        }));
        var before = paths.ToDictionary(path => path, path => File.ReadAllBytes(workspace.Combine(path)), StringComparer.Ordinal);
        var result = await InstallFrontmatterFixture.Operation(workspace).ExecuteAsync(
            await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Root, installed: true), Token);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        foreach (var path in paths)
        {
            Assert.Equal(before[path], File.ReadAllBytes(workspace.Combine(path)));
            Assert.DoesNotContain(result.Facts.Effects, effect => effect.Path == path);
        }
    }

    [Fact(DisplayName = "A header conversion and regenerated Entries form one whole-file effect"), Trait("Feature", "install-frontmatter"), Trait("Evidence", "Integration")]
    public async Task HeaderAndEntriesProduceOneEffect()
    {
        using var workspace = InstallOperationWorkspace.Create("install-convert-navigation");
        await InstallFrontmatterFixture.SeedScopedAsync(workspace);
        using var sources = new InstallFrontmatterSources(workspace);
        sources.Add(".agents/directives/new.md", "---\nopen-forge:\n  description: Added directive\n  tags: [Directive]\n---\n\n# Added\n");
        var request = await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Root, installed: true);
        var build = await new InstallPlanBuilder(new PhysicalPathResolver()).BuildAsync(request, Token);
        var plan = Assert.IsType<InstallPlan>(build.Plan);
        Assert.Empty(plan.Findings);
        var effect = Assert.Single(plan.TargetEffects, effect => effect.RelativePath == Directives);
        Assert.Equal(PlannedFileChangeKind.Replace, effect.Change.Kind);
        var result = await InstallFrontmatterFixture.Operation(workspace).ExecuteAsync(request, Token);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var bytes = File.ReadAllBytes(workspace.Combine(Directives));
        Assert.Equal(effect.Change.IntendedBytes.ToArray(), bytes);
        Assert.DoesNotContain("open-forge:", Encoding.UTF8.GetString(bytes), StringComparison.Ordinal);
        Assert.Contains("new.md", Encoding.UTF8.GetString(bytes), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Repeated configuration with already selected metadata has no effects"), Trait("Feature", "install-frontmatter"), Trait("Evidence", "Integration")]
    public async Task RepeatConfigurationHasNoEffects()
    {
        using var workspace = InstallOperationWorkspace.Create("install-convert-repeat");
        await InstallFrontmatterFixture.SeedScopedAsync(workspace);
        var operation = InstallFrontmatterFixture.Operation(workspace);
        Assert.Equal(CliSemanticStatus.Complete, (await operation.ExecuteAsync(await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Root, installed: true), Token)).Status);
        var before = workspace.SnapshotHashes();
        var repeat = await operation.ExecuteAsync(await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Root, installed: true), Token);
        Assert.Equal(CliSemanticStatus.Complete, repeat.Status);
        Assert.Empty(repeat.Facts.Effects);
        Assert.Null(repeat.Frontmatter?.PreviousForm);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Ownership changed after plan review prevents settings and every conversion"), Trait("Feature", "install-frontmatter"), Trait("Evidence", "Integration")]
    public async Task StaleOwnershipStopsAllWrites()
    {
        using var workspace = InstallOperationWorkspace.Create("install-convert-stale-ownership");
        await InstallFrontmatterFixture.SeedScopedAsync(workspace);
        var before = workspace.SnapshotHashes();
        var selection = (await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Root, installed: true)).Frontmatter;
        var operation = InstallFrontmatterFixture.Operation(workspace, InstallInteractionTestSupport.Confirmation(observe: (_, _) =>
        {
            var bytes = File.ReadAllBytes(workspace.Combine(InstallOperationWorkspace.OwnershipPath));
            File.WriteAllBytes(workspace.Combine(InstallOperationWorkspace.OwnershipPath), bytes.Concat(new byte[] { (byte)'\n' }).ToArray());
        }));
        var result = await operation.ExecuteAsync(new InstallRequest(workspace.Workspace, InstallMode.Apply, false, false, true) { Frontmatter = selection }, Token);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.All(result.Facts.Effects, effect => Assert.Equal(InstallEffectOutcome.NotStarted, effect.Outcome));
        foreach (var pair in before.Where(pair => pair.Key != InstallOperationWorkspace.OwnershipPath))
            Assert.Equal(pair.Value, workspace.SnapshotHashes()[pair.Key]);
    }

    [Fact(DisplayName = "A denied conversion retains recovery with original settings and every replaced file"), Trait("Feature", "install-frontmatter"), Trait("Evidence", "Integration")]
    public async Task InterruptedConversionRetainsCompleteRecovery()
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("This deterministic replacement failure requires Windows file sharing.");

        using var workspace = InstallOperationWorkspace.Create("install-convert-recovery");
        await InstallFrontmatterFixture.SeedScopedAsync(workspace);
        var request = await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Root, installed: true);
        var build = await new InstallPlanBuilder(new PhysicalPathResolver()).BuildAsync(request, Token);
        var plan = Assert.IsType<InstallPlan>(build.Plan);
        Assert.Empty(plan.Findings);
        var before = plan.RecoveryTargets.Where(target => target.RequiresRecovery).Select(target => target.Before.Bytes.ToArray()).ToArray();
        using var held = new FileStream(workspace.Combine(Directives), FileMode.Open, FileAccess.Read, FileShare.Read);
        var result = await InstallFrontmatterFixture.Operation(workspace).ExecuteAsync(request, Token);
        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Equal(InstallResultRecoveryState.Retained, result.Facts.Recovery.State);
        var bundlePath = Assert.IsType<string>(result.Facts.Recovery.ResidualPath);
        var bundle = await RecoveryBundleReader.ReadFinalAsync(workspace.Workspace, bundlePath, Token);
        Assert.Equal(RecoveryBundleReadState.Valid, bundle.State);
        using var archive = ZipFile.OpenRead(bundlePath);
        var payloads = archive.Entries.Skip(1).Select(entry =>
        {
            using var stream = entry.Open();
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return memory.ToArray();
        }).ToArray();
        Assert.Equal(before.Length, payloads.Length);
        for (var index = 0; index < before.Length; index++) Assert.Equal(before[index], payloads[index]);
        Assert.Contains(result.Facts.Effects, effect => effect.Outcome == InstallEffectOutcome.Verified);
        Assert.Contains(plan.TargetEffects, effect => effect.RelativePath == ".agents/open-forge.json");
    }

    [Fact(DisplayName = "An unavailable recorded Extension source keeps its delivered file unchanged"), Trait("Feature", "install-frontmatter"), Trait("Evidence", "Integration")]
    public async Task UnavailableExtensionSourceIsKept()
    {
        using var workspace = InstallOperationWorkspace.Create("install-convert-unavailable-source");
        await InstallFrontmatterFixture.SeedScopedAsync(workspace);
        const string path = ".agents/directives/extension.md";
        using var sources = new InstallFrontmatterSources(workspace);
        sources.Add(path, "---\nopen-forge:\n  description: Extension instruction\n  tags: [Extension]\n---\n\n# Extension\n");
        var read = await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, Token);
        using var source = TemporaryWorkspace.Create("install-unavailable-extension-source");
        var missingSource = source.Combine("missing");
        File.WriteAllBytes(workspace.Combine(InstallOperationWorkspace.OwnershipPath), WorkspaceOwnershipCodec.Write(read.Document with
        { Extensions = [new ExtensionOwnership("local", "1", missingSource, [], [path], [])] }));
        var before = File.ReadAllBytes(workspace.Combine(path));
        var result = await InstallFrontmatterFixture.Operation(workspace).ExecuteAsync(
            await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Root, installed: true), Token);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var kept = Assert.Single(Assert.IsType<InstallFrontmatter>(result.Frontmatter).Kept);
        Assert.Equal(path, kept.Path);
        Assert.Equal(InstallFrontmatterKeptReason.SourceUnavailable, kept.Reason);
        Assert.Equal(before, File.ReadAllBytes(workspace.Combine(path)));
    }

    private static async Task<string[]> SeedAdditionalClaimsAsync(InstallOperationWorkspace workspace, InstallFrontmatterSources sources)
    {
        var payload = Assert.IsType<Core.Framework.Distribution.Models.FrameworkPayload>(EmbeddedFrameworkPayloadReader.Read().Payload);
        sources.Add(".agents/memory/team/_team.md", "---\nopen-forge:\n  description: Team notes\n  tags: [Memory]\n---\n\n# Team\n\n## Entries\n\n");
        sources.Add(Scoped, Encoding.UTF8.GetString((payload.Find(".agents/memory/working/_working.md") ?? throw new InvalidOperationException()).Bytes.AsSpan()));
        var package = EmbeddedExtensionCatalogueReader.Read().Packages.Single(package => package.Id == "collaboration");
        var files = package.Payload.Where(file => file.TargetPath is not null && file.Bytes is not null).ToArray();
        foreach (var file in files)
            sources.Add(file.TargetPath ?? throw new InvalidOperationException(), Encoding.UTF8.GetString((file.Bytes ?? throw new InvalidOperationException()).Span));
        var paths = files.Select(file => file.TargetPath ?? throw new InvalidOperationException()).ToArray();
        var read = await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, Token);
        var framework = Assert.IsType<FrameworkOwnership>(read.Document.Framework);
        File.WriteAllBytes(workspace.Combine(InstallOperationWorkspace.OwnershipPath), WorkspaceOwnershipCodec.Write(read.Document with
        {
            Framework = framework with { Paths = framework.Paths.Add(Scoped), Regions = framework.Regions.Add(new(Scoped, "entries")) },
            Extensions = [new ExtensionOwnership(package.Id, package.Version, null, package.Dependencies.ToImmutableArray(), paths.ToImmutableArray(), [])],
        }));
        return paths.Where(path => path.EndsWith(".md", StringComparison.Ordinal) && !path.EndsWith("/SKILL.md", StringComparison.Ordinal)).ToArray();
    }
}
