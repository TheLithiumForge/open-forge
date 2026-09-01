using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal static class ExtensionInspectBoundaryResultFactory
{
    private const string GeneratedOwnership = "derived-navigation-only";

    internal static ExtensionInspectResult Invalid(
        CliWorkspace? workspace,
        string? supplied,
        ExtensionInspectFindingCode code,
        string cause)
    {
        var finding = ExtensionInspectFindingPolicy.Create(new ExtensionInspectFindingInput
        {
            Code = code,
            Subject = supplied,
            Cause = cause,
        });
        return Empty(new ExtensionInspectEmptyResultInput(
            CliSemanticStatus.Invalid,
            workspace,
            new ExtensionInspectSubject
            {
                Supplied = supplied,
                Form = supplied is null ? null : ExtensionInspectSubjectForm.StableId,
                Id = null,
                State = ExtensionInspectSubjectState.Invalid,
                Candidates = [],
            },
            [finding],
            ExtensionInspectDefinitions.InvalidNext));
    }

    internal static ExtensionInspectResult WorkspaceBlocked(
        string? supplied,
        string cause)
    {
        var finding = ExtensionInspectFindingPolicy.Create(new ExtensionInspectFindingInput
        {
            Code = ExtensionInspectFindingCode.WorkspaceUnavailable,
            Subject = supplied,
            Cause = cause,
        });
        return Empty(new ExtensionInspectEmptyResultInput(
            CliSemanticStatus.Blocked,
            Workspace: null,
            new ExtensionInspectSubject
            {
                Supplied = supplied,
                Form = supplied is null ? null : ExtensionInspectSubjectForm.StableId,
                Id = supplied,
                State = ExtensionInspectSubjectState.NotStarted,
                Candidates = [],
            },
            [finding],
            ExtensionInspectDefinitions.BlockedNext));
    }

    internal static ExtensionInspectResult Empty(ExtensionInspectEmptyResultInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        return new ExtensionInspectResult
        {
            Status = input.Status,
            Workspace = input.Workspace,
            Subject = input.Subject,
            Source = new ExtensionInspectSource
            {
                Supplied = null,
                Explicit = false,
                Identity = null,
                Kind = null,
                State = ExtensionInspectSourceState.NotStarted,
            },
            Lifecycle = new ExtensionInspectLifecycle
            {
                DocumentPath = LifecycleSchema.RelativePath,
                ReadState = ExtensionInspectLifecycleReadState.NotStarted,
                Trust = ExtensionInspectLifecycleTrust.NotStarted,
                Coverage = ExtensionInspectCoverageState.NotStarted,
                WorkspaceBinding = ExtensionInspectWorkspaceBinding.NotChecked,
                FingerprintPolicy = null,
            },
            Installed = new ExtensionInspectInstalled
            {
                State = ExtensionInspectInstalledState.NotStarted,
                Package = null,
            },
            Available = new ExtensionInspectAvailable
            {
                State = ExtensionInspectAvailableState.NotStarted,
                Package = null,
            },
            Dependencies = new ExtensionInspectDependencyClosure
            {
                State = ExtensionInspectDependencyState.NotStarted,
                Declared = [],
                Resolved = [],
                Order = [],
            },
            PathFacts = new ExtensionInspectPathFacts
            {
                State = ExtensionInspectPathState.NotStarted,
                Declared = [],
                Current = [],
            },
            Comparison = new ExtensionInspectComparison
            {
                State = ExtensionInspectComparisonState.NotStarted,
                Mode = ExtensionInspectComparisonMode.None,
                Baseline = ExtensionInspectResultMappings.Side(
                    ExtensionInspectComparisonSideState.NotStarted,
                    []),
                Current = ExtensionInspectResultMappings.Side(
                    ExtensionInspectComparisonSideState.NotStarted,
                    []),
                Intended = ExtensionInspectResultMappings.Side(
                    ExtensionInspectComparisonSideState.NotStarted,
                    []),
                Paths = [],
                Dependencies = new ExtensionInspectDependencyComparison
                {
                    State = ExtensionInspectDependencyComparisonState.NotStarted,
                    Baseline = [],
                    Current = [],
                    Intended = [],
                    Relation = ExtensionInspectDependencyRelation.NotStarted,
                },
            },
            Generated = new ExtensionInspectGenerated
            {
                State = ExtensionInspectGeneratedState.NotStarted,
                Ownership = GeneratedOwnership,
                Regions = [],
            },
            Findings = input.Findings,
            Counts = new ExtensionInspectCounts
            {
                InstalledPackages = null,
                AvailablePackages = null,
                DeclaredPaths = null,
                CurrentPaths = null,
                BaselinePaths = null,
                IntendedPaths = null,
                Dependencies = null,
                UnchangedPaths = null,
                ChangedPaths = null,
                CurrentDivergedPaths = null,
                MissingPaths = null,
                NewPaths = null,
                RetiredPaths = null,
                SharedPaths = null,
                GeneratedRegions = null,
                ExcludedGeneratedBytes = null,
                Findings = input.Findings.Count,
            },
            Next = input.Next,
        };
    }
}
