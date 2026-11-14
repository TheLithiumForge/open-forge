using System.Text;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.NativeSettings;

internal sealed class ToolNativeSettings(IToolProcessHost processes)
{
    internal const string CodeGraphDirectoryName = ".codegraph";
    internal const string TelemetryFileName = "telemetry.json";
    private const int MaximumWriteOutputBytes = 64 * 1024;
    private static readonly TimeSpan WriteTimeout = TimeSpan.FromSeconds(30);
    internal static readonly string[] ApplyArguments = ["telemetry", ToolNativeConfigDefinitions.DisabledValue];
    internal static readonly string[] ReadArguments = ["--no-color", "telemetry", "status"];

    internal static IReadOnlyDictionary<string, string>? EnvironmentOverrides(ToolNativeConfigChange change, IReadOnlyDictionary<string, string>? environment)
    {
        Validate(change);
        if (environment is null) return null;
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        if (environment.TryGetValue(ToolNativeConfigDefinitions.DoNotTrackVariable, out var track) && !string.IsNullOrEmpty(track) && !IsFalse(track))
            result.Add(ToolNativeConfigDefinitions.DoNotTrackVariable, ToolNativeConfigDefinitions.DisabledValue);
        else if (environment.TryGetValue(ToolNativeConfigDefinitions.CodeGraphTelemetryVariable, out var telemetry) && !string.IsNullOrEmpty(telemetry))
            result.Add(ToolNativeConfigDefinitions.CodeGraphTelemetryVariable, IsFalse(telemetry) ? ToolNativeConfigDefinitions.DisabledValue : ToolNativeConfigDefinitions.EnabledValue);
        return result;
    }

    private static bool IsFalse(string value) => value == "0" || string.Equals(value, "false", StringComparison.OrdinalIgnoreCase);

    internal async Task<ToolRecordedValue> ReadAsync(ToolConfigRequest request, ToolNativeConfigChange change, CancellationToken token)
    {
        Validate(change);
        if (change.ReadArguments is { } authoredRead) foreach (var value in authoredRead) _ = ToolRecipeExpansion.Expand(value, request.Context);
        if (change.ReadEnvironment is { } authoredEnvironment)
            foreach (var value in authoredEnvironment.Values) if (value is not null) _ = ToolRecipeExpansion.Expand(value, request.Context);
        if (change.StoredValue is { } stored)
        {
            if (stored.Parser != ToolNativeStoredValueParser.CodeGraphTelemetryV1) throw new ArgumentOutOfRangeException(nameof(change));
            if (stored.MaximumBytes <= 0 || stored.MissingValue != ToolNativeConfigDefinitions.EnabledValue)
                throw new InvalidDataException("The stored native setting reader has invalid bounds or a missing-value default.");
            var path = ToolRecipeExpansion.Expand(stored.PathTemplate, request.Context);
            var scope = ToolNativePaths.Scope(request.Context, path);
            var state = ToolFileReader.Observe(scope, path, token);
            if (state == NoFollowLeafState.Missing) return Value(stored.MissingValue, present: false);
            if (state != NoFollowLeafState.OrdinaryFile) throw new IOException("The stored native setting is not an ordinary file.");
            var resolution = new PhysicalPathResolver().ResolveCandidate(scope.LexicalRoot, scope.PhysicalRoot, path);
            if (resolution.State != PhysicalPathState.Contained) throw new IOException("The stored native setting is outside its physical scope.");
            await using var input = new FileStream(resolution.GetContainedPhysicalPath(), FileMode.Open, FileAccess.Read, FileShare.Read);
            if (input.Length > stored.MaximumBytes) throw new InvalidDataException("The stored native setting exceeds its read bound.");
            using var bytes = new MemoryStream();
            var buffer = new byte[Math.Min(stored.MaximumBytes, 4096)];
            while (true)
            {
                var count = await input.ReadAsync(buffer, token).ConfigureAwait(false);
                if (count == 0) break;
                if (bytes.Length + count > stored.MaximumBytes) throw new InvalidDataException("The stored native setting exceeds its read bound.");
                bytes.Write(buffer, 0, count);
            }
            if (ToolFileReader.Observe(scope, path, token) != NoFollowLeafState.OrdinaryFile) throw new IOException("The native setting path changed during its read.");
            return Value(ToolNativeValueParser.Stored(stored.Parser, bytes.ToArray(), stored.MissingValue));
        }
        if (request.InstalledInvocation is not { } invocation || change.ReadArguments is null || change.ValueParserId is null
            || change.ReadTimeout is not { } timeout || timeout <= TimeSpan.Zero || change.ReadMaximumOutputBytes is not { } bound || bound <= 0)
            throw new InvalidDataException("The native setting needs a stored reader or a bounded read through a verified installed invocation.");
        if (!change.ReadArguments.SequenceEqual(ReadArguments, StringComparer.Ordinal) || change.ValueParserId != ToolNativeConfigDefinitions.CodeGraphTelemetryStatusParserId)
            throw new InvalidDataException("The native setting read command is outside its allow-list.");
        var overlay = change.ReadEnvironment ?? new Dictionary<string, string?>();
        if (!overlay.TryGetValue(ToolNativeConfigDefinitions.DoNotTrackVariable, out var track) || track is not null
            || !overlay.TryGetValue(ToolNativeConfigDefinitions.CodeGraphTelemetryVariable, out var telemetry) || telemetry is not null
            || !overlay.TryGetValue(ToolNativeConfigDefinitions.CodeGraphNoDownloadVariable, out var download) || download != "1")
            throw new InvalidDataException("The native setting read must separate inherited telemetry overrides.");
        var result = await processes.RunAsync(new()
        {
            ExecutablePath = invocation.ExecutablePath,
            Arguments = invocation.PrefixArguments.Concat(change.ReadArguments.Select(value => ToolRecipeExpansion.Expand(value, request.Context))).ToArray(),
            WorkingDirectory = request.Context.WorkspacePath,
            Environment = overlay.ToDictionary(pair => pair.Key, pair => pair.Value is null ? null : ToolRecipeExpansion.Expand(pair.Value, request.Context), StringComparer.Ordinal),
            Timeout = timeout,
            MaximumOutputBytes = bound,
        }, token).ConfigureAwait(false);
        RequireSuccess(result, bound);
        return Value(ToolNativeValueParser.Command(change.ValueParserId, result.StandardOutput));
    }

