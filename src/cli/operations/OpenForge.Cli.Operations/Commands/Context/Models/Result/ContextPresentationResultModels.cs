using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Context.Models.Result;

internal sealed record ContextPresentation
{
    internal ContextPresentation(
        CliDetail? suppliedDetail,
        CliDetail effectiveView,
        ContextContentSelection content)
    {
        if (suppliedDetail is { } supplied && !Enum.IsDefined(supplied))
        {
            throw new ArgumentOutOfRangeException(nameof(suppliedDetail), suppliedDetail, "The supplied Context view is not defined.");
        }

        if (!Enum.IsDefined(effectiveView))
        {
            throw new ArgumentOutOfRangeException(nameof(effectiveView), effectiveView, "The effective Context view is not defined.");
        }

        ArgumentNullException.ThrowIfNull(content);
        SuppliedDetail = suppliedDetail;
        EffectiveView = effectiveView;
        Content = content;
    }

    internal CliDetail? SuppliedDetail { get; }

    internal CliDetail EffectiveView { get; }

    internal ContextContentSelection Content { get; }
}
