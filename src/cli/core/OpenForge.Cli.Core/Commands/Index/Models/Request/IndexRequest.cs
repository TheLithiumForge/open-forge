using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Index.Models.Request;

internal enum IndexMode
{
    Apply,
    DryRun,
}

internal sealed record IndexRequest
{
    internal IndexRequest(
        CliWorkspace workspace,
        IEnumerable<string> sourceReferences,
        IndexMode mode)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(sourceReferences);
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode), mode, "The Index mode is not defined.");
        }

        var sources = sourceReferences
            .Select(value => value ?? throw new ArgumentException(
                "Index source references cannot contain null members.",
                nameof(sourceReferences)))
            .ToArray();
        Workspace = workspace;
        SourceReferences = new ReadOnlyCollection<string>(sources);
        Mode = mode;
    }

    internal CliWorkspace Workspace { get; }

    internal IReadOnlyList<string> SourceReferences { get; }

    internal IndexMode Mode { get; }

    internal bool HasExplicitSources => SourceReferences.Count != 0;

    internal bool IsDryRun => Mode == IndexMode.DryRun;
}
