using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Configuration;

internal sealed class InstallSetupResolver(PhysicalPathResolver paths, InstallSetupInteraction? interaction)
{
    private readonly InstallTargetReader _reader = new(paths);

    internal async ValueTask<InstallSetupResolution> ResolveAsync(InstallRequest request, CancellationToken token)
    {
        if (request.Setup is null && (!request.AllowsInteractiveConfirmation || interaction is null)) return new(null);
        var loader = await _reader.ReadAsync(request.Workspace, FrameworkPayloadAsset.LoaderPath, token).ConfigureAwait(false);
        token.ThrowIfCancellationRequested();
        if (loader.State is not (InstallTargetReadState.Missing or InstallTargetReadState.File))
            return Stopped(ReadTargetCode(loader), loader.Cause ?? "The loader is unsafe or unavailable for setup selection.");
        if (request.Setup is null && loader.State == InstallTargetReadState.File) return new(null);
        var ownership = await WorkspaceOwnershipReader.ReadAsync(paths, request.Workspace, token).ConfigureAwait(false);
        if (request.Setup?.Configure == true && ownership.State is not (WorkspaceOwnershipReadState.Absent or WorkspaceOwnershipReadState.Complete))
            return Stopped(ownership.State == WorkspaceOwnershipReadState.Unavailable ? InstallFindingCode.LifecycleUnavailable : InstallFindingCode.LifecycleBlocked, ownership.Cause ?? "Ownership is unavailable for setup selection.");
        var installed = loader.State == InstallTargetReadState.File || ownership.Document.Framework is not null;
        if (request.Setup is null && installed) return new(null);
        if (request.Setup is { Preset: not null, Configure: false } && installed)
            return Stopped(InstallFindingCode.InvalidInput, "An explicit preset on an installed workspace requires --configure.");

        var settings = await WorkspaceSettingsReader.ReadAsync(paths, request.Workspace, token).ConfigureAwait(false);
        if (settings.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete))
            return Stopped(settings.State == WorkspaceSettingsReadState.Invalid ? InstallFindingCode.InvalidInput : InstallFindingCode.LifecycleUnavailable, settings.Cause ?? "Workspace settings are unavailable for setup selection.");
        var ignore = await _reader.ReadAsync(request.Workspace, InstallIgnoreSection.Path, token).ConfigureAwait(false);
        if (ignore.Snapshot is not { } ignoreSnapshot)
            return Stopped(ReadTargetCode(ignore), ignore.Cause ?? "The Git-ignore file is unsafe or unavailable.");
        ImmutableArray<string> ignored;
        try { ignored = InstallIgnoreSection.Read(ignoreSnapshot.Bytes.AsSpan()); }
        catch (Exception exception) when (exception is InvalidDataException or DecoderFallbackException)
        { return Stopped(InstallFindingCode.TargetUnsafe, exception.Message); }

        var canPrompt = request.AllowsInteractiveConfirmation && interaction is not null;
        var preset = request.Setup?.Preset;
        if (preset is null && canPrompt && interaction is { } presetInteraction)
        {
            var reply = await presetInteraction.Preset(installed ? InstallPreset.Custom : InstallPreset.Essentials, new(true), token).ConfigureAwait(false);
            if (reply.State == CliPromptState.Cancelled) return new(null, Cancelled: true);
            if (reply.State == CliPromptState.Answered) preset = reply.Value;
            else canPrompt = false;
        }
        if (preset is null)
        {
            if (request.Setup?.Configure == true) return Stopped(InstallFindingCode.InvalidInput, "Noninteractive --configure requires an explicit --preset.");
            return new(null);
        }

        var rows = InstallConfigurationChoices.Defaults(preset.Value).ToDictionary(row => row.Id, row => row.Action, StringComparer.Ordinal);
        if (preset == InstallPreset.Custom && installed)
        {
            foreach (var id in InstallConfigurationChoices.RouteIds)
            {
                rows[id] = WorkspaceRemovals.IsPathRemoved(InstallConfigurationChoices.Entrypoint(id), settings.Document)
                    ? InstallRouteAction.Remove : InstallRouteAction.Add;
                if (rows[id] != InstallRouteAction.Remove && ignored.Contains(id, StringComparer.Ordinal)) rows[id] = InstallRouteAction.GitIgnore;
            }
        }
        var overrides = request.Setup?.Overrides ?? [];
        foreach (var row in overrides)
        {
            _ = InstallConfigurationChoices.Name(row.Action);
            rows[row.Id] = row.Action;
        }
        if (preset == InstallPreset.Custom && canPrompt && interaction is { } prompts)
        {
            var fixedIds = overrides.Select(row => row.Id).ToImmutableArray();
            while (true)
            {
                var question = new InstallRouteQuestion(Materialize(rows), fixedIds);
                var picked = await prompts.Route(question, new(true), token).ConfigureAwait(false);
                if (picked.State != CliPromptState.Answered) return new(null, Cancelled: true);
                if (picked.Value.Length == 0) break;
                if (fixedIds.Contains(picked.Value, StringComparer.Ordinal)) continue;
                if (!rows.TryGetValue(picked.Value, out var current))
                    throw new InvalidOperationException("Setup selected an unavailable Custom row.");
                var answer = await prompts.Action(new(picked.Value, current), new(true), token).ConfigureAwait(false);
                if (answer.State != CliPromptState.Answered) return new(null, Cancelled: true);
                _ = InstallConfigurationChoices.Name(answer.Value);
                rows[picked.Value] = answer.Value;
            }
        }
        return new(new(request.Setup?.Configure == true, preset.Value, Materialize(rows)));
    }

    private static InstallSetupResolution Stopped(InstallFindingCode code, string cause) => new(null, new(code, cause));

    private static InstallFindingCode ReadTargetCode(InstallTargetRead read)
        => read.State == InstallTargetReadState.Unavailable ? InstallFindingCode.LifecycleUnavailable : InstallFindingCode.TargetUnsafe;

    private static ImmutableArray<InstallRouteSelection> Materialize(Dictionary<string, InstallRouteAction> rows)
        => InstallConfigurationChoices.RouteIds.Select(id => new InstallRouteSelection(id, rows[id])).ToImmutableArray();
}
