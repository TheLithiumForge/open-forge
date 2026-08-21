using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Shell.Pipeline;

internal interface ICliCommandResult
{
    string Command { get; }

    CliSemanticStatus Status { get; }

    CliWorkspace? Workspace { get; }

    CliNextAction? Next { get; }
}

internal sealed record CliOperationRequest<TRequest>(TRequest Request);

internal sealed record CliOperationResult<TResult>(TResult Result)
    where TResult : ICliCommandResult;

internal sealed record CliPresentationRequest<TResult>(
    TResult Result,
    CliPresentation Presentation)
    where TResult : ICliCommandResult;

internal sealed record CliRenderedOutput(
    CliSemanticStatus Status,
    CliOutputFormat Format,
    CliOutputTarget PrimaryTarget,
    string PrimaryContent,
    string? DiagnosticContent);

internal sealed record CliOutputReceipt(
    CliSemanticStatus Status,
    CliOutputFormat Format,
    CliOutputTarget PrimaryTarget,
    bool DiagnosticWritten);

internal sealed record CliOutputWriters
{
    internal CliOutputWriters(TextWriter standardOutput, TextWriter standardError)
    {
        ArgumentNullException.ThrowIfNull(standardOutput);
        ArgumentNullException.ThrowIfNull(standardError);
        StandardOutput = standardOutput;
        StandardError = standardError;
    }

    internal TextWriter StandardOutput { get; }

    internal TextWriter StandardError { get; }
}

internal delegate ValueTask<TResult> CliOperation<TRequest, TResult>(
    TRequest request,
    CancellationToken cancellationToken)
    where TResult : ICliCommandResult;

internal delegate string CliRenderer<TResult>(
    CliPresentationRequest<TResult> presentation)
    where TResult : ICliCommandResult;

internal delegate string? CliDiagnosticRenderer<TResult>(
    CliPresentationRequest<TResult> presentation)
    where TResult : ICliCommandResult;
