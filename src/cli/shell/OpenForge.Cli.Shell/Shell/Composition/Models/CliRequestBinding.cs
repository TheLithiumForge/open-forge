using System.CommandLine;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Shell.Composition.Models;

internal sealed record CliRequestBinding<TRequest, TResult> where TResult : ICliCommandResult
{
    public required Command Command { get; init; }
    public required CliHelpContent Help { get; init; }
    public required CliWorkspaceRequirement WorkspaceRequirement { get; init; }
    public required CliRequestBinder<TRequest, TResult> Binder { get; init; }
    public required CliContextualInvalidResultFactory<TResult> InvalidResultFactory { get; init; }
    public required CliOperation<TRequest, TResult> Operation { get; init; }
}
