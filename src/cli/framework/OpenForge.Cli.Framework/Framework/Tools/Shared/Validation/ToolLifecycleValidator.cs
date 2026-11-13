using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Validation;

internal static class ToolLifecycleValidator
{
    internal static void Validate(ToolLifecycle lifecycle)
    {
        Command(lifecycle.Install);
        Command(lifecycle.Uninstall);
        Command(lifecycle.IndexCleanup);
        ToolManifestValidator.Require(lifecycle.IndexCleanup.Scope == ToolVocabulary.WorkspaceScope, "index cleanup must select one workspace");
        ToolManifestValidator.Distinct(lifecycle.IgnorePaths, "managed ignore paths");
        foreach (var path in lifecycle.IgnorePaths) WorkspacePath(path.EndsWith('/') ? path[..^1] : path);
    }

    private static void Command(ToolLifecycleCommand command)
    {
        ToolManifestValidator.Require(command is not null, "lifecycle commands cannot be null");
        ToolManifestValidator.Require(command.Scope is ToolVocabulary.WorkspaceScope or ToolVocabulary.HostScope, "invalid lifecycle command scope");
        ToolManifestValidator.Require(command.TimeoutSeconds is > 0 and <= ToolVocabulary.MaximumCommandTimeoutSeconds, "invalid lifecycle command timeout");
        ToolManifestValidator.Require(command.Args is not null && command.Args.All(argument => argument is not null), "lifecycle arguments cannot be null");
        foreach (var argument in command.Args) ToolManifestValidator.Template(argument);
        if (command.Scope == ToolVocabulary.WorkspaceScope)
            ToolManifestValidator.Require(command.Args.All(argument => !argument.Contains("${host}", StringComparison.Ordinal)), "workspace commands cannot select one host");
        ToolManifestValidator.Distinct(command.WorkspaceOutputs, "declared command outputs");
        foreach (var path in command.WorkspaceOutputs) WorkspacePath(path);
    }

    private static void WorkspacePath(string path)
    {
        ToolManifestValidator.Require(ToolIdentity.IsWorkspaceOutputTemplate(path), "command outputs and ignore paths must be contained workspace-relative paths");
    }

    internal static void Validate(IReadOnlyDictionary<string, ToolRunSummary> runs)
    {
        foreach (var (id, run) in runs)
        {
            ToolManifestValidator.Require(ToolIdentity.IsToolId(id) && run is not null && ToolVocabulary.RunActions.Contains(run.Action)
                && Enum.IsDefined(run.State) && run.Commands is not null, "invalid tool run summary");
            foreach (var command in run.Commands)
            {
                ToolManifestValidator.Require(command is not null && ToolVocabulary.LifecycleActions.Contains(command.Action)
                    && (command.Host is null || ToolVocabulary.Hosts.Contains(command.Host)), "invalid lifecycle command identity");
                State(command.State, command.ExitCode);
            }
        }
    }

    internal static void Validate(IReadOnlyDictionary<string, ToolRunRecord> runs)
    {
        foreach (var (id, run) in runs)
        {
            ToolManifestValidator.Require(run is not null && ToolIdentity.IsToolId(id) && ToolIdentity.IsVersion(run.Version)
                && ToolIdentity.IsSha256(run.ManifestSha256) && ToolVocabulary.Targets.Contains(run.Target) && run.Commands is not null, "invalid tool command record");
            ToolManifestValidator.Require(run.ManifestBytes is not null && ToolCatalogueReader.Hash(run.ManifestBytes) == run.ManifestSha256
                && run.ManifestFormat is "json" or "yaml", "invalid command definition snapshot");
            var definition = new ToolCatalogueReader().Definition(run.ManifestBytes, $"run.{run.ManifestFormat}", false);
            ToolManifestValidator.Require(definition.Manifest.Id == id && definition.Manifest.Version == run.Version, "command definition identity differs from the run");
            ToolManifestValidator.Require(run.CodeRoot == "." || ToolIdentity.IsRelativePath(run.CodeRoot), "invalid command code root");
            ToolManifestValidator.Distinct(run.Hosts, "command hosts");
            ToolManifestValidator.Require(run.Hosts.All(ToolVocabulary.Hosts.Contains), "invalid command host");
            Validate(new Dictionary<string, ToolRunSummary>(StringComparer.Ordinal)
            {
                [id] = new(run.Action, run.State, [.. run.Commands.Select(command => command is null
                    ? throw new JsonException("A command receipt cannot be null.") : new ToolCommandSummary(command.Action, command.Host, command.State, command.ExitCode))]),
            });
            foreach (var command in run.Commands)
            {
                ToolManifestValidator.Require(ToolIdentity.IsRelativePath(command.Entry) && ToolIdentity.IsSha256(command.EntrySha256)
                    && command.Args is not null && command.Args.All(argument => argument is not null && !argument.Contains('\0'))
                    && command.Environment is not null && command.Environment.All(pair => pair.Key.Length > 0 && !pair.Key.Contains('=') && !pair.Key.Contains('\0')
                        && pair.Value is not null && !pair.Value.Contains('\0')), "invalid recorded command inputs");
            }
        }
    }

    private static void State(ToolCommandState state, int? exitCode)
    {
        ToolManifestValidator.Require(Enum.IsDefined(state) && (state != ToolCommandState.Exited || exitCode is not null)
            && (state is not (ToolCommandState.Planned or ToolCommandState.Started) || exitCode is null), "invalid command state and exit code");
    }
}
