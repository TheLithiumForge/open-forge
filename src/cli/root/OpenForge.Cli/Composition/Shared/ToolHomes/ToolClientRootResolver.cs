using System.Collections.Immutable;
using System.Text.Json;
using OpenForge.Cli.Composition.Shared.ToolHomes.Models;
using OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodebaseMemory;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;

namespace OpenForge.Cli.Composition.Shared.ToolHomes;

// Resolves reviewed possible destinations. It never detects clients or trusts native inventory as input.
internal sealed partial class ToolClientRootResolver(ToolHomeEnvironment environment, string home,
    ToolOperatingSystem operatingSystem, IReadOnlyDictionary<string, string> injected, Func<string> runtimeParent)
{
    private static readonly ImmutableArray<ToolClientRootRecipe> AccountRoots =
    [
        new(CodebaseMemoryDefinitions.CacheRoot, true, static roots => roots.RawOrHome(ToolHomeEnvironment.CacheDirectory, ".cache/codebase-memory-mcp")),
        // Report-only parent of CBM's identity-named rendezvous directory. Never inspect its contents or derive its identity.
        new(CodebaseMemoryDefinitions.RuntimeRoot, true, static roots => roots.ReadRuntimeParent()),
    ];
    private readonly Dictionary<string, string> _directories = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _files = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _unavailable = new(StringComparer.Ordinal);

    internal ToolClientRoots Resolve()
    {
        foreach (var recipe in BuiltInDirectoryRecipes.Concat(BuiltInFileRecipes).Concat(RegistryRecipes).Concat(AccountRoots)) Bind(recipe);
        foreach (var pair in injected)
        {
            if (_directories.ContainsKey(pair.Key) || _files.ContainsKey(pair.Key) || _unavailable.ContainsKey(pair.Key)) continue;
            BindPath(pair.Key, directory: !IsFileBinding(pair.Key), pair.Value);
        }
        return new(directories: _directories.ToImmutableDictionary(StringComparer.Ordinal), files: _files.ToImmutableDictionary(StringComparer.Ordinal),
            unavailable: _unavailable.ToImmutableDictionary(StringComparer.Ordinal));
    }

    internal static bool IsFileBinding(string key) => key.EndsWith(CodebaseMemoryNativeOutputs.ConfigurationRootSuffix, StringComparison.Ordinal)
        || key == CodebaseMemoryDefinitions.GitLabHooksRoot;

    private string Home(string suffix) => ToolHomePaths.Join(home, suffix);
    private string DirectoryRoot(string key) => _directories.GetValueOrDefault(key)
        ?? throw new IOException($"The native client directory is unavailable: {key}.");
    private string FilePath(string key) => _files.GetValueOrDefault(key)
        ?? throw new IOException($"The native client file is unavailable: {key}.");
    private static string ConfigKey(string id) => CodebaseMemoryNativeOutputs.ConfigurationRootKey(id);
    private string DirectoryFile(string id, string suffix) => ToolHomePaths.Join(DirectoryRoot(id), suffix);
    private string ReadRuntimeParent() => runtimeParent();

    private void Bind(ToolClientRootRecipe recipe)
    {
        string? path;
        try { path = injected.TryGetValue(recipe.Key, out var supplied) ? supplied : recipe.Resolve(this); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or JsonException)
        { _unavailable.Add(recipe.Key, exception.Message); return; }
        BindPath(recipe.Key, recipe.Directory, path);
    }

    private void BindPath(string key, bool directory, string? path)
    {
        try
        {
            if (path is null) throw new IOException($"The native client path has no available choice: {key}.");
            (directory ? _directories : _files).Add(key, ToolHomePaths.Resolve(path, directory));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
        { _unavailable.Add(key, exception.Message); }
    }

    private string ExpandedOrHome(string variable, string suffix) => environment.Expanded(variable, home) ?? Home(suffix);
    private string RawOrHome(string variable, string suffix) => environment.Nonempty(variable) ?? Home(suffix);
}
