using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.References.Shared.Rendering;

internal static partial class ReferencesJsonProjection
{
    private static string Direction(ReferencesDirection direction)
        => direction switch
        {
            ReferencesDirection.In => ReferencesDefinitions.In,
            ReferencesDirection.Out => ReferencesDefinitions.Out,
            ReferencesDirection.Both => ReferencesDefinitions.Both,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "The References direction is not defined."),
        };

    private static string Status(CliSemanticStatus status)
        => CliStatusDefinitions.Read(status).MachineName;

    private static string Role(SourceUniverseSelectorRole role)
        => role switch
        {
            SourceUniverseSelectorRole.Include => "include",
            SourceUniverseSelectorRole.Exclude => "exclude",
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "The selector role is not defined."),
        };

    private static string Expansion(SourceUniverseSelectorExpansion expansion)
        => expansion switch
        {
            SourceUniverseSelectorExpansion.Source => "source",
            SourceUniverseSelectorExpansion.Folder => "folder",
            _ => throw new ArgumentOutOfRangeException(nameof(expansion), expansion, "The selector expansion is not defined."),
        };

    private static string Layer(SourceLayerKind layer)
        => layer switch
        {
            SourceLayerKind.Base => "base",
            SourceLayerKind.Overwrite => "overwrite",
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "The source layer is not defined."),
        };

    private static string Resolution(ReferencesTargetResolution resolution)
        => resolution switch
        {
            ReferencesTargetResolution.Complete => "complete",
            ReferencesTargetResolution.Missing => "missing",
            ReferencesTargetResolution.FragmentMissing => "fragment-missing",
            ReferencesTargetResolution.Malformed => "malformed",
            ReferencesTargetResolution.Absolute => "absolute",
            ReferencesTargetResolution.Query => "query",
            ReferencesTargetResolution.EncodingUnsupported => "encoding-unsupported",
            ReferencesTargetResolution.OutsideWorkspace => "outside-workspace",
            ReferencesTargetResolution.PhysicalEscape => "physical-escape",
            ReferencesTargetResolution.Ambiguous => "ambiguous",
            ReferencesTargetResolution.Unreadable => "unreadable",
            ReferencesTargetResolution.Unsupported => "unsupported",
            ReferencesTargetResolution.ExternalUnchecked => "external-unchecked",
            _ => throw new ArgumentOutOfRangeException(nameof(resolution), resolution, "The target resolution is not defined."),
        };
}
