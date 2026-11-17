using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Framework.Tools.Shared.Records;
using OpenForge.Cli.Core.Framework.Tools.Shared.Versions;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Records;

internal sealed class ToolRecordPublicationFixture : IDisposable
{
    internal const string OtherToolId = "retained-synthetic";
    internal ToolRecordFixture Records { get; } = new();
    internal PublicationBackups Backups { get; } = new();
    internal ToolRecordService Service { get; }
    internal static CancellationToken Token => TestContext.Current.CancellationToken;

    internal ToolRecordPublicationFixture() => Service = new(Records.Catalogue, new ToolVersionService(), Backups);

    internal async Task<ToolRecordsResult> SeedAsync(bool additional = false)
    {
        var observed = await Records.Service.ReadAsync(Records.Locations, Token);
        // The reader observes absences only for catalogue IDs and runtimes. The seed's extra files are absent before it runs.
        var absent = new Dictionary<string, string?>(observed.ExpectedFileSha256, StringComparer.Ordinal);
        foreach (var path in new[] { Records.Paths.Runtime(ToolRecordFixture.InitialVersion), Records.Paths.Runtime(ToolRecordFixture.ChangedVersion), Records.Paths.Tool(OtherToolId) })
            absent.TryAdd(path, null);
        var before = observed with { ExpectedFileSha256 = absent };
        var request = Records.Write(before) with
        {
            Runtimes = [Runtime()],
            WorkspaceMachine = new()
            {
                SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion,
                WorkspaceKey = Records.Paths.WorkspaceKey,
                WorkspacePath = Records.PhysicalWorkspace,
                NativeSettings = [new()
                {
                    ToolId = ToolRecordFixture.Id, ChangeId = "synthetic-native", NativeHomePath = Records.Paths.HomePath,
                    Previous = new() { Present = true, Kind = ToolRecordedValueKind.Native, Native = "before" },
                    Applied = new() { Present = true, Kind = ToolRecordedValueKind.Native, Native = "after" }, ManualRestoreArguments = ["synthetic-restore"],
                }],
            },
            Path = new()
            {
                SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion,
                BinPath = Path.Combine(Records.Paths.HomePath, ToolRecordDefinitions.BinDirectoryName),
                Changes = [],
            },
        };
        if (additional)
        {
            request = request with
            {
                Machine = [Records.Machine(), Records.Machine() with { ToolId = OtherToolId }],
                Runtimes = [Runtime(), Runtime(ToolRecordFixture.ChangedVersion)],
            };
        }
        Assert.Null((await Records.Service.WriteAsync(request, Token)).Failure);
        return await Service.ReadAsync(Records.Locations, Token);
    }

    internal ToolNodeRuntimeRecord Runtime(string version = ToolRecordFixture.InitialVersion) => new()
    {
        SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion,
        Version = version,
        RuntimePath = Path.Combine(Records.Paths.HomePath, ToolRecordDefinitions.RuntimesDirectoryName, ToolRecordDefinitions.NodeDirectoryName, version),
        Integrity = new() { Kind = ToolVerificationKind.SignedNodeChecksums, Source = "synthetic-source", Sha256 = new('b', 64) },
    };

    internal ToolRecordWriteRequest Publication(ToolRecordsResult expected) => Records.Write(expected) with
    {
        Portable = expected.Portable ?? throw new InvalidOperationException(),
        Machine = [],
        Runtimes = [],
    };

    internal ToolRecordWriteRequest Deletion(ToolRecordsResult expected, bool runtime) => Publication(expected) with
    {
        RemovedToolIds = runtime ? [] : [ToolRecordFixture.Id],
        RemovedRuntimeVersions = runtime ? [ToolRecordFixture.InitialVersion] : [],
    };

    internal string SelectedPath(bool runtime) => runtime ? Records.Paths.Runtime(ToolRecordFixture.InitialVersion) : Records.Paths.Tool(ToolRecordFixture.Id);
    public void Dispose() => Records.Dispose();

    // Real record and sibling-backup IO stays in the owned fixtures. Git discovery is unavailable in this deterministic host.
    internal sealed class PublicationBackups : IToolBackupService
    {
        private readonly ToolBackupService _inner = new(new NoGitHost());
        internal int Calls { get; private set; }
        internal Action? BeforePrepare { get; set; }
        internal Func<ToolBackupResult, int, ToolBackupResult>? AfterPrepare { get; set; }

        public async Task<ToolBackupResult> PrepareAsync(ToolBackupRequest request, CancellationToken cancellationToken)
        {
            Calls++;
            BeforePrepare?.Invoke();
            var result = await _inner.PrepareAsync(request, cancellationToken);
            return AfterPrepare?.Invoke(result, Calls) ?? result;
        }
        public Task<ToolBackupCompletionResult> CompleteAsync(ToolBackupCompletionRequest request, CancellationToken cancellationToken)
            => throw new InvalidOperationException("Record services leave backup completion to the operation.");
    }

    private sealed class NoGitHost : IToolProcessHost
    {
        public Task<ToolResolvedCommand?> ResolveAsync(string command, string workingDirectory, CancellationToken cancellationToken)
            => Task.FromResult<ToolResolvedCommand?>(null);
        public Task<ToolProcessResult> RunAsync(ToolProcessRequest request, CancellationToken cancellationToken)
            => throw new InvalidOperationException("No external process is part of these record cases.");
    }
}
