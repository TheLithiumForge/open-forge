using System.Text.Json;
using OpenForge.Cli.Composition.Shared.Git;
using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Git;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;
using OpenForge.Cli.Core.Presentation.Doctor.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Doctor;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.IntegrationTests.Commands.Repair;
using OpenForge.Cli.IntegrationTests.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Doctor;

[Trait("Feature", "doctor-command"), Trait("Evidence", "Integration"), Trait("Boundary", "Processing")]
public sealed class DoctorRenameAgreementIntegrationTests
{
    private const string Old = ".agents/guidance/rounding.md";
    private const string New = ".agents/guidance/money-rounding.md";
    private const string Source = ".agents/guidance/source.md";
    private const string Description = "Specific authored rounding rules";
    private const string Entries = "- [Specific authored rounding rules](rounding.md) - #Guidance\n- [Incoming reference](source.md) - #Guidance";

    [Fact(DisplayName = "Doctor and Repair agree on occurrence-specific weak and absent candidates after Index removed old identity")]
    public async Task AlreadyIndexedOccurrenceAgreement()
    {
        using var workspace = new RepairRenameGitWorkspace();
        const string taxes = ".agents/guidance/billing/taxes.md";
        await workspace.InitializeAsync(new Dictionary<string, string>
        {
            [Source] = Note("Incoming reference", "# Source\n\nRead [rounding](billing/rounding.md) and [taxes](billing/taxes.md).\n"),
            [taxes] = Note("Tax rules", "# Taxes\n\nRead [Rounding](rounding.md).\n"),
            [".agents/guidance/billing/new.md"] = Note(Description, "# Rounding\n\nAuthored rules.\n"),
            [".agents/guidance/_guidance.md"] = Root("- [Guidance](billing/_billing.md) - #Guidance\n- [Incoming reference](source.md) - #Guidance"),
            [".agents/guidance/billing/_billing.md"] = Root("- [Specific authored rounding rules](new.md) - #Guidance\n- [Tax rules](taxes.md) - #Guidance"),
        }, unborn: true);
        var before = Snapshot(workspace);
        var paths = new PhysicalPathResolver();
        var reader = new RenameEvidenceReader(paths, new GitRenameEvidenceReader("git", workspace.Configure, timeProvider: new RenameEvidenceClock()).ReadAsync);
        var doctor = await new DoctorDiagnosisReader(RepairOperationalContributorFactory.Create(paths), reader)
            .ReadAsync(new DoctorRequest(workspace.Workspace), TestContext.Current.CancellationToken);
        var repair = await new RepairCatalogueReader(paths, new RoutedMaintenanceRenameEvidenceAdapter(reader).ReadEvidenceAsync)
            .ReadAsync(workspace.Workspace, doctor.Observation.LocalReferences, TestContext.Current.CancellationToken);
        Assert.NotEmpty(Assert.Single(repair.Proposals, input => input.Proposal.SourceCanonicalPath == taxes).Proposal.Candidates?.Items ?? []);
        Assert.Empty(Assert.Single(repair.Proposals, input => input.Proposal.SourceCanonicalPath == Source).Proposal.Candidates?.Items ?? []);
        var findings = doctor.Result.Diagnosis.Domains.SelectMany(domain => domain.Findings).Where(finding => finding.Kind == DoctorFindingKind.ReferenceTargetMissing).ToArray();
        Assert.Equal(DoctorResolutionLane.GuidedChoice, Assert.Single(findings, finding => finding.Subject.Path == taxes).Resolution);
        var noCandidate = Assert.Single(findings, finding => finding.Subject.Path == Source);
        Assert.Equal(DoctorResolutionLane.ManualDecision, noCandidate.Resolution);
        Assert.Empty(noCandidate.Candidates?.Items ?? []);
        Assert.Equal(before, Snapshot(workspace));
    }

