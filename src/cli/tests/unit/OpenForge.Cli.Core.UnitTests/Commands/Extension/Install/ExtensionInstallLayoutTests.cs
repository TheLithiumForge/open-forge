using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Install;

[Trait("Feature", "extension-install"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class ExtensionInstallLayoutTests
{
    private const string MainRoot = "docs/forge";
    private const string PackageId = "layout-fixture";
    private const string Version = "1.0.0";
    private const string Note = "note" + SourceFormClassifier.MarkdownExtension;
    private const string PackageTarget = WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceCategoryNames.Guidance + "/" + Note;
    private const string InstalledTarget = MainRoot + "/" + WorkspaceCategoryNames.Guidance + "/" + Note;
    private const string SkillTarget = WorkspaceLayout.NativeSkillsPath + "/" + PackageId + "/" + SourceFormClassifier.NativeSkillFileName;
    private static readonly byte[] Content = [0x41, 0x0D, 0x0A];
    private static readonly byte[] OtherContent = [0x42, 0x0A];

    [Fact(DisplayName = "Extension normalization maps targets while preserving package coordinates and native Skills")]
    public void MapsTargets()
    {
        var package = Package(File(PackageTarget, Content), File(SkillTarget, Content));
        var result = ExtensionInstallPayloadNormalizer.Normalize(Layout(), [package], WorkspaceSettingsDocument.Empty);
        Assert.Null(result.Finding);
        var files = Assert.Single(result.Packages).Payload;
        Assert.Contains(files, file => file.TargetPath == InstalledTarget);
        Assert.Contains(files, file => file.TargetPath == SkillTarget);
        Assert.Equal(package.Payload.Select(file => file.Path).Order(), files.Select(file => file.Path).Order());
        Assert.All(files, file => Assert.Equal(Content, Assert.IsType<ReadOnlyMemory<byte>>(file.Bytes).ToArray()));
    }

    [Fact(DisplayName = "Extension exclusions apply after mapping the package destination")]
    public void MappedExclusion()
    {
        var settings = WorkspaceSettingsDocument.Empty with { RemovedFiles = [InstalledTarget] };
        var result = ExtensionInstallPayloadNormalizer.Normalize(Layout(), [Package(File(PackageTarget, Content))], settings);
        Assert.Null(result.Finding);
        Assert.Equal([InstalledTarget], result.ExcludedPaths);
        Assert.Empty(Assert.Single(result.Packages).Payload);
    }

    [Fact(DisplayName = "Mapped and literal Extension destinations collide before writing")]
    public void MappedCollision()
    {
        var result = ExtensionInstallPayloadNormalizer.Normalize(Layout(),
            [Package(File(PackageTarget, Content), File(InstalledTarget, OtherContent))], WorkspaceSettingsDocument.Empty);
        Assert.Equal(ExtensionInstallFindingCode.OwnershipConflict, result.Finding?.Code);
        Assert.Equal(InstalledTarget, result.Finding?.Target);
    }

    [Fact(DisplayName = "Extension implicit admission uses ordinal content roots")]
    public void OrdinalAdmission()
    {
        var layout = Layout();
        Assert.True(ExtensionDestinationPolicy.IsImplicit(layout, InstalledTarget));
        Assert.True(ExtensionDestinationPolicy.IsImplicit(layout, SkillTarget));
        Assert.False(ExtensionDestinationPolicy.IsImplicit(layout, MainRoot.ToUpperInvariant() + "/" + Note));
        Assert.False(ExtensionDestinationPolicy.IsImplicit(layout, WorkspaceLayout.DefaultMainRoot.ToUpperInvariant() + "/" + Note));
        Assert.False(ExtensionDestinationPolicy.IsImplicit(layout, PackageTarget));
    }

    private static WorkspaceLayout Layout()
    {
        var root = Path.GetFullPath(PackageId, AppContext.BaseDirectory);
        return WorkspaceLayout.Create(new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace), MainRoot);
    }

    private static ExtensionPackageFact Package(params ExtensionPackageFileFact[] files)
        => ExtensionPackageFact.Create(new() { Id = PackageId, Name = PackageId, Description = PackageId, Version = Version, Dependencies = [] },
            new() { ManifestPath = ExtensionPackageLayout.ManifestFileName, Payload = files });

    private static ExtensionPackageFileFact File(string target, byte[] bytes)
        => ExtensionPackageFileFact.Create(new()
        {
            Path = ExtensionPackageLayout.ContentPathPrefix + target,
            TargetPath = target,
            State = ExtensionPackageFileReadState.Available,
            Bytes = bytes,
            ByteLength = bytes.Length,
            Sha256 = FileExpectation.Hash(bytes),
        });
}
