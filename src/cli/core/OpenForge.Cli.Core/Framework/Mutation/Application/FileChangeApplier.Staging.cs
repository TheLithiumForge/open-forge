using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Framework.Mutation.Application;

internal sealed partial class FileChangeApplier
{
    private const int StageBufferSize = 4096;

    private async ValueTask<FileChangeReceipt> ApplyWriteAsync(
        ApplicationContext context,
        CancellationToken cancellationToken)
    {
        var targetPath = context.Check.PhysicalPath
            ?? throw new InvalidOperationException("A matched file check requires a physical target.");
        var stagePath = string.Empty;
        var stageCreated = false;
        try
        {
            stagePath = CreateStagePath(targetPath);
            await using (var stream = OpenStage(stagePath))
            {
                stageCreated = true;
                await stream.WriteAsync(
                    context.Change.IntendedBytes.AsMemory(),
                    cancellationToken).ConfigureAwait(false);
            }

            var stagedBytes = await File.ReadAllBytesAsync(
                stagePath,
                cancellationToken).ConfigureAwait(false);
            if (!stagedBytes.AsSpan().SequenceEqual(context.Change.IntendedBytes.AsSpan()))
            {
                return FileChangeReceipt.NotStarted(
                    context.Change,
                    context.Before,
                    FileChangeNotStartedReason.ApplicationFailed,
                    "The staged bytes did not match the intended file bytes.");
            }

            var revalidation = await RevalidateBeforeEffectAsync(
                context,
                cancellationToken).ConfigureAwait(false);
            if (revalidation is not null)
            {
                return revalidation;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return FileChangeReceipt.NotStarted(
                    context.Change,
                    context.Before,
                    FileChangeNotStartedReason.Cancelled,
                    CancellationBeforeEffectCause);
            }

            var overwrite = context.Change.Kind is PlannedFileChangeKind.Replace
                or PlannedFileChangeKind.ReplaceGeneratedRegion;
            try
            {
                File.Move(stagePath, targetPath, overwrite: overwrite);
                stageCreated = false;
            }
            catch (Exception exception) when (IsFilesystemException(exception))
            {
                return await ResolveEffectFailureAsync(
                    context,
                    targetPath,
                    exception).ConfigureAwait(false);
            }

            return await VerifyAsync(
                context,
                targetPath).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return FileChangeReceipt.NotStarted(
                context.Change,
                context.Before,
                FileChangeNotStartedReason.Cancelled,
                CancellationBeforeEffectCause);
        }
        catch (Exception exception) when (IsFilesystemException(exception))
        {
            return FileChangeReceipt.NotStarted(
                context.Change,
                context.Before,
                FileChangeNotStartedReason.ApplicationFailed,
                FilesystemFailure.FromException(
                    FailureKind(exception),
                    exception).DirectCause);
        }
        finally
        {
            if (stageCreated)
            {
                CleanupStage(stagePath);
            }
        }
    }

    private static string CreateStagePath(string targetPath)
    {
        var directory = Path.GetDirectoryName(targetPath)
            ?? throw new InvalidOperationException("A file target requires an adjacent directory.");
        return Path.Combine(
            directory,
            $".open-forge-stage-{Path.GetRandomFileName()}");
    }

    private static FileStream OpenStage(string stagePath)
    {
        return new FileStream(
            stagePath,
            new FileStreamOptions
            {
                Mode = FileMode.CreateNew,
                Access = FileAccess.Write,
                Share = FileShare.None,
                BufferSize = StageBufferSize,
                Options = FileOptions.SequentialScan,
            });
    }

    private static void CleanupStage(string stagePath)
    {
        try
        {
            if (!File.Exists(stagePath))
            {
                return;
            }

            var attributes = File.GetAttributes(stagePath);
            if ((attributes & (FileAttributes.Directory | FileAttributes.Device | FileAttributes.ReparsePoint)) != 0)
            {
                return;
            }

            File.Delete(stagePath);
        }
        catch (Exception exception) when (IsFilesystemException(exception))
        {
        }
    }
}
