using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Operational.Shared.RenameEvidence;

[Trait("Feature", "rename-evidence"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class RenameLayoutEvidenceTests
{
    private const string CustomRoot = "docs/forge";
    private const string OldStem = "old";
    private const string NewStem = "new";
    private const string Description = "Specific authored policy";
    private const string Tag = "Guidance";

    [Theory(DisplayName = "Rename metadata rejects source relative labels for default and nested custom roots")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(CustomRoot, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true), InlineData(CustomRoot, true)]
    public void RootRelativeLabelsStayWeak(string root, bool distinctive)
    {
        var absolute = RenameEvidenceTestFacts.Absolute(string.Empty);
        var layout = WorkspaceLayout.Create(new CliWorkspace(absolute, absolute, CliWorkspaceSelectionMethod.ExplicitWorkspace), root);
        var relative = WorkspaceCategoryNames.Guidance + "/" + OldStem;
        var oldPath = root + "/" + relative + SourceFormClassifier.MarkdownExtension;
        var newPath = root + "/" + WorkspaceCategoryNames.Guidance + "/" + NewStem + SourceFormClassifier.MarkdownExtension;
        var signature = RenameMetadataIdentity.Create(distinctive ? Description : relative, [Tag], []);
        var candidate = RenameEvidenceTestFacts.Candidate(newPath, metadata: signature);
        var old = RenameEvidenceTestFacts.Old(oldPath, body: null, metadata: signature);

        Assert.Equal(distinctive, RenameMetadataIdentity.IsDistinctLabel(layout, old, candidate));
        var facts = RenameEvidenceTestFacts.Facts([candidate], [old]) with { Layout = layout };
        var evidence = new RenameEvidenceEvaluator().Evaluate(facts, TestContext.Current.CancellationToken);
        Assert.Same(layout, evidence.Facts.Layout);
        Assert.Equal(distinctive, evidence.Resolutions[0].Proven is not null);
    }
}
