using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect;

internal static class ExtensionInspectDefinitions
{
    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "extension inspect";
    internal const string FingerprintPolicy = MarkdownFingerprintPolicy.Name;

    internal static readonly CliSyntaxDefinition InspectCommand = new(
        name: "inspect",
        description: "Inspect one installed or available Extension package.");

    internal static readonly CliOptionDefinition<string?> Source = new(
        name: "--source",
        description: "Read one exact local package or catalogue source.",
        arity: CliOptionArity.ExactlyOne,
        defaultValue: null,
        valueName: "package-or-catalogue-path");

    internal static readonly CliNextAction IncompleteNext = new(
        "open-forge doctor",
        "Inspect unavailable lifecycle, source, dependency, path, or fingerprint facts before relying on this result.");

    internal static readonly CliNextAction InvalidNext = new(
        "open-forge extension inspect --help",
        "Correct the named Extension Inspect input, then rerun the request.");

    internal static readonly CliNextAction BlockedNext = new(
        "open-forge doctor",
        "Inspect the blocked workspace, source, identity, or ownership boundary before rerunning Extension Inspect.");

    internal static readonly CliNextAction FailedNext = new(
        "open-forge extension inspect --verbose",
        "Report the failure and retry Extension Inspect with bounded diagnostics.");

    internal static readonly CliNextAction InterruptedNext = new(
        "open-forge extension inspect",
        "Rerun the same Extension Inspect request.");

    internal static CliNextAction UpdateNext(string subjectId)
        => new(
            $"open-forge extension update {subjectId}",
            "Apply the trusted current-source change for this stable ID with the explicit update command.");

    internal static string ReadFindingCode(ExtensionInspectFindingCode code)
        => code switch
        {
            ExtensionInspectFindingCode.InvalidInput => "extension-inspect.invalid-input",
            ExtensionInspectFindingCode.InvalidStableId => "extension-inspect.invalid-stable-id",
            ExtensionInspectFindingCode.WorkspaceUnavailable => "extension-inspect.workspace-unavailable",
            ExtensionInspectFindingCode.WorkspaceUnsafe => "extension-inspect.workspace-unsafe",
            ExtensionInspectFindingCode.SourceUnavailable => "extension-inspect.source-unavailable",
            ExtensionInspectFindingCode.SourceInvalid => "extension-inspect.source-invalid",
            ExtensionInspectFindingCode.SourceOverlap => "extension-inspect.source-overlap",
            ExtensionInspectFindingCode.SourceAmbiguous => "extension-inspect.source-ambiguous",
            ExtensionInspectFindingCode.IdentityAmbiguous => "extension-inspect.identity-ambiguous",
            ExtensionInspectFindingCode.LifecycleUnavailable => "extension-inspect.lifecycle-unavailable",
            ExtensionInspectFindingCode.LifecycleInvalid => "extension-inspect.lifecycle-invalid",
            ExtensionInspectFindingCode.LifecycleBlocked => "extension-inspect.lifecycle-blocked",
            ExtensionInspectFindingCode.PackageUnavailable => "extension-inspect.package-unavailable",
            ExtensionInspectFindingCode.PackageInvalid => "extension-inspect.package-invalid",
            ExtensionInspectFindingCode.DependencyIncomplete => "extension-inspect.dependency-incomplete",
            ExtensionInspectFindingCode.DependencyCycle => "extension-inspect.dependency-cycle",
            ExtensionInspectFindingCode.DependencyConflict => "extension-inspect.dependency-conflict",
            ExtensionInspectFindingCode.PathUnavailable => "extension-inspect.path-unavailable",
            ExtensionInspectFindingCode.PathInvalid => "extension-inspect.path-invalid",
            ExtensionInspectFindingCode.OwnershipConflict => "extension-inspect.ownership-conflict",
            ExtensionInspectFindingCode.FingerprintUnavailable => "extension-inspect.fingerprint-unavailable",
            ExtensionInspectFindingCode.FingerprintFallback => "extension-inspect.fingerprint-fallback",
            ExtensionInspectFindingCode.GeneratedBoundaryInvalid => "extension-inspect.generated-boundary-invalid",
            ExtensionInspectFindingCode.DependencyChanged => "extension-inspect.dependency-changed",
            ExtensionInspectFindingCode.PathChanged => "extension-inspect.path-changed",
            ExtensionInspectFindingCode.PathCurrentDiverged => "extension-inspect.path-current-diverged",
            ExtensionInspectFindingCode.PathMissing => "extension-inspect.path-missing",
            ExtensionInspectFindingCode.PathNew => "extension-inspect.path-new",
            ExtensionInspectFindingCode.PathRetired => "extension-inspect.path-retired",
            ExtensionInspectFindingCode.OperationFailed => "extension-inspect.operation-failed",
            ExtensionInspectFindingCode.Interrupted => "extension-inspect.interrupted",
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Extension Inspect finding code is not defined."),
        };