    internal async Task ApplyAsync(ToolConfigRequest request, ToolNativeConfigChange change, CancellationToken token)
    {
        Validate(change);
        var invocation = request.Invocation ?? throw new InvalidDataException("The native setting write invocation is unavailable.");
        var result = await processes.RunAsync(new()
        {
            ExecutablePath = invocation.ExecutablePath,
            Arguments = invocation.PrefixArguments.Concat(change.ApplyArguments.Select(value => ToolRecipeExpansion.Expand(value, request.Context))).ToArray(),
            WorkingDirectory = request.Context.WorkspacePath,
            Environment = new Dictionary<string, string?>(),
            Timeout = WriteTimeout,
            MaximumOutputBytes = MaximumWriteOutputBytes,
        }, token).ConfigureAwait(false);
        RequireSuccess(result, MaximumWriteOutputBytes);
    }

    internal static void Validate(ToolNativeConfigChange change)
    {
        if (change.Scope != ToolEffectScope.Account || change.AllowListId != ToolNativeConfigDefinitions.CodeGraphTelemetryAllowListId || change.DesiredValue != ToolNativeConfigDefinitions.DisabledValue
            || !change.ApplyArguments.SequenceEqual(ApplyArguments, StringComparer.Ordinal)) throw new InvalidDataException("The native setting write is outside its allow-list.");
    }

    internal static ToolRecordedValue Value(string value, bool present = true) => new() { Present = present, Kind = ToolRecordedValueKind.Native, Native = value };
    internal static void RequireSuccess(ToolProcessResult result, int bound)
    {
        if (result is not { ExitCode: 0, TimedOut: false, Cancelled: false, Failure: null }
            || Encoding.UTF8.GetByteCount(result.StandardOutput) > bound || Encoding.UTF8.GetByteCount(result.StandardError) > bound)
            throw new IOException("The native command failed or returned incomplete output.");
    }
}
