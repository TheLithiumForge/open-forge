using System.Runtime.CompilerServices;
using System.Diagnostics;
using OpenForge.Cli.Core.Commands.Tool.Run;
using OpenForge.Cli.Core.Commands.Tool.Run.Models;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Shared.Storage;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Hosting.Shared.Tools;
using OpenForge.Cli.TestSupport.Tools.Run;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Run.Shared;

internal static class ToolRunProbe
{
    internal const string Argument = "--open-forge-tool-run-probe";
    internal const string HostMode = "host";
    internal const string ChildMode = "child";
    internal const string DescendantMode = "descendant";
    internal const string ExecutableName = "OpenForge.Cli.IntegrationTests";
    internal static string Executable => Path.Combine(AppContext.BaseDirectory, ExecutableName + (OperatingSystem.IsWindows() ? ".exe" : ""));

    internal static Process Start(ToolRunFixture fixture, string[] arguments)
    {
        var start = new ProcessStartInfo(Executable)
        {
            UseShellExecute = false,
            WorkingDirectory = fixture.Cwd,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var argument in new[] { Argument, HostMode, fixture.Temporary.Combine(ToolRunFixture.AccountDirectory), fixture.ManifestPath,
            CliSyntaxDefinitions.Workspace.Name, fixture.Workspace.LexicalRoot }.Concat(arguments)) start.ArgumentList.Add(argument);
        start.Environment.Remove(ToolRunRequest.HostProjectDirectoryVariable);
        start.Environment[ToolRunChild.WitnessVariable] = Path.Combine(fixture.Workspace.LexicalRoot, ToolRunChild.Witness);
        return Process.Start(start) ?? throw new InvalidOperationException("The fixture host didn't start.");
    }

    [ModuleInitializer]
    internal static void Initialize()
    {
        var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
        if (args.FirstOrDefault() != Argument) return;
        try { Environment.Exit(Run(args)); }
        catch (Exception exception) { Console.Error.WriteLine(exception); Environment.Exit(1); }
    }

    private static int Run(string[] args)
    {
        if (args[1] == HostMode)
        {
            var bytes = File.ReadAllBytes(args[3]);
            var definition = new ToolCatalogueReader().Definition(bytes, args[3], false);
            var manifest = definition.Manifest;
            var reader = new ToolCatalogueReader(new ToolReleaseSupport(
                [new(manifest.Id, manifest.Version, definition.Sha256, manifest.Adapter.Revision)], [new(manifest.Adapter.Id, manifest.Adapter.Revision)]));
            var operation = new ToolRunOperation(reader, ToolStoreLocator.FromRoot(args[2]));
            var input = ToolRunRequestParser.Parse(args[4..], new(Environment.CurrentDirectory,
                Environment.GetEnvironmentVariable(ToolRunRequest.HostProjectDirectoryVariable), false));
            if (input.Request is not { } request) return 4;
            // Await only code in other modules. Our module initializer hasn't returned yet.
            return ToolRunHost.RunAsync(request, new(operation.ResolveAsync), Console.Error, CancellationToken.None).AsTask().GetAwaiter().GetResult();
        }
        if (args[1] == DescendantMode) { Thread.Sleep(Timeout.Infinite); return 0; }
        if (args[1] != ChildMode) throw new ArgumentException("Unknown fixture mode.");
        return ToolRunChild.Run(args[2..], Environment.ProcessPath!, [Argument, DescendantMode]);
    }
}
