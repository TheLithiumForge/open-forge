using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Shared.Text;

internal sealed class CliTextStyle
{
    private readonly bool _enabled;
    private CliTextStyle(bool enabled) => _enabled = enabled;
    internal static CliTextStyle Plain { get; } = new(false);
    internal static CliTextStyle Color { get; } = new(true);

    internal static CliTextStyle For(CliSemanticStatus status, CliOutputColors colors, bool json = false)
    {
        if (json)
        {
            return Plain;
        }

        var target = CliStatusDefinitions.Read(status).Disposition.HumanOutputTarget;
        var enabled = target switch
        {
            CliOutputTarget.StandardOutput => colors.StandardOutput,
            CliOutputTarget.StandardError => colors.StandardError,
            _ => throw new ArgumentOutOfRangeException(nameof(status)),
        };
        return enabled ? Color : Plain;
    }

    internal string Severity(string label, CliSeverity severity) => Foreground(label, severity switch
    {
        CliSeverity.Error => 31,
        CliSeverity.Warning => 33,
        CliSeverity.Info => 36,
        _ => throw new ArgumentOutOfRangeException(nameof(severity)),
    });

    internal string Status(string label, CliSemanticStatus status) => status switch
    {
        CliSemanticStatus.Complete => label,
        CliSemanticStatus.Attention or CliSemanticStatus.Incomplete => Warning(label),
        CliSemanticStatus.Invalid or CliSemanticStatus.Blocked
            or CliSemanticStatus.Failed or CliSemanticStatus.Interrupted => Error(label),
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };

    internal string Subject(string value) => _enabled ? $"\u001b[1m{value}\u001b[22m" : value;
    internal string Dim(string value) => _enabled ? $"\u001b[2m{value}\u001b[22m" : value;
    internal string Information(string value) => Foreground(value, 36);
    internal string Warning(string value) => Foreground(value, 33);
    internal string Error(string value) => Foreground(value, 31);
    private string Foreground(string value, int color) => _enabled ? $"\u001b[{color}m{value}\u001b[39m" : value;
}
