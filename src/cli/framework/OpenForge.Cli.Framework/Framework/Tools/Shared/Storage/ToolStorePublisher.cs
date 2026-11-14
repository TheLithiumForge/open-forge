using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Storage;

// The external-tools design requires pending publication and complete reference checks.
internal sealed class ToolStorePublisher(ToolStoreRegistry registry, Action<ToolStorePublicationEvent>? progress = null)
{
    internal async ValueTask<ToolStorePublication> PublishAsync(ToolStoreLease lease, ToolPreparedStoreUnit prepared,
        ToolLocalRegistration? registration, CancellationToken token)
    {
        lease.RequireHeldFor(registry.State);
        var before = await registry.ReadAsync(token).ConfigureAwait(false);
        var area = registry.Area(prepared.Kind);
        var key = ToolStoreIdentity.UnitKey(prepared.Kind, prepared.Path);
        var bindingKind = ToolStoreIdentity.BindingKind(prepared.Kind);
        var binding = ToolStoreIdentity.Binding(bindingKind, area.At(prepared.Path));
        before.Document.Units.TryGetValue(key, out var prior);
        if (prior is not null && prior.RecipeSha256 != prepared.RecipeSha256) throw new IOException("The stored recipe differs from the prepared recipe.");
        if (prior is null && ToolFileReader.Observe(area.Scope, area.At(prepared.Path), token) != NoFollowLeafState.Missing)
            throw new IOException("An unregistered path already occupies the install destination.");
        var unit = new ToolStoredUnit
        {
            Kind = prepared.Kind,
            Path = prepared.Path,
            RecipeSha256 = prepared.RecipeSha256,
            Binding = binding,
            Verified = false,
            Files = [.. prepared.Files.Select(file => file with { StoreId = binding.Id })],
            Directories = prepared.Directories,
            Dependencies = prepared.Dependencies,
            Dependents = prior?.Dependents ?? [],
            Workspaces = prior is null ? new(StringComparer.Ordinal) : new(prior.Workspaces, StringComparer.Ordinal),
            StagingPath = prepared.StagingPath,
            PreviouslyVerified = prior?.Verified == true || prior?.PreviouslyVerified == true,
        };
        ValidatePrepared(unit);
        if (registration is not null) unit.Workspaces[WorkspaceIdentity.Key(registration.PhysicalWorkspace)] = registration;
        var stage = unit with { Path = prepared.StagingPath };
        if (!(await ToolStoreUnitReader.ReadAsync(area, stage, token).ConfigureAwait(false)).Matching)
            throw new IOException("The prepared store members don't match their pinned hashes.");
        RequireCoverage(area, stage.Path, stage, token);
        if (prior is not null)
        {
            RequireCoverage(area, prior.Path, prior, token);
            var observed = await ToolStoreUnitReader.ReadAsync(area, prior, token).ConfigureAwait(false);
            if (observed.Members.Any(member => member.State == ToolStoreMemberState.Unavailable)) throw new IOException("A store destination is unsafe or unavailable.");
            if (!unit.PreviouslyVerified && observed.Members.Any(member => member.State == ToolStoreMemberState.Changed))
                throw new IOException("Unrecognized changed bytes occupy a pending publication destination.");
            if (!prior.Files.OrderBy(file => file.Path, StringComparer.Ordinal).SequenceEqual(unit.Files.OrderBy(file => file.Path, StringComparer.Ordinal))
                || !prior.Directories.Order(StringComparer.Ordinal).SequenceEqual(unit.Directories.Order(StringComparer.Ordinal)))
                throw new IOException("The prepared file coverage differs from the registered recipe.");
        }
        var units = new Dictionary<string, ToolStoredUnit>(before.Document.Units, StringComparer.Ordinal);
        foreach (var dependency in unit.Dependencies)
        {
            if (dependency == key || !units.TryGetValue(dependency, out var target) || !target.Verified)
                throw new IOException("A verified store dependency is unavailable.");
            units[dependency] = target with { Dependents = [.. target.Dependents.Append(key).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)] };
        }
        await RequireVerifiedClosureAsync(before.Document, unit.Dependencies, token).ConfigureAwait(false);
        if (prior is not null && !prior.Dependencies.Order(StringComparer.Ordinal).SequenceEqual(unit.Dependencies.Order(StringComparer.Ordinal)))
            throw new IOException("The prepared dependency closure differs from the registered unit.");
        var expectations = new Dictionary<string, FileExpectation>(StringComparer.Ordinal);
        foreach (var file in unit.Files)
        {
            var path = area.At(unit.Path + "/" + file.Path);
            var leaf = ToolFileReader.Observe(area.Scope, path, token);
            if (leaf == NoFollowLeafState.Missing) expectations.Add(file.Path, FileExpectation.Missing(path));
            else if (leaf == NoFollowLeafState.OrdinaryFile)
                expectations.Add(file.Path, FileExpectation.File(path, path, await ToolArtifactIntegrity.FileHashAsync(path, token).ConfigureAwait(false)));
            else throw new IOException("The publication destination is unsafe or unavailable.");
        }
        if (prior?.StagingPath is { } oldStage && oldStage != prepared.StagingPath)
            await DeleteMembersAsync(area, prior with { Path = oldStage }, token).ConfigureAwait(false);
        units[key] = unit;
        var pending = await registry.WriteAsync(lease, before, before.Document with { Units = units }, token).ConfigureAwait(false);
        progress?.Invoke(new(ToolStorePublicationEventKind.PendingRecorded, unit.Path));
        token.ThrowIfCancellationRequested();
        area.EnsureDirectory(unit.Path, token);
        foreach (var directory in unit.Directories.OrderBy(Depth).ThenBy(path => path, StringComparer.Ordinal))
            area.EnsureDirectory(unit.Path + "/" + directory, token);
        foreach (var file in unit.Files)
        {
            lease.RequireHeldFor(registry.State);
            var source = area.At(stage.Path + "/" + file.Path);
            var destination = area.At(unit.Path + "/" + file.Path);
            if (ToolFileReader.Observe(area.Scope, source, token) != NoFollowLeafState.OrdinaryFile
                || await ToolArtifactIntegrity.FileHashAsync(source, token).ConfigureAwait(false) != file.Sha256)
                throw new IOException("A prepared file changed before publication.");
            var state = ToolFileReader.Observe(area.Scope, destination, token);
            if (state is not NoFollowLeafState.Missing and not NoFollowLeafState.OrdinaryFile)
                throw new IOException("A store destination isn't an ordinary file.");
            token.ThrowIfCancellationRequested();
            var expected = await new FileExpectationValidator(new PhysicalPathResolver()).ValidateAsync(area.Scope, expectations[file.Path], token).ConfigureAwait(false);
            if (expected.State == FileExpectationValidationState.Cancelled) throw new OperationCanceledException(token);
            if (expected.State != FileExpectationValidationState.Matched) throw new IOException("A store destination changed after publication was planned.");
            File.Move(source, destination, overwrite: state == NoFollowLeafState.OrdinaryFile);
            progress?.Invoke(new(ToolStorePublicationEventKind.FileMoved, unit.Path + "/" + file.Path));
            token.ThrowIfCancellationRequested();
        }
        if (!(await ToolStoreUnitReader.ReadAsync(area, unit, token).ConfigureAwait(false)).Matching)
            throw new IOException("The published store members didn't verify.");
        progress?.Invoke(new(ToolStorePublicationEventKind.Verified, unit.Path));
        string? cleanup = null;
        try { await DeleteMembersAsync(area, stage, token).ConfigureAwait(false); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException) { cleanup = exception.Message; }
        unit = unit with { Verified = true, PreviouslyVerified = true, StagingPath = cleanup is null ? null : stage.Path };
        units = new(pending.Document.Units, StringComparer.Ordinal) { [key] = unit };
        await registry.WriteAsync(lease, pending, pending.Document with { Units = units }, token).ConfigureAwait(false);
        return new(unit, cleanup);
    }

    internal async ValueTask<ToolStoredUnit> AttachAsync(ToolStoreLease lease, string key, ToolLocalRegistration registration, CancellationToken token)
    {
        lease.RequireHeldFor(registry.State);
        var before = await registry.ReadAsync(token).ConfigureAwait(false);
        await RequireVerifiedClosureAsync(before.Document, [key], token).ConfigureAwait(false);
        var unit = before.Document.Units[key];
        var workspaces = new Dictionary<string, ToolLocalRegistration>(unit.Workspaces, StringComparer.Ordinal)
        { [WorkspaceIdentity.Key(registration.PhysicalWorkspace)] = registration };
        unit = unit with { Workspaces = workspaces };
        var units = new Dictionary<string, ToolStoredUnit>(before.Document.Units, StringComparer.Ordinal) { [key] = unit };
        await registry.WriteAsync(lease, before, before.Document with { Units = units }, token).ConfigureAwait(false);
        return unit;
    }

    private async ValueTask RequireVerifiedClosureAsync(ToolStoreRegistryDocument document, IEnumerable<string> roots, CancellationToken token)
    {
        var pending = new Queue<string>(roots);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        while (pending.TryDequeue(out var key))
        {
            if (!visited.Add(key)) continue;
            if (!document.Units.TryGetValue(key, out var unit)
                || !(await ToolStoreUnitReader.ReadAsync(registry.Area(unit), unit, token).ConfigureAwait(false)).Verified)
                throw new IOException("The verified store dependency closure is unavailable or changed.");
            foreach (var dependency in unit.Dependencies) pending.Enqueue(dependency);
        }
    }

    internal async ValueTask<bool> ReleaseAsync(ToolStoreLease lease, string key, string? physicalWorkspace, CancellationToken token)
    {
        lease.RequireHeldFor(registry.State);
        var before = await registry.ReadAsync(token).ConfigureAwait(false);
        if (!before.Document.Units.TryGetValue(key, out var unit)) throw new IOException("The selected unit has no complete store record.");
        var workspaces = new Dictionary<string, ToolLocalRegistration>(unit.Workspaces, StringComparer.Ordinal);
        if (physicalWorkspace is not null && !workspaces.Remove(WorkspaceIdentity.Key(physicalWorkspace)))
            throw new IOException("The selected workspace has no recorded reference to this unit.");
        var units = new Dictionary<string, ToolStoredUnit>(before.Document.Units, StringComparer.Ordinal);
        if (workspaces.Count != 0 || unit.Dependents.Length != 0)
        {
            units[key] = unit with { Workspaces = workspaces };
            await registry.WriteAsync(lease, before, before.Document with { Units = units }, token).ConfigureAwait(false);
            return false;
        }
        var area = registry.Area(unit);
        RequireCoverage(area, unit.Path, unit, token);
        await RequireDeletableAsync(area, unit, token).ConfigureAwait(false);
        if (unit.StagingPath is { } stage)
        {
            RequireCoverage(area, stage, unit, token);
            await RequireDeletableAsync(area, unit with { Path = stage }, token).ConfigureAwait(false);
        }
        units[key] = unit with { Verified = false, PreviouslyVerified = unit.Verified || unit.PreviouslyVerified };
        var pending = await registry.WriteAsync(lease, before, before.Document with { Units = units }, token).ConfigureAwait(false);
        await DeleteMembersAsync(area, unit, token).ConfigureAwait(false);
        if (unit.StagingPath is { } savedStage) await DeleteMembersAsync(area, unit with { Path = savedStage }, token).ConfigureAwait(false);
        units = new(pending.Document.Units, StringComparer.Ordinal);
        units.Remove(key);
        foreach (var dependency in unit.Dependencies)
        {
            var target = units[dependency];
            units[dependency] = target with { Dependents = [.. target.Dependents.Where(value => value != key)] };
        }
        await registry.WriteAsync(lease, pending, pending.Document with { Units = units }, token).ConfigureAwait(false);
        return true;
    }

    private static void ValidatePrepared(ToolStoredUnit unit)
    {
        if (!ToolIdentity.IsSha256(unit.RecipeSha256) || !ToolIdentity.IsRelativePath(unit.Path)
            || unit.Files.Length == 0 || unit.Dependencies.Distinct(StringComparer.Ordinal).Count() != unit.Dependencies.Length
            || unit.StagingPath is not { } stage || !stage.StartsWith(ToolStoreRegistryDefinitions.StagingDirectory + "/", StringComparison.Ordinal)
            || !Guid.TryParseExact(stage[(ToolStoreRegistryDefinitions.StagingDirectory.Length + 1)..], "N", out _))
            throw new ArgumentException("The prepared store unit is invalid.", nameof(unit));
        var members = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in unit.Files)
            if (!ToolIdentity.IsRelativePath(file.Path) || !ToolIdentity.IsSha256(file.Sha256) || !members.Add(file.Path))
                throw new ArgumentException("The prepared files are invalid.", nameof(unit));
        foreach (var directory in unit.Directories)
            if (!ToolIdentity.IsRelativePath(directory) || !members.Add(directory)) throw new ArgumentException("The prepared directories are invalid.", nameof(unit));
        var directories = unit.Directories.ToHashSet(StringComparer.Ordinal);
        foreach (var member in members)
            for (var separator = member.LastIndexOf('/'); separator > 0; separator = member.LastIndexOf('/', separator - 1))
                if (!directories.Contains(member[..separator])) throw new ArgumentException("The prepared directory coverage is incomplete.", nameof(unit));
    }

    private static void RequireCoverage(ToolStoreArea area, string path, ToolStoredUnit unit, CancellationToken token)
    {
        var known = unit.Files.Select(file => file.Path).Concat(unit.Directories).ToHashSet(StringComparer.Ordinal);
        if (ToolStoreUnitReader.Members(area, path, token).Any(member => !known.Contains(member)))
            throw new IOException("An unrecognized member prevents changing this store unit.");
    }

    private static async ValueTask RequireDeletableAsync(ToolStoreArea area, ToolStoredUnit unit, CancellationToken token)
    {
        var observation = await ToolStoreUnitReader.ReadAsync(area, unit, token).ConfigureAwait(false);
        if (observation.Members.Any(member => member.State is ToolStoreMemberState.Changed or ToolStoreMemberState.Unavailable))
            throw new IOException("A changed or unavailable store member prevents deletion.");
    }

    private static async ValueTask DeleteMembersAsync(ToolStoreArea area, ToolStoredUnit unit, CancellationToken token)
    {
        RequireCoverage(area, unit.Path, unit, token);
        await RequireDeletableAsync(area, unit, token).ConfigureAwait(false);
        foreach (var file in unit.Files)
        {
            var path = area.At(unit.Path + "/" + file.Path);
            if (ToolFileReader.Observe(area.Scope, path, token) == NoFollowLeafState.Missing) continue;
            if (await ToolArtifactIntegrity.FileHashAsync(path, token).ConfigureAwait(false) != file.Sha256) throw new IOException("A store file changed before deletion.");
            token.ThrowIfCancellationRequested();
            File.Delete(path);
        }
        foreach (var directory in unit.Directories.OrderByDescending(Depth).ThenByDescending(path => path, StringComparer.Ordinal))
        {
            var path = area.At(unit.Path + "/" + directory);
            var state = ToolFileReader.Observe(area.Scope, path, token);
            if (state == NoFollowLeafState.Missing) continue;
            if (state != NoFollowLeafState.Directory) throw new IOException("A store directory changed before deletion.");
            Directory.Delete(path, recursive: false);
        }
        var root = area.At(unit.Path);
        var leaf = ToolFileReader.Observe(area.Scope, root, token);
        if (leaf == NoFollowLeafState.Directory) Directory.Delete(root, recursive: false);
        else if (leaf != NoFollowLeafState.Missing) throw new IOException("The store unit root changed before deletion.");
    }

    private static int Depth(string path) => path.Count(character => character == '/');
}
