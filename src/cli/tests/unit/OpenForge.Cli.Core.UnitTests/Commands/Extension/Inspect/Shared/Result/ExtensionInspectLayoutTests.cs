using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Extension.Inspect.Shared.Result;

[Trait("Feature", "extension-inspect"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class ExtensionInspectLayoutTests
{
    private const string MainRoot = "docs/forge";
    private const string Note = "note" + SourceFormClassifier.MarkdownExtension;
    private const string Skill = WorkspaceLayout.NativeSkillsPath + "/fixture/" + SourceFormClassifier.NativeSkillFileName;

    [Fact(DisplayName = "Extension inspection fingerprints only ordinally admitted content as semantic Markdown")]
    public void FingerprintAdmissionUsesObservedRoots()
    {
        var workspace = new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var layout = WorkspaceLayout.Create(workspace, MainRoot);
        Assert.Equal(ExtensionInspectFingerprintKind.Semantic, ExtensionInspectFingerprintPolicy.ReadKind(layout, MainRoot + "/" + Note));
        Assert.Equal(ExtensionInspectFingerprintKind.Semantic, ExtensionInspectFingerprintPolicy.ReadKind(layout, Skill));
        Assert.Equal(ExtensionInspectFingerprintKind.ExactBytes, ExtensionInspectFingerprintPolicy.ReadKind(layout, WorkspaceLayout.DefaultMainRoot + "/" + Note));
        Assert.Equal(ExtensionInspectFingerprintKind.ExactBytes, ExtensionInspectFingerprintPolicy.ReadKind(layout, MainRoot.ToUpperInvariant() + "/" + Note));
        Assert.Equal(ExtensionInspectFingerprintKind.ExactBytes, ExtensionInspectFingerprintPolicy.ReadKind(layout, WorkspaceLayout.DefaultMainRoot.ToUpperInvariant() + "/" + Note));
    }
}
