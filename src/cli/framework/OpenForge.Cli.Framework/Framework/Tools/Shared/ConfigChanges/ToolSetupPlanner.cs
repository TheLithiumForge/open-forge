using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Tools.Shared.NativeSettings;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;

internal sealed class ToolSetupPlanner(IToolProcessHost processes, IToolNativePlanReader nativePlans, ToolBackupService backups)
{
    internal async Task<ToolSetupPlan> PlanAsync(ToolSetupRequest request, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);
        var problems = new List<string>();
        var outputs = new List<ToolResolvedNativeOutput>();
        var expected = new Dictionary<string, string?>(ToolNativePaths.Comparer);
        IReadOnlyList<string>? clients = null;
        var preview = ToolNativePreviewKind.Description;
        ToolProcessRequest? command = null;
        try
        {
            ToolRecipeExpansion.Validate(request.Context);
            command = Command(request, request.Setup.Arguments, request.Setup.Timeout, request.Setup.MaximumOutputBytes);
            if (request.Setup.NativePlan is { } authoredPlan) _ = Command(request, authoredPlan.Arguments, authoredPlan.Timeout, authoredPlan.MaximumOutputBytes);
            if (request.Setup.DryRunArguments is { } authoredDryRun) _ = Command(request, authoredDryRun, request.Setup.Timeout, request.Setup.MaximumOutputBytes);
            foreach (var effect in request.Setup.Effects)
                if (effect.PathTemplate is { } path) _ = ToolNativeOutputs.PathFor(request.Context, path, effect.Scope);
            ToolNativePlanObservation? inventory = null;
            if (request.InstalledInvocation is { } installed)
            {
                if (request.Setup.NativePlan is { } plan)
                {
                    var probe = Command(request with { Invocation = installed }, plan.Arguments, plan.Timeout, plan.MaximumOutputBytes);
                    var result = await processes.RunAsync(probe, token).ConfigureAwait(false);
                    ToolNativeSettings.RequireSuccess(result, plan.MaximumOutputBytes);
                    inventory = nativePlans.Read(plan.Parser, result);
                    problems.AddRange(inventory.Problems);
                    clients = inventory.DetectedClients;
                    preview = ToolNativePreviewKind.Plan;
                }
                else if (request.Setup.DryRunArguments is { } dryRun)
                {
                    var result = await processes.RunAsync(Command(request with { Invocation = installed }, dryRun, request.Setup.Timeout, request.Setup.MaximumOutputBytes), token).ConfigureAwait(false);
                    ToolNativeSettings.RequireSuccess(result, request.Setup.MaximumOutputBytes);
                    preview = ToolNativePreviewKind.DryRun;
                }
            }
            foreach (var declaration in request.Setup.PossibleOutputs)
            {
                if (declaration.OperatingSystem is { } operatingSystem && !CurrentPlatform(operatingSystem)) continue;
                if (declaration.ClientId is { } client && clients is not null && !clients.Contains(client, StringComparer.Ordinal)) continue;
                if (declaration.Purpose is not (ToolNativeOutputPurpose.Backup or ToolNativeOutputPurpose.ReportOnly)) throw new ArgumentOutOfRangeException(nameof(request));
                var conditionalDescription = request.InstalledInvocation is null && request.Setup.NativePlan is not null && declaration.ClientId is not null;
                var pattern = conditionalDescription
                    ? ToolNativeOutputs.AvailablePathFor(request.Context, declaration.PathTemplate, declaration.Scope)
                    : ToolNativeOutputs.PathFor(request.Context, declaration.PathTemplate, declaration.Scope);
                if (pattern is null) continue;
                ToolNativeOutputs.ValidatePattern(pattern, declaration.Match);
                outputs.Add(new(pattern, declaration.Scope, declaration.Match, declaration.Purpose, declaration.ClientId));
            }
            var targets = new HashSet<string>(ToolNativePaths.Comparer);
            foreach (var output in outputs.Where(output => output.Purpose == ToolNativeOutputPurpose.Backup))
                foreach (var path in ToolNativeOutputs.Expand(request.Context, output.PathPattern, output.Match, token)) targets.Add(path);
            if (inventory is not null && inventory.Problems.Count == 0)
            {
                foreach (var path in inventory.Paths)
                {
                    var absolute = Path.GetFullPath(path, request.Context.WorkspacePath);
                    var declaration = outputs.FirstOrDefault(output => ToolNativeOutputs.Matches(output.PathPattern, absolute, output.Match));
                    if (declaration is null) { problems.Add("Native inventory named an output outside the reviewed possible set."); continue; }
                    if (declaration.Purpose == ToolNativeOutputPurpose.Backup)
                    {
                        _ = ToolNativePaths.Scope(request.Context, absolute);
                        targets.Add(absolute);
                    }
                }
            }
            foreach (var target in targets)
            {
                var scope = ToolNativePaths.Scope(request.Context, target);
                var snapshot = await ToolFileReader.ReadAsync(scope, target, token).ConfigureAwait(false);
                await backups.CheckFeasibilityAsync(target, token).ConfigureAwait(false);
                expected[target] = snapshot.ContentHash;
            }
        }
        catch (Exception exception) when (exception is IOException or InvalidDataException or UnauthorizedAccessException or ArgumentException)
        { problems.Add(exception.Message); }
        command ??= new ToolProcessRequest
        {
            ExecutablePath = request.Invocation.ExecutablePath,
            Arguments = [],
            WorkingDirectory = request.Context.WorkspacePath,
            Environment = new Dictionary<string, string?>(),
            Timeout = request.Setup.Timeout,
            MaximumOutputBytes = request.Setup.MaximumOutputBytes
        };
        return new() { Request = request, Command = command, PreviewKind = preview, DetectedClients = clients, PossibleOutputs = outputs, ExpectedFileSha256 = expected, Problems = problems };
    }

    private static ToolProcessRequest Command(ToolSetupRequest request, IReadOnlyList<string> arguments, TimeSpan timeout, int bound)
    {
        if (timeout <= TimeSpan.Zero || bound <= 0) throw new InvalidDataException("Native setup process bounds must be positive.");
        return new()
        {
            ExecutablePath = request.Invocation.ExecutablePath,
            Arguments = request.Invocation.PrefixArguments.Concat(arguments.Select(value => ToolRecipeExpansion.Expand(value, request.Context))).ToArray(),
            WorkingDirectory = request.Context.WorkspacePath,
            Environment = request.Setup.Environment.ToDictionary(pair => pair.Key, pair => pair.Value is null ? null : ToolRecipeExpansion.Expand(pair.Value, request.Context), StringComparer.Ordinal),
            Timeout = timeout,
            MaximumOutputBytes = bound,
        };
    }

    private static bool CurrentPlatform(ToolOperatingSystem operatingSystem) => operatingSystem switch
    {
        ToolOperatingSystem.Windows => OperatingSystem.IsWindows(),
        ToolOperatingSystem.MacOS => OperatingSystem.IsMacOS(),
        ToolOperatingSystem.Linux => OperatingSystem.IsLinux(),
        _ => throw new ArgumentOutOfRangeException(nameof(operatingSystem)),
    };
}
