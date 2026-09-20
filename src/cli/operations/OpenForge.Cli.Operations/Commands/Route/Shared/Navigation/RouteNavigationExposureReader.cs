using OpenForge.Cli.Core.Commands.Route.Shared.Models.Navigation;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.References.Shared.Resolution;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Navigation;

internal sealed class RouteNavigationExposureReader(MarkdownDocumentParser markdownParser)
{
    private readonly MarkdownDocumentParser _markdownParser = markdownParser;

    internal async ValueTask<RouteNavigationExposure> ReadAsync(
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

            Add(source, text, workspace.LexicalRoot, exposed, unavailable);
        }

        return Form(exposed, unavailable, isCancelled: false);
    }

    private void Add(
        SourceLogicalSource parent,
        string text,
        string lexicalRoot,
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
            if (ReadPath(parent, entry.Destination, lexicalRoot) is { } path)
            {
                exposed.Add(path);
            }
        }
    }

    private static string? ReadPath(
        SourceLogicalSource parent,
        string destination,
        string lexicalRoot)
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

        var lexical = SourceLinkLexicalPathResolver.Resolve(
            lexicalRoot,
            parent.Identity.CanonicalBasePath,
            decoded);
        return lexical is
        { State: SourceLinkLexicalPathState.Complete, CanonicalPath: { } canonicalPath }
            ? canonicalPath
            : null;
    }

    private static RouteNavigationExposure Form(
        IEnumerable<string> exposed,
        IEnumerable<string> unavailable,
        bool isCancelled)
        => new()
        {
            ExposedPaths = [.. exposed.Order(StringComparer.Ordinal)],
            UnavailableParents = [.. unavailable.Order(StringComparer.Ordinal)],
            IsCancelled = isCancelled,
        };
}
