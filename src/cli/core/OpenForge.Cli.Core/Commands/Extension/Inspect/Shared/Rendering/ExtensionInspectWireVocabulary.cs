using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Rendering;

internal static class ExtensionInspectWireVocabulary
{
    internal static string WorkspaceSelection(CliWorkspaceSelectionMethod workspace)
        => workspace switch
        {
            CliWorkspaceSelectionMethod.CurrentDirectory => "current-directory",
            CliWorkspaceSelectionMethod.ExplicitWorkspace => "explicit-workspace",
            _ => throw new ArgumentOutOfRangeException(nameof(workspace), workspace, "The workspace selection method is not defined."),
        };

    internal static string SubjectForm(ExtensionInspectSubjectForm value)
        => value switch
        {
            ExtensionInspectSubjectForm.StableId => "stable-id",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The subject form is not defined."),
        };

    internal static string SubjectState(ExtensionInspectSubjectState value)
        => value switch
        {
            ExtensionInspectSubjectState.NotStarted => "not-started",
            ExtensionInspectSubjectState.Resolved => "resolved",
            ExtensionInspectSubjectState.Invalid => "invalid",
            ExtensionInspectSubjectState.Unknown => "unknown",
            ExtensionInspectSubjectState.Ambiguous => "ambiguous",
            ExtensionInspectSubjectState.Unsafe => "unsafe",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The subject state is not defined."),
        };

    internal static string SourceKind(ExtensionInspectSourceKind value)
        => value switch
        {
            ExtensionInspectSourceKind.EmbeddedCatalogue => "embedded-catalogue",
            ExtensionInspectSourceKind.Package => "package",
            ExtensionInspectSourceKind.Catalogue => "catalogue",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The source kind is not defined."),
        };

    internal static string SourceState(ExtensionInspectSourceState value)
        => value switch
        {
            ExtensionInspectSourceState.NotStarted => "not-started",
            ExtensionInspectSourceState.Available => "available",
            ExtensionInspectSourceState.Missing => "missing",
            ExtensionInspectSourceState.Invalid => "invalid",
            ExtensionInspectSourceState.Blocked => "blocked",
            ExtensionInspectSourceState.Unavailable => "unavailable",
            ExtensionInspectSourceState.Failed => "failed",
            ExtensionInspectSourceState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The source state is not defined."),
        };

    internal static string LifecycleReadState(ExtensionInspectLifecycleReadState value)
        => value switch
        {
            ExtensionInspectLifecycleReadState.NotStarted => "not-started",
            ExtensionInspectLifecycleReadState.Complete => "complete",
            ExtensionInspectLifecycleReadState.Missing => "missing",
            ExtensionInspectLifecycleReadState.Invalid => "invalid",
            ExtensionInspectLifecycleReadState.Unavailable => "unavailable",
            ExtensionInspectLifecycleReadState.Blocked => "blocked",
            ExtensionInspectLifecycleReadState.Failed => "failed",
            ExtensionInspectLifecycleReadState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The lifecycle read state is not defined."),
        };

    internal static string LifecycleTrust(ExtensionInspectLifecycleTrust value)
        => value switch
        {
            ExtensionInspectLifecycleTrust.NotStarted => "not-started",
            ExtensionInspectLifecycleTrust.Trusted => "trusted",
            ExtensionInspectLifecycleTrust.Untrusted => "untrusted",
            ExtensionInspectLifecycleTrust.Incomplete => "incomplete",
            ExtensionInspectLifecycleTrust.Blocked => "blocked",
            ExtensionInspectLifecycleTrust.Absent => "absent",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The lifecycle trust is not defined."),
        };

