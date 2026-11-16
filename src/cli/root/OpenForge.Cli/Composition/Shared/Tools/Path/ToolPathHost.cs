using System.Text;
using System.Security.Cryptography;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Paths;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

namespace OpenForge.Cli.Composition.Shared.Tools.Path;

internal sealed class ToolPathHost(ToolPathEnvironment environment, string shellHome, IToolBackupService backups, Func<bool> hasLiveConsent) : IToolPathHost
{
    private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    public Task<ToolPathFacts> ObserveAsync(string homePath, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var bin = System.IO.Path.Combine(homePath, ToolRecordDefinitions.BinDirectoryName);
        IReadOnlyList<ToolShellFile> files = [];
        if (!environment.IsWindows)
            files = ToolPathPolicy.StartupFiles.Select(file => file with { AbsolutePath = System.IO.Path.Combine(shellHome, file.AbsolutePath.Replace('/', System.IO.Path.DirectorySeparatorChar)) })
                .Where(file => File.Exists(file.AbsolutePath)).ToArray();
        return Task.FromResult(new ToolPathFacts
        {
            BinPath = bin,
            Readiness = Readiness(bin),
            DetectedShellFiles = files,
        });
    }

    private ToolPathReadiness Readiness(string bin)
    {
        if (environment.ProcessPath is null) return ToolPathReadiness.Unavailable;
        return ToolPathPolicy.Contains(environment.ProcessPath, bin, environment.IsWindows) ? ToolPathReadiness.Ready : ToolPathReadiness.Missing;
    }

    public async Task<ToolPathSetupResult> AddAsync(ToolPathSetupRequest request, CancellationToken cancellationToken)
    {
        var pending = new List<ToolBackupCompletionRequest>();
        ToolPathSetupResult Result(ToolPathState? state, IReadOnlyList<string> lines, bool restart, string? failure)
            => new(state, lines, restart, failure) { PendingBackups = pending };
        var bin = request.Facts.BinPath;
        if (!System.IO.Path.IsPathFullyQualified(bin)) return Result(null, [], false, "PATH needs an absolute bin path.");
        if (request.Previous is { } previous && previous.BinPath != bin) return Result(previous, ManualLines(bin), false, "PATH history names a different bin.");
        if (request.Choice == ToolPathChoice.Manual || !hasLiveConsent()) return Result(request.Previous, ManualLines(bin), false, null);
        if (request.Choice != ToolPathChoice.AddForMe) throw new ArgumentOutOfRangeException(nameof(request));
        var changes = request.Previous?.Changes.ToList() ?? [];
        try
        {
            if (environment.IsWindows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                // An unreadable or unsafe stored value is never rewritten. The user gets the manual steps instead.
                if (environment.ReadUserPath() is not { } prior || ToolWindowsUserPath.Append(prior.Text, bin) is not { } applied)
                    return Result(State(bin, changes), ManualLines(bin), false, "The user PATH couldn't be read safely, so it was left unchanged.");
                if (applied != prior.Text)
                {
                    if (environment.ReadUserPath() != prior) return Result(State(bin, changes), ManualLines(bin), false, "User PATH changed before replacement.");
                    var written = prior with { Text = applied, Present = true };
                    if (!environment.WriteUserPath(written)) return Result(State(bin, changes), ManualLines(bin), false, "The user PATH couldn't be written, so it was left unchanged.");
                    changes.Add(new()
                    {
                        Kind = ToolPathChangeKind.WindowsUserPath,
                        Target = ToolPathPolicy.PathVariable,
                        Prior = new() { Present = false, Kind = ToolRecordedValueKind.Native },
                        Applied = new() { Present = true, Kind = ToolRecordedValueKind.Native, Native = bin },
                    });
                    if (environment.ReadUserPath() != written) return Result(State(bin, changes), ManualLines(bin), true, "User PATH didn't verify after replacement.");
                    environment.AnnounceEnvironmentChange();
                }
            }
            else
            {
                foreach (var selected in request.ChosenShellFiles.DistinctBy(file => file.AbsolutePath, StringComparer.Ordinal))
                {
                    var before = await ReadAsync(selected.AbsolutePath, cancellationToken).ConfigureAwait(false);
                    var text = before is null ? "" : Utf8.GetString(before);
                    var block = ToolPathPolicy.Block(bin, selected.Shell);
                    if (text.Contains(ToolPathPolicy.StartMarker, StringComparison.Ordinal) || text.Contains(ToolPathPolicy.EndMarker, StringComparison.Ordinal))
                    {
                        if (!TrySpan(text, out var start, out var length) || text.Substring(start, length) != block)
                            return Result(State(bin, changes), ManualLines(bin), changes.Count > 0, "The shell PATH block conflicts with the chosen entry.");
                        if (!changes.Any(change => change.Kind == ToolPathChangeKind.ShellBlock && change.Target == selected.AbsolutePath))
                            changes.Add(new()
                            {
                                Kind = ToolPathChangeKind.ShellBlock,
                                Target = selected.AbsolutePath,
                                StartMarker = ToolPathPolicy.StartMarker,
                                EndMarker = ToolPathPolicy.EndMarker,
                                Prior = new() { Present = false, Kind = ToolRecordedValueKind.Bytes },
                                Applied = new() { Present = true, Kind = ToolRecordedValueKind.Bytes, Bytes = Utf8.GetBytes(block) },
                            });
                        continue;
                    }
                    var separator = text.Length > 0 && !text.EndsWith('\n') ? "\n" : "";
                    var applied = Utf8.GetBytes($"{text}{separator}{block}");
                    await ReplaceAsync(selected.AbsolutePath, before, applied, pending, cancellationToken).ConfigureAwait(false);
                    changes.Add(new()
                    {
                        Kind = ToolPathChangeKind.ShellBlock,
                        Target = selected.AbsolutePath,
                        StartMarker = ToolPathPolicy.StartMarker,
                        EndMarker = ToolPathPolicy.EndMarker,
                        Prior = new() { Present = false, Kind = ToolRecordedValueKind.Bytes },
                        Applied = new() { Present = true, Kind = ToolRecordedValueKind.Bytes, Bytes = Utf8.GetBytes($"{separator}{block}") },
                    });
                }
            }
            return Result(State(bin, changes), [], changes.Count > 0, null);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or OperationCanceledException)
        {
            return Result(State(bin, changes), ManualLines(bin), changes.Count > 0, "PATH setup failed. Keep file backups for checking.");
        }
    }

