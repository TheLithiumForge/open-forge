using OpenForge.Cli.Core.Commands.Extension.Create.Models.Operation;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Resolution;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Create.Shared.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Create.Shared.Planning;

internal sealed partial class ExtensionCreatePlanner
{
    private readonly PhysicalPathResolver _physicalPathResolver = new();
    private readonly ExtensionCreateDestinationInspector _destinationInspector;

    internal ExtensionCreatePlanner()
    {
        _destinationInspector = new ExtensionCreateDestinationInspector(_physicalPathResolver);
    }

    internal ExtensionCreatePlanningOutcome Plan(ExtensionCreateResolvedRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string catalogue;
        try
        {
            catalogue = Path.GetFullPath(request.CataloguePath);
        }
        catch (Exception exception) when (IsInvalidPath(exception))
        {
            return Terminal(
                request,
                CliSemanticStatus.Invalid,
                ExtensionCreateFindingCode.InvalidInput,
                request.CataloguePath,
                "The catalogue path is invalid.");
        }

        var root = _physicalPathResolver.ResolveRoot(catalogue);
        if (root.State != PhysicalPathState.Contained)
        {
            return RootFailure(request, catalogue, root);
        }

        var physicalCatalogue = root.GetContainedPhysicalPath();
        try
        {
            if (!IsOrdinaryDirectory(physicalCatalogue))
            {
                return Terminal(
                    request,
                    CliSemanticStatus.Invalid,
                    ExtensionCreateFindingCode.InvalidInput,
                    catalogue,
                    "The catalogue parent must be an existing ordinary directory.");
            }
        }
        catch (Exception exception) when (IsUnavailable(exception))
        {
            return Terminal(
                request,
                CliSemanticStatus.Incomplete,
                ExtensionCreateFindingCode.CatalogueUnavailable,
                catalogue,
                exception.Message);
        }

        var destination = Path.Combine(catalogue, request.StableId);
        var observation = _destinationInspector.Observe(
            catalogue: catalogue,
            cataloguePhysicalIdentity: physicalCatalogue,
            destination: destination,
            manifestBytes: request.ManifestBytes);
        if (observation.State is not (ExtensionCreateDestinationState.Absent or ExtensionCreateDestinationState.Exact))
        {
            return DestinationFailure(request, catalogue, destination, observation);
        }

        var intendedEffects = CreateEffects(destination);
        return ExtensionCreatePlanningOutcome.Planned(
            new ExtensionCreatePlan
            {
                Catalogue = catalogue,
                CataloguePhysicalIdentity = physicalCatalogue,
                Destination = destination,
                DestinationPhysicalIdentity = observation.PhysicalIdentity,
                Manifest = request.Manifest,
                ManifestBytes = request.ManifestBytes.ToArray(),
                Mode = request.Mode,
                Automatic = request.Automatic,
                IntendedEffects = intendedEffects,
                IsVerifiedNoOp = observation.State == ExtensionCreateDestinationState.Exact,
            });
    }

    internal ExtensionCreateFinding? Revalidate(ExtensionCreatePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var root = _physicalPathResolver.ResolveRoot(plan.Catalogue);
        if (root.State != PhysicalPathState.Contained)
        {
            var status = IsUnavailable(root.State)
                ? CliSemanticStatus.Incomplete
                : CliSemanticStatus.Blocked;
            return ExtensionCreateResultFactory.Finding(
                code: status == CliSemanticStatus.Incomplete
                    ? ExtensionCreateFindingCode.CatalogueUnavailable
                    : ExtensionCreateFindingCode.DestinationChanged,
                status: status,
                subject: plan.Catalogue,
                cause: Describe(root));
        }

        var physicalCatalogue = root.GetContainedPhysicalPath();
        if (!PhysicalIdentityTracker.PathComparer.Equals(
                physicalCatalogue,
                plan.CataloguePhysicalIdentity))
        {
            return Changed(plan.Catalogue, "The catalogue physical identity changed after planning.");
        }

        try
        {
            if (!IsOrdinaryDirectory(physicalCatalogue))
            {
                return Changed(plan.Catalogue, "The catalogue is no longer an ordinary directory.");
            }
        }
        catch (Exception exception) when (IsUnavailable(exception))
        {
            return ExtensionCreateResultFactory.Finding(
                code: ExtensionCreateFindingCode.CatalogueUnavailable,
                status: CliSemanticStatus.Incomplete,
                subject: plan.Catalogue,
                cause: exception.Message);
        }

        var observation = _destinationInspector.Observe(plan);
        if (observation.State == ExtensionCreateDestinationState.Unavailable)
        {
            return ExtensionCreateResultFactory.Finding(
                code: ExtensionCreateFindingCode.CatalogueUnavailable,
                status: CliSemanticStatus.Incomplete,
                subject: plan.Destination,
                cause: observation.Cause ?? "The destination is unavailable.");
        }

        var expectedState = plan.IsVerifiedNoOp
            ? ExtensionCreateDestinationState.Exact
            : ExtensionCreateDestinationState.Absent;
        if (observation.State != expectedState)
        {
            return Changed(
                plan.Destination,
                observation.Cause ?? "The destination changed after planning.");
        }

        if (plan.DestinationPhysicalIdentity is not null
            && !PhysicalIdentityTracker.PathComparer.Equals(
                observation.PhysicalIdentity,
                plan.DestinationPhysicalIdentity))
        {
            return Changed(plan.Destination, "The destination physical identity changed after planning.");
        }

        return null;
    }

    internal ExtensionCreateDestinationObservation Verify(ExtensionCreatePlan plan)
        => _destinationInspector.Observe(plan);
}
