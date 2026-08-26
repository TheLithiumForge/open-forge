using System.Collections.ObjectModel;
using System.Security.Cryptography;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal sealed class ExtensionInspectComparisonBuilder(MarkdownFingerprintReader fingerprintReader)
{
    private const string GeneratedOwnership = "derived-navigation-only";

    private readonly MarkdownFingerprintReader _fingerprintReader = fingerprintReader;

    internal static IReadOnlyList<LifecycleInstalledPath> ReadBaselineRecords(
        LifecycleReadResult lifecycle,
        LifecycleInstalledPackage? package)
    {
        if (package is null || lifecycle.Trust is not (LifecycleExtensionTrust.Trusted or LifecycleExtensionTrust.Untrusted))
        {
            return [];
        }

        var paths = ReadInstalledClosurePackages(lifecycle.Packages, package)
            .SelectMany(value => value.Paths)
            .ToHashSet(StringComparer.Ordinal);
        return lifecycle.Paths
            .Where(path => paths.Contains(path.Path))
            .OrderBy(path => path.Path, StringComparer.Ordinal)
            .ToArray();
    }

    private static IReadOnlyList<LifecycleInstalledPackage> ReadInstalledClosurePackages(
        IReadOnlyList<LifecycleInstalledPackage> packages,
        LifecycleInstalledPackage? selected)
    {
        if (selected is null)
        {
            return [];
        }

        var byId = packages.ToDictionary(package => package.Id, StringComparer.Ordinal);
        var closure = new HashSet<string>(StringComparer.Ordinal);
        var pending = new Stack<string>();
        pending.Push(selected.Id);
        while (pending.TryPop(out var id))
        {
            if (!closure.Add(id) || !byId.TryGetValue(id, out var package))
            {
                continue;
            }

            foreach (var dependency in package.Dependencies)
            {
                pending.Push(dependency);
            }
        }

        return packages
            .Where(package => closure.Contains(package.Id))
            .OrderBy(package => package.Id, StringComparer.Ordinal)
            .ToArray();
    }

    internal static IReadOnlyList<ExtensionInspectFingerprintFact> ReadBaselineFingerprints(
        IReadOnlyList<LifecycleInstalledPath> records)
        => records
            .Select(record => new ExtensionInspectFingerprintFact
            {
                Path = record.Path,
                Fingerprint = new ExtensionInspectFingerprint
                {
                    Kind = record.FingerprintKind == "semantic"
                        ? ExtensionInspectFingerprintKind.Semantic
                        : ExtensionInspectFingerprintKind.ExactBytes,
                    Policy = ExtensionInspectDefinitions.FingerprintPolicy,
                    Sha256 = record.BaselineFingerprint,
                    Origin = ExtensionInspectFingerprintOrigin.PersistedBaseline,
                },
            })
            .OrderBy(fact => fact.Path, StringComparer.Ordinal)
            .ToArray();

    internal IReadOnlyList<ExtensionInspectFingerprintFact> ReadCurrentFingerprints(
        IReadOnlyList<ExtensionInspectCurrentPath> paths,
        ICollection<ExtensionInspectFinding> findings,
        IDictionary<string, MarkdownFingerprintFacts> markdownFacts)
    {
        var values = new List<ExtensionInspectFingerprintFact>();
        foreach (var path in paths.Where(path => path.State == ExtensionInspectCurrentPathState.Present))
        {
            if (path.Bytes is not { } bytes || path.ExactSha256 is null)
            {
                AddFinding(findings, new ExtensionInspectFindingInput
                {
                    Code = ExtensionInspectFindingCode.FingerprintUnavailable,
                    Path = path.Path,
                    Cause = "Current bytes were not retained for fingerprinting.",
                });
                continue;
            }

            var fingerprint = ReadOperationFingerprint(path.Path, bytes, ExtensionInspectFingerprintOrigin.OperationTimeCurrent, findings, markdownFacts);
            values.Add(new ExtensionInspectFingerprintFact
            {
                Path = path.Path,
                Fingerprint = fingerprint,
            });
        }

        return values.OrderBy(fact => fact.Path, StringComparer.Ordinal).ToArray();
    }

    internal IReadOnlyList<ExtensionInspectFingerprintFact> ReadIntendedFingerprints(
        IReadOnlyList<ExtensionInspectDeclaredPathProjection> paths,
        ICollection<ExtensionInspectFinding> findings,
        IDictionary<string, MarkdownFingerprintFacts> markdownFacts)
    {
        var values = new List<ExtensionInspectFingerprintFact>();
        foreach (var path in paths.Where(path => path.State == ExtensionInspectDeclaredPathState.Available))
        {
            if (path.HasConflict
                || path.Files.FirstOrDefault(file => file.State == ExtensionPackageFileReadState.Available)?.Bytes is not { } bytes)
            {
                AddFinding(findings, new ExtensionInspectFindingInput
                {
                    Code = ExtensionInspectFindingCode.FingerprintUnavailable,
                    Subject = path.Owners.FirstOrDefault(),
                    PackageId = path.Owners.FirstOrDefault(),
                    Path = path.Path,
                    Cause = "Intended package bytes were not retained for fingerprinting.",
                });
                continue;
            }

            values.Add(new ExtensionInspectFingerprintFact
            {
                Path = path.Path,
                Fingerprint = ReadOperationFingerprint(path.Path, bytes, ExtensionInspectFingerprintOrigin.OperationTimeIntended, findings, markdownFacts),
            });
        }

        return values.OrderBy(fact => fact.Path, StringComparer.Ordinal).ToArray();
    }

    private ExtensionInspectFingerprint ReadOperationFingerprint(
        string path,
        ReadOnlyMemory<byte> bytes,
        ExtensionInspectFingerprintOrigin origin,
        ICollection<ExtensionInspectFinding> findings,
        IDictionary<string, MarkdownFingerprintFacts> markdownFacts)
    {
        var markdown = IsMarkdown(path);
        var facts = _fingerprintReader.Read(bytes.Span, supportedMarkdown: markdown);
        if (markdown)
        {
            markdownFacts[path] = facts;
        }

        if (facts.State == MarkdownFingerprintState.ExactBytes)
        {
            AddFinding(findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.FingerprintFallback,
                Path = path,
                Cause = facts.Cause ?? "The Markdown path uses exact-byte fallback.",
            });
        }

        if (facts.Sha256 is null)
        {
            AddFinding(findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.FingerprintUnavailable,
                Path = path,
                Cause = "The Markdown fingerprint is unavailable.",
            });
            return new ExtensionInspectFingerprint
            {
                Kind = ExtensionInspectFingerprintKind.ExactBytes,
                Policy = ExtensionInspectDefinitions.FingerprintPolicy,
                Sha256 = Convert.ToHexStringLower(SHA256.HashData(bytes.Span)),
                Origin = origin,
            };
        }

        return new ExtensionInspectFingerprint
        {
            Kind = facts.State == MarkdownFingerprintState.Semantic
                ? ExtensionInspectFingerprintKind.Semantic
                : ExtensionInspectFingerprintKind.ExactBytes,
            Policy = ExtensionInspectDefinitions.FingerprintPolicy,
            Sha256 = facts.Sha256,
            Origin = origin,
        };
    }

    internal static ExtensionInspectGenerated ReadGenerated(
        IReadOnlyDictionary<string, MarkdownFingerprintFacts> current,
        IReadOnlyDictionary<string, MarkdownFingerprintFacts> intended,
        ExtensionInspectPathState pathState,
        ICollection<ExtensionInspectFinding> findings)
    {
        var regions = new List<ExtensionInspectGeneratedRegion>();
        var paths = current.Keys.Concat(intended.Keys).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal);
        foreach (var path in paths)
        {
            current.TryGetValue(path, out var currentFacts);
            intended.TryGetValue(path, out var intendedFacts);
            var facts = ReadGeneratedFacts(currentFacts, intendedFacts);
            var region = facts.Region;
            var state = region.State switch
            {
                MarkdownFingerprintRegionState.Valid => ExtensionInspectGeneratedRegionState.Valid,
                MarkdownFingerprintRegionState.Absent => ExtensionInspectGeneratedRegionState.Absent,
                MarkdownFingerprintRegionState.Invalid => ExtensionInspectGeneratedRegionState.Invalid,
                MarkdownFingerprintRegionState.Ambiguous => ExtensionInspectGeneratedRegionState.Ambiguous,
                MarkdownFingerprintRegionState.Unavailable => ExtensionInspectGeneratedRegionState.Unavailable,
                _ => throw new ArgumentOutOfRangeException(nameof(region), region.State, "The Markdown fingerprint region state is not defined."),
            };
            regions.Add(new ExtensionInspectGeneratedRegion
            {
                Path = path,
                State = state,
                StartMarker = region.StartMarker,
                EndMarker = region.EndMarker,
                StartByteOffset = region.StartByteOffset,
                EndByteOffset = region.EndByteOffset,
                ExcludedInteriorByteLength = region.ExcludedInteriorByteLength,
                MarkerLinesRetained = region.MarkerLinesRetained,
            });
            if (state is ExtensionInspectGeneratedRegionState.Invalid or ExtensionInspectGeneratedRegionState.Ambiguous)
            {
                AddFinding(findings, new ExtensionInspectFindingInput
                {
                    Code = ExtensionInspectFindingCode.GeneratedBoundaryInvalid,
                    Path = path,
                    Cause = "The generated Markdown region is not a valid final boundary.",
                });
            }
        }

        var invalid = regions.Any(region => region.State is ExtensionInspectGeneratedRegionState.Invalid or ExtensionInspectGeneratedRegionState.Ambiguous);
        return new ExtensionInspectGenerated
        {
            State = invalid || pathState is ExtensionInspectPathState.Incomplete or ExtensionInspectPathState.Blocked
                ? ExtensionInspectGeneratedState.Incomplete
                : ExtensionInspectGeneratedState.Complete,
            Ownership = GeneratedOwnership,
            Regions = regions,
        };
    }

    private static MarkdownFingerprintFacts ReadGeneratedFacts(
        MarkdownFingerprintFacts? current,
        MarkdownFingerprintFacts? intended)
    {
        if (current is null)
        {
            if (intended is { } available)
            {
                return available;
            }

            throw new InvalidOperationException("A generated path requires current or intended Markdown facts.");
        }

        if (intended is null)
        {
            return current;
        }

        return ReadRegionPriority(intended.Region.State) > ReadRegionPriority(current.Region.State)
            ? intended
            : current;
    }

    private static int ReadRegionPriority(MarkdownFingerprintRegionState state)
        => state switch
        {
            MarkdownFingerprintRegionState.Absent => 0,
            MarkdownFingerprintRegionState.Valid => 1,
            MarkdownFingerprintRegionState.Unavailable => 2,
            MarkdownFingerprintRegionState.Ambiguous => 3,
            MarkdownFingerprintRegionState.Invalid => 4,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The Markdown fingerprint region state is not defined."),
        };

    internal static ExtensionInspectComparison ReadComparison(
        ExtensionInspectComparisonInput input)
    {
        var lifecycle = input.Lifecycle;
        var installedPackage = input.InstalledPackage;
        var availablePackage = input.AvailablePackage;
        var dependencies = input.Dependencies;
        var baselineRecords = input.BaselineRecords;
        var baseline = input.Baseline;
        var current = input.Current;
        var intended = input.Intended;
        var findings = input.Findings;
        var trustedLifecycle = lifecycle.Trust == LifecycleExtensionTrust.Trusted
            && lifecycle.Coverage == LifecycleCoverageState.Complete
            && lifecycle.WorkspaceBinding == LifecycleWorkspaceBinding.Matched;
        var hasInstalled = installedPackage is not null;
        var hasAvailable = availablePackage is not null;
        var mode = ReadComparisonMode(trustedLifecycle, hasInstalled, hasAvailable);
        var installedClosure = ReadInstalledClosurePackages(lifecycle.Packages, installedPackage);
        var installedPathCount = installedClosure.SelectMany(package => package.Paths)
            .Distinct(StringComparer.Ordinal)
            .Count();
        var baselineState = ReadBaselineState(
            trustedLifecycle,
            installedPackage,
            baselineRecords,
            baseline,
            installedPathCount);
        var currentState = ReadCurrentState(installedPackage, current, installedPathCount);
        var intendedState = ReadIntendedState(availablePackage, intended, input.AvailableClosure);
        var paths = ReadPathComparisons(input, mode);
        var dependencyComparison = ReadDependencyComparison(
            mode,
            lifecycle.Packages,
            installedPackage,
            dependencies,
            findings);
        var state = mode switch
        {
            ExtensionInspectComparisonMode.ThreeWay
                when baselineState == ExtensionInspectComparisonSideState.Available
                    && currentState == ExtensionInspectComparisonSideState.Available
                    && intendedState == ExtensionInspectComparisonSideState.Available
                => ExtensionInspectComparisonState.Complete,
            ExtensionInspectComparisonMode.InstalledOnly
                when baselineState == ExtensionInspectComparisonSideState.Available
                    && currentState == ExtensionInspectComparisonSideState.Available
                => ExtensionInspectComparisonState.Complete,
            ExtensionInspectComparisonMode.AvailableOnly
                when intendedState == ExtensionInspectComparisonSideState.Available
                => ExtensionInspectComparisonState.Complete,
            ExtensionInspectComparisonMode.None => ExtensionInspectComparisonState.NotStarted,
            _ => ExtensionInspectComparisonState.Incomplete,
        };
        return new ExtensionInspectComparison
        {
            State = state,
            Mode = mode,
            Baseline = Side(baselineState, baseline),
            Current = Side(currentState, current),
            Intended = Side(intendedState, intended),
            Paths = paths,
            Dependencies = dependencyComparison,
        };
    }

    private static ExtensionInspectComparisonMode ReadComparisonMode(
        bool trustedLifecycle,
        bool hasInstalled,
        bool hasAvailable)
    {
        if (trustedLifecycle && hasInstalled && hasAvailable)
        {
            return ExtensionInspectComparisonMode.ThreeWay;
        }

        if (hasInstalled && !hasAvailable)
        {
            return ExtensionInspectComparisonMode.InstalledOnly;
        }

        if (hasAvailable && !hasInstalled)
        {
            return ExtensionInspectComparisonMode.AvailableOnly;
        }

        return ExtensionInspectComparisonMode.None;
    }

    private static ExtensionInspectComparisonSideState ReadBaselineState(
        bool trustedLifecycle,
        LifecycleInstalledPackage? installedPackage,
        IReadOnlyList<LifecycleInstalledPath> baselineRecords,
        IReadOnlyList<ExtensionInspectFingerprintFact> baseline,
        int installedPathCount)
    {
        if (installedPackage is null)
        {
            return ExtensionInspectComparisonSideState.NotApplicable;
        }

        if (!trustedLifecycle)
        {
            return ExtensionInspectComparisonSideState.Unavailable;
        }

        return baseline.Count == baselineRecords.Count
            && baselineRecords.Count == installedPathCount
            ? ExtensionInspectComparisonSideState.Available
            : ExtensionInspectComparisonSideState.Unavailable;
    }

    private static ExtensionInspectComparisonSideState ReadCurrentState(
        LifecycleInstalledPackage? installedPackage,
        IReadOnlyList<ExtensionInspectFingerprintFact> current,
        int installedPathCount)
    {
        if (installedPackage is null)
        {
            return ExtensionInspectComparisonSideState.NotApplicable;
        }

        return current.Count == installedPathCount
            ? ExtensionInspectComparisonSideState.Available
            : ExtensionInspectComparisonSideState.Unavailable;
    }

    private static ExtensionInspectComparisonSideState ReadIntendedState(
        ExtensionPackageFact? availablePackage,
        IReadOnlyList<ExtensionInspectFingerprintFact> intended,
        IReadOnlyList<ExtensionPackageFact> availableClosure)
    {
        if (availablePackage is null)
        {
            return ExtensionInspectComparisonSideState.NotApplicable;
        }

        var availableFileCount = availableClosure
            .SelectMany(package => package.Payload)
            .Where(file => file.State == ExtensionPackageFileReadState.Available && file.TargetPath is not null)
            .Select(file => file.TargetPath)
            .OfType<string>()
            .Distinct(StringComparer.Ordinal)
            .Count();
        return intended.Count == availableFileCount
            ? ExtensionInspectComparisonSideState.Available
            : ExtensionInspectComparisonSideState.Unavailable;
    }

    private static IReadOnlyList<ExtensionInspectPathComparison> ReadPathComparisons(
        ExtensionInspectComparisonInput input,
        ExtensionInspectComparisonMode mode)
    {
        var baselineRecords = input.BaselineRecords;
        var baseline = input.Baseline;
        var current = input.Current;
        var intended = input.Intended;
        var installedPackage = input.InstalledPackage;
        var availableClosure = input.AvailableClosure;
        var findings = input.Findings;
        var baselineByPath = baseline.ToDictionary(fact => fact.Path, StringComparer.Ordinal);
        var currentByPath = current.ToDictionary(fact => fact.Path, StringComparer.Ordinal);
        var intendedByPath = intended.ToDictionary(fact => fact.Path, StringComparer.Ordinal);
        var baselineOwners = baselineRecords.ToDictionary(
            record => record.Path,
            record => record.Owners.Order(StringComparer.Ordinal).ToArray(),
            StringComparer.Ordinal);
        var currentOwners = new Dictionary<string, string[]>(StringComparer.Ordinal);
        foreach (var package in ReadInstalledClosurePackages(input.Lifecycle.Packages, installedPackage))
        {
            foreach (var path in package.Paths)
            {
                if (!currentOwners.TryGetValue(path, out var owners))
                {
                    currentOwners[path] = [package.Id];
                }
                else if (!owners.Contains(package.Id, StringComparer.Ordinal))
                {
                    currentOwners[path] = owners.Append(package.Id).Order(StringComparer.Ordinal).ToArray();
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
                    intendedOwners[targetPath] = owners.Append(owner).Order(StringComparer.Ordinal).ToArray();
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
            var relation = ReadPathRelation(mode, baselineFact?.Fingerprint, currentFact?.Fingerprint, intendedFact?.Fingerprint, findings, path);
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
            AddFinding(findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.PathNew,
                Path = path,
                Cause = "The intended source contains a path without a persisted baseline.",
            });
            return ExtensionInspectPathRelation.New;
        }

        if (baseline is not null && intended is null)
        {
            AddFinding(findings, new ExtensionInspectFindingInput
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

        if (baseline.Kind != ExtensionInspectFingerprintKind.Semantic
            || current.Kind != ExtensionInspectFingerprintKind.Semantic
            || intended.Kind != ExtensionInspectFingerprintKind.Semantic)
        {
            return ExtensionInspectPathRelation.Unknown;
        }

        if (!Equivalent(baseline, current))
        {
            AddFinding(findings, new ExtensionInspectFindingInput
            {
                Code = ExtensionInspectFindingCode.PathCurrentDiverged,
                Path = path,
                Cause = "The current path differs from its persisted baseline.",
            });
            return ExtensionInspectPathRelation.CurrentDiverged;
        }

        if (!Equivalent(baseline, intended))
        {
            AddFinding(findings, new ExtensionInspectFindingInput
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
        AddFinding(findings, new ExtensionInspectFindingInput
        {
            Code = ExtensionInspectFindingCode.PathCurrentDiverged,
            Path = path,
            Cause = "The current path differs from its persisted baseline.",
        });
        return ExtensionInspectPathRelation.CurrentDiverged;
    }

    private static ExtensionInspectDependencyComparison ReadDependencyComparison(
        ExtensionInspectComparisonMode mode,
        IReadOnlyList<LifecycleInstalledPackage> installedPackages,
        LifecycleInstalledPackage? installedPackage,
        ExtensionInspectDependencyClosure dependencies,
        ICollection<ExtensionInspectFinding> findings)
    {
        if (mode == ExtensionInspectComparisonMode.None)
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

        var baseline = installedPackage is null
            ? []
            : ReadInstalledDependencyIds(installedPackages, installedPackage);
        var current = baseline;
        var intended = mode == ExtensionInspectComparisonMode.ThreeWay
            ? dependencies.Resolved
                .Where(package => package.State == ExtensionInspectDependencyPackageState.Available)
                .Select(package => package.Id)
                .Order(StringComparer.Ordinal)
                .ToArray()
            : [];
        var intendedKnown = dependencies.State == ExtensionInspectDependencyState.Complete;
        var state = ReadDependencyComparisonState(mode, intendedKnown);
        var relation = ReadDependencyRelation(mode, intendedKnown, baseline, intended, findings);
        return new ExtensionInspectDependencyComparison
        {
            State = state,
            Baseline = baseline,
            Current = current,
            Intended = intended,
            Relation = relation,
        };
    }

    private static IReadOnlyList<string> ReadInstalledDependencyIds(
        IReadOnlyList<LifecycleInstalledPackage> packages,
        LifecycleInstalledPackage selected)
    {
        var byId = packages.ToDictionary(package => package.Id, StringComparer.Ordinal);
        var resolved = new HashSet<string>(StringComparer.Ordinal);

        void Visit(string id)
        {
            if (!resolved.Add(id) || !byId.TryGetValue(id, out var package))
            {
                return;
            }

            foreach (var dependency in package.Dependencies)
            {
                Visit(dependency);
            }
        }

        Visit(selected.Id);
        return resolved.Order(StringComparer.Ordinal).ToArray();
    }

    private static ExtensionInspectDependencyComparisonState ReadDependencyComparisonState(
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

    private static ExtensionInspectDependencyRelation ReadDependencyRelation(
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

        return AddDependencyChanged(findings);
    }

    private static ExtensionInspectDependencyRelation AddDependencyChanged(
        ICollection<ExtensionInspectFinding> findings)
    {
        AddFinding(findings, new ExtensionInspectFindingInput
        {
            Code = ExtensionInspectFindingCode.DependencyChanged,
            Cause = "The intended dependency closure differs from the current installed dependency identity.",
        });
        return ExtensionInspectDependencyRelation.Changed;
    }

    private static ExtensionInspectComparisonSide Side(
        ExtensionInspectComparisonSideState state,
        IReadOnlyList<ExtensionInspectFingerprintFact> facts)
        => new()
        {
            State = state,
            Fingerprints = facts,
        };

    internal static ExtensionInspectCounts ReadCounts(ExtensionInspectCountsInput input)
    {
        var pathComparisons = input.Comparison.Paths;
        return new ExtensionInspectCounts
        {
            InstalledPackages = ReadInstalledPackageCount(input.Lifecycle),
            AvailablePackages = ReadAvailablePackageCount(input.Source),
            DeclaredPaths = input.AvailablePackage is null ? null : input.PathFacts.Declared.Count,
            CurrentPaths = input.InstalledPackage is null ? null : input.PathFacts.Current.Count,
            BaselinePaths = ReadBaselinePathCount(input),
            IntendedPaths = input.AvailablePackage is null ? null : input.PathFacts.Declared.Count,
            Dependencies = input.Dependencies.State == ExtensionInspectDependencyState.Complete
                ? input.Dependencies.Resolved.Count
                : null,
            UnchangedPaths = ReadPathCount(pathComparisons, input.Comparison.State, ExtensionInspectPathRelation.Unchanged),
            ChangedPaths = ReadPathCount(pathComparisons, input.Comparison.State, ExtensionInspectPathRelation.Changed),
            CurrentDivergedPaths = ReadPathCount(pathComparisons, input.Comparison.State, ExtensionInspectPathRelation.CurrentDiverged),
            MissingPaths = ReadPathCount(pathComparisons, input.Comparison.State, ExtensionInspectPathRelation.Missing),
            NewPaths = ReadPathCount(pathComparisons, input.Comparison.State, ExtensionInspectPathRelation.New),
            RetiredPaths = ReadPathCount(pathComparisons, input.Comparison.State, ExtensionInspectPathRelation.Retired),
            SharedPaths = ReadPathCount(pathComparisons, input.Comparison.State, ExtensionInspectPathRelation.Shared),
            GeneratedRegions = ReadGeneratedRegionCount(input.Generated),
            ExcludedGeneratedBytes = ReadExcludedGeneratedBytes(input.Generated),
            Findings = input.FindingCount,
        };
    }

    private static int? ReadInstalledPackageCount(LifecycleReadResult lifecycle)
    {
        if (lifecycle.Coverage != LifecycleCoverageState.Complete)
        {
            return null;
        }

        return lifecycle.Packages.Count;
    }

    private static int? ReadAvailablePackageCount(ExtensionSourceReadResult source)
    {
        if (source.State != ExtensionSourceReadState.Complete)
        {
            return null;
        }

        return source.Packages.Count;
    }

    private static int? ReadBaselinePathCount(ExtensionInspectCountsInput input)
    {
        if (input.InstalledPackage is null
            || input.Lifecycle.Coverage != LifecycleCoverageState.Complete)
        {
            return null;
        }

        return input.Comparison.Baseline.Fingerprints.Count;
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

    internal static bool IsActionable(
        ExtensionInspectComparison comparison,
        ExtensionInspectGenerated generated,
        LifecycleReadResult lifecycle,
        ExtensionSourceReadResult source,
        IReadOnlyList<ExtensionInspectFinding> findings)
        => comparison.Mode == ExtensionInspectComparisonMode.ThreeWay
            && comparison.State == ExtensionInspectComparisonState.Complete
            && generated.State == ExtensionInspectGeneratedState.Complete
            && lifecycle.Trust == LifecycleExtensionTrust.Trusted
            && source.State == ExtensionSourceReadState.Complete
            && comparison.Baseline.State == ExtensionInspectComparisonSideState.Available
            && comparison.Current.State == ExtensionInspectComparisonSideState.Available
            && comparison.Intended.State == ExtensionInspectComparisonSideState.Available
            && comparison.Baseline.Fingerprints.Concat(comparison.Current.Fingerprints).Concat(comparison.Intended.Fingerprints)
                .All(fact => fact.Fingerprint is { Kind: ExtensionInspectFingerprintKind.Semantic, Policy: ExtensionInspectDefinitions.FingerprintPolicy })
            && (comparison.Paths.Any(path => path.Relation == ExtensionInspectPathRelation.Changed)
                || comparison.Dependencies.Relation == ExtensionInspectDependencyRelation.Changed)
            && !findings.Any(finding => finding.Code is ExtensionInspectFindingCode.PathCurrentDiverged
                or ExtensionInspectFindingCode.PathMissing
                or ExtensionInspectFindingCode.PathRetired
                or ExtensionInspectFindingCode.OwnershipConflict
                or ExtensionInspectFindingCode.DependencyConflict
                or ExtensionInspectFindingCode.FingerprintFallback
                or ExtensionInspectFindingCode.GeneratedBoundaryInvalid);

    private static bool Equivalent(
        ExtensionInspectFingerprint left,
        ExtensionInspectFingerprint right)
        => left.Kind == right.Kind
            && string.Equals(left.Policy, right.Policy, StringComparison.Ordinal)
            && string.Equals(left.Sha256, right.Sha256, StringComparison.Ordinal);

    private static bool IsMarkdown(string path)
        => path.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".markdown", StringComparison.OrdinalIgnoreCase);

    private static void AddFinding(
        ICollection<ExtensionInspectFinding> findings,
        ExtensionInspectFindingInput input)
        => findings.Add(ExtensionInspectFindingFactory.Create(input));
}
