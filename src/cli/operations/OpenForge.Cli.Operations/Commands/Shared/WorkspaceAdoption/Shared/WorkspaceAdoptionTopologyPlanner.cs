using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using EntryPointHost = OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.WorkspaceAdoptionTopologyState.EntryPointHost;
using RouteWalk = OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.WorkspaceAdoptionTopologyState.RouteWalk;
using SelectedRoot = OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.WorkspaceAdoptionTopologyState.SelectedRoot;
using SkillDirectory = OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.WorkspaceAdoptionTopologyState.SkillDirectory;
using SourceFact = OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.WorkspaceAdoptionTopologyState.SourceFact;
using SourceOwner = OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.WorkspaceAdoptionTopologyState.SourceOwner;

namespace OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Shared;

internal static class WorkspaceAdoptionTopologyPlanner
{
    private const string AgentsRoot = SourceLogicalPath.AgentsRoot;

    private static readonly HashSet<string> SelectedRootNameKeys = new(
        ["directives", "guidance", "maps", "patterns", "skills", "templates", "memory"],
        StringComparer.Ordinal);

    private static readonly string AgentsRootKey = ReadPortableKey(AgentsRoot);

    internal static WorkspaceAdoptionTopologyPlan Plan(
        IReadOnlyList<SourceLogicalSource> existingSources,
        IReadOnlyList<SourceLogicalSource> payloadSources,
        IReadOnlyCollection<string> removedPaths,
        WorkspaceOwnershipRead ownership)
    {
        ArgumentNullException.ThrowIfNull(existingSources);
        ArgumentNullException.ThrowIfNull(payloadSources);
        ArgumentNullException.ThrowIfNull(removedPaths);
        ArgumentNullException.ThrowIfNull(ownership);

        var removals = RemovalIndex.Create(removedPaths);
        var existingFacts = ReadSourceFacts(existingSources);
        var payloadFacts = ReadSourceFacts(payloadSources);
        var selectedRoots = ReadSelectedRoots(payloadFacts, removals);
        if (selectedRoots.Length == 0)
        {
            return EmptyPlan();
        }

        var selectedExisting = existingFacts
            .Where(fact => FindSelectedRoot(fact.PathKey, selectedRoots) is not null)
            .ToArray();
        var selectedPayload = payloadFacts
            .Where(fact => FindSelectedRoot(fact.PathKey, selectedRoots) is not null)
            .ToArray();
        var activeExisting = selectedExisting
            .Where(fact => !removals.Contains(fact.PathKey))
            .ToArray();
        var activePayload = selectedPayload
            .Where(fact => !removals.Contains(fact.PathKey))
            .ToArray();
        var skillDirectories = ReadSkillDirectories(selectedExisting, selectedPayload);
        var payloadNonEntrypointKeys = activePayload
            .Where(fact => !SourceFormClassifier.IsEntrypoint(fact.Form))
            .Select(fact => fact.PathKey)
            .ToHashSet(StringComparer.Ordinal);
        var potentialEligibleSources = activeExisting
            .Where(fact => IsPotentiallyEligible(
                fact,
                selectedRoots,
                payloadNonEntrypointKeys,
                skillDirectories))
            .OrderBy(fact => fact.Path, StringComparer.Ordinal)
            .ToArray();

        if (!IsOwnershipKnown(ownership.State))
        {
            if (potentialEligibleSources.Length == 0)
            {
                return EmptyPlan();
            }

            return WorkspaceAdoptionTopologyPlan.Blocked(
                UnknownOwnershipCause(ownership, potentialEligibleSources[0].Path));
        }

        var ownershipIndex = OwnershipIndex.Create(ownership.Document);
        var eligibleSources = potentialEligibleSources
            .Where(fact => ownershipIndex.ReadOwner(fact.PathKey) == SourceOwner.None)
            .ToArray();
        var activeExistingEntryPoints = activeExisting
            .Where(fact => SourceFormClassifier.IsEntrypoint(fact.Form)
                && !IsNativeSkillRoot(fact, skillDirectories))
            .ToArray();
        var activePayloadEntryPoints = activePayload
            .Where(fact => SourceFormClassifier.IsEntrypoint(fact.Form)
                && !IsNativeSkillRoot(fact, skillDirectories))
            .ToArray();

        var duplicatePayloadEntryPoints = activePayloadEntryPoints
            .GroupBy(fact => fact.DirectoryKey, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Select(fact => fact.PathKey)
                .Distinct(StringComparer.Ordinal)
                .Skip(1)
                .Any());
        if (duplicatePayloadEntryPoints is not null)
        {
            return WorkspaceAdoptionTopologyPlan.Blocked(AmbiguousEntryPointsCause(
                duplicatePayloadEntryPoints.First().Directory,
                duplicatePayloadEntryPoints.Select(fact => fact.Path)));
        }

        var existingEntryPointsByDirectory = GroupEntryPointsByDirectory(activeExistingEntryPoints);
        var existingByPath = activeExisting
            .GroupBy(fact => fact.PathKey, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var reusedEntrypoints = new Dictionary<string, string>(StringComparer.Ordinal);
        var effectivePayloadEntryPoints = new List<EntryPointHost>(activePayloadEntryPoints.Length);

        foreach (var payloadEntryPoint in activePayloadEntryPoints.OrderBy(
            fact => fact.Path,
            StringComparer.Ordinal))
        {
            var existingCandidates = existingEntryPointsByDirectory.TryGetValue(
                    payloadEntryPoint.DirectoryKey,
                    out var candidates)
                ? candidates
                : Array.Empty<SourceFact>();
            if (existingCandidates.Length > 1)
            {
                return WorkspaceAdoptionTopologyPlan.Blocked(AmbiguousEntryPointsCause(
                    payloadEntryPoint.Directory,
                    existingCandidates.Select(candidate => candidate.Path)));
            }

            if (existingByPath.TryGetValue(payloadEntryPoint.PathKey, out var pathCollision)
                && !SourceFormClassifier.IsEntrypoint(pathCollision.Form))
            {
                var collisionOwner = ownershipIndex.ReadOwner(pathCollision.PathKey);
                if (collisionOwner is SourceOwner.Extension or SourceOwner.Library)
                {
                    return WorkspaceAdoptionTopologyPlan.Blocked(ForeignOwnershipConflictCause(
                        payloadEntryPoint.Path,
                        pathCollision.Path,
                        collisionOwner));
                }
            }

            if (existingCandidates.Length == 0)
            {
                effectivePayloadEntryPoints.Add(new EntryPointHost(
                    payloadEntryPoint.DirectoryKey,
                    payloadEntryPoint.PathKey,
                    payloadEntryPoint.Path));
                continue;
            }

            var existingEntryPoint = existingCandidates[0];
            var owner = ownershipIndex.ReadOwner(existingEntryPoint.PathKey);
            if (owner == SourceOwner.None)
            {
                reusedEntrypoints.Add(payloadEntryPoint.Path, existingEntryPoint.Path);
                effectivePayloadEntryPoints.Add(new EntryPointHost(
                    payloadEntryPoint.DirectoryKey,
                    existingEntryPoint.PathKey,
                    existingEntryPoint.Path));
                continue;
            }

            if (owner == SourceOwner.Framework
                && string.Equals(
                    existingEntryPoint.PathKey,
                    payloadEntryPoint.PathKey,
                    StringComparison.Ordinal))
            {
                effectivePayloadEntryPoints.Add(new EntryPointHost(
                    payloadEntryPoint.DirectoryKey,
                    payloadEntryPoint.PathKey,
                    payloadEntryPoint.Path));
                continue;
            }

            if (owner == SourceOwner.Framework)
            {
                return WorkspaceAdoptionTopologyPlan.Blocked(
                    $"Embedded payload entrypoint '{payloadEntryPoint.Path}' cannot reuse Framework-owned host '{existingEntryPoint.Path}' and would create a duplicate.");
            }

            return WorkspaceAdoptionTopologyPlan.Blocked(ForeignOwnershipConflictCause(
                payloadEntryPoint.Path,
                existingEntryPoint.Path,
                owner));
        }

        var entryPointsByDirectory = new Dictionary<string, Dictionary<string, string>>(
            StringComparer.Ordinal);
        foreach (var entryPoint in activeExistingEntryPoints)
        {
            AddEntryPointHost(
                entryPointsByDirectory,
                entryPoint.DirectoryKey,
                entryPoint.PathKey,
                entryPoint.Path);
        }

        foreach (var entryPoint in effectivePayloadEntryPoints)
        {
            AddEntryPointHost(
                entryPointsByDirectory,
                entryPoint.DirectoryKey,
                entryPoint.PathKey,
                entryPoint.Path);
        }

        var createdEntryPoints = new HashSet<string>(StringComparer.Ordinal);
        foreach (var source in eligibleSources)
        {
            var root = FindSelectedRoot(source.PathKey, selectedRoots)
                ?? throw new InvalidOperationException(
                    "An eligible workspace-adoption source must remain beneath a selected root.");
            var walk = ReadRouteWalk(source, root, skillDirectories);
            var currentDirectory = walk.StartDirectory;
            var currentDirectoryKey = ReadPortableKey(currentDirectory);
            while (!string.Equals(currentDirectoryKey, walk.StopDirectoryKey, StringComparison.Ordinal))
            {
                if (!IsStrictlyWithin(currentDirectoryKey, root.Key))
                {
                    break;
                }

                var currentEntryPoints = entryPointsByDirectory.TryGetValue(
                        currentDirectoryKey,
                        out var foundEntryPoints)
                    ? foundEntryPoints
                    : null;
                if (currentEntryPoints is { Count: > 1 })
                {
                    return WorkspaceAdoptionTopologyPlan.Blocked(AmbiguousEntryPointsCause(
                        currentDirectory,
                        currentEntryPoints.Values));
                }

                if (currentEntryPoints is null || currentEntryPoints.Count == 0)
                {
                    var requiredEntryPointPath = ReadCanonicalEntryPointPath(currentDirectory);
                    if (!PortableWorkspacePath.TryNormalize(
                        requiredEntryPointPath,
                        out var normalizedEntryPointPath))
                    {
                        return WorkspaceAdoptionTopologyPlan.Blocked(
                            $"Required entrypoint '{requiredEntryPointPath}' is not a valid portable workspace path.");
                    }

                    var requiredEntryPointKey = PortableWorkspacePath.CreatePortableKey(normalizedEntryPointPath);
                    if (removals.TryFindCoveringPath(requiredEntryPointKey, out var removedPath))
                    {
                        return WorkspaceAdoptionTopologyPlan.Blocked(
                            $"Required entrypoint '{requiredEntryPointPath}' was removed by '{removedPath}'.");
                    }

                    if (removals.TryFindEntryPointInDirectory(currentDirectoryKey, out removedPath))
                    {
                        return WorkspaceAdoptionTopologyPlan.Blocked(
                            $"Cannot create required entrypoint '{requiredEntryPointPath}' because local entrypoint '{removedPath}' was deliberately removed.");
                    }

                    if (existingByPath.TryGetValue(requiredEntryPointKey, out var existingCollision))
                    {
                        return WorkspaceAdoptionTopologyPlan.Blocked(
                            $"Required entrypoint '{requiredEntryPointPath}' collides with existing source '{existingCollision.Path}'.");
                    }

                    if (payloadNonEntrypointKeys.Contains(requiredEntryPointKey))
                    {
                        return WorkspaceAdoptionTopologyPlan.Blocked(
                            $"Required entrypoint '{requiredEntryPointPath}' collides with a selected payload source.");
                    }

                    var requiredEntryPointOwner = ownershipIndex.ReadOwner(requiredEntryPointKey);
                    if (requiredEntryPointOwner != SourceOwner.None)
                    {
                        return WorkspaceAdoptionTopologyPlan.Blocked(
                            $"Required entrypoint '{requiredEntryPointPath}' is already claimed by {OwnerName(requiredEntryPointOwner)} ownership.");
                    }

                    createdEntryPoints.Add(requiredEntryPointPath);
                    AddEntryPointHost(
                        entryPointsByDirectory,
                        currentDirectoryKey,
                        requiredEntryPointKey,
                        requiredEntryPointPath);
                }

                currentDirectory = SourceLogicalPath.ReadParent(currentDirectory);
                currentDirectoryKey = ReadPortableKey(currentDirectory);
            }
        }

        return WorkspaceAdoptionTopologyPlan.Complete(
            reusedEntrypoints,
            eligibleSources.Select(fact => fact.Path).ToArray(),
            createdEntryPoints.OrderBy(path => path, StringComparer.Ordinal).ToArray());
    }

    private static WorkspaceAdoptionTopologyPlan EmptyPlan()
        => WorkspaceAdoptionTopologyPlan.Complete(
            ImmutableSortedDictionary.Create<string, string>(StringComparer.Ordinal),
            ImmutableArray<string>.Empty,
            ImmutableArray<string>.Empty);

    private static SourceFact[] ReadSourceFacts(IReadOnlyList<SourceLogicalSource> sources)
    {
        var factsByPath = new Dictionary<string, SourceFact>(StringComparer.Ordinal);
        foreach (var source in sources
            .Select(item => item ?? throw new ArgumentException(
                "Workspace-adoption sources cannot contain null members.",
                nameof(sources)))
            .OrderBy(item => item.Identity.CanonicalBasePath, StringComparer.Ordinal))
        {
            var path = source.Identity.CanonicalBasePath;
            if (!PortableWorkspacePath.TryNormalize(path, out var normalizedPath)
                || !SourceFormClassifier.TryClassify(path, out var form))
            {
                continue;
            }

            var directory = SourceLogicalPath.ReadParent(path);
            var fact = new SourceFact(
                path,
                PortableWorkspacePath.CreatePortableKey(normalizedPath),
                directory,
                ReadPortableKey(directory),
                form);
            factsByPath.TryAdd(fact.PathKey, fact);
        }

        return factsByPath.Values
            .OrderBy(fact => fact.Path, StringComparer.Ordinal)
            .ToArray();
    }

    private static SelectedRoot[] ReadSelectedRoots(
        IReadOnlyList<SourceFact> payloadFacts,
        RemovalIndex removals)
    {
        var rootsByKey = new Dictionary<string, SelectedRoot>(StringComparer.Ordinal);
        foreach (var fact in payloadFacts)
        {
            if (!SourceFormClassifier.IsEntrypoint(fact.Form)
                || removals.Contains(fact.PathKey)
                || !string.Equals(
                    ReadPortableKey(SourceLogicalPath.ReadParent(fact.Directory)),
                    AgentsRootKey,
                    StringComparison.Ordinal))
            {
                continue;
            }

            var rootName = SourceLogicalPath.ReadFileName(fact.Directory);
            if (!SelectedRootNameKeys.Contains(PortableWorkspacePath.CreatePortableKey(rootName)))
            {
                continue;
            }

            rootsByKey.TryAdd(
                fact.DirectoryKey,
                new SelectedRoot(fact.Directory, fact.DirectoryKey));
        }

        return rootsByKey.Values
            .OrderBy(root => root.Path, StringComparer.Ordinal)
            .ToArray();
    }

    private static SelectedRoot? FindSelectedRoot(
        string pathKey,
        IReadOnlyList<SelectedRoot> roots)
        => roots.FirstOrDefault(root => IsStrictlyWithin(pathKey, root.Key));

    private static SkillDirectory[] ReadSkillDirectories(
        IReadOnlyList<SourceFact> existingFacts,
        IReadOnlyList<SourceFact> payloadFacts)
        => existingFacts
            .Concat(payloadFacts)
            .Where(fact => fact.Form == SourceDocumentForm.Skill)
            .GroupBy(fact => fact.DirectoryKey, StringComparer.Ordinal)
            .Select(group => group.OrderBy(fact => fact.Directory, StringComparer.Ordinal).First())
            .Select(fact => new SkillDirectory(fact.Directory, fact.DirectoryKey))
            .OrderBy(skill => skill.Path, StringComparer.Ordinal)
            .ToArray();

    private static bool IsPotentiallyEligible(
        SourceFact fact,
        IReadOnlyList<SelectedRoot> roots,
        IReadOnlySet<string> payloadNonEntrypointKeys,
        IReadOnlyList<SkillDirectory> skillDirectories)
    {
        if (FindSelectedRoot(fact.PathKey, roots) is null
            || fact.Form is SourceDocumentForm.Loader or SourceDocumentForm.OverwriteCompanion
            || (payloadNonEntrypointKeys.Contains(fact.PathKey) && fact.Form != SourceDocumentForm.Skill)
            || IsNativeSkillRoot(fact, skillDirectories))
        {
            return false;
        }

        return true;
    }

    private static bool IsNativeSkillRoot(
        SourceFact fact,
        IReadOnlyList<SkillDirectory> skillDirectories)
        => fact.Form != SourceDocumentForm.Skill
            && skillDirectories.Any(skill => string.Equals(
                skill.Key,
                fact.DirectoryKey,
                StringComparison.Ordinal));

    private static Dictionary<string, SourceFact[]> GroupEntryPointsByDirectory(
        IEnumerable<SourceFact> entryPoints)
        => entryPoints
            .GroupBy(fact => fact.DirectoryKey, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(fact => fact.Path, StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);

    private static void AddEntryPointHost(
        IDictionary<string, Dictionary<string, string>> entryPointsByDirectory,
        string directoryKey,
        string pathKey,
        string path)
    {
        if (!entryPointsByDirectory.TryGetValue(directoryKey, out var entryPoints))
        {
            entryPoints = new Dictionary<string, string>(StringComparer.Ordinal);
            entryPointsByDirectory.Add(directoryKey, entryPoints);
        }

        if (!entryPoints.TryGetValue(pathKey, out var existingPath)
            || string.Compare(path, existingPath, StringComparison.Ordinal) < 0)
        {
            entryPoints[pathKey] = path;
        }
    }

    private static RouteWalk ReadRouteWalk(
        SourceFact source,
        SelectedRoot root,
        IReadOnlyList<SkillDirectory> skillDirectories)
    {
        if (source.Form == SourceDocumentForm.Skill)
        {
            var enclosingSkill = skillDirectories
                .Where(skill => !string.Equals(skill.Key, source.DirectoryKey, StringComparison.Ordinal)
                    && IsStrictlyWithin(source.DirectoryKey, skill.Key))
                .OrderByDescending(skill => skill.Key.Length)
                .ThenBy(skill => skill.Key, StringComparer.Ordinal)
                .FirstOrDefault();
            var startDirectory = SourceLogicalPath.ReadParent(source.Directory);
            var stopDirectoryKey = enclosingSkill?.Key ?? root.Key;
            return new RouteWalk(startDirectory, stopDirectoryKey);
        }

        var nearestSkill = skillDirectories
            .Where(skill => IsStrictlyWithin(source.PathKey, skill.Key))
            .OrderByDescending(skill => skill.Key.Length)
            .ThenBy(skill => skill.Key, StringComparer.Ordinal)
            .FirstOrDefault();
        return new RouteWalk(
            source.Directory,
            nearestSkill?.Key ?? root.Key);
    }

    private static string ReadCanonicalEntryPointPath(string directory)
    {
        var folderName = SourceLogicalPath.ReadFileName(directory);
        return SourceLogicalPath.Combine(directory, $"_{folderName}.md");
    }

    private static string AmbiguousEntryPointsCause(string directory, IEnumerable<string> paths)
        => $"Ambiguous recognized entrypoints in '{directory}': {string.Join(", ", paths.OrderBy(path => path, StringComparer.Ordinal))}.";

    private static string ForeignOwnershipConflictCause(
        string payloadPath,
        string existingPath,
        SourceOwner owner)
        => $"Payload entrypoint '{payloadPath}' conflicts with {OwnerName(owner)}-owned host '{existingPath}'.";

    private static string OwnerName(SourceOwner owner)
        => owner switch
        {
            SourceOwner.Framework => "Framework",
            SourceOwner.Extension => "Extension",
            SourceOwner.Library => "Library",
            SourceOwner.None => "user",
            _ => throw new ArgumentOutOfRangeException(nameof(owner), owner, "The source owner is not defined."),
        };

    private static string UnknownOwnershipCause(WorkspaceOwnershipRead ownership, string sourcePath)
    {
        var state = ownership.State switch
        {
            WorkspaceOwnershipReadState.Invalid => "invalid",
            WorkspaceOwnershipReadState.Unavailable => "unavailable",
            WorkspaceOwnershipReadState.Absent or WorkspaceOwnershipReadState.Complete
                => throw new ArgumentException(
                    "Known ownership cannot produce an unknown-ownership cause.",
                    nameof(ownership)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(ownership),
                ownership.State,
                "The ownership-read state is not defined."),
        };
        var detail = string.IsNullOrWhiteSpace(ownership.Cause)
            ? string.Empty
            : $" Cause: {ownership.Cause}";
        return $"Workspace ownership is {state}; cannot safely adopt existing source '{sourcePath}' because its owner cannot be established.{detail}";
    }

    private static bool IsOwnershipKnown(WorkspaceOwnershipReadState state)
        => state switch
        {
            WorkspaceOwnershipReadState.Absent or WorkspaceOwnershipReadState.Complete => true,
            WorkspaceOwnershipReadState.Invalid or WorkspaceOwnershipReadState.Unavailable => false,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The ownership-read state is not defined."),
        };

    private static string ReadPortableKey(string path)
    {
        if (!PortableWorkspacePath.TryNormalize(path, out var normalizedPath))
        {
            throw new ArgumentException(
                "A workspace-adoption path must be a canonical portable workspace path.",
                nameof(path));
        }

        return PortableWorkspacePath.CreatePortableKey(normalizedPath);
    }

    private static bool IsStrictlyWithin(string pathKey, string parentKey)
        => pathKey.StartsWith($"{parentKey}/", StringComparison.Ordinal);

    private sealed class OwnershipIndex
    {
        private readonly HashSet<string> _frameworkPathKeys;
        private readonly HashSet<string> _extensionHostPathKeys;
        private readonly HashSet<string> _libraryPathKeys;
        private readonly string[] _libraryRootKeys;

        private OwnershipIndex(WorkspaceOwnershipDocument document)
        {
            // Framework region receipts, including generated Entries, do not own authored host bytes.
            _frameworkPathKeys = document.Framework is { } framework
                ? ReadPathKeys(framework.Paths)
                : new HashSet<string>(StringComparer.Ordinal);
            _extensionHostPathKeys = ReadPathKeys(document.Extensions
                .SelectMany(extension => extension.Paths.Concat(extension.Regions.Select(region => region.Path))));
            _libraryPathKeys = ReadPathKeys(document.Libraries.SelectMany(library => library.Paths));
            _libraryRootKeys = document.Libraries
                .Select(library => library.DestinationRoot)
                .Select(TryReadPathKey)
                .Where(key => key is not null)
                .Select(key => key!)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(key => key, StringComparer.Ordinal)
                .ToArray();
        }

        internal static OwnershipIndex Create(WorkspaceOwnershipDocument document)
            => new(document);

        internal SourceOwner ReadOwner(string pathKey)
        {
            if (_extensionHostPathKeys.Contains(pathKey))
            {
                return SourceOwner.Extension;
            }

            if (_libraryPathKeys.Contains(pathKey)
                || _libraryRootKeys.Any(rootKey =>
                    string.Equals(pathKey, rootKey, StringComparison.Ordinal)
                    || IsStrictlyWithin(pathKey, rootKey)))
            {
                return SourceOwner.Library;
            }

            if (_frameworkPathKeys.Contains(pathKey))
            {
                return SourceOwner.Framework;
            }

            return SourceOwner.None;
        }

        private static HashSet<string> ReadPathKeys(IEnumerable<string> paths)
            => paths
                .Select(TryReadPathKey)
                .Where(key => key is not null)
                .Select(key => key!)
                .ToHashSet(StringComparer.Ordinal);

        private static string? TryReadPathKey(string path)
            => PortableWorkspacePath.TryNormalize(path, out var normalizedPath)
                ? PortableWorkspacePath.CreatePortableKey(normalizedPath)
                : null;
    }

    private sealed class RemovalIndex
    {
        private readonly Dictionary<string, string[]> _entryPointPathsByDirectory;
        private readonly KeyValuePair<string, string>[] _pathsByKey;

        private RemovalIndex(
            Dictionary<string, string[]> entryPointPathsByDirectory,
            KeyValuePair<string, string>[] pathsByKey)
        {
            _entryPointPathsByDirectory = entryPointPathsByDirectory;
            _pathsByKey = pathsByKey;
        }

        internal static RemovalIndex Create(IReadOnlyCollection<string> removedPaths)
        {
            var pathsByKey = new Dictionary<string, string>(StringComparer.Ordinal);
            var entryPointsByDirectory = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            foreach (var path in removedPaths.OrderBy(path => path, StringComparer.Ordinal))
            {
                if (!PortableWorkspacePath.TryNormalize(path, out var normalizedPath))
                {
                    continue;
                }

                var pathKey = PortableWorkspacePath.CreatePortableKey(normalizedPath);
                pathsByKey.TryAdd(pathKey, path);
                if (!SourceFormClassifier.TryClassify(path, out var form)
                    || !SourceFormClassifier.IsEntrypoint(form))
                {
                    continue;
                }

                var directoryKey = ReadPortableKey(SourceLogicalPath.ReadParent(path));
                if (!entryPointsByDirectory.TryGetValue(directoryKey, out var removedEntryPoints))
                {
                    removedEntryPoints = [];
                    entryPointsByDirectory.Add(directoryKey, removedEntryPoints);
                }

                removedEntryPoints.Add(path);
            }

            return new RemovalIndex(
                entryPointsByDirectory.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Distinct(StringComparer.Ordinal)
                        .OrderBy(path => path, StringComparer.Ordinal)
                        .ToArray(),
                    StringComparer.Ordinal),
                pathsByKey
                    .OrderByDescending(pair => pair.Key.Length)
                    .ThenBy(pair => pair.Key, StringComparer.Ordinal)
                    .ToArray());
        }

        internal bool Contains(string pathKey)
            => TryFindCoveringPath(pathKey, out _);

        internal bool TryFindCoveringPath(string pathKey, out string removedPath)
        {
            foreach (var (removedPathKey, observedPath) in _pathsByKey)
            {
                if (string.Equals(pathKey, removedPathKey, StringComparison.Ordinal))
                {
                    removedPath = observedPath;
                    return true;
                }
            }

            removedPath = string.Empty;
            return false;
        }

        internal bool TryFindEntryPointInDirectory(string directoryKey, out string removedPath)
        {
            if (_entryPointPathsByDirectory.TryGetValue(directoryKey, out var paths)
                && paths.Length > 0)
            {
                removedPath = paths[0];
                return true;
            }

            removedPath = string.Empty;
            return false;
        }
    }
}
