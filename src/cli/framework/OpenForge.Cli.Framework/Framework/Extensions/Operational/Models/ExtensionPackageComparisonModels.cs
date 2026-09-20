using OpenForge.Cli.Core.Framework.Extensions.Models;

namespace OpenForge.Cli.Core.Framework.Extensions.Operational.Models;

internal enum ExtensionInstalledPackageComparisonState
{
    Current,
    Missing,
    VersionMismatch,
    DependencyMismatch,
    Ambiguous,
    SourceUnavailable,
}

internal sealed class ExtensionInstalledPackageComparison
{
    private ExtensionInstalledPackageComparison(
        InstalledExtensionObservation installed,
        ExtensionInstalledPackageComparisonState state,
        ExtensionPackageFact? sourcePackage,
        string? cause)
    {
        if (sourcePackage is not null
            && !string.Equals(sourcePackage.Id, installed.Id, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "An Extension source package must match the installed package identity.",
                nameof(sourcePackage));
        }

        var versionsMatch = sourcePackage is not null
            && string.Equals(sourcePackage.Version, installed.Version, StringComparison.Ordinal);
        var dependenciesMatch = sourcePackage is not null
            && sourcePackage.Dependencies.SequenceEqual(installed.Dependencies, StringComparer.Ordinal);
        var compatible = state switch
        {
            ExtensionInstalledPackageComparisonState.Current =>
                sourcePackage is not null && versionsMatch && dependenciesMatch && cause is null,
            ExtensionInstalledPackageComparisonState.VersionMismatch =>
                sourcePackage is not null && !versionsMatch && cause is not null,
            ExtensionInstalledPackageComparisonState.DependencyMismatch =>
                sourcePackage is not null && versionsMatch && !dependenciesMatch && cause is not null,
            ExtensionInstalledPackageComparisonState.Missing
                or ExtensionInstalledPackageComparisonState.Ambiguous =>
                sourcePackage is null && cause is not null,
            ExtensionInstalledPackageComparisonState.SourceUnavailable =>
                sourcePackage is null && !string.IsNullOrWhiteSpace(cause),
            _ => false,
        };
        if (!compatible)
        {
            throw new ArgumentException(
                "The installed Extension package comparison state is inconsistent with its exact source facts.",
                nameof(state));
        }

        Installed = installed;
        State = state;
        SourcePackage = sourcePackage;
        Cause = cause;
    }

    internal InstalledExtensionObservation Installed { get; }

    internal ExtensionInstalledPackageComparisonState State { get; }

    internal ExtensionPackageFact? SourcePackage { get; }

    internal string? Cause { get; }

    internal static ExtensionInstalledPackageComparison Current(
        InstalledExtensionObservation installed,
        ExtensionPackageFact sourcePackage)
        => new(installed, ExtensionInstalledPackageComparisonState.Current, sourcePackage, cause: null);

    internal static ExtensionInstalledPackageComparison VersionMismatch(
        InstalledExtensionObservation installed,
        ExtensionPackageFact sourcePackage)
        => new(
            installed,
            ExtensionInstalledPackageComparisonState.VersionMismatch,
            sourcePackage,
            "The installed and source package versions differ.");

    internal static ExtensionInstalledPackageComparison DependencyMismatch(
        InstalledExtensionObservation installed,
        ExtensionPackageFact sourcePackage)
        => new(
            installed,
            ExtensionInstalledPackageComparisonState.DependencyMismatch,
            sourcePackage,
            "The installed and source package dependency lists differ.");

    internal static ExtensionInstalledPackageComparison Missing(
        InstalledExtensionObservation installed)
        => new(
            installed,
            ExtensionInstalledPackageComparisonState.Missing,
            sourcePackage: null,
            "The installed package is absent from its exact source.");

    internal static ExtensionInstalledPackageComparison Ambiguous(
        InstalledExtensionObservation installed)
        => new(
            installed,
            ExtensionInstalledPackageComparisonState.Ambiguous,
            sourcePackage: null,
            "The exact source contains several packages with the installed identity.");

    internal static ExtensionInstalledPackageComparison SourceUnavailable(
        InstalledExtensionObservation installed,
        string cause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        return new(
            installed,
            ExtensionInstalledPackageComparisonState.SourceUnavailable,
            sourcePackage: null,
            cause);
    }
}
