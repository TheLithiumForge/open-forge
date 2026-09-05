using System.Collections.Immutable;
using System.Security.Cryptography;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Recovery;

internal sealed class RecoveryBundleTargetStateReader(
    PhysicalPathResolver physicalPathResolver)
{
    internal async ValueTask<RecoveryBundleComparison> ReadAsync(
        CliWorkspace workspace,
        RecoveryBundleVerifiedRead bundle,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(bundle);
        var targets = ImmutableArray.CreateBuilder<RecoveryBundleTargetComparison>(
            bundle.Entries.Length);
        foreach (var entry in bundle.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            targets.Add(await ReadTargetAsync(
                    workspace,
                    entry,
                    cancellationToken)
                .ConfigureAwait(false));
        }

        return RecoveryBundleComparison.Create(
            bundle.BundlePath,
            bundle.Attribution,
            targets.MoveToImmutable());
    }

    private async ValueTask<RecoveryBundleTargetComparison> ReadTargetAsync(
        CliWorkspace workspace,
        RecoveryBundleEntry entry,
        CancellationToken cancellationToken)
    {
        var lexicalPath = Path.GetFullPath(Path.Combine(
            workspace.LexicalRoot,
            entry.TargetPath.Replace('/', Path.DirectorySeparatorChar)));
        var resolution = physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            lexicalPath);
        if (resolution.State == PhysicalPathState.Missing)
        {
            return entry.Intended is null
                ? RecoveryBundleTargetComparison.Intended(entry.TargetPath, observed: null)
                : RecoveryBundleTargetComparison.Third(entry.TargetPath, observed: null);
        }

        if (resolution.State != PhysicalPathState.Contained)
        {
            return RecoveryBundleTargetComparison.Classified(
                entry.TargetPath,
                resolution.State is PhysicalPathState.Inaccessible
                    or PhysicalPathState.InputOutputFailure
                    ? RecoveryBundleTargetComparisonState.Unavailable
                    : RecoveryBundleTargetComparisonState.Blocked,
                $"The recovery target physical state is {resolution.State}.");
        }

        var physicalPath = resolution.GetContainedPhysicalPath();
        var component = LinkTargetReader.Read(physicalPath);
        if (component.State == PathComponentState.Missing)
        {
            return entry.Intended is null
                ? RecoveryBundleTargetComparison.Intended(entry.TargetPath, observed: null)
                : RecoveryBundleTargetComparison.Third(entry.TargetPath, observed: null);
        }

        if (component.State != PathComponentState.Ordinary
            || component.Attributes is not { } attributes
            || (attributes & FileAttributes.Directory) != 0)
        {
            return RecoveryBundleTargetComparison.Classified(
                entry.TargetPath,
                component.State is PathComponentState.Inaccessible
                    or PathComponentState.InputOutputFailure
                    ? RecoveryBundleTargetComparisonState.Unavailable
                    : RecoveryBundleTargetComparisonState.Blocked,
                $"The recovery target component state is {component.State}.");
        }

        try
        {
            await using var stream = new FileStream(
                physicalPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                RecoveryBundleFormatV1.StreamBufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            var length = stream.Length;
            var hash = Convert.ToHexStringLower(
                await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false));
            var state = RecoveryBundleTargetComparisonState.Third;
            if (entry.Prior.Matches(hash, length))
            {
                state = RecoveryBundleTargetComparisonState.Prior;
            }
            else if (entry.Intended?.Matches(hash, length) == true)
            {
                state = RecoveryBundleTargetComparisonState.Intended;
            }
            var observed = RecoveryContentIdentity.Create(length, hash);
            return state switch
            {
                RecoveryBundleTargetComparisonState.Prior =>
                    RecoveryBundleTargetComparison.Prior(entry.TargetPath, observed),
                RecoveryBundleTargetComparisonState.Intended =>
                    RecoveryBundleTargetComparison.Intended(entry.TargetPath, observed),
                RecoveryBundleTargetComparisonState.Third =>
                    RecoveryBundleTargetComparison.Third(entry.TargetPath, observed),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(state),
                    state,
                    "The observed recovery target state is not defined."),
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
        {
            return RecoveryBundleTargetComparison.Classified(
                entry.TargetPath,
                RecoveryBundleTargetComparisonState.Unavailable,
                exception.Message);
        }
        catch (Exception exception) when (exception is ArgumentException
            or NotSupportedException
            or PlatformNotSupportedException
            or PathTooLongException)
        {
            return RecoveryBundleTargetComparison.Classified(
                entry.TargetPath,
                RecoveryBundleTargetComparisonState.Blocked,
                exception.Message);
        }
    }

}
