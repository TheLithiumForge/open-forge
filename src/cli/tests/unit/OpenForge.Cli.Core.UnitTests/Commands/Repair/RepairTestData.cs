using System.Text;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Repair;

internal static class RepairTestData
{
    internal const string SourcePath = ".agents/docs/guide.md";
    internal const string TargetPath = ".agents/docs/new.md";
    internal const string WorkspaceRoot = "/tmp/open-forge-repair-workspace";

    internal static CliWorkspace Workspace(
        CliWorkspaceSelectionMethod selectedBy = CliWorkspaceSelectionMethod.ExplicitWorkspace)
        => new(WorkspaceRoot, WorkspaceRoot, selectedBy);

    internal static CliInvocation Invocation(
        CliOutputFormat format = CliOutputFormat.Json,
        CliVerbosity verbosity = CliVerbosity.Normal,
        CliWorkspace? workspace = null)
    {
        workspace ??= Workspace();
        return new CliInvocation(
            new CliProcessIdentity("open-forge", "test"),
            new CliPresentation(format, CliView.Expanded, verbosity),
            CliTerminalMode.None,
            new CliWorkspaceRequest(workspace.LexicalRoot, workspace.LexicalRoot),
            workspace);
    }

    internal static RepairRequest Request(
        RepairMode mode = RepairMode.DryRun,
        bool automatic = true,
        IEnumerable<RepairRelinkRequest>? relinks = null,
        bool allowInteraction = false)
        => new(
            Workspace(),
            mode,
            automatic,
            relinks ?? [],
            allowInteraction);

    internal static RepairSourceLocation SourceLocation(
        string path = SourcePath,
        int line = 1,
        int column = 1)
        => new(path, line, column);

    internal static RepairRelinkRequest Relink(
        string expectedDestination = "old.md",
        string selectedTargetPath = TargetPath,
        string? selectedTargetFragment = null,
        string sourcePath = SourcePath,
        int line = 1,
        int column = 1)
        => new(
            sourcePath,
            line,
            column,
            expectedDestination,
            selectedTargetPath,
            selectedTargetFragment);

    internal static RepairTargetSelection Target(
        string path = TargetPath,
        string? fragment = null,
        IEnumerable<RepairCandidateEvidence>? provenance = null)
        => new(path, fragment, provenance ?? []);

    internal static SourceLocation Occurrence(long byteOffset = 0, int byteLength = 3)
        => new(
            line: 1,
            column: checked((int)byteOffset + 1),
            byteOffset,
            byteLength);

