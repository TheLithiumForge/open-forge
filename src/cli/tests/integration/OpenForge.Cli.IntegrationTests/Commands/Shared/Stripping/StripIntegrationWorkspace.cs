using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Shared.Configuration;
using OpenForge.Cli.Core.Commands.Route.Include;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Request;
using OpenForge.Cli.Core.Commands.Shared.Stripping;
using OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Result;
using OpenForge.Cli.Core.Commands.Uninstall;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.Stripping;

/// <summary>An owned workspace for strip and uninstall evidence, with exact-byte helpers.</summary>
internal sealed class StripIntegrationWorkspace : IDisposable
{
    private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);
    private readonly TemporaryWorkspace _temporary;
    private readonly WorkspaceLockTestStore _lockStore;

    private StripIntegrationWorkspace(TemporaryWorkspace temporary, WorkspaceLockTestStore lockStore)
    {
        _temporary = temporary;
        _lockStore = lockStore;
        Workspace = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        _ = lockStore.Track(Workspace);
    }

    internal CliWorkspace Workspace { get; }

    internal string Path => _temporary.Path;

    internal static StripIntegrationWorkspace Create(string purpose)
    {
        var temporary = TemporaryWorkspace.Create(purpose);
        var lockStore = WorkspaceLockTestStore.Create($"{purpose}-lock-store");
        return new StripIntegrationWorkspace(temporary, lockStore);
    }

    internal string Combine(string path) => _temporary.Combine(path);

    // Folders the CLI created belong to the workspace under test, so write into them directly.
    internal void Write(string path, string text)
    {
        var physical = Combine(path);
        if (Directory.Exists(System.IO.Path.GetDirectoryName(physical)))
        {
            File.WriteAllBytes(physical, Utf8.GetBytes(text));
            return;
        }

        _temporary.CreateFile(path, Utf8.GetBytes(text));
    }

    internal string Read(string path) => File.ReadAllText(Combine(path), Utf8);

    internal bool Exists(string path) => File.Exists(Combine(path)) || Directory.Exists(Combine(path));

    internal IReadOnlyDictionary<string, string> SnapshotHashes() => _temporary.SnapshotHashes();

    internal async Task InstallAsync()
    {
        var completion = await RunAsync([
            InstallDefinitions.CommandIdentity,
            InstallDefinitions.PresetOption,
            InstallConfigurationChoices.Name(InstallPreset.Minimal),
            InstallDefinitions.Automatic.Name]);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
    }

    internal async Task IncludeAsync(string folder)
    {
        var result = await new RouteIncludeOperation(_lockStore.StoreRoot).ExecuteAsync(
            new RouteIncludeRequest { Workspace = Workspace, File = folder }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
    }

    internal StripRequest Request(StripCommandKind command, ImmutableArray<StripPart> parts, string? flatten = null, bool dryRun = false)
        => new()
        {
            Workspace = Workspace,
            Command = command,
            Parts = parts,
            Flatten = flatten,
            DryRun = dryRun,
            Automatic = true,
        };

    internal StripOperationRunner Runner()
        => new(_lockStore.StoreRoot, null, static _ => throw new InvalidOperationException("Automatic runs never ask."));

    internal ValueTask<StripResult> StripAsync(StripRequest request)
        => Runner().RunAsync(request, null, TestContext.Current.CancellationToken);

    internal ValueTask<StripResult> UninstallAsync(StripRequest request)
        => new UninstallOperation(Runner()).ExecuteAsync(request, TestContext.Current.CancellationToken);

    private async Task<CliProcessCompletion> RunAsync(IReadOnlyList<string> arguments)
    {
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity(CliSyntaxDefinitions.ExecutableName, nameof(StripIntegrationWorkspace)),
            new CliCompositionInputs
            {
                StandardInput = TextReader.Null,
                PromptOutput = TextWriter.Null,
                StandardInputRedirected = true,
                PromptOutputRedirected = true,
                LockStoreRoot = _lockStore.StoreRoot,
            });
        return await application.RunAsync(
            [.. arguments],
            new CliProcessEnvironment(Path),
            new CliOutputWriters(new StringWriter(), new StringWriter()),
            TestContext.Current.CancellationToken);
    }

    public void Dispose()
    {
        try
        {
            // Install, strip and uninstall create entries the temporary workspace never owned.
            foreach (var entry in new[] { WorkspaceLayout.DefaultMainRoot, WorkspaceLayoutNames.AgentHostFileName, WorkspaceLayoutNames.ClaudeHostFileName, WorkspaceRuntimePaths.DirectoryName })
            {
                var path = Combine(entry);
                if (Directory.Exists(path)) Directory.Delete(path, recursive: true);
                else if (File.Exists(path)) File.Delete(path);
            }
        }
        finally
        {
            _lockStore.Dispose();
            _temporary.Dispose();
        }
    }
}
