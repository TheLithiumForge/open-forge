using OpenForge.Cli.Composition.Shared.Git;
using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Diagnosis;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Presentation.Doctor.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Doctor.Shared.Wording;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Repair;
using OpenForge.Cli.IntegrationTests.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Doctor;

[Trait("Feature", "doctor-command"), Trait("Evidence", "Integration"), Trait("Boundary", "Processing")]
public sealed class DoctorRepairActionableAgreementIntegrationTests
{
    private const string Old = ".agents/guidance/rounding.md";
    private const string New = ".agents/guidance/new.md";
    private const string Source = ".agents/guidance/source.md";

    [Theory(DisplayName = "Doctor and Repair preserve each protection boundary without inventing a link restoration action")]
    [InlineData("ordinary")]
    [InlineData("managed")]
    [InlineData("managed-skill")]
    [InlineData("native-skill")]
    [InlineData("excluded")]
    [InlineData("managed-target")]
    public async Task ProtectionAgreement(string protection)
    {
        using var workspace = new RepairRenameGitWorkspace();
        var source = protection.EndsWith("skill", StringComparison.Ordinal) ? ".agents/skills/checks/SKILL.md" : Source;
        var destination = source == Source ? "rounding.md" : "../../guidance/rounding.md";
        var text = Note("Incoming reference", $"# Source\n\nRead [Rounding]({destination}).\n");
        if (source != Source) text = $"---\nname: checks\ndescription: Check workspace rules\n---\n\n# Checks\n\nRead [Rounding]({destination}).\n";
        await workspace.InitializeAsync(new Dictionary<string, string>
        {
            [source] = text,
            [Old] = Note("Monetary rounding policy", "# Rounding\n\nKeep minor units.\n"),
            [".agents/guidance/_guidance.md"] = Root("- [Monetary rounding policy](rounding.md) - #Guidance"),
        }, unborn: true);
        if (source != Source)
        {
            workspace.Write(".agents/skills/_skills.md", Root("- [Check workspace rules](checks/SKILL.md) - #Skill"));
            File.AppendAllText(workspace.PathFor(".agents/loader.md"), "\n- [Guidance](skills/_skills.md) - #Skill\n");
        }
        if (protection is "managed" or "managed-skill" or "managed-target")
        {
            var path = protection == "managed-target" ? Old : source;
            var ownership = WorkspaceOwnershipDocument.Empty with { Framework = new(new("open-forge", null), [path], []) };
            File.WriteAllBytes(workspace.PathFor(".agents/open-forge.lock.json"), WorkspaceOwnershipCodec.Write(ownership));
        }
        if (protection == "excluded") workspace.Write(".agents/open-forge.json", $"{{\"schemaVersion\":1,\"removedFiles\":[\"{source}\"]}}");
        workspace.Move(Old, New);
        var before = Snapshot(workspace);
        var (doctor, repair) = await Diagnose(workspace);
        var reference = Assert.Single(doctor.Observation.LocalReferences.References, reference => reference.SourcePath == source && reference.Destination == destination);
        var finding = Assert.Single(doctor.Result.Diagnosis.Domains.SelectMany(domain => domain.Findings), finding => finding.Subject.Path == source
            && finding.Kind is DoctorFindingKind.ReferenceTargetMissing or DoctorFindingKind.ReferenceTargetRenamed);
        if (protection is "managed" or "managed-skill" or "managed-target")
        {
            var path = protection == "managed-target" ? Old : source;
            Assert.Equal(DoctorResolutionLane.TargetedOperation, RepairLane(repair, reference));
            Assert.Equal(DoctorResolutionLane.ManualDecision, finding.Resolution);
            Assert.Equal(DoctorFindingKind.ReferenceTargetMissing, finding.Kind);
            Assert.Equal(path, finding.ProtectedReferencePath);
            Assert.Empty(finding.Actions);
            Assert.EndsWith($"through the owner of {path}.", Assert.Single(DoctorWording.Actions(finding)).Command, StringComparison.Ordinal);
            Assert.Null(finding.Proposal);
        }
        else Assert.Equal(RepairLane(repair, reference), finding.Resolution);
        Assert.Equal(before, Snapshot(workspace));
    }

