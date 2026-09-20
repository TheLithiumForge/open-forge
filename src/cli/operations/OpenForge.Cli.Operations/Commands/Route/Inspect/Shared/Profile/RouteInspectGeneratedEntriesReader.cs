using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Profile;

internal static class RouteInspectGeneratedEntriesReader
{
    internal static RouteInspectGeneratedEntries Read(RouteSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source.Base.ReadState != FileReadState.Complete || source.Base.Body is null)
        {
            return RouteInspectGeneratedEntries.Unavailable(
                "The entrypoint body is not completely readable.");
        }

        if (source.Kind != RouteSourceKind.Loader
            && source.Metadata.State != RouteSourceMetadataState.Complete
            && !(source.Kind == RouteSourceKind.Entrypoint
                && source.Metadata.State == RouteSourceMetadataState.Missing))
        {
            return RouteInspectGeneratedEntries.Unavailable(
                "The entrypoint metadata is not completely readable.");
        }

        var document = new MarkdownDocumentParser().Parse(source.Base.Body);
        var facts = SourceGeneratedEntriesParser.Parse(document);
        if (facts.State != SourceGeneratedEntriesState.Complete)
        {
            return RouteInspectGeneratedEntries.Unavailable(
                facts.Cause ?? "The entrypoint Entries section is unavailable.");
        }

        return RouteInspectGeneratedEntries.Available(
            facts.Entries.Select(entry => new RouteInspectGeneratedEntry(
                entry.Destination,
                entry.Tags.Select(tag => $"#{tag}"))));
    }

}