    internal static string Coverage(ExtensionInspectCoverageState value)
        => value switch
        {
            ExtensionInspectCoverageState.NotStarted => "not-started",
            ExtensionInspectCoverageState.Complete => "complete",
            ExtensionInspectCoverageState.Incomplete => "incomplete",
            ExtensionInspectCoverageState.Invalid => "invalid",
            ExtensionInspectCoverageState.Blocked => "blocked",
            ExtensionInspectCoverageState.Failed => "failed",
            ExtensionInspectCoverageState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The coverage state is not defined."),
        };

    internal static string WorkspaceBinding(ExtensionInspectWorkspaceBinding value)
        => value switch
        {
            ExtensionInspectWorkspaceBinding.NotChecked => "not-checked",
            ExtensionInspectWorkspaceBinding.Matched => "matched",
            ExtensionInspectWorkspaceBinding.Mismatched => "mismatched",
            ExtensionInspectWorkspaceBinding.Unavailable => "unavailable",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The workspace binding is not defined."),
        };

    internal static string InstalledState(ExtensionInspectInstalledState value)
        => value switch
        {
            ExtensionInspectInstalledState.NotStarted => "not-started",
            ExtensionInspectInstalledState.Present => "present",
            ExtensionInspectInstalledState.Absent => "absent",
            ExtensionInspectInstalledState.Unavailable => "unavailable",
            ExtensionInspectInstalledState.Invalid => "invalid",
            ExtensionInspectInstalledState.Blocked => "blocked",
            ExtensionInspectInstalledState.Failed => "failed",
            ExtensionInspectInstalledState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The installed state is not defined."),
        };

    internal static string AvailableState(ExtensionInspectAvailableState value)
        => value switch
        {
            ExtensionInspectAvailableState.NotStarted => "not-started",
            ExtensionInspectAvailableState.Present => "present",
            ExtensionInspectAvailableState.Absent => "absent",
            ExtensionInspectAvailableState.Unavailable => "unavailable",
            ExtensionInspectAvailableState.Invalid => "invalid",
            ExtensionInspectAvailableState.Blocked => "blocked",
            ExtensionInspectAvailableState.Failed => "failed",
            ExtensionInspectAvailableState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The available state is not defined."),
        };

    internal static string PackageFileState(ExtensionInspectPackageFileState value)
        => value switch
        {
            ExtensionInspectPackageFileState.NotStarted => "not-started",
            ExtensionInspectPackageFileState.Available => "available",
            ExtensionInspectPackageFileState.Missing => "missing",
            ExtensionInspectPackageFileState.Invalid => "invalid",
            ExtensionInspectPackageFileState.Blocked => "blocked",
            ExtensionInspectPackageFileState.Unavailable => "unavailable",
            ExtensionInspectPackageFileState.Failed => "failed",
            ExtensionInspectPackageFileState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The package file state is not defined."),
        };

    internal static string DependencyState(ExtensionInspectDependencyState value)
        => value switch
        {
            ExtensionInspectDependencyState.NotStarted => "not-started",
            ExtensionInspectDependencyState.Complete => "complete",
            ExtensionInspectDependencyState.Incomplete => "incomplete",
            ExtensionInspectDependencyState.Invalid => "invalid",
            ExtensionInspectDependencyState.Blocked => "blocked",
            ExtensionInspectDependencyState.Failed => "failed",
            ExtensionInspectDependencyState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The dependency state is not defined."),
        };

    internal static string DependencyPackageState(ExtensionInspectDependencyPackageState value)
        => value switch
        {
            ExtensionInspectDependencyPackageState.NotStarted => "not-started",
            ExtensionInspectDependencyPackageState.Available => "available",
            ExtensionInspectDependencyPackageState.Missing => "missing",
            ExtensionInspectDependencyPackageState.Invalid => "invalid",
            ExtensionInspectDependencyPackageState.Blocked => "blocked",
            ExtensionInspectDependencyPackageState.Unavailable => "unavailable",
            ExtensionInspectDependencyPackageState.Failed => "failed",
            ExtensionInspectDependencyPackageState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The dependency package state is not defined."),
        };

