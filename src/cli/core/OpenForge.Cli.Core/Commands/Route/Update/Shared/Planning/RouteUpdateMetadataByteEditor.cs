using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

internal sealed class RouteUpdateMetadataByteEditor
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal ImmutableArray<byte> Apply(RouteUpdateMetadataByteEditInput input)
    {
        var observation = input.Observation;
        var yamlSpan = observation.Markdown.Frontmatter.YamlSpan
            ?? throw new InvalidOperationException(
                "A parsed metadata edit requires one exact YAML span.");
        var byteEdits = input.Edits
            .Select(edit => ToByteEdit(observation.TargetText, yamlSpan.Start, edit))
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

    private static ByteEdit ToByteEdit(
        string targetText,
        int yamlStart,
        RouteUpdateMetadataEdit edit)
    {
        var documentStart = checked(yamlStart + edit.YamlStart);
        var documentEnd = checked(documentStart + edit.YamlLength);
        return new ByteEdit(
            StrictUtf8.GetByteCount(targetText.AsSpan(0, documentStart)),
            StrictUtf8.GetByteCount(targetText.AsSpan(documentStart, documentEnd - documentStart)),
            StrictUtf8.GetBytes(edit.Replacement));
    }

    private sealed record ByteEdit(int Start, int Length, byte[] Replacement);
}
