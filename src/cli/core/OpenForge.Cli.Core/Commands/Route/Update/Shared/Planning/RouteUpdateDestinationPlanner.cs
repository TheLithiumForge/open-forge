using System.Text;
using OpenForge.Cli.Core.Commands.Route.Shared.Templates;
using OpenForge.Cli.Core.Commands.Route.Shared.Templates.Models;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

internal sealed class RouteUpdateDestinationPlanner
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly RouteTemplateResolver _templateResolver;
    private readonly RouteUpdateMetadataPatcher _metadataPatcher;
    private readonly RouteUpdateBodyPlanner _bodyPlanner;

    internal RouteUpdateDestinationPlanner(
        RouteTemplateResolver templateResolver,
        RouteUpdateMetadataPatcher metadataPatcher,
        RouteUpdateBodyPlanner bodyPlanner)
    {
        _templateResolver = templateResolver;
        _metadataPatcher = metadataPatcher;
        _bodyPlanner = bodyPlanner;
    }

    internal async ValueTask<RouteUpdateDestinationBuild> BuildAsync(
        RouteUpdateDestinationInput input,
        CancellationToken cancellationToken)
    {
        var metadataBuild = _metadataPatcher.Build(
            new RouteUpdateMetadataPatchInput
            {
                Observation = input.Observation,
            });
        if (metadataBuild.Boundary is { } metadataBoundary)
        {
            return RouteUpdateDestinationBuild.Stop(metadataBoundary);
        }

        var metadata = metadataBuild.Patch
            ?? throw new InvalidOperationException(
                "Complete Route Update metadata planning requires one patch.");
        RouteTemplateResolution? template = null;
        if (input.Observation.Request.TemplateReference is { } reference)
        {
            template = await _templateResolver.ResolveAsync(
                    new RouteTemplateResolutionRequest
                    {
                        Workspace = input.Observation.Request.Workspace,
                        Reference = reference,
                        Catalogue = input.Observation.Catalogue,
                    },
                    cancellationToken)
                .ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested)
            {
                template = InterruptedTemplate(reference);
            }
        }

        var bodyBuild = _bodyPlanner.Build(
            new RouteUpdateBodyPlanInput
            {
                Metadata = metadata,
                Template = template,
            });
        if (bodyBuild.Boundary is { } bodyBoundary)
        {
            return RouteUpdateDestinationBuild.Stop(bodyBoundary);
        }

        var body = bodyBuild.Body
            ?? throw new InvalidOperationException(
                "Complete Route Update body planning requires one body plan.");
        var intendedText = StrictUtf8.GetString(body.IntendedTargetBytes.AsSpan());
        var document = new MarkdownDocumentParser().Parse(intendedText);
        var facts = new FrameworkDocumentMetadataParser().Parse(document);
        if (facts.State != FrameworkDocumentMetadataState.Complete
            || facts.Metadata is not { } intended)
        {
            return RouteUpdateDestinationBuild.Stop(
                new RouteUpdatePlanningBoundary
                {
                    Formation = BuildInvalidDestination(metadata, body),
                });
        }

        return RouteUpdateDestinationBuild.Complete(
            new RouteUpdateDestinationPlan
            {
                Body = body,
                Template = template,
                IntendedTargetBytes = body.IntendedTargetBytes,
                IntendedMetadata = SourceAuthoredMetadataFacts.Complete(
                    intended.Description,
                    intended.Tags),
            });
    }

    private static RouteTemplateResolution InterruptedTemplate(string reference)
        => new()
        {
            State = RouteTemplateResolutionState.Incomplete,
            Template = null,
            Source = null,
            BodyBytes = [],
            Issue = RouteTemplateResolutionIssue.ResolutionInterrupted,
            Cause = "Route Update Template resolution was interrupted.",
            Target = reference,
        };

    private static RouteUpdateResultFormation BuildInvalidDestination(
        RouteUpdateMetadataPatch metadata,
        RouteUpdateBodyPlan body)
    {
        var observation = metadata.Observation;
        return new RouteUpdateResultFormation
        {
            Workspace = observation.Request.Workspace,
            Mode = observation.Request.Mode,
            Target = observation.Target,
            Patch = metadata.Patch,
            Template = body.Template,
            Plan = new RouteUpdatePlanFacts
            {
                Completeness = RouteUpdatePlanCompleteness.NotEstablished,
                Safety = RouteUpdatePlanSafety.Blocked,
                Body = body.State,
            },
            Effects = [],
            UnchangedPaths = [],
            Recovery = RouteUpdateRecovery.NotRequired(),
            Verification = RouteUpdateVerificationState.NotRequested,
            Findings =
            [
                new RouteUpdateFinding(
                    RouteUpdateFindingCode.FrontmatterUnsafe,
                    "The intended Route Update document did not retain complete metadata.",
                    observation.Target.Path),
            ],
        };
    }
}
