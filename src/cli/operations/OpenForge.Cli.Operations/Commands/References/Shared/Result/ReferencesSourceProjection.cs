using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.References.Shared.Result;

internal static class ReferencesSourceProjection
{
    internal static ReferencesSource Create(SourceLogicalSource source)
    {
        var layers = source.Overwrite is { } overwrite
            ? new ReferencesSourceLayer[]
            {
                new ReferencesSourceLayer(SourceLayerKind.Base, source.Base.CanonicalPath),
                new ReferencesSourceLayer(SourceLayerKind.Overwrite, overwrite.CanonicalPath),
            }
            : new ReferencesSourceLayer[]
            { new ReferencesSourceLayer(SourceLayerKind.Base, source.Base.CanonicalPath) };
        return new ReferencesSource(source.Identity.AutomaticId, source.Identity.CanonicalBasePath, layers);
    }
}
