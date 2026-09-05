using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Framework.Extensions.Models;

internal enum ExtensionSourceKind
{
    EmbeddedCatalogue,
    Package,
    Catalogue,
}

internal enum ExtensionSourceReadState
{
    Complete,
    Missing,
    Invalid,
    Blocked,
    Unavailable,
    Cancelled,
}

internal enum ExtensionSourceFailureKind
{
    None,
    Unavailable,
    Invalid,
    Overlap,
    Ambiguous,
    DependencyIncomplete,
    DependencyCycle,
    DependencyConflict,
    PackageUnavailable,
    ManifestMissing,
    PackageInvalid,
    IdentityAmbiguous,
}

internal enum ExtensionPackageFileReadState
{
    Available,
    Missing,
    Invalid,
    Blocked,
    Unavailable,
    Cancelled,
}

internal sealed record ExtensionPackageFileFact
{
    private ExtensionPackageFileFact()
    {
    }

    internal static ExtensionPackageFileFact Create(ExtensionPackageFileSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshot.Path);
        if (!Enum.IsDefined(snapshot.State))
        {
            throw new ArgumentOutOfRangeException(nameof(snapshot.State), snapshot.State, "The Extension package-file state is not defined.");
        }

        if (snapshot.ByteLength is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(snapshot.ByteLength), snapshot.ByteLength, "A package-file byte length cannot be negative.");
        }

        if (snapshot.Sha256 is not null && !IsLowerHexSha256(snapshot.Sha256))
        {
            throw new ArgumentException("A package-file hash must be lowercase SHA-256.", nameof(snapshot.Sha256));
        }

        if (snapshot.Bytes is { } retained && snapshot.ByteLength != retained.Length)
        {
            throw new ArgumentException("A package-file byte length must match its retained bytes.", nameof(snapshot.ByteLength));
        }

        return new ExtensionPackageFileFact
        {
            Path = snapshot.Path,
            TargetPath = snapshot.TargetPath,
            State = snapshot.State,
            ByteLength = snapshot.ByteLength,
            Sha256 = snapshot.Sha256,
            // Keep a private owned array behind the read-only memory surface.
            Bytes = snapshot.Bytes?.ToArray(),
        };
    }

    private static bool IsLowerHexSha256(string value)
        => value.Length == 64
            && value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');

    public required string Path { get; init; }

    public string? TargetPath { get; init; }

    public required ExtensionPackageFileReadState State { get; init; }

    public long? ByteLength { get; init; }

    public string? Sha256 { get; init; }

    public ReadOnlyMemory<byte>? Bytes { get; init; }
}

internal sealed record ExtensionPackageFileSnapshot
{
    public required string Path { get; init; }

    public string? TargetPath { get; init; }

    public required ExtensionPackageFileReadState State { get; init; }

    public long? ByteLength { get; init; }

    public string? Sha256 { get; init; }

    public ReadOnlyMemory<byte>? Bytes { get; init; }
}

internal sealed record ExtensionPackageManifestFact
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string Version { get; init; }

    public required IReadOnlyList<string> Dependencies { get; init; }
}

internal sealed record ExtensionPackageContentsFact
{
    public required string ManifestPath { get; init; }

    public required IReadOnlyList<ExtensionPackageFileFact> Payload { get; init; }
}

internal sealed record ExtensionPackageFact
{
    private ExtensionPackageFact()
    {
    }

    internal static ExtensionPackageFact Create(
        ExtensionPackageManifestFact manifest,
        ExtensionPackageContentsFact contents)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(contents);
        ArgumentException.ThrowIfNullOrWhiteSpace(manifest.Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(manifest.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(manifest.Description);
        ArgumentException.ThrowIfNullOrWhiteSpace(manifest.Version);
        ArgumentNullException.ThrowIfNull(manifest.Dependencies);
        ArgumentException.ThrowIfNullOrWhiteSpace(contents.ManifestPath);
        ArgumentNullException.ThrowIfNull(contents.Payload);

        var dependencies = manifest.Dependencies
            .Select(value => value ?? throw new ArgumentException("Package dependencies cannot contain null members.", nameof(manifest)))
            .ToArray();
        var payload = contents.Payload
            .Select(value => value ?? throw new ArgumentException("Package payload cannot contain null members.", nameof(contents)))
            .OrderBy(value => value.Path, StringComparer.Ordinal)
            .ToArray();

        return new ExtensionPackageFact
        {
            Id = manifest.Id,
            Name = manifest.Name,
            Description = manifest.Description,
            Version = manifest.Version,
            Dependencies = new ReadOnlyCollection<string>(dependencies),
            ManifestPath = contents.ManifestPath,
            Payload = new ReadOnlyCollection<ExtensionPackageFileFact>(payload),
        };
    }

    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public required string Version { get; init; }

    public required IReadOnlyList<string> Dependencies { get; init; }

    internal int PayloadFileCount => Payload.Count;

    public required string ManifestPath { get; init; }

    public required IReadOnlyList<ExtensionPackageFileFact> Payload { get; init; }
}

internal sealed record ExtensionSourceReadResult
{
    internal ExtensionSourceReadResult(
        ExtensionSourceReadState state,
        ExtensionSourceKind? kind,
        string identity,
        IEnumerable<ExtensionPackageFact> packages,
        string? cause,
        ExtensionSourceFailureKind failureKind = ExtensionSourceFailureKind.None)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The Extension source state is not defined.");
        }

        if (kind is { } sourceKind && !Enum.IsDefined(sourceKind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Extension source kind is not defined.");
        }

        if (!Enum.IsDefined(failureKind))
        {
            throw new ArgumentOutOfRangeException(nameof(failureKind), failureKind, "The Extension source failure kind is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(identity);
        ArgumentNullException.ThrowIfNull(packages);
        State = state;
        Kind = kind;
        Identity = identity;
        Packages = new ReadOnlyCollection<ExtensionPackageFact>(packages.ToArray());
        Cause = cause;
        FailureKind = failureKind;
    }

    internal ExtensionSourceReadState State { get; }

    internal ExtensionSourceKind? Kind { get; }

    internal string Identity { get; }

    internal IReadOnlyList<ExtensionPackageFact> Packages { get; }

    internal string? Cause { get; }

    internal ExtensionSourceFailureKind FailureKind { get; }
}
