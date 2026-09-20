using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Filesystem;

internal sealed class RouteCategoryFilesystemReader(
    PhysicalPathResolver physicalPathResolver,
    FileExpectationValidator expectationValidator)
{
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;
    private readonly FileExpectationValidator _expectationValidator = expectationValidator;

    internal async ValueTask<RouteCategoryFilesystemRead> ReadAsync(
        RouteCategoryFilesystemRequest request,
        CancellationToken cancellationToken)
    {
        var rootLogicalPath = Path.GetDirectoryName(request.EntrypointLogicalPath);
        if (rootLogicalPath is null)
        {
            return Stop(
                RouteCategoryFilesystemReadState.Incomplete,
                "The category entrypoint has no containing directory.");
        }

        var resolution = _physicalPathResolver.ResolveCandidate(
            request.Workspace.LexicalRoot,
            request.Workspace.PhysicalRoot,
            rootLogicalPath);
        if (resolution.State != PhysicalPathState.Contained)
        {
            return Stop(
                IsUnsafe(resolution.State)
                    ? RouteCategoryFilesystemReadState.Unsafe
                    : RouteCategoryFilesystemReadState.Incomplete,
                resolution.Failure?.DirectCause
                    ?? "The category root physical boundary could not be established.");
        }

        var state = new InventoryReadState(
            request,
            rootLogicalPath,
            resolution.GetContainedPhysicalPath());
        await TraverseAsync(state, cancellationToken).ConfigureAwait(false);
        return state.FormResult();
    }

    private async ValueTask TraverseAsync(
        InventoryReadState state,
        CancellationToken cancellationToken)
    {
        while (state.TryDequeue(out var directory))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                state.Stop(
                    RouteCategoryFilesystemReadState.Interrupted,
                    cause: null);
                return;
            }

            string[] entries;
            try
            {
                entries = Directory.GetFileSystemEntries(directory.LogicalPath);
            }
            catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
            {
                state.Stop(
                    RouteCategoryFilesystemReadState.Incomplete,
                    exception.Message);
                return;
            }

            foreach (var logicalPath in entries.Order(StringComparer.Ordinal))
            {
                var entry = state.CreateEntry(logicalPath);
                if (!await ReadEntryAsync(state, entry, cancellationToken).ConfigureAwait(false))
                {
                    return;
                }
            }
        }
    }

    private async ValueTask<bool> ReadEntryAsync(
        InventoryReadState state,
        EntryBoundary entry,
        CancellationToken cancellationToken)
    {
        var component = LinkTargetReader.Read(entry.LogicalPath);
        var resolution = _physicalPathResolver.ResolveCandidate(
            state.Request.Workspace.LexicalRoot,
            state.Request.Workspace.PhysicalRoot,
            entry.LogicalPath);
        if (component.State != PathComponentState.Ordinary
            || component.Attributes is not { } attributes
            || resolution.State != PhysicalPathState.Contained)
        {
            state.Stop(
                RouteCategoryFilesystemReadState.Unsafe,
                component.Failure?.DirectCause
                    ?? resolution.Failure?.DirectCause
                    ?? "A category item has an unsafe physical identity.");
            return false;
        }

        var physicalPath = resolution.GetContainedPhysicalPath();
        if ((attributes & FileAttributes.Directory) != 0)
        {
            state.AddDirectory(entry, physicalPath);
            return true;
        }

        return await ReadFileAsync(
            state,
            entry,
            physicalPath,
            cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<bool> ReadFileAsync(
        InventoryReadState state,
        EntryBoundary entry,
        string physicalPath,
        CancellationToken cancellationToken)
    {
        byte[] bytes;
        try
        {
            bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            state.Stop(
                RouteCategoryFilesystemReadState.Interrupted,
                cause: null);
            return false;
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
        {
            state.Stop(RouteCategoryFilesystemReadState.Incomplete, exception.Message);
            return false;
        }

        var snapshot = FileStateSnapshot.File(entry.LogicalPath, physicalPath, bytes);
        var check = await _expectationValidator.ValidateAsync(
            state.Request.Workspace,
            snapshot.Expectation,
            cancellationToken).ConfigureAwait(false);
        if (check.State != FileExpectationValidationState.Matched)
        {
            state.Stop(ReadFailureState(check.State),
                check.Cause ?? "A category item changed or became unavailable during inventory.");
            return false;
        }

        state.AddFile(entry, snapshot);
        return true;
    }

    private static RouteCategoryFilesystemReadState ReadFailureState(
        FileExpectationValidationState state)
        => state switch
        {
            FileExpectationValidationState.Cancelled => RouteCategoryFilesystemReadState.Interrupted,
            FileExpectationValidationState.Blocked or FileExpectationValidationState.Mismatched =>
                RouteCategoryFilesystemReadState.Unsafe,
            FileExpectationValidationState.Failed => RouteCategoryFilesystemReadState.Incomplete,
            FileExpectationValidationState.Matched => throw new ArgumentOutOfRangeException(
                nameof(state), state, "A matched expectation is not a failure state."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state), state, "The expectation validation state is not defined."),
        };

    private static bool IsUnsafe(PhysicalPathState state)
        => state is PhysicalPathState.Dangling
            or PhysicalPathState.External
            or PhysicalPathState.Cycle
            or PhysicalPathState.Invalid
            or PhysicalPathState.Unsupported;

    private static RouteCategoryFilesystemRead Stop(
        RouteCategoryFilesystemReadState state,
        string? cause)
        => new() { State = state, Cause = cause };

    private static RouteCategoryFilesystemItem ProjectFile(
        RouteCategoryFilesystemRequest request,
        EntryBoundary entry,
        FileStateSnapshot snapshot)
    {
        var source = request.Catalogue.FindByPath(entry.CanonicalPath);
        return new RouteCategoryFilesystemItem
        {
            Kind = ReadItemKind(request, source, entry.CanonicalPath),
            Layer = ReadLayer(source, entry.CanonicalPath),
            SourceId = source?.Identity.AutomaticId,
            SourcePath = snapshot.LogicalPath,
            RelativePath = entry.RelativePath,
            Snapshot = snapshot,
        };
    }

    private static RouteCategoryFilesystemItemKind ReadItemKind(
        RouteCategoryFilesystemRequest request,
        SourceLogicalSource? source,
        string canonicalPath)
    {
        if (request.EntrypointPaths.Contains(canonicalPath, StringComparer.Ordinal))
        {
            return RouteCategoryFilesystemItemKind.Entrypoint;
        }

        if (source?.Base.Form == SourceDocumentForm.Skill)
        {
            return RouteCategoryFilesystemItemKind.NativeSource;
        }

        if (source is not null)
        {
            return request.ExposedPaths.Contains(
                source.Identity.CanonicalBasePath,
                StringComparer.Ordinal)
                ? RouteCategoryFilesystemItemKind.RoutedMarkdown
                : RouteCategoryFilesystemItemKind.UnroutedMarkdown;
        }

        return canonicalPath.EndsWith(".md", StringComparison.Ordinal)
            ? RouteCategoryFilesystemItemKind.UnroutedMarkdown
            : RouteCategoryFilesystemItemKind.Resource;
    }

    private static SourceLayerKind? ReadLayer(
        SourceLogicalSource? source,
        string canonicalPath)
    {
        if (source is null)
        {
            return null;
        }

        return string.Equals(
            source.Overwrite?.CanonicalPath,
            canonicalPath,
            StringComparison.Ordinal)
            ? SourceLayerKind.Overwrite
            : SourceLayerKind.Base;
    }

    private sealed record DirectoryBoundary(string LogicalPath, string PhysicalPath);

    private sealed record EntryBoundary(
        string LogicalPath,
        string CanonicalPath,
        string RelativePath);

    private sealed class InventoryReadState
    {
        private readonly Queue<DirectoryBoundary> _directories = new();
        private readonly List<RouteCategoryFilesystemItem> _items = [];
        private RouteCategoryFilesystemReadState _state =
            RouteCategoryFilesystemReadState.Complete;
        private string? _cause;

        internal InventoryReadState(
            RouteCategoryFilesystemRequest request,
            string rootLogicalPath,
            string rootPhysicalPath)
        {
            Request = request;
            RootLogicalPath = rootLogicalPath;
            _directories.Enqueue(new DirectoryBoundary(rootLogicalPath, rootPhysicalPath));
            _items.Add(new RouteCategoryFilesystemItem
            {
                Kind = RouteCategoryFilesystemItemKind.Directory,
                Layer = null,
                SourceId = null,
                SourcePath = rootLogicalPath,
                RelativePath = ".",
                Snapshot = FileStateSnapshot.Directory(rootLogicalPath, rootPhysicalPath),
            });
        }

        internal RouteCategoryFilesystemRequest Request { get; }

        internal string RootLogicalPath { get; }

        internal bool TryDequeue([NotNullWhen(true)] out DirectoryBoundary? directory)
            => _directories.TryDequeue(out directory);

        internal EntryBoundary CreateEntry(string logicalPath)
            => new(
                logicalPath,
                Path.GetRelativePath(Request.Workspace.LexicalRoot, logicalPath)
                    .Replace(Path.DirectorySeparatorChar, '/'),
                Path.GetRelativePath(RootLogicalPath, logicalPath)
                    .Replace(Path.DirectorySeparatorChar, '/'));

        internal void AddDirectory(EntryBoundary entry, string physicalPath)
        {
            _items.Add(new RouteCategoryFilesystemItem
            {
                Kind = RouteCategoryFilesystemItemKind.Directory,
                Layer = null,
                SourceId = null,
                SourcePath = entry.LogicalPath,
                RelativePath = entry.RelativePath,
                Snapshot = FileStateSnapshot.Directory(entry.LogicalPath, physicalPath),
            });
            _directories.Enqueue(new DirectoryBoundary(entry.LogicalPath, physicalPath));
        }

        internal void AddFile(EntryBoundary entry, FileStateSnapshot snapshot)
            => _items.Add(ProjectFile(Request, entry, snapshot));

        internal void Stop(RouteCategoryFilesystemReadState state, string? cause)
        {
            _state = state;
            _cause = cause;
        }

        internal RouteCategoryFilesystemRead FormResult()
            => _state == RouteCategoryFilesystemReadState.Complete
                ? new RouteCategoryFilesystemRead
                {
                    State = _state,
                    Items = [.. _items.OrderBy(item => item.SourcePath, StringComparer.Ordinal)],
                }
                : new RouteCategoryFilesystemRead
                {
                    State = _state,
                    Cause = _cause,
                };
    }
}
