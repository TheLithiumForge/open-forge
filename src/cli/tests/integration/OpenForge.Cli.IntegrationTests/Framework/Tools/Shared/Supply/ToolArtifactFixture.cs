using System.Formats.Tar;
using System.IO.Compression;
using System.Security.Cryptography;
using OpenForge.Cli.Core.Framework.Tools.Models.Supply;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Supply;

internal sealed class ToolArtifactFixture : IDisposable
{
    internal const string Input = "fixture.archive";
    internal const string Output = "extracted";
    internal const string Wrapper = "wrapper";
    internal const string Entry = "bin/fixture.bin";
    internal static readonly byte[] Payload = [0, 127, 255, 13, 10];
    internal TemporaryWorkspace Temporary { get; } = TemporaryWorkspace.Create("tool-artifact");
    internal string OutputPath => Temporary.Combine(Output);

    internal ToolArtifactExtraction Create(string format, string[]? names = null, TarEntryType? special = null)
    {
        names ??= [Wrapper + "/" + Entry, Wrapper + "/empty.txt"];
        using var bytes = new MemoryStream();
        if (format == "file") bytes.Write(Payload);
        else if (format == "zip")
        {
            using var zip = new ZipArchive(bytes, ZipArchiveMode.Create, leaveOpen: true);
            foreach (var name in names)
            {
                var entry = zip.CreateEntry(name);
                using var content = entry.Open();
                content.Write(Payload);
            }
        }
        else
        {
            using var gzip = new GZipStream(bytes, CompressionMode.Compress, leaveOpen: true);
            using var tar = new TarWriter(gzip, leaveOpen: true);
            foreach (var name in names)
            {
                var entry = new PaxTarEntry(special ?? TarEntryType.RegularFile, name);
                if (special is not null) entry.LinkName = Wrapper + "/target";
                else entry.DataStream = new MemoryStream(Payload, writable: false);
                entry.Mode = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;
                tar.WriteEntry(entry);
                entry.DataStream?.Dispose();
            }
        }
        var artifact = bytes.ToArray();
        Temporary.WriteBytes(Input, artifact);
        Temporary.CreateDirectory(Output);
        return new(new()
        {
            File = Entry,
            Bytes = artifact.Length,
            Integrity = ToolArtifactIntegrity.Sha256Prefix + Convert.ToHexStringLower(SHA256.HashData(artifact)),
            Archive = format,
            StripComponents = format == "file" ? 0 : 1,
        }, Temporary.Combine(Input), OutputPath);
    }

    private static void DeleteChildren(DirectoryInfo directory)
    {
        foreach (var entry in directory.EnumerateFileSystemInfos())
        {
            if (entry is DirectoryInfo child)
            {
                if (entry.LinkTarget is null && (entry.Attributes & FileAttributes.ReparsePoint) == 0) DeleteChildren(child);
                child.Delete(recursive: false);
            }
            else entry.Delete();
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(OutputPath)) DeleteChildren(new DirectoryInfo(OutputPath));
        Temporary.Dispose();
    }
}
