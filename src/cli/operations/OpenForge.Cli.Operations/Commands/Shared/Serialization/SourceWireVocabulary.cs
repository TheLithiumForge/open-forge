using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Shared.Serialization;

/// <summary>
/// Owns the wire names for source identity and layering. Context, Find, and
/// References all project the same facts, so they project them with the same
/// words. Human wording stays separate and is owned by the presentation text renderer.
/// </summary>
internal static class SourceWireVocabulary
{
    internal static string Layer(SourceLayerKind value)
        => value switch
        {
            SourceLayerKind.Base => "base",
            SourceLayerKind.Overwrite => "overwrite",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReferenceKind(SourceReferenceKind value)
        => value switch
        {
            SourceReferenceKind.SourceId => "source-id",
            SourceReferenceKind.SourcePath => "source-path",
            _ => Undefined(nameof(value), value),
        };

    internal static string ReferenceResolution(SourceReferenceResolutionState value)
        => value switch
        {
            SourceReferenceResolutionState.Resolved => "resolved",
            SourceReferenceResolutionState.Invalid => "invalid",
            SourceReferenceResolutionState.Unknown => "unknown",
            SourceReferenceResolutionState.Ambiguous => "ambiguous",
            SourceReferenceResolutionState.Unsupported => "unsupported",
            SourceReferenceResolutionState.Unsafe => "unsafe",
            _ => Undefined(nameof(value), value),
        };

    private static string Undefined<T>(string name, T value)
        where T : struct, Enum
        => throw new ArgumentOutOfRangeException(
            name,
            value,
            "The source wire value is not defined.");
}
