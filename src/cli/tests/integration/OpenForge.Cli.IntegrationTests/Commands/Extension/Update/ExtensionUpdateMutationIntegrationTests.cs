using System.IO.Compression;
using System.Text;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Update;

public sealed class ExtensionUpdateMutationIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Update replaces changed and restores missing current targets with retained recovery"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task ReplacesChangedAndRestoresMissingTargets(bool force, bool git)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-force-mutation");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-update-force-mutation-source");
        source.AddPackage(
            "base",
            [],
            (".agents/base/_base.md", Document("Base v1")));
        source.AddPackage(
            "toolkit",
            ["base"],
            (".agents/toolkit/_toolkit.md", Document("Toolkit v1")));
        await InstallAllAsync(workspace, source);
        var frameworkBefore = workspace.ReadFrameworkOwnership();
        source.ReplacePayload("base", ".agents/base/_base.md", Document("Base v2"));
        source.ReplacePayload("toolkit", ".agents/toolkit/_toolkit.md", Document("Toolkit v2"));
        workspace.ReplaceText(".agents/toolkit/_toolkit.md", Document("User divergence"));
        File.Delete(workspace.Combine(".agents/base/_base.md"));
        var sourceBefore = source.Snapshot();

        if (git) workspace.CreateDirectory(".git");
        string[] arguments = ["extension", "update", "toolkit", "--source", source.Path,
            "--automatic", "--format", "json", .. force ? new[] { "--force" } : []];
        var run = await workspace.RunAsync(arguments);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        var result = root.GetProperty("data");
        Assert.Equal("apply", result.GetProperty("mode").GetString());
        Assert.Equal(force, result.GetProperty("force").GetBoolean());
        Assert.Contains(
            root.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == ".agents/base/_base.md");
        Assert.Contains(
            root.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == ".agents/toolkit/_toolkit.md");
        Assert.Equal(Document("Base v2"), workspace.ReadText(".agents/base/_base.md"));
        Assert.Equal(Document("Toolkit v2"), workspace.ReadText(".agents/toolkit/_toolkit.md"));
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse(frameworkBefore.GetRawText()),
            JsonNode.Parse(workspace.ReadFrameworkOwnership().GetRawText())));
        var recovery = root.GetProperty("recovery");
        Assert.Equal("retained", recovery.GetProperty("disposition").GetString());
        var bundlePath = Assert.IsType<string>(recovery.GetProperty("path").GetString());
        await AssertPriorAsync(workspace, bundlePath, ".agents/toolkit/_toolkit.md", Document("User divergence"));
        var next = document.RootElement.GetProperty("next");
        if (git)
        {
            Assert.Equal("git diff", next.GetProperty("command").GetString());
            Assert.Contains(bundlePath, next.GetProperty("reason").GetString(), StringComparison.Ordinal);
        }
        else
        {
            Assert.Equal(
                $"Review previous content in the recovery bundle at {bundlePath}.",
                next.GetProperty("command").GetString());
            Assert.Equal(
                "Previous content remains available for review.",
                next.GetProperty("reason").GetString());
            Assert.Equal("sentence", next.GetProperty("kind").GetString());
        }
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Update repairs stale generated Entries without replacing authored host content"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RepairsStaleGeneratedEntriesInFrameworkAndPackageHosts(bool installedPackageHost)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            installedPackageHost
                ? "extension-update-stale-entries-package-host"
                : "extension-update-stale-entries-framework-host");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            installedPackageHost
                ? "extension-update-stale-entries-package-host-source"
                : "extension-update-stale-entries-framework-host-source");

        var hostPath = installedPackageHost
            ? ".agents/toolkit/_toolkit.md"
            : ".agents/guidance/_guidance.md";
        var childPath = installedPackageHost
            ? ".agents/toolkit/child.md"
            : ".agents/guidance/toolkit.md";
        if (installedPackageHost)
        {
            source.AddPackage(
                "toolkit",
                [],
                (hostPath, PackageHostDocument()),
                (childPath, Document("Toolkit child")));
        }
        else
        {
            source.AddPackage(
                "toolkit",
                [],
                (childPath, Document("Toolkit child")));
        }

        await InstallAllAsync(workspace, source);
        var healthyHostBytes = File.ReadAllBytes(workspace.Combine(hostPath));
        var sourceBefore = source.Snapshot();
        ReplaceEntriesWithStale(workspace, hostPath);
        var staleHostBytes = File.ReadAllBytes(workspace.Combine(hostPath));
        var staleOutside = ReadEntriesOutside(staleHostBytes);
        var workspaceBefore = workspace.Snapshot();

        using var healthy = ExtensionInstallIntegrationWorkspace.Create(
            installedPackageHost
                ? "extension-update-stale-entries-healthy-package-host"
                : "extension-update-stale-entries-healthy-framework-host");
        await healthy.SeedFrameworkAsync();
        await InstallAllAsync(healthy, source);

        var arguments = new[]
        {
            "extension", "update", "toolkit", "--source", source.Path,
            "--automatic", "--dry-run", "--format", "json", "--detail", "full",
        };
        var preview = await workspace.RunAsync(arguments);

        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(string.Empty, preview.StandardError);
        using var previewDocument = JsonDocument.Parse(preview.StandardOutput);
        var previewRoot = previewDocument.RootElement;
        var previewEffects = previewRoot.GetProperty("effects").EnumerateArray().ToArray();
        Assert.Equal(1, previewEffects.Count(effect => effect.GetProperty("path").GetString() == hostPath));
        var previewSection = Assert.Single(
            previewEffects,
            effect => effect.GetProperty("path").GetString() == hostPath
                && effect.GetProperty("kind").GetString() == "section"
                && effect.GetProperty("outcome").GetString() == "planned");
        Assert.Equal("planned", previewSection.GetProperty("outcome").GetString());
        var previewDataEffect = Assert.Single(
            previewRoot.GetProperty("data").GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == hostPath);
        Assert.Equal(
            "planned",
            previewDataEffect.GetProperty("verification").GetProperty("topology").GetString());
        Assert.Equal(
            previewEffects.Count(effect => effect.GetProperty("kind").GetString() == "section"),
            ReadCount(previewRoot, "sectionsUpdated"));

        var applicationArguments = arguments.Where(argument => argument != "--dry-run").ToArray();
        var application = await workspace.RunAsync(applicationArguments);

        Assert.Equal(0, application.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, application.Status);
        Assert.Equal(string.Empty, application.StandardError);
        using var applicationDocument = JsonDocument.Parse(application.StandardOutput);
        var applicationRoot = applicationDocument.RootElement;
        var applicationEffects = applicationRoot.GetProperty("effects").EnumerateArray().ToArray();
        Assert.Equal(1, applicationEffects.Count(effect => effect.GetProperty("path").GetString() == hostPath));
        var applicationSection = Assert.Single(
            applicationEffects,
            effect => effect.GetProperty("path").GetString() == hostPath
                && effect.GetProperty("kind").GetString() == "section"
                && effect.GetProperty("outcome").GetString() == "done");
        Assert.Equal("done", applicationSection.GetProperty("outcome").GetString());
        var applicationDataEffect = Assert.Single(
            applicationRoot.GetProperty("data").GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == hostPath);
        Assert.Equal(
            "verified",
            applicationDataEffect.GetProperty("verification").GetProperty("topology").GetString());
        Assert.Equal(
            applicationEffects.Count(effect => effect.GetProperty("kind").GetString() == "section"),
            ReadCount(applicationRoot, "sectionsUpdated"));
        Assert.Equal(healthyHostBytes, File.ReadAllBytes(workspace.Combine(hostPath)));
        var appliedOutside = ReadEntriesOutside(File.ReadAllBytes(workspace.Combine(hostPath)));
        Assert.True(staleOutside.Prefix.AsSpan().SequenceEqual(appliedOutside.Prefix));
        Assert.True(staleOutside.Suffix.AsSpan().SequenceEqual(appliedOutside.Suffix));
        AssertWorkspaceUnchangedOutside(workspaceBefore, workspace, hostPath);
        Assert.Equal(sourceBefore, source.Snapshot());

        var healthyRun = await healthy.RunAsync(arguments);
        Assert.Equal(0, healthyRun.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, healthyRun.Status);
        Assert.Equal(string.Empty, healthyRun.StandardError);
        using var healthyDocument = JsonDocument.Parse(healthyRun.StandardOutput);
        var healthyRoot = healthyDocument.RootElement;
        Assert.Empty(healthyRoot.GetProperty("effects").EnumerateArray());
        Assert.Equal(0, ReadCount(healthyRoot, "sectionsUpdated"));
        Assert.Equal(healthyHostBytes, File.ReadAllBytes(healthy.Combine(hostPath)));
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Extension Update prune deletes edited retired files and releases absent ownership without deletion"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PruneDeletesEligibleRetiredTargetOnly(bool missing)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-prune-retired");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-update-prune-retired-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit/_toolkit.md", Document("Toolkit")));
        await InstallAllAsync(workspace, source);
        File.Delete(Path.Combine(
            source.PackagePath("toolkit"),
            "content",
            ".agents",
            "toolkit",
            "_toolkit.md"));
        if (missing) File.Delete(workspace.Combine(".agents/toolkit/_toolkit.md"));
        else workspace.ReplaceText(".agents/toolkit/_toolkit.md", Document("Edited retired bytes"));
        var sourceBefore = source.Snapshot();

        var run = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--prune", "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        var result = root.GetProperty("data");
        Assert.True(result.GetProperty("prune").GetBoolean());
        Assert.Equal(!missing, root.GetProperty("effects").EnumerateArray().Any(
            effect => effect.GetProperty("path").GetString() == ".agents/toolkit/_toolkit.md"));
        Assert.False(File.Exists(workspace.Combine(".agents/toolkit/_toolkit.md")));
        Assert.DoesNotContain(
            ".agents/toolkit/_toolkit.md",
            workspace.ReadText(ExtensionInstallIntegrationWorkspace.OwnershipPath),
            StringComparison.Ordinal);
        if (!missing)
        {
            var recovery = root.GetProperty("recovery");
            Assert.Equal("retained", recovery.GetProperty("disposition").GetString());
            var bundlePath = Assert.IsType<string>(recovery.GetProperty("path").GetString());
            await AssertPriorAsync(workspace, bundlePath, ".agents/toolkit/_toolkit.md", Document("Edited retired bytes"));
        }
        else
        {
            Assert.Equal("retained", root.GetProperty("recovery").GetProperty("disposition").GetString());
        }
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Update lock contention blocks every effect and preserves source and workspace"), Trait("Feature", "extension-update"), Trait("Evidence", "Integration")]
    public async Task LockContentionBlocksEveryEffect()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-update-lock-contention");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-update-lock-contention-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/toolkit/_toolkit.md", Document("Toolkit v1")));
        await InstallAllAsync(workspace, source);
        source.ReplacePayload("toolkit", ".agents/toolkit/_toolkit.md", Document("Toolkit v2"));
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();
        using var heldLease = workspace.HoldLock();

        var run = await workspace.RunAsync(
        [
            "extension", "update", "toolkit",
            "--source", source.Path,
            "--force", "--automatic", "--format", "json",
        ]);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var root = document.RootElement;
        Assert.Contains(
            root.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "extension-update.workspace-lock-unavailable");
        Assert.Empty(root.GetProperty("effects").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("recovery").ValueKind);
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    private static async Task AssertPriorAsync(ExtensionInstallIntegrationWorkspace workspace,
        string bundlePath, string target, string expected)
    {
        var read = await RecoveryBundleReader.ReadFinalAsync(workspace.Workspace, bundlePath, TestContext.Current.CancellationToken);
        var entry = Assert.Single(Assert.IsType<RecoveryBundleVerifiedRead>(read.Verified).Entries, value => value.TargetPath == target);
        using var archive = ZipFile.OpenRead(bundlePath);
        using var prior = new StreamReader(Assert.IsType<ZipArchiveEntry>(archive.GetEntry(entry.PriorPayload!)).Open());
        Assert.Equal(expected, await prior.ReadToEndAsync(TestContext.Current.CancellationToken));
    }

    private static async Task InstallAllAsync(
        ExtensionInstallIntegrationWorkspace workspace,
        ExtensionInstallCatalogue source)
    {
        var run = await workspace.RunAsync(
        [
            "extension", "install", "--all",
            "--source", source.Path,
            "--automatic", "--format", "json",
        ]);

        Assert.Equal(0, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, run.Status);
        Assert.Equal(string.Empty, run.StandardError);
    }

    private static void ReplaceEntriesWithStale(
        ExtensionInstallIntegrationWorkspace workspace,
        string path)
    {
        var document = workspace.ReadText(path);
        var block = new MarkdownDocumentParser().Parse(document).GeneratedRegion.EntriesBlock;
        if (block is not { Exists: true } entriesBlock)
        {
            throw new InvalidOperationException(
                "The stale Entries fixture requires an existing parser-owned Entries block.");
        }

        workspace.ReplaceText(
            path,
            $"{document[..entriesBlock.Span.Start]}- [Stale](stale.md) - #Extension{entriesBlock.LineEnding}"
                + document[entriesBlock.Span.End..]);
    }

    private static (byte[] Prefix, byte[] Suffix) ReadEntriesOutside(byte[] bytes)
    {
        var document = Encoding.UTF8.GetString(bytes);
        var block = new MarkdownDocumentParser().Parse(document).GeneratedRegion.EntriesBlock;
        if (block is not { Exists: true } entriesBlock)
        {
            throw new InvalidOperationException(
                "The Entries outside-byte fixture requires an existing parser-owned Entries block.");
        }

        return (
            Encoding.UTF8.GetBytes(document[..entriesBlock.Span.Start]),
            Encoding.UTF8.GetBytes(document[entriesBlock.Span.End..]));
    }

    private static int ReadCount(JsonElement root, string name)
        => root.GetProperty("counts").GetProperty(name).GetInt32();

    private static void AssertWorkspaceUnchangedOutside(
        IReadOnlyDictionary<string, string> before,
        ExtensionInstallIntegrationWorkspace workspace,
        string changedPath)
    {
        var after = workspace.Snapshot();
        var changedKey = $"file:{changedPath}";
        foreach (var pair in before.Where(pair =>
                     pair.Key != changedKey
                     && !pair.Key.StartsWith("recovery-", StringComparison.Ordinal)))
        {
            Assert.True(after.TryGetValue(pair.Key, out var current), $"Missing unchanged entry '{pair.Key}'.");
            Assert.Equal(pair.Value, current);
        }

        foreach (var pair in after.Where(pair =>
                     pair.Key != changedKey
                     && !pair.Key.StartsWith("recovery-", StringComparison.Ordinal)))
        {
            Assert.True(before.ContainsKey(pair.Key), $"Unexpected changed entry '{pair.Key}'.");
        }
    }

    private static string PackageHostDocument()
        => OpenForgeDocumentSeed.Metadata(
            "Toolkit host",
            ["Extension"],
            "# Toolkit host\n\n## Entries\n");

    private static string Document(string heading)
        => OpenForgeDocumentSeed.Metadata(
            heading,
            ["Extension"],
            $"# {heading}\n");
}
