using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Operation;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Application;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Planning;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Resolution;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.Commands.Extension.Create;

internal sealed partial class ExtensionCreateOperation(CliInteractiveSession interactiveSession)
{
    private readonly ExtensionCreateRequestResolver _requestResolver = new(interactiveSession);
    private readonly ExtensionCreatePlanner _planner = new();
    private readonly ExtensionCreateDestinationWriter _writer = new();

    internal async ValueTask<ExtensionCreateResult> ExecuteAsync(
        ExtensionCreateRequest request,
        CancellationToken cancellationToken)
    {
        var planning = await PlanAsync(request, cancellationToken).ConfigureAwait(false);
        if (planning.Result is not null)
        {
            return planning.Result;
        }

        return await ApplyAsync(
            planning.Plan
                ?? throw new InvalidOperationException("Successful Extension Create planning requires a plan."),
            cancellationToken).ConfigureAwait(false);
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
