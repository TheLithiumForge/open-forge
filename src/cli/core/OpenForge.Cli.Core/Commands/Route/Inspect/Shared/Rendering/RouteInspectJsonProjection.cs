using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;

internal static class RouteInspectJsonProjection
{
    internal static RouteInspectJsonDocument Create(RouteInspectResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new RouteInspectJsonDocument
        {
            SchemaVersion = result.SchemaVersion,
            Command = result.Command,
            Status = RouteInspectJsonNames.Status(result.Status),
            Workspace = result.Workspace is null ? null : Workspace(result.Workspace),
            Result = new RouteInspectJsonResult
            {
                Selection = Selection(result.Selection),
                Identity = result.Identity is null ? null : Identity(result.Identity),
                Profile = result.Profile is null ? null : RouteInspectJsonProfileProjection.Create(result.Profile),
                Observations = result.Observations.Select(Observation).ToArray(),
                Conditions = result.Conditions.Select(Condition).ToArray(),
            },
            Next = result.Next is null
                ? null
                : new RouteInspectJsonNext
                {
                    Command = result.Next.Command,
                    Reason = result.Next.Reason,
                },
        };
    }

    private static RouteInspectJsonWorkspace Workspace(CliWorkspace workspace)
    {
        return new RouteInspectJsonWorkspace
        {
            Path = workspace.LexicalRoot,
            SelectedBy = WorkspaceSelectionWireVocabulary.Read(workspace.SelectedBy),
        };
    }

    private static RouteInspectJsonSelection Selection(RouteInspectSelection selection)
    {
        return new RouteInspectJsonSelection
        {
            ReferenceKind = RouteInspectJsonNames.ReferenceKind(selection.ReferenceKind),
            SelectionMethod = RouteInspectJsonNames.SelectionMethod(selection.SelectionMethod),
            RequestedReference = selection.RequestedReference,
            CandidatePaths = selection.CandidatePaths.ToArray(),
        };
    }

    private static RouteInspectJsonIdentity Identity(RouteInspectIdentity identity)
    {
        return new RouteInspectJsonIdentity
        {
            Id = identity.Id,
            Path = identity.CanonicalWorkspaceRelativePath,
            SourceKind = RouteInspectJsonNames.SourceKind(identity.Kind),
            SourceForm = RouteInspectJsonNames.SourceForm(identity.Form),
            RouteState = RouteInspectJsonNames.RouteState(identity.RouteState),
            PhysicalLayers = identity.PhysicalLayers.Select(layer => new RouteInspectJsonPhysicalLayer
            {
                WorkspaceRelativePath = layer.WorkspaceRelativePath,
                PhysicalPath = layer.PhysicalPath,
                Role = RouteInspectJsonNames.LayerRole(layer.Role),
            }).ToArray(),
        };
    }

    private static RouteInspectJsonObservation Observation(RouteInspectObservation observation)
    {
        return new RouteInspectJsonObservation
        {
            Code = observation.MachineCode,
            Subject = observation.Subject,
            Message = observation.Message,
            Paths = observation.Paths.ToArray(),
        };
    }

    private static RouteInspectJsonCondition Condition(RouteInspectCondition condition)
    {
        return new RouteInspectJsonCondition
        {
            Code = condition.MachineCode,
            Status = RouteInspectJsonNames.Status(condition.Status),
            Subject = condition.Subject,
            Message = condition.Message,
            Paths = condition.Paths.ToArray(),
        };
    }
}
