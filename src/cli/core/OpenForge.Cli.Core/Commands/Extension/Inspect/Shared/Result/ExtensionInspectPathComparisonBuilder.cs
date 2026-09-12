using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal static class ExtensionInspectPathComparisonBuilder
{
    internal static IReadOnlyList<ExtensionInspectPathComparison> Build(
        ExtensionInspectPathComparisonInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var facts = input.ComparisonFacts;
        var baselineRecords = facts.BaselineRecords;
        var baseline = facts.Baseline;
        var current = facts.Current;
        var intended = facts.Intended;
        var availableClosure = facts.AvailableClosure;
        var findings = facts.Findings;
        var baselineByPath = baseline.ToDictionary(fact => fact.Path, StringComparer.Ordinal);
        var currentByPath = current.ToDictionary(fact => fact.Path, StringComparer.Ordinal);
        var intendedByPath = intended.ToDictionary(fact => fact.Path, StringComparer.Ordinal);
        var baselineOwners = baselineRecords.ToDictionary(
            record => record.Path,
            record => record.Owners.Order(StringComparer.Ordinal).ToArray(),
            StringComparer.Ordinal);
        var currentOwners = new Dictionary<string, string[]>(StringComparer.Ordinal);
        foreach (var package in input.InstalledClosure)
        {
            foreach (var path in package.Paths)
            {
                if (!currentOwners.TryGetValue(path, out var owners))
                {
                    currentOwners[path] = [package.Id];
                }
                else if (!owners.Contains(package.Id, StringComparer.Ordinal))
                {
                    currentOwners[path] = [.. owners.Append(package.Id).Order(StringComparer.Ordinal)];
                }
            }
        }

        var intendedOwners = new Dictionary<string, string[]>(StringComparer.Ordinal);
        foreach (var package in availableClosure)
        {
            foreach (var file in package.Payload.Where(file => file.TargetPath is not null))
            {
                if (file.TargetPath is not { } targetPath)
                {
                    continue;
                }

                var owner = package.Id;
                if (!intendedOwners.TryGetValue(targetPath, out var owners))
                {
                    intendedOwners[targetPath] = [owner];
                }
                else if (!owners.Contains(owner, StringComparer.Ordinal))
                {
                    intendedOwners[targetPath] = [.. owners.Append(owner).Order(StringComparer.Ordinal)];
                }
            }
        }

        var paths = baselineByPath.Keys
            .Concat(currentByPath.Keys)
            .Concat(intendedByPath.Keys)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var values = new List<ExtensionInspectPathComparison>(paths.Length);
        foreach (var path in paths)
        {
            baselineByPath.TryGetValue(path, out var baselineFact);
            currentByPath.TryGetValue(path, out var currentFact);
            intendedByPath.TryGetValue(path, out var intendedFact);
            baselineOwners.TryGetValue(path, out var oldOwners);
            currentOwners.TryGetValue(path, out var nowOwners);
            intendedOwners.TryGetValue(path, out var newOwners);
            oldOwners ??= [];
            nowOwners ??= [];
            newOwners ??= [];
            var relation = ReadPathRelation(
                input.Mode,
                baselineFact?.Fingerprint,
                currentFact?.Fingerprint,
                intendedFact?.Fingerprint,
                findings,
                path);
            if (relation == ExtensionInspectPathRelation.Unchanged
                && (oldOwners.Length > 1 || nowOwners.Length > 1 || newOwners.Length > 1))
            {
                relation = ExtensionInspectPathRelation.Shared;
            }
            values.Add(new ExtensionInspectPathComparison
            {
                Path = path,
                Baseline = baselineFact?.Fingerprint,
                Current = currentFact?.Fingerprint,
                Intended = intendedFact?.Fingerprint,
                Relation = relation,
                BaselineOwners = oldOwners,
                CurrentOwners = nowOwners,
                IntendedOwners = newOwners,
            });
        }

        return values;
    }

    private static ExtensionInspectPathRelation ReadPathRelation(
        ExtensionInspectComparisonMode mode,
        ExtensionInspectFingerprint? baseline,
        ExtensionInspectFingerprint? current,
        ExtensionInspectFingerprint? intended,
        ICollection<ExtensionInspectFinding> findings,
        string path)
    {
        if (mode == ExtensionInspectComparisonMode.None)
        {
            return ExtensionInspectPathRelation.NotStarted;
        }

        if (mode == ExtensionInspectComparisonMode.AvailableOnly)
        {
            return ExtensionInspectPathRelation.NotApplicable;
        }

        if (mode == ExtensionInspectComparisonMode.InstalledOnly)
        {
            if (baseline is null || current is null)
            {
                return ExtensionInspectPathRelation.Unknown;
            }

            return Equivalent(baseline, current)
                ? ExtensionInspectPathRelation.Unchanged
                : AddDiverged(findings, path);
        }

        if (baseline is null && intended is not null)
        {
            ExtensionInspectFindingPolicy.Add(findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.PathNew,
                Path = path,
                Cause = "The intended source contains a path without a persisted baseline.",
            });
            return ExtensionInspectPathRelation.New;
        }

        if (baseline is not null && intended is null)
        {
            ExtensionInspectFindingPolicy.Add(findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.PathRetired,
                Path = path,
                Cause = "The intended source no longer contains a previously managed path.",
            });
            return ExtensionInspectPathRelation.Retired;
        }

        if (current is null)
        {
            return ExtensionInspectPathRelation.Missing;
        }

        if (baseline is null || intended is null)
        {
            return ExtensionInspectPathRelation.Unknown;
        }

        var comparableKind = ExtensionDestinationPolicy.IsImplicit(path)
            ? ExtensionInspectFingerprintKind.Semantic
            : ExtensionInspectFingerprintKind.ExactBytes;
        if (baseline.Kind != comparableKind
            || current.Kind != comparableKind
            || intended.Kind != comparableKind)
        {
            return ExtensionInspectPathRelation.Unknown;
        }

        if (!Equivalent(baseline, current))
        {
            ExtensionInspectFindingPolicy.Add(findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.PathCurrentDiverged,
                Path = path,
                Cause = "The current path differs from its persisted baseline.",
            });
            return ExtensionInspectPathRelation.CurrentDiverged;
        }

        if (!Equivalent(baseline, intended))
        {
            ExtensionInspectFindingPolicy.Add(findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.PathChanged,
                Path = path,
                Cause = "The trusted current source differs from the unchanged persisted baseline.",
            });
            return ExtensionInspectPathRelation.Changed;
        }

        return ExtensionInspectPathRelation.Unchanged;
    }

    private static ExtensionInspectPathRelation AddDiverged(
        ICollection<ExtensionInspectFinding> findings,
        string path)
    {
        ExtensionInspectFindingPolicy.Add(findings, new ExtensionInspectFindingInput
        {
            Code = ExtensionInspectFindingCode.PathCurrentDiverged,
            Path = path,
            Cause = "The current path differs from its persisted baseline.",
        });
        return ExtensionInspectPathRelation.CurrentDiverged;
    }

    private static bool Equivalent(
        ExtensionInspectFingerprint left,
        ExtensionInspectFingerprint right)
        => left.Kind == right.Kind
            && string.Equals(left.Policy, right.Policy, StringComparison.Ordinal)
            && string.Equals(left.Sha256, right.Sha256, StringComparison.Ordinal);
}
