using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Models.References;

// A null inventory preserves the complete filesystem scan when Git cannot supply exclusions.
internal delegate ValueTask<ImmutableArray<string>?> RouteMarkdownIgnoredPathEnumerator(
    string workspaceRoot,
    CancellationToken cancellationToken);
