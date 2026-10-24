using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Selection;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.References.Shared.Resolution;

internal sealed class ReferencesSourceResolver
{
    private readonly Func<CliWorkspace, CancellationToken, ValueTask<WorkspaceLayoutRead>> _layoutRead;
    private readonly SourceReadSessionRead _sourceSessionRead;
    private readonly SourceReferenceResolver _sourceReferenceResolver;
    private readonly SourceUniverseFilterResolver _universeFilterResolver;

    internal ReferencesSourceResolver(
        Func<CliWorkspace, CancellationToken, ValueTask<WorkspaceLayoutRead>> layoutRead,
        SourceReadSessionRead sourceSessionRead,
        SourceReferenceResolver sourceReferenceResolver,
        SourceUniverseFilterResolver universeFilterResolver)
    {
        _layoutRead = layoutRead;
        ArgumentNullException.ThrowIfNull(sourceSessionRead);
        ArgumentNullException.ThrowIfNull(sourceReferenceResolver);
        ArgumentNullException.ThrowIfNull(universeFilterResolver);
        _sourceSessionRead = sourceSessionRead;
        _sourceReferenceResolver = sourceReferenceResolver;
        _universeFilterResolver = universeFilterResolver;
    }

    internal async ValueTask<SourceReadSession> ReadAsync(
        CliWorkspace workspace,
        CancellationToken cancellationToken)
    {
        var observation = await _layoutRead(workspace, cancellationToken).ConfigureAwait(false);
        var layout = observation.Layout ?? throw new InvalidOperationException("The workspace layout is unavailable.");
        return await _sourceSessionRead(layout, cancellationToken).ConfigureAwait(false);
    }

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
                session.DefaultSelectionScopes,
                selectorOccurrences));
    }
}
