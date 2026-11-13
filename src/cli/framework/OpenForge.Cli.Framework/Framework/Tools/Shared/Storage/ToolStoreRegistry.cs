using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Storage;

internal sealed class ToolStoreRegistry
{
    internal ToolStoreArea Installs { get; }
    internal ToolStoreArea Archives { get; }
    internal ToolStoreArea State { get; }
    internal ToolStorePaths Paths => new(Installs.Root, Archives.Root, State.Root);

    internal ToolStoreRegistry(ToolStorePaths paths)
    {
        Installs = new(paths.Installs);
        Archives = new(paths.Archives);
        State = new(paths.State);
    }

    internal async ValueTask<ToolStoreRead> ReadAsync(CancellationToken token)
    {
        State.ValidateRoot();
        if (!Directory.Exists(State.Root))
        {
            if (Directory.Exists(Installs.Root) || Directory.Exists(Archives.Root))
                throw new IOException("Existing tool bytes have no readable reference registry.");
            return new(Empty(), FileStateSnapshot.Missing(State.At(ToolStoreRegistryDefinitions.File)));
        }
        var snapshot = await ToolFileReader.ReadAsync(State.Scope, State.At(ToolStoreRegistryDefinitions.File), token).ConfigureAwait(false);
        if (!snapshot.Expectation.Exists)
        {
            if (Directory.Exists(State.Root) || Directory.Exists(Installs.Root) || Directory.Exists(Archives.Root))
                throw new IOException("The tool stores exist without their complete reference registry.");
            return new(Empty(), snapshot);
        }
        JsonDuplicatePropertyValidator.ValidateNoDuplicateProperties(snapshot.Bytes.AsSpan());
        var document = JsonSerializer.Deserialize(snapshot.Bytes.AsSpan(), ToolStoreJsonContext.Default.ToolStoreRegistryDocument)
            ?? throw new JsonException("The tool registry cannot be null.");
        Validate(document);
        return new(document, snapshot);
    }

