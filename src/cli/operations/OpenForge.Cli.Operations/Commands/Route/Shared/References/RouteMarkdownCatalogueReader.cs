using System.Text;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;

namespace OpenForge.Cli.Core.Commands.Route.Shared.References;

internal sealed partial class RouteMarkdownCatalogueReader(PhysicalPathResolver physicalPathResolver)
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;

    internal async ValueTask<RouteMarkdownCatalogue> ReadAsync(
        RouteMarkdownCatalogueRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var state = new ReadState(request);
        if (cancellationToken.IsCancellationRequested)
        {
            return state.Form(interrupted: true);
        }

        if (!ValidateExclusions(state))
        {
            return state.Form(interrupted: false);
        }

        foreach (var includedRoot in request.IncludedRoots)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return state.Form(interrupted: true);
            }

            if (state.IsExcluded(includedRoot))
            {
                continue;
            }

            var resolution = Resolve(request, includedRoot);
            if (resolution.State != PhysicalPathState.Contained)
            {
                state.AddFinding(
                    resolution.State == PhysicalPathState.Missing
                        ? RouteMarkdownCatalogueFindingCode.IncludedRootUnavailable
                        : RouteMarkdownCatalogueFindingCode.IncludedRootUnsafe,
                    includedRoot,
                    ReadResolutionCause(resolution, "The included Markdown root is unavailable."));
                continue;
            }

            var physicalPath = resolution.GetContainedPhysicalPath();
            var component = LinkTargetReader.Read(physicalPath);
            if (component.State != PathComponentState.Ordinary
                || component.Attributes is not { } attributes)
            {
                state.AddFinding(
                    RouteMarkdownCatalogueFindingCode.IncludedRootUnsafe,
                    includedRoot,
                    component.Failure?.DirectCause
                        ?? "The included Markdown root is not an ordinary filesystem target.");
                continue;
            }

            if ((attributes & FileAttributes.Directory) == 0)
            {
                await InspectMarkdownAsync(
                    state,
                    includedRoot,
                    physicalPath,
                    cancellationToken).ConfigureAwait(false);
                continue;
            }

            state.Enqueue(new DirectoryBoundary(includedRoot, physicalPath));
            if (await TraverseAsync(state, cancellationToken).ConfigureAwait(false))
            {
                return state.Form(interrupted: true);
            }
        }

        return state.Form(interrupted: false);
    }

    private bool ValidateExclusions(ReadState state)
    {
        foreach (var excludedPath in state.Request.ExcludedPaths)
        {
            var resolution = Resolve(state.Request, excludedPath);
            if (resolution.State is PhysicalPathState.Contained or PhysicalPathState.Missing)
            {
                continue;
            }

            state.AddFinding(
                RouteMarkdownCatalogueFindingCode.ExcludedPathUnsafe,
                excludedPath,
                ReadResolutionCause(resolution, "The excluded Markdown path is unsafe."));
        }

        return !state.HasBlockedFinding;
    }

    private PhysicalPathResolution Resolve(
        RouteMarkdownCatalogueRequest request,
        string canonicalPath)
        => _physicalPathResolver.ResolveCandidate(
            request.Workspace.LexicalRoot,
            request.Workspace.PhysicalRoot,
            ToLexicalPath(request.Workspace.LexicalRoot, canonicalPath));

    private static string ToLexicalPath(string root, string canonicalPath)
        => canonicalPath == "."
            ? root
            : Path.Combine(root, canonicalPath.Replace('/', Path.DirectorySeparatorChar));

    private static string ReadResolutionCause(
        PhysicalPathResolution resolution,
        string fallback)
        => resolution.Failure?.DirectCause ?? fallback;

}
