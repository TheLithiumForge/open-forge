using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Records;

internal sealed class ToolRecordService(ToolCatalogue catalogue, IToolVersionService versions, IToolBackupService backups) : IToolRecordService
{
    public Task<ToolRecordWriteResult> AddAllowInstallPathsAsync(ToolRecordGrantRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return new ToolRecordGrantWriter(backups).WriteAsync(request, cancellationToken);
    }

    public Task<ToolRecordsResult> ReadAsync(ToolRecordsRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        return new ToolRecordReader(new ToolPortableCodec(versions), catalogue, new ToolRecordPaths(request.WorkspacePath, request.HomePath)).ReadAsync(cancellationToken);
    }

    public Task<ToolRecordWriteResult> WriteAsync(ToolRecordWriteRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        return new ToolRecordWriter(new ToolPortableCodec(versions), backups).WriteAsync(request, cancellationToken);
    }
}
