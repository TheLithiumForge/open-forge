using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Context.Models.Result;

internal sealed record ContextPresentation
{
    internal ContextPresentation(
        CliView? suppliedView,
        CliView effectiveView,
        ContextContentSelection content)
    {
        if (suppliedView is { } supplied && !Enum.IsDefined(supplied))
        {
            throw new ArgumentOutOfRangeException(nameof(suppliedView), suppliedView, "The supplied Context view is not defined.");
        }

        if (!Enum.IsDefined(effectiveView))
        {
            throw new ArgumentOutOfRangeException(nameof(effectiveView), effectiveView, "The effective Context view is not defined.");
        }

        ArgumentNullException.ThrowIfNull(content);
        SuppliedView = suppliedView;
        EffectiveView = effectiveView;
        Content = content;
    }

    internal CliView? SuppliedView { get; }

    internal CliView EffectiveView { get; }

    internal ContextContentSelection Content { get; }
}
