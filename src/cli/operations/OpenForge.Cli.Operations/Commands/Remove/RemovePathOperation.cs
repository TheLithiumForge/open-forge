using OpenForge.Cli.Core.Commands.Remove.Models.Interaction;
using OpenForge.Cli.Core.Commands.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Remove.Shared.Application;
using OpenForge.Cli.Core.Commands.Remove.Shared.Planning;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Remove;

internal sealed class RemovePathOperation
{
    private readonly RemovePathPlanner _planner;
    private readonly RemovePathApplication _application;
    private readonly CliPlanConfirmation<RemoveResult, RemoveConfirmationQuestion>? _confirmation;

    internal RemovePathOperation(
        RemovePathPlanner planner,
        RemovePathApplication application,
        CliPlanConfirmation<RemoveResult, RemoveConfirmationQuestion>? confirmation)
    {
        ArgumentNullException.ThrowIfNull(planner);
        ArgumentNullException.ThrowIfNull(application);
        _planner = planner;
        _application = application;
        _confirmation = confirmation;
    }

    internal async ValueTask<RemoveResult> ExecuteAsync(
        RemovePathRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var outcome = await _planner.PlanAsync(request, cancellationToken).ConfigureAwait(false);
        if (outcome is RemovePathPlanningOutcome.Stopped stopped)
        {
            return stopped.Result;
        }

        var plan = ((RemovePathPlanningOutcome.Planned)outcome).Plan;
        if (request.IsDryRun || plan.IsNoOp)
        {
            return _planner.Preview(plan, request.IsDryRun);
        }

        if (!request.Automatic)
        {
            if (_confirmation is null || !request.AllowInteractiveConfirmation)
            {
                return _planner.Refuse(
                    plan,
                    RemoveFindingCode.ConfirmationRequired,
                    CliSemanticStatus.Invalid,
                    "Remove needs confirmation, and this session cannot ask.",
                    "open-forge remove \"" + plan.Target + "\" --automatic");
            }

            var reply = await _confirmation(
                _planner.Preview(plan, dryRun: true),
                new RemoveConfirmationQuestion(plan.Target, plan.IsMissing, plan.FileCount, plan.DirectoryCount),
                new CliPromptPolicy(true),
                cancellationToken).ConfigureAwait(false);
            if (reply.State != CliPromptState.Answered || !reply.Value)
            {
                return reply.State == CliPromptState.Cancelled
                    ? RemoveResult.Refused(request.Workspace, plan.Target, "path", RemoveFindingCode.Interrupted,
                        CliSemanticStatus.Interrupted, "Remove was cancelled. Nothing was changed.")
                    : RemoveResult.Refused(request.Workspace, plan.Target, "path", RemoveFindingCode.PermissionDeclined,
                        CliSemanticStatus.Invalid, "Remove was not confirmed. Nothing was changed.");
            }
        }

        return await _application.ApplyAsync(plan, _planner, cancellationToken).ConfigureAwait(false);
    }
}
