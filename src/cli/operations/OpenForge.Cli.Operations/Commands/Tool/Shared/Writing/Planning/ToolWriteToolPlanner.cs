using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Planning;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Selection;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Planning;

internal sealed class ToolWriteToolPlanner(
    IToolVersionService versions, IToolChannelService channels, IToolConfigService config, IToolSetupService setup)
{
    internal Task<IReadOnlyList<ToolPackageManagerFacts>> DetectManagersAsync(string workspace, CancellationToken cancellationToken)
        => channels.DetectManagersAsync(workspace, cancellationToken);

    internal async ValueTask<ToolWriteToolPlanningResult> PlanInstallAsync(
        ToolWriteSelection selection, ToolWritePlanningInput input,
        IReadOnlyList<ToolPackageManagerFacts> managers, IReadOnlyList<ToolPrivateNodeSpec> runtimes, CancellationToken cancellationToken)
    {
        var entry = selection.Entry;
        var detected = await DetectAsync(entry, input, cancellationToken).ConfigureAwait(false);
        var chosen = input.Managers.TryGetValue(entry.Identity.Id, out var manager) ? manager : (ToolNpmClient?)null;
        var result = await channels.PlanAsync(new()
        {
            Entry = entry,
            WorkspacePath = input.Request.Workspace.LexicalRoot,
            HomePath = input.Facts.HomePath,
            Selection = selection.Version,
            Existing = detected,
            Target = input.Facts.Target,
            ProjectManagers = input.Facts.ProjectManagers,
            ManagerChoice = input.Request.Yes ? ToolManagerChoiceMode.Default : ToolManagerChoiceMode.Ask,
            AllowLatestLookup = selection.Version.Kind != ToolVersionSelectionKind.Exact,
            AvailableManagers = managers,
            AvailableNodeRuntimes = runtimes,
            ChannelOverride = selection.RequiredChannel,
            ChosenManager = chosen,
        }, cancellationToken).ConfigureAwait(false);
        if (result.Resolution is not { } resolution)
        {
            if (result.ManagerChoices.Count > 1 && result.Problems.Count == 0)
                return new(null, new(entry.Identity.Id, [.. result.ManagerChoices.Select(choice =>
                    new ToolWriteManagerChoice((int)choice.Client, choice.Client.ToString(), choice.Version, choice.GlobalBinPath, choice.GlobalBinReadiness.ToString()))]), [], result.UsedNetwork);
            return new(null, null,
                [new(ToolWriteFindingCode.ChannelBlocked, CliSemanticStatus.Blocked, entry.Identity.Id, Cause: string.Join("\n", result.Problems))], result.UsedNetwork);
        }
        var profile = Profile(entry, resolution.Version);
        if (profile is null)
            return new(null, null, [new(ToolWriteFindingCode.ProfileUnavailable, CliSemanticStatus.Blocked, entry.Identity.Id)], result.UsedNetwork);
        var context = Context(profile, input, resolution.Execution.NativeInstallDirectory);
        if (context is null)
            return new(null, null, [new(ToolWriteFindingCode.NativePlanBlocked, CliSemanticStatus.Blocked, entry.Identity.Id)], result.UsedNetwork);
        var native = await setup.PlanAsync(new()
        {
            Setup = profile.Setup,
            Invocation = resolution.Execution.Invocation,
            InstalledInvocation = detected.Invocation,
            Context = context,
        }, cancellationToken).ConfigureAwait(false);
        var digest = config.ComputeProfileDigest(profile);
        var changes = await config.PlanAsync(new()
        {
            ToolId = entry.Identity.Id,
            Context = context,
            ProfileDigest = digest,
            Invocation = resolution.Execution.Invocation,
            InstalledInvocation = detected.Invocation,
            Changes = profile.Changes,
            AppliedChanges = selection.Lock?.Changes ?? [],
            NativeSettings = NativeSettings(input, entry.Identity.Id),
        }, cancellationToken).ConfigureAwait(false);
        return new(new()
        {
            Entry = entry,
            Detection = detected,
            Channel = resolution,
            Profile = profile,
            Context = context,
            Native = native,
            Config = changes,
            Digest = digest,
        }, null, [], result.UsedNetwork);
    }

    internal async ValueTask<ToolWriteToolPlan> PlanRemoveAsync(
        ToolCatalogueEntry entry, ToolWritePlanningInput input, CancellationToken cancellationToken)
    {
        var id = entry.Identity.Id;
        var portable = input.Records.Portable;
        var locked = portable?.Lock.GetValueOrDefault(id);
        var declared = portable?.Settings.ContainsKey(id) == true;
        var detected = await DetectAsync(entry, input, cancellationToken).ConfigureAwait(false);
        if (locked is null && !declared) return new() { Entry = entry, Detection = detected, NoOp = true };
        var profile = Profile(entry, locked?.Version ?? detected.Version);
        var context = Context(profile, input,
            detected.CommandPath is { } path ? Path.GetDirectoryName(path) ?? input.Facts.Path.BinPath : input.Facts.Path.BinPath);
        if (context is null) return new() { Entry = entry, Detection = detected };
        ToolSetupPlan? native = null;
        ToolSetupRequest? deferred = null;
        if (profile is not null && detected.Invocation is { } invocation)
        {
            var request = new ToolSetupRequest
            {
                Setup = profile.Uninstall,
                Invocation = invocation,
                InstalledInvocation = invocation,
                Context = context,
            };
            if (profile.Uninstall.Effects.Any(effect =>
                effect.Kind is ToolDeclaredEffectKind.PathChange or ToolDeclaredEffectKind.SharedFileDeletion
                || effect.Scope == ToolEffectScope.Account && effect.Kind == ToolDeclaredEffectKind.Configuration))
            {
                deferred = request;
                // This request formats the command only. Keep the complete declaration for confirmed preparation.
                request = request with
                {
                    InstalledInvocation = null,
                    Setup = request.Setup with { DryRunArguments = null, NativePlan = null, Effects = [], PossibleOutputs = [] },
                };
            }
            native = await setup.PlanAsync(request, cancellationToken).ConfigureAwait(false);
        }
        var removal = await config.PlanRemoveAsync(new()
        {
            ToolId = id,
            Context = context,
            Lock = locked ?? new() { Version = detected.Version ?? string.Empty, Channel = detected.ChannelId ?? string.Empty, Changes = [] },
            NativeSettings = NativeSettings(input, id),
        }, cancellationToken).ConfigureAwait(false);
        return new() { Entry = entry, Detection = detected, Profile = profile, Context = context, Native = native, DeferredNativeRemoval = deferred, Removal = removal };
    }

    private Task<ToolDetectionResult> DetectAsync(ToolCatalogueEntry entry, ToolWritePlanningInput input, CancellationToken cancellationToken)
        => channels.DetectAsync(new(entry, input.Request.Workspace.LexicalRoot)
        {
            RecordedState = input.Records.Machine.GetValueOrDefault(entry.Identity.Id),
        }, cancellationToken);

    private ToolVersionProfile? Profile(ToolCatalogueEntry entry, string? version)
        => version is null ? null : entry.Profiles.SingleOrDefault(profile =>
            versions.Satisfies(version, new() { Kind = ToolVersionSelectionKind.MinorRange, Range = profile.Range }));

    private static ToolRecipeContext? Context(ToolVersionProfile? profile, ToolWritePlanningInput input, string installDirectory)
    {
        var policy = profile?.NativeHomePolicy ?? ToolNativeHomePolicy.Node;
        if (!input.Facts.NativeHomes.TryGetValue(policy, out var home) || !Path.IsPathFullyQualified(home)) return null;
        return new()
        {
            WorkspacePath = input.Request.Workspace.LexicalRoot,
            HomePath = input.Facts.HomePath,
            BinPath = input.Facts.Path.BinPath,
            NativeHomePath = home,
            NativeInstallDirectory = installDirectory,
            MainRoot = input.Facts.MainRoot,
            ClientRoots = input.Facts.ClientRoots,
            NativeEnvironment = input.Facts.NativeEnvironment,
        };
    }

    private static IReadOnlyList<ToolNativeSettingRecord> NativeSettings(ToolWritePlanningInput input, string id)
        => [.. (input.Records.WorkspaceMachine?.NativeSettings ?? []).Where(setting => setting.ToolId == id)];
}
