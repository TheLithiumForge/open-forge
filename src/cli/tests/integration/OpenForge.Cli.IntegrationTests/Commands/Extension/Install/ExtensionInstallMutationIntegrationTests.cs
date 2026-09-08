using OpenForge.Cli.Core.Framework.Permissions.Models.Result;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Extension.Install;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Application;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Application;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.IntegrationTests.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Install;

public sealed class ExtensionInstallMutationIntegrationTests
{
    [Fact(DisplayName = "Extension Install stops after verified progress when a later target changes and retains recovery"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task TargetChangedAfterVerifiedEffectRetainsProgressAndRecovery()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-install-partial-target-change");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-install-partial-target-change-source");
        source.AddPackage(
            "toolkit",
            [],
            (".agents/a.txt", "first package bytes\n"),
            (".agents/z.txt", "second package bytes\n"));
        var resolver = new PhysicalPathResolver();
        var validator = new FileExpectationValidator(resolver);
        var revalidator = new MutationRevalidator(validator);
        var lifecycleStore = new LifecycleStore(resolver);
        var planner = new ExtensionInstallPlanner(
            new CliInteractiveSession(TextReader.Null, TextWriter.Null, canPrompt: false),
            resolver,
            lifecycleStore);
        var request = new ExtensionInstallRequest(
            workspace.Workspace,
            ExtensionInstallMode.Apply,
            ["toolkit"],
            all: false,
            source.Path,
            force: false,
            automatic: true,
            allowInteraction: false);
        var build = await planner.BuildAsync(request, TestContext.Current.CancellationToken);
        var plan = Assert.IsType<ExtensionInstallPlan>(build.Plan);
        var operationId = Guid.NewGuid();
        using var lockStore = WorkspaceLockTestStore.Create(
            "extension-install-partial-target-change-locks");
        var lockResult = await lockStore.AcquireAsync(
            new WorkspaceLockRequest(
                workspace.Workspace,
                ExtensionInstallDefinitions.CommandIdentity,
                operationId),
            TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(lockResult.Lease);
        var validation = await new MutationPreflight(validator).ValidateAsync(
            workspace.Workspace,
            plan.DirectoryCreations,
            plan.AllFileChanges,
            TestContext.Current.CancellationToken);
        Assert.Equal(MutationValidationState.Valid, validation.State);
        var preparationResult = await ExtensionInstallRecoveryOperation.PrepareAsync(
            new ExtensionInstallExecutionPlan(plan, new(null,
                WorkspacePermissionResult.NotEvaluated with { Decision = WorkspacePermissionDecision.NotRequired }, null, null, null)),
            operationId,
            TestContext.Current.CancellationToken);
        Assert.NotNull(preparationResult.Preparation);
        var preparation = preparationResult.Preparation;
        var lifecycleBefore = workspace.ReadText(ExtensionInstallIntegrationWorkspace.LifecyclePath);
        workspace.CreateOccupant(".agents/z.txt", "late occupant bytes\n");
        var verifier = new ExtensionInstallAppliedVerifier(resolver, validator, lifecycleStore);
        var application = new ExtensionInstallEffectApplication(
            new DirectoryCreationApplier(revalidator, validator),
            new FileChangeApplier(revalidator, validator),
            verifier);

        var result = await application.ApplyAsync(
            new ExtensionInstallEffectApplicationInput
            {
                Plan = plan,
                Lease = lease,
                Validation = validation,
                Progress = ExtensionInstallApplicationProgress.Start(
                    new ExtensionInstallExecutionPlan(plan, new(null,
                        WorkspacePermissionResult.NotEvaluated with { Decision = WorkspacePermissionDecision.NotRequired }, null, null, null)), preparation),
            },
            TestContext.Current.CancellationToken);

        var finding = Assert.IsType<ExtensionInstallFinding>(result.Finding);
        Assert.Equal(ExtensionInstallFindingCode.TargetChanged, finding.Code);
        Assert.Equal(".agents/z.txt", finding.Target);
        var first = Assert.Single(result.Progress.Effects, effect => effect.Path == ".agents/a.txt");
        Assert.Equal(ExtensionInstallEffectOutcome.Verified, first.Outcome);
        Assert.Equal(ExtensionInstallEffectResidual.Retained, first.Residual);
        var second = Assert.Single(result.Progress.Effects, effect => effect.Path == ".agents/z.txt");
        Assert.Equal(ExtensionInstallEffectOutcome.NotStarted, second.Outcome);
        Assert.Equal(ExtensionInstallEffectResidual.None, second.Residual);
        Assert.Equal(ExtensionInstallLifecycleOutcome.NotStarted, result.Progress.LifecycleOutcome);
        Assert.Equal(ExtensionInstallVerificationState.Unknown, result.Progress.Verification.Targets);
        Assert.Equal(ExtensionInstallVerificationState.Unknown, result.Progress.Verification.Topology);
        Assert.Equal(ExtensionInstallVerificationState.Unknown, result.Progress.Verification.ExtensionsLifecycle);
        Assert.Equal(ExtensionInstallVerificationState.Unknown, result.Progress.Verification.FrameworkLifecycle);
        Assert.Equal(ExtensionInstallRecoveryState.Retained, result.Progress.Recovery.State);
        Assert.Equal(preparation.BundlePath, result.Progress.Recovery.ResidualPath);
        Assert.True(File.Exists(preparation.BundlePath));
        Assert.Equal("first package bytes\n", workspace.ReadText(".agents/a.txt"));
        Assert.Equal("late occupant bytes\n", workspace.ReadText(".agents/z.txt"));
        Assert.Equal(
            lifecycleBefore,
            workspace.ReadText(ExtensionInstallIntegrationWorkspace.LifecyclePath));

        var publicResult = ExtensionInstallResultFactory.Application(
            plan,
            result.Progress,
            result.Finding);
        Assert.Equal(CliSemanticStatus.Blocked, publicResult.Status);
        var json = ExtensionInstallJsonProjection.RenderJson(
            new CliPresentationRequest<ExtensionInstallResult>(
                publicResult,
                new CliPresentation(
                    CliOutputFormat.Json,
                    CliView.Expanded,
                    CliVerbosity.Normal)));
        using var document = JsonDocument.Parse(json);
        var publicFacts = document.RootElement.GetProperty("result");
        Assert.Equal("blocked", document.RootElement.GetProperty("status").GetString());
        Assert.Contains(publicFacts.GetProperty("findings").EnumerateArray(), value =>
            value.GetProperty("code").GetString() == "extension-install.target-changed"
            && value.GetProperty("target").GetString() == ".agents/z.txt");
        var publicFirst = Assert.Single(publicFacts.GetProperty("effects").EnumerateArray(), value =>
            value.GetProperty("path").GetString() == ".agents/a.txt");
        Assert.Equal("verified", publicFirst.GetProperty("outcome").GetString());
        Assert.Equal("retained", publicFirst.GetProperty("residual").GetString());
        var publicSecond = Assert.Single(publicFacts.GetProperty("effects").EnumerateArray(), value =>
            value.GetProperty("path").GetString() == ".agents/z.txt");
        Assert.Equal("not-started", publicSecond.GetProperty("outcome").GetString());
        Assert.Equal("none", publicSecond.GetProperty("residual").GetString());
        Assert.Equal("not-started", publicFacts.GetProperty("lifecycle").GetProperty("outcome").GetString());
        Assert.Equal("retained", publicFacts.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal(preparation.BundlePath, publicFacts.GetProperty("recovery").GetProperty("residualPath").GetString());
        Assert.All(publicFacts.GetProperty("verification").EnumerateObject(), value =>
            Assert.Equal("unknown", value.Value.GetString()));
    }

    [Fact(DisplayName = "Extension Install applies dependency-first verifies topology publishes lifecycle last and converges to an exact no-op"),
     Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task ApplyVerifiesCompleteStateAndNoOp()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-apply");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-install-apply-source");
        source.AddPackage(
            "base",
            [],
            (".agents/base/_base.md", Document("Base")));
        source.AddPackage(
            "toolkit",
            ["base"],
            (".agents/toolkit/_toolkit.md", Document("Toolkit")));
        var sourceBefore = source.Snapshot();
        var frameworkBefore = workspace.ReadFrameworkLifecycle();
        var arguments = new[]
        {
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic", "--json",
        };

        var applied = await workspace.RunAsync(arguments);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal(string.Empty, applied.StandardError);
        using var document = JsonDocument.Parse(applied.StandardOutput);
        var root = document.RootElement;
        Assert.Equal(
            ["schemaVersion", "command", "status", "workspace", "result", "next"],
            Names(root));
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("extension install", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        Assert.Equal(workspace.Path, root.GetProperty("workspace").GetProperty("path").GetString());
        Assert.Equal("current-directory", root.GetProperty("workspace").GetProperty("selectedBy").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        var result = root.GetProperty("result");
        Assert.Equal(
        [
            "mode", "force", "automatic", "selection", "source", "packages", "framework",
            "footprint", "effects", "generatedNavigation", "permissions", "lifecycle", "recovery", "verification", "findings",
        ],
            Names(result));
        Assert.Equal("apply", result.GetProperty("mode").GetString());
        Assert.False(result.GetProperty("force").GetBoolean());
        Assert.True(result.GetProperty("automatic").GetBoolean());
        Assert.Equal(["selectedBy", "rootIds"], Names(result.GetProperty("selection")));
        Assert.Equal(["kind", "path", "identity", "packageCount"], Names(result.GetProperty("source")));
        Assert.Equal("catalogue", result.GetProperty("source").GetProperty("kind").GetString());
        Assert.Equal(source.Path, result.GetProperty("source").GetProperty("path").GetString());
        Assert.Equal(2, result.GetProperty("source").GetProperty("packageCount").GetInt32());
        Assert.Equal(["base", "toolkit"], Strings(result.GetProperty("packages"), "id"));
        Assert.All(result.GetProperty("packages").EnumerateArray(), package =>
            Assert.Equal(["id", "selectedRoot", "dependencies"], Names(package)));
        Assert.Equal(["inventoryFingerprint", "targetCount", "generatedRegionCount"], Names(result.GetProperty("framework")));
        Assert.Matches(
            "^[0-9a-f]{64}$",
            result.GetProperty("framework").GetProperty("inventoryFingerprint").GetString() ?? string.Empty);
        Assert.Equal(["packageCount", "payloadTargets", "generatedRegions", "directories"], Names(result.GetProperty("footprint")));
        Assert.Equal(2, result.GetProperty("footprint").GetProperty("packageCount").GetInt32());
        Assert.Equal([".agents/base/_base.md", ".agents/toolkit/_toolkit.md"],
            Strings(result.GetProperty("footprint").GetProperty("payloadTargets")));
        Assert.NotEmpty(result.GetProperty("effects").EnumerateArray());
        Assert.All(result.GetProperty("effects").EnumerateArray(), effect =>
            Assert.Equal(["path", "packageId", "kind", "action", "outcome", "residual"], Names(effect)));
        AssertPackageEffectsAreDependencyFirst(result.GetProperty("effects"));
        Assert.All(result.GetProperty("effects").EnumerateArray(), effect =>
            Assert.Equal("verified", effect.GetProperty("outcome").GetString()));
        Assert.Equal(["regions"], Names(result.GetProperty("generatedNavigation")));
        Assert.All(result.GetProperty("generatedNavigation").GetProperty("regions").EnumerateArray(), region =>
            Assert.Equal(["path", "state"], Names(region)));
        Assert.Equal(["action", "outcome"], Names(result.GetProperty("lifecycle")));
        Assert.Equal("publish", result.GetProperty("lifecycle").GetProperty("action").GetString());
        Assert.Equal("verified", result.GetProperty("lifecycle").GetProperty("outcome").GetString());
        Assert.Equal(["state", "protectedPaths", "residualPath"], Names(result.GetProperty("recovery")));
        Assert.Equal("removed", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.NotEmpty(result.GetProperty("recovery").GetProperty("protectedPaths").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, result.GetProperty("recovery").GetProperty("residualPath").ValueKind);
        Assert.Equal(["targets", "topology", "extensionsLifecycle", "frameworkLifecycle"], Names(result.GetProperty("verification")));
        Assert.All(result.GetProperty("verification").EnumerateObject(), verification =>
            Assert.Equal("verified", verification.Value.GetString()));
        Assert.Empty(result.GetProperty("findings").EnumerateArray());
        Assert.True(File.Exists(workspace.Combine(".agents/base/_base.md")));
        Assert.True(File.Exists(workspace.Combine(".agents/toolkit/_toolkit.md")));
        Assert.Equal(
            source.ReadPayloadBytes("base", ".agents/base/_base.md"),
            File.ReadAllBytes(workspace.Combine(".agents/base/_base.md")));
        Assert.Equal(
            source.ReadPayloadBytes("toolkit", ".agents/toolkit/_toolkit.md"),
            File.ReadAllBytes(workspace.Combine(".agents/toolkit/_toolkit.md")));
        Assert.Contains("base/_base.md", workspace.ReadText(".agents/loader.md"), StringComparison.Ordinal);
        Assert.Contains("toolkit/_toolkit.md", workspace.ReadText(".agents/loader.md"), StringComparison.Ordinal);
        Assert.True(JsonNode.DeepEquals(
            JsonNode.Parse(frameworkBefore.GetRawText()),
            JsonNode.Parse(workspace.ReadFrameworkLifecycle().GetRawText())));
        var extensionLifecycle = workspace.ReadExtensionsLifecycle();
        Assert.Equal(["base", "toolkit"], Strings(extensionLifecycle.GetProperty("packages"), "id"));
        Assert.Equal(sourceBefore, source.Snapshot());

        var afterApply = workspace.Snapshot();
        var noOp = await workspace.RunAsync(arguments);

        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, noOp.Status);
        using var noOpDocument = JsonDocument.Parse(noOp.StandardOutput);
        var noOpResult = noOpDocument.RootElement.GetProperty("result");
        Assert.Empty(noOpResult.GetProperty("effects").EnumerateArray());
        Assert.Equal("preserve", noOpResult.GetProperty("lifecycle").GetProperty("action").GetString());
        Assert.Equal("already-current", noOpResult.GetProperty("lifecycle").GetProperty("outcome").GetString());
        Assert.Equal("not-required", noOpResult.GetProperty("recovery").GetProperty("state").GetString());
        Assert.All(noOpResult.GetProperty("verification").EnumerateObject(), verification =>
            Assert.Equal("verified", verification.Value.GetString()));
        Assert.Equal(afterApply, workspace.Snapshot());
        Assert.Equal(sourceBefore, source.Snapshot());
    }

    [Fact(DisplayName = "Extension Install lock contention blocks every effect and keeps source and workspace unchanged"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task LockContentionIsNoWriteBlocked()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-lock");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-install-lock-source");
        source.AddPackage("toolkit", [], (".agents/toolkit/_toolkit.md", Document("Toolkit")));
        var beforeWorkspace = workspace.Snapshot();
        var beforeSource = source.Snapshot();
        using var heldLease = workspace.HoldLock();

        var run = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic", "--json",
        ]);

        Assert.Equal(5, run.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, run.Status);
        using var document = JsonDocument.Parse(run.StandardOutput);
        var result = document.RootElement.GetProperty("result");
        Assert.Contains(result.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "extension-install.workspace-lock-unavailable");
        Assert.All(result.GetProperty("effects").EnumerateArray(), effect =>
            Assert.Equal("not-started", effect.GetProperty("outcome").GetString()));
        Assert.Equal("not-created", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal(beforeWorkspace, workspace.Snapshot());
        Assert.Equal(beforeSource, source.Snapshot());
    }

    [Fact(DisplayName = "Extension Install replans from changed source and never replays an earlier dry-run"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task ApplyReadsFreshSourceAfterDryRun()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-fresh-source");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-install-fresh-source-catalogue");
        source.AddPackage("toolkit", [], (".agents/toolkit.md", Document("Version one")));
        var dryRun = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--dry-run", "--json",
        ]);
        Assert.Equal(0, dryRun.ExitCode);
        source.ReplacePayload("toolkit", ".agents/toolkit.md", Document("Version two"));

        var applied = await workspace.RunAsync(
        [
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic", "--json",
        ]);

        Assert.Equal(0, applied.ExitCode);
        Assert.Contains("# Version two", workspace.ReadText(".agents/toolkit.md"), StringComparison.Ordinal);
        Assert.DoesNotContain("# Version one", workspace.ReadText(".agents/toolkit.md"), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Extension Install never uses force to reconcile managed divergence"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task ManagedDivergenceRemainsUpdateOwned()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-install-divergence");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-install-divergence-source");
        source.AddPackage("toolkit", [], (".agents/toolkit.md", Document("Toolkit")));
        var arguments = new[]
        {
            "extension", "install", "toolkit",
            "--source", source.Path,
            "--automatic", "--json",
        };
        var applied = await workspace.RunAsync(arguments);
        Assert.Equal(0, applied.ExitCode);
        workspace.ReplaceText(".agents/toolkit.md", "# Managed divergence\n");
        var beforeBlocked = workspace.Snapshot();

        var blocked = await workspace.RunAsync([.. arguments, "--force"]);

        Assert.Equal(5, blocked.ExitCode);
        Assert.Equal(CliSemanticStatus.Blocked, blocked.Status);
        using var document = JsonDocument.Parse(blocked.StandardOutput);
        var root = document.RootElement;
        var result = root.GetProperty("result");
        Assert.Contains(result.GetProperty("findings").EnumerateArray(), finding =>
            finding.GetProperty("code").GetString() == "extension-install.managed-divergence");
        Assert.Equal("open-forge extension update toolkit", root.GetProperty("next").GetProperty("command").GetString());
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Equal("not-required", result.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal(beforeBlocked, workspace.Snapshot());
    }

    [Fact(DisplayName = "Extension Install pre-delete cleanup cancellation retains the prepared final path"), Trait("Feature", "extension-install"), Trait("Evidence", "Integration")]
    public async Task CleanupCancellationRetainsPreparedFinalPath()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-install-cleanup-cancel");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-install-cleanup-cancel-source");
        source.AddPackage("toolkit", [], (".agents/toolkit.md", Document("Toolkit")));
        workspace.CreateOccupant(".agents/toolkit.md", "plain occupant\n");
        var resolver = new PhysicalPathResolver();
        var lifecycleStore = new LifecycleStore(resolver);
        var planBuild = await new ExtensionInstallPlanner(
            new CliInteractiveSession(TextReader.Null, TextWriter.Null, canPrompt: false),
            resolver,
            lifecycleStore).BuildAsync(
                new ExtensionInstallRequest(
                    workspace.Workspace,
                    ExtensionInstallMode.Apply,
                    ["toolkit"],
                    all: false,
                    source.Path,
                    force: true,
                    automatic: true,
                    allowInteraction: false),
                TestContext.Current.CancellationToken);
        var plan = Assert.IsType<ExtensionInstallPlan>(planBuild.Plan);
        var operationId = Guid.NewGuid();
        using var lockStore = WorkspaceLockTestStore.Create(
            "extension-install-cleanup-cancel-locks");
        var lockResult = await lockStore.AcquireAsync(
            new WorkspaceLockRequest(
                workspace.Workspace,
                ExtensionInstallDefinitions.CommandIdentity,
                operationId),
            TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(lockResult.Lease);
        var preparationResult = await ExtensionInstallRecoveryOperation.PrepareAsync(
            new ExtensionInstallExecutionPlan(plan, new(null,
                WorkspacePermissionResult.NotEvaluated with { Decision = WorkspacePermissionDecision.NotRequired }, null, null, null)),
            operationId,
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<OpenForge.Cli.Core.Framework.Recovery.Models.RecoveryBundlePreparation>(
            preparationResult.Preparation);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var cleanup = await ExtensionInstallRecoveryOperation.CleanupAsync(
            new ExtensionInstallRecoveryCleanupRequest(plan, lease, preparation),
            cancellation.Token);

        Assert.Equal(ExtensionInstallRecoveryState.Unknown, cleanup.Recovery.State);
        Assert.Equal(preparation.BundlePath, cleanup.Recovery.ResidualPath);
        Assert.Equal(
            preparation.Entries.OrderBy(entry => entry.Ordinal).Select(entry => entry.TargetPath),
            cleanup.Recovery.ProtectedPaths);
        Assert.Equal(ExtensionInstallFindingCode.Interrupted, cleanup.Finding?.Code);
        Assert.True(File.Exists(preparation.BundlePath));
    }

    private static void AssertPackageEffectsAreDependencyFirst(JsonElement effects)
    {
        var packageEffects = effects.EnumerateArray()
            .Where(effect => effect.GetProperty("packageId").ValueKind == JsonValueKind.String)
            .ToArray();
        var baseIndex = Array.FindIndex(packageEffects, effect => effect.GetProperty("packageId").GetString() == "base");
        var toolkitIndex = Array.FindIndex(packageEffects, effect => effect.GetProperty("packageId").GetString() == "toolkit");
        Assert.True(baseIndex >= 0);
        Assert.True(toolkitIndex > baseIndex);
    }

    private static string[] Names(JsonElement value)
        => [.. value.EnumerateObject().Select(property => property.Name)];

    private static string?[] Strings(JsonElement array, string property)
        => [.. array.EnumerateArray().Select(value => value.GetProperty(property).GetString())];

    private static string?[] Strings(JsonElement array)
        => [.. array.EnumerateArray().Select(value => value.GetString())];

    private static string Document(string name)
        => OpenForge.Cli.TestSupport.OpenForgeDocumentSeed.Metadata(
            name,
            ["Extension"],
            $"# {name}\n");
}
