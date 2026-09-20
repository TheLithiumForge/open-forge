using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal static class ExtensionInspectCountBuilder
{
    internal static ExtensionInspectCounts Build(ExtensionInspectCountsInput input)
    {
        var pathComparisons = input.Comparison.Paths;
        return new ExtensionInspectCounts
        {
            InstalledPackages = ReadInstalledPackageCount(input.Ownership),
            AvailablePackages = ReadAvailablePackageCount(input.Source),
            DeclaredPaths = input.AvailablePackage is null ? null : input.PathFacts.Declared.Count,
            CurrentPaths = input.InstalledPackage is null ? null : input.PathFacts.Current.Count,
            IntendedPaths = input.AvailablePackage is null ? null : input.PathFacts.Declared.Count,
            Dependencies = input.Dependencies.State == ExtensionInspectDependencyState.Complete
                ? input.Dependencies.Resolved.Count
                : null,
            UnchangedPaths = ReadPathCount(pathComparisons, input.Comparison.State, ExtensionInspectPathRelation.Unchanged),
            ChangedPaths = ReadPathCount(pathComparisons, input.Comparison.State, ExtensionInspectPathRelation.Changed),
            MissingPaths = ReadPathCount(pathComparisons, input.Comparison.State, ExtensionInspectPathRelation.Missing),
            NewPaths = ReadPathCount(pathComparisons, input.Comparison.State, ExtensionInspectPathRelation.New),
            RetiredPaths = ReadPathCount(pathComparisons, input.Comparison.State, ExtensionInspectPathRelation.Retired),
            SharedPaths = ReadPathCount(pathComparisons, input.Comparison.State, ExtensionInspectPathRelation.Shared),
            GeneratedRegions = ReadGeneratedRegionCount(input.Generated),
            ExcludedGeneratedBytes = ReadExcludedGeneratedBytes(input.Generated),
            Findings = input.FindingCount,
        };
    }

    private static int? ReadInstalledPackageCount(WorkspaceOwnershipRead lifecycle)
    {
        if (!lifecycle.IsTrustworthy)
        {
            return null;
        }

        return lifecycle.Document.Extensions.Length;
    }

    private static int? ReadAvailablePackageCount(ExtensionSourceReadResult source)
    {
        if (source.State != ExtensionSourceReadState.Complete)
        {
            return null;
        }

        return source.Packages.Count;
    }

    private static int? ReadPathCount(
        IReadOnlyList<ExtensionInspectPathComparison> paths,
        ExtensionInspectComparisonState comparisonState,
        ExtensionInspectPathRelation relation)
    {
        if (paths.Count == 0)
        {
            return comparisonState == ExtensionInspectComparisonState.Complete ? 0 : null;
        }

        return paths.Count(path => path.Relation == relation);
    }

    private static int? ReadGeneratedRegionCount(ExtensionInspectGenerated generated)
    {
        if (generated.State != ExtensionInspectGeneratedState.Complete)
        {
            return null;
        }

        return generated.Regions.Count(region => region.State == ExtensionInspectGeneratedRegionState.Valid);
    }

    private static int? ReadExcludedGeneratedBytes(ExtensionInspectGenerated generated)
    {
        if (generated.State != ExtensionInspectGeneratedState.Complete)
        {
            return null;
        }

        return generated.Regions
            .Where(region => region.ExcludedInteriorByteLength is not null)
            .Sum(region => region.ExcludedInteriorByteLength ?? 0);
    }
}
