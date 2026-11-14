using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Shared.Execution;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Execution;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Lifecycle;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Templates;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool;

[Trait("Feature", "tool-process-plan"), Trait("Evidence", "Unit")]
public sealed class ToolLifecycleProcessFactoryTests
{
    [Fact(DisplayName = "Lifecycle host commands render separate arguments and independent environments in ordinal host order")]
    public void FormsHostCommands()
    {
        var input = Input(ToolVocabulary.HostScope);
        var commands = ToolLifecycleProcessFactory.Create(input);
        Assert.Equal(input.Hosts.Order(StringComparer.Ordinal), commands.Select(command => command.Host));
        foreach (var command in commands)
        {
            Assert.Equal(input.Action, command.Action);
            Assert.Equal(input.Executable.Root, command.ExecutableRoot);
            Assert.Equal(input.Executable.Entry, command.Executable);
            Assert.Equal(input.Executable.Sha256, command.EntrySha256);
            Assert.Equal(input.Values.Worktree, command.WorkingDirectory);
            Assert.Equal(input.Lifecycle.Install.TimeoutSeconds, command.TimeoutSeconds);
            Assert.Equal([input.Executable.Entry, input.Values.CodeRoot, command.Host, "repeated", "repeated", "literal $(fixture) ; quote\""], command.Arguments);
            Assert.Equal(command.Host, command.Environment[EnvironmentKey]);
        }
        Assert.NotSame(commands[0].Environment, commands[1].Environment);
        Assert.NotSame(commands[0].Arguments, commands[1].Arguments);
        Assert.Equal(Variable(ToolVocabulary.HostVariable), input.Environment[EnvironmentKey]);
    }

    [Theory(DisplayName = "Workspace lifecycle commands form one request independently of selected host count")]
    [InlineData(ToolVocabulary.InstallAction), InlineData(ToolVocabulary.UninstallAction), InlineData(ToolVocabulary.IndexCleanupAction)]
    public void FormsWorkspaceCommand(string action)
    {
        var input = Input(ToolVocabulary.WorkspaceScope) with { Action = action, Hosts = [], Environment = [] };
        var command = Assert.Single(ToolLifecycleProcessFactory.Create(input));
        Assert.Null(command.Host);
        Assert.Equal(action, command.Action);
        Assert.Equal(input.Values.Worktree, command.WorkingDirectory);
        var expected = action switch
        {
            ToolVocabulary.InstallAction => input.Values.Worktree,
            ToolVocabulary.UninstallAction => ToolVocabulary.UninstallAction,
            ToolVocabulary.IndexCleanupAction => input.Values.CodeRoot,
            _ => throw new ArgumentOutOfRangeException(nameof(action)),
        };
        Assert.Equal(expected, Assert.Single(command.Arguments));
    }

    [Theory(DisplayName = "Declared activation and telemetry actions use their exact reviewed command and fixed launcher arguments")]
    [InlineData(ToolVocabulary.ActivateAction), InlineData(ToolVocabulary.DeactivateAction)]
    [InlineData(ToolVocabulary.TelemetryOnAction), InlineData(ToolVocabulary.TelemetryOffAction)]
    public void FormsAdditionalActions(string action)
    {
        var input = Input(ToolVocabulary.WorkspaceScope) with { Action = action, Environment = [] };
        var selected = input.Lifecycle.Install with { Args = [action, Variable(ToolVocabulary.OpenForgeVariable)], TimeoutSeconds = 23 };
        var executable = Path.Combine(input.Executable.Root, "runtime.bin");
        input = input with
        {
            Registration = new() { Activate = selected, Deactivate = selected, Rewrites = [], Components = [], RoleGuidance = [] },
            Lifecycle = input.Lifecycle with { TelemetryOn = selected, TelemetryOff = selected },
            Executable = input.Executable with { Entry = executable, PrefixArguments = [input.Executable.Entry, "literal ${fixture}", "repeated", "repeated"] },
            Values = input.Values with { OpenForge = Path.GetFullPath("open forge host.exe") },
        };
        var command = Assert.Single(ToolLifecycleProcessFactory.Create(input));
        Assert.Equal(action, command.Action);
        Assert.Equal(executable, command.Executable);
        Assert.Equal([.. input.Executable.PrefixArguments, action, input.Values.OpenForge], command.Arguments);
        Assert.Equal(selected.TimeoutSeconds, command.TimeoutSeconds);
    }

