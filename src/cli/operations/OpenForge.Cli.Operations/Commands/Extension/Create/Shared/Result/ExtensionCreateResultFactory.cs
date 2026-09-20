using OpenForge.Cli.Core.Commands.Extension.Create.Models.Manifest;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Resolution;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Create.Shared.Result;

internal static class ExtensionCreateResultFactory
{
    internal static ExtensionCreateResult Create(
        ExtensionCreateRequest request,
        ExtensionCreateResultOutcome outcome)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(outcome);
        return Create(
            catalogue: outcome.Catalogue ?? request.CataloguePath,
            stableId: request.StableId,
            manifest: null,
            mode: request.Mode,
            automatic: request.Automatic,
            outcome: outcome);
    }

    internal static ExtensionCreateResult Create(
        ExtensionCreateResolvedRequest request,
        ExtensionCreateResultOutcome outcome)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(outcome);
        return Create(
            catalogue: outcome.Catalogue ?? request.CataloguePath,
            stableId: request.StableId,
            manifest: request.Manifest,
            mode: request.Mode,
            automatic: request.Automatic,
            outcome: outcome);
    }

    internal static ExtensionCreateResult Create(
        ExtensionCreatePlan plan,
        ExtensionCreateResultOutcome outcome,
        ExtensionCreateMode? mode = null)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(outcome);
        return CreateResult(
            new ExtensionCreateResultInput
            {
                Status = outcome.Status,
                Catalogue = plan.Catalogue,
                Destination = plan.Destination,
                StableId = plan.Manifest.Id,
                Manifest = plan.Manifest,
                Mode = mode ?? plan.Mode,
                Automatic = plan.Automatic,
                IntendedEffects = plan.IntendedEffects,
                AppliedEffects = outcome.AppliedEffects,
                Verification = outcome.Verification,
                Finding = outcome.Finding,
            });
    }

    private static ExtensionCreateResult Create(
        string? catalogue,
        string? stableId,
        ExtensionCreateManifest? manifest,
        ExtensionCreateMode mode,
        bool automatic,
        ExtensionCreateResultOutcome outcome)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        return CreateResult(
            new ExtensionCreateResultInput
            {
                Status = outcome.Status,
                Catalogue = catalogue,
                Destination = outcome.Destination,
                StableId = stableId,
                Manifest = manifest,
                Mode = mode,
                Automatic = automatic,
                IntendedEffects = outcome.IntendedEffects,
                AppliedEffects = outcome.AppliedEffects,
                Verification = outcome.Verification,
                Finding = outcome.Finding,
            });
    }

    private static ExtensionCreateResult CreateResult(ExtensionCreateResultInput input)
    {
        if (input.Status == CliSemanticStatus.Attention)
        {
            throw new ArgumentException("Attention is unreachable for Extension Create.", nameof(input));
        }

        return new ExtensionCreateResult
        {
            Status = input.Status,
            Catalogue = input.Catalogue,
            Destination = input.Destination,
            StableId = input.StableId,
            Manifest = input.Manifest,
            Mode = input.Mode,
            Automatic = input.Automatic,
            IntendedEffects = input.IntendedEffects,
            AppliedEffects = input.AppliedEffects,
            Verification = input.Verification,
            Findings = input.Finding is null ? [] : [input.Finding],
            Next = ExtensionCreateDefinitions.ReadNext(input.Status, input.Finding),
        };
    }

    internal static ExtensionCreateFinding Finding(
        ExtensionCreateFindingCode code,
        CliSemanticStatus status,
        string? subject,
        string cause)
        => new()
        {
            Code = code,
            Status = status,
            Subject = subject,
            Cause = cause,
        };
}
