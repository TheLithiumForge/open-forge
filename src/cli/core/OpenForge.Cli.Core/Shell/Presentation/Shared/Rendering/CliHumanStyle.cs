using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Shell.Presentation.Shared.Rendering;

internal sealed class CliHumanStyle
{
    private const string DefaultForeground = "\u001b[39m";
    private const string Green = "\u001b[32m";
    private const string Yellow = "\u001b[33m";
    private const string Red = "\u001b[31m";
    private const string Cyan = "\u001b[36m";
    private readonly bool _enabled;

    private CliHumanStyle(bool enabled) => _enabled = enabled;

    internal static CliHumanStyle Plain { get; } = new(false);
    internal static CliHumanStyle Color { get; } = new(true);

    internal static CliHumanStyle For<TResult>(CliPresentationRequest<TResult> request)
        where TResult : ICliCommandResult
    {
        if (request.Presentation.Format == CliOutputFormat.Json)
        {
            return Plain;
        }

        var target = CliStatusDefinitions.Read(request.Result.Status).Disposition.HumanOutputTarget;
        var enabled = target switch
        {
            CliOutputTarget.StandardOutput => request.Presentation.Colors.StandardOutput,
            CliOutputTarget.StandardError => request.Presentation.Colors.StandardError,
            _ => throw new ArgumentOutOfRangeException(nameof(request), target, "The output target is not defined."),
        };
        return enabled ? Color : Plain;
    }

    internal string Status(string label, CliSemanticStatus status)
        => Apply(label, status switch
        {
            CliSemanticStatus.Complete => Green,
            CliSemanticStatus.Attention or CliSemanticStatus.Incomplete or CliSemanticStatus.Interrupted => Yellow,
            CliSemanticStatus.Invalid or CliSemanticStatus.Blocked or CliSemanticStatus.Failed => Red,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The status is not defined."),
        });

    internal string Finding(CliSemanticStatus status) => Status(CliHumanText.Status(status).ToUpperInvariant(), status);

    internal string Information(string label) => Apply(label, Cyan);
    internal string Warning(string label) => Apply(label, Yellow);
    internal string Error(string label) => Apply(label, Red);

    private string Apply(string label, string color)
        => _enabled ? $"{color}{label}{DefaultForeground}" : label;
}
