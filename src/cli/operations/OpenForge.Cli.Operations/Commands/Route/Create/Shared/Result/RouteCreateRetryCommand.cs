using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Shared.Retry;

namespace OpenForge.Cli.Core.Commands.Route.Create.Shared.Result;

internal static class RouteCreateRetryCommand
{
    internal static string From(RouteCreateRequest request)
    {
        if (request.RequestCommand is { } command)
        {
            return command;
        }
        var arguments = new List<string> { "route", "create", request.FileTarget };
        RouteRetryCommand.Add(arguments, "--description", request.Metadata.Description);
        RouteRetryCommand.Add(arguments, "--responsibility", request.Metadata.Responsibility);
        RouteRetryCommand.Add(arguments, "--template", request.TemplateReference);
        foreach (var tag in request.Metadata.Tags)
        {
            RouteRetryCommand.Add(arguments, "--tag", tag);
        }
        foreach (var pattern in request.Metadata.ApplyTo)
        {
            RouteRetryCommand.Add(arguments, "--apply-to", pattern.Text);
        }
        return RouteRetryCommand.Build(arguments, request.Workspace, request.IsDryRun);
    }
}
