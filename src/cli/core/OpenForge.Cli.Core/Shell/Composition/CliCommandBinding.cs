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

internal sealed class CliCommandBinding<TRequest, TResult> : ICliCommandBinding
    where TResult : ICliCommandResult
{
    private readonly CliRequestBinder<TRequest, TResult> _binder;
    private readonly CliContextualInvalidResultFactory<TResult> _invalidResultFactory;
    private readonly CliCommandPipeline<TRequest, TResult> _pipeline;

    internal CliCommandBinding(
        Command command,
        CliCommandBindingComponents<TRequest, TResult> components)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(components);
        ArgumentNullException.ThrowIfNull(components.Help);
        ArgumentNullException.ThrowIfNull(components.Binder);
        ArgumentNullException.ThrowIfNull(components.InvalidResultFactory);
        if (!Enum.IsDefined(components.WorkspaceRequirement))
        {
            throw new ArgumentOutOfRangeException(
                nameof(components.WorkspaceRequirement),
                components.WorkspaceRequirement,
                "The workspace requirement is not defined.");
        }

        Command = command;
        Help = components.Help;
        WorkspaceRequirement = components.WorkspaceRequirement;
        _binder = components.Binder;
        _invalidResultFactory = components.InvalidResultFactory;
        _pipeline = new CliCommandPipeline<TRequest, TResult>(
            components.Operation,
            components.Renderers,
            components.DiagnosticRenderer);
    }

    public Command Command { get; }

    public CliHelpContent Help { get; }

    public CliWorkspaceRequirement WorkspaceRequirement { get; }

    public async ValueTask<CliProcessCompletion> InvokeAsync(
        CliBindingParse parse,
        CliInvocation invocation,
        CliOutputWriters writers,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(parse);
        ArgumentNullException.ThrowIfNull(invocation);
        var bound = _binder(parse, invocation);
        ArgumentNullException.ThrowIfNull(bound);
        if (bound.InvalidResult is not null)
        {
            return await _pipeline
                .PresentAsync(bound.InvalidResult, invocation.Presentation, writers)
                .ConfigureAwait(false);
        }

        if (bound.Request is null)
        {
            throw new InvalidOperationException("A binding must return a request or a concrete invalid result.");
        }

        return await _pipeline
            .ExecuteAsync(bound.Request, invocation.Presentation, writers, cancellationToken)
            .ConfigureAwait(false);
    }

    public ValueTask<CliProcessCompletion> PresentInvalidAsync(
        CliInvalidBindingInput input,
        CliOutputWriters writers,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(writers);
        var result = _invalidResultFactory(input);
        ArgumentNullException.ThrowIfNull(result);
        return _pipeline.PresentAsync(result, input.GlobalInput.Presentation, writers);
    }

}
