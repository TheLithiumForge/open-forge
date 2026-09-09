using System.Text.Json;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Move;

public sealed class RouteMoveApplicationIntegrationTests
{
    [Theory(DisplayName = "Composed Route Move invalid operand boundaries retain exact human refusal without writes"),
     InlineData("missing-source"), InlineData("missing-destination"), InlineData("consumed-source"),
     Trait("Feature", "route-move"), Trait("Evidence", "Integration")]
    public static async Task InvalidHumanOperandsRetainExactRefusal(string scenario)
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create($"move-invalid-human-{scenario}");
        string[] arguments = scenario switch
        {
            "missing-source" => ["route", "move"],
            "missing-destination" => ["route", "move", RouteMoveIntegrationWorkspace.LeafId],
            "consumed-source" => ["route", "move", ".agents/guidance/consumed.md", RouteMoveIntegrationWorkspace.CrossRouteDestination],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The invalid operand scenario is not defined."),
        };
        var before = workspace.SnapshotHashes();
        var result = await CliHostCapture.RunAsync(arguments, workspace.Workspace.LexicalRoot);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Output);
        Assert.Contains("Status: invalid", result.Error, StringComparison.Ordinal);
        Assert.Contains("route-move.", result.Error, StringComparison.Ordinal);
        Assert.Contains("Next: open-forge route move --help", result.Error, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Theory(DisplayName = "Composed Route Move safety failures retain exact human status and finding"),
     InlineData("occupied-destination", 5, "blocked", "route-move.destination-occupied"),
     InlineData("ownership-missing", 5, "blocked", "route-move.ownership-unavailable"),
     InlineData("invalid-utf8", 3, "incomplete", "route-move.reference-coverage-incomplete"),
     Trait("Feature", "route-move"), Trait("Evidence", "Integration")]
    public static async Task SafetyFailuresRenderExactStatusAndFinding(string scenario, int expectedExit, string expectedStatus, string expectedCode)
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create($"move-human-{scenario}");
        if (scenario == "ownership-missing")
        {
            File.Delete(workspace.Absolute(RouteMoveIntegrationWorkspace.LifecyclePath));
        }
        else
        {
            workspace.SeedScenario(scenario);
        }

        var before = workspace.SnapshotHashes();
        var result = await CliHostCapture.RunAsync(
            ["route", "move", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.LeafDestination],
            workspace.Workspace.LexicalRoot);

        Assert.Equal(expectedExit, result.ExitCode);
        var primary = expectedStatus == "incomplete" ? result.Output : result.Error;
        Assert.Contains($"Status: {expectedStatus}", primary, StringComparison.Ordinal);
        Assert.Contains(expectedCode, primary, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Composed Route Move compact dry-run names every changed path and exact reference"),
     Trait("Feature", "route-move"), Trait("Evidence", "Integration")]
    public async Task CompactDryRunNamesEveryChangedPathAndReference()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("move-compact-dry-run");
        var before = workspace.SnapshotHashes();
        var result = await CliHostCapture.RunAsync(
            ["route", "move", RouteMoveIntegrationWorkspace.LeafId, RouteMoveIntegrationWorkspace.CrossRouteDestination, "--dry-run"],
            workspace.Workspace.LexicalRoot);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains(".agents/guidance/old guide.md", result.Output, StringComparison.Ordinal);
        Assert.Contains(".agents/guidance/old guide.overwrite.md", result.Output, StringComparison.Ordinal);
        Assert.Contains(".agents/archive/new guide.md", result.Output, StringComparison.Ordinal);
        Assert.Contains(".agents/archive/new guide.overwrite.md", result.Output, StringComparison.Ordinal);
        Assert.Contains("README.md", result.Output, StringComparison.Ordinal);
        Assert.Contains("notes.md", result.Output, StringComparison.Ordinal);
        Assert.Contains(".agents/guidance/_guidance.md", result.Output, StringComparison.Ordinal);
        Assert.Contains(".agents/archive/_archive.md", result.Output, StringComparison.Ordinal);
        Assert.Contains(".agents/guidance/old%20guide.md#section", result.Output, StringComparison.Ordinal);
        Assert.Contains(".agents/archive/new%20guide.md#section", result.Output, StringComparison.Ordinal);
        Assert.Contains("No files changed (--dry-run).", result.Output, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route Move applies and verifies a complete category with exact resource bytes and relative references"),
     Trait("Feature", "route-move"), Trait("Evidence", "Integration")]
    public async Task CategoryApplicationPreservesResourcesAndRelativeReferences()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("move-category-application");
        const string child = ".agents/guidance/topics/child.md";
        workspace.WriteText(child, workspace.ReadText(child) + "\n[Local](notes.md#detail)\n");
        var childBefore = workspace.ReadText(child);
        var overwriteBefore = workspace.ReadText(".agents/guidance/topics/child.overwrite.md");
        var binaryBefore = File.ReadAllBytes(workspace.Absolute(".agents/guidance/topics/image.bin"));
        var settingsBefore = workspace.ReadText(".agents/guidance/topics/assets/settings.json");
        var notesBefore = workspace.ReadText(".agents/guidance/topics/notes.md");
        var nativeBefore = workspace.ReadText(".agents/guidance/topics/native/SKILL.md");
        var build = await RouteMoveIntegrationWorkspace.CreatePlanBuilder().BuildAsync(
            workspace.Request(RouteMoveIntegrationWorkspace.CategoryPath, RouteMoveIntegrationWorkspace.CategoryDestination,
                OpenForge.Cli.Core.Commands.Route.Move.Models.Request.RouteMoveMode.Apply), TestContext.Current.CancellationToken);
        var plan = Assert.IsType<OpenForge.Cli.Core.Commands.Route.Move.Models.Planning.RouteMovePlan>(build.Plan);
        workspace.OwnCategoryDestination();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var prepared = await RouteMoveRecoveryLifecycle.PrepareAsync(
            new RouteMoveRecoveryPreparationInput { Plan = plan, OperationId = operationId, Lease = lease }, TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        workspace.TrackRecovery(preparation);
        var progress = await RouteMoveIntegrationWorkspace.CreateEffectApplication().ApplyAsync(
            new RouteMoveEffectApplicationInput { Plan = plan, Lease = lease, RecoveryPreparation = prepared }, TestContext.Current.CancellationToken);
        var verification = await RouteMoveIntegrationWorkspace.CreateAppliedVerifier().VerifyAsync(
            new RouteMoveAppliedVerificationInput { Plan = plan, Lease = lease, Progress = progress }, TestContext.Current.CancellationToken);

        AssertAllVerified(progress.Receipts);
        Assert.True(verification.State == RouteMoveAppliedVerificationState.Verified, verification.Cause);
        Assert.False(Directory.Exists(workspace.Absolute(".agents/guidance/topics")));
        Assert.True(File.Exists(workspace.Absolute(RouteMoveIntegrationWorkspace.CategoryDestination)));
        Assert.Equal(binaryBefore, File.ReadAllBytes(workspace.Absolute(".agents/archive/topics/image.bin")));
        Assert.Equal(settingsBefore, workspace.ReadText(".agents/archive/topics/assets/settings.json"));
        Assert.Equal(notesBefore, workspace.ReadText(".agents/archive/topics/notes.md"));
        Assert.Equal(nativeBefore, workspace.ReadText(".agents/archive/topics/native/SKILL.md"));
        Assert.Equal(childBefore.Replace("../old%20guide.md#section", "../../guidance/old%20guide.md#section", StringComparison.Ordinal),
            workspace.ReadText(".agents/archive/topics/child.md"));
        Assert.Equal(overwriteBefore.Replace("../old%20guide.md#section", "../../guidance/old%20guide.md#section", StringComparison.Ordinal),
            workspace.ReadText(".agents/archive/topics/child.overwrite.md"));
        Assert.Contains("notes.md#detail", workspace.ReadText(".agents/archive/topics/child.md"), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Composed Route Move invalid JSON keeps unresolved facts and the complete empty graph"),
     Trait("Feature", "route-move"), Trait("Evidence", "Integration")]
    public async Task InvalidJsonKeepsUnresolvedFactsAndEmptyGraph()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("move-invalid-json");
        var before = workspace.SnapshotHashes();
        const string missing = ".agents/guidance/consumed.md";
        var response = await CliHostCapture.RunAsync(
            ["route", "move", missing, RouteMoveIntegrationWorkspace.CrossRouteDestination, "--json"], workspace.Workspace.LexicalRoot);

        Assert.Equal(4, response.ExitCode);
        Assert.Equal(string.Empty, response.Error);
        using var document = JsonDocument.Parse(response.Output);
        var root = document.RootElement;
        var result = root.GetProperty("result");
        Assert.Equal("invalid", root.GetProperty("status").GetString());
        var source = result.GetProperty("source");
        Assert.Equal(missing, source.GetProperty("requested").GetString());
        Assert.Equal("base-path", source.GetProperty("selectedBy").GetString());
        Assert.All(["id", "path", "form"], name => Assert.Equal(JsonValueKind.Null, source.GetProperty(name).ValueKind));
        Assert.Empty(result.GetProperty("subject").GetProperty("layers").EnumerateArray());
        Assert.Empty(result.GetProperty("subject").GetProperty("items").EnumerateArray());
        Assert.Empty(result.GetProperty("ownership").GetProperty("claims").EnumerateArray());
        Assert.Empty(result.GetProperty("references").GetProperty("rewrites").EnumerateArray());
        Assert.Empty(result.GetProperty("generatedNavigation").GetProperty("regions").EnumerateArray());
        Assert.Empty(result.GetProperty("effects").EnumerateArray());
        Assert.Empty(result.GetProperty("unchangedPaths").EnumerateArray());
        Assert.Equal("route-move.source-not-found", Assert.Single(result.GetProperty("findings").EnumerateArray()).GetProperty("code").GetString());
        Assert.Equal("open-forge route move --help", root.GetProperty("next").GetProperty("command").GetString());
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Composed Route Move help retains exact shared exit and stream policy"),
     Trait("Feature", "route-move"), Trait("Evidence", "Integration")]
    public async Task HelpRetainsExactSharedExitAndStreamPolicy()
    {
        using var workspace = TemporaryWorkspace.Create("move-help-policy");
        var before = workspace.SnapshotHashes();
        var missing = workspace.Combine("missing");
        var result = await CliHostCapture.RunAsync(["route", "move", "--help", "--workspace", missing], workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains("open-forge route move", result.Output, StringComparison.Ordinal);
        Assert.Contains("--dry-run", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("--automatic", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("--force", result.Output, StringComparison.Ordinal);
        Assert.Contains("<source-reference> <destination-target>", result.Output, StringComparison.Ordinal);
        Assert.Contains("Name the intended destination by exact path.", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("--recursive", result.Output, StringComparison.Ordinal);
        Assert.Contains("complete: exit 0 and human stdout.", result.Output, StringComparison.Ordinal);
        Assert.Contains("attention: exit 2 and human stdout.", result.Output, StringComparison.Ordinal);
        Assert.Contains("incomplete: exit 3 and human stdout.", result.Output, StringComparison.Ordinal);
        Assert.Contains("invalid: exit 4 and human stderr.", result.Output, StringComparison.Ordinal);
        Assert.Contains("blocked: exit 5 and human stderr.", result.Output, StringComparison.Ordinal);
        Assert.Contains("failed: exit 1 and human stderr.", result.Output, StringComparison.Ordinal);
        Assert.Contains("interrupted: exit 130 and human stderr.", result.Output, StringComparison.Ordinal);
        Assert.False(Directory.Exists(missing));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Composed Route Move JSON dry-run keeps bounded diagnostics separate"),
     Trait("Feature", "route-move"), Trait("Evidence", "Integration")]
    public async Task JsonDryRunPreservesPrimaryDocumentWithVerboseDiagnostics()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create("move-json-diagnostics");
        var before = workspace.SnapshotHashes();
        string[] arguments =
        [
            "route", "move", RouteMoveIntegrationWorkspace.LeafId,
            RouteMoveIntegrationWorkspace.CrossRouteDestination, "--dry-run", "--dry-run", "--json",
        ];

        var plain = await CliHostCapture.RunAsync(arguments, workspace.Workspace.LexicalRoot);
        var verbose = await CliHostCapture.RunAsync([.. arguments, "--verbose"], workspace.Workspace.LexicalRoot);

        Assert.Equal(0, plain.ExitCode);
        Assert.Equal(string.Empty, plain.Error);
        Assert.Equal(plain.ExitCode, verbose.ExitCode);
        Assert.Equal(plain.Output, verbose.Output);
        var diagnostic = Assert.Single(verbose.Error.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries));
        Assert.InRange(diagnostic.Length, 1, 4095);
        Assert.DoesNotContain('\r', diagnostic);
        Assert.DoesNotContain('\n', diagnostic);
        Assert.EndsWith(Environment.NewLine, verbose.Error, StringComparison.Ordinal);
        Assert.Contains("status=complete; mode=dry-run", diagnostic, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(plain.Output);
        var root = document.RootElement;
        var result = root.GetProperty("result");
        Assert.Equal("complete", root.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        Assert.Equal("dry-run", result.GetProperty("mode").GetString());
        Assert.NotEmpty(result.GetProperty("subject").GetProperty("layers").EnumerateArray());
        Assert.Empty(result.GetProperty("subject").GetProperty("items").EnumerateArray());
        Assert.Equal("unmanaged", result.GetProperty("ownership").GetProperty("state").GetString());
        Assert.Empty(result.GetProperty("ownership").GetProperty("claims").EnumerateArray());
        Assert.Equal("complete", result.GetProperty("references").GetProperty("coverage").GetString());
        Assert.NotEmpty(result.GetProperty("references").GetProperty("rewrites").EnumerateArray());
        Assert.Equal("complete", result.GetProperty("generatedNavigation").GetProperty("coverage").GetString());
        Assert.NotEmpty(result.GetProperty("generatedNavigation").GetProperty("regions").EnumerateArray());
        var effects = result.GetProperty("effects").EnumerateArray().ToArray();
        Assert.NotEmpty(effects);
        Assert.All(effects, effect =>
        {
            Assert.Equal("planned", effect.GetProperty("outcome").GetString());
            Assert.Equal("none", effect.GetProperty("residual").GetString());
        });
        Assert.NotEmpty(result.GetProperty("unchangedPaths").EnumerateArray());
        var recovery = result.GetProperty("recovery");
        Assert.Equal("not-created", recovery.GetProperty("state").GetString());
        Assert.NotEmpty(recovery.GetProperty("protectedPaths").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, recovery.GetProperty("residualPath").ValueKind);
        Assert.Equal("not-requested", result.GetProperty("verification").GetString());
        Assert.Empty(result.GetProperty("findings").EnumerateArray());
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route Move prepares one external ZIP before effects and rejects its exact collision"),
     Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task ExternalRecoveryPrecedesEffectsAndCollisionStopsEverything()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-recovery-collision");
        var plan = await workspace.BuildApplicationPlanAsync();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var before = workspace.SnapshotHashes();
        var lifecycleBytes = workspace.ReadText(RouteMoveIntegrationWorkspace.LifecyclePath);
        var input = new RouteMoveRecoveryPreparationInput
        {
            Plan = plan,
            OperationId = operationId,
            Lease = lease,
        };

        var prepared = await RouteMoveRecoveryLifecycle.PrepareAsync(
            input,
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        workspace.TrackRecovery(preparation);
        var collision = await RouteMoveRecoveryLifecycle.PrepareAsync(
            input,
            TestContext.Current.CancellationToken);

        Assert.Equal(RouteMoveRecoveryPreparationState.Prepared, prepared.State);
        Assert.Equal(RouteMoveRecoveryState.NotCreated, prepared.Recovery.State);
        Assert.Null(prepared.Finding);
        Assert.True(File.Exists(preparation.BundlePath));
        Assert.False(Directory.Exists(workspace.Absolute(".agents/archive/application")));
        Assert.Equal(RouteMoveRecoveryPreparationState.Blocked, collision.State);
        Assert.Equal(RouteMoveRecoveryState.Retained, collision.Recovery.State);
        Assert.Equal(preparation.BundlePath, collision.Recovery.ResidualPath);
        Assert.Equal(RouteMoveFindingCode.RecoveryConflict, collision.Finding?.Code);
        Assert.Equal(CliSemanticStatus.Blocked, collision.Finding?.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(lifecycleBytes, workspace.ReadText(RouteMoveIntegrationWorkspace.LifecyclePath));
    }

    [Fact(DisplayName = "Route Move reports unavailable external recovery before any effect"),
     Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task UnavailableRecoveryStopsBeforeEffects()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-recovery-unavailable");
        var plan = await workspace.BuildApplicationPlanAsync();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var blockedPath = workspace.BlockRecoveryWorkspaceDirectory();
        var before = workspace.SnapshotHashes();

        var result = await RouteMoveRecoveryLifecycle.PrepareAsync(
                new RouteMoveRecoveryPreparationInput
                {
                    Plan = plan,
                    OperationId = operationId,
                    Lease = lease,
                },
                TestContext.Current.CancellationToken);

        Assert.True(File.Exists(blockedPath));
        Assert.Equal(RouteMoveRecoveryPreparationState.Incomplete, result.State);
        Assert.Null(result.Preparation);
        Assert.Equal(RouteMoveRecoveryState.Unknown, result.Recovery.State);
        Assert.Equal(RouteMoveFindingCode.RecoveryUnavailable, result.Finding?.Code);
        Assert.Equal(CliSemanticStatus.Incomplete, result.Finding?.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(Directory.Exists(workspace.Absolute(".agents/archive/application")));
    }

    [Fact(DisplayName = "Route Move keeps ordered heterogeneous receipts and never rolls back after a concrete failure"),
     Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task ConcreteFailureLeavesPriorEffectAndAllLaterEffectsNotStarted()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-ordered-failure");
        var plan = await workspace.BuildApplicationPlanAsync();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var prepared = await RouteMoveRecoveryLifecycle.PrepareAsync(
            new RouteMoveRecoveryPreparationInput
            {
                Plan = plan,
                OperationId = operationId,
                Lease = lease,
            },
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        workspace.TrackRecovery(preparation);
        var sourceBefore = workspace.ReadText(RouteMoveIntegrationWorkspace.ApplicationCategoryPath);
        var lifecycleBefore = workspace.ReadText(RouteMoveIntegrationWorkspace.LifecyclePath);
        const string mutatedArchive = "concurrent archive\n";
        workspace.WriteText(".agents/archive/_archive.md", mutatedArchive);

        var progress = await RouteMoveIntegrationWorkspace.CreateEffectApplication()
            .ApplyAsync(
                new RouteMoveEffectApplicationInput
                {
                    Plan = plan,
                    Lease = lease,
                    RecoveryPreparation = prepared,
                },
                TestContext.Current.CancellationToken);

        Assert.Equal(6, progress.Receipts.Length);
        var directory = Assert.IsType<RouteMoveDirectoryCreationReceipt>(progress.Receipts[0]);
        Assert.Equal(FilesystemEffectState.Applied, directory.Receipt.EffectState);
        Assert.Equal(FilesystemVerificationState.Verified, directory.Receipt.VerificationState);
        var destination = Assert.IsType<RouteMoveFileChangeReceipt>(progress.Receipts[1]);
        Assert.Equal(FilesystemEffectState.Applied, destination.Receipt.EffectState);
        Assert.Equal(FilesystemVerificationState.Verified, destination.Receipt.VerificationState);
        var failed = Assert.IsType<RouteMoveFileChangeReceipt>(progress.Receipts[2]);
        Assert.Equal(FilesystemEffectState.NotStarted, failed.Receipt.EffectState);
        Assert.Equal(FilesystemNotStartedReason.TargetChanged, failed.Receipt.NotStartedReason);
        AssertAllLaterNotStarted(progress.Receipts.Skip(3));
        Assert.Contains(
            progress.Findings,
            finding => finding.Code == RouteMoveFindingCode.TargetChangedDuringApply);
        Assert.Equal(RouteMoveRecoveryState.Retained, progress.Recovery.State);
        Assert.Equal(RouteMoveVerificationState.Unknown, progress.Verification);
        Assert.True(Directory.Exists(workspace.Absolute(".agents/archive/application")));
        Assert.Equal(
            sourceBefore,
            workspace.ReadText(RouteMoveIntegrationWorkspace.ApplicationCategoryDestination));
        Assert.Equal(mutatedArchive, workspace.ReadText(".agents/archive/_archive.md"));
        Assert.Equal(sourceBefore, workspace.ReadText(RouteMoveIntegrationWorkspace.ApplicationCategoryPath));
        Assert.Equal(lifecycleBefore, workspace.ReadText(RouteMoveIntegrationWorkspace.LifecyclePath));
    }

    [Fact(DisplayName = "Route Move cancellation leaves every heterogeneous effect not started"),
     Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task CancellationLeavesEveryEffectNotStarted()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-ordered-cancellation");
        var plan = await workspace.BuildApplicationPlanAsync();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var prepared = await RouteMoveRecoveryLifecycle.PrepareAsync(
            new RouteMoveRecoveryPreparationInput
            {
                Plan = plan,
                OperationId = operationId,
                Lease = lease,
            },
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        workspace.TrackRecovery(preparation);
        var before = workspace.SnapshotHashes();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var progress = await RouteMoveIntegrationWorkspace.CreateEffectApplication()
            .ApplyAsync(
                new RouteMoveEffectApplicationInput
                {
                    Plan = plan,
                    Lease = lease,
                    RecoveryPreparation = prepared,
                },
                cancellation.Token);

        Assert.Equal(6, progress.Receipts.Length);
        AssertAllNotStarted(progress.Receipts, FilesystemNotStartedReason.Cancelled);
        Assert.Contains(
            progress.Findings,
            finding => finding.Code == RouteMoveFindingCode.Interrupted);
        Assert.Equal(RouteMoveRecoveryState.Retained, progress.Recovery.State);
        Assert.Equal(RouteMoveVerificationState.NotRequested, progress.Verification);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(Directory.Exists(workspace.Absolute(".agents/archive/application")));
    }

    [Fact(DisplayName = "Route Move post-verification recovery deletion cancelled before candidate selection reports interrupted unknown recovery without inferred retention"),
     Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task PostVerificationRecoveryDeletionCancelledBeforeCandidateSelectionReportsInterruptedUnknownWithoutInferredRetention()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-verified-retained-cleanup");
        var plan = await workspace.BuildApplicationPlanAsync();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var lifecycleBefore = workspace.ReadText(RouteMoveIntegrationWorkspace.LifecyclePath);
        var prepared = await RouteMoveRecoveryLifecycle.PrepareAsync(
            new RouteMoveRecoveryPreparationInput
            {
                Plan = plan,
                OperationId = operationId,
                Lease = lease,
            },
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        workspace.TrackRecovery(preparation);
        Assert.True(File.Exists(preparation.BundlePath));
        Assert.False(Directory.Exists(workspace.Absolute(".agents/archive/application")));

        var progress = await RouteMoveIntegrationWorkspace.CreateEffectApplication()
            .ApplyAsync(
                new RouteMoveEffectApplicationInput
                {
                    Plan = plan,
                    Lease = lease,
                    RecoveryPreparation = prepared,
                },
                TestContext.Current.CancellationToken);
        var verification = await RouteMoveIntegrationWorkspace.CreateAppliedVerifier()
            .VerifyAsync(
                new RouteMoveAppliedVerificationInput
                {
                    Plan = plan,
                    Lease = lease,
                    Progress = progress,
                },
                TestContext.Current.CancellationToken);

        AssertAllVerified(progress.Receipts);
        Assert.True(
            verification.State == RouteMoveAppliedVerificationState.Verified,
            verification.Cause);
        Assert.Null(verification.Cause);
        Assert.Equal(lifecycleBefore, workspace.ReadText(RouteMoveIntegrationWorkspace.LifecyclePath));
        Assert.True(File.Exists(workspace.Absolute(RouteMoveIntegrationWorkspace.ApplicationCategoryDestination)));
        Assert.False(File.Exists(workspace.Absolute(RouteMoveIntegrationWorkspace.ApplicationCategoryPath)));
        Assert.False(Directory.Exists(workspace.Absolute(".agents/guidance/application")));

        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var deletion = await RouteMoveRecoveryLifecycle.DeleteExactAsync(
            new RouteMoveRecoveryDeletionInput
            {
                Plan = plan,
                OperationId = operationId,
                Lease = lease,
                Preparation = preparation,
            },
            cancellation.Token);

        Assert.Equal(RouteMoveRecoveryState.Unknown, deletion.Recovery.State);
        Assert.Null(deletion.Recovery.ResidualPath);
        Assert.Equal(RouteMoveFindingCode.Interrupted, deletion.Finding?.Code);
        Assert.Equal(CliSemanticStatus.Interrupted, deletion.Finding?.Status);
        Assert.True(File.Exists(preparation.BundlePath));
        Assert.Equal(lifecycleBefore, workspace.ReadText(RouteMoveIntegrationWorkspace.LifecyclePath));
    }

    [Fact(DisplayName = "Route Move final verification rejects lifecycle drift after applied effects"),
     Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task FinalVerificationRejectsChangedLifecycleObservation()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-final-lifecycle-drift");
        var plan = await workspace.BuildApplicationPlanAsync();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var prepared = await RouteMoveRecoveryLifecycle.PrepareAsync(
            new RouteMoveRecoveryPreparationInput
            {
                Plan = plan,
                OperationId = operationId,
                Lease = lease,
            },
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        workspace.TrackRecovery(preparation);
        var progress = await RouteMoveIntegrationWorkspace.CreateEffectApplication()
            .ApplyAsync(
                new RouteMoveEffectApplicationInput
                {
                    Plan = plan,
                    Lease = lease,
                    RecoveryPreparation = prepared,
                },
                TestContext.Current.CancellationToken);
        AssertAllVerified(progress.Receipts);
        workspace.SeedScenario("unrelated-ownership-claim");

        var verification = await RouteMoveIntegrationWorkspace.CreateAppliedVerifier()
            .VerifyAsync(
                new RouteMoveAppliedVerificationInput
                {
                    Plan = plan,
                    Lease = lease,
                    Progress = progress,
                },
                TestContext.Current.CancellationToken);

        Assert.Equal(RouteMoveAppliedVerificationState.Failed, verification.State);
        Assert.False(string.IsNullOrWhiteSpace(verification.Cause));
        Assert.True(File.Exists(workspace.Absolute(RouteMoveIntegrationWorkspace.ApplicationCategoryDestination)));
        Assert.False(File.Exists(workspace.Absolute(RouteMoveIntegrationWorkspace.ApplicationCategoryPath)));
        Assert.True(File.Exists(preparation.BundlePath));
    }

    [Fact(DisplayName = "Route Move final verification rejects an ambiguous destination topology"),
     Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task FinalVerificationRejectsAmbiguousDestinationTopology()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-final-destination-ambiguity");
        var plan = await workspace.BuildApplicationPlanAsync();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var prepared = await RouteMoveRecoveryLifecycle.PrepareAsync(
            new RouteMoveRecoveryPreparationInput
            {
                Plan = plan,
                OperationId = operationId,
                Lease = lease,
            },
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        workspace.TrackRecovery(preparation);
        var progress = await RouteMoveIntegrationWorkspace.CreateEffectApplication()
            .ApplyAsync(
                new RouteMoveEffectApplicationInput
                {
                    Plan = plan,
                    Lease = lease,
                    RecoveryPreparation = prepared,
                },
                TestContext.Current.CancellationToken);
        AssertAllVerified(progress.Receipts);

        const string ambiguousEntrypoint = ".agents/archive/application/index.md";
        var ambiguousEntrypointPath = workspace.Absolute(ambiguousEntrypoint);
        File.WriteAllText(
            ambiguousEntrypointPath,
            workspace.ReadText(RouteMoveIntegrationWorkspace.ApplicationCategoryDestination));
        try
        {
            var verification = await RouteMoveIntegrationWorkspace.CreateAppliedVerifier()
                .VerifyAsync(
                    new RouteMoveAppliedVerificationInput
                    {
                        Plan = plan,
                        Lease = lease,
                        Progress = progress,
                    },
                    TestContext.Current.CancellationToken);

            Assert.Equal(RouteMoveAppliedVerificationState.Failed, verification.State);
            Assert.False(string.IsNullOrWhiteSpace(verification.Cause));
            Assert.Equal(
                plan.Projection.FileChanges[0].IntendedBytes,
                File.ReadAllBytes(workspace.Absolute(RouteMoveIntegrationWorkspace.ApplicationCategoryDestination)));
            Assert.False(File.Exists(workspace.Absolute(RouteMoveIntegrationWorkspace.ApplicationCategoryPath)));
            Assert.True(File.Exists(preparation.BundlePath));
        }
        finally
        {
            File.Delete(ambiguousEntrypointPath);
        }
    }

    [Fact(DisplayName = "Route Move changed recovery identity is a failed unknown disposition"),
     Trait("Feature", "route-move"), Trait("Evidence", "IntegrationSafety")]
    public async Task ChangedRecoveryIdentityFailsUnknownWithoutDeletingReplacement()
    {
        using var workspace = RouteMoveIntegrationWorkspace.Create(
            "route-move-recovery-identity-changed");
        var plan = await workspace.BuildApplicationPlanAsync();
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var prepared = await RouteMoveRecoveryLifecycle.PrepareAsync(
            new RouteMoveRecoveryPreparationInput
            {
                Plan = plan,
                OperationId = operationId,
                Lease = lease,
            },
            TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        workspace.ReplaceRecoveryWithDirectory(preparation);
        var lifecycleBefore = workspace.ReadText(RouteMoveIntegrationWorkspace.LifecyclePath);

        var deletion = await RouteMoveRecoveryLifecycle.DeleteExactAsync(
            new RouteMoveRecoveryDeletionInput
            {
                Plan = plan,
                OperationId = operationId,
                Lease = lease,
                Preparation = preparation,
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(RouteMoveRecoveryState.Unknown, deletion.Recovery.State);
        Assert.Null(deletion.Recovery.ResidualPath);
        Assert.Equal(RouteMoveFindingCode.RecoveryFailed, deletion.Finding?.Code);
        Assert.Equal(CliSemanticStatus.Failed, deletion.Finding?.Status);
        Assert.True(Directory.Exists(preparation.BundlePath));
        Assert.Equal(lifecycleBefore, workspace.ReadText(RouteMoveIntegrationWorkspace.LifecyclePath));
    }

    private static void AssertAllLaterNotStarted(IEnumerable<RouteMoveApplicationReceipt> receipts)
    {
        foreach (var receipt in receipts)
        {
            Assert.Equal(FilesystemEffectState.NotStarted, ReadEffectState(receipt));
            Assert.Equal(FilesystemNotStartedReason.ApplicationFailed, ReadNotStartedReason(receipt));
        }
    }

    private static void AssertAllNotStarted(
        IEnumerable<RouteMoveApplicationReceipt> receipts,
        FilesystemNotStartedReason reason)
    {
        foreach (var receipt in receipts)
        {
            Assert.Equal(FilesystemEffectState.NotStarted, ReadEffectState(receipt));
            Assert.Equal(reason, ReadNotStartedReason(receipt));
        }
    }

    private static void AssertAllVerified(IEnumerable<RouteMoveApplicationReceipt> receipts)
    {
        foreach (var receipt in receipts)
        {
            Assert.Equal(FilesystemEffectState.Applied, ReadEffectState(receipt));
            Assert.Equal(FilesystemVerificationState.Verified, ReadVerificationState(receipt));
        }
    }

    private static FilesystemEffectState ReadEffectState(RouteMoveApplicationReceipt receipt)
        => receipt switch
        {
            RouteMoveDirectoryCreationReceipt creation => creation.Receipt.EffectState,
            RouteMoveFileChangeReceipt change => change.Receipt.EffectState,
            RouteMoveDirectoryDeletionReceipt deletion => deletion.Receipt.State.EffectState,
            _ => throw new ArgumentOutOfRangeException(nameof(receipt), receipt, "Unknown Route Move receipt."),
        };

    private static FilesystemVerificationState ReadVerificationState(
        RouteMoveApplicationReceipt receipt)
        => receipt switch
        {
            RouteMoveDirectoryCreationReceipt creation => creation.Receipt.VerificationState,
            RouteMoveFileChangeReceipt change => change.Receipt.VerificationState,
            RouteMoveDirectoryDeletionReceipt deletion => deletion.Receipt.State.VerificationState,
            _ => throw new ArgumentOutOfRangeException(nameof(receipt), receipt, "Unknown Route Move receipt."),
        };

    private static FilesystemNotStartedReason? ReadNotStartedReason(
        RouteMoveApplicationReceipt receipt)
        => receipt switch
        {
            RouteMoveDirectoryCreationReceipt creation => creation.Receipt.NotStartedReason,
            RouteMoveFileChangeReceipt change => change.Receipt.NotStartedReason,
            RouteMoveDirectoryDeletionReceipt deletion => deletion.Receipt.State.NotStartedReason,
            _ => throw new ArgumentOutOfRangeException(nameof(receipt), receipt, "Unknown Route Move receipt."),
        };
}
