using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;

namespace OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;

internal sealed record CommandOutputCaptureRun(
    CliProcessCompletion Completion,
    string StandardOutput,
    string StandardError,
    string PromptOutput,
    bool PrimaryIsStandardOutput,
    string? PrimaryContentOverride = null,
    string? DiagnosticContentOverride = null)
{
    internal string PrimaryContent => PrimaryContentOverride
        ?? (PrimaryIsStandardOutput ? StandardOutput : StandardError);

    internal string SecondaryContent => PrimaryIsStandardOutput ? StandardError : StandardOutput;

    internal string DiagnosticContent => DiagnosticContentOverride
        ?? (PrimaryIsStandardOutput ? StandardError : string.Empty);
}
