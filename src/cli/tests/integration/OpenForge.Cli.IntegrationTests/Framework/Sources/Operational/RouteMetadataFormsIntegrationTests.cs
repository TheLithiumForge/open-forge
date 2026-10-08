using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Operational;

public sealed class RouteMetadataFormsIntegrationTests
{
    [Theory(DisplayName = "Both metadata forms reach operational source observations and duplicate findings")]
    [InlineData(false)]
    [InlineData(true)]
    [Trait("Boundary", "OS"), Trait("Feature", "source-metadata"), Trait("Evidence", "Integration")]
    public async Task RootMetadataReachesOperationalFindings(bool root)
    {
        using var temporary = TemporaryWorkspace.Create("route-metadata-forms");
        temporary.CreateFile("AGENTS.md", "# Workspace\n");
        temporary.CreateFile(".agents/loader.md", "# Loader\n## Entries\n- [Docs](docs/_docs.md)\n");
        temporary.CreateFile(".agents/docs/_docs.md", Document(root, "description: Docs\ntags: [Docs]\n") + "# Docs\n## Axioms\n- inherited\n## Entries\n");
        temporary.CreateFile(".agents/docs/valid.md", Document(root, "description: Valid\ntags: [工作]\nresponsibility: Owner\n"));
        temporary.CreateFile(".agents/docs/duplicate.md", Document(root, "description: Duplicate\ntags: [First]\ntags: [Later]\n"));

        var inspection = await ReadAsync(temporary);
        var valid = Assert.Single(inspection.Sources, source => source.Source.Identity.CanonicalBasePath == ".agents/docs/valid.md");
        Assert.Equal(SourceAuthoredMetadataState.Complete, valid.AuthoredMetadata.State);
        Assert.Same(valid.FrameworkMetadata, valid.AuthoredMetadata.FrameworkMetadata);
        Assert.Equal(root ? FrontmatterForm.Root : FrontmatterForm.Scoped, valid.FrameworkMetadata.Syntax.AuthoredForm);
        Assert.Equal("Owner", valid.FrameworkMetadata.Metadata?.Responsibility);
        Assert.Equal(["工作"], valid.AuthoredMetadata.Tags);
        Assert.Empty(valid.WorkspaceIssues);

        var duplicate = Assert.Single(inspection.Sources, source => source.Source.Identity.CanonicalBasePath == ".agents/docs/duplicate.md");
        Assert.Equal(FrameworkDocumentMetadataFailureKind.Duplicate, duplicate.FrameworkMetadata.FailureKind);
        var issue = Assert.Single(duplicate.WorkspaceIssues);
        Assert.Equal(RouteWorkspaceSourceIssueKind.FrontmatterDuplicate, issue.Kind);
    }

    [Fact(DisplayName = "Native Skill and Loader root fields do not acquire ordinary Open Forge diagnostics")]
    [Trait("Boundary", "OS"), Trait("Feature", "source-metadata"), Trait("Evidence", "Integration")]
    public async Task NativeRootFieldsDoNotBecomeOpenForgeDiagnostics()
    {
        using var temporary = TemporaryWorkspace.Create("route-native-metadata");
        temporary.CreateFile("AGENTS.md", "# Workspace\n");
        temporary.CreateFile(".agents/loader.md", "---\ndescription: [Foreign]\ntags: [invalid tag]\n---\n# Loader\n## Entries\n");
        temporary.CreateFile(".agents/skills/native/SKILL.md", "---\nname: native\ndescription: Native description\ntags: [invalid tag]\nresponsibility: [Foreign]\n---\n# Native\n");

        var inspection = await ReadAsync(temporary);
        var loader = Assert.Single(inspection.Sources, source => source.Source.Identity.CanonicalBasePath == ".agents/loader.md");
        var skill = Assert.Single(inspection.Sources, source => source.Source.Identity.CanonicalBasePath == ".agents/skills/native/SKILL.md");
        Assert.Equal(SourceAuthoredMetadataState.NotApplicable, loader.AuthoredMetadata.State);
        Assert.Equal(SourceAuthoredMetadataState.Complete, skill.AuthoredMetadata.State);
        Assert.Equal("Native description", skill.AuthoredMetadata.Description);
        foreach (var source in new[] { loader, skill })
        {
            Assert.Equal(FrameworkDocumentMetadataState.Missing, source.FrameworkMetadata.State);
            Assert.Null(source.AuthoredMetadata.FrameworkMetadata);
            Assert.Empty(source.FrameworkMetadata.Syntax.Members);
            Assert.Empty(source.WorkspaceIssues);
        }
    }

    private static string Document(bool root, string fields)
    {
        var yaml = root ? fields : $"open-forge:\n  {fields.Replace("\n", "\n  ", StringComparison.Ordinal)}\n";
        return $"---\n{yaml}---\n";
    }

    private static ValueTask<RouteSourceInspection> ReadAsync(TemporaryWorkspace temporary)
    {
        var resolver = new PhysicalPathResolver();
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        return new RouteSourceInspector(new SourceReadSessionReader(resolver), resolver)
            .ReadAsync(workspace, TestContext.Current.CancellationToken);
    }
}
