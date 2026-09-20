namespace OpenForge.Cli.Core.Commands.Context;

internal static class ContextOperationFactory
{
    internal static ContextOperation Create()
    {
        var graphBuilder = new Shared.Graph.ContextGraphBuilder();
        return new ContextOperation(graphBuilder.BuildAsync);
    }
}
