using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal static class ExtensionInspectDependencyComparisonBuilder
{
    internal static ExtensionInspectDependencyComparison Build(
        ExtensionInspectDependencyComparisonInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.Mode == ExtensionInspectComparisonMode.None)
        {
            return new ExtensionInspectDependencyComparison
            {
                State = ExtensionInspectDependencyComparisonState.NotStarted,
                Baseline = [],
                Current = [],
                Intended = [],
                Relation = ExtensionInspectDependencyRelation.NotStarted,
            };
        }

        var baseline = input.InstalledPackage is null
            ? []
            : input.InstalledClosure
                .Select(package => package.Id)
                .Order(StringComparer.Ordinal)
                .ToArray();
        var current = baseline;
        var intended = input.Mode == ExtensionInspectComparisonMode.ThreeWay
            ? input.Dependencies.Resolved
                .Where(package => package.State == ExtensionInspectDependencyPackageState.Available)
                .Select(package => package.Id)
                .Order(StringComparer.Ordinal)
                .ToArray()
            : [];
        var intendedKnown = input.Dependencies.State == ExtensionInspectDependencyState.Complete;
        var state = ReadState(input.Mode, intendedKnown);
        var relation = ReadRelation(input.Mode, intendedKnown, baseline, intended, input.Findings);
        return new ExtensionInspectDependencyComparison
        {
            State = state,
            Baseline = baseline,
            Current = current,
            Intended = intended,
            Relation = relation,
        };
    }

    private static ExtensionInspectDependencyComparisonState ReadState(
        ExtensionInspectComparisonMode mode,
        bool intendedKnown)
    {
        if (mode == ExtensionInspectComparisonMode.AvailableOnly)
        {
            return ExtensionInspectDependencyComparisonState.NotApplicable;
        }

        return intendedKnown
            ? ExtensionInspectDependencyComparisonState.Available
            : ExtensionInspectDependencyComparisonState.Unavailable;
    }

    private static ExtensionInspectDependencyRelation ReadRelation(
        ExtensionInspectComparisonMode mode,
        bool intendedKnown,
        IReadOnlyList<string> baseline,
        IReadOnlyList<string> intended,
        ICollection<ExtensionInspectFinding> findings)
    {
        if (mode != ExtensionInspectComparisonMode.ThreeWay)
        {
            return ExtensionInspectDependencyRelation.NotApplicable;
        }

        if (!intendedKnown)
        {
            return ExtensionInspectDependencyRelation.Unavailable;
        }

        if (baseline.SequenceEqual(intended, StringComparer.Ordinal))
        {
            return ExtensionInspectDependencyRelation.Equal;
        }

        ExtensionInspectFindingPolicy.Add(findings, new ExtensionInspectFindingInput
        {
            Code = ExtensionInspectFindingCode.DependencyChanged,
            Cause = "The intended dependency closure differs from the current installed dependency identity.",
        });
        return ExtensionInspectDependencyRelation.Changed;
    }
}
