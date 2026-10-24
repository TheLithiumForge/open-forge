using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Status;
using OpenForge.Cli.Core.Commands.Status.Models.Request;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Operational;

[Trait("Feature", "workspace-layout"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class WorkspaceLayoutObservationIntegrationTests
{
    private const string Fixture = "operational-layout";
    private const string CustomRoot = "docs/forge";
    private const string Leaf = "policy" + SourceFormClassifier.MarkdownExtension;
    private const string SkillName = "example";
    private const string SkillPath = WorkspaceLayout.NativeSkillsPath + "/" + SkillName + "/" + SourceFormClassifier.NativeSkillFileName;
    private const string Loader = "# Loader\n\n## Entries\n";
    private const string Note = "---\ndescription: An authored policy\ntags: [Guidance]\n---\n# Policy\n";
    private const string Skill = "---\nname: " + SkillName + "\ndescription: An authored Skill\n---\n# Skill\n";
    private const string InvalidSettings = "{broken";

    [Theory(DisplayName = "Unavailable settings retain every diagnostic domain without selecting a root or changing bytes")]
    [InlineData(false), InlineData(true)]
    public async Task UnavailableSettingsRetainDomains(bool unavailable)
    {
        using var temporary = TemporaryWorkspace.Create(Fixture);
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        if (unavailable) temporary.CreateDirectory(WorkspaceSettingsDefinitions.RelativePath);
        else temporary.CreateFile(WorkspaceSettingsDefinitions.RelativePath, InvalidSettings);
        temporary.CreateFile(CustomRoot + "/" + WorkspaceLayoutNames.LoaderFileName, Loader);
        temporary.CreateFile(WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.LoaderFileName, Loader);
        temporary.CreateFile(CustomRoot + "/" + Leaf, Note);
        temporary.CreateFile(SkillPath, Skill);
        var before = Directory.GetFiles(temporary.Path, "*", SearchOption.AllDirectories)
            .ToDictionary(path => path, File.ReadAllBytes);
        var token = TestContext.Current.CancellationToken;
        var paths = new PhysicalPathResolver();
        var observation = await new WorkspaceLayoutReader(paths).ReadAsync(workspace, token);
        var contributors = RepairOperationalContributorFactory.Create(paths);
        var routes = await contributors.Routes.ReadDoctorAsync(observation, new SourceSharing([]), token);
        var references = await contributors.LocalReferences.ReadDoctorAsync(observation, token, routes);
        var doctor = await new DoctorOperation(contributors).ExecuteAsync(new DoctorRequest(workspace), token);
        var status = await new StatusOperation(contributors, paths).ExecuteAsync(new StatusRequest(workspace), token);

        Assert.Null(observation.Layout);
        Assert.Null(routes.Catalogue);
        Assert.Empty(routes.Sources);
        Assert.Empty(references.References);
        Assert.Same(observation.Issues[0].Cause, routes.UnavailableCause);
        Assert.Same(observation.Issues[0].Cause, references.UnavailableCause);
        Assert.NotEqual(CliSemanticStatus.Failed, doctor.Status);
        Assert.NotEqual(CliSemanticStatus.Failed, status.Status);
        Assert.Equal(Enum.GetValues<DoctorDomainKind>(), doctor.Diagnosis.Domains.Select(domain => domain.Domain));
        foreach (var kind in new[] { DoctorDomainKind.WorkspaceEntry, DoctorDomainKind.RoutesMetadataOverwritesGeneratedNavigation, DoctorDomainKind.LocalReferences })
            Assert.Equal(DoctorCoverageState.Incomplete, Assert.Single(doctor.Diagnosis.Domains, domain => domain.Domain == kind).Coverage);
        Assert.Equal(DoctorCoverageState.Complete, Assert.Single(doctor.Diagnosis.Domains,
            domain => domain.Domain == DoctorDomainKind.RecoveryResiduals).Coverage);
        Assert.Null(doctor.Diagnosis.CoverageCounts?.RoutesChecked);
        Assert.Null(doctor.Diagnosis.CoverageCounts?.LinksChecked);
        Assert.Equal(StatusValueState.Unavailable, status.Facts.Context.TotalAvailable.Files.State);
        Assert.Equal(before.Keys.Order(), Directory.GetFiles(temporary.Path, "*", SearchOption.AllDirectories).Order());
        foreach (var (path, bytes) in before) Assert.Equal(bytes, File.ReadAllBytes(path));
    }

    [Theory(DisplayName = "Operational readers retain one selected layout and settings observation without effects")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(CustomRoot)]
    public async Task RetainedObservationSurvivesSettingsChange(string root)
    {
        using var temporary = TemporaryWorkspace.Create(Fixture);
        var workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        temporary.CreateFile(WorkspaceSettingsDefinitions.RelativePath,
            $$$"""{"{{{WorkspaceSettingsDefinitions.MainRootProperty}}}":"{{{root}}}"}""");
        temporary.CreateFile(root + "/" + WorkspaceLayoutNames.LoaderFileName, Loader);
        temporary.CreateFile(root + "/" + Leaf, Note);
        temporary.CreateFile(SkillPath, Skill);
        var token = TestContext.Current.CancellationToken;
        var paths = new PhysicalPathResolver();
        var observation = await new WorkspaceLayoutReader(paths).ReadAsync(workspace, token);
        var layout = Assert.IsType<WorkspaceLayout>(observation.Layout);
        File.WriteAllText(temporary.Combine(WorkspaceSettingsDefinitions.RelativePath), InvalidSettings);
        var before = Directory.GetFiles(temporary.Path, "*", SearchOption.AllDirectories)
            .ToDictionary(path => path, File.ReadAllBytes);
        var contributors = RepairOperationalContributorFactory.Create(paths);
        var sharing = new SourceSharing([]);
        var routes = await contributors.Routes.ReadDoctorAsync(observation, sharing, token);
        var status = await contributors.Routes.ReadStatusAsync(observation, sharing, token);
        var references = await contributors.LocalReferences.ReadDoctorAsync(observation, token, routes);
        var entry = await contributors.WorkspaceEntry.ReadStatusAsync(workspace, observation, token);
        var framework = await contributors.FrameworkLifecycle.ReadDoctorAsync(observation, token);
        var extensions = await contributors.ExtensionLifecycle.ReadDoctorAsync(observation, token);

        Assert.NotNull(routes.Catalogue);
        Assert.Same(layout, routes.Catalogue.Layout);
        Assert.Same(routes.Catalogue, references.Catalogue);
        Assert.NotNull(routes.Catalogue.FindByPath(root + "/" + Leaf));
        Assert.Equal(SourceDocumentForm.Skill, routes.Catalogue.FindByPath(SkillPath)?.Base.Form);
        Assert.Equal(root + "/" + WorkspaceLayoutNames.LoaderFileName, entry.LoaderPath);
        Assert.Same(observation.Ownership, framework.Ownership);
        Assert.Same(observation.Ownership, extensions.Ownership);
        Assert.NotNull(status);
        Assert.Equal(before.Keys.Order(), Directory.GetFiles(temporary.Path, "*", SearchOption.AllDirectories).Order());
        foreach (var (path, bytes) in before) Assert.Equal(bytes, File.ReadAllBytes(path));
    }
}
