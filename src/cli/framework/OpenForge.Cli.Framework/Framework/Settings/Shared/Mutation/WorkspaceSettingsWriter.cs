using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;

/// <summary>Authors explicitly requested allow-list entries; content mutations never call this implicitly.</summary>
internal static class WorkspaceSettingsWriter
{
    internal static async ValueTask<WorkspaceSettingsWrite> AddAllowInstallPathsAsync(
        PhysicalPathResolver resolver,
        CliWorkspace workspace,
        ImmutableArray<string> paths,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(workspace);
        cancellationToken.ThrowIfCancellationRequested();
        var relative = WorkspaceSettingsDefinitions.RelativePath.Replace('/', Path.DirectorySeparatorChar);
        var logicalPath = Path.Combine(workspace.LexicalRoot, relative);
        var physicalPath = Path.Combine(workspace.PhysicalRoot, relative);
        if (paths.IsDefaultOrEmpty)
        {
            return new(WorkspaceSettingsWriteState.AlreadyAdmitted, [], logicalPath, Cause: null);
        }
        var observation = await WorkspaceSettingsReader.ReadAsync(resolver, workspace, cancellationToken).ConfigureAwait(false);
        if (observation.State is not (WorkspaceSettingsReadState.Absent or WorkspaceSettingsReadState.Complete)
            || observation.Snapshot is not { } snapshot)
        {
            return new(WorkspaceSettingsWriteState.Refused, [], logicalPath, observation.Cause);
        }
        try
        {
            ReadOnlyMemory<byte> current = snapshot.Bytes.ToArray();
            var added = ImmutableArray.CreateBuilder<string>();
            foreach (var path in paths.Distinct(StringComparer.Ordinal))
            {
                if (!string.IsNullOrWhiteSpace(path)
                    && WorkspaceSettingsCodec.AddAllowInstallPath(current, path) is { } updated)
                {
                    current = updated;
                    added.Add(path);
                }
            }
            if (added.Count == 0)
            {
                return new(WorkspaceSettingsWriteState.AlreadyAdmitted, [], logicalPath, Cause: null);
            }
            var parent = Path.GetDirectoryName(physicalPath)
                ?? throw new InvalidOperationException("The settings file requires its .agents parent.");
            Directory.CreateDirectory(parent);
            var confirmed = await WorkspaceSettingsReader.ReadAsync(resolver, workspace, cancellationToken).ConfigureAwait(false);
            if (!observation.MatchesObservation(confirmed))
            {
                return new(WorkspaceSettingsWriteState.Refused, [], logicalPath, "The authored settings changed before the grant was written.");
            }
            var temporaryPath = Path.Combine(parent, Path.GetRandomFileName());
            var ownsTemporary = false;
            try
            {
                await using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    ownsTemporary = true;
                    await stream.WriteAsync(current, cancellationToken).ConfigureAwait(false);
                }
                File.Move(temporaryPath, physicalPath, overwrite: true);
            }
            finally
            {
                if (ownsTemporary && File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
            return new(WorkspaceSettingsWriteState.Written, added.ToImmutable(), logicalPath, Cause: null);
        }
        catch (ArgumentException exception)
        {
            return new(WorkspaceSettingsWriteState.Refused, [], logicalPath, exception.Message);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
        {
            return new(WorkspaceSettingsWriteState.Refused, [], logicalPath, $"{WorkspaceSettingsDefinitions.RelativePath} could not be written.");
        }
    }
}
