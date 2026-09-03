using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal sealed class RouteMoveCategoryFilesystemReader(
    PhysicalPathResolver physicalPathResolver,
    FileExpectationValidator expectationValidator)
{
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;
    private readonly FileExpectationValidator _expectationValidator = expectationValidator;

    internal async ValueTask<RouteMoveCategoryFilesystemReadResult> ReadAsync(
        RouteMoveResolvedSubject subject,
        LifecycleOwnershipReadResult ownership,
        CancellationToken cancellationToken)
    {
        var rootLogicalPath = Path.GetDirectoryName(subject.Layers[0].Snapshot.LogicalPath);
        if (rootLogicalPath is null)
        {
            return Stop(
                RouteMoveCategoryFilesystemReadState.Incomplete,
                "The category entrypoint has no containing directory.");
        }

        var resolution = _physicalPathResolver.ResolveCandidate(
            subject.Request.Workspace.LexicalRoot,
            subject.Request.Workspace.PhysicalRoot,
            rootLogicalPath);
        if (resolution.State != PhysicalPathState.Contained)
        {
            return Stop(
                IsUnsafe(resolution.State)
                    ? RouteMoveCategoryFilesystemReadState.Unsafe
                    : RouteMoveCategoryFilesystemReadState.Incomplete,
                resolution.Failure?.DirectCause
                    ?? "The category root physical boundary could not be established.");
        }

        var state = new InventoryReadState(
            subject,
            ownership,
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
                    RouteMoveCategoryFilesystemReadState.Interrupted,
                    "Route Move category inventory was interrupted.");
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
                    RouteMoveCategoryFilesystemReadState.Incomplete,
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
            state.Subject.Request.Workspace.LexicalRoot,
            state.Subject.Request.Workspace.PhysicalRoot,
            entry.LogicalPath);
        if (component.State != PathComponentState.Ordinary
            || component.Attributes is not { } attributes
            || resolution.State != PhysicalPathState.Contained)
        {
            state.Stop(
                RouteMoveCategoryFilesystemReadState.Unsafe,
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
                RouteMoveCategoryFilesystemReadState.Interrupted,
                "Route Move category inventory was interrupted.");
            return false;
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
        {
            state.Stop(RouteMoveCategoryFilesystemReadState.Incomplete, exception.Message);
            return false;
        }

        var snapshot = FileStateSnapshot.File(entry.LogicalPath, physicalPath, bytes);
        var check = await _expectationValidator.ValidateAsync(
            state.Subject.Request.Workspace,
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

    private static RouteMoveCategoryFilesystemReadState ReadFailureState(
        FileExpectationValidationState state)
        => state switch
        {
            FileExpectationValidationState.Cancelled => RouteMoveCategoryFilesystemReadState.Interrupted,
            FileExpectationValidationState.Blocked or FileExpectationValidationState.Mismatched =>
                RouteMoveCategoryFilesystemReadState.Unsafe,
            FileExpectationValidationState.Failed => RouteMoveCategoryFilesystemReadState.Incomplete,
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

    private static RouteMoveCategoryFilesystemReadResult Stop(
        RouteMoveCategoryFilesystemReadState state,
        string cause)
        => new() { State = state, Cause = cause };

    private static RouteMoveInventoryItem ProjectFile(
        RouteMoveResolvedSubject subject,
        EntryBoundary entry,
        FileStateSnapshot snapshot)
    {
        var source = subject.Catalogue.FindByPath(entry.CanonicalPath);
        return new RouteMoveInventoryItem
        {
            Kind = ReadItemKind(subject, source, entry.CanonicalPath),
            Layer = ReadLayer(source, entry.CanonicalPath),
            SourceId = source?.Identity.AutomaticId,
            SourcePath = snapshot.LogicalPath,
            RelativePath = entry.RelativePath,
            Snapshot = snapshot,
        };
    }

    private static RouteMoveItemKind ReadItemKind(
        RouteMoveResolvedSubject subject,
        SourceLogicalSource? source,
        string canonicalPath)
    {
        if (subject.Layers.Any(layer => string.Equals(
                layer.Layer.CanonicalPath,
                canonicalPath,
                StringComparison.Ordinal)))
        {
            return RouteMoveItemKind.Entrypoint;
        }

        if (source?.Base.Form == SourceDocumentForm.Skill)
        {
            return RouteMoveItemKind.NativeSource;
        }

        if (source is not null)
        {
            return subject.NavigationExposure.ExposedPaths.Contains(
                source.Identity.CanonicalBasePath,
                StringComparer.Ordinal)
                ? RouteMoveItemKind.RoutedMarkdown
                : RouteMoveItemKind.UnroutedMarkdown;
        }

        return canonicalPath.EndsWith(".md", StringComparison.Ordinal)
            ? RouteMoveItemKind.UnroutedMarkdown
            : RouteMoveItemKind.Resource;
    }

    private static RouteMoveLayerKind? ReadLayer(
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
            ? RouteMoveLayerKind.Overwrite
            : RouteMoveLayerKind.Base;
    }

    private sealed record DirectoryBoundary(string LogicalPath, string PhysicalPath);

    private sealed record EntryBoundary(
        string LogicalPath,
        string CanonicalPath,
        string RelativePath);

    private sealed class InventoryReadState
    {
        private readonly Queue<DirectoryBoundary> _directories = new();
        private readonly List<RouteMoveInventoryItem> _items = [];
        private RouteMoveCategoryFilesystemReadState _state =
            RouteMoveCategoryFilesystemReadState.Complete;
        private string? _cause;

        internal InventoryReadState(
            RouteMoveResolvedSubject subject,
            LifecycleOwnershipReadResult ownership,
            string rootLogicalPath,
            string rootPhysicalPath)
        {
            Subject = subject;
            Ownership = ownership;
            RootLogicalPath = rootLogicalPath;
            _directories.Enqueue(new DirectoryBoundary(rootLogicalPath, rootPhysicalPath));
            _items.Add(new RouteMoveInventoryItem
            {
                Kind = RouteMoveItemKind.Directory,
                Layer = null,
                SourceId = null,
                SourcePath = rootLogicalPath,
                RelativePath = ".",
                Snapshot = FileStateSnapshot.Directory(rootLogicalPath, rootPhysicalPath),
            });
        }

        internal RouteMoveResolvedSubject Subject { get; }

        internal LifecycleOwnershipReadResult Ownership { get; }

        internal string RootLogicalPath { get; }

        internal bool TryDequeue([NotNullWhen(true)] out DirectoryBoundary? directory)
            => _directories.TryDequeue(out directory);

        internal EntryBoundary CreateEntry(string logicalPath)
            => new(
                logicalPath,
                Path.GetRelativePath(Subject.Request.Workspace.LexicalRoot, logicalPath)
                    .Replace(Path.DirectorySeparatorChar, '/'),
                Path.GetRelativePath(RootLogicalPath, logicalPath)
                    .Replace(Path.DirectorySeparatorChar, '/'));

        internal void AddDirectory(EntryBoundary entry, string physicalPath)
        {
            _items.Add(new RouteMoveInventoryItem
            {
                Kind = RouteMoveItemKind.Directory,
                Layer = null,
                SourceId = null,
                SourcePath = entry.LogicalPath,
                RelativePath = entry.RelativePath,
                Snapshot = FileStateSnapshot.Directory(entry.LogicalPath, physicalPath),
            });
            _directories.Enqueue(new DirectoryBoundary(entry.LogicalPath, physicalPath));
        }

        internal void AddFile(EntryBoundary entry, FileStateSnapshot snapshot)
            => _items.Add(ProjectFile(Subject, entry, snapshot));

        internal void Stop(RouteMoveCategoryFilesystemReadState state, string cause)
        {
            _state = state;
            _cause = cause;
        }

        internal RouteMoveCategoryFilesystemReadResult FormResult()
            => _state == RouteMoveCategoryFilesystemReadState.Complete
                ? new RouteMoveCategoryFilesystemReadResult
                {
                    State = _state,
                    Inventory = FormInventory(),
                }
                : new RouteMoveCategoryFilesystemReadResult
                {
                    State = _state,
                    Cause = _cause,
                };

        private RouteMoveCategoryInventory FormInventory()
            => new()
            {
                Subject = Subject,
                Ownership = Ownership,
                Items = _items
                    .OrderBy(item => item.SourcePath, StringComparer.Ordinal)
                    .ToImmutableArray(),
            };
    }
}
