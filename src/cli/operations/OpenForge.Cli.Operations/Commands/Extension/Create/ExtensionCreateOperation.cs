using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Interaction;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Operation;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Application;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Planning;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Resolution;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Create;

internal sealed partial class ExtensionCreateOperation
{
    private readonly ExtensionCreateRequestResolver _requestResolver;
    private readonly ExtensionCreatePlanner _planner = new();
    private readonly ExtensionCreateDestinationWriter _writer = new();
    private readonly ExtensionCreateInteraction _interaction;

    internal ExtensionCreateOperation(ExtensionCreateInteraction interaction)
    {
        ArgumentNullException.ThrowIfNull(interaction);
        _interaction = interaction;
        _requestResolver = new ExtensionCreateRequestResolver(interaction);
    }

    internal async ValueTask<ExtensionCreateResult> ExecuteAsync(
        ExtensionCreateRequest request,
        CancellationToken cancellationToken)
    {
        var planning = await PlanAsync(request, cancellationToken).ConfigureAwait(false);
        if (planning.Result is not null)
        {
            return planning.Result;
        }

        var plan = planning.Plan
            ?? throw new InvalidOperationException("Successful Extension Create planning requires a plan.");
        if (!AppliesEffects(plan.Mode) || plan.IsVerifiedNoOp || plan.Automatic)
        {
            return await ApplyAsync(plan, cancellationToken).ConfigureAwait(false);
        }

        if (!request.AllowInteraction)
        {
            return ConfirmationRequired(plan);
        }

        var preview = Complete(plan, [], verified: false, mode: ExtensionCreateMode.DryRun);
        CliPromptReply<bool> approval;
        try
        {
            approval = await _interaction.Confirm(
                preview,
                _interaction.ConfirmationQuestion,
                new CliPromptPolicy(Allowed: request.AllowInteraction),
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Interrupted(plan);
        }

        return approval.State switch
        {
            CliPromptState.Answered when approval.Value => await ApplyAsync(plan, cancellationToken).ConfigureAwait(false),
            CliPromptState.Answered => Interrupted(plan),
            CliPromptState.Unavailable => ConfirmationRequired(plan),
            CliPromptState.Cancelled => Interrupted(plan),
            _ => throw new ArgumentOutOfRangeException(nameof(approval), approval.State,
                "The Extension Create confirmation state is not defined."),
        };
    }

    internal async ValueTask<ExtensionCreatePlanningOutcome> PlanAsync(
        ExtensionCreateRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        _ = AppliesEffects(request.Mode);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var resolution = await _requestResolver
                .ResolveAsync(request, cancellationToken)
                .ConfigureAwait(false);
            if (resolution.Finding is not null)
            {
                return ExtensionCreatePlanningOutcome.Terminal(
                    CreateTerminalRequestResult(
                        resolution.PartialRequest
                            ?? throw new InvalidOperationException("A terminal request resolution requires partial request facts."),
                        resolution.Finding));
            }

            return _planner.Plan(
                resolution.Request
                    ?? throw new InvalidOperationException("Successful request resolution requires a request."));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ExtensionCreatePlanningOutcome.Terminal(Interrupted(request));
        }
    }

    internal async ValueTask<ExtensionCreateResult> ApplyAsync(
        ExtensionCreatePlan plan,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var appliesEffects = AppliesEffects(plan.Mode);
        if (cancellationToken.IsCancellationRequested)
        {
            return Interrupted(plan);
        }

        var changed = _planner.Revalidate(plan);
        if (changed is not null)
        {
            return Failure(plan, [], changed);
        }

        if (!appliesEffects)
        {
            return Complete(plan, [], plan.IsVerifiedNoOp);
        }

        if (plan.IsVerifiedNoOp)
        {
            return Complete(plan, [], verified: true);
        }

        var applied = new List<ExtensionCreateEffect>();
        foreach (var effect in plan.IntendedEffects)
        {
            var application = await _writer
                .ApplyAsync(plan, effect, cancellationToken)
                .ConfigureAwait(false);
            if (!application.Applied)
            {
                return Failure(
                    plan,
                    applied,
                    application.Finding
                        ?? throw new InvalidOperationException("A failed create effect requires a finding."));
            }

            applied.Add(effect);
        }

        var verification = _planner.Verify(plan);
        if (verification.State != ExtensionCreateDestinationState.Exact)
        {
            var finding = ExtensionCreateResultFactory.Finding(
                code: ExtensionCreateFindingCode.VerificationFailed,
                status: CliSemanticStatus.Failed,
                subject: plan.Destination,
                cause: verification.Cause ?? "The complete destination could not be verified.");
            return Failure(plan, applied, finding);
        }

        return Complete(plan, applied, verified: true);
    }

    private static bool AppliesEffects(ExtensionCreateMode mode)
        => mode switch
        {
            ExtensionCreateMode.Apply => true,
            ExtensionCreateMode.DryRun => false,
            _ => throw new ArgumentOutOfRangeException(
                nameof(mode),
                mode,
                "The Extension Create mode is not defined."),
        };
}
