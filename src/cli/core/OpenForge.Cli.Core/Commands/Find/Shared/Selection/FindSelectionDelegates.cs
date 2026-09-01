using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Selection;

internal delegate ValueTask<SourceRouteFacts> FindRouteFactsReader(
    SourceRouteFactsRequest request,
    SourceDocumentReader reader,
    CancellationToken cancellationToken);
