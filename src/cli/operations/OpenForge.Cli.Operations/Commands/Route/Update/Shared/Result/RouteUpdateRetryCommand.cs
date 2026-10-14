using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Shared.Retry;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Result;

internal static class RouteUpdateRetryCommand
{
    internal static string From(RouteUpdateRequest request)
    {
        if (request.RequestCommand is { } command)
        {
            return command;
        }
        var arguments = new List<string> { "route", "update", request.SourceReference };
        var patch = request.Patch;
        if (patch.Description.Requested)
        {
            RouteRetryCommand.Add(arguments, "--description", patch.Description.Value);
        }
        if (patch.Responsibility.Operation != RouteUpdateResponsibilityOperation.NotRequested)
        {
            RouteRetryCommand.Add(arguments, "--responsibility", patch.Responsibility.Value ?? string.Empty);
        }
        if (patch.Tags.Requested)
        {
            if (patch.Tags.Values.IsEmpty)
            {
                arguments.Add("--tag");
            }
            foreach (var tag in patch.Tags.Values)
            {
                RouteRetryCommand.Add(arguments, "--tag", tag);
            }
        }
        if (patch.ApplyTo.Operation == RouteUpdateApplyToOperation.Clear)
        {
            arguments.Add("--clear-apply-to");
        }
        foreach (var pattern in patch.ApplyTo.Values)
        {
            RouteRetryCommand.Add(arguments, "--apply-to", pattern);
        }
        RouteRetryCommand.Add(arguments, "--template", request.TemplateReference);
        return RouteRetryCommand.Build(arguments, request.Workspace, request.IsDryRun);
    }
}
