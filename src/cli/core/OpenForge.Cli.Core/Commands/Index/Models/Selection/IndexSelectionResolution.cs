using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Index.Models.Selection;

internal sealed record IndexSelectionResolution
{
    internal IndexSelectionResolution(
        IndexSelection selection,
        IEnumerable<SourceLogicalSource> targets,
        IEnumerable<IndexFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(targets);
        ArgumentNullException.ThrowIfNull(findings);
        var targetValues = targets
            .Select(target => target ?? throw new ArgumentException(
                "Index selection targets cannot contain null members.",
                nameof(targets)))
            .OrderBy(target => target.Identity.CanonicalBasePath, StringComparer.Ordinal)
            .ToArray();
        var findingValues = findings
            .Select(finding => finding ?? throw new ArgumentException(
                "Index selection findings cannot contain null members.",
                nameof(findings)))
            .ToArray();
        if (targetValues.Select(target => target.Identity.CanonicalBasePath)
            .Distinct(StringComparer.Ordinal)
            .Count() != targetValues.Length)
        {
            throw new ArgumentException("Index selection targets require unique canonical paths.", nameof(targets));
        }
        if (targetValues.Any(target => target.Base.Form != SourceDocumentForm.Loader
            && !SourceFormClassifier.IsEntrypoint(target.Base.Form)))
        {
            throw new ArgumentException(
                "Index selection targets must be the Loader or recognized entrypoints.",
                nameof(targets));
        }
        if (targetValues.Select(target => target.Base.PhysicalPath)
            .Distinct(PhysicalIdentityTracker.PathComparer)
            .Count() != targetValues.Length)
        {
            throw new ArgumentException(
                "Index selection targets require unique current-host physical identities.",
                nameof(targets));
        }
        if (findingValues.Length != 0 && targetValues.Length != 0)
        {
            throw new ArgumentException(
                "An incomplete Index selection cannot expose executable targets.",
                nameof(targets));
        }
        if (findingValues.Length == 0
            && (selection.Scope == IndexSelectionScope.NotEstablished || targetValues.Length == 0))
        {
            throw new ArgumentException(
                "A complete Index selection requires an established selection and at least one target.",
                nameof(targets));
        }

        Selection = selection;
        Targets = new ReadOnlyCollection<SourceLogicalSource>(targetValues);
        Findings = new ReadOnlyCollection<IndexFinding>(IndexResult.OrderFindings(findingValues).ToArray());
    }

    internal IndexSelection Selection { get; }

    internal IReadOnlyList<SourceLogicalSource> Targets { get; }

    internal IReadOnlyList<IndexFinding> Findings { get; }

    internal bool IsComplete => Findings.Count == 0;
}
