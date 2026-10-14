using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Shared.Retry;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Result;

internal static class RouteInitRetryCommand
{
    internal static string From(RouteInitRequest request, bool diagnostics = false)
    {
        var arguments = new List<string> { "route", "init", request.RouteTarget };
        if (request.Scaffold == RouteInitScaffold.Framework) arguments.Add("--framework");
        CliRetryCommand.Add(arguments, "--description", request.Metadata.Description);
        if (request.Metadata.ResponsibilitySpecified)
            CliRetryCommand.Add(arguments, "--responsibility", request.Metadata.Responsibility);
        foreach (var tag in request.Metadata.Tags) CliRetryCommand.Add(arguments, "--tag", tag);
        foreach (var pattern in request.Metadata.ApplyTo) CliRetryCommand.Add(arguments, "--apply-to", pattern.Text);
        CliRetryCommand.AddPresentation(arguments, request.RetryOptions, diagnostics);
        return RouteRetryCommand.Build(arguments, request.Workspace, request.IsDryRun);
    }
}
