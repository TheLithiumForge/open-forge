using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Shared.Documents;
using OpenForge.Cli.Core.Commands.Find.Shared.Result;
using OpenForge.Cli.Core.Commands.Find.Shared.Selection;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Selection;

namespace OpenForge.Cli.Core.Commands.Find.Models.Operation;

internal sealed class FindOperationComponents
{
    internal required FindSourceBoundaryReader SourceBoundaryReader { get; init; }

    internal required SourceUniverseFilterResolver UniverseFilterResolver { get; init; }

    internal required SourcePhysicalPathResolver PhysicalPathResolver { get; init; }

    internal required FindSelectedLayerReader SelectedLayerReader { get; init; }

    internal required FindRouteFactsReader RouteFactsReader { get; init; }

    internal required FindMarkdownDocumentReader MarkdownDocumentReader { get; init; }

    internal required FindFrontmatterFactsReader FrontmatterFactsReader { get; init; }

    internal required FindResultBuilder ResultBuilder { get; init; }
}
