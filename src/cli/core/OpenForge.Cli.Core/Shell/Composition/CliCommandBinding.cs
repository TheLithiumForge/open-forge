using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Shell.Composition;

internal enum CliWorkspaceRequirement
{
    Required,
    Absent,
}

internal sealed record CliBindingParse(
    ParseResult Result,
    IReadOnlyList<string> OriginalArguments);

internal sealed class CliBindResult<TRequest, TResult>
    where TResult : ICliCommandResult
{
    private CliBindResult(TRequest? request, TResult? invalidResult)
    {
        Request = request;
        InvalidResult = invalidResult;
    }

    internal TRequest? Request { get; }

    internal TResult? InvalidResult { get; }

    internal static CliBindResult<TRequest, TResult> Bound(TRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new CliBindResult<TRequest, TResult>(request, default);
    }

    internal static CliBindResult<TRequest, TResult> Invalid(TResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        CliOperationStage.ValidateResult(result);
        return new CliBindResult<TRequest, TResult>(default, result);
    }
}

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
        CliHelpContent help,
        CliWorkspaceRequirement workspaceRequirement,
        CliRequestBinder<TRequest, TResult> binder,
        CliContextualInvalidResultFactory<TResult> invalidResultFactory,
        CliOperation<TRequest, TResult> operation,
        CliRendererSet<TResult> renderers,
        CliDiagnosticRenderer<TResult>? diagnosticRenderer = null)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(help);
        ArgumentNullException.ThrowIfNull(binder);
        ArgumentNullException.ThrowIfNull(invalidResultFactory);
        if (!Enum.IsDefined(workspaceRequirement))
        {
            throw new ArgumentOutOfRangeException(
                nameof(workspaceRequirement),
                workspaceRequirement,
                "The workspace requirement is not defined.");
        }

        Command = command;
        Help = help;
        WorkspaceRequirement = workspaceRequirement;
        _binder = binder;
        _invalidResultFactory = invalidResultFactory;
        _pipeline = new CliCommandPipeline<TRequest, TResult>(operation, renderers, diagnosticRenderer);
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
