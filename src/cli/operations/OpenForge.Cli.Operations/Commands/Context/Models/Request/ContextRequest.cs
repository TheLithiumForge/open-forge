using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Framework.Workspace.Models;
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
        CliDetail? suppliedDetail,
        CliDetail effectiveView)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(sourceReferences);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(linkExpansion);
        if (suppliedDetail is { } supplied && !Enum.IsDefined(supplied))
        {
            throw new ArgumentOutOfRangeException(nameof(suppliedDetail), suppliedDetail, "The supplied Context view is not defined.");
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
        SuppliedDetail = suppliedDetail;
        EffectiveView = effectiveView;
    }

    internal CliWorkspace Workspace { get; }

    internal IReadOnlyList<string> SourceReferences { get; }

    internal bool AdditionsOnly { get; }

    internal ContextContentSelection Content { get; }

    internal ContextLinkExpansion LinkExpansion { get; }

    internal CliDetail? SuppliedDetail { get; }

    internal CliDetail EffectiveView { get; }
}
