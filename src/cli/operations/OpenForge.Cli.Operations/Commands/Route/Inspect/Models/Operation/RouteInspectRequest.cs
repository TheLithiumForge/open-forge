using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;

internal sealed record RouteInspectRequest
{
    internal RouteInspectRequest(
        CliWorkspace workspace,
        string sourceReference,
        bool allowInteractiveSourceSelection,
        IEnumerable<string>? workingPaths = null,
        bool matchingFiles = false)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceReference);
        Workspace = workspace;
        MatchingFiles = matchingFiles;
        SourceReference = sourceReference;
        AllowInteractiveSourceSelection = allowInteractiveSourceSelection;
        var materializedPaths = (workingPaths ?? []).ToArray();
        if (materializedPaths.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Working paths cannot be blank.", nameof(workingPaths));
        }

        WorkingPaths = workingPaths is null
            ? null
            : new ReadOnlyCollection<string>(materializedPaths);
    }

    internal CliWorkspace Workspace { get; }

    internal bool MatchingFiles { get; }

    internal string SourceReference { get; }

    internal bool AllowInteractiveSourceSelection { get; }

    internal IReadOnlyList<string>? WorkingPaths { get; }
}
