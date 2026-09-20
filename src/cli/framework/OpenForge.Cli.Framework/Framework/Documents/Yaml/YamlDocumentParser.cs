using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace OpenForge.Cli.Core.Framework.Documents.Yaml;

internal sealed class YamlDocumentParser
{
    internal YamlDocumentFacts Parse(string source)
    {
        ArgumentNullException.ThrowIfNull(source);

        try
        {
            var reader = new YamlEventReader(source);
            reader.Require<StreamStart>();
            reader.Advance();
            if (reader.Current is StreamEnd)
            {
                return new YamlDocumentFacts(source, YamlDocumentState.Complete, null);
            }

            reader.Require<DocumentStart>();
            reader.Advance();
            var root = reader.Current is DocumentEnd ? null : ReadNode(reader);
            reader.Require<DocumentEnd>();
            reader.Advance();
            reader.Require<StreamEnd>();
            return new YamlDocumentFacts(source, YamlDocumentState.Complete, root);
        }
        catch (Exception exception) when (IsUnavailableYamlFailure(exception))
        {
            return new YamlDocumentFacts(source, YamlDocumentState.Unavailable, null);
        }
    }

    private static YamlNode ReadNode(YamlEventReader reader)
    {
        return reader.Current switch
        {
            Scalar scalar => ReadScalar(reader, scalar),
            AnchorAlias alias => ReadAlias(reader, alias),
            SequenceStart sequence => ReadSequence(reader, sequence),
            MappingStart mapping => ReadMapping(reader, mapping),
            _ => throw new InvalidOperationException("The YAML event stream does not describe one node."),
        };
    }

    private static YamlNode ReadScalar(YamlEventReader reader, Scalar scalar)
    {
        reader.Advance();
        return YamlNode.FromScalar(new YamlScalar(scalar.Value, CreateSpan(scalar.Start, scalar.End)));
    }

    private static YamlNode ReadAlias(YamlEventReader reader, AnchorAlias alias)
    {
        reader.Advance();
        return YamlNode.Alias(CreateSpan(alias.Start, alias.End));
    }

    private static YamlNode ReadSequence(YamlEventReader reader, SequenceStart start)
    {
        reader.Advance();
        var values = new List<YamlNode>();
        while (reader.Current is not SequenceEnd)
        {
            values.Add(ReadNode(reader));
        }

        var end = reader.Require<SequenceEnd>();
        reader.Advance();
        return YamlNode.FromSequence(CreateCollectionSpan(start, end, start.Style == SequenceStyle.Flow), values);
    }

    private static YamlNode ReadMapping(YamlEventReader reader, MappingStart start)
    {
        reader.Advance();
        var entries = new List<YamlMappingEntry>();
        while (reader.Current is not MappingEnd)
        {
            entries.Add(new YamlMappingEntry(ReadNode(reader), ReadNode(reader)));
        }

        var end = reader.Require<MappingEnd>();
        reader.Advance();
        return YamlNode.FromMapping(CreateCollectionSpan(start, end, start.Style == MappingStyle.Flow), entries);
    }

    private static YamlTextSpan CreateCollectionSpan(
        NodeEvent start,
        ParsingEvent end,
        bool includesClosingDelimiter)
    {
        var endIndex = checked((int)end.End.Index);
        if (includesClosingDelimiter)
        {
            endIndex = checked(endIndex + 1);
        }

        return CreateSpan(start.Start, endIndex);
    }

    private static YamlTextSpan CreateSpan(Mark start, Mark end)
        => CreateSpan(start, checked((int)end.Index));

    private static YamlTextSpan CreateSpan(Mark start, int endIndex)
    {
        var startIndex = checked((int)start.Index);
        return new YamlTextSpan(startIndex, checked(endIndex - startIndex));
    }

    private static bool IsUnavailableYamlFailure(Exception exception)
        => exception is YamlException
            or InvalidOperationException
            or ArgumentException
            or InvalidCastException
            or FormatException
            or OverflowException;

    private sealed class YamlEventReader
    {
        private readonly Parser _parser;

        internal YamlEventReader(string source)
        {
            _parser = new Parser(new StringReader(source));
            if (!_parser.MoveNext())
            {
                throw new InvalidOperationException("The YAML event stream ended unexpectedly.");
            }
        }

        internal ParsingEvent Current
            => _parser.Current
                ?? throw new InvalidOperationException("The YAML event stream ended unexpectedly.");

        internal void Advance()
        {
            if (!_parser.MoveNext())
            {
                throw new InvalidOperationException("The YAML event stream ended unexpectedly.");
            }
        }

        internal T Require<T>() where T : ParsingEvent
            => Current as T
                ?? throw new InvalidOperationException("The YAML event stream has an unexpected boundary.");
    }
}
