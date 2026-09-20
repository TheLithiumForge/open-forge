using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal static class ExtensionInspectComparisonProjector
{
    internal static ExtensionInspectComparison Build(ExtensionInspectComparisonInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var lifecycle = input.Ownership;
        var installedPackage = input.InstalledPackage;
        var availablePackage = input.AvailablePackage;
        var current = input.Current;
        var intended = input.Intended;
        var hasInstalled = installedPackage is not null;
        var hasAvailable = availablePackage is not null;
        var mode = ReadMode(hasInstalled, hasAvailable);
        var installedClosure = ExtensionInspectInstalledClosureReader.Read(
            lifecycle.Document.Extensions,
            installedPackage);
        var installedPathCount = installedClosure.SelectMany(package => package.Paths)
            .Distinct(StringComparer.Ordinal)
            .Count();
        var currentState = ReadCurrentState(installedPackage, current, installedPathCount);
        var intendedState = ReadIntendedState(availablePackage, intended, input.AvailableClosure);
        var paths = ExtensionInspectPathComparisonBuilder.Build(
            new ExtensionInspectPathComparisonInput
            {
                ComparisonFacts = input,
                Mode = mode,
                InstalledClosure = installedClosure,
            });
        var dependencyComparison = ExtensionInspectDependencyComparisonBuilder.Build(
            new ExtensionInspectDependencyComparisonInput
            {
                Mode = mode,
                InstalledPackage = installedPackage,
                InstalledClosure = installedClosure,
                Dependencies = input.Dependencies,
                Findings = input.Findings,
            });
        var state = ReadComparisonState(mode, currentState, intendedState);
        return new ExtensionInspectComparison
        {
            State = state,
            Mode = mode,
            Current = Side(currentState, current),
            Intended = Side(intendedState, intended),
            Paths = paths,
            Dependencies = dependencyComparison,
        };
    }

    internal static ExtensionInspectComparisonState ReadComparisonState(
        ExtensionInspectComparisonMode mode,
        ExtensionInspectComparisonSideState currentState,
        ExtensionInspectComparisonSideState intendedState)
        => mode switch
        {
            ExtensionInspectComparisonMode.InstalledAndAvailable
                when currentState == ExtensionInspectComparisonSideState.Available
                    && intendedState == ExtensionInspectComparisonSideState.Available
                => ExtensionInspectComparisonState.Complete,
            ExtensionInspectComparisonMode.InstalledOnly
                when currentState == ExtensionInspectComparisonSideState.Available
                => ExtensionInspectComparisonState.Complete,
            ExtensionInspectComparisonMode.AvailableOnly
                when intendedState == ExtensionInspectComparisonSideState.Available
                => ExtensionInspectComparisonState.Complete,
            ExtensionInspectComparisonMode.InstalledAndAvailable => ExtensionInspectComparisonState.Incomplete,
            ExtensionInspectComparisonMode.InstalledOnly => ExtensionInspectComparisonState.Incomplete,
            ExtensionInspectComparisonMode.AvailableOnly => ExtensionInspectComparisonState.Incomplete,
            ExtensionInspectComparisonMode.None => ExtensionInspectComparisonState.NotStarted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "The Extension Inspect comparison mode is not defined."),
        };

    private static ExtensionInspectComparisonMode ReadMode(
        bool hasInstalled,
        bool hasAvailable)
    {
        if (hasInstalled && hasAvailable)
        {
            return ExtensionInspectComparisonMode.InstalledAndAvailable;
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

    private static ExtensionInspectComparisonSideState ReadCurrentState(
        ExtensionOwnership? installedPackage,
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

    private static ExtensionInspectComparisonSide Side(
        ExtensionInspectComparisonSideState state,
        IReadOnlyList<ExtensionInspectFingerprintFact> facts)
        => new()
        {
            State = state,
            Fingerprints = facts,
        };
}