    [Theory(DisplayName = "An undeclared activation or telemetry action blocks process preparation")]
    [InlineData(ToolVocabulary.ActivateAction), InlineData(ToolVocabulary.DeactivateAction)]
    [InlineData(ToolVocabulary.TelemetryOnAction), InlineData(ToolVocabulary.TelemetryOffAction)]
    public void RejectsMissingAdditionalActions(string action)
        => Assert.Throws<ArgumentException>(() => ToolLifecycleProcessFactory.Create(Input(ToolVocabulary.WorkspaceScope) with { Action = action }));

    [Fact(DisplayName = "Launcher arguments stay literal and reject null characters before process preparation")]
    public void RejectsInvalidLauncherArguments()
    {
        var input = Input(ToolVocabulary.WorkspaceScope);
        Assert.Throws<ArgumentException>(() => ToolLifecycleProcessFactory.Create(input with
        { Executable = input.Executable with { PrefixArguments = ["bad\0argument"] } }));
    }

    [Fact(DisplayName = "A preview describes exact command inputs without an executable hash and execution still requires that hash")]
    public void DescribesBeforeArtifactVerification()
    {
        var input = Input(ToolVocabulary.HostScope);
        var descriptions = ToolLifecycleProcessFactory.Describe(new()
        {
            Lifecycle = input.Lifecycle,
            Registration = input.Registration,
            Action = input.Action,
            Executable = new(input.Executable.Root, input.Executable.Entry, input.Executable.PrefixArguments),
            Values = input.Values,
            Hosts = input.Hosts,
            Environment = input.Environment,
        });
        var commands = ToolLifecycleProcessFactory.Create(input);
        Assert.Equal(commands.Length, descriptions.Length);
        for (var index = 0; index < commands.Length; index++)
        {
            Assert.Equal(commands[index].Arguments, descriptions[index].Arguments);
            Assert.Equal(commands[index].Environment, descriptions[index].Environment);
            Assert.Equal(commands[index].Executable, descriptions[index].Executable);
            Assert.Equal(commands[index].WorkingDirectory, descriptions[index].WorkingDirectory);
        }
        Assert.Throws<ArgumentException>(() => ToolLifecycleProcessFactory.Create(input with
        { Executable = input.Executable with { Sha256 = string.Empty } }));
    }

    [Fact(DisplayName = "Lifecycle profile environment resolves the declared cache, state and process-scope values")]
    public void ResolvesProfileScope()
    {
        var input = Input(ToolVocabulary.WorkspaceScope);
        var values = input.Values with
        {
            State = Path.GetFullPath("fixture state"),
            Cache = Path.GetFullPath("fixture cache"),
            HookSink = Path.GetFullPath("fixture hook sink"),
            ProjectId = "fixture-project",
            IndexDirName = "fixture-index",
        };
        string[] names = [ToolVocabulary.StateVariable, ToolVocabulary.CacheVariable, ToolVocabulary.HookSinkVariable,
            ToolVocabulary.ProjectIdVariable, ToolVocabulary.IndexDirNameVariable, ToolVocabulary.TargetVariable, ToolVocabulary.InstallVariable];
        input = input with { Values = values, Environment = new() { [EnvironmentKey] = string.Join('|', names.Select(Variable)) } };
        var command = Assert.Single(ToolLifecycleProcessFactory.Create(input));
        Assert.Equal(string.Join('|', values.State, values.Cache, values.HookSink, values.ProjectId, values.IndexDirName, values.Target, input.Executable.Root), command.Environment[EnvironmentKey]);
    }

    [Fact(DisplayName = "Lifecycle requests reject null characters introduced by a resolved scope value")]
    public void RejectsNullResolvedValue()
    {
        var input = Input(ToolVocabulary.WorkspaceScope);
        input = input with
        {
            Values = input.Values with { ProjectId = "fixture\0project" },
            Environment = new() { [EnvironmentKey] = Variable(ToolVocabulary.ProjectIdVariable) },
        };
        Assert.Throws<InvalidOperationException>(() => ToolLifecycleProcessFactory.Create(input));
    }

    [Fact(DisplayName = "Unused unavailable template values do not invent paths and used unavailable values block planning")]
    public void RequiresUsedValues()
    {
        var input = Input(ToolVocabulary.WorkspaceScope) with { Environment = [] };
        Assert.Single(ToolLifecycleProcessFactory.Create(input));
        input = input with { Environment = new() { [EnvironmentKey] = Variable(ToolVocabulary.CacheVariable) } };
        Assert.Throws<InvalidOperationException>(() => ToolLifecycleProcessFactory.Create(input));
    }

