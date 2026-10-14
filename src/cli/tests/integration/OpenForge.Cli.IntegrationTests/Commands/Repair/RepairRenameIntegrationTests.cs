using OpenForge.Cli.IntegrationTests.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Composition.Shared.Git;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Repair.Models.Interaction;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Presentation.Repair.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Commands.Repair.Shared.Application;
using OpenForge.Cli.Core.Commands.Repair.Shared.Result;
using OpenForge.Cli.Core.Framework.Mutation.Application;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands.Models;
using OpenForge.Cli.Core.Presentation.Repair;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Text;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair;

public sealed class RepairRenameIntegrationTests
{
    [Fact(DisplayName = "A metadata-only choice keeps stale identity evidence until its last broken occurrence is relinked")]
    [Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "Processing")]
    public async Task SequentialMetadataChoicesKeepEvidenceUntilLastOccurrence()
    {
        using var fixture = new RepairRenameGitWorkspace();
        const string oldPath = ".agents/guidance/billing/rounding.md";
        const string targetPath = ".agents/guidance/billing/new.md";
        const string navigationPath = ".agents/guidance/billing/_billing.md";
        const string firstSource = ".agents/guidance/billing/taxes.md";
        const string secondSource = ".agents/guidance/notes.md";
        await fixture.InitializeAsync(new Dictionary<string, string>
        {
            [oldPath] = Document("Money rounding policy", "# Rounding\n\nRound only at the final calculation.\n"),
            [navigationPath] = Document("Billing rules", "# Billing\n\n## Axioms\n\n- Keep our billing rules.\n\n## Entries\n"),
            [firstSource] = Document("Tax references", "# Taxes\n\nRead [ZZFirstLabelZZ](rounding.md#rounding).\n"),
            [secondSource] = Document("Maintainer notes", "# Notes\n\nRead [ZZSecondLabelZZ](billing/rounding.md#rounding).\n"),
        }, unborn: true);
        await OpenForge.Cli.Core.Commands.Index.IndexOperationFactory.Create(renameEvidence: Scaffold(fixture))
            .ExecuteAsync(new(fixture.Workspace, [], IndexMode.Apply), TestContext.Current.CancellationToken);
        var oldNavigation = File.ReadAllBytes(fixture.PathFor(navigationPath));
        var secondBytes = File.ReadAllBytes(fixture.PathFor(secondSource));
        fixture.Move(oldPath, targetPath);
        fixture.Write(targetPath, Document("Money rounding policy", "# Changed heading\n\nCompletely different body.\n"));
        var operation = new RepairOperation(Components(fixture));
        async Task<RepairResult> Read() => await operation.ExecuteAsync(new(fixture.Workspace, RepairMode.DryRun, true, [], false), TestContext.Current.CancellationToken);
        RepairRelinkRequest Choose(RepairResult result, string source)
        {
            var proposal = Assert.Single(result.Selection?.Unselected ?? [], item => item.SourceCanonicalPath == source);
            var candidate = Assert.Single(proposal.Candidates?.Items ?? []);
            Assert.Null(candidate.Target.TargetFragment);
            Assert.NotNull(candidate.Target.RenameProof);
            return new(new(source, proposal.OccurrenceView.Line, proposal.OccurrenceView.Column), proposal.ExpectedDestination, new(targetPath, null));
        }
        var initial = await Read();
        Assert.Equal(2, initial.Selection?.Unselected.Count);
        var first = Choose(initial, firstSource);
        var preview = await operation.ExecuteAsync(new(fixture.Workspace, RepairMode.DryRun, true, [first], false), TestContext.Current.CancellationToken);
        Assert.DoesNotContain(preview.MaintenanceResults.Navigation, row => row.Path == navigationPath);
        var applied = await operation.ExecuteAsync(new(fixture.Workspace, RepairMode.Apply, true, [first], false), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Attention, applied.Status);
        Assert.Equal(oldNavigation, File.ReadAllBytes(fixture.PathFor(navigationPath)));
        Assert.Equal(secondBytes, File.ReadAllBytes(fixture.PathFor(secondSource)));
        Assert.Contains("[ZZFirstLabelZZ](new.md)", File.ReadAllText(fixture.PathFor(firstSource)));
        Assert.Equal("Choose a target above, then run its command.", RepairReportSelector.Select(applied, new(CliDetail.Full)).Next?.Command);
        var continuation = await Read();
        var second = Choose(continuation, secondSource);
        var last = await operation.ExecuteAsync(new(fixture.Workspace, RepairMode.Apply, true, [second], false), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, last.Status);
        Assert.Contains("[ZZSecondLabelZZ](billing/new.md)", File.ReadAllText(fixture.PathFor(secondSource)));
        var finalNavigation = File.ReadAllText(fixture.PathFor(navigationPath));
        Assert.DoesNotContain("](rounding.md)", finalNavigation);
        Assert.Contains("](new.md)", finalNavigation);
        Assert.Contains("- Keep our billing rules.", finalNavigation);
        Assert.Contains(last.MaintenanceResults.Navigation, row => row.Path == navigationPath);
        await AssertHealthyAsync(fixture, Components(fixture));
    }

    [Theory(DisplayName = "An explicit choice completes both route closures and leaves only unrelated Index work")]
    [InlineData(false), InlineData(true), Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "Processing")]
    public async Task ExplicitChoiceCompletesOldAndNewClosures(bool unrelatedWork)
    {
        using var fixture = new RepairRenameGitWorkspace();
        const string oldPath = ".agents/guidance/billing/rounding.md";
        const string targetPath = ".agents/guidance/finance/deep/rounding.md";
        const string sourcePath = ".agents/guidance/notes/notes.md";
        await fixture.InitializeAsync(new Dictionary<string, string>
        {
            [oldPath] = Document("Money rounding policy", "# Rounding\n\nKeep integer minor units.\n"),
            [".agents/guidance/billing/_billing.md"] = Document("Billing rules", "# Billing\n\n## Axioms\n\n- Keep our billing rules.\n\n## Entries\n"),
            [".agents/guidance/notes/_notes.md"] = Document("Notes route", "# Notes route\n\n## Entries\n"),
            [sourcePath] = Document("Maintainer notes", "# Notes\n\nRead [Rounding](../billing/rounding.md#rounding).\n"),
            [".agents/guidance/unrelated/_unrelated.md"] = Document("Unrelated route", "# Unrelated\n\n## Entries\n"),
        });
        var scaffold = Scaffold(fixture);
        var index = OpenForge.Cli.Core.Commands.Index.IndexOperationFactory.Create(renameEvidence: scaffold);
        await index.ExecuteAsync(new(fixture.Workspace, [], IndexMode.Apply), TestContext.Current.CancellationToken);
        File.Delete(fixture.PathFor(oldPath));
        fixture.Write(targetPath, "# Rounding\n\nA different rounding policy selected by its author.\n");
        fixture.Write(".agents/guidance/other-rounding.md", Document("Other arithmetic guidance", "# Rounding\n\nAnother possible policy.\n"));
        if (unrelatedWork) fixture.Write(".agents/guidance/unrelated/note.md", "# Unrelated note\n\nKeep unrelated work separate.\n");
        var operation = new RepairOperation(Components(fixture));
        var initial = await operation.ExecuteAsync(new(fixture.Workspace, RepairMode.DryRun, true, [], false), TestContext.Current.CancellationToken);
        var proposal = Assert.Single(initial.Selection?.Unselected ?? []);
        var relink = new RepairRelinkRequest(new(sourcePath, proposal.OccurrenceView.Line, proposal.OccurrenceView.Column), proposal.ExpectedDestination, new(targetPath, "rounding"));
        var before = File.ReadAllText(fixture.PathFor(sourcePath));
        var preview = await operation.ExecuteAsync(new(fixture.Workspace, RepairMode.DryRun, true, [relink], false), TestContext.Current.CancellationToken);
        Assert.Equal(before, File.ReadAllText(fixture.PathFor(sourcePath)));
        Assert.False(File.Exists(fixture.PathFor(".agents/guidance/finance/_finance.md")));
        Assert.Contains(preview.MaintenanceResults.Metadata, row => row.Path == targetPath);
        Assert.Equal(2, preview.MaintenanceResults.Entrypoints.Count);
        var result = await operation.ExecuteAsync(new(fixture.Workspace, RepairMode.Apply, true, [relink], false), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Contains("[Rounding](../finance/deep/rounding.md#rounding)", File.ReadAllText(fixture.PathFor(sourcePath)));
        Assert.DoesNotContain("](rounding.md)", File.ReadAllText(fixture.PathFor(".agents/guidance/billing/_billing.md")));
        Assert.Contains("- Keep our billing rules.", File.ReadAllText(fixture.PathFor(".agents/guidance/billing/_billing.md")));
        Assert.Contains("](deep/_deep.md)", File.ReadAllText(fixture.PathFor(".agents/guidance/finance/_finance.md")));
        Assert.Contains("](rounding.md)", File.ReadAllText(fixture.PathFor(".agents/guidance/finance/deep/_deep.md")));
        Assert.Contains("open-forge:", File.ReadAllText(fixture.PathFor(targetPath)));
        Assert.Equal(unrelatedWork ? "open-forge index" : "open-forge doctor", RepairReportSelector.Select(result, new(CliDetail.Standard)).Next?.Command);
        if (unrelatedWork)
        {
            Assert.Equal("# Unrelated note\n\nKeep unrelated work separate.\n", File.ReadAllText(fixture.PathFor(".agents/guidance/unrelated/note.md")));
            await index.ExecuteAsync(new(fixture.Workspace, [], IndexMode.Apply), TestContext.Current.CancellationToken);
        }
        await AssertHealthyAsync(fixture, Components(fixture));
    }

    [Fact(DisplayName = "Timed-out Git cannot hide a committed successor identity and authorize an automatic relink")]
    [Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
    public async Task GitTimeoutCannotRelinkToContradictingCommittedIdentity()
    {
        using var fixture = new RepairRenameGitWorkspace();
        const string oldPath = ".agents/guidance/rounding.md";
        const string otherPath = ".agents/guidance/taxes.md";
        const string sourcePath = ".agents/guidance/notes.md";
        const string navigationPath = ".agents/guidance/_guidance.md";
        await fixture.InitializeAsync(new Dictionary<string, string>
        {
            [oldPath] = Document("Money rounding policy", "# Rounding\n\nUse integer minor units.\n"),
            [otherPath] = Document("Money rounding policy", "# Taxes\n\nThis independent tax note is not rounding.\n"),
            [sourcePath] = Document("Maintainer notes", "# Notes\n\nRead [Rounding](rounding.md).\n"),
        });
        var healthy = Scaffold(fixture);
        await OpenForge.Cli.Core.Commands.Index.IndexOperationFactory.Create(renameEvidence: healthy).ExecuteAsync(new(fixture.Workspace, [], IndexMode.Apply), TestContext.Current.CancellationToken);
        File.Delete(fixture.PathFor(oldPath));
        fixture.Write(navigationPath, string.Join('\n', File.ReadAllText(fixture.PathFor(navigationPath)).Split('\n').Where(line => !line.Contains("](taxes.md)", StringComparison.Ordinal))));
        var before = Directory.GetFiles(fixture.PathFor(".agents"), "*", SearchOption.AllDirectories).ToDictionary(path => path, File.ReadAllBytes);
        var normal = await new RepairOperation(Components(fixture)).ExecuteAsync(new(fixture.Workspace, RepairMode.DryRun, true, [], false), TestContext.Current.CancellationToken);
        Assert.Empty(normal.Selection?.Selected ?? []);
        Assert.Null((await healthy.ReadEvidenceAsync(fixture.Workspace, TestContext.Current.CancellationToken)).Find(oldPath)?.Proven);

        var timedOut = new RoutedMaintenanceRenameEvidenceAdapter(new(new PhysicalPathResolver(), (_, _) => ValueTask.FromResult(RenameGitObservation.Unavailable(RenameGitAvailability.TimedOut))));
        var result = await new RepairOperation(RepairOperationFactory.CreateDefaultComponents(new(timedOut.ReadEvidenceAsync, timedOut)))
            .ExecuteAsync(new(fixture.Workspace, RepairMode.Apply, true, [], false), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Empty(result.Selection?.Selected ?? []);
        Assert.Empty(result.MaintenanceResults.Renames);
        foreach (var file in before) Assert.Equal(file.Value, File.ReadAllBytes(file.Key));
        Assert.Equal(before.Count, Directory.GetFiles(fixture.PathFor(".agents"), "*", SearchOption.AllDirectories).Length);
        var report = RepairReportSelector.Select(result, new(CliDetail.Standard));
        Assert.Contains(report.Limitations, limitation => limitation.What.Contains("Git did not answer in time", StringComparison.Ordinal));
        Assert.DoesNotContain(report.Findings, finding => finding.Code == "repair.rename-proven");
        Assert.DoesNotContain(report.Findings.SelectMany(finding => finding.Actions), action => action.Command.StartsWith("Update or remove this link", StringComparison.Ordinal));
        var retry = $"open-forge repair --automatic --workspace '{fixture.Root}' --format json --detail full";
        var replay = new CliRequestReplay("open-forge repair", "open-forge repair", retry, retry, retry) { WorkspaceSpecified = true, ApplyCommand = retry };
        Assert.Equal(retry, CliNextActionCompletion.Complete(report, replay).Next?.Command);
    }

    [Fact(DisplayName = "A stale-metadata relink loses authorization if Git times out during the lease-time recheck")]
    [Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "Processing")]
    public async Task GitTimeoutUnderLeaseInvalidatesMetadataProofBeforeEffects()
    {
        using var fixture = new RepairRenameGitWorkspace();
        await fixture.InitializeAsync(new Dictionary<string, string>
        {
            [".agents/guidance/rounding.md"] = Document("Money rounding policy", "# Rounding\n\nUse integer minor units.\n"),
            [".agents/guidance/notes.md"] = Document("Maintainer notes", "# Notes\n\nRead [Rounding](rounding.md).\n"),
        });
        await OpenForge.Cli.Core.Commands.Index.IndexOperationFactory.Create(renameEvidence: Scaffold(fixture)).ExecuteAsync(new(fixture.Workspace, [], IndexMode.Apply), TestContext.Current.CancellationToken);
        fixture.Move(".agents/guidance/rounding.md", ".agents/guidance/money-rounding.md");
        var availability = RenameGitAvailability.Unavailable;
        var scaffold = new RoutedMaintenanceRenameEvidenceAdapter(new(new PhysicalPathResolver(), (_, _) => ValueTask.FromResult(RenameGitObservation.Unavailable(availability))));
        var components = RepairOperationFactory.CreateDefaultComponents(new(scaffold.ReadEvidenceAsync, scaffold));
        var preview = await new RepairOperation(components).ExecuteAsync(new(fixture.Workspace, RepairMode.DryRun, true, [], false), TestContext.Current.CancellationToken);
        Assert.Equal("recorded-metadata", Assert.Single(preview.MaintenanceResults.Renames).Evidence);
        var original = Assert.IsType<OpenForge.Cli.Core.Commands.Repair.Models.Planning.RepairPlan>(preview.Plan);
        var plan = new OpenForge.Cli.Core.Commands.Repair.Models.Planning.RepairPlan(new(fixture.Workspace, RepairMode.Apply, true, [], false), original.Selection, original.Steps, original.Conflicts, original.LibrarySteps)
        { Maintenance = original.Maintenance, RenameEvidence = original.RenameEvidence };
        var before = Directory.GetFiles(fixture.PathFor(".agents"), "*", SearchOption.AllDirectories).ToDictionary(path => path, File.ReadAllBytes);
        availability = RenameGitAvailability.TimedOut;
        var outcome = await components.Application.ExecuteAsync(plan, [], TestContext.Current.CancellationToken);
        Assert.Equal(RepairPreflightState.Blocked, outcome.Preflight.State);
        Assert.NotEqual(RepairApplicationState.Applied, outcome.Application.State);
        foreach (var file in before) Assert.Equal(file.Value, File.ReadAllBytes(file.Key));
        Assert.Equal(before.Count, Directory.GetFiles(fixture.PathFor(".agents"), "*", SearchOption.AllDirectories).Length);
    }

    [Theory, InlineData(false), InlineData(true), Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
    public async Task CannotAskListsEveryCopyableCandidateAndNoOpNextIsStable(bool automatic)
    {
        using var fixture = new RepairRenameGitWorkspace();
        await fixture.InitializeAsync(new Dictionary<string, string>
        {
            [".agents/guidance/rounding.md"] = Document("Money rounding", "# Rounding\n\nKeep monetary rules.\n"),
            [".agents/guidance/notes.md"] = Document("Notes", "# Notes\n\nRead [Rounding](rounding.md).\n"),
        });
        fixture.Move(".agents/guidance/rounding.md", ".agents/guidance/money-rounding.md");
        fixture.Write(".agents/guidance/alternate-rounding.md", File.ReadAllText(fixture.PathFor(".agents/guidance/money-rounding.md")));
        await OpenForge.Cli.Core.Commands.Index.IndexOperationFactory.Create(renameEvidence: Scaffold(fixture)).ExecuteAsync(new(fixture.Workspace, [], IndexMode.Apply), TestContext.Current.CancellationToken);
        var components = Components(fixture);
        var operation = new RepairOperation(components);
        var before = File.ReadAllText(fixture.PathFor(".agents/guidance/notes.md"));
        var preview = await operation.ExecuteAsync(new(fixture.Workspace, RepairMode.DryRun, automatic, [], false), TestContext.Current.CancellationToken);
        var result = await operation.ExecuteAsync(new(fixture.Workspace, RepairMode.Apply, automatic, [], false), TestContext.Current.CancellationToken);
        var report = RepairReportSelector.Select(result, new(CliDetail.Minimal));
        Assert.Equal(automatic ? CliSemanticStatus.Attention : CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(RepairReportSelector.Select(preview, new(CliDetail.Minimal)).Next?.Command, report.Next?.Command);
        Assert.Equal(before, File.ReadAllText(fixture.PathFor(".agents/guidance/notes.md")));
        var selection = new CliSelection(CliDetail.Standard);
        var selected = RepairPresentation.Rendering.SelectText!(CliReportTrimmer.Trim(
            RepairReportSelector.Select(result, selection), selection, CliCommandShape.ChangeReport));
        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain.WithWidth(60), RepairPresentation.Rendering.DataTextRenderer).Content;
        var commands = text.Split('\n').Where(line => line.Contains("open-forge repair --relink", StringComparison.Ordinal)).ToArray();
        Assert.Equal(2, commands.Length);
        Assert.All(commands, line =>
        {
            Assert.StartsWith("           open-forge repair --relink", line);
            Assert.Contains(" --automatic", line);
        });
        Assert.Contains("To use alternate-rounding.md (same content):", text);
        Assert.Contains("To use money-rounding.md (same content):", text);
        Assert.DoesNotContain("possible target ", text);
        Assert.DoesNotContain("title match:", text);
        if (!automatic)
        {
            Assert.StartsWith("Repair cannot ask for a choice in this session. Nothing was changed.", text);
            Assert.DoesNotContain("  Error  ", text);
            Assert.DoesNotContain("problems remaining", text);
            Assert.Contains(report.Findings, finding => finding.Code == "repair.selection-required");
        }
        Assert.Equal("Choose a target above, then run its command.", report.Next?.Command);
    }

    [Theory, InlineData(false), InlineData(true), Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "Processing")]
    public async Task ProvenRenameCompletesAffectedEntriesInEitherOrder(bool indexFirst)
    {
        using var fixture = new RepairRenameGitWorkspace();
        await fixture.InitializeAsync();
        const string oldPath = ".agents/guidance/cli-design.md";
        const string newPath = ".agents/guidance/command-design.md";
        fixture.Write(".agents/guidance/rename-notes.md", "---\nopen-forge:\n  description: Rename journey\n  tags: [Guidance]\n---\n\n# Notes\n\nRead [Design](cli-design.md).\n");
        var evidence = new RenameEvidenceReader(new PhysicalPathResolver(), new GitRenameEvidenceReader("git", fixture.Configure, timeProvider: new RenameEvidenceClock()).ReadAsync);
        var scaffold = new RoutedMaintenanceRenameEvidenceAdapter(evidence);
        var index = OpenForge.Cli.Core.Commands.Index.IndexOperationFactory.Create(renameEvidence: scaffold);
        await index.ExecuteAsync(new(fixture.Workspace, [], OpenForge.Cli.Core.Commands.Index.Models.Request.IndexMode.Apply), TestContext.Current.CancellationToken);
        fixture.Move(oldPath, newPath);
        if (indexFirst) await index.ExecuteAsync(new(fixture.Workspace, [], OpenForge.Cli.Core.Commands.Index.Models.Request.IndexMode.Apply), TestContext.Current.CancellationToken);
        var repair = new RepairOperation(RepairOperationFactory.CreateDefaultComponents(new(scaffold.ReadEvidenceAsync, scaffold)));
        var preview = await repair.ExecuteAsync(new(fixture.Workspace, RepairMode.DryRun, true, [], false), TestContext.Current.CancellationToken);
        Assert.Single(preview.Plan?.Selection.Selected ?? []);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Contains("cli-design.md", File.ReadAllText(fixture.PathFor(".agents/guidance/rename-notes.md")));
        var applied = await repair.ExecuteAsync(new(fixture.Workspace, RepairMode.Apply, true, [], false), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Contains("[Design](command-design.md)", File.ReadAllText(fixture.PathFor(".agents/guidance/rename-notes.md")));
        Assert.DoesNotContain("(cli-design.md)", File.ReadAllText(fixture.PathFor(".agents/guidance/_guidance.md")));
        Assert.Contains("(command-design.md)", File.ReadAllText(fixture.PathFor(".agents/guidance/_guidance.md")));
        Assert.Equal(1, applied.Counts.Repaired);
        Assert.DoesNotContain(applied.MaintenanceResults.Effects, effect => effect.Path == ".agents/guidance/rename-notes.md");
        Assert.All(applied.Verification.FileReceipts, receipt => Assert.Equal(OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts.FilesystemVerificationState.Verified, receipt.VerificationState));
        await AssertHealthyAsync(fixture, Components(fixture));
    }

    [Theory, InlineData(false), InlineData(true), Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "Processing")]
    public async Task CanonicalFolderRenameUsesRealEvidenceInBothCommands(bool indexFirst)
    {
        using var fixture = new RepairRenameGitWorkspace();
        await fixture.InitializeAsync(new Dictionary<string, string>
        {
            [".agents/guidance/billing/_billing.md"] = Document("Billing rules", "# Billing\n\n## Axioms\n\n- Keep exact rounding rules.\n\n## Entries\n\n## Authored\n\nKeep this authored tail.\n"),
            [".agents/guidance/billing/rounding.md"] = Document("Money rounding policy", "# Rounding\n\nUse integer minor units.\n"),
            [".agents/guidance/notes.md"] = Document("Billing journey notes", "# Notes\n\nRead [Billing](billing/_billing.md) and [Rounding](billing/rounding.md).\n"),
        });
        var scaffold = Scaffold(fixture);
        var index = OpenForge.Cli.Core.Commands.Index.IndexOperationFactory.Create(renameEvidence: scaffold);
        await index.ExecuteAsync(new(fixture.Workspace, [], IndexMode.Apply), TestContext.Current.CancellationToken);
        await fixture.RecordTreeAsync();
        Directory.Move(fixture.PathFor(".agents/guidance/billing"), fixture.PathFor(".agents/guidance/invoices"));
        if (indexFirst)
        {
            var indexed = await index.ExecuteAsync(new(fixture.Workspace, [], IndexMode.Apply), TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Complete, indexed.Status);
            Assert.False(File.Exists(fixture.PathFor(".agents/guidance/invoices/_billing.md")));
        }
        var result = await new RepairOperation(Components(fixture)).ExecuteAsync(new(fixture.Workspace, RepairMode.Apply, true, [], false), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(2, result.Counts.Repaired);
        foreach (var detail in Enum.GetValues<CliDetail>())
        {
            var rows = RepairReportSelector.Select(result, new(detail)).Data.TextRows;
            var prefix = detail >= CliDetail.Full ? ".agents/guidance/" : "";
            Assert.Contains($"{prefix}billing/_billing.md was moved to {prefix}invoices/_invoices.md", rows[1].Wording);
            Assert.Contains($"{prefix}billing/rounding.md was moved to {prefix}invoices/rounding.md", rows[3].Wording);
            if (!indexFirst)
            {
                Assert.Contains(rows, row => row.Path == ".agents/guidance/invoices/_invoices.md" && row.Wording == "renamed from _billing.md, your rules were kept");
                Assert.DoesNotContain(rows, row => row.Path == ".agents/guidance/invoices/_invoices.md" && row.Wording == "Entries updated");
            }
        }
        Assert.Contains("[Billing](invoices/_invoices.md)", File.ReadAllText(fixture.PathFor(".agents/guidance/notes.md")));
        Assert.Contains("[Rounding](invoices/rounding.md)", File.ReadAllText(fixture.PathFor(".agents/guidance/notes.md")));
        var canonical = File.ReadAllText(fixture.PathFor(".agents/guidance/invoices/_invoices.md"));
        Assert.Contains("- Keep exact rounding rules.", canonical);
        Assert.EndsWith("Keep this authored tail.\n", canonical);
        Assert.False(File.Exists(fixture.PathFor(".agents/guidance/invoices/_billing.md")));
        await AssertHealthyAsync(fixture, Components(fixture));
    }

    [Fact, Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "Processing")]
    public async Task MoveComposesOutgoingAndIncomingLinksWithMissingChainAndMetadata()
    {
        using var fixture = new RepairRenameGitWorkspace();
        await fixture.InitializeAsync(new Dictionary<string, string>
        {
            [".agents/guidance/billing/rounding.md"] = Document("Money rounding policy", "# Rounding\n\nRead [Rule](rule.md#rule).\n\n" + string.Concat(Enumerable.Repeat("Keep monetary values in minor units.\n", 12))),
            [".agents/guidance/billing/rule.md"] = Document("Rounding reference rules", "# Rule\n\nKeep this reference.\n"),
            [".agents/guidance/notes.md"] = "# Notes\n\nRead [Rounding](billing/rounding.md#rounding).\nKeep my authored bytes.\n",
        });
        fixture.Move(".agents/guidance/billing/rounding.md", ".agents/guidance/finance/deep/money-rounding.md");
        var components = Components(fixture);
        var repair = new RepairOperation(components);
        var preview = await repair.ExecuteAsync(new(fixture.Workspace, RepairMode.DryRun, true, [], false), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(2, preview.Counts.SelectedFindings);
        var plan = Assert.IsType<OpenForge.Cli.Core.Commands.Repair.Models.Planning.RepairPlan>(preview.Plan);
        Assert.Equal(plan.FileChanges.Count, plan.FileChanges.Select(change => change.LogicalPath).Distinct().Count());
        var maintenance = Assert.IsType<OpenForge.Cli.Core.Commands.Repair.Models.Planning.RepairMaintenancePlan>(plan.Maintenance);
        Assert.Equal(maintenance.Effects.RecoveryTargets.Length, maintenance.Effects.RecoveryTargets.Select(target => target.Change.LogicalPath).Distinct().Count());
        Assert.False(File.Exists(fixture.PathFor(".agents/guidance/finance/_finance.md")));
        var result = await repair.ExecuteAsync(new(fixture.Workspace, RepairMode.Apply, true, [], false), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(2, result.Counts.Repaired);
        Assert.Contains("[Rule](../../billing/rule.md#rule)", File.ReadAllText(fixture.PathFor(".agents/guidance/finance/deep/money-rounding.md")));
        Assert.EndsWith("Keep my authored bytes.\n", File.ReadAllText(fixture.PathFor(".agents/guidance/notes.md")));
        Assert.Contains("[Rounding](finance/deep/money-rounding.md#rounding)", File.ReadAllText(fixture.PathFor(".agents/guidance/notes.md")));
        Assert.Single(result.MaintenanceResults.Metadata);
        Assert.True(result.MaintenanceResults.Entrypoints.Count >= 3);
        foreach (var detail in Enum.GetValues<CliDetail>())
        {
            var rows = RepairReportSelector.Select(result, new(detail)).Data.TextRows;
            Assert.All(result.MaintenanceResults.Entrypoints, entrypoint =>
            {
                var row = Assert.Single(rows, row => row.Path == entrypoint.Path);
                Assert.Equal("entrypoint created", row.Wording);
            });
        }
        var report = RepairReportSelector.Select(result, new(CliDetail.Full));
        Assert.Equal(FileExpectation.Hash(File.ReadAllBytes(fixture.PathFor(".agents/guidance/notes.md"))), report.Data.Repairs.Single(row => row.Path.EndsWith("notes.md", StringComparison.Ordinal)).After);
        Assert.Contains(report.Effects, effect => effect.Kind == CliEffectKind.File && effect.Action == CliEffectAction.Created);
        await AssertHealthyAsync(fixture, components);
    }

    [Theory, InlineData("edited"), InlineData("two"), InlineData("weak"), InlineData("deleted"), InlineData("unborn"), InlineData("no-executable"), InlineData("timeout"), Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "Processing")]
    public async Task ProofAndResidualLanesFollowCurrentIdentity(string scenario)
    {
        using var fixture = new RepairRenameGitWorkspace();
        await fixture.InitializeAsync(new Dictionary<string, string>
        {
            [".agents/guidance/rounding.md"] = Document("Money rounding policy", "# Rounding\n\n" + string.Concat(Enumerable.Repeat("Use integer minor units for calculations.\n", 30))),
            [".agents/guidance/notes.md"] = Document("Rename maintainer notes", "# Notes\n\nRead [Rounding](rounding.md).\n"),
        }, unborn: scenario == "unborn");
        var scaffold = scenario switch
        {
            "no-executable" => new RoutedMaintenanceRenameEvidenceAdapter(new(new PhysicalPathResolver(), new GitRenameEvidenceReader("open-forge-no-such-git", fixture.Configure, timeProvider: new RenameEvidenceClock()).ReadAsync)),
            "timeout" => new RoutedMaintenanceRenameEvidenceAdapter(new(new PhysicalPathResolver(), (_, _) => ValueTask.FromResult(OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git.RenameGitObservation.Unavailable(OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git.RenameGitAvailability.TimedOut)))),
            _ => Scaffold(fixture),
        };
        await OpenForge.Cli.Core.Commands.Index.IndexOperationFactory.Create(renameEvidence: scaffold).ExecuteAsync(new(fixture.Workspace, [], IndexMode.Apply), TestContext.Current.CancellationToken);
        const string newPath = ".agents/guidance/money-rounding.md";
        fixture.Move(".agents/guidance/rounding.md", newPath);
        if (scenario == "edited") fixture.Write(newPath, File.ReadAllText(fixture.PathFor(newPath)) + "One edited line.\n");
        if (scenario == "two") fixture.Write(".agents/guidance/other-rounding.md", File.ReadAllText(fixture.PathFor(newPath)));
        if (scenario == "weak") fixture.Write(newPath, Document("Unrelated arithmetic", "# Rounding\n\nDifferent content.\n"));
        if (scenario == "deleted") File.Delete(fixture.PathFor(newPath));
        var components = RepairOperationFactory.CreateDefaultComponents(new(scaffold.ReadEvidenceAsync, scaffold));
        var result = await new RepairOperation(components).ExecuteAsync(new(fixture.Workspace, RepairMode.Apply, true, [], false), TestContext.Current.CancellationToken);
        if (scenario is "edited" or "unborn" or "no-executable")
        {
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            var rename = Assert.Single(result.MaintenanceResults.Renames);
            Assert.Equal(scenario == "edited" ? "git-rename" : "recorded-metadata", rename.Evidence);
            if (scenario == "edited") Assert.InRange(rename.Similarity ?? 0, 90, 100);
            Assert.Contains("[Rounding](money-rounding.md)", File.ReadAllText(fixture.PathFor(".agents/guidance/notes.md")));
            await AssertHealthyAsync(fixture, components);
        }
        else if (scenario == "timeout")
        {
            Assert.Equal(CliSemanticStatus.Attention, result.Status);
            Assert.Empty(result.Selection?.Selected ?? []);
            Assert.Empty(result.MaintenanceResults.Renames);
            Assert.Contains("[Rounding](rounding.md)", File.ReadAllText(fixture.PathFor(".agents/guidance/notes.md")));
            var report = RepairReportSelector.Select(result, new(CliDetail.Standard));
            Assert.Contains(report.Limitations, limitation => limitation.What.Contains("Git did not answer in time", StringComparison.Ordinal));
            Assert.Equal("open-forge repair", report.Next?.Command);
        }
        else
        {
            Assert.Equal(CliSemanticStatus.Attention, result.Status);
            Assert.Empty(result.Selection?.Selected ?? []);
            var remaining = Assert.Single(result.Selection?.Unselected ?? []);
            Assert.Equal(scenario != "deleted", remaining.IsGuided);
            var report = RepairReportSelector.Select(result, new(CliDetail.Standard));
            Assert.Equal(scenario == "deleted" ? "manual" : "guided", Assert.Single(report.Data.Remaining).Kind);
            Assert.Contains(scenario switch { "two" => "More than one file looks like its new location.", "weak" => "Its new location could not be proven.", _ => "no replacement was found." }, Assert.Single(report.Findings).Message);
            if (report.Next?.Command == "open-forge index")
            {
                await OpenForge.Cli.Core.Commands.Index.IndexOperationFactory.Create(renameEvidence: scaffold).ExecuteAsync(new(fixture.Workspace, [], IndexMode.Apply), TestContext.Current.CancellationToken);
                var continued = await new RepairOperation(components).ExecuteAsync(new(fixture.Workspace, RepairMode.Apply, true, [], false), TestContext.Current.CancellationToken);
                report = RepairReportSelector.Select(continued, new(CliDetail.Standard));
            }
            Assert.Equal(CliNextActionKind.Sentence, report.Next?.Kind);
            if (scenario != "deleted") Assert.Equal("Choose a target above, then run its command.", report.Next?.Command);
            fixture.Write(".agents/guidance/notes.md", Document("Rename maintainer notes", scenario == "deleted" ? "# Notes\n\nThe obsolete link was removed.\n" : "# Notes\n\nRead [Rounding](money-rounding.md).\n"));
            await OpenForge.Cli.Core.Commands.Index.IndexOperationFactory.Create(renameEvidence: scaffold).ExecuteAsync(new(fixture.Workspace, [], IndexMode.Apply), TestContext.Current.CancellationToken);
            await AssertHealthyAsync(fixture, components);
        }
    }

    [Theory, InlineData("source"), InlineData("candidate"), InlineData("disappeared"), InlineData("second-candidate"), InlineData("head"), InlineData("settings"), InlineData("lost-proof"), InlineData("entries"), Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "Processing")]
    public async Task CombinedPlanRechecksDriftUnderLeaseBeforeAnyEffect(string drift)
    {
        using var fixture = new RepairRenameGitWorkspace();
        await fixture.InitializeAsync();
        fixture.Write(".agents/guidance/notes.md", Document("Rename maintainer notes", "# Notes\n\nRead [Design](cli-design.md).\n"));
        fixture.Move(".agents/guidance/cli-design.md", ".agents/guidance/command-design.md");
        var components = Components(fixture);
        var preview = await new RepairOperation(components).ExecuteAsync(new(fixture.Workspace, RepairMode.DryRun, true, [], false), TestContext.Current.CancellationToken);
        var original = preview.Plan!;
        var plan = new OpenForge.Cli.Core.Commands.Repair.Models.Planning.RepairPlan(new(fixture.Workspace, RepairMode.Apply, true, [], false), original.Selection, original.Steps, original.Conflicts, original.LibrarySteps)
        { Maintenance = original.Maintenance, RenameEvidence = original.RenameEvidence };
        var before = File.ReadAllText(fixture.PathFor(".agents/guidance/notes.md"));
        switch (drift)
        {
            case "source": fixture.Write(".agents/guidance/notes.md", before + "Authored edit.\n"); break;
            case "candidate": fixture.Write(".agents/guidance/command-design.md", "# Changed\n"); break;
            case "disappeared": File.Delete(fixture.PathFor(".agents/guidance/command-design.md")); break;
            case "second-candidate": fixture.Write(".agents/guidance/second-design.md", File.ReadAllText(fixture.PathFor(".agents/guidance/command-design.md"))); break;
            case "head": await fixture.GitAsync("update-ref", "HEAD", (await fixture.GitAsync("rev-parse", "HEAD^")).Trim()); break;
            case "lost-proof": await fixture.GitAsync("update-ref", "-d", "HEAD"); break;
            case "settings": fixture.Write(".agents/open-forge.json", "{\"schemaVersion\":1,\"frontmatter\":\"root\"}"); break;
            case "entries": fixture.Write(".agents/guidance/_guidance.md", "# Guidance\n\n## Entries\n\n## Entries\n"); break;
        }
        var outcome = await components.Application.ExecuteAsync(plan, [], TestContext.Current.CancellationToken);
        Assert.NotEqual(RepairApplicationState.Applied, outcome.Application.State);
        Assert.Equal(RepairPreflightState.Blocked, outcome.Preflight.State);
        Assert.Equal(before + (drift == "source" ? "Authored edit.\n" : ""), File.ReadAllText(fixture.PathFor(".agents/guidance/notes.md")));
        Assert.Empty(Directory.GetFiles(fixture.Root, "*.tmp", SearchOption.AllDirectories));
    }

    [Fact, Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "Processing")]
    public async Task InteractiveProofShowsCombinedPlanAndRequiresFinalConfirmationWithoutTargetChoice()
    {
        using var fixture = new RepairRenameGitWorkspace();
        await fixture.InitializeAsync();
        fixture.Write(".agents/guidance/notes.md", "# Notes\n\nRead [Design](cli-design.md).\n");
        fixture.Move(".agents/guidance/cli-design.md", ".agents/guidance/command-design.md");
        var confirmations = new List<RepairConfirmationKind>();
        var components = Components(fixture) with
        {
            Interaction = new(
            (_, _, _) => throw new InvalidOperationException("A proven rename must not ask for a target."),
            (_, _, _) => throw new InvalidOperationException(),
            (preview, question, _, _) =>
            {
                confirmations.Add(question.Kind);
                Assert.NotEmpty(preview.MaintenanceResults.Navigation);
                Assert.Single(preview.MaintenanceResults.Metadata);
                return ValueTask.FromResult(CliPromptReply<bool>.Answered(true));
            })
        };
        var result = await new RepairOperation(components).ExecuteAsync(new(fixture.Workspace, RepairMode.Apply, false, [], true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Contains(RepairConfirmationKind.Final, confirmations);
    }

    [Fact, Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "Processing")]
    public async Task CompletionUsesFreshResidualLocationAndLinkOnlyCounts()
    {
        using var fixture = new RepairRenameGitWorkspace();
        await fixture.InitializeAsync();
        fixture.Write(".agents/guidance/notes.md", "# Notes\n\nRead [Design](cli-design.md).\nRead [Deleted](removed-zebra.md).\n");
        fixture.Move(".agents/guidance/cli-design.md", ".agents/guidance/command-design.md");
        var result = await new RepairOperation(Components(fixture)).ExecuteAsync(new(fixture.Workspace, RepairMode.Apply, true, [], false), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Equal(1, result.Counts.Repaired);
        Assert.Equal(1, result.Counts.Manual);
        var text = File.ReadAllText(fixture.PathFor(".agents/guidance/notes.md"));
        var line = text[..text.IndexOf("removed-zebra.md", StringComparison.Ordinal)].Count(character => character == '\n') + 1;
        var report = RepairReportSelector.Select(result, new(CliDetail.Minimal));
        Assert.Equal(line, Assert.Single(report.Data.Remaining).Location?.Line);
        Assert.Contains($"notes.md at line {line}, column ", report.Next?.Command);
    }

    [Theory, InlineData("kept"), InlineData("canonical"), InlineData("lost"), Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "Processing")]
    public async Task RenameRelinkRequiresAResolvingFragment(string fragmentCase)
    {
        using var fixture = new RepairRenameGitWorkspace();
        await fixture.InitializeAsync(new Dictionary<string, string>
        {
            [".agents/guidance/rounding.md"] = Document("Money rounding policy", "# Rounding\n\n" + string.Concat(Enumerable.Repeat("Keep monetary values in minor units.\n", 30))),
            [".agents/guidance/notes.md"] = Document("Maintainer fragment journey", $"# Notes\n\nRead [Rounding](rounding.md#{(fragmentCase == "canonical" ? "Rounding" : "rounding")}).\n"),
        });
        fixture.Move(".agents/guidance/rounding.md", ".agents/guidance/money-rounding.md");
        if (fragmentCase == "lost") fixture.Write(".agents/guidance/money-rounding.md", File.ReadAllText(fixture.PathFor(".agents/guidance/money-rounding.md")).Replace("# Rounding", "# Mathematics", StringComparison.Ordinal));
        var result = await new RepairOperation(Components(fixture)).ExecuteAsync(new(fixture.Workspace, RepairMode.Apply, true, [], false), TestContext.Current.CancellationToken);
        if (fragmentCase == "lost")
        {
            Assert.Equal(CliSemanticStatus.Attention, result.Status);
            Assert.Empty(result.Selection!.Selected);
            Assert.True(Assert.Single(result.Selection.Unselected).IsGuided);
            Assert.Contains("(rounding.md#rounding)", File.ReadAllText(fixture.PathFor(".agents/guidance/notes.md")));
        }
        else
        {
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Contains("(money-rounding.md#rounding)", File.ReadAllText(fixture.PathFor(".agents/guidance/notes.md")));
            await AssertHealthyAsync(fixture, Components(fixture));
        }
    }

    [Theory, InlineData(false), InlineData(true), Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "Processing")]
    public async Task MalformedMetadataSkipsOnlyIndependentSources(bool required)
    {
        using var fixture = new RepairRenameGitWorkspace();
        await fixture.InitializeAsync();
        fixture.Write(".agents/guidance/notes.md", Document("Rename maintainer notes", "# Notes\n\nRead [Design](cli-design.md).\n"));
        fixture.Write(required ? ".agents/guidance/_guidance.md" : ".agents/guidance/broken.md", "---\ndescription: [unterminated\n---\n\n# Broken\n");
        fixture.Move(".agents/guidance/cli-design.md", ".agents/guidance/command-design.md");
        var result = await new RepairOperation(Components(fixture)).ExecuteAsync(new(fixture.Workspace, RepairMode.Apply, true, [], false), TestContext.Current.CancellationToken);
        if (required)
        {
            Assert.Equal(CliSemanticStatus.Blocked, result.Status);
            Assert.Contains("(cli-design.md)", File.ReadAllText(fixture.PathFor(".agents/guidance/notes.md")));
        }
        else
        {
            Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
            Assert.Equal(1, result.Counts.Repaired);
            var report = RepairReportSelector.Select(result, new(CliDetail.Standard));
            Assert.StartsWith("Repaired 1 link.", report.Headline.Sentence);
            Assert.DoesNotContain("Nothing was changed", report.Headline.Sentence);
            Assert.Single(report.Findings, finding => finding.Code == "index.metadata-skipped");
            Assert.Equal("---\ndescription: [unterminated\n---\n\n# Broken\n", File.ReadAllText(fixture.PathFor(".agents/guidance/broken.md")));
        }
    }

    [Fact(DisplayName = "Incomplete Repair preview applies its safe plan before malformed-file advice")]
    [Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
    public async Task IncompletePreviewKeepsSafePlanPrecedence()
    {
        using var fixture = new RepairRenameGitWorkspace();
        await fixture.InitializeAsync();
        fixture.Write(".agents/guidance/notes.md", Document("Rename maintainer notes", "# Notes\n\nRead [Design](cli-design.md).\n"));
        fixture.Write(".agents/guidance/broken.md", "---\ndescription: [unterminated\n---\n\n# Broken\n");
        fixture.Move(".agents/guidance/cli-design.md", ".agents/guidance/command-design.md");
        var operation = new RepairOperation(Components(fixture));
        var before = File.ReadAllBytes(fixture.PathFor(".agents/guidance/notes.md"));
        var preview = await operation.ExecuteAsync(new(fixture.Workspace, RepairMode.DryRun, true, [], false), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Incomplete, preview.Status);
        Assert.Equal(before, File.ReadAllBytes(fixture.PathFor(".agents/guidance/notes.md")));
        var report = RepairReportSelector.Select(preview, new(CliDetail.Standard));
        Assert.StartsWith("Would repair 1 link.", report.Headline.Sentence);
        Assert.Equal("open-forge repair --automatic", report.Next?.Command);
        var applied = await operation.ExecuteAsync(new(fixture.Workspace, RepairMode.Apply, true, [], false), TestContext.Current.CancellationToken);
        Assert.Contains("(command-design.md)", File.ReadAllText(fixture.PathFor(".agents/guidance/notes.md")));
        var next = RepairReportSelector.Select(applied, new(CliDetail.Standard)).Next?.Command;
        Assert.StartsWith("Fix the frontmatter", next);
        Assert.Contains("--workspace", next);
        Assert.Contains(fixture.Root, next);
    }

    [Fact(DisplayName = "Git timeout after Index is reported as uncertainty with the same Repair retry")]
    [Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
    public async Task GitTimeoutNeverBecomesManualDeletionAdvice()
    {
        using var fixture = new RepairRenameGitWorkspace();
        await fixture.InitializeAsync();
        fixture.Write(".agents/guidance/notes.md", Document("Rename maintainer notes", "# Notes\n\nRead [Design](cli-design.md).\n"));
        fixture.Move(".agents/guidance/cli-design.md", ".agents/guidance/command-design.md");
        await OpenForge.Cli.Core.Commands.Index.IndexOperationFactory.Create().ExecuteAsync(new(fixture.Workspace, [], IndexMode.Apply), TestContext.Current.CancellationToken);
        var scaffold = new RoutedMaintenanceRenameEvidenceAdapter(new(new PhysicalPathResolver(), (_, _) => ValueTask.FromResult(
            OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git.RenameGitObservation.Unavailable(
                OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git.RenameGitAvailability.TimedOut))));
        var operation = new RepairOperation(RepairOperationFactory.CreateDefaultComponents(new(scaffold.ReadEvidenceAsync, scaffold)));
        var result = await operation.ExecuteAsync(new(fixture.Workspace, RepairMode.DryRun, true, [], false), TestContext.Current.CancellationToken);
        var report = RepairReportSelector.Select(result, new(CliDetail.Standard));
        Assert.Contains(report.Limitations, limitation => limitation.What.Contains("Git did not answer in time", StringComparison.Ordinal));
        Assert.Equal("open-forge repair", report.Next?.Command);
        var retry = $"open-forge repair --automatic --dry-run --workspace '{fixture.Root}' --format json --detail full";
        var replay = new CliRequestReplay("open-forge repair", "open-forge repair", retry, retry, retry)
        {
            WorkspaceSpecified = true,
            ApplyCommand = $"open-forge repair --automatic --workspace '{fixture.Root}' --format json --detail full",
        };
        Assert.Equal(retry, CliNextActionCompletion.Complete(report, replay).Next?.Command);
        Assert.DoesNotContain(report.Findings.SelectMany(finding => finding.Actions), action => action.Command.StartsWith("Update or remove this link", StringComparison.Ordinal));
    }

    [Fact, Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "Processing")]
    public async Task CombinedPlanHonorsWorkspaceLockBeforeRecovery()
    {
        using var fixture = new RepairRenameGitWorkspace();
        await fixture.InitializeAsync();
        fixture.Write(".agents/guidance/notes.md", "# Notes\n\nRead [Design](cli-design.md).\n");
        fixture.Move(".agents/guidance/cli-design.md", ".agents/guidance/command-design.md");
        var acquired = await WorkspaceLockManager.CreateForCurrentUser().AcquireAsync(new(fixture.Workspace, "test", Guid.NewGuid()), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        var result = await new RepairOperation(Components(fixture)).ExecuteAsync(new(fixture.Workspace, RepairMode.Apply, true, [], false), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == RepairFindingCode.WorkspaceLockUnavailable);
        Assert.Contains("(cli-design.md)", File.ReadAllText(fixture.PathFor(".agents/guidance/notes.md")));
    }

    [Fact, Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "Processing")]
    public async Task InterruptedCombinedApplicationRetainsVerifiedEffectsAndOneRecoveryEntryPerFile()
    {
        using var fixture = new RepairRenameGitWorkspace();
        await fixture.InitializeAsync();
        fixture.Write(".agents/guidance/notes.md", "# Notes\n\nRead [Design](cli-design.md).\n");
        fixture.Move(".agents/guidance/cli-design.md", ".agents/guidance/command-design.md");
        var components = Components(fixture);
        var preview = await new RepairOperation(components).ExecuteAsync(new(fixture.Workspace, RepairMode.DryRun, true, [], false), TestContext.Current.CancellationToken);
        var original = preview.Plan!;
        var plan = new OpenForge.Cli.Core.Commands.Repair.Models.Planning.RepairPlan(new(fixture.Workspace, RepairMode.Apply, true, [], false), original.Selection, original.Steps, original.Conflicts, original.LibrarySteps)
        { Maintenance = original.Maintenance, RenameEvidence = original.RenameEvidence };
        var operationId = Guid.NewGuid();
        var acquired = await WorkspaceLockManager.CreateForCurrentUser().AcquireAsync(new(fixture.Workspace, RepairDefinitions.CommandIdentity, operationId), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        var prepared = await RepairRecoveryLifecycle.PrepareAsync(plan, operationId, TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        try
        {
            var validator = new FileExpectationValidator(new PhysicalPathResolver());
            var revalidator = new MutationRevalidator(validator);
            var validation = await revalidator.ValidateAsync(lease, plan.FileChanges, TestContext.Current.CancellationToken);
            var index = Enumerable.Range(0, plan.FileChanges.Count).Single(index => plan.FileChanges[index].LogicalPath == fixture.PathFor(".agents/guidance/notes.md"));
            var receipt = await new FileChangeApplier(revalidator, validator).ApplyAsync(lease, plan.FileChanges[index], validation.Checks[index], preparation, TestContext.Current.CancellationToken);
            Assert.Equal(FilesystemVerificationState.Verified, receipt.VerificationState);
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var outcome = await new RepairApplicationCompletion(new RepairPostVerifier(components.DiagnosisReader, components.CatalogueReader))
                .CompleteAsync(new(plan, lease, preparation, [receipt]), cancellation.Token);
            var result = RepairResultBuilder.Build(new()
            {
                Request = plan.Request,
                Plan = plan,
                Diagnosis = preview.Diagnosis,
                InitialFindings = preview.Findings,
                Findings = outcome.Findings,
                Preflight = outcome.Preflight,
                Application = outcome.Application,
                Verification = outcome.Verification,
                Recovery = outcome.Recovery,
                PostDiagnosis = outcome.PostDiagnosis,
            });
            Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
            Assert.Equal(1, result.Counts.Repaired);
            Assert.Single(result.MaintenanceResults.Renames);
            Assert.Equal(RepairRecoveryState.Retained, result.Recovery.State);
            Assert.True(File.Exists(preparation.BundlePath));
            Assert.Equal(plan.Maintenance!.Effects.RecoveryTargets.Length, plan.Maintenance.Effects.RecoveryTargets.Select(target => target.Change.LogicalPath).Distinct().Count());
            Assert.Contains("[Design](command-design.md)", File.ReadAllText(fixture.PathFor(".agents/guidance/notes.md")));
        }
        finally { File.Delete(preparation.BundlePath); }
    }

    [Theory, InlineData(false), InlineData(true), Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "Processing")]
    public async Task PostVerificationRechecksMaintenanceBytesAndDeletedEntrypoints(bool recreatedEntrypoint)
    {
        using var fixture = new RepairRenameGitWorkspace();
        await fixture.InitializeAsync(recreatedEntrypoint ? new Dictionary<string, string>
        {
            [".agents/guidance/billing/_billing.md"] = Document("Billing rules", "# Billing\n\n## Axioms\n\n- Keep exact monetary rules.\n\n## Entries\n"),
            [".agents/guidance/notes.md"] = Document("Canonical rename notes", "# Notes\n\nRead [Billing](billing/_billing.md).\n"),
        } : null);
        if (recreatedEntrypoint) fixture.Move(".agents/guidance/billing/_billing.md", ".agents/guidance/invoices/_billing.md");
        else
        {
            fixture.Write(".agents/guidance/notes.md", "# Notes\n\nRead [Design](cli-design.md).\n");
            fixture.Move(".agents/guidance/cli-design.md", ".agents/guidance/command-design.md");
        }
        var components = Components(fixture);
        var preview = await new RepairOperation(components).ExecuteAsync(new(fixture.Workspace, RepairMode.DryRun, true, [], false), TestContext.Current.CancellationToken);
        var plan = preview.Plan!;
        var operationId = Guid.NewGuid();
        var acquired = await WorkspaceLockManager.CreateForCurrentUser().AcquireAsync(new(fixture.Workspace, RepairDefinitions.CommandIdentity, operationId), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        var prepared = await RepairRecoveryLifecycle.PrepareAsync(plan, operationId, TestContext.Current.CancellationToken);
        var preparation = Assert.IsType<RecoveryBundlePreparation>(prepared.Preparation);
        try
        {
            var validator = new FileExpectationValidator(new PhysicalPathResolver());
            var revalidator = new MutationRevalidator(validator);
            var validation = await revalidator.ValidateAsync(lease, plan.FileChanges, TestContext.Current.CancellationToken);
            var receipts = new List<FileChangeReceipt>();
            for (var index = 0; index < plan.FileChanges.Count; index++)
            {
                var change = plan.FileChanges[index];
                receipts.Add(await new FileChangeApplier(revalidator, validator).ApplyAsync(lease, change, validation.Checks[index], change.Kind == PlannedFileChangeKind.Create ? null : preparation, TestContext.Current.CancellationToken));
            }
            Assert.All(receipts, receipt => Assert.Equal(FilesystemVerificationState.Verified, receipt.VerificationState));
            if (recreatedEntrypoint) fixture.Write(".agents/guidance/invoices/_billing.md", "# Recreated externally\n");
            else File.AppendAllText(fixture.PathFor(".agents/guidance/_guidance.md"), "\n## Authored tail\n\nAn external edit after the effect.\n");
            var verification = await new RepairPostVerifier(components.DiagnosisReader, components.CatalogueReader).VerifyAsync(plan, receipts, TestContext.Current.CancellationToken);
            Assert.Equal(RepairVerificationState.Failed, verification.Verification.ResultingBytes);
            Assert.Contains(verification.Findings, finding => finding.Code == RepairFindingCode.VerificationFailed);
        }
        finally { File.Delete(preparation.BundlePath); }
    }

    [Fact, Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "Processing")]
    public async Task ManagedEntrypointRowsStillRecordUnmanagedChildIdentity()
    {
        using var fixture = new RepairRenameGitWorkspace();
        await fixture.InitializeAsync(new Dictionary<string, string>
        {
            [".agents/guidance/rounding.md"] = Document("Money rounding policy", "# Rounding\n\nKeep monetary rules.\n"),
        });
        await OpenForge.Cli.Core.Commands.Index.IndexOperationFactory.Create().ExecuteAsync(new(fixture.Workspace, [], IndexMode.Apply), TestContext.Current.CancellationToken);
        fixture.Write(".agents/open-forge.lock.json", "{\"schemaVersion\":1,\"framework\":{\"source\":{\"id\":\"open-forge\",\"version\":null},\"paths\":[\".agents/guidance/_guidance.md\"],\"regions\":[]},\"extensions\":[],\"libraries\":[]}");
        fixture.Move(".agents/guidance/rounding.md", ".agents/guidance/money-rounding.md");
        var adapter = new RoutedMaintenanceRenameEvidenceAdapter(new(new PhysicalPathResolver()));
        var evidence = await adapter.ReadEvidenceAsync(fixture.Workspace, TestContext.Current.CancellationToken);
        Assert.Equal(OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations.RenameEvidenceBasis.RecordedMetadata,
            evidence.Find(".agents/guidance/rounding.md")?.Proven?.Basis);
        Assert.DoesNotContain(evidence.Facts.Candidates, candidate => candidate.Path == ".agents/guidance/_guidance.md");
    }

    private static string Document(string description, string body) => $"---\nopen-forge:\n  description: {description}\n  tags: [Guidance]\n---\n\n{body}";
    private static RoutedMaintenanceRenameEvidenceAdapter Scaffold(RepairRenameGitWorkspace fixture)
        => new(new RenameEvidenceReader(new PhysicalPathResolver(), new GitRenameEvidenceReader("git", fixture.Configure, timeProvider: new RenameEvidenceClock()).ReadAsync));
    private static RepairOperationComponents Components(RepairRenameGitWorkspace fixture)
    {
        var scaffold = Scaffold(fixture);
        return RepairOperationFactory.CreateDefaultComponents(new(scaffold.ReadEvidenceAsync, scaffold));
    }
    private static async Task AssertHealthyAsync(RepairRenameGitWorkspace fixture, RepairOperationComponents components)
    {
        var diagnosis = await components.DiagnosisReader.ReadAsync(new DoctorRequest(fixture.Workspace), TestContext.Current.CancellationToken);
        Assert.True(diagnosis.Result.Status == CliSemanticStatus.Complete, string.Join("\n", diagnosis.Result.Diagnosis.Domains.SelectMany(domain => domain.Findings).Select(finding => $"{finding.Kind}: {finding.Message}")));
    }
}
