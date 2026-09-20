using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Shell.Pipeline;

internal delegate ValueTask<TResult> CliOperation<TRequest, TResult>(
    TRequest request,
    CancellationToken cancellationToken)
    where TResult : ICliCommandResult;

internal delegate string CliRenderer<TResult>(
    CliPresentationRequest<TResult> presentation)
    where TResult : ICliCommandResult;

internal delegate IReadOnlyList<string>? CliDiagnosticRenderer<TResult>(
    CliPresentationRequest<TResult> presentation)
    where TResult : ICliCommandResult;