    [Fact(DisplayName = "Substitution keeps injected placeholder text literal instead of expanding it again")]
    public void SubstitutesOnce()
    {
        var values = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            [ToolVocabulary.EntryVariable] = Variable(ToolVocabulary.CacheVariable),
            [ToolVocabulary.CacheVariable] = "unused-value",
        };
        Assert.Equal(Variable(ToolVocabulary.CacheVariable), ToolTemplate.Render(Variable(ToolVocabulary.EntryVariable), values));
    }

    [Theory(DisplayName = "Both template admission and rendering reject the same malformed variable syntax")]
    [InlineData("${unknown}"), InlineData("${entry"), InlineData("${}"), InlineData("bad\0value")]
    public void KeepsTemplateValidation(string value)
    {
        Assert.Throws<JsonException>(() => ToolManifestValidator.Template(value));
        Assert.Throws<JsonException>(() => ToolTemplate.Render(value, new Dictionary<string, string?>()));
    }

    [Theory(DisplayName = "Process planning rejects relative or escaping executable and code-root coordinates")]
    [InlineData("relative"), InlineData("entry"), InlineData("code-root")]
    public void RejectsInvalidCoordinates(string situation)
    {
        var input = Input(ToolVocabulary.WorkspaceScope) with { Environment = [] };
        input = situation switch
        {
            "relative" => input with { Executable = input.Executable with { Entry = ToolFixtureSource.ArtifactName } },
            "entry" => input with { Executable = input.Executable with { Entry = Path.GetFullPath("outside.bin", Path.GetDirectoryName(input.Executable.Root) ?? throw new InvalidOperationException()) } },
            "code-root" => input with { Values = input.Values with { CodeRoot = Path.GetFullPath("outside", Path.GetDirectoryName(input.Values.Worktree) ?? throw new InvalidOperationException()) } },
            _ => throw new ArgumentOutOfRangeException(nameof(situation)),
        };
        Assert.Throws<ArgumentException>(() => ToolLifecycleProcessFactory.Create(input));
    }

    [Theory(DisplayName = "Index directory template values require one contained segment")]
    [InlineData(".."), InlineData("nested/index"), InlineData("")]
    public void RejectsIndexSegments(string segment)
    {
        var input = Input(ToolVocabulary.WorkspaceScope) with { Environment = [] };
        Assert.Throws<ArgumentException>(() => ToolLifecycleProcessFactory.Create(input with { Values = input.Values with { IndexDirName = segment } }));
    }

    [Fact(DisplayName = "Unknown lifecycle actions and duplicate selected hosts cannot produce process requests")]
    public void RejectsUnknownSelections()
    {
        var input = Input(ToolVocabulary.HostScope);
        Assert.Throws<ArgumentException>(() => ToolLifecycleProcessFactory.Create(input with { Action = ToolVocabulary.ConfigureAction }));
        Assert.Throws<JsonException>(() => ToolLifecycleProcessFactory.Create(input with { Hosts = [input.Hosts[0], input.Hosts[0]] }));
    }

    private const string EnvironmentKey = "FIXTURE_HOST";
    private static string Variable(string name) => "${" + name + "}";

    private static ToolLifecyclePreparation Input(string scope)
    {
        var root = Path.GetFullPath("fixture install");
        var worktree = Path.GetFullPath("fixture workspace");
        var command = new ToolLifecycleCommand
        {
            Args = scope == ToolVocabulary.HostScope
                ? [Variable(ToolVocabulary.EntryVariable), Variable(ToolVocabulary.CodeRootVariable), Variable(ToolVocabulary.HostVariable), "repeated", "repeated", "literal $(fixture) ; quote\""]
                : [Variable(ToolVocabulary.WorktreeVariable)],
            Scope = scope,
            WorkspaceOutputs = [],
            TimeoutSeconds = 30,
        };
        return new()
        {
            Lifecycle = new()
            {
                Install = command,
                Uninstall = command with { Args = [ToolVocabulary.UninstallAction] },
                IndexCleanup = command with { Scope = ToolVocabulary.WorkspaceScope, Args = [Variable(ToolVocabulary.CodeRootVariable)] },
                IgnorePaths = [],
            },
            Action = ToolVocabulary.InstallAction,
            Executable = new(root, Path.Combine(root, ToolFixtureSource.ArtifactName), ToolCatalogueReader.Hash(ToolFixtureSource.ArtifactBytes())),
            Values = new() { Worktree = worktree, CodeRoot = Path.Combine(worktree, "source code"), Target = ToolVocabulary.Targets.Order(StringComparer.Ordinal).First() },
            Hosts = ToolVocabulary.Hosts.OrderDescending(StringComparer.Ordinal).ToArray(),
            Environment = new() { [EnvironmentKey] = Variable(ToolVocabulary.HostVariable) },
        };
    }
}
