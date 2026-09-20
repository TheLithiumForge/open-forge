using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Shell.Definitions;

internal enum CliFormat { Text, Json }
internal enum CliOutputTarget { StandardOutput, StandardError }

internal static class CliPresentationDefinitions
{
    internal const int MaximumDiagnosticLength = 4096;

    internal const string Minimal = "minimal";
    internal const string Standard = "standard";
    internal const string Full = "full";
    internal const string Debug = "debug";

    internal static void Validate(CliPresentation presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        ValidateEnum(presentation.Format, nameof(presentation.Format));
        ValidateEnum(presentation.Detail, nameof(presentation.Detail));
        if (presentation.Filter is { } filter)
        {
            foreach (var severity in filter) ValidateEnum(severity, nameof(presentation.Filter));
        }
    }

    internal static void ValidateTarget(CliOutputTarget target) => ValidateEnum(target, nameof(target));

    internal static void ValidateOutputPolicy(CliSemanticStatus status, CliFormat format, CliOutputTarget target)
    {
        var statusDefinition = CliStatusDefinitions.Read(status);
        ValidateEnum(format, nameof(format));
        ValidateTarget(target);
        var expected = format switch
        {
            CliFormat.Text => statusDefinition.Disposition.HumanOutputTarget,
            CliFormat.Json => CliOutputTarget.StandardOutput,
            _ => throw new ArgumentOutOfRangeException(nameof(format)),
        };
        if (target != expected)
        {
            throw new ArgumentException($"The {format} output target does not match the {statusDefinition.MachineName} status policy.", nameof(target));
        }
    }

    private static void ValidateEnum<T>(T value, string name) where T : struct, Enum
    {
        if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(name, value, "The value is not defined.");
    }
}
