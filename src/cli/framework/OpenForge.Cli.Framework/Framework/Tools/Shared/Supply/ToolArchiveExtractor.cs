using System.Buffers;
using System.Formats.Tar;
using System.IO.Compression;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Tools.Models.Receipts;
using OpenForge.Cli.Core.Framework.Tools.Models.Supply;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

internal static class ToolArchiveExtractor
{
    private const int BufferSize = 81920;
    private const int UnixTypeMask = 0xf000;
    private const int UnixRegular = 0x8000;
    private const int UnixDirectory = 0x4000;
    private const int UnixModeShift = 16;
    private const int PermissionsMask = 0x1ff;

    internal static async ValueTask<ToolArchivePlan> ReadAsync(ToolArtifactExtraction request, CancellationToken token)
    {
        await ToolArtifactIntegrity.VerifyAsync(request.ArchivePath, request.Artifact.Bytes ?? throw new InvalidDataException("An artifact needs a pinned byte size."),
            request.Artifact.Integrity, token).ConfigureAwait(false);
        var members = new List<ToolArchiveMember>();
        await using var file = new FileStream(request.ArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (request.Artifact.Archive == "file")
            Add(members, new(request.Artifact.File, MemberPath(request.Artifact.File, 0, false) ?? throw new InvalidDataException("The artifact file path is missing."),
                file.Length, false, UnixFileMode.UserRead | UnixFileMode.UserWrite));
        else if (request.Artifact.Archive == "zip")
        {
            using var zip = new ZipArchive(file, ZipArchiveMode.Read, leaveOpen: true);
            foreach (var entry in zip.Entries)
            {
                token.ThrowIfCancellationRequested();
                var directory = entry.FullName.EndsWith('/');
                var type = entry.ExternalAttributes >> UnixModeShift & UnixTypeMask;
                if (type != 0 && type != (directory ? UnixDirectory : UnixRegular)
                    || (entry.ExternalAttributes & (int)(FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
                    throw new InvalidDataException("Archive links and special members aren't allowed.");
                if (MemberPath(entry.FullName, request.Artifact.StripComponents, directory) is { } path)
                    Add(members, new(entry.FullName, path, entry.Length, directory, (UnixFileMode)(entry.ExternalAttributes >> UnixModeShift & PermissionsMask)));
            }
        }
        else if (request.Artifact.Archive is "tar.gz" or "tgz")
        {
            await using var gzip = new GZipStream(file, CompressionMode.Decompress, leaveOpen: true);
            await using var tar = new TarReader(gzip, leaveOpen: true);
            for (var entry = await tar.GetNextEntryAsync(copyData: false, token).ConfigureAwait(false); entry is not null;
                entry = await tar.GetNextEntryAsync(copyData: false, token).ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                var directory = entry.EntryType == TarEntryType.Directory;
                if (!directory && entry.EntryType is not TarEntryType.RegularFile and not TarEntryType.V7RegularFile)
                    throw new InvalidDataException("Archive links and special members aren't allowed.");
                if (MemberPath(entry.Name, request.Artifact.StripComponents, directory) is { } path)
                    Add(members, new(entry.Name, path, entry.Length, directory, entry.Mode & (UnixFileMode)PermissionsMask));
            }
        }
        else throw new InvalidDataException("The artifact archive format isn't supported.");

        var paths = members.ToDictionary(member => PortableWorkspacePath.CreatePortableKey(member.Path), StringComparer.Ordinal);
        foreach (var member in members)
        {
            for (var parent = Parent(member.Path); parent is not null; parent = Parent(parent))
                if (paths.TryGetValue(PortableWorkspacePath.CreatePortableKey(parent), out var ancestor) && !ancestor.Directory)
                    throw new InvalidDataException("An archive file is also another member's parent.");
        }
        long expanded = 0;
        try { foreach (var member in members) expanded = checked(expanded + member.Bytes); }
        catch (OverflowException exception) { throw new InvalidDataException("The archive's expanded size exceeds the supported range.", exception); }
        return new([.. members], expanded);
    }

    internal static async ValueTask<ToolExtractedArtifact> ExtractAsync(ToolArtifactExtraction request, CancellationToken token)
    {
        var plan = await ReadAsync(request, token).ConfigureAwait(false);
        var root = Path.GetFullPath(request.DestinationRoot);
        if (!Directory.Exists(root) || new DirectoryInfo(root).LinkTarget is not null || Directory.EnumerateFileSystemEntries(root).Any())
            throw new IOException("Extraction requires an empty ordinary private directory.");
        var drive = new DriveInfo(Path.GetPathRoot(root) ?? throw new IOException("The extraction volume isn't available."));
        if (drive.AvailableFreeSpace < plan.ExpandedBytes) throw new IOException("The extraction volume has insufficient space.");
        var files = new List<ToolFileReceipt>();
        var directories = new HashSet<string>(StringComparer.Ordinal);
        foreach (var member in plan.Members)
        {
            var directory = member.Directory ? member.Path : Parent(member.Path);
            for (; directory is not null; directory = Parent(directory)) directories.Add(directory);
        }
        foreach (var directory in directories.OrderBy(path => path.Count(character => character == '/')).ThenBy(path => path, StringComparer.Ordinal))
        {
            token.ThrowIfCancellationRequested();
            Directory.CreateDirectory(Path.Combine(root, directory.Replace('/', Path.DirectorySeparatorChar)));
        }
        var expected = plan.Members.Where(member => !member.Directory).ToDictionary(member => member.Name, StringComparer.Ordinal);
        await using var archive = new FileStream(request.ArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (request.Artifact.Archive == "file")
            await WriteAsync(expected[request.Artifact.File], archive, root, files, token).ConfigureAwait(false);
        else if (request.Artifact.Archive == "zip")
        {
            using var zip = new ZipArchive(archive, ZipArchiveMode.Read, leaveOpen: true);
            foreach (var entry in zip.Entries)
                if (expected.Remove(entry.FullName, out var member))
                {
                    await using var input = entry.Open();
                    await WriteAsync(member, input, root, files, token).ConfigureAwait(false);
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
                {
                    var data = entry.DataStream ?? throw new InvalidDataException("An archive file has no data stream.");
                    await WriteAsync(member, data, root, files, token).ConfigureAwait(false);
                }
            if (expected.Count != 0) throw new InvalidDataException("The archive members changed before extraction.");
        }
        return new([.. files.OrderBy(file => file.Path, StringComparer.Ordinal)], [.. directories.Order(StringComparer.Ordinal)]);
    }

    private static async ValueTask WriteAsync(ToolArchiveMember member, Stream input, string root, List<ToolFileReceipt> files, CancellationToken token)
    {
        var path = Path.Combine(root, member.Path.Replace('/', Path.DirectorySeparatorChar));
        await using (var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);
            try
            {
                long copied = 0;
                for (var count = await input.ReadAsync(buffer, token).ConfigureAwait(false); count != 0; count = await input.ReadAsync(buffer, token).ConfigureAwait(false))
                {
                    if (count > member.Bytes - copied) throw new InvalidDataException("An archive file exceeds its declared length.");
                    await output.WriteAsync(buffer.AsMemory(0, count), token).ConfigureAwait(false);
                    copied += count;
                }
            }
            finally { ArrayPool<byte>.Shared.Return(buffer); }
            if (output.Length != member.Bytes) throw new InvalidDataException("An extracted file differs from its declared length.");
        }
        var mode = member.Mode | UnixFileMode.UserRead | UnixFileMode.UserWrite;
        if (!OperatingSystem.IsWindows()) File.SetUnixFileMode(path, mode);
        var executable = (mode & (UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute)) != 0;
        files.Add(new(null, member.Path, await ToolArtifactIntegrity.FileHashAsync(path, token).ConfigureAwait(false), executable));
    }

    private static string? MemberPath(string name, int strip, bool directory)
    {
        var raw = directory ? name.TrimEnd('/') : name;
        while (raw.StartsWith("./", StringComparison.Ordinal)) raw = raw[2..];
        if (directory && raw == ".") return null;
        if (!ToolIdentity.IsRelativePath(raw)) throw new InvalidDataException("An archive member has an unsafe path.");
        var segments = raw.Split('/');
        if (segments.Length <= strip)
        {
            if (directory) return null;
            throw new InvalidDataException("Stripping archive components removes a file name.");
        }
        return string.Join('/', segments.Skip(strip));
    }

    private static void Add(List<ToolArchiveMember> members, ToolArchiveMember member)
    {
        var key = PortableWorkspacePath.CreatePortableKey(member.Path);
        if (member.Bytes < 0 || members.Any(existing => PortableWorkspacePath.CreatePortableKey(existing.Path) == key))
            throw new InvalidDataException("An archive has a duplicate destination or invalid length.");
        members.Add(member);
    }

    private static string? Parent(string path)
        => path.LastIndexOf('/') is var separator && separator >= 0 ? path[..separator] : null;
}
