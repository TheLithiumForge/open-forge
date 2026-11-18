using OpenForge.Cli.Composition;
using OpenForge.Cli.Composition.Models;
using OpenForge.Cli.Core.Commands.Tool.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Hosting;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Shared;

internal sealed record ToolCommandRun(int ExitCode, string Output, string Error, string Prompts);

/// <summary>
/// One owned workspace and one composed CLI with a fake process host. The platform is pinned so the catalogue's
/// commands, and so the snapshots, are the same on every operating system.
/// </summary>
internal sealed class ToolCommandFixture : IDisposable
{
    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("open-forge-tool-");
    private const string ProjectFolder = "project";
    private readonly WorkspaceLockTestStore _locks = WorkspaceLockTestStore.Create("tool-command-locks");

    internal ToolCommandFixture(ToolPlatform platform = ToolPlatform.Unix, bool openForge = true)
    {
        Platform = platform;
        Catalogue = ToolCatalogue.Create(platform);
        Processes = new ToolTestProcessHost(Catalogue);
        // The CLI creates and deletes files of its own, so the fixture owns the whole folder and deletes it recursively.
        Path = Directory.CreateDirectory(System.IO.Path.Combine(_root.FullName, ProjectFolder)).FullName;
        if (openForge) Directory.CreateDirectory(Native(WorkspaceSettingsDefinitions.DirectoryName));
        _ = _locks.Track(new CliWorkspace(Path, Path, CliWorkspaceSelectionMethod.CurrentDirectory));
    }

    internal ToolPlatform Platform { get; }
    internal ToolCatalogue Catalogue { get; }
    internal ToolTestProcessHost Processes { get; }
    internal string Path { get; }
    internal ToolEntry CodeGraph => Catalogue.Find(CodeGraphEntry.Id)!;
    internal ToolEntry CodebaseMemory => Catalogue.Find(CodebaseMemoryEntry.Id)!;

    internal static string[] Install(string? id, params string[] options)
        => [ToolCommandDefinitions.Group.Name, ToolCommandDefinitions.InstallName, .. id is null ? Array.Empty<string>() : [id], .. options];

    internal static string[] Remove(string id, params string[] options)
        => [ToolCommandDefinitions.Group.Name, ToolCommandDefinitions.RemoveName, id, .. options];

    internal static string[] List(params string[] options) => [ToolCommandDefinitions.Group.Name, ToolCommandDefinitions.ListName, .. options];

    internal static string[] Json => [CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat];

    internal static string[] Standard => [CliSyntaxDefinitions.Detail.Name, CliPresentationDefinitions.Standard];

    internal static int Exit(CliSemanticStatus status) => CliStatusDefinitions.Read(status).Disposition.ExitCode;

    internal string? Read(string relativePath)
    {
        var path = Native(relativePath);
        return File.Exists(path) ? File.ReadAllText(path) : null;
    }

    internal void Write(string relativePath, string contents)
    {
        var path = Native(relativePath);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        File.WriteAllText(path, contents);
    }

    /// <summary>Every project file with its bytes, so a comparison proves nothing was created, changed or deleted.</summary>
    internal IReadOnlyDictionary<string, string> Files()
        => Directory.EnumerateFiles(Path, "*", SearchOption.AllDirectories)
            .Order(StringComparer.Ordinal)
            .ToDictionary(path => System.IO.Path.GetRelativePath(Path, path), path => Convert.ToHexString(File.ReadAllBytes(path)), StringComparer.Ordinal);

    private string Native(string relativePath) => System.IO.Path.Combine(Path, relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar));

    internal string[] ConfiguredTools()
    {
        var path = Native(WorkspaceSettingsDefinitions.RelativePath);
        var bytes = File.Exists(path) ? File.ReadAllBytes(path) : [];
        return bytes.Length == 0 ? [] : [.. WorkspaceSettingsCodec.Read(bytes).Document!.Tools];
    }

    /// <summary>Runs the CLI. With answers it is interactive and reads them line by line, as a terminal without key reading does.</summary>
    internal async Task<ToolCommandRun> RunAsync(string[] arguments, string? answers = null)
    {
        using var input = new StringReader(answers ?? string.Empty);
        using var output = new StringWriter();
        using var error = new StringWriter();
        using var prompts = new StringWriter();
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity(CliSyntaxDefinitions.ExecutableName, CliBuildVersion.InformationalVersion),
            new CliCompositionInputs
            {
                StandardInput = input,
                PromptOutput = prompts,
                StandardInputRedirected = answers is null,
                PromptOutputRedirected = answers is null,
                LockStoreRoot = _locks.StoreRoot,
                ToolProcesses = Processes,
                ToolPlatform = Platform,
            });
        var completion = await application.RunAsync(arguments, new CliProcessEnvironment(Path), new CliOutputWriters(output, error),
            TestContext.Current.CancellationToken);
        return new(completion.ExitCode, Normalize(output.ToString()), Normalize(error.ToString()), Normalize(prompts.ToString()));
    }

    private string Normalize(string content)
        => CommandOutputNormalization.Normalize(content, Path, CliBuildVersion.InformationalVersion);

    public void Dispose()
    {
        _locks.Dispose();
        _root.Delete(recursive: true);
    }
}
