using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Shell.Composition.Models;

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
