using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Configuration;

public sealed class InstallConfigurationFrontmatterSourceIntegrationTests
{
    [Theory(DisplayName = "Configure uses recorded Extension package readers and preserves excluded packages"), InlineData(false), InlineData(true)]
    [Trait("Feature", "install-frontmatter"), Trait("Evidence", "Integration")]
    public async Task RecordedPackageSourcesAndExclusions(bool excluded)
    {
        using var workspace = InstallOperationWorkspace.Create("install-frontmatter-recorded-source");
        using var source = TemporaryWorkspace.Create("install-frontmatter-package");
        const string path = ".agents/directives/recorded.md";
        const string scoped = "---\nopen-forge:\n  description: Recorded source\n  tags: [Extension]\n---\n\n# Recorded\n\nKeep source bytes.\n";
        const string root = "---\ndescription: Recorded source\ntags: [Extension]\n---\n\n# Recorded\n\nKeep source bytes.\n";
        source.WriteText("extension.json", """{"id":"local","name":"Local","description":"Recorded package","version":"1.0.0","dependencies":[]}""");
        source.WriteText("content/" + path, scoped);
        await InstallFrontmatterFixture.SeedScopedAsync(workspace);
        using var delivered = new InstallFrontmatterSources(workspace);
        delivered.Add(path, scoped);
        if (excluded) workspace.ReplaceInstalledText(".agents/open-forge.json", """{"frontmatter":"scoped","removedExtensions":["local"]}""");
        var ownership = await WorkspaceOwnershipReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, TestContext.Current.CancellationToken);
        File.WriteAllBytes(workspace.Combine(InstallOperationWorkspace.OwnershipPath), WorkspaceOwnershipCodec.Write(ownership.Document with
        {
            Extensions = [new ExtensionOwnership("local", "1.0.0", source.Path, [], [path], [])],
        }));
        var sourceBefore = source.SnapshotHashes();
        var result = await InstallFrontmatterFixture.Operation(workspace).ExecuteAsync(
            await InstallFrontmatterFixture.RequestAsync(workspace, FrontmatterForm.Root, installed: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(Assert.IsType<InstallFrontmatter>(result.Frontmatter).Kept);
        Assert.Equal(excluded ? scoped : root, File.ReadAllText(workspace.Combine(path)));
        Assert.Equal(!excluded, result.Facts.Effects.Any(effect => effect.Path == path));
        Assert.Equal(sourceBefore, source.SnapshotHashes());
    }
}
