using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using System.Text.Json;
using OpenForge.Cli.Composition.Shared.Tools;
using OpenForge.Cli.Core.Framework.Tools.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Commands.Tool;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Binding;
using OpenForge.Cli.Core.Framework.Tools.Catalogue.Shared.CodebaseMemory;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.WindowsJourneys;

internal static class ManualAccountRemovalProof
{
    internal static async Task<int> DiagnoseProjectOnlyAsync(string root)
    {
        root = Path.GetFullPath(root);
        if (!root.StartsWith("D:\\of-task90-e-final\\", StringComparison.OrdinalIgnoreCase)) throw new IOException("Unowned diagnostic root.");
        var environmentFile = Path.Combine(root, CodebaseMemoryDefinitions.Id, "environment.json");
        using var environmentJson = JsonDocument.Parse(File.ReadAllBytes(environmentFile));
        var environment = environmentJson.RootElement.EnumerateObject().ToDictionary(value => value.Name, value => value.Value.GetString());
        var catalogue = ToolCatalogueFactory.Create(ToolHostFactory.CreateNodeRuntimes());
        var process = new ManualUninstallPreviewProcesses(ToolHostFactory.CreateProcesses(environment, catalogue));
        return await ManualToolJourneyHost.RunAsync(
            [root, CodebaseMemoryDefinitions.Id, ToolDefinitions.Group.Name, ToolWriteBinding.Name(ToolWriteKind.Remove),
                CodebaseMemoryDefinitions.Id, ToolWriteBinding.YesOption, CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat],
            injectedProcesses: process, answers: null);
    }

    // This separate owned fixture simulates native effects. It never runs the actual uninstaller.
    internal static async Task<int> RunAsync(string root)
    {
        root = Path.GetFullPath(root);
        if (!root.StartsWith("D:\\of-task90-e-final\\", StringComparison.OrdinalIgnoreCase)) throw new IOException("Unowned injected proof root.");
        var location = Path.Combine(root, CodebaseMemoryDefinitions.Id);
        var workspace = Path.Combine(location, "project");
        var account = Path.Combine(location, "account");
        var bin = Path.Combine(account, ToolRecordDefinitions.HomeDirectoryName, ToolRecordDefinitions.BinDirectoryName);
        Directory.CreateDirectory(workspace);
        Directory.CreateDirectory(bin);
        Directory.CreateDirectory(Path.Combine(location, "runtime"));
        Directory.CreateDirectory(Path.Combine(location, "temp"));
        var binary = Path.Combine(bin, CodebaseMemoryDefinitions.WindowsBinary);
        File.WriteAllText(binary, "inert injected native boundary\n");
        var ignore = Path.Combine(workspace, CodebaseMemoryDefinitions.IgnorePath);
        const string original = "# Owned project exclusions\n/generated/\n";
        File.WriteAllText(ignore, original);
        var index = Path.Combine(account, "owned-index-sentinel");
        File.WriteAllText(index, "keep the owned index\n");
        var indexBefore = File.ReadAllBytes(index);
        var process = new ManualAccountRemovalProcesses(location);
        var install = await ManualToolJourneyHost.RunAsync(
            [root, CodebaseMemoryDefinitions.Id, ToolDefinitions.Group.Name, ToolWriteBinding.Name(ToolWriteKind.Install),
                CodebaseMemoryDefinitions.Id, ToolWriteBinding.YesOption, CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat],
            injectedProcesses: process, answers: null);
        if (install != 0) throw new InvalidOperationException("The injected fixture setup failed.");
        var remove = await ManualToolJourneyHost.RunAsync(
            [root, CodebaseMemoryDefinitions.Id, ToolDefinitions.Group.Name, ToolWriteBinding.Name(ToolWriteKind.Remove), CodebaseMemoryDefinitions.Id],
            injectedProcesses: process, answers: "y\n2\n");
        if (remove != 0 || process.Uninstalls.Count != 1 || !process.NativeSawProjectBlock || !process.NativePathEffectSimulated
            || File.Exists(binary) || File.ReadAllText(ignore) != original || !File.ReadAllBytes(index).SequenceEqual(indexBefore))
            throw new InvalidOperationException("The injected account-removal operation did not complete its declared effects.");
        using var stream = File.Create(Path.Combine(location, "account-removal-receipt.json"));
        using var json = new Utf8JsonWriter(stream, new() { Indented = true });
        json.WriteStartObject();
        json.WriteString("kind", "injected-production-operation");
        json.WriteBoolean("realNativeUninstall", false);
        json.WriteBoolean("realPathOrRegistryEdit", false);
        json.WriteString("nativeVersionProbe", "injected boundary, not a native compatibility check");
        json.WriteNumber("installExit", install);
        json.WriteNumber("removeExit", remove);
        json.WriteNumber("uninstallCalls", process.Uninstalls.Count);
        json.WriteBoolean("nativeBeforeProjectCleanup", process.NativeSawProjectBlock);
        json.WriteBoolean("indexKept", true);
        json.WriteString("executable", process.Uninstalls[0].ExecutablePath);
        json.WriteStartArray("arguments");
        foreach (var value in process.Uninstalls[0].Arguments) json.WriteStringValue(value);
        json.WriteEndArray();
        json.WriteEndObject();
        return 0;
    }
}

// Diagnoses remaining planning after substituting only the unsafe native preview. Apply stays forbidden.
internal sealed class ManualUninstallPreviewProcesses(IToolProcessHost real) : IToolProcessHost
{
    public Task<ToolResolvedCommand?> ResolveAsync(string command, string workingDirectory, CancellationToken cancellationToken)
        => real.ResolveAsync(command, workingDirectory, cancellationToken);

    public Task<ToolProcessResult> RunAsync(ToolProcessRequest request, CancellationToken cancellationToken)
    {
        if (request.Arguments.FirstOrDefault() != ToolCatalogueDefinitions.UninstallCommand) return real.RunAsync(request, cancellationToken);
        if (!request.Arguments.Contains(ToolCatalogueDefinitions.DryRunArgument)) throw new IOException("Actual native account uninstall is forbidden.");
        Console.Error.WriteLine("Injected native uninstall preview. HKCU was not accessed.");
        return Task.FromResult(new ToolProcessResult
        {
            ExitCode = 0,
            StandardOutput = string.Empty,
            StandardError = string.Empty,
            TimedOut = false,
            Cancelled = false,
        });
    }
}
