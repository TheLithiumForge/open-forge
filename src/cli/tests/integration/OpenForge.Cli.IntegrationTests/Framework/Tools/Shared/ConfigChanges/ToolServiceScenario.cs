using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;
using OpenForge.Cli.Core.Framework.Tools.Shared.NativeSettings;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges;

internal sealed class ToolServiceScenario : IDisposable, IToolProcessHost, IToolNativePlanReader
{
    internal const string ToolId = "synthetic-native";
    internal readonly ToolConfigFixture Files = new("tool-service");
    internal ToolRecipeContext Context { get; set; }
    internal ToolBackupService Backups { get; }
    internal ToolConfigService Service { get; }
    internal List<ToolProcessRequest> Calls { get; } = [];
    internal Func<ToolProcessRequest, ToolProcessResult>? Run { get; set; }
    internal bool? Ignored { get; set; }
    internal ToolNativePlanObservation Inventory { get; set; } = new() { DetectedClients = [], Paths = [], Problems = [] };
    internal string NativeFile => Path.Combine(Context.NativeHomePath, ToolNativeSettings.CodeGraphDirectoryName, ToolNativeSettings.TelemetryFileName);
    internal ToolCommandTarget Invocation => new() { ExecutablePath = Files.Combine("future-executable"), PrefixArguments = ["synthetic-prefix"] };

    internal ToolServiceScenario()
    {
        Context = new()
        {
            WorkspacePath = Files.Path,
            HomePath = Files.Combine("home"),
            BinPath = Files.Combine("home/bin"),
            NativeHomePath = Files.Combine("native-home"),
            NativeInstallDirectory = Files.Combine("native-install"),
            MainRoot = "context",
            ClientRoots = new Dictionary<string, string>()
        };
        Backups = new(this);
        Service = new(this, Backups, this, _ => "${mainRoot}\n"u8.ToArray());
    }

    internal ToolConfigRequest Request(params ToolConfigChange[] changes) => new()
    { ToolId = ToolId, Context = Context, ProfileDigest = "synthetic-profile", Invocation = Invocation, Changes = changes, AppliedChanges = [], NativeSettings = [] };

    internal static ToolNativeConfigChange Native(bool stored = true) => new()
    {
        Id = "telemetry",
        Scope = ToolEffectScope.Account,
        InitialState = ToolInitialConfigState.ReadableNativeValue,
        Description = "synthetic telemetry deletion effect",
        AllowListId = ToolNativeConfigDefinitions.CodeGraphTelemetryAllowListId,
        ApplyArguments = ToolNativeSettings.ApplyArguments,
        ReadArguments = ToolNativeSettings.ReadArguments,
        ReadTimeout = TimeSpan.FromSeconds(5),
        ReadMaximumOutputBytes = 4096,
        ReadEnvironment = new Dictionary<string, string?>
        {
            [ToolNativeConfigDefinitions.DoNotTrackVariable] = null,
            [ToolNativeConfigDefinitions.CodeGraphTelemetryVariable] = null,
            [ToolNativeConfigDefinitions.CodeGraphNoDownloadVariable] = "1"
        },
        ValueParserId = ToolNativeConfigDefinitions.CodeGraphTelemetryStatusParserId,
        StoredValue = stored ? new()
        {
            PathTemplate = ToolRecipeDefinitions.NativeHomeToken + "/" + ToolNativeSettings.CodeGraphDirectoryName + "/" + ToolNativeSettings.TelemetryFileName,
            Parser = ToolNativeStoredValueParser.CodeGraphTelemetryV1,
            MissingValue = ToolNativeConfigDefinitions.EnabledValue,
            MaximumBytes = 4096
        } : null,
        DesiredValue = ToolNativeConfigDefinitions.DisabledValue,
        ManualRestoreArgumentsTemplate = [ToolNativeSettings.ApplyArguments[0], ToolRecipeDefinitions.PreviousValueToken],
    };

    internal void WriteNative(bool enabled)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(NativeFile) ?? throw new InvalidOperationException());
        using var stream = File.Create(NativeFile);
        using var writer = new Utf8JsonWriter(stream);
        writer.WriteStartObject();
        writer.WriteBoolean(ToolNativeValueParser.EnabledProperty, enabled);
        if (enabled) writer.WriteString(ToolNativeValueParser.MachineIdProperty, "synthetic-identity");
        else writer.WriteNull(ToolNativeValueParser.MachineIdProperty);
        writer.WriteEndObject();
    }

    internal async Task CompleteAsync(IEnumerable<ToolBackupCompletionRequest> pending)
    {
        foreach (var request in pending.GroupBy(value => value.Backup.TargetPath, OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal).Select(group => group.Last()))
            Assert.Null((await Backups.CompleteAsync(request with { Verified = true }, TestContext.Current.CancellationToken)).Failure);
    }

    public Task<ToolResolvedCommand?> ResolveAsync(string command, string workingDirectory, CancellationToken cancellationToken)
        => Task.FromResult(Ignored is null ? null : new ToolResolvedCommand(Invocation.ExecutablePath, Invocation));
    public Task<ToolProcessResult> RunAsync(ToolProcessRequest request, CancellationToken cancellationToken)
    {
        Calls.Add(request);
        return Task.FromResult(Run?.Invoke(request) ?? Success());
    }
    public ToolNativePlanObservation Read(ToolNativePlanParser parser, ToolProcessResult result)
        => parser == ToolNativePlanParser.CbmInstallPlanV1 ? Inventory : throw new ArgumentOutOfRangeException(nameof(parser));
    internal static ToolProcessResult Success(string stdout = "") => new() { ExitCode = 0, TimedOut = false, Cancelled = false, StandardOutput = stdout, StandardError = string.Empty };
    internal static ToolSetupSpec Setup(params ToolNativeOutputSpec[] outputs) => new()
    {
        Arguments = ["install", ToolRecipeDefinitions.NativeInstallDirectoryToken],
        Environment = new Dictionary<string, string?>(),
        Effects = [],
        Timeout = TimeSpan.FromSeconds(10),
        MaximumOutputBytes = 4096,
        PossibleOutputs = outputs
    };
    public void Dispose() => Files.Dispose();
}
