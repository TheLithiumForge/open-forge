using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Hosting.Shared.ToolIsolation;

// Shared in-process capture isolation. The normal composition input carries every Tool capability.
internal sealed class CliToolIsolation : IDisposable, IToolProcessHost, IToolDownloadHost, IToolPathHost
{
    private readonly TemporaryWorkspace _owned = TemporaryWorkspace.Create("captured-tool-home");
    internal CliToolHostInputs Inputs => new()
    {
        HomePath = _owned.Combine(ToolRecordDefinitions.HomeDirectoryName),
        NativeHomePath = _owned.Path,
        ClientRoots = new Dictionary<string, string>(),
        ProcessEnvironment = new Dictionary<string, string>(),
        Processes = this,
        Downloads = this,
        Path = this,
    };

    public Task<ToolResolvedCommand?> ResolveAsync(string command, string workingDirectory, CancellationToken cancellationToken)
        => Task.FromResult<ToolResolvedCommand?>(null);
    public Task<ToolProcessResult> RunAsync(ToolProcessRequest request, CancellationToken cancellationToken)
        => throw new InvalidOperationException("The isolated capture didn't authorize a Tool process.");
    public Task<ToolNpmMetadataResult> ReadNpmMetadataAsync(ToolNpmMetadataRequest request, CancellationToken cancellationToken)
        => throw new InvalidOperationException("The isolated capture didn't authorize Tool metadata.");
    public Task<ToolDownloadResult> DownloadAsync(ToolDownloadRequest request, CancellationToken cancellationToken)
        => throw new InvalidOperationException("The isolated capture didn't authorize a Tool download.");
    public Task<ToolPathFacts> ObserveAsync(string homePath, CancellationToken cancellationToken)
        => Task.FromResult(new ToolPathFacts
        {
            BinPath = System.IO.Path.Combine(homePath, ToolRecordDefinitions.BinDirectoryName),
            Readiness = ToolPathReadiness.Ready,
            DetectedShellFiles = [],
        });
    public Task<ToolPathSetupResult> AddAsync(ToolPathSetupRequest request, CancellationToken cancellationToken)
        => throw new InvalidOperationException("The isolated capture didn't authorize a PATH effect.");
    public Task<ToolPathRemoveResult> RemoveAsync(ToolPathRemoveRequest request, CancellationToken cancellationToken)
        => throw new InvalidOperationException("The isolated capture didn't authorize a PATH effect.");
    public void Dispose() => _owned.Dispose();
}