    internal static string PathState(ExtensionInspectPathState value)
        => value switch
        {
            ExtensionInspectPathState.NotStarted => "not-started",
            ExtensionInspectPathState.Complete => "complete",
            ExtensionInspectPathState.Incomplete => "incomplete",
            ExtensionInspectPathState.Invalid => "invalid",
            ExtensionInspectPathState.Blocked => "blocked",
            ExtensionInspectPathState.Failed => "failed",
            ExtensionInspectPathState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The path state is not defined."),
        };

    internal static string DeclaredPathState(ExtensionInspectDeclaredPathState value)
        => value switch
        {
            ExtensionInspectDeclaredPathState.NotStarted => "not-started",
            ExtensionInspectDeclaredPathState.Available => "available",
            ExtensionInspectDeclaredPathState.Missing => "missing",
            ExtensionInspectDeclaredPathState.Invalid => "invalid",
            ExtensionInspectDeclaredPathState.Blocked => "blocked",
            ExtensionInspectDeclaredPathState.Unavailable => "unavailable",
            ExtensionInspectDeclaredPathState.Failed => "failed",
            ExtensionInspectDeclaredPathState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The declared path state is not defined."),
        };

    internal static string CurrentPathState(ExtensionInspectCurrentPathState value)
        => value switch
        {
            ExtensionInspectCurrentPathState.NotStarted => "not-started",
            ExtensionInspectCurrentPathState.Present => "present",
            ExtensionInspectCurrentPathState.Missing => "missing",
            ExtensionInspectCurrentPathState.Invalid => "invalid",
            ExtensionInspectCurrentPathState.Blocked => "blocked",
            ExtensionInspectCurrentPathState.Unavailable => "unavailable",
            ExtensionInspectCurrentPathState.Failed => "failed",
            ExtensionInspectCurrentPathState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The current path state is not defined."),
        };

    internal static string ComparisonState(ExtensionInspectComparisonState value)
        => value switch
        {
            ExtensionInspectComparisonState.NotStarted => "not-started",
            ExtensionInspectComparisonState.Complete => "complete",
            ExtensionInspectComparisonState.Incomplete => "incomplete",
            ExtensionInspectComparisonState.Invalid => "invalid",
            ExtensionInspectComparisonState.Blocked => "blocked",
            ExtensionInspectComparisonState.Failed => "failed",
            ExtensionInspectComparisonState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The comparison state is not defined."),
        };

    internal static string ComparisonMode(ExtensionInspectComparisonMode value)
        => value switch
        {
            ExtensionInspectComparisonMode.None => "none",
            ExtensionInspectComparisonMode.InstalledOnly => "installed-only",
            ExtensionInspectComparisonMode.AvailableOnly => "available-only",
            ExtensionInspectComparisonMode.ThreeWay => "three-way",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The comparison mode is not defined."),
        };

    internal static string ComparisonSideState(ExtensionInspectComparisonSideState value)
        => value switch
        {
            ExtensionInspectComparisonSideState.NotStarted => "not-started",
            ExtensionInspectComparisonSideState.NotApplicable => "not-applicable",
            ExtensionInspectComparisonSideState.Available => "available",
            ExtensionInspectComparisonSideState.Unavailable => "unavailable",
            ExtensionInspectComparisonSideState.Invalid => "invalid",
            ExtensionInspectComparisonSideState.Blocked => "blocked",
            ExtensionInspectComparisonSideState.Failed => "failed",
            ExtensionInspectComparisonSideState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The comparison side state is not defined."),
        };

    internal static string FingerprintKind(ExtensionInspectFingerprintKind value)
        => value switch
        {
            ExtensionInspectFingerprintKind.Semantic => "semantic",
            ExtensionInspectFingerprintKind.ExactBytes => "exact-bytes",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The fingerprint kind is not defined."),
        };

