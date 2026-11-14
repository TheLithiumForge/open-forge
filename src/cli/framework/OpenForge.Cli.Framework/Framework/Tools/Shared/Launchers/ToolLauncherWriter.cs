using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Launchers;

internal sealed class ToolLauncherWriter(IToolBackupService backups)
{
    internal static byte[] Render(ToolLauncherSpec launcher)
    {
        Validate(launcher);
        var arguments = new List<string>();
        var executable = launcher.EntryPath;
        if (launcher.TargetKind == ToolLauncherTargetKind.RuntimeEntry)
        {
            executable = launcher.RuntimePath ?? throw new ArgumentException("A runtime launcher needs its runtime.", nameof(launcher));
            arguments.Add(launcher.EntryPath);
        }
        arguments.AddRange(launcher.Arguments);
        var text = launcher.Kind switch
        {
            ToolLauncherKind.WindowsCmd => $"@echo off\nsetlocal DisableDelayedExpansion\n{WindowsArgument(executable)} {string.Join(' ', arguments.Select(WindowsArgument))} %*\nexit /b %errorlevel%\n",
            ToolLauncherKind.PosixSh => $"#!/bin/sh\nexec {ShellArgument(executable)} {string.Join(' ', arguments.Select(ShellArgument))} \"$@\"\n",
            ToolLauncherKind.NativeExecutable => throw new ArgumentException("A native binary is copied as bytes.", nameof(launcher)),
            _ => throw new ArgumentOutOfRangeException(nameof(launcher)),
        };
        return Encoding.UTF8.GetBytes(text);
    }

    internal async Task<ToolLauncherWriteResult> WriteAsync(ToolLauncherWriteRequest request, List<ToolBackupCompletionRequest> pendingBackups, CancellationToken token)
    {
        var written = new List<string>();
        try
        {
            foreach (var launcher in request.Launchers)
            {
                token.ThrowIfCancellationRequested();
                byte[] bytes;
                if (launcher.Kind == ToolLauncherKind.NativeExecutable)
                {
                    Validate(launcher);
                    bytes = await File.ReadAllBytesAsync(launcher.EntryPath, token).ConfigureAwait(false);
                }
                else bytes = Render(launcher);
                var path = System.IO.Path.GetFullPath(launcher.LauncherPath);
                var parent = System.IO.Path.GetDirectoryName(path) ?? throw new IOException("The launcher has no parent directory.");
                EnsureOrdinaryParents(parent);
                Directory.CreateDirectory(parent);
                EnsureOrdinaryParents(parent);
                if (File.Exists(path) && File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint)) throw new IOException("A launcher is a link.");
                var expected = File.Exists(path) ? await ToolArtifactIntegrity.FileHashAsync(path, token).ConfigureAwait(false) : null;
                var backup = await backups.PrepareAsync(new(path), token).ConfigureAwait(false);
                if (backup.Failure is not null) return new(written, backup.Failure);
                var hash = Convert.ToHexStringLower(SHA256.HashData(bytes));
                var backupIndex = pendingBackups.Count;
                pendingBackups.Add(new(backup, hash, Verified: false));
                var temporary = System.IO.Path.Combine(parent, $".launcher-{Guid.NewGuid():N}");
                await File.WriteAllBytesAsync(temporary, bytes, token).ConfigureAwait(false);
                if (!OperatingSystem.IsWindows())
                {
                    var mode = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute | UnixFileMode.GroupRead | UnixFileMode.GroupExecute
                        | UnixFileMode.OtherRead | UnixFileMode.OtherExecute;
                    if (File.Exists(path)) mode = File.GetUnixFileMode(path) | UnixFileMode.UserExecute;
                    File.SetUnixFileMode(temporary, mode);
                }
                EnsureOrdinaryParents(parent);
                if (File.Exists(path) && File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint)) throw new IOException("A launcher changed into a link.");
                var current = File.Exists(path) ? await ToolArtifactIntegrity.FileHashAsync(path, token).ConfigureAwait(false) : null;
                if (current != expected) return new(written, "The launcher changed before replacement. Keep its backup and temporary file.");
                if (expected is null) File.Move(temporary, path);
                else File.Replace(temporary, path, destinationBackupFileName: null);
                if (await ToolArtifactIntegrity.FileHashAsync(path, token).ConfigureAwait(false) != hash)
                    return new(written, "The launcher didn't verify after replacement. Keep its backup.");
                written.Add(path);
                pendingBackups[backupIndex] = new(backup, hash, Verified: true);
            }
            return new(written, null);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException or OperationCanceledException)
        {
            return new(written, "Launcher publication failed. Keep backups and temporary files for checking.");
        }
    }

    internal static string ShellArgument(string value)
    {
        if (value.Contains('\0')) throw new ArgumentException("A shell argument contains a null character.", nameof(value));
        return $"'{value.Replace("'", "'\"'\"'", StringComparison.Ordinal)}'";
    }

    private static void Validate(ToolLauncherSpec launcher)
    {
        ArgumentNullException.ThrowIfNull(launcher);
        if (!System.IO.Path.IsPathFullyQualified(launcher.LauncherPath) || !System.IO.Path.IsPathFullyQualified(launcher.EntryPath)
            || launcher.RuntimePath is { } runtime && !System.IO.Path.IsPathFullyQualified(runtime))
            throw new ArgumentException("A launcher needs absolute paths.", nameof(launcher));
        var supported = launcher.TargetKind switch
        {
            ToolLauncherTargetKind.RuntimeEntry => launcher.RuntimePath is not null && launcher.Kind != ToolLauncherKind.NativeExecutable,
            ToolLauncherTargetKind.ReleaseLauncher => launcher.Kind != ToolLauncherKind.NativeExecutable,
            ToolLauncherTargetKind.NativeBinary => true,
            _ => throw new ArgumentOutOfRangeException(nameof(launcher)),
        };
        if (!supported) throw new ArgumentException("The launcher target doesn't match its runtime or binary kind.", nameof(launcher));
    }

    internal static string WindowsArgument(string value)
    {
        if (value.Any(character => character is '\0' or '\r' or '\n' or '"'))
            throw new ArgumentException("A cmd launcher argument contains an unsupported character.", nameof(value));
        var escaped = value.Replace("%", "%%", StringComparison.Ordinal);
        var trailing = escaped.Length - escaped.TrimEnd('\\').Length;
        return $"\"{escaped}{new string('\\', trailing)}\"";
    }

    private static void EnsureOrdinaryParents(string parent)
    {
        for (var current = new DirectoryInfo(parent); current is not null; current = current.Parent)
            if (current.Exists && current.Attributes.HasFlag(FileAttributes.ReparsePoint)) throw new IOException("A launcher directory is a link.");
    }
}
