using System.Text;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Route.Shared.References;

internal sealed partial class RouteMarkdownCatalogueReader
{
    private async ValueTask<bool> TraverseAsync(
        ReadState state,
        CancellationToken cancellationToken)
    {
        while (state.TryDequeue(out var directory))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return true;
            }

            if (!state.MarkDirectory(directory.PhysicalPath))
            {
                state.AddFinding(
                    RouteMarkdownCatalogueFindingCode.MarkdownPathUnsafe,
                    directory.CanonicalPath,
                    "The Markdown directory resolves through an already inspected physical boundary.");
                continue;
            }

            var enumeration = DirectoryEntryEnumerator.Enumerate(
                directory.PhysicalPath,
                directory.CanonicalPath,
                cancellationToken);
            if (enumeration.State == DirectoryEnumerationState.Cancelled)
            {
                return true;
            }

            if (enumeration.State != DirectoryEnumerationState.Complete
                || enumeration.Entries is not { } entries)
            {
                state.AddFinding(
                    RouteMarkdownCatalogueFindingCode.EnumerationUnavailable,
                    directory.CanonicalPath,
                    enumeration.Failure?.DirectCause
                        ?? "The Markdown directory could not be enumerated.");
                continue;
            }

            foreach (var lexicalEntry in entries
                .OrderBy(Path.GetFileName, StringComparer.Ordinal)
                .ThenBy(path => path, StringComparer.Ordinal))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return true;
                }

                var name = Path.GetFileName(lexicalEntry);
                if (!SourceLogicalPath.IsCanonicalSegment(name))
                {
                    state.AddFinding(
                        RouteMarkdownCatalogueFindingCode.MarkdownPathUnsafe,
                        directory.CanonicalPath,
                        "The Markdown directory contains a filesystem name that cannot be represented as a canonical workspace-relative path.");
                    continue;
                }

                var canonicalPath = directory.CanonicalPath == "."
                    ? name
                    : $"{directory.CanonicalPath}/{name}";
                if (state.IsExcluded(canonicalPath))
                {
                    continue;
                }

                var component = LinkTargetReader.Read(lexicalEntry);
                var resolution = Resolve(state.Request, canonicalPath);
                if (component.State != PathComponentState.Ordinary
                    || component.Attributes is not { } attributes
                    || resolution.State != PhysicalPathState.Contained)
                {
                    state.AddFinding(
                        RouteMarkdownCatalogueFindingCode.MarkdownPathUnsafe,
                        canonicalPath,
                        component.Failure?.DirectCause
                            ?? resolution.Failure?.DirectCause
                            ?? "The Markdown path is not one ordinary contained filesystem target.");
                    continue;
                }

                var physicalPath = resolution.GetContainedPhysicalPath();
                if ((attributes & FileAttributes.Directory) != 0)
                {
                    state.Enqueue(new DirectoryBoundary(canonicalPath, physicalPath));
                    continue;
                }

                if (state.Request.Filters.SupportedExtensions.Contains(
                    Path.GetExtension(name),
                    StringComparer.Ordinal))
                {
                    await InspectMarkdownAsync(
                        state,
                        canonicalPath,
                        physicalPath,
                        cancellationToken).ConfigureAwait(false);
                }
            }
        }

        return cancellationToken.IsCancellationRequested;
    }

    private static async ValueTask InspectMarkdownAsync(
        ReadState state,
        string canonicalPath,
        string physicalPath,
        CancellationToken cancellationToken)
    {
        if (!state.Request.Filters.SupportedExtensions.Contains(
                Path.GetExtension(canonicalPath),
                StringComparer.Ordinal))
        {
            return;
        }

        try
        {
            var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken)
                .ConfigureAwait(false);
            _ = StrictUtf8.GetString(bytes);
            state.AddSelected(canonicalPath);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            state.MarkInterrupted();
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException
            or IOException
            or DecoderFallbackException)
        {
            state.AddFinding(
                RouteMarkdownCatalogueFindingCode.MarkdownInspectionUnavailable,
                canonicalPath,
                exception.Message);
        }
    }
}
