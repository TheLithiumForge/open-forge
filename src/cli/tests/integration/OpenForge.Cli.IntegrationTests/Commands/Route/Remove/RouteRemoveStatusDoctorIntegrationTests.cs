using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

public sealed class RouteRemoveStatusDoctorIntegrationTests
{
    [Fact(DisplayName = "Route Remove recovery attribution is observed by the existing Status and Doctor domains"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationStatusDoctor")]
    public async Task RouteRemoveRecoveryRemainsInExistingOperationalVocabulary()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-status-doctor");
        var plan = await workspace.BuildApplicationPlanAsync();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var prepared = await RouteRemoveRecoveryLifecycle.Create().PrepareAsync(
            new RouteRemoveRecoveryPreparationInput
            {
                Plan = plan,
                OperationId = operationId,
                Lease = lease,
            },
            TestContext.Current.CancellationToken);
        Assert.Equal(RouteRemoveRecoveryPreparationState.Prepared, prepared.State);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        var verified = await new RecoveryBundleReader().ReadFinalAsync(
            workspace.Workspace,
            preparation.BundlePath,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleReadState.Valid, verified.State);
        Assert.NotNull(verified.Verified);
        Assert.Equal(RecoveryBundleProducer.Route, verified.Verified!.Attribution.Producer);
        Assert.Equal(RecoveryBundleOperation.Remove, verified.Verified.Attribution.Operation);

        var status = await CliHostCapture.RunAsync(
            ["status", "--workspace", workspace.Workspace.LexicalRoot, "--json"],
            workspace.Workspace.LexicalRoot);
        Assert.Equal(3, status.ExitCode);
        Assert.Equal(string.Empty, status.Error);
        using var statusDocument = JsonDocument.Parse(status.Output);
        var statusRoot = statusDocument.RootElement;
        Assert.Equal("status", statusRoot.GetProperty("command").GetString());
        Assert.Equal(
            1,
            statusRoot.GetProperty("result")
                .GetProperty("recovery")
                .GetProperty("verifiedFinals")
                .GetProperty("value")
                .GetInt32());
        Assert.Contains(
            statusRoot.GetProperty("result").GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "recovery-candidate-verified");

        var doctor = await CliHostCapture.RunAsync(
            ["doctor", "--workspace", workspace.Workspace.LexicalRoot, "--json"],
            workspace.Workspace.LexicalRoot);
        Assert.Equal(3, doctor.ExitCode);
        using var doctorDocument = JsonDocument.Parse(doctor.Output);
        var doctorRoot = doctorDocument.RootElement;
        var domains = doctorRoot.GetProperty("result").GetProperty("domains").EnumerateArray().ToArray();
        Assert.Equal(
            [
                "workspace-entry",
                "recovery-residuals",
                "routes-metadata-overwrites-generated-navigation",
                "local-references",
                "framework-lifecycle",
                "extension-lifecycle",
            ],
            domains.Select(domain => domain.GetProperty("domain").GetString()));
        var recovery = Assert.Single(
            domains,
            domain => domain.GetProperty("domain").GetString() == "recovery-residuals");
        var finding = Assert.Single(
            recovery.GetProperty("findings").EnumerateArray(),
            candidate => candidate.GetProperty("kind").GetString() == "recovery.bundle-recognized");
        Assert.Equal("recovery-item", finding.GetProperty("subject").GetProperty("kind").GetString());
        Assert.Contains(
            finding.GetProperty("evidence").EnumerateArray(),
            evidence => evidence.GetProperty("kind").GetString() == "integrity"
                && evidence.GetProperty("state").GetString() == "verified");
        Assert.Equal(string.Empty, doctor.Error);
    }

    [Fact(DisplayName = "Route Remove application keeps the existing recovery attribution tuple while completing"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationStatusDoctor")]
    public async Task ApplicationCompletesWithRouteRemoveAttribution()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-status-doctor-apply");
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId, "--json"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        using var document = JsonDocument.Parse(output.ToString());
        Assert.Equal("route remove", document.RootElement.GetProperty("command").GetString());
        Assert.Equal("removed", document.RootElement.GetProperty("result")
            .GetProperty("recovery").GetProperty("state").GetString());
    }

    [Fact(DisplayName = "Route Remove failure leaves the six existing Doctor domains available for diagnosis"),
     Trait("Feature", "route-remove"), Trait("Evidence", "IntegrationStatusDoctor")]
    public async Task BlockedApplicationRetainsDoctorDomainCoverage()
    {
        using var workspace = RouteRemoveIntegrationWorkspace.Create("route-remove-status-doctor-blocked");
        workspace.SeedFrameworkClaim(RouteRemoveIntegrationWorkspace.LeafPath);
        var output = new StringWriter();
        var error = new StringWriter();
        var completion = await workspace.RunAsync(
            ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId],
            output,
            error);

        Assert.Equal(5, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, completion.Status);
        Assert.Contains("route-remove.ownership-claimed", error.ToString(), StringComparison.Ordinal);
        Assert.Equal(string.Empty, output.ToString());

        var doctorOutput = new StringWriter();
        var doctorError = new StringWriter();
        var doctor = await workspace.RunAsync(
            ["doctor", "--workspace", workspace.Path, "--json"],
            doctorOutput,
            doctorError);
        Assert.Equal(3, doctor.ExitCode);
        Assert.Equal(CliSemanticStatus.Incomplete, doctor.Status);
        Assert.Equal(string.Empty, doctorError.ToString());
        using var doctorDocument = JsonDocument.Parse(doctorOutput.ToString());
        Assert.Equal(
            6,
            doctorDocument.RootElement.GetProperty("result").GetProperty("domains").GetArrayLength());
    }

}
