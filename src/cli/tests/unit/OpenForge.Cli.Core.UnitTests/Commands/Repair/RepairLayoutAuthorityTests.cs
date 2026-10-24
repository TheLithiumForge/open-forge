using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

[Trait("Feature", "repair"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class RepairLayoutAuthorityTests
{
    [Theory(DisplayName = "Repair admits literal content locations and rejects their case aliases")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData("knowledge/nested")]
    public void ContentAuthorityUsesOrdinalRoots(string mainRoot)
    {
        var root = Path.GetFullPath("repair-layout");
        var layout = WorkspaceLayout.Create(new(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace), mainRoot);
        var policy = new RoutedMetadataPolicyContext
        {
            Layout = layout,
            Settings = WorkspaceSettingsDocument.Empty with { DeclaredMainRoot = mainRoot },
            Ownership = WorkspaceOwnershipDocument.Empty,
        };
        const string leaf = "/notes/source" + SourceFormClassifier.MarkdownExtension;

        Assert.True(RepairProtectionReader.Allows(mainRoot + leaf, policy));
        Assert.False(RepairProtectionReader.Allows(mainRoot.ToUpperInvariant() + leaf, policy));
        var native = WorkspaceLayout.NativeSkillsPath + "/workshop/reference" + SourceFormClassifier.MarkdownExtension;
        Assert.True(RepairProtectionReader.Allows(native, policy));
        Assert.False(RepairProtectionReader.Allows(WorkspaceLayout.NativeSkillsPath.ToUpperInvariant() + "/workshop/reference" + SourceFormClassifier.MarkdownExtension, policy));
    }
}
