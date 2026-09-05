using System.Text;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.References;

internal sealed partial class RouteRemoveReferenceScanner
{
    internal async ValueTask<RouteRemoveReferencePostRemoveResult> ObserveAsync(
        RouteRemovePlan plan,
        CancellationToken cancellationToken)
    {
        foreach (var document in plan.Projection.References.Documents)
        {
            var validation = await _expectationValidator.ValidateAsync(
                plan.Request.Workspace,
                FileExpectation.File(
                    document.Snapshot.LogicalPath,
                    document.Snapshot.PhysicalPath
                        ?? throw new InvalidOperationException(
                            "A Route Remove reference document requires its physical path."),
                    FileExpectation.Hash(Encoding.UTF8.GetBytes(document.IntendedText))),
                cancellationToken).ConfigureAwait(false);
            if (validation.State == FileExpectationValidationState.Cancelled)
            {
                return new RouteRemoveReferencePostRemoveResult(
                    RouteRemoveReferencePostRemoveState.Interrupted,
                    "Final Route Remove reference verification was interrupted.");
            }

            if (validation.State != FileExpectationValidationState.Matched)
            {
                return new RouteRemoveReferencePostRemoveResult(
                    RouteRemoveReferencePostRemoveState.Failed,
                    "One detached Route Remove reference source does not match its intended bytes.");
            }
        }

        return new RouteRemoveReferencePostRemoveResult(
            RouteRemoveReferencePostRemoveState.Verified,
            Cause: null);
    }
}
