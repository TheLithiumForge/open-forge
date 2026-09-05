using OpenForge.Cli.Core.Commands.Status.Models.Request;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Invocation;

namespace OpenForge.Cli.Core.Commands.Status;

internal sealed class StatusRequestBinder
{
    internal CliBindResult<StatusRequest, StatusResult> Bind(
        CliBindingParse parse,
        CliInvocation invocation)
    {
        var workspace = invocation.Workspace
            ?? throw new InvalidOperationException(
                "A bound Status invocation requires a selected workspace.");
        return CliBindResult<StatusRequest, StatusResult>.Bound(
            new StatusRequest(workspace));
    }
}
