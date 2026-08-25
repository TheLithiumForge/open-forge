using System.CommandLine;
using System.CommandLine.Parsing;
using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.List.Models.Binding;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
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
        RouteListBindingComponents components)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(components);
        ArgumentNullException.ThrowIfNull(components.Operation);
        return new CliCommandBinding<RouteListRequest, RouteListResult>(
            symbols.ListCommand,
            new CliCommandBindingComponents<RouteListRequest, RouteListResult>
            {
                Help = components.Help,
                WorkspaceRequirement = CliWorkspaceRequirement.Required,
                Binder = CreateBinder(symbols),
                InvalidResultFactory = CreateInvalidResultFactory(),
                Operation = components.Operation.Invoke,
                Renderers = components.Renderers,
                DiagnosticRenderer = components.DiagnosticRenderer,
            });
    }

    internal static CliRequestBinder<RouteListRequest, RouteListResult> CreateBinder(
        RouteListSymbols symbols)
    {
        ArgumentNullException.ThrowIfNull(symbols);
        return (parse, invocation) => Bind(
            parse.Result,
            invocation,
            symbols);
    }

    internal static CliContextualInvalidResultFactory<RouteListResult> CreateInvalidResultFactory()
    {
        return RouteListBindingInputPolicy.CreateWorkspaceInvalidResult;
    }

    internal static CliBindResult<RouteListRequest, RouteListResult> Bind(
        ParseResult parseResult,
        CliInvocation invocation,
        RouteListSymbols symbols)
    {
        ArgumentNullException.ThrowIfNull(parseResult);
        ArgumentNullException.ThrowIfNull(invocation);
        ArgumentNullException.ThrowIfNull(symbols);
        var sourceReference = parseResult.GetValue(symbols.SourceReference);
        var depthFacts = CliOptionResultFactsReader.Read(parseResult, symbols.Depth);
        var depthSpelling = depthFacts.IsExplicitWithoutValue
            ? null
            : parseResult.GetValue(symbols.Depth);
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
