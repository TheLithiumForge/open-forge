using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning.Metadata;
using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning.Metadata;

internal sealed class RouteUpdateMetadataByteEditor
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal ImmutableArray<byte> Apply(RouteUpdateMetadataByteEditInput input)
    {
        var observation = input.Observation;
        var byteEdits = input.Edits
            .Select(edit => ToByteEdit(observation.TargetText, edit))
            .OrderBy(edit => edit.Start)
            .ToArray();
        var source = observation.TargetSnapshot.Bytes;
        var expectedLength = checked(
            source.Length + byteEdits.Sum(edit => edit.Replacement.Length - edit.Length));
        var result = ImmutableArray.CreateBuilder<byte>(expectedLength);
        var sourcePosition = 0;
        foreach (var edit in byteEdits)
        {
            if (edit.Start < sourcePosition || edit.Start + edit.Length > source.Length)
            {
                throw new InvalidOperationException(
                    "Route Update metadata edits must be ordered, disjoint, and contained.");
            }

            result.AddRange(source.AsSpan(sourcePosition, edit.Start - sourcePosition));
            result.AddRange(edit.Replacement);
            sourcePosition = edit.Start + edit.Length;
        }

        result.AddRange(source.AsSpan()[sourcePosition..]);
        return result.MoveToImmutable();
    }

    private static RouteUpdateMetadataByteEdit ToByteEdit(
        string targetText,
        RouteUpdateMetadataEdit edit)
    {
        var documentStart = edit.DocumentStart;
        var documentEnd = checked(documentStart + edit.DocumentLength);
        return new RouteUpdateMetadataByteEdit(
            StrictUtf8.GetByteCount(targetText.AsSpan(0, documentStart)),
            StrictUtf8.GetByteCount(targetText.AsSpan(documentStart, documentEnd - documentStart)),
            StrictUtf8.GetBytes(edit.Replacement));
    }

}