    internal static CliSemanticStatus ReadFindingStatus(ExtensionInspectFindingCode code)
        => code switch
        {
            ExtensionInspectFindingCode.InvalidInput or ExtensionInspectFindingCode.InvalidStableId
                => CliSemanticStatus.Invalid,
            ExtensionInspectFindingCode.WorkspaceUnavailable or ExtensionInspectFindingCode.WorkspaceUnsafe
                or ExtensionInspectFindingCode.SourceOverlap or ExtensionInspectFindingCode.SourceAmbiguous
                or ExtensionInspectFindingCode.IdentityAmbiguous or ExtensionInspectFindingCode.LifecycleBlocked
                or ExtensionInspectFindingCode.DependencyCycle or ExtensionInspectFindingCode.DependencyConflict
                or ExtensionInspectFindingCode.PathInvalid or ExtensionInspectFindingCode.OwnershipConflict
                => CliSemanticStatus.Blocked,
            ExtensionInspectFindingCode.SourceUnavailable or ExtensionInspectFindingCode.DependencyChanged
                or ExtensionInspectFindingCode.PathChanged or ExtensionInspectFindingCode.PathCurrentDiverged
                or ExtensionInspectFindingCode.PathMissing or ExtensionInspectFindingCode.PathNew
                or ExtensionInspectFindingCode.PathRetired
                => CliSemanticStatus.Attention,
            ExtensionInspectFindingCode.SourceInvalid or ExtensionInspectFindingCode.LifecycleUnavailable
                or ExtensionInspectFindingCode.LifecycleInvalid or ExtensionInspectFindingCode.PackageUnavailable
                or ExtensionInspectFindingCode.PackageInvalid or ExtensionInspectFindingCode.DependencyIncomplete
                or ExtensionInspectFindingCode.PathUnavailable or ExtensionInspectFindingCode.FingerprintUnavailable
                or ExtensionInspectFindingCode.FingerprintFallback or ExtensionInspectFindingCode.GeneratedBoundaryInvalid
                => CliSemanticStatus.Incomplete,
            ExtensionInspectFindingCode.OperationFailed => CliSemanticStatus.Failed,
            ExtensionInspectFindingCode.Interrupted => CliSemanticStatus.Interrupted,
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The Extension Inspect finding code is not defined."),
        };

    internal static CliNextAction? ReadNext(
        CliSemanticStatus status,
        bool actionable,
        string? subjectId)
        => status switch
        {
            CliSemanticStatus.Complete => null,
            CliSemanticStatus.Attention when actionable && subjectId is not null => UpdateNext(subjectId),
            CliSemanticStatus.Attention => null,
            CliSemanticStatus.Incomplete => IncompleteNext,
            CliSemanticStatus.Invalid => InvalidNext,
            CliSemanticStatus.Blocked => BlockedNext,
            CliSemanticStatus.Failed => FailedNext,
            CliSemanticStatus.Interrupted => InterruptedNext,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Extension Inspect status is not defined."),
        };
}
