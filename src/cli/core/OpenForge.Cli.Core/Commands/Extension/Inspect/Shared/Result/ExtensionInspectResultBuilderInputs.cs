using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Request;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal sealed record ExtensionInspectEventInput
{
    public required ExtensionInspectRequest Request { get; init; }

    public ExtensionSourceReadResult? Source { get; init; }

    public LifecycleReadResult? Lifecycle { get; init; }

    public IReadOnlyList<ExtensionInspectCurrentPath>? CurrentPaths { get; init; }

    public required CliSemanticStatus Status { get; init; }

    public required ExtensionInspectFindingCode Code { get; init; }

    public required string Cause { get; init; }
}

internal sealed record ExtensionInspectDependencyNode(
    string Id,
    IReadOnlyList<string> Dependencies,
    string? Version,
    string? Source);

internal sealed record ExtensionInspectComparisonInput
{
    public required LifecycleReadResult Lifecycle { get; init; }

    public required LifecycleInstalledPackage? InstalledPackage { get; init; }

    public required ExtensionPackageFact? AvailablePackage { get; init; }

    public required IReadOnlyList<ExtensionPackageFact> AvailableClosure { get; init; }

    public required ExtensionInspectDependencyClosure Dependencies { get; init; }

    public required IReadOnlyList<LifecycleInstalledPath> BaselineRecords { get; init; }

    public required IReadOnlyList<ExtensionInspectFingerprintFact> Baseline { get; init; }

    public required IReadOnlyList<ExtensionInspectFingerprintFact> Current { get; init; }

    public required IReadOnlyList<ExtensionInspectFingerprintFact> Intended { get; init; }

    public required ICollection<ExtensionInspectFinding> Findings { get; init; }
}

internal sealed record ExtensionInspectDeclaredPathProjection
{
    public required string Path { get; init; }

    public required string? SourcePath { get; init; }

    public required ExtensionInspectDeclaredPathState State { get; init; }

    public required IReadOnlyList<string> Owners { get; init; }

    public required IReadOnlyList<ExtensionPackageFileFact> Files { get; init; }

    public required bool HasConflict { get; init; }
}

internal sealed record ExtensionInspectPathFileInput
{
    public required string PackageId { get; init; }

    public required ExtensionPackageFileFact File { get; init; }
}

internal sealed record ExtensionInspectDependencyInput
{
    public required ExtensionSourceReadResult Source { get; init; }

    public required ExtensionPackageFact? SourcePackage { get; init; }

    public required IReadOnlyList<LifecycleInstalledPackage> InstalledPackages { get; init; }

    public required LifecycleInstalledPackage? InstalledPackage { get; init; }

    public required string SubjectId { get; init; }

    public required ICollection<ExtensionInspectFinding> Findings { get; init; }
}

internal sealed record ExtensionInspectCountsInput
{
    public required LifecycleReadResult Lifecycle { get; init; }

    public required ExtensionSourceReadResult Source { get; init; }

    public required LifecycleInstalledPackage? InstalledPackage { get; init; }

    public required ExtensionPackageFact? AvailablePackage { get; init; }

    public required ExtensionInspectDependencyClosure Dependencies { get; init; }

    public required ExtensionInspectPathFacts PathFacts { get; init; }

    public required ExtensionInspectComparison Comparison { get; init; }

    public required ExtensionInspectGenerated Generated { get; init; }

    public required int FindingCount { get; init; }
}
