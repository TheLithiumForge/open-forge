using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Links;

internal sealed record ContextLinkDestinationResolution
{
    public required SourceLinkDestinationFacts Facts { get; init; }

    public required bool CaseMismatch { get; init; }
}

internal static class ContextLinkCaseMismatchResolver
{
    internal static async ValueTask<ContextLinkDestinationResolution> ResolveAsync(
        SourceLinkDestinationInput input,
        SourceLinkDestinationFacts initial,
        SourceLinkDestinationResolver resolver,
        CancellationToken cancellationToken)
    {
        if (initial.Target.Resolution is not (
                SourceLinkTargetResolution.Complete
                or SourceLinkTargetResolution.Missing
                or SourceLinkTargetResolution.FragmentMissing)
            || initial.Target.Path is not { } attemptedPath
            || TryReadActualPath(input.Workspace, attemptedPath, cancellationToken) is not { } actualPath)
        {
            return new ContextLinkDestinationResolution
            {
                Facts = initial,
                CaseMismatch = false,
            };
        }

        var corrected = CorrectedDestination(input.SourceCanonicalPath, actualPath, input.RawDestination);
        var facts = await resolver.ResolveAsync(
            input with { RawDestination = corrected },
            cancellationToken).ConfigureAwait(false);
        if (facts.Target.Resolution is SourceLinkTargetResolution.Complete
            or SourceLinkTargetResolution.FragmentMissing)
        {
            return new ContextLinkDestinationResolution
            {
                Facts = facts,
                CaseMismatch = true,
            };
        }

        return new ContextLinkDestinationResolution
        {
            Facts = initial,
            CaseMismatch = false,
        };
    }

    private static string? TryReadActualPath(
        CliWorkspace workspace,
        string attemptedPath,
        CancellationToken cancellationToken)
    {
        var segments = attemptedPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            return null;
        }

        var physicalDirectory = workspace.PhysicalRoot;
        var lexicalDirectory = workspace.LexicalRoot;
        var actualSegments = new List<string>(segments.Length);
        var mismatch = false;
        var physicalResolver = new PhysicalPathResolver();
        for (var index = 0; index < segments.Length; index++)
        {
            var entries = DirectoryEntryEnumerator.Enumerate(
                physicalDirectory,
                actualSegments.Count == 0 ? "." : string.Join('/', actualSegments),
                cancellationToken);
            if (entries.State == DirectoryEnumerationState.Cancelled)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            if (entries.State != DirectoryEnumerationState.Complete || entries.Entries is null)
            {
                return null;
            }

            var matches = entries.Entries
                .Where(entry => string.Equals(
                    Path.GetFileName(entry),
                    segments[index],
                    StringComparison.OrdinalIgnoreCase))
                .OrderBy(entry => Path.GetFileName(entry), StringComparer.Ordinal)
                .ThenBy(entry => entry, StringComparer.Ordinal)
                .ToArray();
            if (matches.Length != 1)
            {
                return null;
            }

            var actualName = Path.GetFileName(matches[0]);
            mismatch |= !string.Equals(actualName, segments[index], StringComparison.Ordinal);
            actualSegments.Add(actualName);
            var lexicalCandidate = Path.Combine(lexicalDirectory, actualName);
            var physical = physicalResolver.ResolveCandidate(
                workspace.LexicalRoot,
                workspace.PhysicalRoot,
                lexicalCandidate);
            if (physical.State != PhysicalPathState.Contained)
            {
                return null;
            }

            var physicalCandidate = physical.GetContainedPhysicalPath();
            var component = LinkTargetReader.Read(physicalCandidate);
            if (component.State != PathComponentState.Ordinary || component.Attributes is not { } attributes)
            {
                return null;
            }

            var last = index == segments.Length - 1;
            if ((!last && (attributes & FileAttributes.Directory) == 0)
                || (last && (attributes & FileAttributes.Directory) != 0))
            {
                return null;
            }

            physicalDirectory = physicalCandidate;
            lexicalDirectory = lexicalCandidate;
        }

        return mismatch ? string.Join('/', actualSegments) : null;
    }

    private static string CorrectedDestination(
        string sourceLayerPath,
        string actualPath,
        string originalDestination)
    {
        var sourceDirectory = Path.GetDirectoryName(sourceLayerPath)?.Replace(
            Path.DirectorySeparatorChar,
            '/');
        var relative = Path.GetRelativePath(sourceDirectory ?? ".", actualPath)
            .Replace(Path.DirectorySeparatorChar, '/');
        var encoded = string.Join('/', relative.Split('/').Select(Uri.EscapeDataString));
        var fragmentIndex = originalDestination.IndexOf('#');
        return fragmentIndex < 0
            ? encoded
            : encoded + originalDestination[fragmentIndex..];
    }
}
