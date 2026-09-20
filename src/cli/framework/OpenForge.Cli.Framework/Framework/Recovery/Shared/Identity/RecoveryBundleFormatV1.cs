using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;

namespace OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;

internal static class RecoveryBundleFormatV1
{
    internal const int SchemaVersion = 1;
    internal const int StreamBufferSize = 64 * 1024;
    internal const int Sha256HexLength = 64;
    internal const string ApplicationDirectoryName = "OpenForge";
    internal const string RecoveryDirectoryName = "recovery";
    internal const string VersionDirectoryName = "v1";
    internal const string ManifestEntryName = "manifest.json";
    internal const string PayloadDirectoryName = "payloads";
    internal const string FinalFilePrefix = "operation-";
    internal const string FinalFileExtension = ".zip";
    internal const string DraftFileExtension = ".draft";
    internal const string OperationIdFormat = "N";

    internal static string PayloadName(int ordinal)
        => $"{PayloadDirectoryName}/{ordinal:D8}.bin";

    internal static string FinalFileName(Guid operationId)
        => $"{FinalFilePrefix}{operationId.ToString(OperationIdFormat)}{FinalFileExtension}";

    internal static string DraftFileName(Guid operationId)
        => $"{FinalFilePrefix}{operationId.ToString(OperationIdFormat)}{DraftFileExtension}";

    internal static bool TryParseCandidateFileName(
        string fileName,
        out Guid operationId,
        out RecoveryBundleCandidateKind kind)
    {
        operationId = Guid.Empty;
        kind = default;
        if (!fileName.StartsWith(FinalFilePrefix, StringComparison.Ordinal))
        {
            return false;
        }

        string extension;
        if (fileName.EndsWith(FinalFileExtension, StringComparison.Ordinal))
        {
            extension = FinalFileExtension;
        }
        else if (fileName.EndsWith(DraftFileExtension, StringComparison.Ordinal))
        {
            extension = DraftFileExtension;
        }
        else
        {
            return false;
        }

        var operationText = fileName[FinalFilePrefix.Length..^extension.Length];
        if (!Guid.TryParseExact(operationText, OperationIdFormat, out operationId))
        {
            return false;
        }

        kind = extension == FinalFileExtension
            ? RecoveryBundleCandidateKind.Final
            : RecoveryBundleCandidateKind.Draft;
        var expected = kind == RecoveryBundleCandidateKind.Final
            ? FinalFileName(operationId)
            : DraftFileName(operationId);
        return string.Equals(fileName, expected, StringComparison.Ordinal);
    }
}
