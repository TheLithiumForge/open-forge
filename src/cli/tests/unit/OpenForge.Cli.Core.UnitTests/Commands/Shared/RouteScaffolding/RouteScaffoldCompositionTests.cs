using System.Text;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Rename;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Shared.RouteScaffolding;

[Trait("Feature", "route-scaffolding"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class RouteScaffoldCompositionTests
{
    [Theory(DisplayName = "Maintenance scaffolds use explicit folder title metadata and workspace form")]
    [InlineData((int)FrontmatterForm.Scoped)]
    [InlineData((int)FrontmatterForm.Root)]
    public void ExplicitMetadataIsComposed(int formValue)
    {
        var form = (FrontmatterForm)formValue;
        var document = RouteScaffoldComposer.FolderDocument("directives/review-rules", "Review rules", ["Directive"], form);
        var text = Encoding.UTF8.GetString(new RouteScaffoldComposer().Compose(document).AsSpan());

        var header = form == FrontmatterForm.Scoped
            ? "---\nopen-forge:\n  description: Notes in directives/review-rules\n  tags: [Directive]\n---\n"
            : "---\ndescription: Notes in directives/review-rules\ntags: [Directive]\n---\n";
        Assert.Equal(header + "\n# Review rules\n\nNotes in directives/review-rules.\n\n## Axioms\n\n"
            + "- inherited - No local axioms; loaded ancestor axioms remain active.\n\n## Entries\n\n- none - No entries - #Empty", text);
    }

    [Theory(DisplayName = "Canonical folder rename requires bound committed or strong Git identity evidence")]
    [InlineData((int)RouteScaffoldRenameBasis.CommittedContent, null, true)]
    [InlineData((int)RouteScaffoldRenameBasis.GitRemoval, 90, true)]
    [InlineData((int)RouteScaffoldRenameBasis.GitRemoval, 89, false)]
    public void FolderRenameRequiresProof(int basis, int? similarity, bool expected)
    {
        var candidate = Candidate();
        var proof = new RouteScaffoldRenameProof
        {
            OldCanonicalPath = ".agents/guidance/billing/_billing.md",
            CurrentCanonicalPath = ".agents/guidance/invoices/_billing.md",
            CurrentContentHash = candidate.OldSnapshot?.ContentHash ?? "",
            Basis = (RouteScaffoldRenameBasis)basis,
            Similarity = similarity,
        };
        var intent = new RouteScaffoldRenamePlanner().Decide(DefaultWorkspaceLayout, candidate, proof);

        Assert.Equal(expected, intent.IsRename);
        Assert.Equal(expected ? candidate.OldSnapshot?.Bytes : candidate.ScaffoldBytes, intent.IntendedBytes);
        Assert.Equal(!expected, intent.OldNavigation is not null);
    }

    [Fact(DisplayName = "Unproven or contradictory folder identity keeps the old file and supplies Doctor facts")]
    public void UnprovenIdentityKeepsOldFile()
    {
        var candidate = Candidate();
        var intent = new RouteScaffoldRenamePlanner().Decide(DefaultWorkspaceLayout, candidate, proof: null);
        Assert.False(intent.IsRename);
        Assert.Null(intent.DeleteBefore);
        Assert.Equal(".agents/guidance/invoices/_billing.md", intent.OldNavigation?.Path);
        Assert.Equal("_invoices.md", intent.OldNavigation?.CanonicalFileName);
        var wrongProof = new RouteScaffoldRenameProof
        {
            OldCanonicalPath = ".agents/guidance/billing/_billing.md",
            CurrentCanonicalPath = ".agents/guidance/invoices/_billing.md",
            CurrentContentHash = FileExpectation.Hash("changed"u8),
            Basis = RouteScaffoldRenameBasis.CommittedContent,
        };
        Assert.False(new RouteScaffoldRenamePlanner().Decide(DefaultWorkspaceLayout, candidate, wrongProof).IsRename);
    }

    [Fact(DisplayName = "Scaffold rename rejects undefined proof bases and unrelated old filenames")]
    public void InvalidProofCannotRename()
    {
        var candidate = Candidate();
        var proof = new RouteScaffoldRenameProof
        {
            OldCanonicalPath = ".agents/guidance/other/_other.md",
            CurrentCanonicalPath = ".agents/guidance/invoices/_billing.md",
            CurrentContentHash = candidate.OldSnapshot?.ContentHash ?? "",
            Basis = RouteScaffoldRenameBasis.CommittedContent,
        };
        var planner = new RouteScaffoldRenamePlanner();
        Assert.False(planner.Decide(DefaultWorkspaceLayout, candidate, proof).IsRename);
        Assert.Throws<ArgumentOutOfRangeException>(() => planner.Decide(DefaultWorkspaceLayout, candidate, proof with { Basis = (RouteScaffoldRenameBasis)int.MaxValue }));
    }

    internal static RouteScaffoldRenameCandidate Candidate()
    {
        var request = RouteScaffoldChainTests.Request(
            [".agents/guidance/_guidance.md", ".agents/guidance/invoices/_billing.md"], []);
        var builder = new RouteScaffoldSourceBuilder();
        var old = request.Catalogue.FindByPath(".agents/guidance/invoices/_billing.md")
            ?? throw new InvalidOperationException();
        var destination = builder.Create(WorkspaceLayout.Create(request.Catalogue.Workspace, WorkspaceLayout.DefaultMainRoot), ".agents/guidance/invoices/_invoices.md");
        return new RouteScaffoldRenameCandidate
        {
            Destination = destination,
            DestinationBefore = FileStateSnapshot.Missing(destination.Base.PhysicalPath),
            OldSource = old,
            OldSnapshot = FileStateSnapshot.File(old.Base.PhysicalPath, old.Base.PhysicalPath, "# Billing\n\nKeep local rules.\n"u8),
            ScaffoldBytes = new RouteScaffoldComposer().Compose(
                RouteScaffoldComposer.FolderDocument("guidance/invoices", "Invoices", ["Guidance"], FrontmatterForm.Scoped)),
        };
    }

    private static readonly WorkspaceLayout DefaultWorkspaceLayout = WorkspaceLayout.Create(
            new CliWorkspace(
                Path.GetFullPath(nameof(RouteScaffoldCompositionTests)),
                Path.GetFullPath(nameof(RouteScaffoldCompositionTests)),
                CliWorkspaceSelectionMethod.ExplicitWorkspace),
            WorkspaceLayout.DefaultMainRoot);
}
