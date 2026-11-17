using System.Formats.Tar;
using System.IO.Compression;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Runtimes;

internal static class ToolNodeArchivePreparation
{
    private const int UnixTypeMask = 0xf000;
    private const int UnixSymbolicLink = 0xa000;
    private const int UnixModeShift = 16;

    internal static async Task<ToolExtractionRequest> PrepareAsync(ToolExtractionRequest request, string preparedPath, CancellationToken token)
    {
        if (request.Preparation != ToolArchivePreparation.OmitLinks || request.Asset.Url.Host != ToolArtifactSource.NodeHost)
            throw new InvalidDataException("Only an official Node archive can omit links.");
        await using var input = new FileStream(request.ArchivePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        await ToolArtifactIntegrity.VerifyAsync(request.ArchivePath, request.Asset, token).ConfigureAwait(false);
        long expanded = 0;
        var members = 0;
        await using (var output = new FileStream(preparedPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            if (request.Asset.Archive == ToolArchiveKind.Zip)
            {
                using var source = new ZipArchive(input, ZipArchiveMode.Read, leaveOpen: true);
                using var destination = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true);
                foreach (var entry in source.Entries)
                {
                    token.ThrowIfCancellationRequested();
                    CheckSize(entry.Length);
                    if ((entry.ExternalAttributes >> UnixModeShift & UnixTypeMask) == UnixSymbolicLink) continue;
                    var copy = destination.CreateEntry(entry.FullName);
                    copy.ExternalAttributes = entry.ExternalAttributes;
                    await using var data = entry.Open();
                    await using var target = copy.Open();
                    await data.CopyToAsync(target, token).ConfigureAwait(false);
                }
            }
            else if (request.Asset.Archive == ToolArchiveKind.TarGzip)
            {
                await using var decompressed = new GZipStream(input, CompressionMode.Decompress, leaveOpen: true);
                await using var reader = new TarReader(decompressed, leaveOpen: true);
                await using var seekable = new FileStream(TarPreparationPath(preparedPath), FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
                await using (var writer = new TarWriter(seekable, leaveOpen: true))
                {
                    for (var entry = await reader.GetNextEntryAsync(copyData: false, token).ConfigureAwait(false); entry is not null;
                        entry = await reader.GetNextEntryAsync(copyData: false, token).ConfigureAwait(false))
                    {
                        CheckSize(entry.Length);
                        if (entry.EntryType is not (TarEntryType.SymbolicLink or TarEntryType.HardLink))
                            await writer.WriteEntryAsync(entry, token).ConfigureAwait(false);
                    }
                }
                seekable.Position = 0;
                await using var compressed = new GZipStream(output, CompressionLevel.Fastest, leaveOpen: true);
                await seekable.CopyToAsync(compressed, token).ConfigureAwait(false);
            }
            else throw new InvalidDataException("A Node runtime needs a portable archive.");
        }
        return request with
        {
            Preparation = ToolArchivePreparation.Original,
            ArchivePath = preparedPath,
            Asset = request.Asset with
            {
                Sha256 = await ToolArtifactIntegrity.FileHashAsync(preparedPath, token).ConfigureAwait(false),
                MaximumBytes = new FileInfo(preparedPath).Length,
            },
        };

        void CheckSize(long length)
        {
            if (++members > ToolArchiveExtractor.MaximumMembers || length < 0 || length > ToolArchiveExtractor.MaximumExpandedBytes - expanded)
                throw new InvalidDataException("Node archive preparation exceeds its member or size limit.");
            expanded += length;
        }
    }

    internal static string TarPreparationPath(string preparedPath) => $"{preparedPath}.tar";
}
