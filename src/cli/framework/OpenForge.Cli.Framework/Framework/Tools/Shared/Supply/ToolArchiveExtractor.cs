using System.Formats.Tar;
using System.IO.Compression;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply.Models;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

internal static class ToolArchiveExtractor
{
    private const int BufferSize = 81920;
    private const int UnixTypeMask = 0xf000;
    private const int UnixRegular = 0x8000;
    private const int UnixDirectory = 0x4000;
    private const int UnixModeShift = 16;
    private const int PermissionsMask = 0x1ff;
    internal const int MaximumMembers = 100000;
    internal const long MaximumExpandedBytes = 4L * 1024 * 1024 * 1024;

    internal static async ValueTask<ToolArchivePlan> ReadAsync(ToolExtractionRequest request, CancellationToken token)
    {
        if (request.Preparation != ToolArchivePreparation.Original || request.Asset.StripComponents < 0)
            throw new InvalidDataException("Prepare archive links before extraction.");
        await ToolArtifactIntegrity.VerifyAsync(request.ArchivePath, request.Asset, token).ConfigureAwait(false);
        var members = new Dictionary<string, ToolArchiveMember>(StringComparer.Ordinal);
        await using var file = new FileStream(request.ArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        switch (request.Asset.Archive)
        {
            case ToolArchiveKind.File:
                var name = request.Asset.EntryRelativePath;
                Add(new(name, MemberPath(name, 0, directory: false) ?? throw new InvalidDataException("The artifact file path is missing."),
                    file.Length, false, UnixFileMode.UserRead | UnixFileMode.UserWrite));
                break;
            case ToolArchiveKind.Zip:
                using (var zip = new ZipArchive(file, ZipArchiveMode.Read, leaveOpen: true))
                    foreach (var entry in zip.Entries)
                    {
                        token.ThrowIfCancellationRequested();
                        var directory = entry.FullName.EndsWith('/');
                        var type = entry.ExternalAttributes >> UnixModeShift & UnixTypeMask;
                        if (type != 0 && type != (directory ? UnixDirectory : UnixRegular)
                            || (entry.ExternalAttributes & (int)(FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
                            throw new InvalidDataException("Archive links and special members aren't allowed.");
                        if (MemberPath(entry.FullName, request.Asset.StripComponents, directory) is { } path)
                            Add(new(entry.FullName, path, entry.Length, directory, (UnixFileMode)(entry.ExternalAttributes >> UnixModeShift & PermissionsMask)));
                    }
                break;
            case ToolArchiveKind.TarGzip:
                await using (var gzip = new GZipStream(file, CompressionMode.Decompress, leaveOpen: true))
                await using (var tar = new TarReader(gzip, leaveOpen: true))
                    for (var entry = await tar.GetNextEntryAsync(copyData: false, token).ConfigureAwait(false); entry is not null;
                        entry = await tar.GetNextEntryAsync(copyData: false, token).ConfigureAwait(false))
                    {
                        token.ThrowIfCancellationRequested();
                        var directory = entry.EntryType == TarEntryType.Directory;
                        if (!directory && entry.EntryType is not TarEntryType.RegularFile and not TarEntryType.V7RegularFile)
                            throw new InvalidDataException("Archive links and special members aren't allowed.");
                        if (MemberPath(entry.Name, request.Asset.StripComponents, directory) is { } path)
                            Add(new(entry.Name, path, entry.Length, directory, entry.Mode & (UnixFileMode)PermissionsMask));
                    }
                break;
            default: throw new ArgumentOutOfRangeException(nameof(request));
        }
        long expanded = 0;
        foreach (var member in members.Values)
        {
            for (var parent = Parent(member.Path); parent is not null; parent = Parent(parent))
                if (members.TryGetValue(PortableWorkspacePath.CreatePortableKey(parent), out var ancestor) && !ancestor.Directory)
                    throw new InvalidDataException("An archive file is also another member's parent.");
            if (member.Bytes > MaximumExpandedBytes - expanded) throw new InvalidDataException("The expanded archive exceeds its size limit.");
            expanded += member.Bytes;
        }
        return new(members.Values.ToArray(), expanded);

        void Add(ToolArchiveMember member)
        {
            if (member.Bytes < 0 || members.Count == MaximumMembers || !members.TryAdd(PortableWorkspacePath.CreatePortableKey(member.Path), member))
                throw new InvalidDataException("An archive has duplicate destinations or exceeds its member limit.");
        }
    }

    internal static async ValueTask<ToolExtractionResult> ExtractAsync(ToolExtractionRequest request, CancellationToken token)
    {
        await using var archive = new FileStream(request.ArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var plan = await ReadAsync(request, token).ConfigureAwait(false);
        var root = System.IO.Path.GetFullPath(request.DestinationPath);
        if (!Directory.Exists(root) || new DirectoryInfo(root).Attributes.HasFlag(FileAttributes.ReparsePoint) || Directory.EnumerateFileSystemEntries(root).Any())
            throw new IOException("Extraction requires an empty ordinary private directory.");
        var physical = new PhysicalPathResolver().ResolveRoot(root).GetContainedPhysicalPath();
        var drive = new DriveInfo(System.IO.Path.GetPathRoot(physical) ?? throw new IOException("The extraction volume isn't available."));
        if (drive.AvailableFreeSpace < plan.ExpandedBytes) throw new IOException("The extraction volume has insufficient space.");
        var directories = new HashSet<string>(StringComparer.Ordinal);
        foreach (var member in plan.Members)
            for (var directory = member.Directory ? member.Path : Parent(member.Path); directory is not null; directory = Parent(directory)) directories.Add(directory);
        foreach (var directory in directories.OrderBy(path => path.Count(character => character == '/')).ThenBy(path => path, StringComparer.Ordinal))
        {
            token.ThrowIfCancellationRequested();
            var target = Contained(root, physical, directory);
            Directory.CreateDirectory(target);
        }
        var expected = plan.Members.Where(member => !member.Directory).ToDictionary(member => member.Name, StringComparer.Ordinal);
        var written = new List<string>();
        if (request.Asset.Archive == ToolArchiveKind.File)
            await WriteAsync(expected[request.Asset.EntryRelativePath], archive).ConfigureAwait(false);
        else if (request.Asset.Archive == ToolArchiveKind.Zip)
        {
            using var zip = new ZipArchive(archive, ZipArchiveMode.Read, leaveOpen: true);
            foreach (var entry in zip.Entries)
                if (expected.Remove(entry.FullName, out var member))
                {
                    await using var input = entry.Open();
                    await WriteAsync(member, input).ConfigureAwait(false);
                }
            if (expected.Count != 0) throw new InvalidDataException("The archive members changed before extraction.");
        }
        else
        {
            await using var gzip = new GZipStream(archive, CompressionMode.Decompress, leaveOpen: true);
            await using var tar = new TarReader(gzip, leaveOpen: true);
            for (var entry = await tar.GetNextEntryAsync(copyData: false, token).ConfigureAwait(false); entry is not null;
                entry = await tar.GetNextEntryAsync(copyData: false, token).ConfigureAwait(false))
                if (expected.Remove(entry.Name, out var member))
                    await WriteAsync(member, entry.DataStream ?? throw new InvalidDataException("An archive file has no data stream.")).ConfigureAwait(false);
            if (expected.Count != 0) throw new InvalidDataException("The archive members changed before extraction.");
        }
        return new(written, null);

        async ValueTask WriteAsync(ToolArchiveMember member, Stream input)
        {
            var target = Contained(root, physical, member.Path);
            await using (var output = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                var buffer = new byte[BufferSize];
                long copied = 0;
                for (var count = await input.ReadAsync(buffer, token).ConfigureAwait(false); count != 0; count = await input.ReadAsync(buffer, token).ConfigureAwait(false))
                {
                    if (count > member.Bytes - copied) throw new InvalidDataException("An archive file exceeds its declared length.");
                    await output.WriteAsync(buffer.AsMemory(0, count), token).ConfigureAwait(false);
                    copied += count;
                }
                if (output.Length != member.Bytes) throw new InvalidDataException("An extracted file differs from its declared length.");
            }
            if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(target, member.Mode | UnixFileMode.UserRead | UnixFileMode.UserWrite);
            written.Add(target);
        }
    }

    private static string Contained(string root, string physical, string relative)
    {
        var target = System.IO.Path.Combine(root, relative.Replace('/', System.IO.Path.DirectorySeparatorChar));
        var result = new PhysicalPathResolver().ResolveCandidate(root, physical, target);
        if (result.State is not (PhysicalPathState.Missing or PhysicalPathState.Contained)) throw new IOException("An extraction path leaves its directory.");
        return target;
    }

    private static string? MemberPath(string name, int strip, bool directory)
    {
        var raw = directory ? name.TrimEnd('/') : name;
        while (raw.StartsWith("./", StringComparison.Ordinal)) raw = raw[2..];
        if (directory && raw == ".") return null;
        if (!PortableWorkspacePath.TryNormalize(raw, out _)) throw new InvalidDataException("An archive member has an unsafe path.");
        var segments = raw.Split('/');
        if (segments.Length > strip) return string.Join('/', segments.Skip(strip));
        if (directory) return null;
        throw new InvalidDataException("Stripping archive components removes a file name.");
    }

    private static string? Parent(string path)
        => path.LastIndexOf('/') is var separator && separator >= 0 ? path[..separator] : null;
}
