using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Route.Include.Shared.Binding;

internal static class RouteIncludeInvalidResultFactory
{
    internal static RouteIncludeResult Create(CliInvalidBindingInput input)
        => Invalid(null, "workspace-file", string.Join(" ", input.InvalidInput.Diagnostics));

    internal static RouteIncludeResult Invalid(CliWorkspace? workspace, string file, string cause)
        => new()
        {
            Workspace = workspace,
            Requested = file,
            Outcome = new RouteInclusionOutcome { Status = CliSemanticStatus.Invalid, Code = RouteInclusionFindingCode.InvalidInput, Cause = $"'{file}': {cause}" },
        };
}
