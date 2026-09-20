using OpenForge.Cli.Core.Commands.Extension.Create.Models.Manifest;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;

namespace OpenForge.Cli.Core.Commands.Extension.Create.Models.Planning;

internal enum ExtensionCreateEffectKind
{
    ManifestFile,
    PayloadAgentsDirectory,
}

internal sealed record ExtensionCreateEffect
{
    public required ExtensionCreateEffectKind Kind { get; init; }

    public required string Path { get; init; }
}

internal sealed record ExtensionCreatePlan
{
    public required string Catalogue { get; init; }

    public required string CataloguePhysicalIdentity { get; init; }

    public required string Destination { get; init; }

    public required string? DestinationPhysicalIdentity { get; init; }

    public required ExtensionCreateManifest Manifest { get; init; }

    public required ReadOnlyMemory<byte> ManifestBytes { get; init; }

    public required ExtensionCreateMode Mode { get; init; }

    public bool Automatic { get; init; }

    public required IReadOnlyList<ExtensionCreateEffect> IntendedEffects { get; init; }

    public required bool IsVerifiedNoOp { get; init; }
}
