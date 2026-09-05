using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal sealed class RouteRemoveCategoryFilesystemReader(
    PhysicalPathResolver physicalPathResolver,
    FileExpectationValidator expectationValidator)
{
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;
    private readonly FileExpectationValidator _expectationValidator = expectationValidator;

    internal async ValueTask<RouteRemoveCategoryFilesystemReadResult> ReadAsync(
        RouteRemoveResolvedSubject subject,
        LifecycleOwnershipReadResult ownership,
        CancellationToken cancellationToken)
    {
        var rootLogicalPath = Path.GetDirectoryName(subject.Layers[0].Snapshot.LogicalPath);
        if (rootLogicalPath is null)
        {
            return Stop(
                RouteRemoveCategoryFilesystemReadState.Incomplete,
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
                    ? RouteRemoveCategoryFilesystemReadState.Unsafe
                    : RouteRemoveCategoryFilesystemReadState.Incomplete,
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
                    RouteRemoveCategoryFilesystemReadState.Interrupted,
                    "Route Remove category inventory was interrupted.");
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
                    RouteRemoveCategoryFilesystemReadState.Incomplete,
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
                RouteRemoveCategoryFilesystemReadState.Unsafe,
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
                RouteRemoveCategoryFilesystemReadState.Interrupted,
                "Route Remove category inventory was interrupted.");
            return false;
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
        {
            state.Stop(RouteRemoveCategoryFilesystemReadState.Incomplete, exception.Message);
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

    private static RouteRemoveCategoryFilesystemReadState ReadFailureState(
        FileExpectationValidationState state)
        => state switch
        {
            FileExpectationValidationState.Cancelled => RouteRemoveCategoryFilesystemReadState.Interrupted,
            FileExpectationValidationState.Blocked or FileExpectationValidationState.Mismatched =>
                RouteRemoveCategoryFilesystemReadState.Unsafe,
            FileExpectationValidationState.Failed => RouteRemoveCategoryFilesystemReadState.Incomplete,
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

    private static RouteRemoveCategoryFilesystemReadResult Stop(
        RouteRemoveCategoryFilesystemReadState state,
        string cause)
        => new() { State = state, Cause = cause };

    private static RouteRemoveInventoryItem ProjectFile(
        RouteRemoveResolvedSubject subject,
        EntryBoundary entry,
        FileStateSnapshot snapshot)
    {
        var source = subject.Catalogue.FindByPath(entry.CanonicalPath);
        return new RouteRemoveInventoryItem
        {
            Kind = ReadItemKind(subject, source, entry.CanonicalPath),
            Layer = ReadLayer(source, entry.CanonicalPath),
            SourceId = source?.Identity.AutomaticId,
            SourcePath = snapshot.LogicalPath,
            RelativePath = entry.RelativePath,
            Snapshot = snapshot,
        };
    }

    private static RouteRemoveItemKind ReadItemKind(
        RouteRemoveResolvedSubject subject,
        SourceLogicalSource? source,
        string canonicalPath)
    {
        if (subject.Layers.Any(layer => string.Equals(
                layer.Layer.CanonicalPath,
                canonicalPath,
                StringComparison.Ordinal)))
        {
            return RouteRemoveItemKind.Entrypoint;
        }

        if (source?.Base.Form == SourceDocumentForm.Skill)
        {
            return RouteRemoveItemKind.NativeSource;
        }

        if (source is not null)
        {
            return subject.NavigationExposure.ExposedPaths.Contains(
                source.Identity.CanonicalBasePath,
                StringComparer.Ordinal)
                ? RouteRemoveItemKind.RoutedMarkdown
                : RouteRemoveItemKind.UnroutedMarkdown;
        }

        return canonicalPath.EndsWith(".md", StringComparison.Ordinal)
            ? RouteRemoveItemKind.UnroutedMarkdown
            : RouteRemoveItemKind.Resource;
    }

    private static RouteRemoveLayerKind? ReadLayer(
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
            ? RouteRemoveLayerKind.Overwrite
            : RouteRemoveLayerKind.Base;
    }

    private sealed record DirectoryBoundary(string LogicalPath, string PhysicalPath);

    private sealed record EntryBoundary(
        string LogicalPath,
        string CanonicalPath,
        string RelativePath);

    private sealed class InventoryReadState
    {
        private readonly Queue<DirectoryBoundary> _directories = new();
        private readonly List<RouteRemoveInventoryItem> _items = [];
        private RouteRemoveCategoryFilesystemReadState _state =
            RouteRemoveCategoryFilesystemReadState.Complete;
        private string? _cause;

        internal InventoryReadState(
            RouteRemoveResolvedSubject subject,
            LifecycleOwnershipReadResult ownership,
            string rootLogicalPath,
            string rootPhysicalPath)
        {
            Subject = subject;
            Ownership = ownership;
            RootLogicalPath = rootLogicalPath;
            _directories.Enqueue(new DirectoryBoundary(rootLogicalPath, rootPhysicalPath));
            _items.Add(new RouteRemoveInventoryItem
            {
                Kind = RouteRemoveItemKind.Directory,
                Layer = null,
                SourceId = null,
                SourcePath = rootLogicalPath,
                RelativePath = ".",
                Snapshot = FileStateSnapshot.Directory(rootLogicalPath, rootPhysicalPath),
            });
        }

        internal RouteRemoveResolvedSubject Subject { get; }

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
            _items.Add(new RouteRemoveInventoryItem
            {
                Kind = RouteRemoveItemKind.Directory,
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

        internal void Stop(RouteRemoveCategoryFilesystemReadState state, string cause)
        {
            _state = state;
            _cause = cause;
        }

        internal RouteRemoveCategoryFilesystemReadResult FormResult()
            => _state == RouteRemoveCategoryFilesystemReadState.Complete
                ? new RouteRemoveCategoryFilesystemReadResult
                {
                    State = _state,
                    Inventory = FormInventory(),
                }
                : new RouteRemoveCategoryFilesystemReadResult
                {
                    State = _state,
                    Cause = _cause,
                };

        private RouteRemoveCategoryInventory FormInventory()
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
