using System.Text;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Distribution.Operational;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Extensions.Operational;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Operational;

[Trait("Feature", "workspace-layout"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class WorkspaceLayoutTargetIntegrationTests
{
    private const string Fixture = "operational-target-layout";
    private const string CustomRoot = "docs/forge";
    private const string RelativeFile = WorkspaceCategoryNames.Guidance + "/policy" + SourceFormClassifier.MarkdownExtension;
    private const string PackagePath = WorkspaceLayout.DefaultMainRoot + "/" + RelativeFile;
    private const string Body = "# Authored policy\n";
    private const string Version = "1";

    [Theory(DisplayName = "Extension ownership destinations compare against the mapped package source")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(CustomRoot)]
    public async Task MappedExtensionOwnershipFindsPackageAsset(string root)
    {
        using var temporary = TemporaryWorkspace.Create(Fixture);
        var layout = WorkspaceLayout.Create(new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace), root);
        var destination = root + "/" + RelativeFile;
        var bytes = Encoding.UTF8.GetBytes(Body);
        temporary.CreateFile(destination, Body);
        var file = ExtensionPackageFileFact.Create(new ExtensionPackageFileSnapshot
        {
            Path = PackagePath,
            TargetPath = PackagePath,
            State = ExtensionPackageFileReadState.Available,
            Bytes = bytes,
            ByteLength = bytes.Length,
        });
        var package = ExtensionPackageFact.Create(new ExtensionPackageManifestFact
        {
            Id = Fixture,
            Name = Fixture,
            Description = Body,
            Version = Version,
            Dependencies = [],
        }, new ExtensionPackageContentsFact { ManifestPath = ExtensionPackageLayout.ManifestFileName, Payload = [file] });
        var ownership = WorkspaceOwnershipDocument.Empty with
        {
            Extensions = [new ExtensionOwnership(Fixture, Version, null, [], [destination], [])],
        };
        var source = new ExtensionSourceObservation(null,
            new ExtensionSourceReadResult(ExtensionSourceReadState.Complete, ExtensionSourceKind.EmbeddedCatalogue, Fixture, [package], null));

        var targets = await new ExtensionLifecycleTargetReader(new PhysicalPathResolver()).ReadAsync(
            layout, ownership, [source], FrontmatterForm.Scoped, TestContext.Current.CancellationToken);

        var target = Assert.Single(targets);
        Assert.Equal(destination, target.Path);
        Assert.Equal(OperationalTargetState.Current, target.State);
        Assert.Equal(bytes, File.ReadAllBytes(temporary.Combine(destination)));
    }

    [Theory(DisplayName = "Framework ownership destinations compare against the mapped package source")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(CustomRoot)]
    public async Task MappedOwnershipFindsPackageAsset(string root)
    {
        using var temporary = TemporaryWorkspace.Create(Fixture);
        var layout = WorkspaceLayout.Create(new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace), root);
        var destination = root + "/" + RelativeFile;
        var bytes = Encoding.UTF8.GetBytes(Body);
        temporary.CreateFile(destination, Body);
        var payload = FrameworkPayload.Create(new[] { FrameworkPayloadAsset.RootAgentPath, FrameworkPayloadAsset.RootClaudePath,
            FrameworkPayloadAsset.LoaderPath, PackagePath }.Select(path => FrameworkPayloadAsset.Create(path, bytes)));
        var ownership = new FrameworkOwnership(new OwnedSource(Fixture, null), [destination], []);

        var targets = await new FrameworkLifecycleTargetReader(new PhysicalPathResolver()).ReadAsync(
            layout, ownership, payload, FrontmatterForm.Scoped, TestContext.Current.CancellationToken);

        var target = Assert.Single(targets);
        Assert.Equal(destination, target.Path);
        Assert.Equal(PackagePath, target.SourceAssetPath);
        Assert.Equal(OperationalTargetState.Current, target.State);
        Assert.Equal(bytes, File.ReadAllBytes(temporary.Combine(destination)));
    }
}
