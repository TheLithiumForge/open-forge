using System.CommandLine;
using System.CommandLine.Parsing;
using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Commands.Route.Shared.Source;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.List;

internal sealed record RouteListSymbols(
    Command RouteGroup,
    Command ListCommand,
    Argument<string?> SourceReference,
    Option<string?> Depth,
    IReadOnlyList<CliDelimiterPolicy> DelimiterPolicies);

internal static class RouteListBinding
{
    internal static RouteListSymbols CreateSymbols(Command routeGroup)
    {
        ArgumentNullException.ThrowIfNull(routeGroup);
        var sourceReference = new Argument<string?>(RouteListDefinitions.SourceReference.Name)
        {
            Description = RouteListDefinitions.SourceReference.Description,
            Arity = ArgumentArity.ZeroOrOne,
        };
        var depth = new Option<string?>(RouteListDefinitions.Depth.Name)
        {
            Description = RouteListDefinitions.Depth.Description,
            HelpName = RouteListDefinitions.Depth.ValueName,
            // Keep attached-empty depth from consuming a following global option;
            // the binding still rejects every explicit value outside the public grammar.
            Arity = ArgumentArity.ZeroOrOne,
            DefaultValueFactory = _ => RouteListDefinitions.Depth.DefaultValue,
        };
        var list = new Command(
            RouteListDefinitions.ListCommand.Name,
            RouteListDefinitions.ListCommand.Description);
        list.Arguments.Add(sourceReference);
        list.Options.Add(depth);
        routeGroup.Subcommands.Add(list);
        return new RouteListSymbols(
            routeGroup,
            list,
            sourceReference,
            depth,
            Array.AsReadOnly(
            [
                new CliDelimiterPolicy(RouteListDefinitions.Depth.Name, CliDelimiterShape.Equals),
            ]));
    }

    internal static CliCommandBinding<RouteListRequest, RouteListResult> Close(
        RouteListSymbols symbols,
        CliHelpContent help,
        CliRequestBinder<RouteListRequest, RouteListResult> binder,
        CliContextualInvalidResultFactory<RouteListResult> invalidResultFactory,
        RouteListOperation operation,
        CliRendererSet<RouteListResult> renderers,
        CliDiagnosticRenderer<RouteListResult>? diagnosticRenderer = null)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(operation);
        return new CliCommandBinding<RouteListRequest, RouteListResult>(
            symbols.ListCommand,
            help,
            CliWorkspaceRequirement.Required,
            binder,
            invalidResultFactory,
            operation.Invoke,
            renderers,
            diagnosticRenderer);
    }

    internal static CliRequestBinder<RouteListRequest, RouteListResult> CreateBinder(
        RouteListSymbols symbols)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        return (parse, invocation) => Bind(
            parse.Result,
            parse.OriginalArguments,
            invocation,
            symbols);
    }

    internal static CliContextualInvalidResultFactory<RouteListResult> CreateInvalidResultFactory()
    {
        return RouteListBindingInputPolicy.CreateWorkspaceInvalidResult;
    }

    internal static CliBindResult<RouteListRequest, RouteListResult> Bind(
        ParseResult parseResult,
        IReadOnlyList<string> originalArguments,
        CliInvocation invocation,
        RouteListSymbols symbols)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(originalArguments);
        ArgumentNullException.ThrowIfNull(invocation);
        ArgumentNullException.ThrowIfNull(symbols);
        var sourceReference = parseResult.GetValue(symbols.SourceReference);
        var depthSpelling = RouteListBindingInputPolicy.ReadDepthSpelling(
            originalArguments,
            parseResult,
            symbols.Depth);
        if (!TryParseDepth(depthSpelling, out var requestedDepth))
        {
            return CliBindResult<RouteListRequest, RouteListResult>.Invalid(
                RouteListBindingInputPolicy.CreateInvalidDepthResult(
                    invocation.Workspace,
                    sourceReference,
                    depthSpelling));
        }

        var workspace = invocation.Workspace
            ?? throw new InvalidOperationException("The route-list binding requires a selected workspace.");
        return CliBindResult<RouteListRequest, RouteListResult>.Bound(
            new RouteListRequest(workspace, sourceReference, requestedDepth));
    }

    internal static bool TryParseDepth(
        string? spelling,
        out RouteListDepth depth)
    {
        if (string.Equals(spelling, RouteListDefinitions.AllDepth, StringComparison.Ordinal))
        {
            depth = RouteListDepth.All;
            return true;
        }

        if (spelling is not null
            && spelling.Length > 0
            && int.TryParse(
                spelling,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var finiteDepth)
            && finiteDepth >= 0)
        {
            depth = RouteListDepth.Finite(finiteDepth);
            return true;
        }

        depth = RouteListDepth.Default;
        return false;
    }
}