    [Theory(DisplayName = "Doctor and Repair agree on actionable fragments and preserve metadata identity before Index")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FragmentAgreement(bool alreadyIndexed)
    {
        using var workspace = new RepairRenameGitWorkspace();
        await workspace.InitializeAsync(new Dictionary<string, string>
        {
            [Source] = Note("Incoming reference", "# Source\n\nRead [ZZUnrelatedLabelZZ](rounding.md#rounding).\n"),
            [Old] = Note("Monetary rounding policy", "# Rounding\n\nKeep minor units.\n"),
            [".agents/guidance/_guidance.md"] = Root("- [Monetary rounding policy](rounding.md) - #Guidance\n- [Incoming reference](source.md) - #Guidance"),
        });
        workspace.Move(Old, New);
        workspace.Write(New, Note("Monetary rounding policy", "# Changed heading\n\nCompletely different body.\n"));
        if (alreadyIndexed) workspace.Write(".agents/guidance/_guidance.md",
            Root("- [Monetary rounding policy](new.md) - #Guidance\n- [Incoming reference](source.md) - #Guidance"));
        var before = Snapshot(workspace);
        var (doctor, repair) = await Diagnose(workspace);
        var reference = Assert.Single(doctor.Observation.LocalReferences.References, reference => reference.SourcePath == Source);
        var finding = Assert.Single(doctor.Result.Diagnosis.Domains.SelectMany(domain => domain.Findings), finding => finding.Subject.Path == Source
            && finding.Kind is DoctorFindingKind.ReferenceTargetMissing or DoctorFindingKind.ReferenceTargetRenamed);
        Assert.Equal(RepairLane(repair, reference), finding.Resolution);
        var proposal = Assert.Single(repair.Proposals, input => input.Proposal.SourceCanonicalPath == Source).Proposal;
        Assert.Equal(proposal.Candidates?.Items.Count ?? 0, finding.Candidates?.Items.Count ?? 0);
        if (!alreadyIndexed)
        {
            Assert.Equal(DoctorResolutionLane.GuidedChoice, finding.Resolution);
            Assert.Equal("open-forge repair", DoctorReportSelector.Select(doctor.Result, new CliSelection(CliDetail.Standard)).Next?.Command);
            Assert.Contains("rounding.md", File.ReadAllText(workspace.PathFor(".agents/guidance/_guidance.md")), StringComparison.Ordinal);
        }
        else Assert.Equal(DoctorResolutionLane.ManualDecision, finding.Resolution);
        Assert.Equal(before, Snapshot(workspace));
    }

    private static async Task<(OpenForge.Cli.Core.Commands.Doctor.Models.Observation.DoctorDiagnosisRead Doctor, RepairCatalogueRead Repair)> Diagnose(RepairRenameGitWorkspace workspace)
    {
        var paths = new PhysicalPathResolver();
        var reader = new RenameEvidenceReader(paths,
            new GitRenameEvidenceReader("git", workspace.Configure, timeProvider: new RenameEvidenceClock()).ReadAsync);
        var doctor = await new DoctorDiagnosisReader(RepairOperationalContributorFactory.Create(paths), reader)
            .ReadAsync(new DoctorRequest(workspace.Workspace), TestContext.Current.CancellationToken);
        var repair = await new RepairCatalogueReader(paths, new RoutedMaintenanceRenameEvidenceAdapter(reader).ReadEvidenceAsync)
            .ReadAsync(workspace.Workspace, doctor.Observation.LocalReferences, TestContext.Current.CancellationToken);
        return (doctor, repair);
    }

    private static DoctorResolutionLane RepairLane(RepairCatalogueRead repair, LocalReferenceObservation reference)
    {
        var protection = repair.Findings.FirstOrDefault(finding => finding.Protection && finding.SourceCanonicalPath == reference.SourcePath
            && finding.Occurrence?.ByteOffset == reference.DestinationLocation?.ByteOffset);
        if (protection is not null) return protection.OwnerAction is null ? DoctorResolutionLane.ManualDecision : DoctorResolutionLane.TargetedOperation;
        var proposal = Assert.Single(repair.Proposals, input => input.Proposal.SourceCanonicalPath == reference.SourcePath
            && input.Proposal.Occurrence.ByteOffset == reference.DestinationLocation?.ByteOffset).Proposal;
        if (proposal.IsSafeExact) return DoctorResolutionLane.SafeExact;
        return proposal.Candidates is { Items.Count: > 0 } ? DoctorResolutionLane.GuidedChoice : DoctorResolutionLane.ManualDecision;
    }

    private static string Note(string description, string body) => OpenForgeDocumentSeed.Metadata(description, ["Guidance"], "\n" + body);
    private static string Root(string entries) => Note("Guidance", $"# Guidance\n\n## Axioms\n\n- inherited - No local axioms; loaded ancestor axioms remain active.\n\n## Entries\n\n{entries}\n");
    private static string[] Snapshot(RepairRenameGitWorkspace workspace) => Directory.EnumerateFiles(workspace.PathFor(".agents"), "*", SearchOption.AllDirectories)
        .Order(StringComparer.Ordinal).Select(path => path + ":" + Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path)))).ToArray();
}
