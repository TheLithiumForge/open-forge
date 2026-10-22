using System.CommandLine;
using System.Collections.Immutable;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Binding;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Result;

using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;

namespace OpenForge.Cli.Core.Commands.Route.Exclude.Shared.Binding;

internal static class RouteExcludeRequestBinder
{
    internal static CliBindResult<RouteExcludeRequest, RouteExcludeResult> Bind(ParseResult parse, CliInvocation invocation, RouteExcludeSymbols symbols)
    {
        var file = parse.GetValue(symbols.File) ?? "workspace-file";
        CliBindResult<RouteExcludeRequest, RouteExcludeResult> Invalid(string cause)
            => CliBindResult<RouteExcludeRequest, RouteExcludeResult>.Invalid(RouteExcludeInvalidResultFactory.Invalid(invocation.Workspace, file, cause));
        if (parse.Errors.Count > 0) return Invalid(string.Join(" ", parse.Errors.Select(error => error.Message)));
        if (string.IsNullOrWhiteSpace(parse.GetValue(symbols.File))) return Invalid("Supply one workspace-relative file.");
        var workspace = invocation.Workspace ?? throw new InvalidOperationException("A selected workspace is required.");
        return CliBindResult<RouteExcludeRequest, RouteExcludeResult>.Bound(new RouteExcludeRequest
        {
            Workspace = workspace,
            File = file,
            DryRun = parse.GetValue(symbols.DryRun),
        });
    }
}
