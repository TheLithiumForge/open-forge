using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal static class RouteRemoveAuthoredRowProjector
{
    internal static ImmutableArray<RouteRemoveReferenceDocumentEdit> Read(
        SourceCatalogue catalogue, string parent, MarkdownDocumentFacts document, SourceGeneratedEntriesFacts parsed, Func<string, bool> removed)
    {
        if (parsed.State != SourceGeneratedEntriesState.Complete)
            throw new ArgumentException("Authored row projection requires complete parsed Entries evidence.", nameof(parsed));
        var map = new Utf8SourceMap(document.Source);
        var edits = ImmutableArray.CreateBuilder<RouteRemoveReferenceDocumentEdit>();
        foreach (var row in parsed.Entries)
        {
            var path = SourceGeneratedDestinationResolver.ResolveLinked(catalogue.Layout, parent,
                    parent == catalogue.Layout.LoaderPath, row.Destination)
                ?? SourceGeneratedDestinationResolver.Resolve(catalogue.Layout, parent,
                    parent == catalogue.Layout.LoaderPath, row.Destination);
            if (path is null) continue;
            var retained = catalogue.Aliases.GetValueOrDefault(path) ?? path;
            if (!removed(path) && !removed(retained)) continue;
            var end = row.Span.End;
            if (end < document.Source.Length && document.Source[end] == '\r') end++;
            if (end < document.Source.Length && document.Source[end] == '\n') end++;
            edits.Add(new RouteRemoveReferenceDocumentEdit
            {
                Location = map.Map(row.Span.Start, end - row.Span.Start),
                Before = document.Source[row.Span.Start..end],
                Expected = string.Empty,
            });
        }
        return edits.ToImmutable();
    }

    internal static string Apply(string original, IEnumerable<RouteRemoveReferenceDocumentEdit> edits)
    {
        var bytes = Encoding.UTF8.GetBytes(original);
        foreach (var edit in edits.OrderByDescending(edit => edit.Location.ByteOffset))
        {
            var start = checked((int)edit.Location.ByteOffset);
            var length = checked((int)edit.Location.ByteLength);
            if (!bytes.AsSpan(start, length).SequenceEqual(Encoding.UTF8.GetBytes(edit.Before)))
                throw new InvalidOperationException("An outside row edit must retain its original exact span.");
            var expected = Encoding.UTF8.GetBytes(edit.Expected);
            var changed = new byte[bytes.Length - length + expected.Length];
            bytes.AsSpan(0, start).CopyTo(changed);
            expected.CopyTo(changed.AsSpan(start));
            bytes.AsSpan(start + length).CopyTo(changed.AsSpan(start + expected.Length));
            bytes = changed;
        }
        return Encoding.UTF8.GetString(bytes);
    }
}
