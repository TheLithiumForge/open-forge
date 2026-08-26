using OpenForge.Cli.Core.Commands.Context.Models.Operation;
using OpenForge.Cli.Core.Commands.Context.Models.Request;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Shared.Graph;
using OpenForge.Cli.Core.Commands.Context.Shared.Links;
using OpenForge.Cli.Core.Commands.Context.Shared.Projection;
using OpenForge.Cli.Core.Commands.Context.Shared.Result;
using OpenForge.Cli.Core.Commands.Context.Shared.Selection;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Context;

internal delegate ValueTask<ContextGraph> ContextGraphReader(
    CliWorkspace workspace,
    CancellationToken cancellationToken);

internal sealed class ContextOperation
{
    private readonly ContextGraphReader _graphReader;
    private readonly ContextResultBuilder _resultBuilder = new();

    internal ContextOperation(ContextGraphReader graphReader)
    {
        ArgumentNullException.ThrowIfNull(graphReader);
        _graphReader = graphReader;
    }

    internal async ValueTask<ContextResult> ExecuteAsync(
        ContextRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var graph = await _graphReader(request.Workspace, cancellationToken)
                .ConfigureAwait(false);
            var closure = new ContextClosureResolver().Resolve(request, graph);
            var links = await new ContextLinkExpander()
                .ExpandAsync(request, graph, closure, cancellationToken)
                .ConfigureAwait(false);
            var expandedClosure = closure.WithLinkExpansion(links);
            var projection = new ContextProjectionBuilder().Build(expandedClosure, request.Content);
            return _resultBuilder.Build(request, closure, links, projection);
        }
        catch (OperationCanceledException)
        {
            return _resultBuilder.Event(request, interrupted: true);
        }
        catch (Exception)
        {
            return _resultBuilder.Event(request, interrupted: false);
        }
    }
}
