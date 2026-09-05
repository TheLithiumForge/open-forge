using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal sealed class RouteRemoveNavigationExposureReader(MarkdownDocumentParser markdownParser)
{
    private readonly MarkdownDocumentParser _markdownParser = markdownParser;

    internal async ValueTask<RouteRemoveNavigationExposure> ReadAsync(
        RouteRemoveSubjectDiscovery discovery,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(discovery);
        return await ReadAsync(
                discovery.Request.Workspace,
                discovery.Catalogue,
                cancellationToken)
            .ConfigureAwait(false);
    }

    internal async ValueTask<RouteRemoveNavigationExposure> ReadAsync(
        CliWorkspace workspace,
        SourceCatalogue catalogue,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(catalogue);
        var exposed = new HashSet<string>(StringComparer.Ordinal);
        var unavailable = new HashSet<string>(StringComparer.Ordinal);
        var reader = new SourceDocumentReader(workspace);
        foreach (var source in catalogue.Sources.Where(source =>
                     SourceFormClassifier.IsEntrypoint(source.Base.Form)))
        {
            var read = await reader.ReadAsync(source.Base, cancellationToken).ConfigureAwait(false);
            if (read.Verification.State == SourceLayerVerificationState.Cancelled
                || read.Read?.State == FileReadState.Cancelled)
            {
                return Form(exposed, unavailable, isCancelled: true);
            }

            if (read.Read?.State != FileReadState.Complete || read.Read.Value is not { } text)
            {
                unavailable.Add(source.Identity.CanonicalBasePath);
                continue;
            }

            Add(source, text, exposed, unavailable);
        }

        return Form(exposed, unavailable, isCancelled: false);
    }

    private void Add(
        SourceLogicalSource parent,
        string text,
        ISet<string> exposed,
        ISet<string> unavailable)
    {
        var entries = SourceGeneratedEntriesParser.Parse(_markdownParser.Parse(text));
        if (entries.State != SourceGeneratedEntriesState.Complete)
        {
            unavailable.Add(parent.Identity.CanonicalBasePath);
            return;
        }

        foreach (var entry in entries.Entries)
        {
            if (ReadPath(parent, entry.Destination) is { } path)
            {
                exposed.Add(path);
            }
        }
    }

    private static string? ReadPath(SourceLogicalSource parent, string destination)
    {
        var rawPath = destination.Split('#', 2)[0];
        string decoded;
        try
        {
            decoded = Uri.UnescapeDataString(rawPath);
        }
        catch (UriFormatException)
        {
            return null;
        }

        var parentPath = SourceLogicalPath.ReadParent(parent.Identity.CanonicalBasePath);
        var combined = Path.GetFullPath(Path.Combine("/", parentPath, decoded));
        return combined.TrimStart('/').Replace(Path.DirectorySeparatorChar, '/');
    }

    private static RouteRemoveNavigationExposure Form(
        IEnumerable<string> exposed,
        IEnumerable<string> unavailable,
        bool isCancelled)
        => new()
        {
            ExposedPaths = exposed.Order(StringComparer.Ordinal).ToImmutableArray(),
            UnavailableParents = unavailable.Order(StringComparer.Ordinal).ToImmutableArray(),
            IsCancelled = isCancelled,
        };
}
