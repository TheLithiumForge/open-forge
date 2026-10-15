using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.References;

namespace OpenForge.Cli.Core.Framework.Sources.References.Shared.Resolution;

internal static class SourceLinkTargetIdentityReader
{
    internal static SourceLayer? FindLayer(SourceLogicalSource source, string canonicalPath)
        => ReadLayers(source).FirstOrDefault(layer => string.Equals(layer.CanonicalPath, canonicalPath, StringComparison.Ordinal));

    private static IReadOnlyList<SourceLayer> ReadLayers(SourceLogicalSource source)
        => source.Overwrite is { } overwrite ? [source.Base, overwrite] : [source.Base];

    internal static SourceLinkIdentity ToIdentity(SourceLogicalSource source)
        => new()
        {
            Id = source.Identity.AutomaticId,
            Path = source.Identity.CanonicalBasePath,
        };

    internal static SourceLinkDestinationFacts ReadCollision(
        SourceLinkTarget target,
        IEnumerable<SourceLogicalSource> candidates,
        string? fragment = null)
        => new()
        {
            Fragment = fragment,
            Target = target,
            Finding = new SourceLinkDestinationFinding
            {
                Code = SourceLinkDestinationFindingCode.IdentityCollision,
                Cause = "The target path is exact, but its automatic source ID is shared by more than one logical source.",
                Candidates = candidates.Select(ToIdentity).ToArray(),
            },
        };
}
