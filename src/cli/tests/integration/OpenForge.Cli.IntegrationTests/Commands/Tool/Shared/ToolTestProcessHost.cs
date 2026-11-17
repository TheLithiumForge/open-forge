using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Shared;

/// <summary>
/// Answers Tool commands without running anything, because real installs reach the network and the account. Every
/// command succeeds unless a test says otherwise, except detection, which reports the tool as not installed.
/// </summary>
internal sealed class ToolTestProcessHost(ToolCatalogue catalogue) : IToolProcessHost
{
    internal const string FailureOutput = "synthetic failure line one\nsynthetic failure line two";
    internal const int FailureExitCode = 7;

    private readonly Dictionary<string, ToolProcessResult> _results = new(StringComparer.Ordinal);

    internal List<string> Commands { get; } = [];

    internal void Installed(ToolEntry entry)
        => _results[entry.Detection.Text] = new() { Started = true, ExitCode = 0, OutputTail = entry.TestedVersion };

    internal void Missing(ToolCommand command) => _results[command.Text] = new() { Started = false };

    internal void Fail(ToolCommand command) => _results[command.Text] = new() { Started = true, ExitCode = FailureExitCode, OutputTail = FailureOutput };

    /// <summary>The commands that changed something, leaving out detection and requirement probes.</summary>
    internal IEnumerable<string> Effects => Commands.Where(command => !Reads.Contains(command));

    private HashSet<string> Reads => [.. catalogue.Entries.Select(entry => entry.Detection.Text),
        .. Enum.GetValues<ToolRequirement>().Select(requirement => ToolDefinitions.Probe(requirement).Text)];

    public Task<ToolProcessResult> RunAsync(ToolProcessRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        Commands.Add(request.Command.Text);
        if (_results.TryGetValue(request.Command.Text, out var result)) return Task.FromResult(result);
        var detection = catalogue.Entries.Any(entry => entry.Detection.Text == request.Command.Text);
        return Task.FromResult(detection ? new ToolProcessResult { Started = false } : new ToolProcessResult { Started = true, ExitCode = 0 });
    }
}
