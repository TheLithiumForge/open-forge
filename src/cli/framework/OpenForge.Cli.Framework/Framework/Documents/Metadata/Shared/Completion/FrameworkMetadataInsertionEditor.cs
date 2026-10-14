using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion;

internal static class FrameworkMetadataInsertionEditor
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal static string Newline(string source)
    {
        var firstLf = source.IndexOf('\n');
        return firstLf > 0 && source[firstLf - 1] == '\r' ? "\r\n" : "\n";
    }

    internal static bool TryInsertMapping(
        string yaml,
        YamlNode mapping,
        FrameworkDocumentMetadataEmission fields,
        FrontmatterForm form,
        out (int Offset, string Text) insertion)
    {
        insertion = default;
        if (mapping.Mapping is not { } members || mapping.Span.End > yaml.Length) return false;
        if (mapping.Span.Length > 1 && yaml[mapping.Span.Start] == '{' && yaml[mapping.Span.End - 1] == '}')
        {
            var text = FrameworkMetadataCompletionEmitter.Emit(fields, form, flow: true);
            if (members.Count > 0) text += ", ";
            insertion = (mapping.Span.Start + 1, text);
            return true;
        }

        if (members.Count == 0) return false;
        var firstKey = members[0].Key.Span.Start;
        var lineStart = firstKey == 0 ? 0 : yaml.LastIndexOf('\n', firstKey - 1) + 1;
        var indentation = yaml[lineStart..firstKey];
        if (!indentation.AsSpan().Trim(' ').IsEmpty) return false;
        var newline = Newline(yaml);
        var block = FrameworkMetadataCompletionEmitter.Emit(fields, form, flow: false);
        var indented = string.Join(newline, block.TrimEnd('\n').Split('\n').Select(line => indentation + line)) + newline;
        insertion = (lineStart, indented);
        return true;
    }

    internal static ImmutableArray<byte> Insert(ImmutableArray<byte> original, string source, (int Offset, string Text) insertion)
    {
        var offset = StrictUtf8.GetByteCount(source.AsSpan(0, insertion.Offset));
        var inserted = StrictUtf8.GetBytes(insertion.Text);
        var result = new byte[original.Length + inserted.Length];
        original.AsSpan(0, offset).CopyTo(result);
        inserted.CopyTo(result.AsSpan(offset));
        original.AsSpan(offset..).CopyTo(result.AsSpan(offset + inserted.Length));
        if (!original.AsSpan(0, offset).SequenceEqual(result.AsSpan(0, offset))
            || !original.AsSpan(offset..).SequenceEqual(result.AsSpan(offset + inserted.Length)))
            throw new InvalidOperationException("Metadata insertion changed an untouched byte span.");
        return [.. result];
    }
}
