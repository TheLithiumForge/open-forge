using OpenForge.Cli.Core.Commands.Repair.Models.Presentation;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Rendering;

internal static class RepairJsonProjection
{
    internal static RepairJsonDocument Create(RepairResult result)
    {
        CliOperationStage.ValidateResult(result);
        return new RepairJsonDocument
        {
            SchemaVersion = RepairDefinitions.SchemaVersion,
            Command = result.Command,
            Status = CliStatusDefinitions.Read(result.Status).MachineName,
            Workspace = result.Workspace is null ? null : Workspace(result.Workspace),
            Result = new RepairJsonResult
            {
                LibraryExecution = result.LibraryExecution is { } execution ? RepairLibraryPresentation.Execution(execution) : null,
                Mode = RepairDefinitions.ReadMachineName(result.Mode),
                Automatic = result.Automatic,
                SelectionMode = RepairDefinitions.ReadMachineName(result.SelectionMode),
                Relinks = [.. result.Relinks.Select(Relink)],
                Diagnosis = Coverage(result.Diagnosis),
                Selection = result.Selection is null ? null : Selection(result.Selection),
                Plan = result.Plan is null ? null : Plan(result.Plan),
                AffectedPaths = [.. result.AffectedPaths],
                Counts = Counts(result.Counts),
                Preflight = Preflight(result.Preflight),
                Application = Application(result.Application),
                Verification = Verification(result.Verification),
                Recovery = Recovery(result.Recovery),
                PostDiagnosis = PostDiagnosis(result.PostDiagnosis),
                Findings = [.. result.Findings.Select(Finding)],
            },
            Next = result.Next is null
                ? null
                : new RepairJsonNext
                {
                    Command = result.Next.Command,
                    Reason = result.Next.Reason,
                },
        };
    }

