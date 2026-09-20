using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;

namespace OpenForge.Cli.Core.Commands.References.Models.Result;

internal static class ReferencesLayerFacts
{
    internal static ReferencesLayer From(SourceLayerKind layer) => layer switch
    {
        SourceLayerKind.Base => ReferencesLayer.Base,
        SourceLayerKind.Overwrite => ReferencesLayer.Overwrite,
        _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "The References source layer is not defined."),
    };

    internal static ReferencesSelectorRole From(SourceUniverseSelectorRole role) => role switch
    {
        SourceUniverseSelectorRole.Include => ReferencesSelectorRole.Include,
        SourceUniverseSelectorRole.Exclude => ReferencesSelectorRole.Exclude,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "The References selector role is not defined."),
    };
}
