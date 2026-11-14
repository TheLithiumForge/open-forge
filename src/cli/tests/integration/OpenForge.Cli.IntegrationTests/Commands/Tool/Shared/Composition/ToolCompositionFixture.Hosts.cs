using OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Composition.Models;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Composition;

internal sealed partial class ToolCompositionFixture : IToolProcessHost, IToolDownloadHost, IToolPathHost
{
    public Task<ToolResolvedCommand?> ResolveAsync(string command, string workingDirectory, CancellationToken cancellationToken)
    {
        CheckHosts(); ResolveCalls++;
        if (command == ToolGitEnvironment.Executable && _git is not null) { GitCalls++; return _git.ResolveAsync(command, workingDirectory, cancellationToken); }
        if (Missing || !Catalogue.Entries.ContainsKey(command)) return Task.FromResult<ToolResolvedCommand?>(null);
        return Task.FromResult<ToolResolvedCommand?>(new(Command(command), new() { ExecutablePath = Command(command), PrefixArguments = [] }));
    }

    public Task<ToolProcessResult> RunAsync(ToolProcessRequest request, CancellationToken cancellationToken)
    {
        CheckHosts(); cancellationToken.ThrowIfCancellationRequested();
        if (_git is not null && request.ExecutablePath == _gitExecutable) return _git.RunAsync(request, cancellationToken);
        var operation = request.Arguments.FirstOrDefault();
        NativeRequests.Add(request);
        if (Catalogue.Entries.Values.Any(entry => request.Arguments.SequenceEqual(entry.Detection.Arguments)))
        {
            Events.Add(ToolHostEvent.Probe);
            return Task.FromResult(Result(DetectedVersion));
        }
        if (operation == Inventory)
        {
            Events.Add(ToolHostEvent.Inventory);
            return Task.FromResult(Result("{}"));
        }
        var id = request.Arguments.Last();
        if (operation == Setup)
        {
            Events.Add(ToolHostEvent.Setup);
            NativeSawBackup |= File.Exists(ToolBackupPaths.Beside(NativeFile(id)));
            File.WriteAllText(NativeFile(id), "synthetic native effect");
            return Task.FromResult(Result(string.Empty, FailSetup ? 1 : 0));
        }
        if (operation == Uninstall)
        {
            Events.Add(ToolHostEvent.Uninstall);
            NativeSawProjectAddition = HasMember(id);
            File.Delete(NativeFile(id));
            return Task.FromResult(Result(string.Empty));
        }
        throw new InvalidOperationException("Unexpected synthetic Tool process.");
    }

    private static ToolProcessResult Result(string output, int exit = 0) => new()
    { ExitCode = exit, StandardOutput = output, StandardError = string.Empty, TimedOut = false, Cancelled = false };

    public Task<ToolNpmMetadataResult> ReadNpmMetadataAsync(ToolNpmMetadataRequest request, CancellationToken cancellationToken)
    { CheckHosts(); DownloadCalls++; throw new InvalidOperationException("Synthetic existing installations need no metadata."); }
    public Task<ToolDownloadResult> DownloadAsync(ToolDownloadRequest request, CancellationToken cancellationToken)
    {
        CheckHosts(); DownloadCalls++; cancellationToken.ThrowIfCancellationRequested();
        if (!AllowSyntheticDownload) throw new InvalidOperationException("Synthetic existing installations need no download.");
        Core.Framework.Tools.Shared.Records.ToolRecordPaths.Check(Home, request.DestinationPath);
        File.WriteAllBytes(request.DestinationPath, DownloadBytes);
        return Task.FromResult(new ToolDownloadResult(request.DestinationPath,
            new() { Kind = ToolVerificationKind.PinnedSha256, Source = request.Asset.Url.AbsoluteUri, Sha256 = request.Asset.Sha256 }, null));
    }
    public Task<ToolPathFacts> ObserveAsync(string homePath, CancellationToken cancellationToken)
    {
        CheckHosts(); PathObservations++;
        Assert.Equal(TestContext.Current.CancellationToken, cancellationToken);
        if (CancelPathObservation) throw new OperationCanceledException(cancellationToken);
        Assert.Equal(Home, homePath);
        return Task.FromResult(new ToolPathFacts { BinPath = System.IO.Path.Combine(Home, ToolRecordDefinitions.BinDirectoryName), Readiness = PathReadiness, DetectedShellFiles = [] });
    }
    public Task<ToolPathSetupResult> AddAsync(ToolPathSetupRequest request, CancellationToken cancellationToken)
    {
        CheckHosts();
        if (request.Choice == ToolPathChoice.Manual)
            return Task.FromResult(new ToolPathSetupResult(null, [], false, null) { PendingBackups = [] });
        PathEffects++; Events.Add(ToolHostEvent.PathAdd);
        return Task.FromResult(new ToolPathSetupResult(new() { SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion, BinPath = request.Facts.BinPath, Changes = [] }, [], true, null) { PendingBackups = [] });
    }
    public Task<ToolPathRemoveResult> RemoveAsync(ToolPathRemoveRequest request, CancellationToken cancellationToken)
    {
        CheckHosts(); PathEffects++; Events.Add(ToolHostEvent.PathRemove);
        return Task.FromResult(new ToolPathRemoveResult([], [], true, null) { PendingBackups = [] });
    }
    private void CheckHosts()
    {
        if (ThrowHosts) throw new InvalidOperationException("An inert invocation called a Tool host.");
    }
}
