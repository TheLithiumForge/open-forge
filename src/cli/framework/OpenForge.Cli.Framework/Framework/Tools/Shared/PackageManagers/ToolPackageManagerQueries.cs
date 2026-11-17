using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.PackageManagers;

internal sealed class ToolPackageManagerQueries(IToolProcessHost processes)
{
    private const string BunModulesSuffix = " node_modules";
    internal async Task<string?> RootAsync(ToolPackageManagerFacts manager, string workspace, CancellationToken token)
    {
        IReadOnlyList<string> arguments = manager.Client switch
        {
            ToolNpmClient.Npm or ToolNpmClient.Pnpm => ["root", "--global"],
            ToolNpmClient.YarnClassic => ["global", "dir"],
            ToolNpmClient.Bun => ["pm", "ls", "--global"],
            ToolNpmClient.YarnModern => [],
            _ => throw new ArgumentOutOfRangeException(nameof(manager)),
        };
        if (arguments.Count == 0) return null;
        var result = await processes.RunAsync(ToolPackageManagerDetector.ReadRequest(manager.Invocation, arguments, workspace), token).ConfigureAwait(false);
        if (!ToolPackageManagerDetector.Succeeded(result)) return null;
        var output = result.StandardOutput.Trim();
        if (manager.Client == ToolNpmClient.Bun)
        {
            output = output.Split('\n')[0].TrimEnd('\r');
            var suffix = output.IndexOf(BunModulesSuffix, StringComparison.Ordinal);
            if (suffix < 0) return null;
            output = System.IO.Path.Combine(output[..suffix], ToolNpmPackageReader.ModulesDirectory);
        }
        if (manager.Client == ToolNpmClient.YarnClassic) output = System.IO.Path.Combine(output, ToolNpmPackageReader.ModulesDirectory);
        return System.IO.Path.IsPathFullyQualified(output) && !output.Contains('\n') ? output : null;
    }

    internal async Task<IReadOnlyList<string>> VersionsAsync(ToolPackageManagerFacts manager, string package, string workspace, CancellationToken token)
    {
        IReadOnlyList<string> arguments = manager.Client switch
        {
            ToolNpmClient.Npm or ToolNpmClient.Pnpm => ["view", package, "versions", "--json"],
            ToolNpmClient.YarnClassic => ["info", package, "versions", "--json"],
            ToolNpmClient.Bun => ["info", package, "versions", "--json"],
            ToolNpmClient.YarnModern => [],
            _ => throw new ArgumentOutOfRangeException(nameof(manager)),
        };
        if (arguments.Count == 0) return [];
        var result = await processes.RunAsync(ToolPackageManagerDetector.ReadRequest(manager.Invocation, arguments, workspace), token).ConfigureAwait(false);
        if (!ToolPackageManagerDetector.Succeeded(result)) return [];
        using var json = JsonDocument.Parse(result.StandardOutput);
        var values = json.RootElement;
        if (manager.Client == ToolNpmClient.YarnClassic && values.ValueKind == JsonValueKind.Object && values.TryGetProperty("data", out var data)) values = data;
        if (values.ValueKind == JsonValueKind.String && values.GetString() is { } single) return [single];
        return values.ValueKind == JsonValueKind.Array ? values.EnumerateArray().Where(value => value.ValueKind == JsonValueKind.String).Select(value => value.GetString()).OfType<string>().ToArray() : [];
    }

}
