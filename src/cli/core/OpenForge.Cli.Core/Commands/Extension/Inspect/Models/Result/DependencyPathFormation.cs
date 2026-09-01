using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;

internal sealed record ExtensionInspectDependencyPathInput
{
    public required ExtensionSourceReadResult Source { get; init; }

    public required ExtensionPackageFact? AvailablePackage { get; init; }

    public required IReadOnlyList<LifecycleInstalledPackage> InstalledPackages { get; init; }

    public required LifecycleInstalledPackage? InstalledPackage { get; init; }

    public required string SubjectId { get; init; }

    public required IReadOnlyList<ExtensionInspectCurrentPath> CurrentSnapshot { get; init; }

    public required ICollection<ExtensionInspectFinding> Findings { get; init; }
}

internal sealed record ExtensionInspectDependencyPathFacts
{
    public required ExtensionInspectDependencyClosure Dependencies { get; init; }

    public required IReadOnlyList<ExtensionPackageFact> AvailableClosure { get; init; }

    public required IReadOnlyList<ExtensionInspectDeclaredPathProjection> SourcePathProjections { get; init; }

    public required ExtensionInspectPathFacts PathFacts { get; init; }
}

internal sealed record ExtensionInspectDependencyNode(
    string Id,
    IReadOnlyList<string> Dependencies,
    string? Version,
    string? Source);

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
