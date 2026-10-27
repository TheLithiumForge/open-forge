using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;

internal static class RouteUpdateOutsideInputs
{
    internal static async ValueTask<RouteUpdateFinding?> ValidateAsync(RouteUpdatePlan plan, CancellationToken token)
    {
        if (RouteUpdateOutsideBoundary.Find(plan.LayoutRead, plan.Observation.TargetSource.Base.CanonicalPath) is null)
            return null;
        var changed = plan.FileChanges.Select(item => item.LogicalPath).ToHashSet(StringComparer.Ordinal);
        var inputs = plan.Navigation.ReadonlySnapshots.AsEnumerable();
        if (plan.Observation.OverwriteSnapshot is { } overwrite) inputs = inputs.Append(overwrite);
        var expectations = inputs.Where(item => !changed.Contains(item.Expectation.LogicalPath)).Select(item => item.Expectation)
            .Append(plan.LayoutRead.Settings.Snapshot?.Expectation ?? FileExpectation.Missing(plan.LayoutRead.Settings.LogicalPath))
            .Append(plan.LayoutRead.Ownership.Snapshot?.Expectation ?? FileExpectation.Missing(plan.LayoutRead.Ownership.LogicalPath))
            .Distinct();
        var validator = new FileExpectationValidator(new PhysicalPathResolver());
        foreach (var expectation in expectations)
        {
            var check = await validator.ValidateAsync(plan.Request.Workspace, expectation, token).ConfigureAwait(false);
            if (check.State != FileExpectationValidationState.Matched)
                return new(check.State == FileExpectationValidationState.Cancelled
                    ? RouteUpdateFindingCode.Interrupted : RouteUpdateFindingCode.TargetChanged,
                    check.Cause ?? "An outside supporting input changed after planning.", expectation.LogicalPath);
        }
        return null;
    }
}
