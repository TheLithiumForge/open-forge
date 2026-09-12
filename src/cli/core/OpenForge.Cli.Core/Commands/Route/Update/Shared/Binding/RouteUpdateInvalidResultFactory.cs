using OpenForge.Cli.Core.Commands.Route.Update.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Binding;

internal sealed class RouteUpdateInvalidResultFactory
{
    internal RouteUpdateResult CreateContextual(CliInvalidBindingInput input)
    {
        var cause = input.InvalidInput.Diagnostics.Count == 1
            ? input.InvalidInput.Diagnostics[0]
            : string.Join(" ", input.InvalidInput.Diagnostics);
        return Create(
            workspace: null,
            EmptyBindingInput(),
            RouteUpdateMode.Apply,
            new RouteUpdateBindingFailure(
                RouteUpdateFindingCode.InvalidInput,
                string.IsNullOrWhiteSpace(cause)
                    ? "The Route Update command input is invalid."
                    : cause));
    }

    internal RouteUpdateResult Create(
        CliWorkspace? workspace,
        RouteUpdateBindingInput binding,
        RouteUpdateMode mode,
        RouteUpdateBindingFailure failure)
    {
        var target = string.IsNullOrWhiteSpace(binding.Target)
            ? RouteUpdateDefinitions.SourceReferenceValueName
            : binding.Target;
        var patchRequest = RouteUpdateBindingValidator.CreatePatch(binding);
        var formation = new RouteUpdateResultFormation
        {
            Workspace = workspace,
            Mode = mode,
            Target = RouteUpdateTarget.Unresolved(target),
            Patch = RouteUpdatePatch.Unresolved(patchRequest),
            Template = binding.TemplateFacts.IsExplicit
                ? RouteUpdateTemplate.Unresolved(
                    binding.Template ?? RouteUpdateDefinitions.TemplateReferenceValueName)
                : null,
            Plan = new RouteUpdatePlanFacts
            {
                Completeness = RouteUpdatePlanCompleteness.NotEstablished,
                Safety = RouteUpdatePlanSafety.NotEstablished,
                Body = RouteUpdateBodyState.NotEstablished,
            },
            Effects = [],
            UnchangedPaths = [],
            Recovery = RouteUpdateRecovery.NotRequired(),
            Verification = RouteUpdateVerificationState.NotRequested,
            Findings =
            [
                new RouteUpdateFinding(failure.Code, failure.Cause, target),
            ],
        };
        return new RouteUpdateResultBuilder().Build(formation);
    }

    private static RouteUpdateBindingInput EmptyBindingInput()
        => new()
        {
            Target = null,
            Description = null,
            Tags = [],
            Responsibility = null,
            Template = null,
            DescriptionFacts = new CliOptionResultFacts(false, 0, 0),
            TagFacts = new CliOptionResultFacts(false, 0, 0),
            ResponsibilityFacts = new CliOptionResultFacts(false, 0, 0),
            TemplateFacts = new CliOptionResultFacts(false, 0, 0),
            ParserErrors = [],
        };
}
