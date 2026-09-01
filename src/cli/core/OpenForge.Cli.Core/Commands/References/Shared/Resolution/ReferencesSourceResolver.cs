using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Selection;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.References.Shared.Resolution;

internal sealed class ReferencesSourceResolver
{
    private readonly SourceReadSessionRead _sourceSessionRead;
    private readonly SourceReferenceResolver _sourceReferenceResolver;
    private readonly SourceUniverseFilterResolver _universeFilterResolver;

    internal ReferencesSourceResolver(
        SourceReadSessionRead sourceSessionRead,
        SourceReferenceResolver sourceReferenceResolver,
        SourceUniverseFilterResolver universeFilterResolver)
    {
        ArgumentNullException.ThrowIfNull(sourceSessionRead);
        ArgumentNullException.ThrowIfNull(sourceReferenceResolver);
        ArgumentNullException.ThrowIfNull(universeFilterResolver);
        _sourceSessionRead = sourceSessionRead;
        _sourceReferenceResolver = sourceReferenceResolver;
        _universeFilterResolver = universeFilterResolver;
    }

    internal ValueTask<SourceReadSession> ReadAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
        => _sourceSessionRead(workspace, cancellationToken);

    internal SourceReferenceResolution ResolveReference(
        string sourceReference,
        SourceReadSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return _sourceReferenceResolver.Resolve(sourceReference, session.Catalogue);
    }

    internal SourceUniverseFilterResolution ResolveUniverse(
        SourceReadSession session,
        IReadOnlyList<SourceUniverseSelectorOccurrence> selectorOccurrences)
    {
        ArgumentNullException.ThrowIfNull(session);
        return _universeFilterResolver.Resolve(
            new SourceUniverseFilterRequest(
                session.Catalogue,
                session.DefaultSelectionScope,
                selectorOccurrences));
    }
}
