using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Binding;

internal static class RouteCreateInvalidResultFactory
{
    internal static RouteCreateResult CreateContextualInvalidResult(
        CliInvalidBindingInput input)
    {
        var cause = input.InvalidInput.Diagnostics.Count == 1
            ? input.InvalidInput.Diagnostics[0]
            : string.Join(" ", input.InvalidInput.Diagnostics);
        return CreateInvalidResult(
            workspace: null,
            requested: RouteCreateDefinitions.FileTargetValueName,
            mode: RouteCreateMode.Apply,
            code: RouteCreateFindingCode.InvalidInput,
            cause: string.IsNullOrWhiteSpace(cause)
                ? "The Route Create command input is invalid."
                : cause);
    }

    internal static RouteCreateResult CreateInvalidResult(
        CliWorkspace? workspace,
        string? requested,
        RouteCreateMode mode,
        RouteCreateFindingCode code,
        string cause)
    {
        var attemptedTarget = string.IsNullOrWhiteSpace(requested)
            ? RouteCreateDefinitions.FileTargetValueName
            : requested;
        var formation = new RouteCreateResultFormation
        {
            Workspace = workspace,
            Mode = mode,
            Target = new RouteCreateTarget
            {
                Requested = attemptedTarget,
            },
            Parent = null,
            Metadata = new RouteCreateMetadata
            {
                Description = string.Empty,
                Responsibility = null,
                Tags = [],
            },
            Template = null,
            Plan = new RouteCreatePlanFacts
            {
                Completeness = RouteCreatePlanCompleteness.NotEstablished,
                Safety = RouteCreatePlanSafety.NotEstablished,
            },
            Effects = [],
            UnchangedPaths = [],
            Recovery = new RouteCreateRecovery
            {
                State = RouteCreateRecoveryState.NotRequired,
                ResidualPath = null,
            },
            Verification = RouteCreateVerificationState.NotRequested,
            Findings =
            [
                new RouteCreateFinding(code, cause, attemptedTarget),
            ],
        };
        return new RouteCreateResultBuilder().Build(formation);
    }
}
