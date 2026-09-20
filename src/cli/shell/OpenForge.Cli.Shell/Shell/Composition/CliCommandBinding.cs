using System.CommandLine;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Shell.Composition;

internal delegate CliBindResult<TRequest, TResult> CliRequestBinder<TRequest, TResult>(
    CliBindingParse parse,
    CliInvocation invocation)
    where TResult : ICliCommandResult;

internal delegate TResult CliContextualInvalidResultFactory<TResult>(
    CliInvalidBindingInput input)
    where TResult : ICliCommandResult;

internal interface ICliCommandBinding
{
    Command Command { get; }

    CliHelpContent Help { get; }

    CliWorkspaceRequirement WorkspaceRequirement { get; }

    ValueTask<CliProcessCompletion> InvokeAsync(
        CliBindingParse parse,
        CliInvocation invocation,
        CliOutputWriters writers,
        CancellationToken cancellationToken);

    ValueTask<CliProcessCompletion> PresentInvalidAsync(
        CliInvalidBindingInput input,
        CliOutputWriters writers,
        CancellationToken cancellationToken);
}