    private IReadOnlyList<string> ManualLines(string bin)
        => environment.IsWindows ? [$"Add {bin} to the user PATH."]
            : [ToolPathPolicy.ManualLine(bin, ToolShellKind.Posix), ToolPathPolicy.ManualLine(bin, ToolShellKind.Fish)];

    private static ToolPathState State(string bin, IReadOnlyList<ToolPathChangeRecord> changes)
        => new() { SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion, BinPath = bin, Changes = changes };

    private static bool TrySpan(string text, out int start, out int length)
    {
        start = text.IndexOf(ToolPathPolicy.StartMarker, StringComparison.Ordinal);
        var end = text.IndexOf(ToolPathPolicy.EndMarker, StringComparison.Ordinal);
        length = 0;
        if (start < 0 || end < start || start > 0 && text[start - 1] != '\n'
            || text.IndexOf(ToolPathPolicy.StartMarker, start + 1, StringComparison.Ordinal) >= 0
            || text.IndexOf(ToolPathPolicy.EndMarker, end + 1, StringComparison.Ordinal) >= 0) return false;
        var tail = end + ToolPathPolicy.EndMarker.Length;
        if (tail < text.Length && text[tail] == '\n') tail++;
        else if (tail < text.Length) return false;
        length = tail - start;
        return true;
    }

    private static async Task<byte[]?> ReadAsync(string path, CancellationToken token)
    {
        if (!System.IO.Path.IsPathFullyQualified(path)) throw new IOException("A chosen shell file needs an absolute path.");
        for (var current = new FileInfo(path) as FileSystemInfo; current is not null; current = Directory.GetParent(current.FullName))
            if (current.LinkTarget is not null || current.Exists && current.Attributes.HasFlag(FileAttributes.ReparsePoint)) throw new IOException("A chosen shell path is a link.");
        return File.Exists(path) ? await File.ReadAllBytesAsync(path, token).ConfigureAwait(false) : null;
    }

    private async Task ReplaceAsync(string path, byte[]? expected, byte[] applied, List<ToolBackupCompletionRequest> pending, CancellationToken token)
    {
        var backup = await backups.PrepareAsync(new(path), token).ConfigureAwait(false);
        if (backup.Failure is not null) throw new IOException(backup.Failure);
        var hash = Convert.ToHexStringLower(SHA256.HashData(applied));
        var backupIndex = pending.Count;
        pending.Add(new(backup, hash, Verified: false));
        var parent = System.IO.Path.GetDirectoryName(path) ?? throw new IOException("The chosen shell file has no parent.");
        Directory.CreateDirectory(parent);
        var temporary = System.IO.Path.Combine(parent, $".path-{Guid.NewGuid():N}");
        await File.WriteAllBytesAsync(temporary, applied, token).ConfigureAwait(false);
        if (!OperatingSystem.IsWindows() && expected is not null) File.SetUnixFileMode(temporary, File.GetUnixFileMode(path));
        var current = await ReadAsync(path, token).ConfigureAwait(false);
        if (current is null != (expected is null) || current is not null && expected is not null && !current.AsSpan().SequenceEqual(expected))
            throw new IOException("The chosen shell file changed before replacement.");
        if (expected is null) File.Move(temporary, path);
        else File.Replace(temporary, path, destinationBackupFileName: null);
        if (!(await File.ReadAllBytesAsync(path, token).ConfigureAwait(false)).AsSpan().SequenceEqual(applied)) throw new IOException("The chosen shell file didn't verify.");
        if (await ToolArtifactIntegrity.FileHashAsync(path, token).ConfigureAwait(false) != hash) throw new IOException("The chosen shell file changed after replacement.");
        pending[backupIndex] = new(backup, hash, Verified: true);
    }
}
