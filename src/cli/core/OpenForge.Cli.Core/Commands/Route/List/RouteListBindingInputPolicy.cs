using System.CommandLine.Parsing;
using System.CommandLine;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Parsing;

namespace OpenForge.Cli.Core.Commands.Route.List;

internal static class RouteListBindingInputPolicy
{
    internal static RouteListResult CreateInvalidDepthResult(
        CliWorkspace? workspace,
        string? sourceReference,
        string? depthSpelling)
    {
        var selection = RouteListSelectionFactory.Attempted(sourceReference);
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

    internal static RouteListResult CreateWorkspaceInvalidResult(CliInvalidBindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var cause = input.InvalidInput.Diagnostics.Count == 1
            ? input.InvalidInput.Diagnostics[0]
            : string.Join(" ", input.InvalidInput.Diagnostics);
        return RouteListResult.Create(
            CliSemanticStatus.Invalid,
            null,
            RouteListSelectionFactory.LoaderRoots(),
            RouteListCoverage.NotStarted(null),
            [],
            [new RouteListFinding(
                RouteListFindingCode.InvalidWorkspace,
                CliSemanticStatus.Invalid,
                input.GlobalInput.WorkspaceValue ?? input.ProcessEnvironment.CurrentDirectory,
                cause)],
            new CliNextAction(
                "open-forge route list --help",
                "Select an available directory with --workspace, then rerun the operation."));
    }

    internal static string? ReadDepthSpelling(
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