    internal static string FingerprintOrigin(ExtensionInspectFingerprintOrigin value)
        => value switch
        {
            ExtensionInspectFingerprintOrigin.PersistedBaseline => "persisted-baseline",
            ExtensionInspectFingerprintOrigin.OperationTimeCurrent => "operation-time-current",
            ExtensionInspectFingerprintOrigin.OperationTimeIntended => "operation-time-intended",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The fingerprint origin is not defined."),
        };

    internal static string PathRelation(ExtensionInspectPathRelation value)
        => value switch
        {
            ExtensionInspectPathRelation.NotStarted => "not-started",
            ExtensionInspectPathRelation.NotApplicable => "not-applicable",
            ExtensionInspectPathRelation.Unchanged => "unchanged",
            ExtensionInspectPathRelation.Changed => "changed",
            ExtensionInspectPathRelation.CurrentDiverged => "current-diverged",
            ExtensionInspectPathRelation.Missing => "missing",
            ExtensionInspectPathRelation.New => "new",
            ExtensionInspectPathRelation.Retired => "retired",
            ExtensionInspectPathRelation.Shared => "shared",
            ExtensionInspectPathRelation.Unknown => "unknown",
            ExtensionInspectPathRelation.Unavailable => "unavailable",
            ExtensionInspectPathRelation.Invalid => "invalid",
            ExtensionInspectPathRelation.Blocked => "blocked",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The path relation is not defined."),
        };

    internal static string DependencyComparisonState(ExtensionInspectDependencyComparisonState value)
        => value switch
        {
            ExtensionInspectDependencyComparisonState.NotStarted => "not-started",
            ExtensionInspectDependencyComparisonState.NotApplicable => "not-applicable",
            ExtensionInspectDependencyComparisonState.Available => "available",
            ExtensionInspectDependencyComparisonState.Unavailable => "unavailable",
            ExtensionInspectDependencyComparisonState.Invalid => "invalid",
            ExtensionInspectDependencyComparisonState.Blocked => "blocked",
            ExtensionInspectDependencyComparisonState.Failed => "failed",
            ExtensionInspectDependencyComparisonState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The dependency comparison state is not defined."),
        };

    internal static string DependencyRelation(ExtensionInspectDependencyRelation value)
        => value switch
        {
            ExtensionInspectDependencyRelation.NotStarted => "not-started",
            ExtensionInspectDependencyRelation.NotApplicable => "not-applicable",
            ExtensionInspectDependencyRelation.Equal => "equal",
            ExtensionInspectDependencyRelation.Changed => "changed",
            ExtensionInspectDependencyRelation.Unavailable => "unavailable",
            ExtensionInspectDependencyRelation.Invalid => "invalid",
            ExtensionInspectDependencyRelation.Blocked => "blocked",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The dependency relation is not defined."),
        };

    internal static string GeneratedState(ExtensionInspectGeneratedState value)
        => value switch
        {
            ExtensionInspectGeneratedState.NotStarted => "not-started",
            ExtensionInspectGeneratedState.Complete => "complete",
            ExtensionInspectGeneratedState.Incomplete => "incomplete",
            ExtensionInspectGeneratedState.Invalid => "invalid",
            ExtensionInspectGeneratedState.Blocked => "blocked",
            ExtensionInspectGeneratedState.Failed => "failed",
            ExtensionInspectGeneratedState.Interrupted => "interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The generated state is not defined."),
        };

    internal static string GeneratedRegionState(ExtensionInspectGeneratedRegionState value)
        => value switch
        {
            ExtensionInspectGeneratedRegionState.NotStarted => "not-started",
            ExtensionInspectGeneratedRegionState.Valid => "valid",
            ExtensionInspectGeneratedRegionState.Absent => "absent",
            ExtensionInspectGeneratedRegionState.Invalid => "invalid",
            ExtensionInspectGeneratedRegionState.Ambiguous => "ambiguous",
            ExtensionInspectGeneratedRegionState.Unavailable => "unavailable",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The generated region state is not defined."),
        };
}
