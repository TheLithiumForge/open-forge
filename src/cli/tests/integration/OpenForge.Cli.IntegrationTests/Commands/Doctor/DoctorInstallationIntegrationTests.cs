using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Framework.Distribution.Operational;
using OpenForge.Cli.Core.Framework.Distribution.Operational.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.IntegrationTests.Framework.Distribution.Operational.Shared.Frontmatter;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Doctor;

public sealed class DoctorInstallationIntegrationTests
{
    [Fact(DisplayName = "Doctor reports matching root Framework delivery as current"), Trait("Boundary", "OS"), Trait("Feature", "doctor-command"), Trait("Evidence", "Integration")]
    public async Task MatchingRootDeliveryIsCurrent()
    {
        using var workspace = TemporaryWorkspace.Create("doctor-root-framework");
        FrameworkLifecycleFrontmatterFixture.Write(workspace, FrontmatterForm.Root);
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, """{"schemaVersion":1,"frontmatter":"root"}""");
        var view = await ReadAsync(workspace);
        Assert.Equal(FrameworkManagedSetState.Current, view.ManagedSet);
        Assert.Equal(OperationalTargetState.Current, Assert.Single(view.Targets).Target.State);
        Assert.Empty(InspectTarget(view));
    }

    [Theory(DisplayName = "Doctor reports opposite Framework forms with the existing managed-change finding and Update advice")]
    [Trait("Boundary", "OS"), Trait("Feature", "doctor-command"), Trait("Evidence", "Integration")]
    [InlineData("root", (int)FrontmatterForm.Scoped)]
    [InlineData("scoped", (int)FrontmatterForm.Root)]
    public async Task OppositeFormReportsManagedChange(string setting, int authoredForm)
    {
        using var workspace = TemporaryWorkspace.Create("doctor-opposite-framework");
        FrameworkLifecycleFrontmatterFixture.Write(workspace, (FrontmatterForm)authoredForm);
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, $$"""{"schemaVersion":1,"frontmatter":"{{setting}}"}""");
        var view = await ReadAsync(workspace);
        Assert.Equal(OperationalTargetState.Changed, Assert.Single(view.Targets).Target.State);
        var finding = Assert.Single(InspectTarget(view));
        Assert.Equal("framework.managed-changed", DoctorDefinitions.ReadFindingKind(finding.Kind));
        Assert.Equal(DoctorNextOperation.Update, Assert.Single(finding.Actions).Operation);
    }

    [Theory(DisplayName = "Doctor leaves Framework comparisons unavailable for invalid or unavailable settings")]
    [Trait("Boundary", "OS"), Trait("Feature", "doctor-command"), Trait("Evidence", "Integration")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InvalidSettingsMakesComparisonUnavailable(bool unavailable)
    {
        using var workspace = TemporaryWorkspace.Create("doctor-invalid-form");
        FrameworkLifecycleFrontmatterFixture.Write(workspace, FrontmatterForm.Scoped);
        if (unavailable) workspace.CreateDirectory(WorkspaceSettingsDefinitions.RelativePath);
        else workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, """{"schemaVersion":1,"frontmatter":"unknown"}""");
        var view = await ReadAsync(workspace);
        Assert.Equal(OperationalViewState.Incomplete, view.State);
        Assert.Equal(FrameworkManagedSetState.Unavailable, view.ManagedSet);
        var target = Assert.Single(view.Targets).Target;
        Assert.Equal(OperationalTargetState.Unavailable, target.State);
        Assert.Null(target.IntendedFingerprint);
        Assert.Empty(InspectTarget(view));
    }

    private static async Task<FrameworkLifecycleDoctorView> ReadAsync(TemporaryWorkspace workspace)
    {
        var before = workspace.SnapshotHashes();
        var resolver = new PhysicalPathResolver();
        var contributor = new FrameworkLifecycleOperationalContributor(resolver, new FrameworkLifecycleTargetReader(resolver));
        var view = await contributor.ReadDoctorAsync(
            new CliWorkspace(lexicalRoot: workspace.Path, physicalRoot: workspace.Path, selectedBy: CliWorkspaceSelectionMethod.ExplicitWorkspace),
            TestContext.Current.CancellationToken);
        Assert.Equal(before, workspace.SnapshotHashes());
        return view;
    }

    private static IReadOnlyList<DoctorFinding> InspectTarget(FrameworkLifecycleDoctorView view)
    {
        var findings = new List<DoctorFinding>();
        var limitations = new List<DoctorLimitation>();
        var coverage = DoctorCoverageState.Complete;
        FrameworkManagedTargetDoctorInspector.Inspect(Assert.Single(view.Targets), findings, limitations, ref coverage);
        return findings;
    }
}
