using System.Text;
using OpenForge.Cli.Core.Commands.Route.Update;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;
using OpenForge.Cli.Core.Presentation.Route.Update;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Update;

public sealed class RouteUpdateApplicationIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Route Update help retains exact shared exit and stream policy"),
     Trait("Feature", "route-update"), Trait("Evidence", "Integration")]
    public async Task HelpRetainsExactSharedExitAndStreamPolicy()
    {
        using var workspace = TemporaryWorkspace.Create("update-help-policy");
        var before = workspace.SnapshotHashes();
        var missing = workspace.Combine("missing");
        var result = await CliHostCapture.RunAsync(["route", "update", "--help", "--workspace", missing], workspace.Path);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains("open-forge route update", result.Output, StringComparison.Ordinal);
        Assert.Contains("--dry-run", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("--automatic", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("--force", result.Output, StringComparison.Ordinal);
        Assert.Contains("--template <template-reference>", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("--yes", result.Output, StringComparison.Ordinal);
        Assert.Contains("completed: exit 0 and text stdout.", result.Output, StringComparison.Ordinal);
        Assert.Contains("completed-with-warnings: exit 2 and text stdout.", result.Output, StringComparison.Ordinal);
        Assert.Contains("incomplete: exit 3 and text stdout.", result.Output, StringComparison.Ordinal);
        Assert.Contains("invalid-input: exit 4 and text stderr.", result.Output, StringComparison.Ordinal);
        Assert.Contains("blocked: exit 5 and text stderr.", result.Output, StringComparison.Ordinal);
        Assert.Contains("failed: exit 1 and text stderr.", result.Output, StringComparison.Ordinal);
        Assert.Contains("cancelled: exit 130 and text stderr.", result.Output, StringComparison.Ordinal);
        Assert.False(Directory.Exists(missing));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Route Update JSON dry-run keeps bounded diagnostics separate"),
     Trait("Feature", "route-update"), Trait("Evidence", "Integration")]
    public async Task JsonDryRunPreservesPrimaryDocumentWithVerboseDiagnostics()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create("update-json-diagnostics");
        var before = workspace.SnapshotHashes();
        string[] arguments =
        [
            "route", "update", RouteUpdateIntegrationWorkspace.TargetId,
            "--description", "After overview", "--dry-run", "--format", "json",
        ];

        var plain = await CliHostCapture.RunAsync(arguments, workspace.Workspace.LexicalRoot);
        var verbose = await CliHostCapture.RunAsync([.. arguments, "--detail", "debug"], workspace.Workspace.LexicalRoot);

        Assert.Equal(0, plain.ExitCode);
        Assert.Equal(string.Empty, plain.Error);
        Assert.Equal(plain.ExitCode, verbose.ExitCode);
        var diagnostics = verbose.Error.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(10, diagnostics.Length);
        Assert.InRange(verbose.Error.Length, 1, 4096);
        Assert.All(diagnostics, diagnostic =>
        {
            Assert.InRange(diagnostic.Length, 1, 240);
            Assert.DoesNotContain('\r', diagnostic);
            Assert.DoesNotContain('\n', diagnostic);
        });
        Assert.EndsWith(Environment.NewLine, verbose.Error, StringComparison.Ordinal);
        Assert.Equal("status=completed", diagnostics[0]);
        Assert.Equal("mode=dry-run", diagnostics[1]);
        using var plainDocument = System.Text.Json.JsonDocument.Parse(plain.Output);
        using var verboseDocument = System.Text.Json.JsonDocument.Parse(verbose.Output);
        var plainData = plainDocument.RootElement.GetProperty("data");
        var verboseData = verboseDocument.RootElement.GetProperty("data");
        Assert.Equal("minimal", plainDocument.RootElement.GetProperty("detail").GetString());
        Assert.Equal("debug", verboseDocument.RootElement.GetProperty("detail").GetString());
        Assert.Equal(plainData.GetProperty("mode").GetString(), verboseData.GetProperty("mode").GetString());
        Assert.Equal(
            plainData.GetProperty("target").GetProperty("id").GetString(),
            verboseData.GetProperty("target").GetProperty("id").GetString());
        Assert.False(plainData.TryGetProperty("frontmatterBefore", out _));
        Assert.False(plainData.TryGetProperty("frontmatterAfter", out _));
        Assert.True(verboseData.TryGetProperty("frontmatterBefore", out _));
        Assert.True(verboseData.TryGetProperty("frontmatterAfter", out _));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Route Update ID base overwrite and normalized paths apply the base then converge"),
     InlineData(RouteUpdateIntegrationWorkspace.TargetId, (int)RouteUpdateTargetSelection.SourceId),
     InlineData(RouteUpdateIntegrationWorkspace.TargetPath, (int)RouteUpdateTargetSelection.BasePath),
     InlineData("./" + RouteUpdateIntegrationWorkspace.TargetPath, (int)RouteUpdateTargetSelection.BasePath),
     InlineData(RouteUpdateIntegrationWorkspace.OverwritePath, (int)RouteUpdateTargetSelection.OverwritePath),
     InlineData("./" + RouteUpdateIntegrationWorkspace.OverwritePath, (int)RouteUpdateTargetSelection.OverwritePath),
     Trait("Feature", "route-update"), Trait("Evidence", "IntegrationBehavior")]
    public static async Task ApplyThenRepeatConverges(
        string sourceReference,
        int expectedSelectionValue)
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            $"route-update-apply-{expectedSelectionValue}");
        workspace.SeedOverwrite();
        var request = workspace.Request(sourceReference: sourceReference);
        var overwriteBefore = workspace.ReadBytes(RouteUpdateIntegrationWorkspace.OverwritePath);
        var before = workspace.SnapshotHashes();

        var applied = await workspace.ExecuteAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal(RouteUpdateVerificationState.Verified, applied.Verification);
        Assert.Equal(
            (RouteUpdateTargetSelection)expectedSelectionValue,
            applied.Target.SelectedBy);
        Assert.Equal("After overview", ReadDescription(workspace.ReadText(
            RouteUpdateIntegrationWorkspace.TargetPath)));
        Assert.Equal(
            overwriteBefore,
            workspace.ReadBytes(RouteUpdateIntegrationWorkspace.OverwritePath));
        Assert.NotEqual(before, workspace.SnapshotHashes());
        var after = workspace.SnapshotHashes();

        var noOp = await workspace.ExecuteAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, noOp.Status);
        Assert.Empty(noOp.Effects);
        Assert.Equal(RouteUpdateRecoveryState.NotRequired, noOp.Recovery.State);
        Assert.Equal(after, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Route Update applies canonical and compatibility entrypoints without identity drift"),
     InlineData(".agents/memory/project-alpha/overview/_overview.md", (int)RouteUpdateTargetForm.CanonicalEntrypoint),
     InlineData(".agents/memory/project-alpha/overview/index.md", (int)RouteUpdateTargetForm.CompatibilityEntrypoint),
     InlineData(".agents/memory/project-alpha/overview/_index.md", (int)RouteUpdateTargetForm.CompatibilityEntrypoint),
     InlineData(".agents/memory/project-alpha/overview/references.md", (int)RouteUpdateTargetForm.CompatibilityEntrypoint),
     InlineData(".agents/memory/project-alpha/overview/_references.md", (int)RouteUpdateTargetForm.CompatibilityEntrypoint),
     Trait("Feature", "route-update"), Trait("Evidence", "IntegrationBehavior")]
    public static async Task EntrypointFormsApplyWithoutIdentityDrift(
        string path,
        int expectedFormValue)
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            $"route-update-entrypoint-{Path.GetFileNameWithoutExtension(path)}");
        workspace.SeedTargetForm(path);

        var result = await workspace.ExecuteAsync(
            workspace.Request(sourceReference: path),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(RouteUpdateVerificationState.Verified, result.Verification);
        Assert.Equal(path, result.Target.Path);
        Assert.Equal((RouteUpdateTargetForm)expectedFormValue, result.Target.Form);
        Assert.False(File.Exists(workspace.Absolute(RouteUpdateIntegrationWorkspace.TargetPath)));
        Assert.Equal(
            "After overview",
            ReadDescription(workspace.ReadText(path)));
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Route Update Template-only application validates and completes entrypoint navigation"),
     InlineData(".agents/memory/project-alpha/overview/_overview.md"),
     InlineData(".agents/memory/project-alpha/overview/index.md"),
     InlineData(".agents/memory/project-alpha/overview/_index.md"),
     InlineData(".agents/memory/project-alpha/overview/references.md"),
     InlineData(".agents/memory/project-alpha/overview/_references.md"),
     Trait("Feature", "route-update"), Trait("Evidence", "IntegrationBehavior")]
    public static async Task TemplateOnlyEntrypointApplicationCompletesNavigation(string path)
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            $"route-update-template-entrypoint-{Path.GetFileNameWithoutExtension(path)}");
        workspace.SeedEmptyEntrypointTarget(path);
        workspace.SeedEntrypointTemplate(includeGeneratedRegion: true);
        var request = workspace.Request(
            sourceReference: path,
            patch: RouteUpdateIntegrationWorkspace.Patch(),
            templateReference: RouteUpdateIntegrationWorkspace.TemplateId);

        var build = await RouteUpdateIntegrationWorkspace.BuildPlanAsync(request);
        var plan = Assert.IsType<RouteUpdatePlan>(build.Plan);
        Assert.Equal(
            [path, RouteUpdateIntegrationWorkspace.ParentPath],
            plan.Navigation.Regions.Select(region => region.Source.Identity.CanonicalBasePath));
        Assert.Equal(
            [workspace.Absolute(path), workspace.Absolute(RouteUpdateIntegrationWorkspace.ParentPath)],
            plan.FileChanges.Select(change => change.LogicalPath));

        var result = await workspace.ExecuteAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(RouteUpdateBodyState.TemplateCopied, result.Plan.Body);
        Assert.Equal(RouteUpdateVerificationState.Verified, result.Verification);
        Assert.Contains(
            "- [Child](child.md) - #Child",
            workspace.ReadText(path),
            StringComparison.Ordinal);
        Assert.DoesNotContain(
            "- [Stale child](child.md) - #Stale",
            workspace.ReadText(path),
            StringComparison.Ordinal);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Update blocks a Template-only entrypoint whose intended body has no generated region"),
     Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task TemplateOnlyEntrypointRequiresValidIntendedRepresentation()
    {
        const string path = ".agents/memory/project-alpha/overview/_overview.md";
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-template-entrypoint-invalid");
        workspace.SeedEmptyEntrypointTarget(path);
        workspace.SeedEntrypointTemplate(includeGeneratedRegion: false);
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(
            workspace.Request(
                sourceReference: path,
                patch: RouteUpdateIntegrationWorkspace.Patch(),
                templateReference: RouteUpdateIntegrationWorkspace.TemplateId),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RouteUpdateFindingCode.GeneratedRegionUnsafe);
        Assert.Empty(result.Effects);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Update real application preserves opaque YAML Unicode comments and mixed line endings byte-exact"),
     Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task ApplicationPreservesOpaqueTargetBytes()
    {
        const string before = "---\r\nopen-forge:\r\n  opaque: café # keep\r\n"
            + "  description: \"Before overview\"\n"
            + "  responsibility: Owns the overview\r\n"
            + "  tags: [Before, Memory]\r\n"
            + "  unknown: one\r\n  unknown: two\r\n---\r\n\r\n"
            + "# Authored Ω\r\n\r\nPreserve this body.\r\n";
        const string expected = "---\r\nopen-forge:\r\n  opaque: café # keep\r\n"
            + "  description: After overview\n"
            + "  responsibility: Owns the overview\r\n"
            + "  tags: [Before, Memory]\r\n"
            + "  unknown: one\r\n  unknown: two\r\n---\r\n\r\n"
            + "# Authored Ω\r\n\r\nPreserve this body.\r\n";
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-opaque-byte-preservation");
        workspace.SeedTargetText(before);

        var result = await workspace.ExecuteAsync(
            workspace.Request(),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(RouteUpdateVerificationState.Verified, result.Verification);
        Assert.Equal(
            Encoding.UTF8.GetBytes(expected),
            workspace.ReadBytes(RouteUpdateIntegrationWorkspace.TargetPath));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Update applies an eligible Template body and verifies the protected no-op transition"),
     Trait("Feature", "route-update"), Trait("Evidence", "IntegrationBehavior")]
    public async Task EligibleTemplateBodyAppliesAndVerifies()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-template-apply");
        workspace.SeedEmptyBodyTarget();
        workspace.SeedOverwrite();
        workspace.SeedTemplate();
        var parentBefore = workspace.ReadText(RouteUpdateIntegrationWorkspace.ParentPath);
        var overwriteBefore = workspace.ReadText(RouteUpdateIntegrationWorkspace.OverwritePath);

        var result = await workspace.ExecuteAsync(
            workspace.Request(
                patch: RouteUpdateIntegrationWorkspace.Patch(),
                templateReference: RouteUpdateIntegrationWorkspace.TemplateId),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(RouteUpdateVerificationState.Verified, result.Verification);
        Assert.Equal(RouteUpdateBodyState.TemplateCopied, result.Plan.Body);
        Assert.Equal(RouteUpdateTemplateDecision.Copied, result.Template?.Decision);
        Assert.Equal(RouteUpdateRecoveryState.Removed, result.Recovery.State);
        var effect = Assert.Single(result.Effects);
        Assert.Equal(RouteUpdateIntegrationWorkspace.TargetPath, effect.Path);
        Assert.Equal(RouteUpdateEffectOutcome.Verified, effect.Outcome);
        var expectedBody = Encoding.UTF8.GetBytes(RouteUpdateIntegrationWorkspace.TemplateBody);
        var targetBytes = File.ReadAllBytes(workspace.Absolute(
            RouteUpdateIntegrationWorkspace.TargetPath));
        Assert.True(targetBytes.AsSpan(targetBytes.Length - expectedBody.Length)
            .SequenceEqual(expectedBody));
        Assert.Equal(
            parentBefore,
            workspace.ReadText(RouteUpdateIntegrationWorkspace.ParentPath));
        Assert.Equal(
            overwriteBefore,
            workspace.ReadText(RouteUpdateIntegrationWorkspace.OverwritePath));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Update dry-run returns the complete plan with zero workspace writes"),
     Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task DryRunDoesNotAcquireOrWrite()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-dry-run-zero-write");
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(
            workspace.Request(mode: RouteUpdateMode.DryRun),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(RouteUpdateMode.DryRun, result.Mode);
        Assert.All(
            result.Effects,
            effect => Assert.Equal(RouteUpdateEffectOutcome.Planned, effect.Outcome));
        Assert.Equal(RouteUpdateRecoveryState.NotCreated, result.Recovery.State);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Update lock contention is blocked before revalidation recovery or writes"),
     Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task ExistingWorkspaceLeaseBlocksApplication()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-lock-race");
        await using var lease = await workspace.AcquireLeaseAsync(Guid.NewGuid());
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(
            workspace.Request(),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RouteUpdateFindingCode.WorkspaceLockUnavailable);
        Assert.Equal(RouteUpdateRecoveryState.NotCreated, result.Recovery.State);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "Route Update revalidation rejects target overwrite Template and parent races before writes"),
     InlineData("target"),
     InlineData("overwrite"),
     InlineData("template"),
     InlineData("parent"),
     Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public static async Task CompletePlanMustRemainExact(string changedSource)
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            $"route-update-revalidation-{changedSource}");
        workspace.SeedOverwrite();
        workspace.SeedTemplate();
        var request = workspace.Request(
            templateReference: RouteUpdateIntegrationWorkspace.TemplateId);
        var build = await RouteUpdateIntegrationWorkspace.BuildPlanAsync(request);
        var plan = Assert.IsType<RouteUpdatePlan>(build.Plan);
        Change(workspace, changedSource);
        var afterRace = workspace.SnapshotHashes();

        var result = await workspace.ExecutePlanAsync(plan);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RouteUpdateFindingCode.TargetChanged);
        Assert.Equal(afterRace, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Update cancellation is returned as interrupted without mutation"),
     Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task CallerCancellationIsTypedAndWriteFree()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-interrupted");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var before = workspace.SnapshotHashes();

        var result = await workspace.ExecuteAsync(
            workspace.Request(),
            cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RouteUpdateFindingCode.Interrupted);
        Assert.Empty(result.Effects);
        Assert.Equal(RouteUpdateRecoveryState.NotCreated, result.Recovery.State);
        Assert.Equal(RouteUpdateVerificationState.NotRequested, result.Verification);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Route Update top-level verification failure retains exact applied effects and recovery"),
     Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task TopLevelPostWriteFailureReturnsCompleteResidualFacts()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-top-level-post-write-failure");
        var planBuild = await RouteUpdateIntegrationWorkspace.BuildPlanAsync(workspace.Request());
        var plan = Assert.IsType<RouteUpdatePlan>(planBuild.Plan);
        var operationId = Guid.NewGuid();
        await using var lease = await workspace.AcquireLeaseAsync(operationId);
        var validator = new FileExpectationValidator(new PhysicalPathResolver());
        var revalidator = new MutationRevalidator(validator);
        var planBuilder = RouteUpdateIntegrationWorkspace.CreatePlanBuilder();
        var preparation = await new RouteUpdateApplicationPreparer(
            new RouteUpdatePlanRevalidator(
                planBuilder,
                new RouteUpdatePlanEquivalence()),
            revalidator).PrepareAsync(
                new RouteUpdateApplicationPipelineInput
                {
                    Plan = plan,
                    Lease = lease,
                    OperationId = operationId,
                },
                TestContext.Current.CancellationToken);
        Assert.Equal(RouteUpdateApplicationPreparationState.Ready, preparation.State);
        var recoveryPreparation = Assert.IsType<OpenForge.Cli.Core.Framework.Recovery.Models.Preparation.RecoveryBundlePreparation>(
            preparation.RecoveryPreparation);
        workspace.TrackRecovery(recoveryPreparation);
        var validation = Assert.IsType<OpenForge.Cli.Core.Framework.Mutation.Validation.Models.MutationValidationResult>(
            preparation.Validation);
        var application = await new RouteUpdateEffectApplication(
            new FileChangeApplier(revalidator, validator).ApplyAsync).ApplyAsync(
                new RouteUpdateEffectApplicationInput
                {
                    Plan = plan,
                    Preparation = recoveryPreparation,
                    Lease = lease,
                    Validation = validation,
                },
                TestContext.Current.CancellationToken);
        Assert.Empty(application.Findings);
        Assert.Equal(plan.FileChanges.Length, application.Receipts.Length);
        workspace.MutateTargetAfterPlanning();

        var verification = await new RouteUpdateAppliedVerifier(
            planBuilder,
            validator).VerifyAsync(
                new RouteUpdateAppliedVerificationInput
                {
                    Plan = plan,
                    Progress = application,
                    Lease = lease,
                },
                TestContext.Current.CancellationToken);
        Assert.Equal(RouteUpdateAppliedVerificationState.Failed, verification.State);
        var progress = application with
        {
            Verification = RouteUpdateVerificationState.Failed,
            Findings =
            [
                new RouteUpdateFinding(
                    RouteUpdateFindingCode.VerificationFailed,
                    verification.Cause
                        ?? "Final Route Update verification did not complete.",
                    plan.Preview.Target.Path),
            ],
        };
        var result = new RouteUpdateResultBuilder().Build(
            RouteUpdateApplicationResultFactory.Build(plan, progress));

        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Equal(RouteUpdateVerificationState.Failed, result.Verification);
        Assert.Equal(RouteUpdateRecoveryState.Retained, result.Recovery.State);
        Assert.False(string.IsNullOrWhiteSpace(result.Recovery.ResidualPath));
        Assert.All(result.Effects, effect =>
        {
            Assert.Equal(RouteUpdateEffectOutcome.Verified, effect.Outcome);
            Assert.Equal(RouteUpdateEffectResidual.Retained, effect.Residual);
        });
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RouteUpdateFindingCode.VerificationFailed);
        Assert.Equal("open-forge route update --detail debug", result.Next?.Command);
        Assert.Equal(recoveryPreparation.BundlePath, result.Recovery.ResidualPath);
        Assert.True(File.Exists(recoveryPreparation.BundlePath));
        var bundleDirectory = Assert.IsType<string>(Path.GetDirectoryName(recoveryPreparation.BundlePath));
        Assert.Equal([recoveryPreparation.BundlePath], Directory.GetFiles(bundleDirectory));

        var presentation = CliPresentationStage.Create(
            result,
            new CliPresentation(CliFormat.Text, CliDetail.Full, null));
        var rendered = CliRenderingStage.Render(
            presentation,
            RouteUpdatePresentation.Rendering);
        using var stdout = new StringWriter();
        using var stderr = new StringWriter();
        var receipt = await CliOutputStage.WriteAsync(
            rendered,
            new CliOutputWriters(stdout, stderr),
            TestContext.Current.CancellationToken);
        var completion = CliCompletionStage.Complete(receipt);

        Assert.Equal(1, completion.ExitCode);
        Assert.Equal(string.Empty, stdout.ToString());
        var human = stderr.ToString();
        Assert.Contains("Route update stopped after", human, StringComparison.Ordinal);
        Assert.Contains(Assert.IsType<string>(plan.Preview.Target.Id), human, StringComparison.Ordinal);
        Assert.DoesNotContain("Status:", human, StringComparison.Ordinal);
        Assert.Contains("Before:", human, StringComparison.Ordinal);
        Assert.Contains("After:", human, StringComparison.Ordinal);
        Assert.Contains("Recovery data", human, StringComparison.Ordinal);
        Assert.Contains(recoveryPreparation.BundlePath, human, StringComparison.Ordinal);
        Assert.Contains("Next: open-forge route update --detail debug", human, StringComparison.Ordinal);
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Route Update cancellation after the first effect retains its receipt and stops later effects"),
     Trait("Feature", "route-update"), Trait("Evidence", "IntegrationSafety")]
    public async Task MidApplicationCancellationRetainsExactProgress()
    {
        using var workspace = RouteUpdateIntegrationWorkspace.Create(
            "route-update-mid-application-cancellation");
        var expectedTarget = Encoding.UTF8.GetBytes(
            workspace.ReadText(RouteUpdateIntegrationWorkspace.TargetPath).Replace(
                "description: Before overview",
                "description: After overview",
                StringComparison.Ordinal));
        var parentBefore = workspace.ReadBytes(RouteUpdateIntegrationWorkspace.ParentPath);
        using var cancellation = new CancellationTokenSource();
        var validator = new FileExpectationValidator(new PhysicalPathResolver());
        var revalidator = new MutationRevalidator(validator);
        var fileApplier = new FileChangeApplier(revalidator, validator);
        var receipts = new List<FileChangeReceipt>();
        RouteUpdateFileApplication applyFile = async (lease, change, check, preparation, token) =>
        {
            var receipt = await fileApplier.ApplyAsync(lease, change, check, preparation, token);
            receipts.Add(receipt);
            if (receipts.Count == 1
                && receipt.EffectState == FilesystemEffectState.Applied
                && receipt.VerificationState == FilesystemVerificationState.Verified)
            {
                cancellation.Cancel();
            }

            return receipt;
        };
        var planBuilder = RouteUpdateIntegrationWorkspace.CreatePlanBuilder();
        var resultBuilder = new RouteUpdateResultBuilder();
        var operation = new RouteUpdateOperation(
            planBuilder,
            new RouteUpdateApplicationOperation(
                new RouteUpdateApplicationPipeline(
                    new RouteUpdateApplicationPreparer(
                        new RouteUpdatePlanRevalidator(planBuilder, new RouteUpdatePlanEquivalence()),
                        revalidator),
                    new RouteUpdateEffectApplication(applyFile),
                    new RouteUpdateAppliedVerifier(planBuilder, validator)),
                resultBuilder,
                workspace.LockStoreRoot),
            resultBuilder);

        var result = await operation.ExecuteAsync(workspace.Request(), cancellation.Token);

        Assert.True(cancellation.IsCancellationRequested);
        Assert.Collection(
            receipts,
            first =>
            {
                Assert.Equal(FilesystemEffectState.Applied, first.EffectState);
                Assert.Equal(FilesystemVerificationState.Verified, first.VerificationState);
            },
            second =>
            {
                Assert.Equal(FilesystemEffectState.NotStarted, second.EffectState);
                Assert.Equal(FilesystemVerificationState.NotStarted, second.VerificationState);
                Assert.Equal(FilesystemNotStartedReason.Cancelled, second.NotStartedReason);
            });
        Assert.Equal(expectedTarget, workspace.ReadBytes(RouteUpdateIntegrationWorkspace.TargetPath));
        Assert.Equal(parentBefore, workspace.ReadBytes(RouteUpdateIntegrationWorkspace.ParentPath));
        Assert.Equal(2, result.Effects.Length);
        Assert.Equal(RouteUpdateIntegrationWorkspace.TargetPath, result.Effects[0].Path);
        Assert.Equal(RouteUpdateIntegrationWorkspace.ParentPath, result.Effects[1].Path);
        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(RouteUpdateRecoveryState.Retained, result.Recovery.State);
        Assert.False(string.IsNullOrWhiteSpace(result.Recovery.ResidualPath));
        Assert.Equal(RouteUpdateVerificationState.Unknown, result.Verification);
        Assert.Equal(RouteUpdateEffectOutcome.Verified, result.Effects[0].Outcome);
        Assert.Equal(RouteUpdateEffectResidual.Retained, result.Effects[0].Residual);
        Assert.Equal(RouteUpdateEffectOutcome.NotStarted, result.Effects[1].Outcome);
        Assert.Equal(RouteUpdateEffectResidual.None, result.Effects[1].Residual);
        Assert.Contains(
            result.Findings,
            finding => finding.Code == RouteUpdateFindingCode.Interrupted);
        Assert.Equal("open-forge route update", result.Next?.Command);
    }

    private static void Change(
        RouteUpdateIntegrationWorkspace workspace,
        string source)
    {
        switch (source)
        {
            case "target":
                workspace.MutateTargetAfterPlanning();
                return;
            case "overwrite":
                workspace.MutateOverwriteAfterPlanning();
                return;
            case "template":
                workspace.MutateTemplateAfterPlanning();
                return;
            case "parent":
                workspace.MutateParentAfterPlanning();
                return;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(source),
                    source,
                    "The Route Update race source is not defined.");
        }
    }

    private static string ReadDescription(string document)
    {
        const string prefix = "  description: ";
        return document.Split('\n', StringSplitOptions.None)
            .Single(line => line.StartsWith(prefix, StringComparison.Ordinal))[prefix.Length..];
    }
}
