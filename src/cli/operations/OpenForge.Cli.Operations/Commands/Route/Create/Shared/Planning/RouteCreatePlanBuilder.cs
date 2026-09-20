using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;

internal sealed class RouteCreatePlanBuilder
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly RouteCreateTargetInspector _targetInspector = new();
    private readonly RouteCreateTemplateResolver _templateResolver = new();
    private readonly FrameworkMarkdownDocumentWriter _documentWriter = new();
    private readonly RouteCreateGeneratedNavigationPlanner _navigationPlanner = new();
    private readonly RouteCreatePlanResultProjector _resultProjector = new();

    internal async ValueTask<RouteCreatePlanBuild> BuildAsync(
        RouteCreateRequest request,
        CancellationToken cancellationToken)
    {
        var targetBuild = await _targetInspector.InspectAsync(request, cancellationToken)
            .ConfigureAwait(false);
        if (targetBuild.Boundary is { } targetBoundary)
        {
            return _resultProjector.Stop(request, targetBoundary);
        }

        var inspection = targetBuild.Inspection
            ?? throw new InvalidOperationException(
                "Successful Route Create target inspection requires its stage result.");
        var template = await _templateResolver.ResolveAsync(
                request,
                inspection.Catalogue,
                cancellationToken)
            .ConfigureAwait(false);
        if (template.Finding is { } templateFinding)
        {
            return _resultProjector.Stop(request, new RouteCreatePlanningBoundary
            {
                Target = inspection.Target,
                Parent = null,
                Template = null,
                Finding = templateFinding,
                IsIncomplete = template.State == RouteCreateTemplateResolutionState.Incomplete,
            });
        }

        var body = StrictUtf8.GetString(template.BodyBytes.AsSpan());
        var intendedBytes = HasCompleteMetadata(request.Metadata)
            ? _documentWriter.Write(
                new FrameworkDocumentMetadata(
                    request.Metadata.Description!,
                    request.Metadata.Tags,
                    request.Metadata.Responsibility),
                body)
            : _documentWriter.WriteOptional(
                new FrameworkDocumentMetadataEmission(
                    request.Metadata.Description,
                    request.Metadata.Tags,
                    request.Metadata.Responsibility),
                body);
        if (inspection.Snapshot.Kind == FileExpectationKind.File
            && !inspection.Snapshot.Bytes.AsSpan().SequenceEqual(intendedBytes.AsSpan()))
        {
            return _resultProjector.Stop(request, new RouteCreatePlanningBoundary
            {
                Target = inspection.Target,
                Parent = null,
                Template = template.Template,
                Finding = new RouteCreateFinding(
                    RouteCreateFindingCode.TargetContentDiffers,
                    "The existing Route Create target differs from the requested content.",
                    inspection.Target.Path ?? inspection.Target.Requested),
                IsIncomplete = false,
            });
        }

        var destination = new RouteCreateDestinationPlan
        {
            Inspection = inspection,
            Template = template,
            TargetSource = TargetSource(inspection),
            IntendedBytes = intendedBytes,
        };
        var navigationBuild = await _navigationPlanner.BuildAsync(
                destination,
                cancellationToken)
            .ConfigureAwait(false);
        if (navigationBuild.Boundary is { } navigationBoundary)
        {
            return _resultProjector.Stop(request, navigationBoundary);
        }

        var navigation = navigationBuild.Plan
            ?? throw new InvalidOperationException(
                "Successful Route Create navigation planning requires its stage result.");
        var complete = Complete(destination, navigation);
        var preview = _resultProjector.Preview(complete);
        var plan = new RouteCreatePlan
        {
            Request = request,
            Preview = preview,
            NavigationFormation = navigation.Formation,
            TargetSnapshot = inspection.Snapshot,
            TargetSource = destination.TargetSource,
            ParentSource = navigation.ParentSource,
            NavigationChanges = navigation.NavigationChanges,
            TemplateSource = template.Source,
            IntendedTargetBytes = intendedBytes,
            DirectoryCreations = complete.DirectoryCreations,
            FileChanges = complete.FileChanges,
            RecoveryTargets = complete.RecoveryTargets,
        };
        return new RouteCreatePlanBuild
        {
            Plan = plan,
            Formation = preview,
        };
    }

    private static RouteCreateCompletePlanStage Complete(
        RouteCreateDestinationPlan destination,
        RouteCreateNavigationPlan navigation)
    {
        var changes = ImmutableArray.CreateBuilder<PlannedFileChange>(
            navigation.NavigationChanges.Length + 1);
        foreach (var navigationChange in navigation.NavigationChanges.Where(
                     change => change.IsCreate))
        {
            changes.Add(PlannedFileChange.Create(
                navigationChange.Before.Expectation,
                navigationChange.IntendedBytes.AsSpan()));
        }

        if (destination.Inspection.Snapshot.Kind == FileExpectationKind.Missing)
        {
            changes.Add(PlannedFileChange.Create(
                destination.Inspection.Snapshot.Expectation,
                destination.IntendedBytes.AsSpan()));
        }

        var existingChanges = navigation.NavigationChanges
            .Where(change => !change.IsCreate && change.RequiresFileChange)
            .OrderBy(change => change.Source.Identity.CanonicalBasePath, StringComparer.Ordinal);
        foreach (var navigationChange in existingChanges)
        {
            changes.Add(PlannedFileChange.ReplaceGeneratedRegion(
                navigationChange.Before.Expectation,
                navigationChange.IntendedBytes.AsSpan()));
        }

        var fileChanges = changes.ToImmutable();
        var recoveryTargets = existingChanges
            .Where(change => change.RegionChange is not null)
            .Select(change => RecoveryBundleTarget.Create(
                fileChanges.Single(fileChange => string.Equals(
                    fileChange.LogicalPath,
                    change.Before.LogicalPath,
                    StringComparison.Ordinal)),
                change.Before))
            .ToImmutableArray();
        return new RouteCreateCompletePlanStage
        {
            Destination = destination,
            Navigation = navigation,
            DirectoryCreations = navigation.DirectoryCreations,
            FileChanges = fileChanges,
            RecoveryTargets = recoveryTargets,
        };
    }

    private static SourceLogicalSource TargetSource(RouteCreateTargetInspection inspection)
    {
        var targetPath = inspection.Target.Path
            ?? throw new InvalidOperationException(
                "A resolved Route Create target requires a canonical path.");
        return new SourceLogicalSource(
            inspection.Identity,
            new SourceLayer(
                canonicalPath: targetPath,
                physicalPath: inspection.Snapshot.PhysicalPath
                    ?? inspection.Snapshot.LogicalPath,
                form: SourceDocumentForm.Markdown,
                kind: SourceLayerKind.Base));
    }

    private static bool HasCompleteMetadata(RouteCreateMetadataInput metadata)
        => metadata.Description is not null && !metadata.Tags.IsEmpty;
}