    [Theory(DisplayName = "Doctor and Repair use the same complete evidence and rename resolution in the same workspace")]
    [InlineData("proven", "Proven")]
    [InlineData("metadata-only", "Proven")]
    [InlineData("contradicted", "WeakOnly")]
    [InlineData("contradicted-committed", "Contradictory")]
    [InlineData("two-strong", "SeveralStrongCandidates")]
    [InlineData("weak-only", "WeakOnly")]
    [InlineData("deleted", "NoCandidate")]
    [InlineData("unborn", "Proven")]
    [InlineData("no-git", "Proven")]
    public async Task ResolutionAgreement(string scenario, string expected)
    {
        using var workspace = new RepairRenameGitWorkspace();
        var body = "# Rounding\n\n" + string.Join('\n', Enumerable.Range(0, 30).Select(index => $"Authored rounding rule {index}.")) + "\n";
        var documents = new Dictionary<string, string>
        {
            [Old] = Note(Description, body),
            [Source] = Note("Incoming reference", "# Source\n\n[Rounding](rounding.md)\n"),
            [".agents/guidance/_guidance.md"] = Root(Entries),
        };
        if (scenario == "contradicted-committed") documents[New] = documents[Old];
        await workspace.InitializeAsync(documents, unborn: scenario is "unborn" or "contradicted" or "no-git");
        if (scenario == "no-git")
        {
            var gitPath = Path.GetFullPath(workspace.PathFor(".git"));
            Assert.StartsWith(workspace.Root + Path.DirectorySeparatorChar, gitPath, StringComparison.Ordinal);
            Assert.Null(new DirectoryInfo(gitPath).LinkTarget);
            foreach (var file in Directory.EnumerateFiles(gitPath, "*", SearchOption.AllDirectories)) File.SetAttributes(file, FileAttributes.Normal);
            Directory.Delete(gitPath, recursive: true);
        }
        if (scenario is "deleted" or "contradicted-committed") File.Delete(workspace.PathFor(Old));
        else workspace.Move(Old, New);
        if (scenario is "metadata-only" or "weak-only") workspace.Write(New, Note(Description, "# Rounding\n\nChanged authored rules.\n"));
        if (scenario is "two-strong" or "weak-only") workspace.Write(".agents/guidance/other-rounding.md", File.ReadAllText(workspace.PathFor(New)));
        if (scenario == "contradicted") workspace.Write(".agents/guidance/_guidance.md",
            Root(Entries + "\n- [Specific authored rounding rules](money-rounding.md) - #Guidance"));
        var before = Snapshot(workspace);
        var paths = new PhysicalPathResolver();
        var reader = new RenameEvidenceReader(paths, new GitRenameEvidenceReader("git", workspace.Configure, timeProvider: new RenameEvidenceClock()).ReadAsync);
        var contributors = RepairOperationalContributorFactory.Create(paths);
        var doctor = await new DoctorDiagnosisReader(contributors, reader).ReadAsync(new DoctorRequest(workspace.Workspace), TestContext.Current.CancellationToken);
        var adapter = new RoutedMaintenanceRenameEvidenceAdapter(reader);
        var repair = await new RepairCatalogueReader(paths, adapter.ReadEvidenceAsync).ReadAsync(workspace.Workspace,
            doctor.Observation.LocalReferences, TestContext.Current.CancellationToken);
        var repairEvidence = Assert.IsType<RenameEvidenceResult>(repair.RenameEvidence);
        var doctorEvidence = Assert.IsType<RenameEvidenceResult>(doctor.Observation.Maintenance?.Renames);
        var resolution = Enum.Parse<RenameResolution>(expected);
        Assert.Equal(resolution, repairEvidence.Find(Old)?.Resolution);
        Assert.Equal(repairEvidence.Find(Old)?.Resolution, doctorEvidence.Find(Old)?.Resolution);
        Assert.Equal(repairEvidence.Facts.OldIdentities.Select(identity => identity.Path).Order(StringComparer.Ordinal),
            doctorEvidence.Facts.OldIdentities.Select(identity => identity.Path).Order(StringComparer.Ordinal));
        Assert.Equal(repairEvidence.Facts.Candidates.Select(candidate => candidate.Path), doctorEvidence.Facts.Candidates.Select(candidate => candidate.Path));
        Assert.Equal(repairEvidence.Facts.MissingPaths, doctorEvidence.Facts.MissingPaths);
        Assert.Empty(doctorEvidence.Facts.WeakCandidates);
        Assert.Equal(repairEvidence.Facts.PolicyExpectations, doctorEvidence.Facts.PolicyExpectations);
        Assert.Equal(repairEvidence.Facts.CandidateUniverseComplete, doctorEvidence.Facts.CandidateUniverseComplete);
        var proposal = Assert.Single(repair.Proposals, input => input.Proposal.SourceCanonicalPath == Source).Proposal;
        Assert.Equal(resolution, proposal.RenameResolution);
        var finding = Assert.Single(doctor.Result.Diagnosis.Domains.SelectMany(domain => domain.Findings),
            finding => finding.Subject.Path == Source && finding.Kind is DoctorFindingKind.ReferenceTargetRenamed or DoctorFindingKind.ReferenceTargetMissing);
        Assert.Equal(resolution == RenameResolution.Proven, finding.Resolution == DoctorResolutionLane.SafeExact);
        var lane = DoctorResolutionLane.ManualDecision;
        if (proposal.IsSafeExact) lane = DoctorResolutionLane.SafeExact;
        else if (proposal.Candidates is { Items.Count: > 0 }) lane = DoctorResolutionLane.GuidedChoice;
        Assert.Equal(lane, finding.Resolution);
        Assert.Equal(proposal.Candidates?.Items.Count ?? 0, finding.Candidates?.Items.Count ?? 0);
        Assert.Equal(before, Snapshot(workspace));
    }

