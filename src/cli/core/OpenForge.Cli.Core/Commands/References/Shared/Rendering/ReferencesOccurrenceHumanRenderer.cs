using System.Text;
using OpenForge.Cli.Core.Commands.References.Models.Occurrence;
using static OpenForge.Cli.Core.Commands.References.Shared.Rendering.ReferencesHumanValues;

namespace OpenForge.Cli.Core.Commands.References.Shared.Rendering;

internal static class ReferencesOccurrenceHumanRenderer
{
    internal static void Append(StringBuilder builder, ReferencesOccurrence occurrence, bool expanded)
    {
        var source = occurrence.Source;
        var target = occurrence.Target;
        var destination = target.Path ?? occurrence.RawDestination;
        if (expanded)
        {
            builder.AppendLine($"  {Text(source.Path)}{Location(occurrence.Location)}");
            if (source.Id is { } sourceId)
            {
                builder.AppendLine($"    Source ID: {Text(sourceId)}");
            }
            builder.AppendLine($"    Links to: {Text(destination)}");
        }
        else
        {
            builder.AppendLine($"  {Text(source.Id ?? "unavailable")}  {Text(source.Path)}{Location(occurrence.Location)} -> {Text(destination)}");
        }

        if (target.Id is { } targetId)
        {
            builder.AppendLine($"    Target ID: {Text(targetId)}");
        }
        if (occurrence.RawDestination != destination)
        {
            builder.AppendLine($"    Written as: {Text(occurrence.RawDestination)}");
        }
        builder.AppendLine($"    Resolution: {ReferencesHumanValues.Resolution(target.Resolution)}; source: {Layer(source.Layer)}");
        if (!expanded)
        {
            return;
        }

        if (occurrence.Fragment is { } fragment)
        {
            builder.AppendLine($"    Fragment: {Text(fragment)}");
        }
        if (target.Layer is { } targetLayer)
        {
            builder.AppendLine($"    Target layer: {Layer(targetLayer)}");
        }
        if (occurrence.DestinationLocation is { } destinationLocation)
        {
            builder.AppendLine($"    Destination written at: {Text(source.Path)}{Location(destinationLocation)}");
        }
        builder.AppendLine($"    Found by: {Origin(occurrence.Provenance)}");
    }
}
