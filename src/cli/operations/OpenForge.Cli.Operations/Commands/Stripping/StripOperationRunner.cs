using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Commands.Stripping.Shared.Application;
using OpenForge.Cli.Core.Commands.Stripping.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Stripping;

/// <summary>
/// The shared strip and uninstall flow: plan, preview or ask, then apply the same plan.
/// </summary>
internal sealed class StripOperationRunner(
    WorkspaceLockStoreRoot? lockStoreRoot,
    CliPlanConfirmation<StripResult, CliConfirmQuestion>? confirmation,
    Func<StripPlan, CliConfirmQuestion> question)
{
    private readonly PhysicalPathResolver _resolver = new();

    internal async ValueTask<StripResult> RunAsync(StripRequest request, IStripRemovalSource? removals, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);
        var builder = new StripPlanBuilder(_resolver, removals);
        var built = await builder.BuildAsync(request, token).ConfigureAwait(false);
        if (built.Plan is not { } plan)
        {
            return built.Stopped ?? throw new InvalidOperationException("A stopped strip plan needs its result.");
        }

        if (request.DryRun || plan.IsNoOp)
        {
            return StripResultFactory.Preview(plan);
        }

        if (!request.Automatic)
        {
            var refused = await ConfirmAsync(plan, token).ConfigureAwait(false);
            if (refused is not null)
            {
                return refused;
            }
        }

        return await new StripApplication(_resolver, lockStoreRoot)
            .ApplyAsync(plan, rebuildToken => builder.BuildAsync(request, rebuildToken), token).ConfigureAwait(false);
    }

    private async ValueTask<StripResult?> ConfirmAsync(StripPlan plan, CancellationToken token)
    {
        if (confirmation is null || !plan.Request.AllowInteractiveConfirmation)
        {
            return RequireConfirmation(plan);
        }

        var preview = StripResultFactory.Preview(plan with { Request = plan.Request with { DryRun = true } });
        var reply = await confirmation(preview, question(plan), new CliPromptPolicy(true), token).ConfigureAwait(false);
        return reply.State switch
        {
            CliPromptState.Answered when reply.Value => null,
            CliPromptState.Answered => StripResultFactory.Refuse(plan, CliSemanticStatus.Invalid,
                StripResultFactory.Error(StripFindingCode.PermissionDeclined, "The plan was not confirmed. Nothing was changed.")),
            CliPromptState.Cancelled => StripResultFactory.Refuse(plan, CliSemanticStatus.Interrupted,
                StripResultFactory.Error(StripFindingCode.Interrupted, "The run was cancelled. Nothing was changed.")),
            _ => RequireConfirmation(plan),
        };
    }

    private static StripResult RequireConfirmation(StripPlan plan)
        => StripResultFactory.Refuse(plan, CliSemanticStatus.Invalid,
            StripResultFactory.Error(StripFindingCode.ConfirmationRequired, "Applying this plan needs confirmation or --automatic. Nothing was changed."));
}
