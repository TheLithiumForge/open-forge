using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Selection;

internal delegate ValueTask<FindSourceReadContext> FindSourceBoundaryReader(
    CliWorkspace workspace,
    CancellationToken cancellationToken);

internal delegate PhysicalPathResolution FindPhysicalPathResolver(
    CliWorkspace workspace,
    string canonicalPath);

internal delegate ValueTask<SourceRouteFacts> FindRouteFactsReader(
    SourceRouteFactsRequest request,
    SourceDocumentReader reader,
    CancellationToken cancellationToken);
