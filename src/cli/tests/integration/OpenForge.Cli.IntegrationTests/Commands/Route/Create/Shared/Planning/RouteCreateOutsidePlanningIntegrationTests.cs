using System.Text;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models.Syntax;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Create.Shared.Planning;

[Trait("Feature", "route-create"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RouteCreateOutsidePlanningIntegrationTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Outside Create preserves supplied metadata fields and leaves each omitted field absent")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(RouteCreateOutsideWorkspace.CustomRoot, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true), InlineData(RouteCreateOutsideWorkspace.CustomRoot, true)]
    public async Task SuppliedMetadata(string mainRoot, bool descriptionOnly)
    {
        using var fixture = new RouteCreateOutsideWorkspace(mainRoot);
        if (mainRoot == RouteCreateOutsideWorkspace.CustomRoot)
            fixture.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, fixture.Settings([RouteCreateOutsideWorkspace.Directory],
                $",\"{WorkspaceSettingsDefinitions.FrontmatterProperty}\":\"{WorkspaceSettingsDefinitions.FrontmatterRootName}\""));
        var path = RouteCreateOutsideWorkspace.Directory + "/note" + SourceFormClassifier.MarkdownExtension;
        const string responsibility = "Authored destination responsibility";
        const string condition = "src/**/*.cs";
        var pattern = ApplyToPatternMatcher.Parse(condition).Pattern ?? throw new InvalidOperationException();
        var request = new RouteCreateRequest(fixture.Workspace, SourceWorkspaceRelativePath.LiteralReferencePrefix + path,
            new(descriptionOnly ? RouteCreateOutsideWorkspace.Description : null,
                descriptionOnly ? [] : [RouteCreateOutsideWorkspace.Tag], responsibility, [pattern]), null, RouteCreateMode.DryRun);
        var build = await new RouteCreatePlanBuilder().BuildAsync(request, Token);
        var plan = Assert.IsType<RouteCreatePlan>(build.Plan);
        var facts = new FrameworkDocumentMetadataParser().Parse(new MarkdownDocumentParser().Parse(
            Encoding.UTF8.GetString(plan.IntendedTargetBytes.AsSpan())), FrameworkMetadataReadScope.RoutedSource);
        Assert.Equal(descriptionOnly ? RouteCreateOutsideWorkspace.Description : null, facts.ObservedDescription);
        Assert.Equal(mainRoot == RouteCreateOutsideWorkspace.CustomRoot ? FrontmatterForm.Root : FrontmatterForm.Scoped, facts.Syntax.AuthoredForm);
        Assert.Equal(descriptionOnly ? [] : new[] { RouteCreateOutsideWorkspace.Tag }, facts.ObservedTags);
        Assert.Equal(responsibility, Assert.Single(facts.Syntax.Members, member => member.Field == FrameworkMetadataField.Responsibility).Entry.Value.Scalar?.Value);
        Assert.Equal(new[] { condition }, facts.ApplyTo.Patterns.Select(value => value.Text));
        Assert.Contains(build.Formation.Findings, finding => finding.Code == RouteCreateFindingCode.OptionalMetadata);
    }

    [Theory(DisplayName = "Outside Create plans exact nested destinations inside listed and unlisted anchors without effects")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true), InlineData(WorkspaceLayout.DefaultMainRoot, false)]
    [InlineData(RouteCreateOutsideWorkspace.CustomRoot, true), InlineData(RouteCreateOutsideWorkspace.CustomRoot, false)]
    public async Task BoundedNestedPlan(string mainRoot, bool listed)
    {
        using var fixture = new RouteCreateOutsideWorkspace(mainRoot, listed: listed);
        var path = RouteCreateOutsideWorkspace.Directory + "/scope/topic/note" + SourceFormClassifier.MarkdownExtension;
        var before = fixture.Files.SnapshotHashes();
        var build = await fixture.Build(path);
        var plan = Assert.IsType<RouteCreatePlan>(build.Plan);
        Assert.Equal(before, fixture.Files.SnapshotHashes());
        Assert.Same(plan.LayoutRead.Layout, plan.NavigationFormation.Catalogue.Layout);
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + path, plan.TargetSource.Identity.AutomaticId);
        Assert.Equal(2, plan.DirectoryCreations.Length);
        Assert.Equal(new[] { RouteCreateOutsideWorkspace.Directory + "/scope", RouteCreateOutsideWorkspace.Directory + "/scope/topic",
            WorkspaceRoutePath.CanonicalEntrypointPath(RouteCreateOutsideWorkspace.Directory + "/scope"),
            WorkspaceRoutePath.CanonicalEntrypointPath(RouteCreateOutsideWorkspace.Directory + "/scope/topic"), path, fixture.Anchor },
            build.Formation.Effects.Select(effect => effect.Path));
        Assert.DoesNotContain(build.Formation.Effects, effect => effect.Path == fixture.Layout.LoaderPath);
        Assert.Single(plan.RecoveryTargets);
    }

    [Theory(DisplayName = "Outside Create requires its existing anchor and reports ambiguity before effects")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(RouteCreateOutsideWorkspace.CustomRoot, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true), InlineData(RouteCreateOutsideWorkspace.CustomRoot, true)]
    public async Task RequiredAnchor(string mainRoot, bool ambiguous)
    {
        using var fixture = new RouteCreateOutsideWorkspace(mainRoot, listed: false);
        if (ambiguous) fixture.Files.WriteText(RouteCreateOutsideWorkspace.Directory + "/" + SourceFormClassifier.IndexFileName, RouteCreateOutsideWorkspace.Document());
        else File.Delete(fixture.Files.Combine(fixture.Anchor));
        var before = fixture.Files.SnapshotHashes();
        var build = await fixture.Build(RouteCreateOutsideWorkspace.Directory + "/scope/note" + SourceFormClassifier.MarkdownExtension);
        Assert.Null(build.Plan);
        Assert.Contains(build.Formation.Findings, finding => finding.Code == (ambiguous ? RouteCreateFindingCode.RouteAmbiguous : RouteCreateFindingCode.ParentMissing));
        Assert.Empty(build.Formation.Effects);
        Assert.Equal(before, fixture.Files.SnapshotHashes());
    }

    [Theory(DisplayName = "Unavailable unrelated registrations do not block or expand the selected outside Create plan")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteCreateOutsideWorkspace.CustomRoot)]
    public async Task UnrelatedMissingRegistration(string mainRoot)
    {
        using var fixture = new RouteCreateOutsideWorkspace(mainRoot);
        fixture.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath,
            fixture.Settings([RouteCreateOutsideWorkspace.Directory, RouteCreateOutsideWorkspace.OtherDirectory]));
        var path = RouteCreateOutsideWorkspace.Directory + "/note" + SourceFormClassifier.MarkdownExtension;
        var plan = Assert.IsType<RouteCreatePlan>((await fixture.Build(path)).Plan);
        Assert.Equal(new[] { path, fixture.Anchor }, plan.Preview.Effects.Select(effect => effect.Path));
        var missing = await fixture.Build(RouteCreateOutsideWorkspace.OtherDirectory + "/note" + SourceFormClassifier.MarkdownExtension);
        Assert.Null(missing.Plan);
        Assert.Contains(missing.Formation.Findings, finding => finding.Code == RouteCreateFindingCode.TargetUnsafe);
    }

    [Theory(DisplayName = "An ordinary file include does not grant outside Create maintenance")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteCreateOutsideWorkspace.CustomRoot)]
    public async Task FileInclusionAlone(string mainRoot)
    {
        using var fixture = new RouteCreateOutsideWorkspace(mainRoot);
        fixture.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, fixture.Settings([],
            $",\"{WorkspaceSettingsDefinitions.RootsProperty}\":[\"{fixture.Anchor}\"]"));
        var before = fixture.Files.SnapshotHashes();
        var build = await fixture.Build(RouteCreateOutsideWorkspace.Directory + "/note" + SourceFormClassifier.MarkdownExtension);
        Assert.Null(build.Plan);
        Assert.Contains(build.Formation.Findings, finding => finding.Code == RouteCreateFindingCode.InvalidTarget);
        Assert.Equal(before, fixture.Files.SnapshotHashes());
    }

    [Theory(DisplayName = "Outside category names and SKILL filenames retain Create draft metadata and ordinary classification")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, WorkspaceCategoryNames.Directives)]
    [InlineData(RouteCreateOutsideWorkspace.CustomRoot, WorkspaceCategoryNames.Directives)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, WorkspaceCategoryNames.Memory)]
    [InlineData(RouteCreateOutsideWorkspace.CustomRoot, WorkspaceCategoryNames.Memory)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, WorkspaceLayoutNames.SkillsDirectoryName)]
    [InlineData(RouteCreateOutsideWorkspace.CustomRoot, WorkspaceLayoutNames.SkillsDirectoryName)]
    public async Task GenericOutsideMetadata(string mainRoot, string directory)
    {
        using var fixture = new RouteCreateOutsideWorkspace(mainRoot, directory);
        var path = directory + "/scope/" + SourceFormClassifier.NativeSkillFileName;
        var build = await fixture.Build(path, optional: true);
        var plan = Assert.IsType<RouteCreatePlan>(build.Plan);
        Assert.Equal(SourceDocumentForm.Markdown, plan.TargetSource.Base.Form);
        Assert.Contains(build.Formation.Findings, finding => finding.Code == RouteCreateFindingCode.OptionalMetadata);
        var reader = new FrameworkDocumentMetadataParser();
        foreach (var change in plan.FileChanges.Where(change => change.Kind == PlannedFileChangeKind.Create))
        {
            var facts = reader.Parse(new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(change.IntendedBytes.AsSpan())), FrameworkMetadataReadScope.RoutedSource);
            Assert.Null(facts.Metadata?.Description);
            Assert.True(facts.Metadata is null || facts.Metadata.Tags.IsEmpty);
        }
    }
}
