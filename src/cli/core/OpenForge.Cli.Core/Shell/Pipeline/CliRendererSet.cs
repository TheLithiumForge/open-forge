using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

namespace OpenForge.Cli.Core.Shell.Pipeline;

internal sealed class CliRendererSet<TResult>
    where TResult : ICliCommandResult
{
    private readonly CliRenderer<TResult>[] _renderers;

    internal CliRendererSet(
        CliRenderer<TResult> human,
        CliRenderer<TResult> json)
        : this(new CliViewRenderers<TResult>(human, human), new CliViewRenderers<TResult>(json, json))
    {
        ArgumentNullException.ThrowIfNull(human);
        ArgumentNullException.ThrowIfNull(json);
    }

    internal CliRendererSet(
        CliViewRenderers<TResult> human,
        CliViewRenderers<TResult> json)
    {
        _renderers = [human.Render, json.Render];
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
