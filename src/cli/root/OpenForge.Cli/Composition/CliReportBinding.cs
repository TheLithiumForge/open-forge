using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Shell.Composition;

internal static class CliReportBinding
{
    internal static CliReportCommandBinding<TRequest, TResult, TData> Close<TRequest, TResult, TData>(
        CliRequestBinding<TRequest, TResult> binding, CliReportRendering<TResult, TData> rendering)
        where TResult : ICliCommandResult where TData : class
        => new(binding, rendering);
}
