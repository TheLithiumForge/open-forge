using System.Text.Json;
using System.Text.Json.Serialization;
using OpenForge.Cli.Core.Commands.Route.List.Models.Result;
using OpenForge.Cli.Core.Presentation.Route.List.Models;

namespace OpenForge.Cli.Core.Presentation.Route.List.Shared.Rendering;

internal sealed class RouteListDataJsonConverter : JsonConverter<RouteListData>
{
    public override RouteListData? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
        => throw new NotSupportedException("Route-list presentation data is write-only.");

    public override void Write(
        Utf8JsonWriter writer,
        RouteListData value,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        writer.WriteStartObject();
        writer.WritePropertyName("subject");
        if (value.Subject is { } subject)
        {
            writer.WriteStartObject();
            writer.WriteString("id", subject.Id);
            writer.WriteString("path", subject.Path);
            writer.WriteEndObject();
        }
        else
        {
            writer.WriteNullValue();
        }

        writer.WritePropertyName("depth");
        if (value.Depth is { } depth)
        {
            if (depth.Value.Kind == RouteListPresentationDepthKind.All)
            {
                writer.WriteStringValue("all");
            }
            else
            {
                writer.WriteNumberValue(depth.Value.FiniteValue);
            }
        }
        else
        {
            writer.WriteNullValue();
        }

        writer.WriteStartArray("rows");
        foreach (var row in value.Rows)
        {
            writer.WriteStartObject();
            writer.WriteString("id", row.Id);
            writer.WriteString("path", row.Path);
            writer.WriteString("description", row.Description);
            writer.WriteStartArray("tags");
            foreach (var tag in row.Tags)
            {
                writer.WriteStringValue(tag);
            }

            writer.WriteEndArray();
            writer.WriteNumber("relativeDepth", row.RelativeDepth);
            if (value.ShowDetails)
            {
                writer.WriteString("parentId", row.Result.ParentId);
                if (row.Result.AbsoluteDepth is { } absoluteDepth)
                {
                    writer.WriteNumber("absoluteDepth", absoluteDepth);
                }
                else
                {
                    writer.WriteNull("absoluteDepth");
                }

                writer.WriteString("kind", Kind(row.Result.Kind));
                if (row.Result.DirectChildCount is { } directChildren)
                {
                    writer.WriteNumber("directChildren", directChildren);
                }
                else
                {
                    writer.WriteNull("directChildren");
                }

                writer.WriteString("selectedAs", SelectedAs(row.Result.Selection));
                writer.WriteBoolean("hasOverwrite", row.Result.HasOverwrite);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private static string Kind(RouteListPresentationRowKind kind) => kind switch
    {
        RouteListPresentationRowKind.Entrypoint => "entrypoint",
        RouteListPresentationRowKind.RoutedLeaf => "file",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The route-list row kind is not defined."),
    };

    private static string SelectedAs(RouteListPresentationSelectionProvenance provenance) => provenance switch
    {
        RouteListPresentationSelectionProvenance.LoaderRoot => "loader-root",
        RouteListPresentationSelectionProvenance.ExplicitRoot => "explicit-root",
        RouteListPresentationSelectionProvenance.DetachedRoot => "detached-root",
        RouteListPresentationSelectionProvenance.Descendant => "descendant",
        _ => throw new ArgumentOutOfRangeException(nameof(provenance), provenance, "The route-list selection provenance is not defined."),
    };
}