    private static RepairJsonWorkspace Workspace(CliWorkspace workspace)
        => new()
        {
            Path = workspace.LexicalRoot,
            SelectedBy = workspace.SelectedBy switch
            {
                CliWorkspaceSelectionMethod.CurrentDirectory => "current-directory",
                CliWorkspaceSelectionMethod.ExplicitWorkspace => "explicit-workspace",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(workspace),
                    workspace.SelectedBy,
                    "The workspace selection method is not defined."),
            },
        };

    private static RepairJsonRelink Relink(RepairRelinkRequest relink)
        => new()
        {
            SourcePath = relink.SourceCanonicalPath,
            Line = relink.Line,
            Column = relink.Column,
            ExpectedDestination = relink.ExpectedDestination,
            Target = Target(relink.Target),
        };

    private static RepairJsonCoverage Coverage(RepairDiagnosisCoverage coverage)
        => new()
        {
            WorkspaceAndPath = RepairDefinitions.ReadMachineName(coverage.WorkspaceAndPath),
            RouteAndHeading = RepairDefinitions.ReadMachineName(coverage.RouteAndHeading),
            LocalReferences = RepairDefinitions.ReadMachineName(coverage.LocalReferences),
            SelectedScope = RepairDefinitions.ReadMachineName(coverage.SelectedScope),
        };

    private static RepairJsonSelection Selection(RepairSelection selection)
        => new()
        {
            SelectedLibraries = [.. selection.Libraries.Selected.Select(RepairLibraryPresentation.Selected)],
            UnselectedLibraries = [.. selection.Libraries.Unselected.Select(RepairLibraryPresentation.Proposal)],
            Mode = RepairDefinitions.ReadMachineName(selection.Mode),
            Selected = [.. selection.Selected.Select(SelectedProposal)],
            Unselected = [.. selection.Unselected.Select(Proposal)],
        };

    private static RepairJsonProposal Proposal(RepairProposal proposal)
        => new()
        {
            Member = RepairDefinitions.ReadMachineName(proposal.Member),
            SourcePath = proposal.SourceCanonicalPath,
            Occurrence = Location(proposal.Occurrence),
            ExpectedDestination = proposal.ExpectedDestination,
            IntendedDestination = proposal.IntendedDestination,
            Target = proposal.Target is null ? null : Target(proposal.Target),
            Candidates = proposal.Candidates is null ? null : Candidates(proposal.Candidates),
        };

    private static RepairJsonSelectedProposal SelectedProposal(RepairSelectedProposal selected)
        => new()
        {
            Proposal = Proposal(selected.Proposal),
            Resolution = Resolution(selected.Resolution),
            Origins = [.. selected.Origins.Select(RepairDefinitions.ReadMachineName)],
        };

    private static RepairJsonResolution Resolution(RepairProposalResolution resolution)
        => new()
        {
            IntendedDestination = resolution.IntendedDestination,
            Target = Target(resolution.Target),
        };

    private static RepairJsonCandidateSet Candidates(RepairCandidateSet candidates)
        => new()
        {
            Cardinality = RepairDefinitions.ReadMachineName(candidates.Cardinality),
            Items = [.. candidates.Items.Select(Candidate)],
        };

    private static RepairJsonCandidate Candidate(RepairCandidate candidate)
        => new()
        {
            Target = Target(candidate.Target),
            Evidence = [.. candidate.Evidence.Select(Evidence)],
            RecommendedForReview = candidate.RecommendedForReview,
        };

    private static RepairJsonCandidateEvidence Evidence(RepairCandidateEvidence evidence)
        => new()
        {
            Kind = RepairDefinitions.ReadMachineName(evidence.Kind),
            Value = evidence.Value,
            Location = evidence.Location is null ? null : Location(evidence.Location),
        };

    private static RepairJsonPlan Plan(RepairPlan plan)
        => new()
        {
            LibrarySteps = [.. plan.LibrarySteps.Select(RepairLibraryPresentation.Step)],
            Blocked = plan.IsBlocked,
            NoOp = plan.IsNoOp,
            Steps = [.. plan.Steps.Select(Step)],
            Effects = [.. plan.Effects.Select(Effect)],
            NoOps = [.. plan.NoOps.Select(NoOp)],
            Conflicts = [.. plan.Conflicts.Select(Conflict)],
        };

    private static RepairJsonStep Step(RepairStep step)
        => new()
        {
            Ordinal = step.Ordinal,
            Proposal = Proposal(step.Proposal),
            Resolution = Resolution(step.Resolution),
            Origins = [.. step.Origins.Select(RepairDefinitions.ReadMachineName)],
            Dependencies = [.. step.Dependency.Domains.Select(RepairDefinitions.ReadMachineName)],
            Verification = [.. step.Verification.Kinds.Select(RepairDefinitions.ReadMachineName)],
            RecoveryRequirement = RepairDefinitions.ReadMachineName(step.Recovery.Kind),
            Effect = step.Effect is null ? null : Effect(step.Effect),
            NoOp = step.NoOp is null ? null : NoOp(step.NoOp),
            Outcome = RepairDefinitions.ReadMachineName(step.Outcome),
        };

    private static RepairJsonEffect Effect(RepairEffect effect)
        => new()
        {
            SourcePath = effect.SourceCanonicalPath,
            ExpectedState = State(effect.ExpectedState),
            IntendedState = State(effect.IntendedState),
            Changes = [.. effect.Changes.Select(Change)],
            Recovery = Attribution(effect.RecoveryAttribution),
            FileChangeKind = RepairDefinitions.ReadMachineName(effect.FileChange.Kind),
        };

    private static RepairJsonChange Change(RepairChange change)
        => new()
        {
            Occurrence = Location(change.Occurrence),
            ExpectedDestination = change.ExpectedDestination,
            IntendedDestination = change.IntendedDestination,
            CatalogueMember = RepairDefinitions.ReadMachineName(change.CatalogueMember),
            Target = Target(change.Target),
            Origins = [.. change.Origins.Select(RepairDefinitions.ReadMachineName)],
        };

    private static RepairJsonNoOp NoOp(RepairNoOp noOp)
        => new()
        {
            SourcePath = noOp.SourceCanonicalPath,
            Occurrence = Location(noOp.Occurrence),
            Destination = noOp.Destination,
            CatalogueMember = RepairDefinitions.ReadMachineName(noOp.CatalogueMember),
            Target = Target(noOp.Target),
            CurrentState = State(noOp.CurrentState),
            Origins = [.. noOp.Origins.Select(RepairDefinitions.ReadMachineName)],
        };

    private static RepairJsonConflict Conflict(RepairConflict conflict)
        => new()
        {
            Library = conflict.Library is { } library ? RepairLibraryPresentation.Proposal(new RepairLibraryRecoveryProposal(library)) : null,
            Kind = RepairDefinitions.ReadMachineName(conflict.Kind),
            SourcePath = conflict.SourceCanonicalPath,
            Occurrence = conflict.Occurrence is null ? null : Location(conflict.Occurrence),
            Cause = conflict.Cause,
        };

    private static RepairJsonState State(FileStateSnapshot state)
        => new()
        {
            Kind = RepairDefinitions.ReadMachineName(state.Kind),
            LogicalPath = state.LogicalPath,
            PhysicalPath = state.PhysicalPath,
            ContentHash = state.ContentHash,
            ByteLength = state.Bytes.Length,
            HasBytes = state.HasBytes,
        };

    private static RepairJsonAttribution Attribution(RecoveryBundleAttribution attribution)
        => new()
        {
            Producer = attribution.Producer switch
            {
                RecoveryBundleProducer.Framework => "framework",
                RecoveryBundleProducer.Extension => "extension",
                RecoveryBundleProducer.Index => "index",
                RecoveryBundleProducer.Route => "route",
                RecoveryBundleProducer.Repair => "repair",
                RecoveryBundleProducer.Library => "library",
                _ => throw new ArgumentOutOfRangeException(nameof(attribution), attribution.Producer, "The recovery producer is not defined."),
            },
            Operation = attribution.Operation switch
            {
                RecoveryBundleOperation.Install => "install",
                RecoveryBundleOperation.Index => "index",
                RecoveryBundleOperation.Create => "create",
                RecoveryBundleOperation.Init => "init",
                RecoveryBundleOperation.Move => "move",
                RecoveryBundleOperation.Update => "update",
                RecoveryBundleOperation.Remove => "remove",
                RecoveryBundleOperation.Repair => "repair",
                RecoveryBundleOperation.Attach => "attach",
                RecoveryBundleOperation.Sync => "sync",
                RecoveryBundleOperation.Detach => "detach",
                _ => throw new ArgumentOutOfRangeException(nameof(attribution), attribution.Operation, "The recovery operation is not defined."),
            },
            SubjectKind = attribution.Subject.Kind switch
            {
                RecoveryBundleSubjectKind.Workspace => "workspace",
                _ => throw new ArgumentOutOfRangeException(nameof(attribution), attribution.Subject.Kind, "The recovery subject kind is not defined."),
            },
            SubjectIdentity = attribution.Subject.Identity,
        };

    private static RepairJsonCounts Counts(RepairCounts counts)
        => new()
        {
            SelectedFindings = counts.SelectedFindings,
            UnselectedFindings = counts.UnselectedFindings,
            Repaired = counts.Repaired,
            Remaining = counts.Remaining,
            NewFindings = counts.NewFindings,
            Manual = counts.Manual,
            Guided = counts.Guided,
            Blocked = counts.Blocked,
            SelectedEffects = counts.SelectedEffects,
            AppliedEffects = counts.AppliedEffects,
            VerifiedEffects = counts.VerifiedEffects,
            NoOps = counts.NoOps,
            Conflicts = counts.Conflicts,
        };

    private static RepairJsonPreflight Preflight(RepairPreflight preflight)
        => new()
        {
            State = RepairDefinitions.ReadMachineName(preflight.State),
            Cause = preflight.Cause,
            Recovery = RepairDefinitions.ReadMachineName(preflight.Recovery),
        };

    private static RepairJsonApplication Application(RepairApplication application)
        => new()
        {
            State = RepairDefinitions.ReadMachineName(application.State),
            AppliedEffects = application.AppliedEffects,
            Cause = application.Cause,
        };

    private static RepairJsonVerification Verification(RepairVerification verification)
        => new()
        {
            Targets = RepairDefinitions.ReadMachineName(verification.Targets),
            ResultingBytes = RepairDefinitions.ReadMachineName(verification.ResultingBytes),
            PostConditions = RepairDefinitions.ReadMachineName(verification.PostConditions),
        };

    private static RepairJsonRecovery Recovery(RepairRecovery recovery)
        => new()
        {
            State = RepairDefinitions.ReadMachineName(recovery.State),
            Residual = RepairDefinitions.ReadMachineName(recovery.Residual),
            ResidualPath = recovery.ResidualPath,
            Attribution = recovery.Attribution is null ? null : Attribution(recovery.Attribution),
        };

    private static RepairJsonPostDiagnosis PostDiagnosis(RepairPostDiagnosis postDiagnosis)
        => new()
        {
            State = RepairDefinitions.ReadMachineName(postDiagnosis.State),
            Coverage = Coverage(postDiagnosis.Coverage),
            Findings = [.. postDiagnosis.Findings.Select(Finding)],
        };

    private static RepairJsonFinding Finding(RepairFinding finding)
        => new()
        {
            Code = RepairDefinitions.ReadMachineName(finding.Code),
            Status = CliStatusDefinitions.Read(finding.Status).MachineName,
            Cause = finding.Cause,
            SourcePath = finding.SourceCanonicalPath,
            Occurrence = finding.Occurrence is null ? null : Location(finding.Occurrence),
            Target = finding.Target is null ? null : Target(finding.Target),
        };

    private static RepairJsonLocation Location(SourceLocation location)
        => new()
        {
            Line = location.Line,
            Column = location.Column,
            ByteOffset = location.ByteOffset,
            ByteLength = location.ByteLength,
        };

    private static RepairJsonTarget Target(RepairTargetSelection target)
        => new()
        {
            Path = target.CanonicalTargetPath,
            Fragment = target.TargetFragment,
        };
}
