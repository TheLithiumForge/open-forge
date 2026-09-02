using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Result;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Update;

internal static partial class RouteUpdateTestData
{
    internal static RouteUpdateResultFormation VerifiedNoOpFormation(
        RouteUpdateTemplate? template = null,
        RouteUpdateBodyState body = RouteUpdateBodyState.Preserved,
        ImmutableArray<RouteUpdateFinding> findings = default)
        => new()
        {
            Workspace = Workspace(),
            Mode = RouteUpdateMode.Apply,
            Target = Target(),
            Patch = PatchFacts(),
            Template = template,
            Plan = new RouteUpdatePlanFacts
            {
                Completeness = RouteUpdatePlanCompleteness.Complete,
                Safety = RouteUpdatePlanSafety.Safe,
                Body = body,
            },
            Effects = [],
            UnchangedPaths = [ParentPath, TargetPath],
            Recovery = new RouteUpdateRecovery
            {
                State = RouteUpdateRecoveryState.NotRequired,
                ResidualPath = null,
            },
            Verification = RouteUpdateVerificationState.Verified,
            Findings = findings.IsDefault ? [] : findings,
        };

    internal static RouteUpdateResult Result(
        RouteUpdateResultFormation? formation = null)
        => new RouteUpdateResultBuilder().Build(
            formation ?? VerifiedNoOpFormation());

    internal static RouteUpdateFinding Finding(
        RouteUpdateFindingCode code,
        string? target = TargetPath,
        string cause = "A bounded Route Update finding.")
        => new(code, cause, target);

    internal static RouteUpdateTarget Target()
        => new()
        {
            Requested = TargetId,
            SelectedBy = RouteUpdateTargetSelection.SourceId,
            Id = TargetId,
            Path = TargetPath,
            Form = RouteUpdateTargetForm.OrdinaryMarkdown,
            OverwritePaths = [],
        };

    internal static RouteUpdatePatch PatchFacts(
        string expectedDescription = "Before",
        RouteUpdatePatchState descriptionState = RouteUpdatePatchState.Unchanged)
        => new()
        {
            Description = new RouteUpdateDescriptionPatch
            {
                Requested = true,
                Before = "Before",
                Expected = expectedDescription,
                State = descriptionState,
            },
            Responsibility = new RouteUpdateResponsibilityPatch
            {
                Requested = false,
                Operation = RouteUpdateResponsibilityOperation.NotRequested,
                Before = "Before responsibility",
                Expected = null,
                State = RouteUpdatePatchState.NotRequested,
            },
            Tags = new RouteUpdateTagsPatch
            {
                Requested = false,
                Before = ["Memory", "Before"],
                Expected = null,
                State = RouteUpdatePatchState.NotRequested,
            },
        };

    internal static RouteUpdateTemplate ProtectedTemplate()
        => new()
        {
            Requested = TemplateId,
            Id = TemplateId,
            Path = TemplatePath,
            Classification = RouteUpdateTemplateClassification.Template,
            BodyByteLength = 16,
            Decision = RouteUpdateTemplateDecision.AuthoredBodyProtected,
        };

    internal static RouteUpdateEffect Effect(
        string path = TargetPath,
        RouteUpdateEffectKind kind = RouteUpdateEffectKind.RoutedFile)
        => new()
        {
            Path = path,
            Kind = kind,
            Action = RouteUpdateEffectAction.Replace,
            Change = new RouteUpdateEffectChange
            {
                Before = "before-hash",
                Expected = "expected-hash",
            },
            Preview = [],
            Outcome = RouteUpdateEffectOutcome.Verified,
            Residual = RouteUpdateEffectResidual.None,
        };
}
