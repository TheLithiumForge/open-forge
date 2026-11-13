using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.Core.Framework.Tools.Shared.Records;
using OpenForge.Cli.Core.Framework.Tools.Shared.Storage;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing;

internal static class ToolWriterReader
{
    internal static async ValueTask<ToolWriterState> ReadAsync(CliWorkspace workspace, CancellationToken token)
    {
        var paths = new PhysicalPathResolver();
        var settings = await WorkspaceSettingsReader.ReadAsync(paths, workspace, token).ConfigureAwait(false);
        RequireSettings(settings);
        var ownership = await WorkspaceOwnershipReader.ReadAsync(paths, workspace, token).ConfigureAwait(false);
        if (ownership.State is not WorkspaceOwnershipReadState.Absent and not WorkspaceOwnershipReadState.Complete)
            throw Failure(ToolWriteIssueCode.LockMismatch, CliSemanticStatus.Incomplete, ownership.LogicalPath, ownership.Cause ?? "The tool lock cannot be read.");
        if (settings.Snapshot is null || ownership.Snapshot is null)
            throw Failure(ToolWriteIssueCode.LockMismatch, CliSemanticStatus.Incomplete, ownership.LogicalPath, "The control records have no complete byte observations.");
        return new(settings, ownership, WorkspaceLayout.Create(workspace, settings.Document.MainRoot));
    }

    internal static void RequireSettings(WorkspaceSettingsRead settings)
    {
        if (settings.State is not WorkspaceSettingsReadState.Absent and not WorkspaceSettingsReadState.Complete)
            throw Failure(ToolWriteIssueCode.LockMismatch, CliSemanticStatus.Incomplete, settings.LogicalPath, settings.Cause ?? "Workspace declarations cannot be read.");
    }

    internal static void RequireConsistent(ToolWriterState state)
    {
        try { ToolRecordConsistency.Validate(state.Settings.Document, state.Lock.Document); }
        catch (JsonException exception)
        { throw Failure(ToolWriteIssueCode.LockMismatch, CliSemanticStatus.Incomplete, state.Lock.LogicalPath, exception.Message); }
    }

    internal static ToolDefinition Stored(ToolWriterCatalogue catalogue, ToolReceipt receipt)
    {
        var definition = catalogue.Reader.Definition(receipt.ManifestBytes, "receipt." + receipt.ManifestFormat, false);
        if (!definition.Reviewed)
            throw Failure(ToolWriteIssueCode.PinNotSupported, CliSemanticStatus.Blocked, receipt.Id, definition.Blocker ?? "The recorded pin isn't supported by this release.");
        if (definition.Sha256 != receipt.ManifestSha256 || definition.Manifest.Id != receipt.Id || definition.Manifest.Version != receipt.Version)
            throw Failure(ToolWriteIssueCode.LockMismatch, CliSemanticStatus.Incomplete, receipt.Id, "The recorded definition differs from its tool receipt.");
        return definition;
    }

    internal static async ValueTask<ToolWriterStoreRead> StoreAsync(ToolStoreRegistry registry, ToolDefinition definition, string target, CancellationToken token)
    {
        try
        {
            var read = await registry.ReadAsync(token).ConfigureAwait(false);
            var path = ToolStoreIdentity.InstallPath(definition.Manifest, target);
            var key = ToolStoreIdentity.UnitKey(ToolStoreRegistryDefinitions.Install, path);
            read.Document.Units.TryGetValue(key, out var install);
            var verified = false;
            if (install is not null)
            {
                if (install.RecipeSha256 != definition.Sha256)
                    throw new IOException("The existing store unit belongs to another reviewed recipe.");
                var actual = ToolStoreUnitReader.Members(registry.Installs, install.Path, token);
                var expected = install.Files.Select(file => file.Path).Concat(install.Directories).ToHashSet(StringComparer.Ordinal);
                if (actual.Any(member => !expected.Contains(member)))
                    throw new IOException("The install contains unrecognized members.");
                verified = (await ToolStoreUnitReader.ReadAsync(registry.Installs, install, token).ConfigureAwait(false)).Verified;
            }
            return new(read, key, install, verified);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        { throw Failure(ToolWriteIssueCode.StoreUnavailable, CliSemanticStatus.Incomplete, definition.Manifest.Id, exception.Message); }
    }

    internal static ToolWriterFailure Failure(ToolWriteIssueCode code, CliSemanticStatus status, string subject, string cause)
        => new(new(code, status, subject, cause));
}
