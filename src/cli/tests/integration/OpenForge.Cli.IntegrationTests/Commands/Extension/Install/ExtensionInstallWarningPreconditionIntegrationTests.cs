using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Application;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Presentation.Extension.Install.Shared.Wording;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

public sealed class ExtensionInstallWarningPreconditionIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Install precondition accepts the unchanged warning set under a held lease"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task UnchangedWarningSetPassesPreconditionValidation()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-install-warning-precondition-positive");
        await workspace.SeedFrameworkAsync();
        const string notePath = ".agents/patterns/old-note.md";
        const string healthyPath = ".agents/patterns/healthy.md";
        workspace.CreateOccupant(notePath, MalformedDocument("Old note"));
        workspace.CreateOccupant(healthyPath, Document("Healthy"));
        using var source = ExtensionInstallCatalogue.Create(
            "extension-install-warning-precondition-positive-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/guidance/toolkit.md", Document("Toolkit")));

        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var revalidator = new MutationRevalidator(validator);
        var foundationReader = new ExtensionInstallFoundationReader(resolver);
        var planner = new ExtensionInstallPlanner(
            ExtensionInteractionTestFactory.UnavailableSelection,
            ExtensionInstallWording.Selection(),
            ExtensionInteractionTestFactory.UnavailableInstallConfirmation,
            static paths => new CliConfirmQuestion(ExtensionInstallWording.ReplaceExisting(paths)),
            resolver);
        var request = new ExtensionInstallRequest(
            workspace.Workspace,
            ExtensionInstallMode.Apply,
            ["toolkit"],
            all: false,
            source.Path,
            force: false,
            automatic: true,
            allowInteraction: false);
        var planBuild = await planner.BuildAsync(
            request,
            TestContext.Current.CancellationToken);
        var plan = Assert.IsType<ExtensionInstallPlan>(planBuild.Plan);
        var currentObservation = await foundationReader.ReadAsync(
            plan.Request,
            plan.ValidationPackages,
            TestContext.Current.CancellationToken);
        Assert.Null(currentObservation.Finding);
        Assert.NotNull(currentObservation.Foundation);
        var current = currentObservation.Foundation!;
        var expectedWarning = (
            ExtensionInstallFindingCode.MetadataProjectionSkipped,
            (string?)notePath,
            "Every direct routed child requires complete authored source metadata.");
        Assert.Equal(new[] { expectedWarning }, FindingIdentities(plan.TopologyFindings));
        Assert.True(ExtensionInstallApplicationPreconditionValidator.TopologyEquals(
            plan.Topology,
            current.Topology));
        Assert.Equal(
            FindingIdentities(plan.TopologyFindings),
            FindingIdentities(current.TopologyFindings));

        var workspaceBeforeValidation = workspace.Snapshot();
        var sourceBeforeValidation = source.Snapshot();
        using var lockStore = WorkspaceLockTestStore.Create(
            "extension-install-warning-precondition-positive-locks");
        var operationId = Guid.NewGuid();
        var lockResult = await lockStore.AcquireAsync(
            new WorkspaceLockRequest(
                workspace.Workspace,
                ExtensionInstallDefinitions.CommandIdentity,
                operationId),
            TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(lockResult.Lease);

        var precondition = await new ExtensionInstallApplicationPreconditionValidator(
            new ExtensionInstallSourceResolver(resolver),
            foundationReader,
            revalidator,
            validator).ValidateAsync(
                plan,
                lease,
                TestContext.Current.CancellationToken);

        Assert.Null(precondition.Finding);
        Assert.NotNull(precondition.Validation);
        Assert.Equal(MutationValidationState.Valid, precondition.Validation!.State);
        Assert.True(lease.IsHeldFor(workspace.Workspace));
        Assert.Equal(workspaceBeforeValidation, workspace.Snapshot());
        Assert.Equal(sourceBeforeValidation, source.Snapshot());
        Assert.Equal(
            System.Text.Encoding.UTF8.GetBytes(MalformedDocument("Old note")),
            File.ReadAllBytes(workspace.Combine(notePath)));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Extension Install precondition rejects a changed warning set before application"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task ChangedWarningSetIsTargetChangedBeforeApplication()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-install-warning-precondition-negative");
        await workspace.SeedFrameworkAsync();
        const string notePath = ".agents/patterns/old-note.md";
        const string healthyPath = ".agents/patterns/healthy.md";
        workspace.CreateOccupant(notePath, MalformedDocument("Old note"));
        workspace.CreateOccupant(healthyPath, Document("Healthy"));
        using var source = ExtensionInstallCatalogue.Create(
            "extension-install-warning-precondition-negative-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/guidance/toolkit.md", Document("Toolkit")));
        var sourceBeforePlanning = source.Snapshot();

        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var revalidator = new MutationRevalidator(validator);
        var foundationReader = new ExtensionInstallFoundationReader(resolver);
        var planner = new ExtensionInstallPlanner(
            ExtensionInteractionTestFactory.UnavailableSelection,
            ExtensionInstallWording.Selection(),
            ExtensionInteractionTestFactory.UnavailableInstallConfirmation,
            static paths => new CliConfirmQuestion(ExtensionInstallWording.ReplaceExisting(paths)),
            resolver);
        var planBuild = await planner.BuildAsync(
            new ExtensionInstallRequest(
                workspace.Workspace,
                ExtensionInstallMode.Apply,
                ["toolkit"],
                all: false,
                source.Path,
                force: false,
                automatic: true,
                allowInteraction: false),
            TestContext.Current.CancellationToken);
        var plan = Assert.IsType<ExtensionInstallPlan>(planBuild.Plan);
        var expectedWarning = (
            ExtensionInstallFindingCode.MetadataProjectionSkipped,
            (string?)notePath,
            "Every direct routed child requires complete authored source metadata.");
        Assert.Equal(new[] { expectedWarning }, FindingIdentities(plan.TopologyFindings));

        var oldNoteBytes = File.ReadAllBytes(workspace.Combine(notePath));
        var healthyMalformed = MalformedDocument("Healthy");
        workspace.ReplaceText(healthyPath, healthyMalformed);
        var workspaceBeforeValidation = workspace.Snapshot();
        var sourceBeforeValidation = source.Snapshot();
        var currentObservation = await foundationReader.ReadAsync(
            plan.Request,
            plan.ValidationPackages,
            TestContext.Current.CancellationToken);
        Assert.Null(currentObservation.Finding);
        Assert.NotNull(currentObservation.Foundation);
        var current = currentObservation.Foundation!;
        Assert.True(ExtensionInstallApplicationPreconditionValidator.TopologyEquals(
            plan.Topology,
            current.Topology));
        Assert.True(plan.Topology.ProtectedPaths.SetEquals(current.Topology.ProtectedPaths));
        var currentWarning = (
            ExtensionInstallFindingCode.MetadataProjectionSkipped,
            (string?)healthyPath,
            "Every direct routed child requires complete authored source metadata.");
        Assert.Equal(
            new[] { currentWarning, expectedWarning },
            FindingIdentities(current.TopologyFindings));
        Assert.False(FindingIdentities(plan.TopologyFindings)
            .SequenceEqual(FindingIdentities(current.TopologyFindings)));
        Assert.Equal(sourceBeforePlanning, source.Snapshot());

        using var lockStore = WorkspaceLockTestStore.Create(
            "extension-install-warning-precondition-negative-locks");
        var operationId = Guid.NewGuid();
        var lockResult = await lockStore.AcquireAsync(
            new WorkspaceLockRequest(
                workspace.Workspace,
                ExtensionInstallDefinitions.CommandIdentity,
                operationId),
            TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(lockResult.Lease);

        var precondition = await new ExtensionInstallApplicationPreconditionValidator(
            new ExtensionInstallSourceResolver(resolver),
            foundationReader,
            revalidator,
            validator).ValidateAsync(
                plan,
                lease,
                TestContext.Current.CancellationToken);

        Assert.False(precondition.IsValid);
        Assert.Equal(
            ExtensionInstallFindingCode.TargetChanged,
            precondition.Finding?.Code);
        Assert.Null(precondition.Validation);
        Assert.Equal(workspaceBeforeValidation, workspace.Snapshot());
        Assert.Equal(sourceBeforeValidation, source.Snapshot());
        Assert.True(lease.IsHeldFor(workspace.Workspace));
        Assert.Equal(oldNoteBytes, File.ReadAllBytes(workspace.Combine(notePath)));
        Assert.Equal(
            System.Text.Encoding.UTF8.GetBytes(healthyMalformed),
            File.ReadAllBytes(workspace.Combine(healthyPath)));
    }

    private static IReadOnlyList<(ExtensionInstallFindingCode Code, string? Target, string Cause)> FindingIdentities(
        IEnumerable<ExtensionInstallFinding> findings)
        => [.. findings
            .OrderBy(finding => finding.Code)
            .ThenBy(finding => finding.Target is null ? 0 : 1)
            .ThenBy(finding => finding.Target, StringComparer.Ordinal)
            .ThenBy(finding => finding.Cause, StringComparer.Ordinal)
            .Select(finding => (finding.Code, finding.Target, finding.Cause))];

    private static string MalformedDocument(string name)
        => $"---\nopen-forge:\n  description: [\n---\n# {name}\n";

    private static string Document(string name)
        => OpenForge.Cli.TestSupport.OpenForgeDocumentSeed.Metadata(
            name,
            ["Extension"],
            $"# {name}\n");
}
