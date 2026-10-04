using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.GitIgnore;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Framework.Libraries.Shared.GitIgnore;

internal static class LibraryGitIgnorePlanner
{
    internal static async ValueTask<LibraryGitIgnoreRead> PlanAsync(LibraryGitIgnoreRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var current = Paths(request.Current);
        var intended = Paths(request.Intended);
        if (current.IsEmpty && intended.IsEmpty) return new(LibraryGitIgnoreReadState.Complete, null, null);
        if ((request.Current?.Libraries ?? []).Concat(request.Intended?.Libraries ?? [])
            .SelectMany(library => LibraryPathIdentity.Mappings(library)).Any(mapping =>
                PortableWorkspacePath.CreatePortableKey(mapping.DestinationPath.Value) == PortableWorkspacePath.CreatePortableKey(LibraryGitIgnoreSection.Path)))
        {
            return Blocked("A projected Library link overlaps the required .gitignore file. Attach that Library with --git-ignore false.");
        }
        var logical = System.IO.Path.Combine(request.Workspace.LexicalRoot, LibraryGitIgnoreSection.Path);
        var resolver = new PhysicalPathResolver();
        var leaf = NoFollowLeafObserver.Observe(resolver, request.Workspace, logical, cancellationToken);
        if (leaf.State == NoFollowLeafState.Inaccessible
            || leaf.State == NoFollowLeafState.Unknown && leaf.Failure?.Kind is FilesystemFailureKind.AccessDenied or FilesystemFailureKind.InputOutput)
        {
            return new(LibraryGitIgnoreReadState.Unavailable, null, "The Library Git-ignore target could not be observed safely.");
        }
        if (leaf.State == NoFollowLeafState.Missing && request.IsRemoval)
        {
            return new(LibraryGitIgnoreReadState.Complete, null, null);
        }
        if (leaf.State is not (NoFollowLeafState.Missing or NoFollowLeafState.OrdinaryFile))
        {
            return Blocked("The Library Git-ignore target must be an ordinary workspace file or an absent file.");
        }
        try
        {
            var physical = System.IO.Path.Combine(request.Workspace.PhysicalRoot, LibraryGitIgnoreSection.Path);
            var before = leaf.State == NoFollowLeafState.Missing
                ? FileStateSnapshot.Missing(logical)
                : FileStateSnapshot.File(logical, physical, await File.ReadAllBytesAsync(physical, cancellationToken).ConfigureAwait(false));
            var confirmed = NoFollowLeafObserver.Observe(resolver, request.Workspace, logical, cancellationToken);
            if (confirmed.State != leaf.State)
            {
                return Blocked("The Library Git-ignore target changed while it was being read.");
            }
            var bytes = LibraryGitIgnoreSection.Rewrite(before.Bytes.AsSpan(), current, intended);
            if (before.Bytes.AsSpan().SequenceEqual(bytes)) return new(LibraryGitIgnoreReadState.Complete, null, null);
            if (request.IsExcluded && !request.IsRemoval)
            {
                return Blocked("The required .gitignore change is excluded by workspace settings.");
            }
            var change = before.Kind == FileExpectationKind.Missing
                ? PlannedFileChange.Create(before.Expectation, bytes)
                : PlannedFileChange.Replace(before.Expectation, bytes);
            return new(LibraryGitIgnoreReadState.Complete, new(change, before, intended), null);
        }
        catch (Exception exception) when (exception is InvalidDataException or DecoderFallbackException)
        {
            return Blocked(exception.Message);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return new(LibraryGitIgnoreReadState.Unavailable, null, "The Library Git-ignore target could not be read safely.");
        }
    }

    private static ImmutableArray<string> Paths(LibraryRegistrationSet? record)
        => [.. (record?.Libraries ?? []).Where(library => library.GitIgnore)
            .SelectMany(library => LibraryPathIdentity.Mappings(library)).Select(mapping => mapping.DestinationPath.Value)
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];

    private static LibraryGitIgnoreRead Blocked(string cause) => new(LibraryGitIgnoreReadState.Blocked, null, cause);
}
