using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Doctor.Models.Observation;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Maintenance;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Inline;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;
using OpenForge.Cli.Core.UnitTests.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.TestSupport.Snapshots;
using OpenForge.Cli.Core.Presentation.Doctor.Shared.Wording;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor.Shared.Maintenance;

[Trait("Feature", "doctor-command"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class DoctorMaintenanceGuidanceTests
{
    [Fact(DisplayName = "Doctor leaves links in a user-owned Skill package to ordinary repair proof")]
    public void UserOwnedSkillPackageIsNotProtected()
    {
        var reference = Reference() with { SourcePath = ".agents/skills/checks/SKILL.md" };
        var policy = new RoutedMetadataPolicyContext
        {
            Layout = WorkspaceLayout.Create(
        new CliWorkspace(
            Path.GetFullPath(nameof(DoctorMaintenanceGuidanceTests)),
            Path.GetFullPath(nameof(DoctorMaintenanceGuidanceTests)),
            CliWorkspaceSelectionMethod.ExplicitWorkspace),
        WorkspaceLayout.DefaultMainRoot),
            Settings = WorkspaceSettingsDocument.Empty,
            Ownership = WorkspaceOwnershipDocument.Empty
        };
        Assert.Null(DoctorReferenceProtectionFindingFactory.Read(reference, policy));
    }

    [Theory(DisplayName = "Doctor applies source and original target protection before projecting repair proof")]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void ReferenceProtection(bool skill, bool targetOwned)
    {
        var reference = Reference();
        if (skill) reference = reference with { SourcePath = ".agents/skills/checks/SKILL.md" };
        var path = targetOwned ? RenameEvidenceTestFacts.OldPath : reference.SourcePath;
        var policy = new RoutedMetadataPolicyContext
        {
            Layout = WorkspaceLayout.Create(
        new CliWorkspace(
            Path.GetFullPath(nameof(DoctorMaintenanceGuidanceTests)),
            Path.GetFullPath(nameof(DoctorMaintenanceGuidanceTests)),
            CliWorkspaceSelectionMethod.ExplicitWorkspace),
        WorkspaceLayout.DefaultMainRoot),
            Settings = WorkspaceSettingsDocument.Empty,
            Ownership = WorkspaceOwnershipDocument.Empty with { Framework = new(new("open-forge", null), [path], []) },
        };
        var finding = DoctorReferenceProtectionFindingFactory.Read(reference, policy);
        Assert.NotNull(finding);
        Assert.Equal(DoctorFindingKind.ReferenceTargetMissing, finding.Kind);
        Assert.Equal(DoctorFindingSeverity.Warning, finding.Severity);
        Assert.Equal(DoctorResolutionLane.ManualDecision, finding.Resolution);
        Assert.Equal(path, finding.ProtectedReferencePath);
        Assert.Null(finding.Proposal);
        Assert.Empty(finding.Actions);
        Assert.EndsWith($"through the owner of {path}.", Assert.Single(DoctorWording.Actions(finding)).Command, StringComparison.Ordinal);
        Assert.DoesNotContain("Repair can update", DoctorWording.Message(finding), StringComparison.Ordinal);
        var healthy = reference with { Facts = reference.Facts with { Target = reference.Facts.Target with { Resolution = SourceLinkTargetResolution.Complete } } };
        Assert.Null(DoctorReferenceProtectionFindingFactory.Read(healthy, policy));
    }

    [Theory(DisplayName = "Doctor offers only weak candidates whose authored fragment is actionable")]
    [InlineData("different-title", "GuidedChoice")]
    [InlineData("different%2Dtitle", "GuidedChoice")]
    [InlineData("gone", "ManualDecision")]
    [InlineData("DIFFERENT-TITLE", "ManualDecision")]
    public void WeakCandidateFragments(string fragment, string expected)
    {
        var reference = Reference();
        reference = reference with { Destination = $"old.md#{fragment}", Facts = reference.Facts with { Fragment = fragment } };
        var candidate = LocalReferenceCandidateObservation.Create(reference.SourcePath, reference.Destination,
            new SourceLinkIdentity { Id = "new", Path = RenameEvidenceTestFacts.NewPath },
            [LocalReferenceCandidateBasis.Create(LocalReferenceCandidateBasisKind.Title, "Old", null)]);
        var facts = RenameEvidenceTestFacts.Facts([RenameEvidenceTestFacts.Candidate()], [RenameEvidenceTestFacts.Old(body: null)]);
        var evidence = new RenameEvidenceEvaluator().Evaluate(facts, TestContext.Current.CancellationToken);
        var finding = DoctorRenameFindingFactory.Create(reference, evidence, [candidate]);
        Assert.Equal(Enum.Parse<DoctorResolutionLane>(expected), finding.Resolution);
        Assert.Equal(expected == "GuidedChoice" ? 1 : 0, finding.Candidates?.Items.Count ?? 0);
    }

    [Theory(DisplayName = "Doctor keeps weak rename hints within their own authored occurrence")]
    [InlineData(true, "GuidedChoice")]
    [InlineData(false, "ManualDecision")]
    public void OccurrenceScopedWeakHints(bool hintedOccurrence, string expected)
    {
        var first = Reference();
        var second = first with { SourcePath = ".agents/guidance/other-source.md" };
        var candidate = LocalReferenceCandidateObservation.Create(first.SourcePath, first.Destination,
            new SourceLinkIdentity { Id = "new", Path = RenameEvidenceTestFacts.NewPath },
            [LocalReferenceCandidateBasis.Create(LocalReferenceCandidateBasisKind.Title, "Old", null)]);
        var facts = RenameEvidenceTestFacts.Facts([RenameEvidenceTestFacts.Candidate()], [RenameEvidenceTestFacts.Old(body: null)]);
        var maintenance = new DoctorMaintenanceObservation
        {
            Eligibility = ImmutableDictionary<string, RoutedMetadataEligibility>.Empty,
            IndexablePaths = [],
            ChainGapPaths = [],
            OldNavigationPaths = ImmutableDictionary<string, string>.Empty,
            Renames = new RenameEvidenceEvaluator().Evaluate(facts, TestContext.Current.CancellationToken),
        };
        var view = new LocalReferenceDoctorView(OperationalViewState.Complete, [first, second], LocalReferenceCandidateScanState.Complete, [candidate], []);
        var report = LocalReferenceDoctorInspector.Inspect(view, maintenance);
        var source = hintedOccurrence ? first.SourcePath : second.SourcePath;
        Assert.Equal(Enum.Parse<DoctorResolutionLane>(expected), Assert.Single(report.Findings, finding => finding.Subject.Path == source).Resolution);
    }

    [Fact(DisplayName = "Doctor reports a Git timeout once without inventing manual missing-link advice")]
    public void GitTimeout()
    {
        var facts = RenameEvidenceTestFacts.Facts([RenameEvidenceTestFacts.Candidate()], [RenameEvidenceTestFacts.Old()]) with
        {
            Git = RenameGitObservation.Unavailable(RenameGitAvailability.TimedOut),
        };
        var maintenance = new DoctorMaintenanceObservation
        {
            Eligibility = ImmutableDictionary<string, RoutedMetadataEligibility>.Empty,
            IndexablePaths = [],
            ChainGapPaths = [],
            OldNavigationPaths = ImmutableDictionary<string, string>.Empty,
            Renames = new RenameEvidenceEvaluator().Evaluate(facts, TestContext.Current.CancellationToken),
        };
        var view = new LocalReferenceDoctorView(OperationalViewState.Complete, [Reference()], LocalReferenceCandidateScanState.Complete, [], []);
        var report = LocalReferenceDoctorInspector.Inspect(view, maintenance);
        Assert.Equal(DoctorCoverageState.Incomplete, report.Coverage);
        Assert.Empty(report.Findings);
        Assert.Single(report.Limitations);
        Assert.Equal("Rename evidence did not finish in time, so renamed files could not be proven. Run the command again.",
            DoctorWording.IncompleteLine(report.Domain, report));
    }

    [Theory(DisplayName = "Only missing references inside the generated Entries byte span are suppressed")]
    [InlineData(10, true)]
    [InlineData(9, false)]
    [InlineData(20, false)]
    public void MissingGeneratedOccurrence(long offset, bool suppressed)
    {
        var reference = Reference() with { Location = new(4, 3, offset, 1) };
        var maintenance = new DoctorMaintenanceObservation
        {
            Eligibility = ImmutableDictionary<string, RoutedMetadataEligibility>.Empty,
            IndexablePaths = [],
            ChainGapPaths = [],
            OldNavigationPaths = ImmutableDictionary<string, string>.Empty,
            Renames = null,
            GeneratedEntries = ImmutableDictionary<string, SourceLocation>.Empty.Add(reference.SourcePath, new(4, 3, 10, 10)),
        };
        var view = new LocalReferenceDoctorView(OperationalViewState.Complete, [reference], LocalReferenceCandidateScanState.Complete, [], []);
        Assert.Equal(suppressed ? 0 : 1, LocalReferenceDoctorInspector.Inspect(view, maintenance).Findings.Count);
    }

    [Fact(DisplayName = "Doctor derives a proven successor's destination from a root host occurrence")]
    public void RootHostProposal()
    {
        var evidence = new RenameEvidenceEvaluator().Evaluate(RenameEvidenceTestFacts.Facts([RenameEvidenceTestFacts.Candidate()], [RenameEvidenceTestFacts.Old()]), TestContext.Current.CancellationToken);
        var reference = Reference() with { SourcePath = "AGENTS.md", RoutePath = "AGENTS.md", Destination = RenameEvidenceTestFacts.OldPath };
        var finding = DoctorRenameFindingFactory.Create(reference, evidence.Resolutions[0], evidence);
        Assert.Equal(DoctorResolutionLane.SafeExact, finding.Resolution);
        Assert.Equal(RenameEvidenceTestFacts.NewPath, finding.Proposal?.Reference?.IntendedValue);
    }

    [Theory(DisplayName = "Doctor names the exact Framework or Extension operation that owns managed metadata")]
    [InlineData((int)OwnedPathManager.Framework, "open-forge update")]
    [InlineData((int)OwnedPathManager.Extension, "open-forge extension update example")]
    public void ManagingOperations(int manager, string command)
        => Assert.Equal(command, DoctorMaintenanceReader.ManagingUpdate(new(".agents/guidance/note.md", (OwnedPathManager)manager, "example")).Command);

    [Fact(DisplayName = "Doctor rejects undefined maintenance rename and managing-operation values")]
    public void UndefinedValues()
    {
        var evidence = new RenameEvidenceEvaluator().Evaluate(RenameEvidenceTestFacts.Facts([RenameEvidenceTestFacts.Candidate()], [RenameEvidenceTestFacts.Old()]), TestContext.Current.CancellationToken);
        Assert.Throws<ArgumentOutOfRangeException>(() => DoctorRenameFindingFactory.Create(Reference(),
            evidence.Resolutions[0] with { Resolution = (RenameResolution)int.MaxValue }, evidence));
        var finding = DoctorRenameFindingFactory.Create(Reference(), evidence.Resolutions[0], evidence);
        Assert.Throws<ArgumentOutOfRangeException>(() => DoctorMaintenanceWording.Message(finding,
            new() { Condition = (DoctorMaintenanceCondition)int.MaxValue }));
        Assert.Throws<ArgumentOutOfRangeException>(() => DoctorMaintenanceReader.ManagingUpdate(
            new(".agents/guidance/note.md", (OwnedPathManager)int.MaxValue, "example")));
    }

    [Theory(DisplayName = "Doctor offers rename only before canonical navigation exists and copy-rules afterwards")]
    [InlineData(false)]
    [InlineData(true)]
    public void OldNavigationWording(bool canonicalExists)
    {
        var evidence = new RenameEvidenceEvaluator().Evaluate(RenameEvidenceTestFacts.Facts([], [RenameEvidenceTestFacts.Old()]), TestContext.Current.CancellationToken);
        var finding = DoctorRenameFindingFactory.Create(Reference(), evidence.Resolutions[0], evidence) with
        {
            Subject = DoctorDomainSupport.Subject(DoctorSubjectKind.Route, ".agents/guidance/invoices/_billing.md"),
            Maintenance = new()
            {
                Condition = DoctorMaintenanceCondition.OldNavigation,
                CanonicalName = "_invoices.md",
                ExistingCanonicalPath = canonicalExists ? ".agents/guidance/invoices/_invoices.md" : null,
                IndexCommand = "open-forge index",
            },
        };
        var action = DoctorWording.Actions(finding)[0].Command;
        if (canonicalExists)
            Assert.Equal("Copy any rules you want to keep from .agents/guidance/invoices/_billing.md into .agents/guidance/invoices/_invoices.md, then delete .agents/guidance/invoices/_billing.md and run open-forge index.", action);
        else
            Assert.Equal("Rename .agents/guidance/invoices/_billing.md to _invoices.md to keep its rules, or delete it, then run open-forge index.", action);
        Snapshot(finding, canonicalExists ? "old-navigation-beside-new" : "old-navigation");
    }

    [Fact(DisplayName = "Doctor never calls an incomplete candidate universe a safe exact rename")]
    public void IncompleteUniverse()
    {
        var facts = RenameEvidenceTestFacts.Facts([RenameEvidenceTestFacts.Candidate()], [RenameEvidenceTestFacts.Old()]) with { CandidateUniverseComplete = false };
        var evidence = new RenameEvidenceEvaluator().Evaluate(facts, TestContext.Current.CancellationToken);
        var finding = DoctorRenameFindingFactory.Create(Reference(), evidence.Resolutions[0], evidence);
        Assert.Equal(DoctorResolutionLane.BlockedRepair, finding.Resolution);
        Assert.Null(finding.Proposal);
        Snapshot(finding, "incomplete-universe");
    }

    [Theory(DisplayName = "Doctor maps shared completion eligibility into only the lane that can resolve it")]
    [InlineData("Eligible", "RouteMetadataRequiredMissing", "TargetedOperation", true)]
    [InlineData("Eligible", "RouteUnreachable", "TargetedOperation", true)]
    [InlineData("OutsideRoot", "RouteUnreachable", "ManualDecision", false)]
    [InlineData("ManagedContent", "RouteMetadataRequiredMissing", "TargetedOperation", false)]
    [InlineData("SettingsExcluded", "RouteUnreachable", "ManualDecision", false)]
    [InlineData("PrivateContent", "RouteUnreachable", "ManualDecision", false)]
    [InlineData("UnsafeLeaf", "RouteUnreachable", "ManualDecision", false)]
    [InlineData("Unreadable", "RouteUnreachable", "ManualDecision", false)]
    [InlineData("Eligible", "WorkspaceFrontmatterMalformed", "ManualDecision", false)]
    public void EligibilityLanes(string eligibilityName, string kindName, string laneName, bool indexes)
    {
        const string path = ".agents/guidance/note.md";
        var eligibility = Enum.Parse<RoutedMetadataEligibility>(eligibilityName);
        var kind = Enum.Parse<DoctorFindingKind>(kindName);
        var view = new DoctorMaintenanceObservation
        {
            Eligibility = ImmutableDictionary<string, RoutedMetadataEligibility>.Empty.Add(path, eligibility),
            IndexablePaths = eligibility == RoutedMetadataEligibility.Eligible ? [path] : [],
            ChainGapPaths = [path],
            OldNavigationPaths = ImmutableDictionary<string, string>.Empty,
            Renames = null,
        };
        var original = DoctorDomainSupport.Create(DoctorDomainSupport.Warning(kind, "An observed boundary.", DoctorResolutionLane.ManualDecision),
            DoctorDomainSupport.Subject(DoctorSubjectKind.Path, path),
            DoctorDomainSupport.Provenance(DoctorDomainKind.RoutesMetadataOverwritesGeneratedNavigation, DoctorProvenanceSource.RouteMetadata),
            [new DoctorStateEvidence(DoctorObservedState.Missing)]);
        var finding = DoctorMaintenanceGuidance.Project(original, view, WorkspaceLayout.Create(DoctorOperationTestSupport.Workspace(), WorkspaceLayout.DefaultMainRoot));
        Assert.Equal(Enum.Parse<DoctorResolutionLane>(laneName), finding.Resolution);
        Assert.Equal(indexes, finding.Actions.Any(action => action.Operation == DoctorNextOperation.Index));
        Snapshot(finding, $"{eligibilityName}.{kindName}");
        if (kind == DoctorFindingKind.RouteUnreachable && eligibility == RoutedMetadataEligibility.Eligible)
        {
            var unlisted = DoctorMaintenanceGuidance.Project(original, view with { ChainGapPaths = [] }, WorkspaceLayout.Create(DoctorOperationTestSupport.Workspace(), WorkspaceLayout.DefaultMainRoot));
            Assert.Equal(DoctorResolutionLane.TargetedOperation, unlisted.Resolution);
            Assert.Null(unlisted.Maintenance);
            Assert.Equal("open-forge index", Assert.Single(unlisted.Actions).Command);
        }
    }

    [Fact(DisplayName = "Doctor sends a SKILL.md missing its native metadata to Index")]
    public void SkillMetadataUsesIndex()
    {
        const string path = ".agents/skills/release-notes/SKILL.md";
        var view = new DoctorMaintenanceObservation
        {
            Eligibility = ImmutableDictionary<string, RoutedMetadataEligibility>.Empty.Add(path, RoutedMetadataEligibility.Eligible),
            IndexablePaths = [path],
            ChainGapPaths = [],
            OldNavigationPaths = ImmutableDictionary<string, string>.Empty,
            Renames = null,
        };
        var original = DoctorDomainSupport.Create(DoctorDomainSupport.Warning(DoctorFindingKind.RouteMetadataRequiredMissing, "Metadata is missing.", DoctorResolutionLane.ManualDecision),
            DoctorDomainSupport.Subject(DoctorSubjectKind.Path, path),
            DoctorDomainSupport.Provenance(DoctorDomainKind.RoutesMetadataOverwritesGeneratedNavigation, DoctorProvenanceSource.RouteMetadata),
            [new DoctorStateEvidence(DoctorObservedState.Missing)]);

        var finding = DoctorMaintenanceGuidance.Project(original, view, WorkspaceLayout.Create(DoctorOperationTestSupport.Workspace(), WorkspaceLayout.DefaultMainRoot));

        Assert.Equal(DoctorResolutionLane.TargetedOperation, finding.Resolution);
        Assert.Equal($"{path} needs its Skill name or description. Index can fill them from the folder name and heading and keep your content.", DoctorWording.Message(finding));
        Assert.Equal("open-forge index", Assert.Single(DoctorWording.Actions(finding)).Command);
    }

    [Theory(DisplayName = "Doctor distinguishes zero one and several strong successors and retains exact proposal data")]
    [InlineData(0, "ReferenceTargetMissing", "ManualDecision")]
    [InlineData(1, "ReferenceTargetRenamed", "SafeExact")]
    [InlineData(2, "ReferenceTargetMissing", "GuidedChoice")]
    public void RenameLanes(int count, string kind, string lane)
    {
        var evidence = new RenameEvidenceEvaluator().Evaluate(RenameEvidenceTestFacts.Facts(
            [.. Enumerable.Range(0, count).Select(index => RenameEvidenceTestFacts.Candidate($".agents/guidance/new-{index}.md"))], [RenameEvidenceTestFacts.Old()]), TestContext.Current.CancellationToken);
        var finding = DoctorRenameFindingFactory.Create(Reference(), evidence.Resolutions[0], evidence);
        Assert.Equal(Enum.Parse<DoctorFindingKind>(kind), finding.Kind);
        Assert.Equal(Enum.Parse<DoctorResolutionLane>(lane), finding.Resolution);
        if (count == 1)
        {
            Assert.NotNull(finding.Proposal);
            Assert.Equal("old.md", finding.Proposal.Reference?.ExpectedValue);
            Assert.Equal("new-0.md", finding.Proposal.Reference?.IntendedValue);
            Assert.Equal(DoctorProposalRecoveryKind.RepairReceiptRequired, finding.Proposal.Recovery);
        }
        Snapshot(finding, $"strong-{count}");
    }

    [Theory(DisplayName = "Doctor filters self and neighborhood guesses and keeps independent weak matches guided")]
    [InlineData(false, false, "GuidedChoice")]
    [InlineData(true, false, "ManualDecision")]
    [InlineData(false, true, "ManualDecision")]
    public void WeakLimits(bool self, bool neighborhood, string lane)
    {
        var reference = Reference();
        var candidatePath = self ? reference.SourcePath : RenameEvidenceTestFacts.NewPath;
        var candidate = LocalReferenceCandidateObservation.Create(reference.SourcePath, reference.Destination,
            new SourceLinkIdentity { Id = "new", Path = candidatePath },
            [LocalReferenceCandidateBasis.Create(neighborhood ? LocalReferenceCandidateBasisKind.RouteNeighborhood : LocalReferenceCandidateBasisKind.Title, "Old", null)]);
        var facts = RenameEvidenceTestFacts.Facts([RenameEvidenceTestFacts.Candidate(candidatePath, "# Old\nDifferent content.\n")], [RenameEvidenceTestFacts.Old()])
            with
        { WeakCandidates = [new(RenameEvidenceTestFacts.OldPath, candidate)] };
        var evidence = new RenameEvidenceEvaluator().Evaluate(facts, TestContext.Current.CancellationToken);
        var finding = DoctorRenameFindingFactory.Create(reference, evidence.Resolutions[0], evidence);
        Assert.Equal(Enum.Parse<DoctorResolutionLane>(lane), finding.Resolution);
        Snapshot(finding, $"weak.self-{self}.neighborhood-{neighborhood}");
    }

    private static void Snapshot(DoctorFinding finding, string name)
    {
        finding = DoctorLayoutPresentation.Project(WorkspaceLayout.Create(DoctorOperationTestSupport.Workspace(), WorkspaceLayout.DefaultMainRoot), finding);
        var content = $"{DoctorWording.Message(finding)}\n{finding.Resolution}\n{DoctorWording.Actions(finding)[0].Command}\n";
        content = CommandOutputNormalization.ReplaceDelimitedPath(content, DoctorOperationTestSupport.Workspace().LexicalRoot, "<workspace>");
        CommandOutputSnapshot.MatchSnapshot(content, name);
    }

    [Fact(DisplayName = "Doctor gives a moved source's broken outgoing link an exact relative proposal")]
    public void SourceMoveProposal()
    {
        const string source = ".agents/guidance/invoices/new.md";
        const string target = ".agents/guidance/target.md";
        var candidate = RenameEvidenceTestFacts.Candidate(source);
        var targetCandidate = RenameEvidenceTestFacts.Candidate(target, "# Target\nDifferent target content.\n");
        var evidence = new RenameEvidenceEvaluator().Evaluate(RenameEvidenceTestFacts.Facts([candidate, targetCandidate], [RenameEvidenceTestFacts.Old()]), TestContext.Current.CancellationToken);
        var proof = evidence.Resolutions[0].Proven ?? throw new InvalidOperationException();
        var reference = Reference() with { SourcePath = source, Destination = "target.md" };
        var finding = DoctorRenameFindingFactory.SourceMove(reference, new(proof, null, target, target, null, targetCandidate.Snapshot), evidence);
        Assert.NotNull(finding);
        Assert.Equal(DoctorResolutionLane.SafeExact, finding.Resolution);
        Assert.Equal("../target.md", finding.Proposal?.Reference?.IntendedValue);
        Snapshot(finding, "source-move");
    }

    [Theory(DisplayName = "Doctor prints the exact same-content scored metadata and moved proof forms")]
    [InlineData("near")]
    [InlineData("metadata")]
    [InlineData("move")]
    public void ProofForms(string form)
    {
        var path = form == "move" ? ".agents/guidance/invoices/new.md" : RenameEvidenceTestFacts.NewPath;
        var evidence = new RenameEvidenceEvaluator().Evaluate(RenameEvidenceTestFacts.Facts([RenameEvidenceTestFacts.Candidate(path)], [RenameEvidenceTestFacts.Old()]), TestContext.Current.CancellationToken);
        var resolution = evidence.Resolutions[0];
        var proof = resolution.Proven ?? throw new InvalidOperationException();
        if (form == "near") proof = proof with { Basis = RenameEvidenceBasis.GitRename, Similarity = 90, SameContent = false };
        if (form == "metadata") proof = proof with { Basis = RenameEvidenceBasis.RecordedMetadata, SameContent = false };
        resolution = resolution with { StrongEvidence = [proof] };
        var finding = DoctorRenameFindingFactory.Create(Reference(), resolution, evidence);
        Assert.Equal(DoctorResolutionLane.SafeExact, finding.Resolution);
        Snapshot(finding, form);
    }

    [Theory(DisplayName = "Doctor verifies renamed fragments and retains a verified authored encoding exactly")]
    [InlineData("different%2Dtitle", true)]
    [InlineData("DIFFERENT-TITLE", true)]
    [InlineData("gone", false)]
    public void FragmentVerification(string fragment, bool safe)
    {
        var evidence = new RenameEvidenceEvaluator().Evaluate(RenameEvidenceTestFacts.Facts([RenameEvidenceTestFacts.Candidate()], [RenameEvidenceTestFacts.Old()]), TestContext.Current.CancellationToken);
        var reference = Reference();
        reference = reference with { Destination = $"old.md#{fragment}", Facts = reference.Facts with { Fragment = fragment } };
        var finding = DoctorRenameFindingFactory.Create(reference, evidence.Resolutions[0], evidence);
        Assert.Equal(safe ? DoctorResolutionLane.SafeExact : DoctorResolutionLane.GuidedChoice, finding.Resolution);
        if (safe) Assert.Equal($"new.md#{(fragment == "DIFFERENT-TITLE" ? "different-title" : fragment)}", finding.Proposal?.Reference?.IntendedValue);
        else Assert.Null(finding.Proposal);
    }

    private static LocalReferenceObservation Reference()
        => new()
        {
            SourcePath = ".agents/guidance/source.md",
            RoutePath = ".agents/guidance/source.md",
            Kind = LocalReferenceKind.Link,
            Destination = "old.md",
            Label = MarkdownLinkLabelFact.Supported("Old"),
            Location = new(4, 3, 10, 8),
            DestinationLocation = new(4, 9, 16, 6),
            Facts = new()
            {
                Fragment = null,
                Target = new()
                {
                    Kind = SourceLinkTargetKind.Local,
                    Id = null,
                    Path = RenameEvidenceTestFacts.OldPath,
                    PhysicalPath = null,
                    Layer = null,
                    Resolution = SourceLinkTargetResolution.Missing,
                    Network = null
                },
                Finding = null,
            },
            Fragment = LocalReferenceFragmentObservation.NotRequested(),
            Canonicalizations = [],
        };
}
