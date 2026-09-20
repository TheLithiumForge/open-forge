using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal static class ExtensionInspectPathComparisonBuilder
{
    internal static IReadOnlyList<ExtensionInspectPathComparison> Build(
        ExtensionInspectPathComparisonInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var facts = input.ComparisonFacts;
        var current = facts.Current;
        var intended = facts.Intended;
        var availableClosure = facts.AvailableClosure;
        var findings = facts.Findings;
        var currentByPath = current.ToDictionary(fact => fact.Path, StringComparer.Ordinal);
        var intendedByPath = intended.ToDictionary(fact => fact.Path, StringComparer.Ordinal);
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

        var paths = currentOwners.Keys
            .Concat(currentByPath.Keys)
            .Concat(intendedByPath.Keys)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var values = new List<ExtensionInspectPathComparison>(paths.Length);
        foreach (var path in paths)
        {
            currentByPath.TryGetValue(path, out var currentFact);
            intendedByPath.TryGetValue(path, out var intendedFact);
            currentOwners.TryGetValue(path, out var nowOwners);
            intendedOwners.TryGetValue(path, out var newOwners);
            nowOwners ??= [];
            newOwners ??= [];
            var relation = ReadPathRelation(
                input.Mode,
                nowOwners.Length > 0,
                intendedOwners.ContainsKey(path),
                facts.CurrentPaths.FirstOrDefault(observation => observation.Path == path)?.State,
                currentFact?.Fingerprint,
                intendedFact?.Fingerprint,
                findings,
                path);
            if (relation == ExtensionInspectPathRelation.Unchanged
                && (nowOwners.Length > 1 || newOwners.Length > 1))
            {
                relation = ExtensionInspectPathRelation.Shared;
            }
            values.Add(new ExtensionInspectPathComparison
            {
                Path = path,
                Current = currentFact?.Fingerprint,
                Intended = intendedFact?.Fingerprint,
                Relation = relation,
                CurrentOwners = nowOwners,
                IntendedOwners = newOwners,
            });
        }

        return values;
    }

    private static ExtensionInspectPathRelation ReadPathRelation(
        ExtensionInspectComparisonMode mode,
        bool owned,
        bool declared,
        ExtensionInspectCurrentPathState? currentState,
        ExtensionInspectFingerprint? current,
        ExtensionInspectFingerprint? intended,
        ICollection<ExtensionInspectFinding> findings,
        string path)
    {
        if (mode == ExtensionInspectComparisonMode.None) return ExtensionInspectPathRelation.NotStarted;
        if (mode == ExtensionInspectComparisonMode.AvailableOnly) return ExtensionInspectPathRelation.NotApplicable;
        if (mode == ExtensionInspectComparisonMode.InstalledOnly) return ExtensionInspectPathRelation.Unknown;
        if (!owned && intended is not null)
        {
            ExtensionInspectFindingPolicy.Add(findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.PathNew,
                Path = path,
                Cause = "The intended source contains a path without recorded ownership.",
            });
            return ExtensionInspectPathRelation.New;
        }
        if (owned && !declared)
        {
            ExtensionInspectFindingPolicy.Add(findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.PathRetired,
                Path = path,
                Cause = "The intended source no longer contains a previously managed path.",
            });
            return ExtensionInspectPathRelation.Retired;
        }
        if (current is null) return currentState switch
        {
            ExtensionInspectCurrentPathState.Missing => ExtensionInspectPathRelation.Missing,
            ExtensionInspectCurrentPathState.Blocked => ExtensionInspectPathRelation.Blocked,
            ExtensionInspectCurrentPathState.Invalid => ExtensionInspectPathRelation.Invalid,
            _ => ExtensionInspectPathRelation.Unavailable,
        };
        if (intended is null) return ExtensionInspectPathRelation.Unknown;
        var comparableKind = ExtensionInspectFingerprintPolicy.ReadKind(path);
        if (current.Kind != comparableKind || intended.Kind != comparableKind) return ExtensionInspectPathRelation.Unknown;
        if (Equivalent(current, intended)) return ExtensionInspectPathRelation.Unchanged;
        ExtensionInspectFindingPolicy.Add(findings, new ExtensionInspectFindingInput
        {
            Code = ExtensionInspectFindingCode.PathChanged,
            Path = path,
            Cause = "The current workspace differs from the selected package.",
        });
        return ExtensionInspectPathRelation.Changed;
    }

    private static bool Equivalent(
        ExtensionInspectFingerprint left,
        ExtensionInspectFingerprint right)
        => left.Kind == right.Kind
            && string.Equals(left.Policy, right.Policy, StringComparison.Ordinal)
            && string.Equals(left.Sha256, right.Sha256, StringComparison.Ordinal);
}
