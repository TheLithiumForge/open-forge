using OpenForge.Cli.Core.Commands.Extension.Create;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Application;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Create;

public sealed class ExtensionCreateStagingIntegrationTests
{
    [Fact(DisplayName = "Extension Create cancellation after planning reports planned destination and no started scaffold effects"),
     Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task CancellationAfterPlanningRetainsTruthfulPreEffectVerification()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-staging-cancel");
        var operation = CreateOperation();
        var request = Request(catalogue.Path);
        var planning = await operation.PlanAsync(request, TestContext.Current.CancellationToken);
        var plan = Assert.IsType<ExtensionCreatePlan>(planning.Plan);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await operation.ApplyAsync(plan, cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(ExtensionCreateVerificationState.Verified, result.Verification.Catalogue);
        Assert.Equal(ExtensionCreateVerificationState.Planned, result.Verification.Destination);
        Assert.Equal(ExtensionCreateVerificationState.NotStarted, result.Verification.Manifest);
        Assert.Equal(ExtensionCreateVerificationState.NotStarted, result.Verification.Payload);
        Assert.Empty(result.AppliedEffects);
        Assert.False(Directory.Exists(catalogue.Combine("development-toolkit")));
    }

    [Fact(DisplayName = "Extension Create rejects undefined request and plan modes before effects"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task UndefinedModesAreRejectedBeforeEffects()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-staging-undefined-mode");
        var operation = CreateOperation();
        var before = catalogue.SnapshotHashes();
        var undefined = (ExtensionCreateMode)int.MaxValue;

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => operation.ExecuteAsync(
                Request(catalogue.Path) with { Mode = undefined },
                TestContext.Current.CancellationToken).AsTask());
        Assert.Equal(before, catalogue.SnapshotHashes());

        var planning = await operation.PlanAsync(
            Request(catalogue.Path),
            TestContext.Current.CancellationToken);
        var plan = Assert.IsType<ExtensionCreatePlan>(planning.Plan);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => operation.ApplyAsync(
                plan with { Mode = undefined },
                TestContext.Current.CancellationToken).AsTask());
        Assert.Equal(before, catalogue.SnapshotHashes());
    }

    [Fact(DisplayName = "Extension Create rejects an undefined effect kind before the writer handles application failures"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task UndefinedEffectKindEscapesBeforeWrites()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-staging-undefined-effect");
        var operation = CreateOperation();
        var planning = await operation.PlanAsync(
            Request(catalogue.Path),
            TestContext.Current.CancellationToken);
        var plan = Assert.IsType<ExtensionCreatePlan>(planning.Plan);
        var writer = new ExtensionCreateDestinationWriter();
        var before = catalogue.SnapshotHashes();
        var effect = plan.IntendedEffects[0] with { Kind = (ExtensionCreateEffectKind)int.MaxValue };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => writer.ApplyAsync(
                plan,
                effect,
                TestContext.Current.CancellationToken).AsTask());

        Assert.Equal(before, catalogue.SnapshotHashes());
    }

    [Fact(DisplayName = "Extension Create rejects a destination occupant introduced after forming the same plan"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task SamePlanRevalidationRejectsIntroducedOccupant()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-same-plan-race");
        var operation = CreateOperation();
        var planning = await operation.PlanAsync(
            Request(catalogue.Path),
            TestContext.Current.CancellationToken);
        var plan = Assert.IsType<ExtensionCreatePlan>(planning.Plan);
        catalogue.CreateFile("development-toolkit/extension.json", "occupant");
        var beforeApply = catalogue.SnapshotHashes();

        var result = await operation.ApplyAsync(plan, TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == ExtensionCreateFindingCode.DestinationChanged);
        Assert.Equal(beforeApply, catalogue.SnapshotHashes());
        Assert.Equal("occupant", File.ReadAllText(catalogue.Combine("development-toolkit", "extension.json")));
    }

    [Fact(DisplayName = "Extension Create retains a real first create effect when a later effect collides"), Trait("Feature", "extension-create"), Trait("Evidence", "Integration")]
    public async Task LaterEffectFailureRetainsAppliedManifest()
    {
        using var catalogue = TemporaryWorkspace.Create("extension-create-partial-retention");
        var operation = CreateOperation();
        var planning = await operation.PlanAsync(
            Request(catalogue.Path),
            TestContext.Current.CancellationToken);
        var plan = Assert.IsType<Core.Commands.Extension.Create.Models.Planning.ExtensionCreatePlan>(planning.Plan);
        var writer = new ExtensionCreateDestinationWriter();
        var destination = catalogue.Combine("development-toolkit");
        try
        {
            var first = await writer.ApplyAsync(
                plan,
                plan.IntendedEffects[0],
                TestContext.Current.CancellationToken);
            Assert.True(first.Applied);
            Assert.Null(first.Finding);
            var manifestPath = catalogue.Combine("development-toolkit", "extension.json");
            var retainedManifest = await File.ReadAllBytesAsync(
                manifestPath,
                TestContext.Current.CancellationToken);
            File.WriteAllText(catalogue.Combine("development-toolkit", "content"), "collision");

            var second = await writer.ApplyAsync(
                plan,
                plan.IntendedEffects[1],
                TestContext.Current.CancellationToken);

            Assert.False(second.Applied);
            Assert.Equal(ExtensionCreateFindingCode.ApplicationFailed, second.Finding?.Code);
            Assert.Equal(retainedManifest, await File.ReadAllBytesAsync(manifestPath, TestContext.Current.CancellationToken));
            Assert.Equal("collision", File.ReadAllText(catalogue.Combine("development-toolkit", "content")));
        }
        finally
        {
            if (Directory.Exists(destination))
            {
                Directory.Delete(destination, recursive: true);
            }
        }
    }

    private static ExtensionCreateOperation CreateOperation()
    {
        var session = new CliInteractiveSession(
            new StringReader(string.Empty),
            new StringWriter(),
            canPrompt: false);
        return ExtensionCreateOperationFactory.Create(session);
    }

    private static ExtensionCreateRequest Request(string cataloguePath)
        => new()
        {
            StableId = "development-toolkit",
            CataloguePath = cataloguePath,
            Name = null,
            Description = null,
            PackageVersion = null,
            Dependencies = [],
            AllowInteraction = false,
            Mode = ExtensionCreateMode.Apply,
        };
}
