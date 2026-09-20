using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;

internal enum ExtensionCreateMode
{
    Apply,
    DryRun,
}

internal sealed record ExtensionCreateRequest
{
    private IReadOnlyList<string> _dependencies = [];

    public required string? StableId { get; init; }

    public required string? CataloguePath { get; init; }

    public required string? Name { get; init; }

    public required string? Description { get; init; }

    public required string? PackageVersion { get; init; }

    public required IReadOnlyList<string> Dependencies
    {
        get => _dependencies;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _dependencies = new ReadOnlyCollection<string>(value.ToArray());
        }
    }

    public required bool AllowInteraction { get; init; }

    public bool Automatic { get; init; }

    public required ExtensionCreateMode Mode { get; init; }
}
