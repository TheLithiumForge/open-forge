using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;

internal sealed partial class RouteUpdatePlanProjector
{
    internal RouteUpdatePlanBuild Build(RouteUpdatePlanProjectionInput input)
    {
        var destination = input.Destination;
        var observation = destination.Body.Metadata.Observation;
        var targetRegion = input.Navigation.Regions.SingleOrDefault(region => region.IsTarget);
        var intendedTargetBytes = targetRegion?.Change.ExpectedDocumentBytes
            ?? destination.IntendedTargetBytes;
        var changes = ImmutableArray.CreateBuilder<PlannedFileChange>();
        var targets = ImmutableArray.CreateBuilder<RecoveryBundleTarget>();
        var effects = ImmutableArray.CreateBuilder<RouteUpdateEffect>();

        AddTargetChange(
            destination,
            intendedTargetBytes,
            targetRegion,
            changes,
            targets,
            effects);
        foreach (var region in input.Navigation.Regions
                     .Where(region => !region.IsTarget)
                     .OrderBy(region => region.Source.Identity.CanonicalBasePath, StringComparer.Ordinal))
        {
            AddGeneratedChange(region, changes, targets, effects);
        }

        var effectPaths = effects
            .Select(effect => effect.Path)
            .ToHashSet(StringComparer.Ordinal);
        var unchangedPaths = ReadCandidatePaths(observation, input.Navigation)
            .Where(path => !effectPaths.Contains(path))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToImmutableArray();
        var findings = destination.Body.State == RouteUpdateBodyState.AuthoredBodyProtected
            ? ImmutableArray.Create(
                new RouteUpdateFinding(
                    RouteUpdateFindingCode.TemplateBodyProtected,
                    "The target contains authored body bytes; its Template body was preserved.",
                    observation.Target.Path))
            : ImmutableArray<RouteUpdateFinding>.Empty;
        var formation = new RouteUpdateResultFormation
        {
            Workspace = observation.Request.Workspace,
            Mode = observation.Request.Mode,
            Target = observation.Target,
            Patch = destination.Body.Metadata.Patch,
            Template = destination.Body.Template,
            Plan = new RouteUpdatePlanFacts
            {
                Completeness = RouteUpdatePlanCompleteness.Complete,
                Safety = RouteUpdatePlanSafety.Safe,
                Body = destination.Body.State,
            },
            Effects = effects.ToImmutable(),
            UnchangedPaths = unchangedPaths,
            Recovery = RouteUpdateRecovery.NotCreated(),
            Verification = RouteUpdateVerificationState.NotRequested,
            Findings = findings,
        };
        var plan = new RouteUpdatePlan
        {
            Request = observation.Request,
            Preview = formation,
            Observation = observation,
            Template = destination.Template,
            Destination = destination,
            Navigation = input.Navigation,
            FileChanges = changes.ToImmutable(),
            RecoveryTargets = targets.ToImmutable(),
        };
        return new RouteUpdatePlanBuild
        {
            Plan = plan,
            Formation = formation,
        };
    }

}
