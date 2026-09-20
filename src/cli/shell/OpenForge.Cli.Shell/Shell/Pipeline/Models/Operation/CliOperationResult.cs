namespace OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

internal sealed record CliOperationResult<TResult>(TResult Result)
    where TResult : ICliCommandResult;
