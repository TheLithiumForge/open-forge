using System.Formats.Tar;
using System.IO.Compression;
using System.Security.Cryptography;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Runtimes;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Supply;

[Trait("Feature", "tool-supply"), Trait("Evidence", "Integration")]
public sealed class ToolArchiveExtractorTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;
    private static readonly byte[] Payload = [1, 127, 255];

    [Theory(DisplayName = "Verified archives extract contained bytes before returning completion")]
    [InlineData("zip")]
    [InlineData("tar")]
    [InlineData("file")]
    public async Task ExtractsVerifiedBytes(string format)
    {
        using var temporary = TemporaryWorkspace.Create("tool-archive");
        var request = Create(temporary, format, ["wrapper/sample"]);
        File.Delete(temporary.CreateFile("output/sample"));
        var before = temporary.SnapshotHashes();
        var plan = await ToolArchiveExtractor.ReadAsync(request, Token);
        Assert.Equal(before, temporary.SnapshotHashes());
        Assert.Equal(Payload.Length, plan.ExpandedBytes);
        var result = await ToolArchiveExtractor.ExtractAsync(request, Token);
        Assert.Null(result.Failure);
        Assert.Equal(Payload, await File.ReadAllBytesAsync(Assert.Single(result.ExtractedPaths), Token));
    }

    [Theory(DisplayName = "Unsafe archive paths block before extracting any member")]
    [InlineData("traversal")]
    [InlineData("absolute")]
    [InlineData("backslash")]
    [InlineData("duplicate")]
    [InlineData("case-collision")]
    [InlineData("file-parent")]
    public async Task RejectsUnsafeDestinations(string scenario)
    {
        using var temporary = TemporaryWorkspace.Create("tool-archive");
        string[] names = scenario switch
        {
            "traversal" => ["wrapper/../escape"],
            "absolute" => [temporary.Combine("escape")],
            "backslash" => ["wrapper\\escape"],
            "duplicate" => ["wrapper/same", "wrapper/same"],
            "case-collision" => ["wrapper/Same", "wrapper/same"],
            "file-parent" => ["wrapper/parent", "wrapper/parent/child"],
            _ => throw new ArgumentOutOfRangeException(nameof(scenario)),
        };
        var request = Create(temporary, "zip", names);
        var before = temporary.SnapshotHashes();
        await Assert.ThrowsAsync<InvalidDataException>(async () => await ToolArchiveExtractor.ExtractAsync(request, Token));
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Theory(DisplayName = "Archive links and special members are rejected")]
    [InlineData(TarEntryType.SymbolicLink)]
    [InlineData(TarEntryType.HardLink)]
    [InlineData(TarEntryType.Fifo)]
    public async Task RejectsLinks(TarEntryType kind)
    {
        using var temporary = TemporaryWorkspace.Create("tool-archive");
        var request = Create(temporary, "tar", ["wrapper/link"], kind);
        await Assert.ThrowsAsync<InvalidDataException>(async () => await ToolArchiveExtractor.ExtractAsync(request, Token));
        Assert.Empty(Directory.EnumerateFileSystemEntries(request.DestinationPath));
    }

    [Theory(DisplayName = "Digest and size mismatches block before archive interpretation")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RejectsDifferentBytes(bool size)
    {
        using var temporary = TemporaryWorkspace.Create("tool-archive");
        var request = Create(temporary, "zip", ["wrapper/sample"]);
        var asset = size ? request.Asset with { MaximumBytes = 1 } : request.Asset with { Sha256 = new string('0', SHA256.HashSizeInBytes * 2) };
        await Assert.ThrowsAsync<InvalidDataException>(async () => await ToolArchiveExtractor.ExtractAsync(request with { Asset = asset }, Token));
        Assert.Empty(Directory.EnumerateFileSystemEntries(request.DestinationPath));
    }

    [Fact(DisplayName = "Official Node preparation removes links before the unchanged rejecting extractor")]
    public async Task PreparesNodeLinks()
    {
        using var temporary = TemporaryWorkspace.Create("tool-node-archive");
        var request = Create(temporary, "tar", ["wrapper/link"], TarEntryType.SymbolicLink);
        File.Delete(temporary.CreateFile("output/sample"));
        request = request with { Asset = request.Asset with { Url = new($"https://{ToolArtifactSource.NodeHost}/dist/v{request.Asset.Version}/node.tar.gz") }, Preparation = ToolArchivePreparation.OmitLinks };
        var preparedPath = temporary.CreateFile("prepared");
        File.Delete(preparedPath);
        var seekable = ToolNodeArchivePreparation.TarPreparationPath(preparedPath);
        File.Delete(temporary.CreateFile(System.IO.Path.GetRelativePath(temporary.Path, seekable)));
        var prepared = await ToolNodeArchivePreparation.PrepareAsync(request, preparedPath, Token);
        Assert.Equal(ToolArchivePreparation.Original, prepared.Preparation);
        var result = await ToolArchiveExtractor.ExtractAsync(prepared, Token);
        Assert.Equal(Payload, await File.ReadAllBytesAsync(Assert.Single(result.ExtractedPaths), Token));
        Assert.False(File.Exists(System.IO.Path.Combine(request.DestinationPath, "link")));
    }

    private static ToolExtractionRequest Create(TemporaryWorkspace temporary, string format, string[] names, TarEntryType kind = TarEntryType.RegularFile)
    {
        var path = temporary.CreateFile("archive");
        using (var stream = File.Create(path))
        {
            if (format == "zip")
            {
                using var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true);
                foreach (var name in names)
                {
                    using var entry = archive.CreateEntry(name).Open();
                    entry.Write(Payload);
                }
            }
            else if (format == "tar")
            {
                using var gzip = new GZipStream(stream, CompressionLevel.Fastest, leaveOpen: true);
                using var writer = new TarWriter(gzip, leaveOpen: true);
                foreach (var name in names)
                {
                    var entry = new PaxTarEntry(kind, name);
                    if (kind == TarEntryType.RegularFile) entry.DataStream = new MemoryStream(Payload);
                    if (kind is TarEntryType.SymbolicLink or TarEntryType.HardLink) entry.LinkName = "sample";
                    writer.WriteEntry(entry);
                    entry.DataStream?.Dispose();
                }
                if (kind != TarEntryType.RegularFile)
                {
                    using var data = new MemoryStream(Payload);
                    writer.WriteEntry(new PaxTarEntry(TarEntryType.RegularFile, "wrapper/sample") { DataStream = data });
                }
            }
            else stream.Write(Payload);
        }
        return new(new()
        {
            Version = "2.4.3",
            Target = new() { OperatingSystem = ToolOperatingSystem.Windows, Cpu = ToolCpu.X64 },
            Url = new("https://github.com/example/sample/releases/download/v2.4.3/sample.zip"),
            Archive = format switch { "zip" => ToolArchiveKind.Zip, "tar" => ToolArchiveKind.TarGzip, _ => ToolArchiveKind.File },
            EntryRelativePath = "sample",
            StripComponents = 1,
            MaximumBytes = new FileInfo(path).Length,
            Sha256 = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(path))),
        }, path, temporary.CreateDirectory("output"), ToolArchivePreparation.Original);
    }
}
