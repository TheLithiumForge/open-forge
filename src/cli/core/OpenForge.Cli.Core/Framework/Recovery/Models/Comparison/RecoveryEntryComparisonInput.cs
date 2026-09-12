using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;

internal sealed record RecoveryEntryComparisonContext
{
    internal RecoveryEntryComparisonContext(CliWorkspace workspace, RecoveryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(entry);
        Workspace = workspace;
        Entry = entry;
        LogicalPath = Path.GetFullPath(Path.Combine(
            workspace.LexicalRoot,
            entry.TargetPath.Replace('/', Path.DirectorySeparatorChar)));
    }

    internal CliWorkspace Workspace { get; }
    internal RecoveryEntry Entry { get; }
    internal string LogicalPath { get; }
}

internal sealed record RecoveryEntryComparisonInput
{
    internal RecoveryEntryComparisonInput(
        RecoveryEntryComparisonContext context,
        NoFollowLeafObservation leaf,
        RecoveryOrdinaryContentObservation? ordinaryContent)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(leaf);
        if (!PhysicalIdentityTracker.PathComparer.Equals(context.LogicalPath, leaf.LogicalPath))
        {
            throw new ArgumentException("The recovery leaf observation must name the entry's workspace target.", nameof(leaf));
        }

        if (leaf.State == NoFollowLeafState.OrdinaryFile)
        {
            if (ordinaryContent is null)
            {
                throw new ArgumentException("An ordinary recovery leaf requires independent content-read facts.", nameof(ordinaryContent));
            }

            if (!PhysicalIdentityTracker.PathComparer.Equals(leaf.LogicalPath, ordinaryContent.LogicalPath))
            {
                throw new ArgumentException("Ordinary content-read facts must name the observed recovery leaf.", nameof(ordinaryContent));
            }
        }
        else if (ordinaryContent is not null)
        {
            throw new ArgumentException("Only an ordinary no-follow leaf may carry content-read facts.", nameof(ordinaryContent));
        }

        Context = context;
        Leaf = leaf;
        OrdinaryContent = ordinaryContent;
    }

    internal RecoveryEntryComparisonContext Context { get; }
    internal NoFollowLeafObservation Leaf { get; }
    internal RecoveryOrdinaryContentObservation? OrdinaryContent { get; }
}