    internal async ValueTask InitializeAsync(CancellationToken token)
    {
        if (Directory.Exists(State.Root)) { await ReadAsync(token).ConfigureAwait(false); return; }
        if (Directory.Exists(Installs.Root) || Directory.Exists(Archives.Root))
            throw new IOException("Existing tool bytes have no readable reference registry.");
        var parent = new ToolStoreArea(Path.GetDirectoryName(State.Root) ?? throw new IOException("The tool state has no parent."));
        parent.EnsureRoot(token);
        var name = ToolStoreLocator.StateDirectory + "." + Guid.NewGuid().ToString("N");
        parent.EnsureDirectory(name, token);
        var temporary = parent.At(name);
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(Empty(), ToolStoreJsonContext.Default.ToolStoreRegistryDocument);
            await File.WriteAllBytesAsync(Path.Combine(temporary, ToolStoreRegistryDefinitions.File), bytes, token).ConfigureAwait(false);
            State.ValidateRoot();
            token.ThrowIfCancellationRequested();
            try { Directory.Move(temporary, State.Root); }
            catch (IOException) when (Directory.Exists(State.Root)) { await ReadAsync(token).ConfigureAwait(false); }
        }
        finally
        {
            if (Directory.Exists(temporary))
            {
                File.Delete(Path.Combine(temporary, ToolStoreRegistryDefinitions.File));
                Directory.Delete(temporary, recursive: false);
            }
        }
    }

    internal async ValueTask<ToolStoreRead> WriteAsync(ToolStoreLease lease, ToolStoreRead prior, ToolStoreRegistryDocument document, CancellationToken token)
    {
        lease.RequireHeldFor(State);
        Validate(document);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(document, ToolStoreJsonContext.Default.ToolStoreRegistryDocument);
        await ToolFileReplacement.WriteAsync(State.Scope, new(prior.Snapshot.Expectation, bytes), lease.RequireHeld, token).ConfigureAwait(false);
        var result = await ReadAsync(token).ConfigureAwait(false);
        if (!result.Snapshot.Bytes.AsSpan().SequenceEqual(bytes)) throw new IOException("The tool reference registry didn't verify after publication.");
        return result;
    }

    internal ToolStoreArea Area(ToolStoredUnit unit) => Area(unit.Kind);
    internal ToolStoreArea Area(string kind) => ToolStoreIdentity.BindingKind(kind) == ToolStoreRegistryDefinitions.Archive ? Archives : Installs;

    private ToolStoreRegistryDocument Empty() => new() { Roots = Roots(), Units = new(StringComparer.Ordinal) };

    private Dictionary<string, string> Roots() => new(StringComparer.Ordinal)
    {
        [ToolStoreRegistryDefinitions.Install] = ToolStoreIdentity.Binding(ToolStoreRegistryDefinitions.Install, Installs.Root).IdentitySha256,
        [ToolStoreRegistryDefinitions.Archive] = ToolStoreIdentity.Binding(ToolStoreRegistryDefinitions.Archive, Archives.Root).IdentitySha256,
        [ToolStoreRegistryDefinitions.State] = ToolStoreIdentity.Binding(ToolStoreRegistryDefinitions.State, State.Root).IdentitySha256,
    };

    private void Validate(ToolStoreRegistryDocument document)
    {
        Require(document.SchemaVersion == ToolStoreRegistryDefinitions.SchemaVersion && document.Roots is not null && document.Units is not null,
            "The tool registry shape or version isn't supported.");
        var roots = Roots();
        Require(document.Roots.Count == roots.Count && roots.All(pair => document.Roots.GetValueOrDefault(pair.Key) == pair.Value), "The tool store root bindings changed.");
        var bindings = new HashSet<string>(StringComparer.Ordinal);
        var stages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, unit) in document.Units)
        {
            Require(unit is not null && ToolIdentity.IsSha256(key) && ToolIdentity.IsRelativePath(unit.Path)
                && unit.Kind is ToolStoreRegistryDefinitions.Install or ToolStoreRegistryDefinitions.Archive or ToolStoreRegistryDefinitions.Runtime,
                "A tool store unit has an invalid identity.");
            Require(key == ToolStoreIdentity.UnitKey(unit.Kind, unit.Path), "A tool store unit key doesn't match its identity.");
            Require(!unit.Path.StartsWith(ToolStoreRegistryDefinitions.StagingDirectory + "/", StringComparison.Ordinal), "A registered unit cannot be a staging directory.");
            if (unit.StagingPath is { } stage)
                Require(ToolIdentity.IsRelativePath(stage) && stage.StartsWith(ToolStoreRegistryDefinitions.StagingDirectory + "/", StringComparison.Ordinal)
                    && Guid.TryParseExact(stage[(ToolStoreRegistryDefinitions.StagingDirectory.Length + 1)..], "N", out _)
                    && stages.Add(ToolStoreIdentity.BindingKind(unit.Kind) + ":" + stage), "A staging directory identity is invalid or shared.");
            var kind = ToolStoreIdentity.BindingKind(unit.Kind);
            var binding = ToolStoreIdentity.Binding(kind, Area(unit).At(unit.Path));
            Require(unit.Binding == binding && bindings.Add(binding.Id) && ToolIdentity.IsSha256(unit.RecipeSha256), "A tool store unit has an invalid physical binding.");
            Require(unit.Files is not null && unit.Directories is not null && unit.Workspaces is not null && unit.Dependencies is not null && unit.Dependents is not null,
                "A tool store unit has unavailable references or member coverage.");
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in unit.Files)
                Require(file is not null && file.StoreId == binding.Id && ToolIdentity.IsRelativePath(file.Path) && files.Add(file.Path)
                    && ToolIdentity.IsSha256(file.Sha256), "A tool store file receipt is invalid.");
            Require(unit.Directories.All(ToolIdentity.IsRelativePath) && unit.Directories.Distinct(StringComparer.OrdinalIgnoreCase).Count() == unit.Directories.Length,
                "A tool store directory receipt is invalid.");
            foreach (var (workspace, reference) in unit.Workspaces)
                Require(reference is not null && Path.IsPathFullyQualified(reference.PhysicalWorkspace) && workspace == WorkspaceIdentity.Key(reference.PhysicalWorkspace)
                    && (reference.CodeRoot == "." || ToolIdentity.IsRelativePath(reference.CodeRoot))
                    && (reference.ProcessConsentSha256 is null || ToolIdentity.IsSha256(reference.ProcessConsentSha256)), "A tool workspace reference is invalid.");
            Require(unit.Dependencies.Distinct(StringComparer.Ordinal).Count() == unit.Dependencies.Length
                && unit.Dependents.Distinct(StringComparer.Ordinal).Count() == unit.Dependents.Length, "Tool store references contain duplicate identities.");
            foreach (var dependency in unit.Dependencies)
                Require(document.Units.TryGetValue(dependency, out var target) && target is not null && target.Dependents is not null && target.Dependents.Contains(key, StringComparer.Ordinal), "A dependency reference is unavailable.");
            foreach (var dependent in unit.Dependents)
                Require(document.Units.TryGetValue(dependent, out var target) && target is not null && target.Dependencies is not null && target.Dependencies.Contains(key, StringComparer.Ordinal), "A dependent reference is unavailable.");
        }
    }

    private static void Require([DoesNotReturnIf(false)] bool condition, string cause)
    {
        if (!condition) throw new JsonException(cause);
    }
}
