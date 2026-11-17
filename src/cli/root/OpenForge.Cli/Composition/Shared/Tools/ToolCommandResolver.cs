using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.PackageManagers;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;

namespace OpenForge.Cli.Composition.Shared.Tools;

internal sealed class ToolCommandResolver(IReadOnlyDictionary<string, string?> environment, ToolCatalogue? catalogue = null)
{
    internal const string CodeGraphId = "codegraph";
    internal ToolResolvedCommand? Resolve(string command, string workingDirectory)
    {
        try
        {
            var path = Find(command, workingDirectory);
            if (path is null) return null;
            if (Bundle(path) is { } bundle) return bundle;
            var extension = System.IO.Path.GetExtension(path);
            if (!extension.Equals(".cmd", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".bat", StringComparison.OrdinalIgnoreCase))
                return new(path, new() { ExecutablePath = path, PrefixArguments = [] });
            var parent = System.IO.Path.GetDirectoryName(path);
            if (parent is null) return null;
            var name = System.IO.Path.GetFileNameWithoutExtension(path);
            var node = Find("node", parent);
            var adjacent = System.IO.Path.Combine(parent, "node.exe");
            if (File.Exists(adjacent)) node = adjacent;
            if (node is null) throw new NotSupportedException("The command's Node interpreter isn't available.");
            foreach (var package in ToolNpmPackageReader.PackageDirectories(System.IO.Path.Combine(parent, ToolNpmPackageReader.ModulesDirectory)))
                if (ToolNpmPackageReader.Read(package, name) is { } manifest && File.Exists(manifest.Entry))
                    return new(path, new() { ExecutablePath = node, PrefixArguments = [manifest.Entry] });
            throw new NotSupportedException("The command needs a declared executable invocation.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or System.Text.Json.JsonException or ArgumentException) { return null; }
    }

    private ToolResolvedCommand? Bundle(string command)
    {
        if (catalogue is null) return null;
        foreach (var asset in catalogue.Entries.Values.SelectMany(entry => entry.Channels).OfType<OpenForge.Cli.Core.Framework.Tools.Models.Channels.ToolGitHubReleaseChannel>().SelectMany(channel => channel.Assets))
        {
            if (asset.BundledRuntime is not { } runtime) continue;
            var relative = asset.EntryRelativePath.Replace('/', System.IO.Path.DirectorySeparatorChar);
            if (!command.EndsWith(System.IO.Path.DirectorySeparatorChar + relative, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) continue;
            var root = command[..^relative.Length];
            var executable = System.IO.Path.Combine(root, runtime.RuntimeRelativePath.Replace('/', System.IO.Path.DirectorySeparatorChar));
            var entry = System.IO.Path.Combine(root, runtime.EntryRelativePath.Replace('/', System.IO.Path.DirectorySeparatorChar));
            if (File.Exists(executable) && File.Exists(entry)) return new(command, new() { ExecutablePath = executable, PrefixArguments = [.. runtime.Arguments, entry] });
        }
        return null;
    }

    internal bool IsCodeGraphBundle(ToolProcessRequest request)
    {
        if (catalogue is null || !catalogue.Entries.TryGetValue(CodeGraphId, out var entry)) return false;
        foreach (var asset in entry.Channels.OfType<OpenForge.Cli.Core.Framework.Tools.Models.Channels.ToolGitHubReleaseChannel>().SelectMany(channel => channel.Assets))
        {
            if (asset.BundledRuntime is not { } runtime || request.Arguments.Count <= runtime.Arguments.Count
                || !request.Arguments.Take(runtime.Arguments.Count).SequenceEqual(runtime.Arguments)) continue;
            var script = request.Arguments[runtime.Arguments.Count];
            var relative = runtime.EntryRelativePath.Replace('/', System.IO.Path.DirectorySeparatorChar);
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (!script.EndsWith(System.IO.Path.DirectorySeparatorChar + relative, comparison)) continue;
            var root = script[..^relative.Length];
            if (string.Equals(request.ExecutablePath, System.IO.Path.Combine(root, runtime.RuntimeRelativePath.Replace('/', System.IO.Path.DirectorySeparatorChar)), comparison)) return true;
        }
        return false;
    }

    private string? Find(string command, string workingDirectory)
    {
        if (System.IO.Path.IsPathFullyQualified(command)) return File.Exists(command) ? System.IO.Path.GetFullPath(command) : null;
        if (command.Contains('/') || command.Contains('\\'))
        {
            var local = System.IO.Path.GetFullPath(command, workingDirectory);
            return File.Exists(local) ? local : null;
        }
        var path = environment.FirstOrDefault(pair => pair.Key.Equals("PATH", StringComparison.OrdinalIgnoreCase)).Value;
        if (path is null) return null;
        string[] extensions = OperatingSystem.IsWindows() ? [".exe", ".com", ".cmd", ".bat", ""] : [""];
        foreach (var directory in path.Split(System.IO.Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries).Take(512))
        {
            var folder = directory.Trim('"');
            if (!System.IO.Path.IsPathFullyQualified(folder)) continue;
            foreach (var extension in extensions)
            {
                var candidate = System.IO.Path.Combine(folder, command + extension);
                if (!File.Exists(candidate)) continue;
                if (!OperatingSystem.IsWindows() && (File.GetUnixFileMode(candidate) & (UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute)) == 0) continue;
                return System.IO.Path.GetFullPath(candidate);
            }
        }
        return null;
    }
}
