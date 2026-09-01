using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;

internal sealed record ExtensionInspectComparisonBuildInput
{
    public required LifecycleReadResult Lifecycle { get; init; }

    public required LifecycleInstalledPackage? InstalledPackage { get; init; }

    public required ExtensionPackageFact? AvailablePackage { get; init; }

    public required IReadOnlyList<ExtensionPackageFact> AvailableClosure { get; init; }

    public required ExtensionInspectDependencyClosure Dependencies { get; init; }

    public required ExtensionInspectPathFacts PathFacts { get; init; }

    public required IReadOnlyList<ExtensionInspectDeclaredPathProjection> SourcePathProjections { get; init; }

    public required ICollection<ExtensionInspectFinding> Findings { get; init; }
}

internal sealed record ExtensionInspectComparisonFacts
{
    public required ExtensionInspectGenerated Generated { get; init; }

    public required ExtensionInspectComparison Comparison { get; init; }
}

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

internal sealed record ExtensionInspectPathComparisonInput
{
    public required ExtensionInspectComparisonInput ComparisonFacts { get; init; }

    public required ExtensionInspectComparisonMode Mode { get; init; }

    public required IReadOnlyList<LifecycleInstalledPackage> InstalledClosure { get; init; }
}

internal sealed record ExtensionInspectDependencyComparisonInput
{
    public required ExtensionInspectComparisonMode Mode { get; init; }

    public required LifecycleInstalledPackage? InstalledPackage { get; init; }

    public required IReadOnlyList<LifecycleInstalledPackage> InstalledClosure { get; init; }

    public required ExtensionInspectDependencyClosure Dependencies { get; init; }

    public required ICollection<ExtensionInspectFinding> Findings { get; init; }
}

internal sealed record ExtensionInspectActionabilityInput
{
    public required ExtensionInspectComparison Comparison { get; init; }

    public required ExtensionInspectGenerated Generated { get; init; }

    public required LifecycleReadResult Lifecycle { get; init; }

    public required ExtensionSourceReadResult Source { get; init; }

    public required IReadOnlyList<ExtensionInspectFinding> Findings { get; init; }
}
