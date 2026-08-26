using System.Globalization;
using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.References.Shared.Rendering;

internal static partial class ReferencesExpandedRenderer
{
    private static string Source(ReferencesSource source)
        => $"{Escape(source.Id)} ({Escape(source.Path)}) layers={string.Join(",", source.Layers.Select(layer => layer.Kind == SourceLayerKind.Base ? "base" : "overwrite"))}";

    private static string TargetIdentity(ReferencesTarget target)
        => target.Kind switch
        {
            ReferencesTargetKind.External or ReferencesTargetKind.Unsupported => "null",
            ReferencesTargetKind.Local => $"id={Escape(target.Id ?? "null")} path={Escape(target.Path ?? "null")}",
            _ => throw new ArgumentOutOfRangeException(nameof(target), target.Kind, "The References target kind is not defined."),
        };

    private static string TargetKind(ReferencesTargetKind kind)
        => kind switch
        {
            ReferencesTargetKind.Local => "local",
            ReferencesTargetKind.External => "external",
            ReferencesTargetKind.Unsupported => "unsupported",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The References target kind is not defined."),
        };

    private static string FormatLocation(SourceLocation location)
        => $":{location.Line.ToString(CultureInfo.InvariantCulture)}:{location.Column.ToString(CultureInfo.InvariantCulture)} (bytes {location.ByteOffset.ToString(CultureInfo.InvariantCulture)}+{location.ByteLength.ToString(CultureInfo.InvariantCulture)})";

    private static string Direction(ReferencesDirection direction)
        => direction switch
        {
            ReferencesDirection.In => ReferencesDefinitions.In,
            ReferencesDirection.Out => ReferencesDefinitions.Out,
            ReferencesDirection.Both => ReferencesDefinitions.Both,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "The References direction is not defined."),
        };

    private static string Coverage(ReferencesCoverage coverage)
        => coverage switch
        {
            ReferencesCoverage.Complete => "complete",
            ReferencesCoverage.Incomplete => "incomplete",
            ReferencesCoverage.Blocked => "blocked",
            _ => throw new ArgumentOutOfRangeException(nameof(coverage), coverage, "The References coverage is not defined."),
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

    private static string Resolution(ReferencesTargetResolution resolution)
        => resolution.ToString().ToLowerInvariant().Replace("fragmentmissing", "fragment-missing", StringComparison.Ordinal).Replace("externalunchecked", "external-unchecked", StringComparison.Ordinal).Replace("encodingunsupported", "encoding-unsupported", StringComparison.Ordinal).Replace("outsideworkspace", "outside-workspace", StringComparison.Ordinal).Replace("physicalescape", "physical-escape", StringComparison.Ordinal);

    private static string Provenance(ReferencesProvenance provenance)
        => provenance switch
        {
            ReferencesProvenance.SelectedSource => "selected-source",
            ReferencesProvenance.DefaultIncomingScan => "default-incoming-scan",
            ReferencesProvenance.FilteredIncomingScan => "filtered-incoming-scan",
            _ => throw new ArgumentOutOfRangeException(nameof(provenance), provenance, "The provenance is not defined."),
        };

    private static string Escape(string value) => ReferencesTextEscaping.Escape(value);
}
