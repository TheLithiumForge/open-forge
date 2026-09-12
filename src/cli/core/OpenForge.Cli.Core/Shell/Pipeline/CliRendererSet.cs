using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Shell.Pipeline;

internal sealed class CliRendererSet<TResult>
    where TResult : ICliCommandResult
{
    private readonly CliRenderer<TResult>[] _renderers;

    internal CliRendererSet(
        CliRenderer<TResult> human,
        CliRenderer<TResult> json)
    {
        ArgumentNullException.ThrowIfNull(human);
        ArgumentNullException.ThrowIfNull(json);
        _renderers = [human, json];
    }

    internal CliRenderer<TResult> Read(CliOutputFormat format)
    {
        var index = (int)format;
        if ((uint)index >= (uint)_renderers.Length || !Enum.IsDefined(format))
        {
            throw new ArgumentOutOfRangeException(nameof(format), format, "The output format is not defined.");
        }

        return _renderers[index];
    }
}