    internal static FileStateSnapshot FileState(
        string text,
        string logicalPath = SourcePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(WorkspaceRoot, logicalPath));
        return FileStateSnapshot.File(fullPath, fullPath, Encoding.UTF8.GetBytes(text));
    }

    internal static FileStateSnapshot FileState(
        ReadOnlySpan<byte> bytes,
        string logicalPath = SourcePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(WorkspaceRoot, logicalPath));
        return FileStateSnapshot.File(fullPath, fullPath, bytes);
    }

    internal static RecoveryBundleAttribution Attribution()
        => RecoveryBundleAttribution.Read(
            RecoveryBundleProducer.Repair,
            RecoveryBundleOperation.Repair,
            RecoveryBundleSubject.Workspace(new string('a', 64)));

    internal static RepairableReferenceInput Reference(
        string expectedDestination = "old",
        string observedDestination = "old",
        string intendedDestination = "new",
        RepairCatalogueMember member = RepairCatalogueMember.SameTargetPath,
        RepairSelectionOrigin origin = RepairSelectionOrigin.Automatic,
        string sourcePath = SourcePath,
        long byteOffset = 0,
        int byteLength = 3,
        string fileText = "old")
        => new(
            new RepairOccurrenceState(
                sourcePath,
                Occurrence(byteOffset, byteLength),
                FileState(fileText, sourcePath)),
            new RepairDestinationTransition(
                expectedDestination,
                observedDestination,
                intendedDestination),
            member,
            origin,
            Target());

    internal static RepairProposal SafeProposal(
        string expectedDestination = "old",
        string intendedDestination = "new",
        string sourcePath = SourcePath,
        long byteOffset = 0,
        int byteLength = 3,
        RepairCatalogueMember member = RepairCatalogueMember.SameTargetPath)
        => new(
            member,
            sourcePath,
            Occurrence(byteOffset, byteLength),
            expectedDestination,
            intendedDestination,
            Target());

    internal static RepairSelectedProposal Selected(
        RepairProposal? proposal = null,
        RepairSelectionOrigin origin = RepairSelectionOrigin.Automatic)
    {
        proposal ??= SafeProposal();
        var resolution = proposal.Resolution
            ?? throw new InvalidOperationException("The test proposal must be safe-exact.");
        return new(proposal, resolution, [origin]);
    }

    internal static RepairSelection Selection(
        RepairSelectedProposal selected,
        RepairSelectionMode mode = RepairSelectionMode.Automatic,
        IEnumerable<RepairProposal>? unselected = null)
        => new(mode, [selected], unselected ?? []);

    internal static RepairEffect Effect(
        string sourcePath = SourcePath,
        string expectedText = "old",
        string intendedText = "new",
        RepairChange? change = null)
    {
        change ??= new RepairChange(
            Occurrence(byteLength: 3),
            "old",
            "new",
            RepairCatalogueMember.SameTargetPath,
            Target(),
            [RepairSelectionOrigin.Automatic]);
        return new(
            sourcePath,
            FileState(expectedText, sourcePath),
            FileState(intendedText, sourcePath),
            [change],
            Attribution());
    }

    internal static RepairNoOp NoOp(
        string sourcePath = SourcePath,
        string destination = "new",
        long byteOffset = 0,
        int byteLength = 3,
        RepairSelectionOrigin origin = RepairSelectionOrigin.Automatic)
        => new(
            sourcePath,
            Occurrence(byteOffset, byteLength),
            destination,
            RepairCatalogueMember.SameTargetPath,
            Target(),
            FileState(destination, sourcePath),
            [origin]);

    internal static RepairStep Step(
        RepairSelectedProposal selected,
        RepairEffect? effect = null,
        RepairNoOp? noOp = null,
        RepairStepOutcome outcome = RepairStepOutcome.Verified)
        => new(
            1,
            selected,
            new RepairDependency(
            [
                RepairDependencyDomain.WorkspaceContainment,
                RepairDependencyDomain.RouteAndHeading,
                RepairDependencyDomain.LocalReference,
            ]),
            new RepairVerificationRequirement(
            [
                RepairVerificationKind.DestinationLiteral,
                RepairVerificationKind.SameTargetIdentity,
                RepairVerificationKind.ResultingBytes,
            ]),
            effect is null
                ? RepairRecoveryRequirement.NotRequired
                : RepairRecoveryRequirement.Required(Attribution()),
            effect,
            noOp,
            outcome);

    internal static RepairPlan Plan(
        RepairSelectedProposal selected,
        RepairStep step,
        RepairRequest? request = null,
        IEnumerable<RepairConflict>? conflicts = null)
    {
        request ??= Request();
        return new(request, Selection(selected), [step], conflicts ?? []);
    }

    internal static RepairResult Result(
        IEnumerable<RepairFinding>? findings = null,
        RepairResultFacts? facts = null,
        RepairMode mode = RepairMode.DryRun,
        bool automatic = true,
        RepairSelectionMode selectionMode = RepairSelectionMode.Automatic,
        IEnumerable<RepairRelinkRequest>? relinks = null,
        CliWorkspace? workspace = null)
    {
        facts ??= new RepairResultFacts
        {
            Diagnosis = new RepairDiagnosisCoverage(
                RepairCoverageState.NotRequested,
                RepairCoverageState.NotRequested,
                RepairCoverageState.NotRequested,
                RepairCoverageState.NotRequested),
            Selection = null,
            Plan = null,
            Findings = [],
            AffectedPaths = [],
            Counts = RepairCounts.Empty,
            Preflight = RepairPreflight.NotRequested,
            Application = RepairApplication.NotRequested,
            Verification = RepairVerification.NotRequested,
            Recovery = RepairRecovery.NotRequired,
            PostDiagnosis = RepairPostDiagnosis.NotRequested,
        };

        var formation = new RepairResultFormation
        {
            Workspace = workspace ?? Workspace(),
            Mode = mode,
            Automatic = automatic,
            Relinks = [.. relinks ?? []],
            SelectionMode = selectionMode,
            Facts = facts,
        };
        return findings is null
            ? new RepairResult(formation)
            : new RepairResult(formation, findings);
    }

    internal static RepairDiagnosisCoverage CompleteCoverage()
        => new(
            RepairCoverageState.Complete,
            RepairCoverageState.Complete,
            RepairCoverageState.Complete,
            RepairCoverageState.Complete);

    internal static RepairResultFacts CompleteFacts(
        RepairSelection? selection = null,
        RepairPlan? plan = null,
        RepairPostDiagnosis? postDiagnosis = null,
        RepairRecovery? recovery = null,
        RepairApplication? application = null,
        RepairPreflight? preflight = null,
        RepairVerification? verification = null,
        IEnumerable<RepairFinding>? findings = null,
        IEnumerable<string>? affectedPaths = null,
        RepairCounts? counts = null,
        RepairDiagnosisCoverage? diagnosis = null)
        => new()
        {
            Diagnosis = diagnosis ?? CompleteCoverage(),
            Selection = selection,
            Plan = plan,
            Findings = findings?.ToArray() ?? [],
            AffectedPaths = affectedPaths?.ToArray() ?? [],
            Counts = counts ?? RepairCounts.Empty,
            Preflight = preflight ?? RepairPreflight.NotRequested,
            Application = application ?? RepairApplication.NotRequested,
            Verification = verification ?? RepairVerification.NotRequested,
            Recovery = recovery ?? RepairRecovery.NotRequired,
            PostDiagnosis = postDiagnosis ?? RepairPostDiagnosis.NotRequested,
        };
}
