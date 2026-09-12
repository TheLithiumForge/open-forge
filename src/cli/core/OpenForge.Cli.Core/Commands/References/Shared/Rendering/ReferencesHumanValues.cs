using System.Globalization;
using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;

namespace OpenForge.Cli.Core.Commands.References.Shared.Rendering;

internal static class ReferencesHumanValues
{
    internal static string Text(string value) => ReferencesTextEscaping.Escape(value);

    internal static string Location(SourceLocation? value) => value is null
        ? string.Empty
        : string.Create(CultureInfo.InvariantCulture, $":{value.Line}:{value.Column}");

    internal static string Direction(ReferencesDirection value) => value switch
    {
        ReferencesDirection.In => "in",
        ReferencesDirection.Out => "out",
        ReferencesDirection.Both => "both",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The direction is not defined."),
    };

    internal static string Mode(ReferencesSelectionMode value) => value switch
    {
        ReferencesSelectionMode.Default => "default",
        ReferencesSelectionMode.Filtered => "filtered",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The scan mode is not defined."),
    };

    internal static string Coverage(ReferencesCoverage value) => value switch
    {
        ReferencesCoverage.Complete => "complete",
        ReferencesCoverage.Incomplete => "incomplete",
        ReferencesCoverage.Blocked => "blocked",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The coverage is not defined."),
    };

    internal static string Role(SourceUniverseSelectorRole value) => value switch
    {
        SourceUniverseSelectorRole.Include => "Include",
        SourceUniverseSelectorRole.Exclude => "Exclude",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The selector role is not defined."),
    };

    internal static string Layer(SourceLayerKind value) => value switch
    {
        SourceLayerKind.Base => "base file",
        SourceLayerKind.Overwrite => "overwrite file",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The source layer is not defined."),
    };

    internal static string Resolution(ReferencesTargetResolution value) => value switch
    {
        ReferencesTargetResolution.Complete => "target found",
        ReferencesTargetResolution.Missing => "target missing",
        ReferencesTargetResolution.FragmentMissing => "target fragment missing",
        ReferencesTargetResolution.Malformed => "malformed destination",
        ReferencesTargetResolution.Absolute => "absolute destination is not supported",
        ReferencesTargetResolution.Query => "query in destination is not supported",
        ReferencesTargetResolution.EncodingUnsupported => "destination encoding is not supported",
        ReferencesTargetResolution.OutsideWorkspace => "target is outside the workspace",
        ReferencesTargetResolution.PhysicalEscape => "target crosses the workspace boundary",
        ReferencesTargetResolution.Ambiguous => "target is ambiguous; no choice made",
        ReferencesTargetResolution.Unreadable => "target could not be read",
        ReferencesTargetResolution.Unsupported => "destination is not supported",
        ReferencesTargetResolution.ExternalUnchecked => "external URL; not checked over the network",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The target resolution is not defined."),
    };

    internal static string Origin(ReferencesProvenance value) => value switch
    {
        ReferencesProvenance.SelectedSource => "selected source inspection",
        ReferencesProvenance.DefaultIncomingScan => "default incoming scan",
        ReferencesProvenance.FilteredIncomingScan => "filtered incoming scan",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The scan origin is not defined."),
    };
}
