using OpenForge.Cli.Core.Commands.Tool.Run;
using OpenForge.Cli.Core.Commands.Tool.Run.Models;
using OpenForge.Cli.Core.Commands.Status;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Commands.Tool.Shared.Reporting;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.IntegrationTests.Commands.Tool.Run.Shared;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Run;

[Trait("Feature", "tool-run"), Trait("Evidence", "Integration")]
public sealed class ToolRunMissingReaderTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact(DisplayName = "Workspace reports receive the same passive missing-tool fact and resolved workspace as Run")]
    public async Task SharesMissingFact()
    {
        using var fixture = await ToolRunFixture.CreateAsync(Token);
        await fixture.RewriteRegistryAsync([], Token);
        var before = fixture.Temporary.SnapshotHashes();
        var reader = new ToolRunMissingReader(fixture.Operation);
        var missing = Assert.Single(await reader.ReadAsync(fixture.Workspace, Token));
        var run = Assert.IsType<ToolRunRefused>(await fixture.Operation.ResolveAsync(fixture.Request, Token)).Failure;
        Assert.Equal(run.Code, missing.Code);
        Assert.Equal(run.Id, missing.Id);
        Assert.Equal(run.Workspace?.PhysicalRoot, missing.Workspace?.PhysicalRoot);
        Assert.Equal(before, fixture.Temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "Unavailable store evidence never becomes a missing-tool fact in workspace reports")]
    public async Task KeepsUnavailableSeparate()
    {
        using var fixture = await ToolRunFixture.CreateAsync(Token);
        await File.WriteAllTextAsync(fixture.Store.State.At(ToolStoreRegistryDefinitions.File), "{", Token);
        var reader = new ToolRunMissingReader(fixture.Operation);
        Assert.Empty(await reader.ReadAsync(fixture.Workspace, Token));
        var run = Assert.IsType<ToolRunRefused>(await fixture.Operation.ResolveAsync(fixture.Request, Token)).Failure;
        Assert.Equal(ToolRunFailureCode.VerificationUnavailable, run.Code);
    }

    [Fact(DisplayName = "Workspace Status and Doctor wire the same missing local pin without writing fixture bytes")]
    public async Task ReportsMissingPinInWorkspaceCommands()
    {
        using var fixture = await ToolRunFixture.CreateAsync(Token);
        await fixture.RewriteRegistryAsync([], Token);
        var before = fixture.Temporary.SnapshotHashes();
        var paths = new PhysicalPathResolver();
        var contributors = RepairOperationalContributorFactory.Create(paths);
        var missing = new ToolRunMissingReader(fixture.Operation);
        var status = await new StatusOperation(contributors, paths, missing).ExecuteAsync(new(fixture.Workspace), Token);
        Assert.Single(status.Findings, finding => finding.Code == StatusFindingCode.ToolDeclaredMissing);
        var doctor = await new DoctorOperation(contributors, missingTools: missing).ExecuteAsync(new(fixture.Workspace), Token);
        var finding = Assert.Single(doctor.Diagnosis.Domains.SelectMany(domain => domain.Findings), finding => finding.Kind == DoctorFindingKind.ToolDeclaredMissing);
        Assert.Equal(ToolContractCodes.RestoreCommand, Assert.Single(finding.Actions).Command);
        Assert.Equal(before, fixture.Temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "Missing-tool reports keep the caller's control-file observation instead of rereading changed records")]
    public async Task KeepsObservedWorkspaceRecords()
    {
        using var fixture = await ToolRunFixture.CreateAsync(Token);
        await fixture.RewriteRegistryAsync([], Token);
        var observed = await new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync(fixture.Workspace, Token);
        File.Delete(Path.Combine(fixture.Workspace.LexicalRoot, WorkspaceOwnershipDefinitions.RelativePath));
        var reader = new ToolRunMissingReader(fixture.Operation);
        var missing = Assert.Single(await reader.ReadAsync(fixture.Workspace, new(observed.Settings, observed.Ownership), Token));
        Assert.Equal(ToolRunFailureCode.DeclaredToolMissing, missing.Code);
        var current = Assert.IsType<ToolRunRefused>(await fixture.Operation.ResolveAsync(fixture.Request, Token)).Failure;
        Assert.Equal(ToolRunFailureCode.VerificationUnavailable, current.Code);
    }
}
