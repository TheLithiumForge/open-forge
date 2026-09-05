using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.References;

internal static class RouteRemoveReferenceChangeProjector
{
    internal static ImmutableArray<PlannedFileChange> BuildFileChanges(
        IEnumerable<RouteRemoveReferenceDocumentPlan> documents)
        => documents
            .Where(document => !string.Equals(
                Encoding.UTF8.GetString(document.Snapshot.Bytes.AsSpan()),
                document.IntendedText,
                StringComparison.Ordinal))
            .OrderBy(document => document.SourcePath, StringComparer.Ordinal)
            .Select(document => PlannedFileChange.Replace(
                document.Snapshot.Expectation,
                Encoding.UTF8.GetBytes(document.IntendedText)))
            .ToImmutableArray();
}
