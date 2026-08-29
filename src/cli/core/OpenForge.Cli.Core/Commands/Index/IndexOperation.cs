using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Commands.Index.Shared.Operation;
using OpenForge.Cli.Core.Commands.Index.Shared.Planning;
using OpenForge.Cli.Core.Commands.Index.Shared.Projection;
using OpenForge.Cli.Core.Commands.Index.Shared.Result;

namespace OpenForge.Cli.Core.Commands.Index;

internal sealed class IndexOperation(
    IndexProjectionReader projectionReader,
    IndexPlanBuilder planBuilder,
    IndexApplicationOperation applicationOperation,
    IndexResultBuilder resultBuilder)
{
    private readonly IndexProjectionReader _projectionReader = projectionReader;
    private readonly IndexPlanBuilder _planBuilder = planBuilder;
    private readonly IndexApplicationOperation _applicationOperation = applicationOperation;
    private readonly IndexResultBuilder _resultBuilder = resultBuilder;

    internal async ValueTask<IndexResult> ExecuteAsync(
        IndexRequest request,
        CancellationToken cancellationToken)
    {
        IndexProjectionReadResult read;
        try
        {
            read = await _projectionReader.ReadAsync(request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return InitialEvent(request, IndexFindingCode.Interrupted);
        }
        catch (Exception)
        {
            return InitialEvent(request, IndexFindingCode.OperationFailed);
        }

        return read.State switch
        {
            IndexProjectionReadState.Cancelled
                or IndexProjectionReadState.SelectionIncomplete => ReadBoundaryResult(request, read),
            IndexProjectionReadState.Projected => await ExecuteProjectedAsync(
                    request,
                    read,
                    cancellationToken)
                .ConfigureAwait(false),
            _ => throw new ArgumentOutOfRangeException(
                nameof(read),
                read.State,
                "The Index projection-read state is not defined."),
        };
    }

    private async ValueTask<IndexResult> ExecuteProjectedAsync(
        IndexRequest request,
        IndexProjectionReadResult read,
        CancellationToken cancellationToken)
    {
        var projection = read.Projection
            ?? throw new InvalidOperationException("A projected Index read requires its projection formation.");
        var plan = _planBuilder.Build(new IndexPlanningInput
        {
            Request = request,
            Projection = projection,
        });
        if (!plan.IsComplete || request.Mode == IndexMode.DryRun || plan.IsNoOp)
        {
            return _resultBuilder.Create(new IndexOperationOutcome
            {
                Request = request,
                Selection = projection.Selection.Selection,
                Regions = plan.Regions,
                Recovery = IndexRecovery.NotRequired,
                Findings = projection.Findings,
            });
        }

        var outcome = await _applicationOperation.ExecuteAsync(plan, cancellationToken)
            .ConfigureAwait(false);
        return _resultBuilder.Create(outcome);
    }

    private IndexResult ReadBoundaryResult(
        IndexRequest request,
        IndexProjectionReadResult read)
        => _resultBuilder.Create(new IndexOperationOutcome
        {
            Request = request,
            Selection = read.Selection,
            Regions = [],
            Recovery = IndexRecovery.NotRequired,
            Findings = read.Findings,
        });

    private IndexResult InitialEvent(
        IndexRequest request,
        IndexFindingCode findingCode)
        => _resultBuilder.Create(new IndexOperationOutcome
        {
            Request = request,
            Selection = IndexSelection.NotEstablished(request.HasExplicitSources
                ? IndexSelectionOrigin.ExplicitSources
                : IndexSelectionOrigin.AutomaticLoader),
            Regions = [],
            Recovery = IndexRecovery.NotRequired,
            Findings = [IndexMutationMapper.CreateFinding(findingCode)],
        });
}
