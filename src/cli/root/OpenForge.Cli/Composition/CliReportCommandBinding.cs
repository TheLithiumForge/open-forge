using System.CommandLine;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Shell.Composition;

internal sealed class CliReportCommandBinding<TRequest, TResult, TData> : ICliCommandBinding
    where TResult : ICliCommandResult where TData : class
{
    private readonly CliRequestBinding<TRequest, TResult> _binding;
    private readonly CliReportPipeline<TRequest, TResult, TData> _pipeline;

    internal CliReportCommandBinding(CliRequestBinding<TRequest, TResult> binding, CliReportRendering<TResult, TData> rendering)
    {
        ArgumentNullException.ThrowIfNull(binding);
        ArgumentNullException.ThrowIfNull(rendering);
        ArgumentNullException.ThrowIfNull(binding.Command);
        ArgumentNullException.ThrowIfNull(binding.Help);
        ArgumentNullException.ThrowIfNull(binding.Binder);
        ArgumentNullException.ThrowIfNull(binding.InvalidResultFactory);
        ArgumentNullException.ThrowIfNull(binding.Operation);
        ArgumentNullException.ThrowIfNull(rendering.Selector);
        ArgumentNullException.ThrowIfNull(rendering.DataTextRenderer);
        ArgumentNullException.ThrowIfNull(rendering.DataJsonTypeInfo);
        _binding = binding;
        _pipeline = new CliReportPipeline<TRequest, TResult, TData>(binding.Operation, rendering);
    }

    public Command Command => _binding.Command;
    public CliHelpContent Help => _binding.Help;
    public CliWorkspaceRequirement WorkspaceRequirement => _binding.WorkspaceRequirement;

    public ValueTask<CliProcessCompletion> InvokeAsync(
        CliBindingParse parse, CliInvocation invocation, CliOutputWriters writers, CancellationToken cancellationToken)
    {
        var bound = _binding.Binder(parse, invocation);
        if (bound.InvalidResult is { } invalid)
        {
            return _pipeline.PresentAsync(invalid, invocation.Presentation, writers);
        }

        if (bound.Request is not { } request)
        {
            throw new InvalidOperationException("A binding must return a request or a concrete invalid result.");
        }

        return _pipeline.ExecuteAsync(request, invocation.Presentation, writers, cancellationToken);
    }

    public ValueTask<CliProcessCompletion> PresentInvalidAsync(
        CliInvalidBindingInput input, CliOutputWriters writers, CancellationToken cancellationToken)
        => _pipeline.PresentAsync(_binding.InvalidResultFactory(input), input.GlobalInput.Presentation, writers);
}
