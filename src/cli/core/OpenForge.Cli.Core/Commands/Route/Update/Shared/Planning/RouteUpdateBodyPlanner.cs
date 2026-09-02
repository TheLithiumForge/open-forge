using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Shared.Templates.Models;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

internal sealed class RouteUpdateBodyPlanner
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    internal RouteUpdateBodyPlanBuild Build(RouteUpdateBodyPlanInput input)
    {
        var metadata = input.Metadata;
        if (input.Template is null)
        {
            return Complete(
                metadata,
                template: null,
                RouteUpdateBodyState.Preserved,
                metadata.IntendedTargetBytes,
                []);
        }

        if (input.Template.State != RouteTemplateResolutionState.Resolved
            || input.Template.Template is not { } selection)
        {
            return Stop(metadata, input.Template);
        }

        var bodySpan = metadata.Observation.Markdown.BodySpan
            ?? throw new InvalidOperationException(
                "Complete Route Update frontmatter requires one body span.");
        var body = metadata.Observation.TargetText[bodySpan.Start..bodySpan.End];
        var template = new RouteUpdateTemplate
        {
            Requested = selection.Requested,
            Id = selection.Id,
            Path = selection.Path,
            Classification = RouteUpdateTemplateClassification.Template,
            BodyByteLength = selection.BodyByteLength,
            Decision = body.All(char.IsWhiteSpace)
                ? RouteUpdateTemplateDecision.Copied
                : RouteUpdateTemplateDecision.AuthoredBodyProtected,
        };
        if (!body.All(char.IsWhiteSpace))
        {
            return Complete(
                metadata,
                template,
                RouteUpdateBodyState.AuthoredBodyProtected,
                metadata.IntendedTargetBytes,
                []);
        }

        var originalBodyBytes = StrictUtf8.GetBytes(body);
        if (!metadata.IntendedTargetBytes.AsSpan().EndsWith(originalBodyBytes))
        {
            throw new InvalidOperationException(
                "Parsed-span metadata editing must preserve the exact body suffix.");
        }

        var prefixLength = metadata.IntendedTargetBytes.Length - originalBodyBytes.Length;
        var intended = ImmutableArray.CreateBuilder<byte>(
            prefixLength + input.Template.BodyBytes.Length);
        intended.AddRange(metadata.IntendedTargetBytes.AsSpan()[..prefixLength]);
        intended.AddRange(input.Template.BodyBytes);
        ImmutableArray<RouteUpdatePreviewHunk> preview =
        [
            new RouteUpdatePreviewHunk
            {
                Kind = RouteUpdatePreviewKind.TemplateBody,
                Before = body,
                Expected = StrictUtf8.GetString(input.Template.BodyBytes.AsSpan()),
            },
        ];
        return Complete(
            metadata,
            template,
            RouteUpdateBodyState.TemplateCopied,
            intended.MoveToImmutable(),
            preview);
    }

    private static RouteUpdateBodyPlanBuild Complete(
        RouteUpdateMetadataPatch metadata,
        RouteUpdateTemplate? template,
        RouteUpdateBodyState state,
        ImmutableArray<byte> intendedBytes,
        ImmutableArray<RouteUpdatePreviewHunk> preview)
        => RouteUpdateBodyPlanBuild.Complete(
            new RouteUpdateBodyPlan
            {
                Metadata = metadata,
                Template = template,
                State = state,
                IntendedTargetBytes = intendedBytes,
                Preview = preview,
            });

    private static RouteUpdateBodyPlanBuild Stop(
        RouteUpdateMetadataPatch metadata,
        RouteTemplateResolution template)
        => RouteUpdateBodyPlanBuild.Stop(
            new RouteUpdatePlanningBoundary
            {
                Formation = new RouteUpdateResultFormation
                {
                    Workspace = metadata.Observation.Request.Workspace,
                    Mode = metadata.Observation.Request.Mode,
                    Target = metadata.Observation.Target,
                    Patch = metadata.Patch,
                    Template = RouteUpdateTemplate.Unresolved(
                        metadata.Observation.Request.TemplateReference
                            ?? RouteUpdateDefinitions.TemplateReferenceValueName),
                    Plan = new RouteUpdatePlanFacts
                    {
                        Completeness = template.State == RouteTemplateResolutionState.Incomplete
                            ? RouteUpdatePlanCompleteness.Incomplete
                            : RouteUpdatePlanCompleteness.NotEstablished,
                        Safety = template.State == RouteTemplateResolutionState.Blocked
                            ? RouteUpdatePlanSafety.Blocked
                            : RouteUpdatePlanSafety.NotEstablished,
                        Body = RouteUpdateBodyState.NotEstablished,
                    },
                    Effects = [],
                    UnchangedPaths = [],
                    Recovery = RouteUpdateRecovery.NotRequired(),
                    Verification = RouteUpdateVerificationState.NotRequested,
                    Findings =
                    [
                        new RouteUpdateFinding(
                            RouteUpdateTemplateFindingAdapter.Read(
                                template.State,
                                template.Issue),
                            template.Cause ?? "The requested Template is unavailable.",
                            template.Target),
                    ],
                },
            });
}
