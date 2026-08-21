using System.CommandLine;
using System.CommandLine.Parsing;
using System.Globalization;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
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
    internal static RouteListSymbols CreateSymbols()
    {
        var sourceReference = new Argument<string?>(RouteListDefinitions.SourceReference.Name)
        {
            Description = RouteListDefinitions.SourceReference.Description,
            Arity = ArgumentArity.ZeroOrOne,
        };
        var depth = new Option<string?>(RouteListDefinitions.Depth.Name)
        {
            Description = RouteListDefinitions.Depth.Description,
            HelpName = RouteListDefinitions.Depth.ValueName,
            Arity = ArgumentArity.ExactlyOne,
            DefaultValueFactory = _ => RouteListDefinitions.Depth.DefaultValue,
        };
        var list = new Command(
            RouteListDefinitions.ListCommand.Name,
            RouteListDefinitions.ListCommand.Description);
        list.Arguments.Add(sourceReference);
        list.Options.Add(depth);
        var route = new Command(
            RouteListDefinitions.RouteGroup.Name,
            RouteListDefinitions.RouteGroup.Description);
        route.SetAction(static _ => 0);
        route.Subcommands.Add(list);
        return new RouteListSymbols(
            route,
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
        CliInvalidResultFactory<RouteListResult> invalidResultFactory,
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

    internal static CliInvalidResultFactory<RouteListResult> CreateInvalidResultFactory()
    {
        return static (invalidInput, input, environment) => CreateWorkspaceInvalidResult(
            invalidInput,
            input,
            environment);
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
        var depthSpelling = ReadDepthSpelling(originalArguments, parseResult, symbols.Depth);
        if (!TryParseDepth(depthSpelling, out var requestedDepth))
        {
            return CliBindResult<RouteListRequest, RouteListResult>.Invalid(
                CreateInvalidDepthResult(invocation.Workspace, sourceReference, depthSpelling));
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

    private static RouteListResult CreateInvalidDepthResult(
        CliWorkspace? workspace,
        string? sourceReference,
        string? depthSpelling)
    {
        var selection = ReadAttemptedSelection(sourceReference);
        var subject = string.IsNullOrWhiteSpace(depthSpelling)
            ? "--depth"
            : RouteListTextEscaping.Clamp(depthSpelling, RouteListTextEscaping.ShortValueLimit);
        var finding = new RouteListFinding(
            RouteListFindingCode.InvalidDepth,
            CliSemanticStatus.Invalid,
            subject,
            "Depth must be a non-negative Int32 or the exact value all.");
        return RouteListResult.Create(
            CliSemanticStatus.Invalid,
            workspace,
            selection,
            RouteListCoverage.NotStarted(null),
            [],
            [finding],
            new CliNextAction(
                "open-forge route list --help",
                "Use a non-negative Int32 depth or all, then rerun the operation."));
    }

    private static RouteListResult CreateWorkspaceInvalidResult(
        CliInvalidInput invalidInput,
        CliGlobalInput input,
        CliProcessEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(invalidInput);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(environment);
        var cause = invalidInput.Diagnostics.Count == 1
            ? invalidInput.Diagnostics[0]
            : string.Join(" ", invalidInput.Diagnostics);
        return RouteListResult.Create(
            CliSemanticStatus.Invalid,
            null,
            RouteListSelectionFactory.LoaderRoots(),
            RouteListCoverage.NotStarted(null),
            [],
            [new RouteListFinding(
                RouteListFindingCode.InvalidWorkspace,
                CliSemanticStatus.Invalid,
                input.WorkspaceValue ?? environment.CurrentDirectory,
                cause)],
            new CliNextAction(
                "open-forge route list --help",
                "Select an available directory with --workspace, then rerun the operation."));
    }

    private static RouteListSelection ReadAttemptedSelection(string? sourceReference)
    {
        var parsed = RouteListSourceReferenceParser.Parse(sourceReference);
        return parsed.Kind switch
        {
            RouteListSourceReferenceKind.LoaderRoots => RouteListSelectionFactory.LoaderRoots(),
            RouteListSourceReferenceKind.SourceId when parsed.AttemptedId is not null
                => RouteListSelectionFactory.AttemptedId(parsed.AttemptedId),
            RouteListSourceReferenceKind.SourcePath when parsed.AttemptedPath is not null
                => RouteListSelectionFactory.AttemptedPath(parsed.AttemptedPath),
            _ => RouteListSelectionFactory.LoaderRoots(),
        };
    }

    private static string? ReadDepthSpelling(
        IReadOnlyList<string> originalArguments,
        ParseResult parseResult,
        Option<string?> depth)
    {
        var prefix = $"{RouteListDefinitions.Depth.Name}=";
        var original = originalArguments
            .TakeWhile(value => !string.Equals(value, "--", StringComparison.Ordinal))
            .FirstOrDefault(value => value.StartsWith(prefix, StringComparison.Ordinal));
        if (original is not null)
        {
            return original[prefix.Length..];
        }

        try
        {
            return parseResult.GetValue(depth);
        }
        catch (InvalidOperationException)
        {
            return string.Empty;
        }
    }
}
