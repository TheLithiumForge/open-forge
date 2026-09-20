using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;

internal enum ExtensionInspectFindingCode
{
    InvalidInput,
    InvalidStableId,
    WorkspaceUnavailable,
    WorkspaceUnsafe,
    SourceUnavailable,
    SourceInvalid,
    SourceOverlap,
    SourceAmbiguous,
    IdentityAmbiguous,
    OwnershipObservation,
    PackageUnavailable,
    PackageInvalid,
    DependencyIncomplete,
    DependencyCycle,
    DependencyConflict,
    PathUnavailable,
    PathInvalid,
    OwnershipConflict,
    FingerprintUnavailable,
    FingerprintFallback,
    GeneratedBoundaryInvalid,
    DependencyChanged,
    PathChanged,
    PathMissing,
    PathNew,
    PathRetired,
    OperationFailed,
    Interrupted,
}

internal sealed record ExtensionInspectFinding
{
    public required ExtensionInspectFindingCode Code { get; init; }

    public required CliSemanticStatus Status { get; init; }

    public required string? Subject { get; init; }

    public required string? PackageId { get; init; }

    public required string? Dependency { get; init; }

    public required string? Path { get; init; }

    public required string Cause { get; init; }

    public required SourceLocation? Location { get; init; }

    public required IReadOnlyList<ExtensionInspectCandidate> Candidates { get; init; }

    internal string MachineCode => ExtensionInspectDefinitions.ReadFindingCode(Code);
}

internal sealed record ExtensionInspectCounts
{
    public required int? InstalledPackages { get; init; }

    public required int? AvailablePackages { get; init; }

    public required int? DeclaredPaths { get; init; }

    public required int? CurrentPaths { get; init; }


    public required int? IntendedPaths { get; init; }

    public required int? Dependencies { get; init; }

    public required int? UnchangedPaths { get; init; }

    public required int? ChangedPaths { get; init; }


    public required int? MissingPaths { get; init; }

    public required int? NewPaths { get; init; }

    public required int? RetiredPaths { get; init; }

    public required int? SharedPaths { get; init; }

    public required int? GeneratedRegions { get; init; }

    public required int? ExcludedGeneratedBytes { get; init; }

    public required int Findings { get; init; }
}

internal sealed record ExtensionInspectResult : ICliCommandResult
{
    public required CliSemanticStatus Status { get; init; }

    public required CliWorkspace? Workspace { get; init; }

    internal string? WorkspacePath => Workspace?.LexicalRoot;

    internal bool WorkspaceExplicit => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;

    public required ExtensionInspectSubject Subject { get; init; }

    public required ExtensionInspectSource Source { get; init; }

    public required ExtensionInspectLifecycle Lifecycle { get; init; }

    public required ExtensionInspectInstalled Installed { get; init; }

    public required ExtensionInspectAvailable Available { get; init; }

    public required ExtensionInspectDependencyClosure Dependencies { get; init; }

    public required ExtensionInspectPathFacts PathFacts { get; init; }

    public required ExtensionInspectComparison Comparison { get; init; }

    public required ExtensionInspectGenerated Generated { get; init; }

    public required IReadOnlyList<ExtensionInspectFinding> Findings { get; init; }

    public required ExtensionInspectCounts Counts { get; init; }

    public required CliNextAction? Next { get; init; }

    public string Command => ExtensionInspectDefinitions.CommandIdentity;
}
