using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Framework.Libraries.Operational;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Permissions;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;
using OpenForge.Cli.IntegrationTests.Commands.Shared.LibraryRecovery;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair;

[Trait("Feature", "library-git-ignore"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class LibraryGitIgnoreRepairIntegrationTests
{
    [Theory]
    [InlineData("granted", false), InlineData("granted", true), InlineData("missing", false), InlineData("revoked", true)]
    public static async Task AttributableIgnoreInverseUsesOnlyCurrentExactTargetGrant(string grant, bool replace)
    {
        using var workspace = new LibraryResidualWorkspace();
        await workspace.PrepareAsync("link-create", includeIgnore: true, replaceIgnore: replace);
        if (grant != "missing") workspace.Files.Write(".agents/open-forge.json", """{"allowInstallPaths":[".gitignore"]}""");
        var token = TestContext.Current.CancellationToken;
        var libraries = await new LibraryOperationalContributor().ReadDoctorAsync(workspace.Files.Workspace, token);
        var residual = workspace.Evidence.Residual;
        var recovery = new RecoveryResidualDoctorView(OperationalViewState.Complete,
            [RecoveryDoctorCandidateObservation.Create(residual.Candidate, null, residual)], null);
        var attributed = await LibraryResidualAttributionReader.ReadAsync(workspace.Files.Workspace, libraries, recovery, token);
        var evidence = Assert.Single(attributed, item => item.Entry.Input.Context.Entry.TargetPath == ".gitignore");
        Assert.Equal("team-knowledge", evidence.LibraryId.Value);
        var permission = await LibraryRecoveryPermissionReader.ObserveAsync(workspace.Files.Workspace, evidence, token);
        Assert.Equal(grant != "missing", permission.IsAdmitted);
        Assert.Equal(new[] { ".gitignore" }, permission.Evaluation.Required);
        var plan = RepairLibraryRecoveryPlanner.Build(new RepairLibraryPlanningInput
        {
            Request = new RepairRequest(workspace.Files.Workspace, RepairMode.Apply, automatic: false, [], allowInteraction: true),
            References = [],
            Libraries = [.. attributed.Select(item => new RepairLibraryRecoveryProposal(item))],
            PromptRelinks = [],
            PromptLibraries = [new RepairLibraryRecoveryProposal(evidence)],
        });
        var application = RepairOperationFactory.CreateDefaultComponents().Application;
        if (grant == "revoked")
        {
            Assert.Equal(RepairPreflightState.Ready, (await application.PreflightAsync(plan, token)).Outcome.Preflight.State);
            workspace.Files.Replace(".agents/open-forge.json", """{"allowInstallPaths":[]}""");
        }
        var source = workspace.Sources();
        var beforeIgnore = File.ReadAllBytes(workspace.Files.Absolute(".gitignore"));
        var bundle = File.ReadAllBytes(workspace.Preparation.BundlePath);
        var outcome = await application.ExecuteAsync(plan, [], token);
        Assert.Equal(grant == "granted" ? 1 : 0, outcome.Application.AppliedEffects);
        if (grant == "granted")
        {
            if (replace) Assert.Equal("# authored\n", File.ReadAllText(workspace.Files.Absolute(".gitignore")));
            else Assert.False(File.Exists(workspace.Files.Absolute(".gitignore")));
        }
        else
        {
            Assert.Equal(RepairPreflightState.Blocked, outcome.Preflight.State);
            Assert.Equal(beforeIgnore, File.ReadAllBytes(workspace.Files.Absolute(".gitignore")));
        }
        Assert.Equal(source, workspace.Sources());
        Assert.Equal(bundle, File.ReadAllBytes(workspace.Preparation.BundlePath));
        Assert.Equal("../../shared/team-knowledge/.agents/directives/review.md", new FileInfo(workspace.Files.Absolute(".agents/directives/review.md")).LinkTarget);
    }
}
