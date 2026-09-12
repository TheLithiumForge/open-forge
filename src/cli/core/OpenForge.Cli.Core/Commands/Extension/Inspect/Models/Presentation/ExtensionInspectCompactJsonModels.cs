namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Presentation;

internal sealed class ExtensionInspectCompactJsonResult
{
    public required ExtensionInspectJsonSubject Subject { get; init; }

    public required ExtensionInspectJsonSource Source { get; init; }

    public required ExtensionInspectJsonLifecycle Lifecycle { get; init; }

    public required ExtensionInspectJsonInstalled Installed { get; init; }

    public required ExtensionInspectJsonAvailable Available { get; init; }

    public required ExtensionInspectJsonDependencyClosure Dependencies { get; init; }

    public required ExtensionInspectJsonPathFacts PathFacts { get; init; }

    public required ExtensionInspectCompactJsonComparison Comparison { get; init; }

    public required ExtensionInspectJsonGenerated Generated { get; init; }

    public required ExtensionInspectJsonFinding[] Findings { get; init; }

    public required ExtensionInspectJsonCounts Counts { get; init; }
}

internal sealed class ExtensionInspectCompactJsonComparison
{
    public required string State { get; init; }

    public required string Mode { get; init; }

    public required ExtensionInspectCompactJsonComparisonSide Baseline { get; init; }

    public required ExtensionInspectCompactJsonComparisonSide Current { get; init; }

    public required ExtensionInspectCompactJsonComparisonSide Intended { get; init; }

    public required ExtensionInspectCompactJsonPathComparison[] Paths { get; init; }

    public required ExtensionInspectJsonDependencyComparison Dependencies { get; init; }
}

internal sealed class ExtensionInspectCompactJsonComparisonSide
{
    public required string State { get; init; }

}

internal sealed class ExtensionInspectCompactJsonPathComparison
{
    public required string Path { get; init; }




    public required string Relation { get; init; }

    public required string[] BaselineOwners { get; init; }

    public required string[] CurrentOwners { get; init; }

    public required string[] IntendedOwners { get; init; }
}
