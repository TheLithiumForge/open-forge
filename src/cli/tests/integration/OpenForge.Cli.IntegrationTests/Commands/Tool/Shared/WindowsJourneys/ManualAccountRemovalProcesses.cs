using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Tools.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodebaseMemory;
using OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.WindowsJourneys;

// Only the separate injected account-removal proof uses this host. It starts no process or transfer.
internal sealed class ManualAccountRemovalProcesses(string location) : IToolProcessHost, IToolDownloadHost
{
    private ToolRecipeContext? _context;
    private string Binary => Path.Combine(location, "account", ToolRecordDefinitions.HomeDirectoryName, ToolRecordDefinitions.BinDirectoryName, CodebaseMemoryDefinitions.WindowsBinary);
    private string NativeFile => Context.ClientRoots[CodebaseMemoryNativeOutputs.ConfigurationRootKey(CodebaseMemoryDefinitions.Codex)];
    private ToolRecipeContext Context => _context ?? throw new InvalidOperationException("The injected host needs its owned context.");
    internal List<ToolProcessRequest> Uninstalls { get; } = [];
    internal bool NativeSawProjectBlock { get; private set; }
    internal bool NativePathEffectSimulated { get; private set; }

    internal void Configure(ToolRecipeContext context) => _context = context;

    internal void BindUnavailable(IEnumerable<string> keys, IDictionary<string, string> bindings)
    {
        foreach (var key in keys) bindings[key] = Path.Combine(location, "account", "inert-client-bindings", key + ".json");
    }

    public Task<ToolResolvedCommand?> ResolveAsync(string command, string workingDirectory, CancellationToken cancellationToken)
        => Task.FromResult<ToolResolvedCommand?>(command == CodebaseMemoryDefinitions.Command && File.Exists(Binary)
            ? new(Binary, new() { ExecutablePath = Binary, PrefixArguments = [] }) : null);

    public Task<ToolProcessResult> RunAsync(ToolProcessRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (request.ExecutablePath != Binary) throw new IOException("The injected proof received an unexpected executable.");
        var entry = CodebaseMemoryCatalogueEntry.Create();
        if (request.Arguments.SequenceEqual(entry.Detection.Arguments)) return Result(CodebaseMemoryDefinitions.Version);
        if (request.Arguments.FirstOrDefault() == ToolCatalogueDefinitions.InstallCommand)
        {
            if (request.Arguments.Contains(CodebaseMemoryDefinitions.PlanArgument))
            {
                return Result(JsonSerializer.Serialize(new ToolCbmNativePlan
                {
                    Type = ToolNativePlanReader.CbmPlanType,
                    AgentsDetected = [CodebaseMemoryDefinitions.Codex],
                    ConfigFilesPlanned = [NativeFile],
                    InstructionFilesPlanned = [],
                    SkillFilesPlanned = [],
                    AgentFilesPlanned = [],
                    PromptFilesPlanned = [],
                    HooksPlanned = [],
                    WritesStarted = false,
                    NetworkAfterInstall = false,
                    NextSafeCommand = ToolCatalogueDefinitions.InstallCommand,
                }, ToolNativePlanJsonContext.Default.ToolCbmNativePlan));
            }
            Directory.CreateDirectory(Path.GetDirectoryName(NativeFile) ?? throw new IOException("The owned client file needs a parent."));
            File.WriteAllText(NativeFile, "inert native account entry\n");
            return Result(string.Empty);
        }
        if (request.Arguments.FirstOrDefault() != ToolCatalogueDefinitions.UninstallCommand)
            throw new IOException("The injected proof received an unexpected native vector.");
        if (request.Arguments.Contains(ToolCatalogueDefinitions.DryRunArgument)) return Result(string.Empty);
        var expected = entry.Profiles[0].Uninstall.Arguments.Select(value => ToolRecipeExpansion.Expand(value, Context));
        if (!request.Arguments.SequenceEqual(expected) || request.Environment.Count != 0 || request.WorkingDirectory != Context.WorkspacePath)
            throw new IOException("The account uninstall vector or environment differs from its native recipe.");
        Uninstalls.Add(request);
        NativeSawProjectBlock = File.ReadAllText(Path.Combine(Context.WorkspacePath, CodebaseMemoryDefinitions.IgnorePath))
            .Contains(CodebaseMemoryDefinitions.IgnoreStartMarker, StringComparison.Ordinal);
        File.Delete(NativeFile);
        File.Delete(Binary);
        NativePathEffectSimulated = true;
        return Result(string.Empty);
    }

    public Task<ToolNpmMetadataResult> ReadNpmMetadataAsync(ToolNpmMetadataRequest request, CancellationToken cancellationToken)
        => throw new IOException("The injected removal proof permits no network.");

    public Task<ToolDownloadResult> DownloadAsync(ToolDownloadRequest request, CancellationToken cancellationToken)
        => throw new IOException("The injected removal proof permits no download.");

    private static Task<ToolProcessResult> Result(string output) => Task.FromResult(new ToolProcessResult
    {
        ExitCode = 0,
        StandardOutput = output,
        StandardError = string.Empty,
        TimedOut = false,
        Cancelled = false,
    });
}
