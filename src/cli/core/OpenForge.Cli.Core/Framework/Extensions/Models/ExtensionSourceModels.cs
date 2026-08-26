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

internal sealed record ExtensionPackageFact
{
    internal ExtensionPackageFact(
        string id,
        string name,
        string description,
        string version,
        IEnumerable<string> dependencies,
        int payloadFileCount)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        ArgumentNullException.ThrowIfNull(dependencies);
        if (payloadFileCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(payloadFileCount), payloadFileCount, "Payload file count cannot be negative.");
        }

        Id = id;
        Name = name;
        Description = description;
        Version = version;
        Dependencies = new ReadOnlyCollection<string>(dependencies.ToArray());
        PayloadFileCount = payloadFileCount;
    }

    internal string Id { get; }

    internal string Name { get; }

    internal string Description { get; }

    internal string Version { get; }

    internal IReadOnlyList<string> Dependencies { get; }

    internal int PayloadFileCount { get; }
}

internal sealed record ExtensionSourceReadResult
{
    internal ExtensionSourceReadResult(
        ExtensionSourceReadState state,
        ExtensionSourceKind? kind,
        string identity,
        IEnumerable<ExtensionPackageFact> packages,
        string? cause)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The Extension source state is not defined.");
        }

        if (kind is { } sourceKind && !Enum.IsDefined(sourceKind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Extension source kind is not defined.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(identity);
        ArgumentNullException.ThrowIfNull(packages);
        State = state;
        Kind = kind;
        Identity = identity;
        Packages = new ReadOnlyCollection<ExtensionPackageFact>(packages.ToArray());
        Cause = cause;
    }

    internal ExtensionSourceReadState State { get; }

    internal ExtensionSourceKind? Kind { get; }

    internal string Identity { get; }

    internal IReadOnlyList<ExtensionPackageFact> Packages { get; }

    internal string? Cause { get; }
}
