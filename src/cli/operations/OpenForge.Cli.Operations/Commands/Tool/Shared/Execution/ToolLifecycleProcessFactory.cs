using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Execution;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Execution;
using OpenForge.Cli.Core.Framework.Tools.Shared.Templates;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Execution;

// Planners provide resolved executable facts and the complete reviewed profile environment.
internal static class ToolLifecycleProcessFactory
{
    internal static ToolProcessRequest[] Create(ToolLifecyclePreparation input)
    {
        if (!ToolIdentity.IsSha256(input.Executable.Sha256))
            throw new ArgumentException("The executable needs its verified hash before process preparation.", nameof(input));
        var commands = Describe(new()
        {
            Lifecycle = input.Lifecycle,
            Registration = input.Registration,
            Action = input.Action,
            Executable = new(input.Executable.Root, input.Executable.Entry, input.Executable.PrefixArguments),
            Values = input.Values,
            Hosts = input.Hosts,
            Environment = input.Environment,
        });
        return [.. commands.Select(command => new ToolProcessRequest
        {
            Action = command.Action,
            Host = command.Host,
            ExecutableRoot = command.ExecutableRoot,
            Executable = command.Executable,
            EntrySha256 = input.Executable.Sha256,
            WorkingDirectory = command.WorkingDirectory,
            Arguments = command.Arguments,
            Environment = command.Environment,
            TimeoutSeconds = command.TimeoutSeconds,
        })];
    }

    internal static ToolProcessDescription[] Describe(ToolLifecycleDescription input)
    {
        var lifecycle = input.Lifecycle;
        ToolLifecycleValidator.Validate(lifecycle);
        var command = input.Action switch
        {
            ToolVocabulary.InstallAction => lifecycle.Install,
            ToolVocabulary.UninstallAction => lifecycle.Uninstall,
            ToolVocabulary.IndexCleanupAction => lifecycle.IndexCleanup,
            ToolVocabulary.ActivateAction => input.Registration?.Activate,
            ToolVocabulary.DeactivateAction => input.Registration?.Deactivate,
            ToolVocabulary.TelemetryOnAction => lifecycle.TelemetryOn,
            ToolVocabulary.TelemetryOffAction => lifecycle.TelemetryOff,
            _ => throw new ArgumentException("The action isn't a declared lifecycle command.", nameof(input)),
        } ?? throw new ArgumentException("The selected action has no reviewed command declaration.", nameof(input));
        ToolLifecycleValidator.Validate(command);
        Validate(input);
        ToolManifestValidator.Distinct(input.Hosts, "selected command hosts");
        ToolManifestValidator.Require(input.Hosts.All(ToolVocabulary.Hosts.Contains), "unknown selected command host");
        ToolManifestValidator.Environment(input.Environment);
        string?[] hosts = command.Scope == ToolVocabulary.HostScope ? input.Hosts.Order(StringComparer.Ordinal).Cast<string?>().ToArray() : [null];
        var requests = new List<ToolProcessDescription>();
        foreach (var host in hosts)
        {
            var values = Values(input, host);
            requests.Add(new()
            {
                Action = input.Action,
                Host = host,
                ExecutableRoot = input.Executable.Root,
                Executable = input.Executable.Entry,
                WorkingDirectory = input.Values.Worktree,
                Arguments = [.. input.Executable.PrefixArguments, .. command.Args.Select(argument => ToolTemplate.Render(argument, values))],
                Environment = input.Environment.ToDictionary(pair => pair.Key, pair => ToolTemplate.Render(pair.Value, values), StringComparer.Ordinal),
                TimeoutSeconds = command.TimeoutSeconds,
            });
        }
        return [.. requests];
    }

    private static Dictionary<string, string?> Values(ToolLifecycleDescription input, string? host) => new(StringComparer.Ordinal)
    {
        [ToolVocabulary.InstallVariable] = input.Executable.Root,
        [ToolVocabulary.EntryVariable] = input.Executable.Entry,
        [ToolVocabulary.WorktreeVariable] = input.Values.Worktree,
        [ToolVocabulary.CodeRootVariable] = input.Values.CodeRoot,
        [ToolVocabulary.TargetVariable] = input.Values.Target,
        [ToolVocabulary.StateVariable] = input.Values.State,
        [ToolVocabulary.CacheVariable] = input.Values.Cache,
        [ToolVocabulary.HookSinkVariable] = input.Values.HookSink,
        [ToolVocabulary.ProjectIdVariable] = input.Values.ProjectId,
        [ToolVocabulary.IndexDirNameVariable] = input.Values.IndexDirName,
        [ToolVocabulary.HostVariable] = host,
        [ToolVocabulary.OpenForgeVariable] = input.Values.OpenForge,
    };

    private static void Validate(ToolLifecycleDescription input)
    {
        if (input.Executable.PrefixArguments.Any(argument => argument is null || argument.Contains('\0')))
            throw new ArgumentException("Launcher arguments cannot contain null values or characters.", nameof(input));
        foreach (var path in new[] { input.Executable.Root, input.Executable.Entry, input.Values.Worktree, input.Values.CodeRoot })
            if (!Path.IsPathFullyQualified(path)) throw new ArgumentException("Tool command paths must already be absolute.", nameof(input));
        var entry = Path.GetRelativePath(input.Executable.Root, input.Executable.Entry).Replace(Path.DirectorySeparatorChar, '/');
        var root = Path.GetRelativePath(input.Values.Worktree, input.Values.CodeRoot).Replace(Path.DirectorySeparatorChar, '/');
        if (!ToolIdentity.IsRelativePath(entry) || root != "." && !ToolIdentity.IsRelativePath(root)
            || !ToolVocabulary.Targets.Contains(input.Values.Target))
            throw new ArgumentException("The tool executable, code root, hash or target is invalid.", nameof(input));
        foreach (var path in new[] { input.Values.State, input.Values.Cache, input.Values.HookSink })
            if (path is not null && !Path.IsPathFullyQualified(path)) throw new ArgumentException("Tool state and cache paths must already be absolute.", nameof(input));
        if (input.Values.IndexDirName is { } segment && (!ToolIdentity.IsRelativePath(segment) || segment.Contains('/')))
            throw new ArgumentException("The index directory name must contain one valid segment.", nameof(input));
    }
}
