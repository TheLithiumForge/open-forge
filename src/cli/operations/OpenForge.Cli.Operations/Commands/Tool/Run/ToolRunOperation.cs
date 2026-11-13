using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Run.Models;
using OpenForge.Cli.Core.Commands.Tool.Run.Models.Resolution;
using OpenForge.Cli.Core.Commands.Tool.Run.Shared.Resolution;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.Core.Framework.Tools.Shared.Storage;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Run;

internal sealed class ToolRunOperation(ToolCatalogueReader? catalogue = null, ToolStorePaths? stores = null)
{
    private readonly ToolCatalogueReader _catalogue = catalogue ?? new();

    internal async ValueTask<ToolRunResolution> ResolveAsync(ToolRunRequest request, CancellationToken token)
    {
        CliWorkspace? workspace = null;
        ToolRunRefused Refuse(ToolRunFailureCode code, string cause)
            => new(new(code, request.Id, cause, false) { Workspace = workspace });
        try
        {
            token.ThrowIfCancellationRequested();
            workspace = ToolRunWorkspaceSelector.Select(request, token);
            if (workspace is null) return Refuse(ToolRunFailureCode.WorkspaceUnavailable, "Select an available Open Forge workspace with --workspace before the tool ID.");
            if (!ToolIdentity.IsToolId(request.Id) || request.Arguments.Any(argument => argument is null || argument.Contains('\0')))
                return Refuse(ToolRunFailureCode.InvalidInput, "Run requires a declared tool ID and separate arguments.");
            var paths = new PhysicalPathResolver();
            var settings = await WorkspaceSettingsReader.ReadAsync(paths, workspace, token).ConfigureAwait(false);
            var ownership = await WorkspaceOwnershipReader.ReadAsync(paths, workspace, token).ConfigureAwait(false);
            return await ResolveSelectedAsync(request, workspace, new(settings, ownership), token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { return Refuse(ToolRunFailureCode.Cancelled, "Tool Run was cancelled before launch."); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or ArgumentException or InvalidOperationException or NotSupportedException)
        { return Refuse(ToolRunFailureCode.VerificationUnavailable, exception.Message); }
    }

    // Workspace reports pass their existing control-file observation and validated settings IDs.
    internal async ValueTask<ToolRunResolution> ResolveSelectedAsync(ToolRunRequest request, CliWorkspace workspace, ToolRunWorkspaceRecords records, CancellationToken token)
    {
        ToolRunRefused Refuse(ToolRunFailureCode code, string cause)
            => new(new(code, request.Id, cause, false) { Workspace = workspace });
        try
        {
            token.ThrowIfCancellationRequested();
            var settings = records.Settings;
            var ownership = records.Ownership;
            if (settings.State is not (WorkspaceSettingsReadState.Complete or WorkspaceSettingsReadState.Absent))
                return Refuse(ToolRunFailureCode.VerificationUnavailable, settings.Cause ?? "Workspace settings aren't readable.");
            if (!settings.Document.Tools.TryGetValue(request.Id, out var setting))
                return Refuse(ToolRunFailureCode.ToolUndeclared, "This workspace doesn't declare that tool ID.");
            if (ownership.State != WorkspaceOwnershipReadState.Complete || !ownership.Document.Tools.TryGetValue(request.Id, out var receipt))
                return Refuse(ToolRunFailureCode.VerificationUnavailable, ownership.Cause ?? "The declared tool has no readable verification receipt.");
            var store = new ToolStoreRegistry(stores ?? ToolStoreLocator.ForCurrentUser());
            var registry = await store.ReadAsync(token).ConfigureAwait(false);
            var resolution = new ToolRunPinResolver(_catalogue, ToolNativeTarget.Current).Resolve(request, workspace, new(setting, receipt, registry.Document));
            if (resolution is not ToolRunReady ready) return resolution;
            if (!ToolRunIntegrity.Matches(ready.Dispatch.Launch, store, registry.Document, token))
                return Refuse(ToolRunFailureCode.LaunchIntegrityMismatch, "An immutable tool member is missing or its recorded size, UTC timestamp or executable mode changed.");
            token.ThrowIfCancellationRequested();
            return ready;
        }
        catch (OperationCanceledException) { return Refuse(ToolRunFailureCode.Cancelled, "Tool Run was cancelled before launch."); }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or ArgumentException or InvalidOperationException or NotSupportedException)
        { return Refuse(ToolRunFailureCode.VerificationUnavailable, exception.Message); }
    }
}