    [Theory(DisplayName = "Doctor retries an explicit workspace after Git timeout instead of prescribing an edit or claiming metadata proof")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GitTimeout(bool hasMetadataSuccessor)
    {
        using var workspace = new RepairRenameGitWorkspace();
        await workspace.InitializeAsync(new Dictionary<string, string>
        {
            [Source] = Note("Incoming reference", "# Source\n\n[Rounding](rounding.md)\n"),
            [".agents/guidance/_guidance.md"] = Root(Entries),
        }, unborn: true);
        if (hasMetadataSuccessor) workspace.Write(New, Note(Description, "# Rounding\n\nAuthored rules.\n"));
        var before = Snapshot(workspace);
        var paths = new PhysicalPathResolver();
        var reader = new RenameEvidenceReader(paths, (_, _) => ValueTask.FromResult(RenameGitObservation.Unavailable(RenameGitAvailability.TimedOut)));
        var diagnosis = await new DoctorDiagnosisReader(RepairOperationalContributorFactory.Create(paths), reader)
            .ReadAsync(new DoctorRequest(workspace.Workspace), TestContext.Current.CancellationToken);
        var report = CliNextActionCompletion.Complete(DoctorReportSelector.Select(diagnosis.Result, new CliSelection(CliDetail.Full)), null);
        Assert.Equal(CliSemanticStatus.Incomplete, report.Status);
        Assert.Equal($"open-forge doctor --workspace {CliCommandText.Quote(workspace.Root)}", report.Next?.Command);
        Assert.Equal(workspace.Root, report.Workspace?.Path);
        Assert.Single(report.Limitations);
        Assert.Equal("Git did not answer in time, so renamed files could not be proven. Run the command again.", report.Limitations[0].Why);
        Assert.DoesNotContain(report.Findings, finding => finding.Code is "reference.target-missing" or "reference.target-renamed");
        foreach (var detail in Enum.GetValues<CliDetail>())
        {
            var text = CliRenderingStage.Render(new CliPresentationRequest<DoctorResult>(diagnosis.Result, new(CliFormat.Text, detail, null)), DoctorPresentation.Rendering).PrimaryContent;
            const string limitation = "Git did not answer in time, so renamed files could not be proven. Run the command again.";
            Assert.StartsWith("Doctor could not finish 1 check.", text, StringComparison.Ordinal);
            Assert.DoesNotContain("No problems found", text, StringComparison.Ordinal);
            Assert.Equal(1, text.Split(limitation, StringSplitOptions.None).Length - 1);
            Assert.Contains($"Next: {report.Next?.Command}", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Update or remove this link", text, StringComparison.Ordinal);
            var json = CliRenderingStage.Render(new CliPresentationRequest<DoctorResult>(diagnosis.Result, new(CliFormat.Json, detail, null)), DoctorPresentation.Rendering).PrimaryContent;
            using var document = JsonDocument.Parse(json);
            Assert.Equal(report.Next?.Command, document.RootElement.GetProperty("next").GetProperty("command").GetString());
            Assert.Single(document.RootElement.GetProperty("limitations").EnumerateArray());
            Assert.DoesNotContain("gitRenameEvidenceTimedOut", json, StringComparison.Ordinal);
            Assert.DoesNotContain("Git rename evidence timed out.", json, StringComparison.Ordinal);
        }
        Assert.Equal(before, Snapshot(workspace));
    }

    private static string Note(string description, string body) => OpenForgeDocumentSeed.Metadata(description, ["Guidance"], "\n" + body);
    private static string Root(string entries) => Note("Guidance", $"# Guidance\n\n## Axioms\n\n- inherited - No local axioms; loaded ancestor axioms remain active.\n\n## Entries\n\n{entries}\n");
    private static string[] Snapshot(RepairRenameGitWorkspace workspace) => Directory.EnumerateFiles(workspace.PathFor(".agents"), "*", SearchOption.AllDirectories)
        .Order(StringComparer.Ordinal).Select(path => path + ":" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path)))).ToArray();
}
