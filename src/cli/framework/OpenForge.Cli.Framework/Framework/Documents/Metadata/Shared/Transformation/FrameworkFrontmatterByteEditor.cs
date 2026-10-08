using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Transformation.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Transformation;

internal static class FrameworkFrontmatterByteEditor
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal static FrameworkFrontmatterTransformResult Flatten(FrameworkFrontmatterTransformInput input)
    {
        var yaml = input.Source[input.YamlSpan.Start..input.YamlSpan.End];
        var lines = ReadLines(yaml);
        var scope = input.Scope;
        var keyLine = FindLine(lines, scope.Key.Span.Start);
        var keyIndent = scope.Key.Span.Start - keyLine.Start;
        if (!yaml.AsSpan(keyLine.Start, keyIndent).Trim(' ').IsEmpty
            || scope.Key.Span.End >= keyLine.ContentEnd
            || yaml[scope.Key.Span.End] != ':')
        {
            return FrameworkFrontmatterTransformResult.Invalid("The open-forge key does not occupy a removable block mapping line.");
        }

        var removals = new List<YamlTextSpan>();
        var wrapperEnd = scope.Key.Span.End + 1;
        if (scope.Value.Mapping is { Count: 0 })
        {
            wrapperEnd = scope.Value.Span.End;
        }

        if (wrapperEnd > keyLine.ContentEnd)
        {
            return FrameworkFrontmatterTransformResult.Invalid("The canonical empty Open Forge mapping must occupy the key line.");
        }

        AddDeclarationRemoval(yaml, keyLine, keyLine.Start, wrapperEnd, removals);
        var members = scope.Value.Mapping;
        if (members is { Count: > 0 })
        {
            var firstKey = members[0].Key.Span.Start;
            var memberLine = FindLine(lines, firstKey);
            var indentation = firstKey - memberLine.Start;
            if (memberLine.Start < keyLine.End || indentation <= keyIndent
                || !yaml.AsSpan(memberLine.Start, indentation).Trim(' ').IsEmpty)
            {
                return FrameworkFrontmatterTransformResult.Invalid("The Open Forge mapping has no removable block indentation.");
            }

            var redundant = input.RedundantApplyTo;
            if (redundant is not null)
            {
                if (!AddApplyToRemoval(yaml, lines, redundant, removals))
                {
                    return FrameworkFrontmatterTransformResult.Invalid("The redundant applyTo declaration cannot be removed without losing comments.");
                }
            }

            var prefix = yaml.AsSpan(memberLine.Start, indentation);
            foreach (var line in lines)
            {
                if (line.Start < keyLine.End || line.Start >= scope.Value.Span.End)
                {
                    continue;
                }

                if (yaml.AsSpan(line.Start, line.ContentEnd - line.Start).StartsWith(prefix))
                {
                    removals.Add(new YamlTextSpan(line.Start + keyIndent, indentation - keyIndent));
                }
            }
        }

        using var rendered = new MemoryStream(input.Bytes.Length);
        var cursor = 0;
        foreach (var removal in MergeRemovals(removals))
        {
            var start = ByteOffset(input, removal.Start);
            var end = ByteOffset(input, removal.End);
            rendered.Write(input.Bytes.Span[cursor..start]);
            cursor = end;
        }

        rendered.Write(input.Bytes.Span[cursor..]);
        return FrameworkFrontmatterTransformResult.Changed(rendered.ToArray());
    }

    private static void AddDeclarationRemoval(
        string yaml,
        FrameworkFrontmatterLineSpan line,
        int start,
        int syntaxEnd,
        ICollection<YamlTextSpan> removals)
    {
        var suffix = yaml.AsSpan(syntaxEnd, line.ContentEnd - syntaxEnd);
        var end = suffix.Trim().IsEmpty ? line.End : syntaxEnd;
        removals.Add(new YamlTextSpan(start, end - start));
    }

    private static bool AddApplyToRemoval(
        string yaml,
        IReadOnlyList<FrameworkFrontmatterLineSpan> lines,
        YamlMappingEntry entry,
        ICollection<YamlTextSpan> removals)
    {
        var keyLine = FindLine(lines, entry.Key.Span.Start);
        var value = entry.Value;
        if (value.Span.End <= keyLine.ContentEnd)
        {
            AddDeclarationRemoval(yaml, keyLine, keyLine.Start, value.Span.End, removals);
            return true;
        }

        if (value.Sequence is not { Count: > 0 } items || yaml[value.Span.Start] == '[')
        {
            return false;
        }

        AddDeclarationRemoval(yaml, keyLine, keyLine.Start, entry.Key.Span.End + 1, removals);
        foreach (var item in items)
        {
            var line = FindLine(lines, item.Span.Start);
            if (item.Span.End > line.ContentEnd)
            {
                return false;
            }

            AddDeclarationRemoval(yaml, line, line.Start, item.Span.End, removals);
        }

        return true;
    }

    private static IEnumerable<YamlTextSpan> MergeRemovals(IEnumerable<YamlTextSpan> removals)
    {
        YamlTextSpan? combined = null;
        foreach (var span in removals.OrderBy(span => span.Start))
        {
            if (combined is null)
            {
                combined = span;
            }
            else if (span.Start <= combined.End)
            {
                combined = new YamlTextSpan(combined.Start, Math.Max(combined.End, span.End) - combined.Start);
            }
            else
            {
                yield return combined;
                combined = span;
            }
        }

        if (combined is not null)
        {
            yield return combined;
        }
    }

    private static int ByteOffset(FrameworkFrontmatterTransformInput input, int yamlPosition)
        => StrictUtf8.GetByteCount(input.Source.AsSpan(0, input.YamlSpan.Start + yamlPosition));

    private static FrameworkFrontmatterLineSpan FindLine(IReadOnlyList<FrameworkFrontmatterLineSpan> lines, int position)
        => lines.First(line => line.Start <= position && position < line.End);

    private static IReadOnlyList<FrameworkFrontmatterLineSpan> ReadLines(string source)
    {
        var result = new List<FrameworkFrontmatterLineSpan>();
        var start = 0;
        foreach (var content in source.AsSpan().EnumerateLines())
        {
            var contentEnd = start + content.Length;
            var end = contentEnd;
            if (end < source.Length)
            {
                end += source.AsSpan(end).StartsWith("\r\n") ? 2 : 1;
            }

            result.Add(new FrameworkFrontmatterLineSpan(start, contentEnd, end));
            start = end;
        }

        return result;
    }
}
