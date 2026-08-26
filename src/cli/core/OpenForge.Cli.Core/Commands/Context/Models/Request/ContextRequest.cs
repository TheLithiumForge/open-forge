using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Context.Models.Request;

internal sealed record ContextRequest
{
    internal ContextRequest(
        CliWorkspace workspace,
        IEnumerable<string> sourceReferences,
        bool additionsOnly,
        ContextContentSelection content,
        ContextLinkExpansion linkExpansion,
        CliView? suppliedView,
        CliView effectiveView)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(sourceReferences);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(linkExpansion);
        if (suppliedView is { } supplied && !Enum.IsDefined(supplied))
        {
            throw new ArgumentOutOfRangeException(nameof(suppliedView), suppliedView, "The supplied Context view is not defined.");
        }

        if (!Enum.IsDefined(effectiveView))
        {
            throw new ArgumentOutOfRangeException(nameof(effectiveView), effectiveView, "The effective Context view is not defined.");
        }

        var sources = sourceReferences
            .Select(value => value ?? throw new ArgumentException("Context source references cannot contain null members.", nameof(sourceReferences)))
            .ToArray();
        Workspace = workspace;
        SourceReferences = new ReadOnlyCollection<string>(sources);
        AdditionsOnly = additionsOnly;
        Content = content;
        LinkExpansion = linkExpansion;
        SuppliedView = suppliedView;
        EffectiveView = effectiveView;
    }

    internal CliWorkspace Workspace { get; }

    internal IReadOnlyList<string> SourceReferences { get; }

    internal bool AdditionsOnly { get; }

    internal ContextContentSelection Content { get; }

    internal ContextLinkExpansion LinkExpansion { get; }

    internal CliView? SuppliedView { get; }

    internal CliView EffectiveView { get; }
}
