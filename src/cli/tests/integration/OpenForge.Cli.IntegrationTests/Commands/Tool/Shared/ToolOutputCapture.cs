using System.Runtime.CompilerServices;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Shared;

/// <summary>
/// One Tool situation: a fresh project prepared the same way for every capture, then one command line.
/// </summary>
internal sealed record ToolScenario(string Situation, string[] Arguments)
{
    public Func<ToolCommandFixture> Create { get; init; } = static () => new ToolCommandFixture();
    public Func<ToolCommandFixture, Task>? Arrange { get; init; }

    /// <summary>Line answers for an interactive run. JSON never prompts, so an interactive situation captures text only.</summary>
    public string? Answers { get; init; }
}

/// <summary>What the minimal text capture left behind, so a test can assert effects on the same run its snapshot shows.</summary>
internal sealed record ToolScenarioRun(ToolCommandFixture Fixture, ToolCommandRun Run, IReadOnlyDictionary<string, string> Before) : IDisposable
{
    public void Dispose() => Fixture.Dispose();
}

/// <summary>
/// Captures every detail level as text and JSON, like the other command output snapshots. Tool commands change the
/// project, so each capture gets its own fresh project instead of sharing one.
/// </summary>
internal static class ToolOutputCapture
{
    internal static async Task<ToolScenarioRun> MatchAsync(ToolScenario scenario, [CallerFilePath] string sourceFile = "")
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        ToolScenarioRun? first = null;
        var formats = scenario.Answers is null ? new[] { false, true } : [false];
        foreach (var detail in CommandOutputDetailVocabulary.All)
        {
            var detailName = CommandOutputDetailVocabulary.Name(detail);
            foreach (var json in formats)
            {
                var fixture = scenario.Create();
                try
                {
                    if (scenario.Arrange is { } arrange) await arrange(fixture);
                    fixture.Processes.Commands.Clear();
                    var before = fixture.Files();
                    var run = await fixture.RunAsync(
                        [.. scenario.Arguments, CliSyntaxDefinitions.Detail.Name, detailName, .. json ? ToolCommandFixture.Json : []], scenario.Answers);
                    var key = $"{scenario.Situation}{(json ? CommandOutputSnapshot.JsonContentNameSegment : ".")}{detailName}";
                    var primaryIsOutput = json || run.ExitCode is 0 or 2 or 3;
                    var primary = primaryIsOutput ? run.Output : run.Error;
                    var secondary = primaryIsOutput ? run.Error : run.Output;
                    snapshots[key] = json ? CommandOutputSnapshotFormatting.PrettyPrintJson(primary) : primary;
                    if (detail == CliDetail.Debug && secondary.Length > 0) snapshots[$"{key}.diagnostics"] = secondary;
                    if (first is null)
                    {
                        first = new(fixture, run, before);
                        continue;
                    }
                }
                catch
                {
                    fixture.Dispose();
                    throw;
                }
                fixture.Dispose();
            }
        }
        SplitTextDebug(snapshots, scenario.Situation);
        try
        {
            CommandOutputSnapshot.MatchDetailSnapshot(snapshots, sourceFile, scenario.Situation);
        }
        catch
        {
            first?.Dispose();
            throw;
        }
        return first ?? throw new InvalidOperationException("A scenario needs at least one capture.");
    }

    /// <summary>The prompt transcript of an interactive run. It is not a command report, so it is stored as content.</summary>
    internal static void MatchPrompt(string prompts, string name, string owner, [CallerFilePath] string sourceFile = "")
        => CommandOutputSnapshot.MatchSnapshot(prompts, $"{name}.content", sourceFile, owner);

    // Text on stderr carries debug diagnostics after the full report. Keep the report and the diagnostics apart,
    // as the shared read-command capture does.
    private static void SplitTextDebug(Dictionary<string, string> snapshots, string situation)
    {
        var fullKey = $"{situation}.{CommandOutputDetailVocabulary.Name(CliDetail.Full)}";
        var debugKey = $"{situation}.{CommandOutputDetailVocabulary.Name(CliDetail.Debug)}";
        if (!snapshots.TryGetValue(fullKey, out var full) || !snapshots.TryGetValue(debugKey, out var debug)) return;
        var expected = CommandOutputDetailComparison.PromoteTextRetryDetail(full);
        if (debug == expected || !debug.StartsWith(expected, StringComparison.Ordinal)) return;
        var diagnostics = debug[expected.Length..].TrimEnd('\r', '\n');
        snapshots[debugKey] = expected;
        if (diagnostics.Length > 0) snapshots[$"{debugKey}.diagnostics"] = diagnostics;
    }
}
