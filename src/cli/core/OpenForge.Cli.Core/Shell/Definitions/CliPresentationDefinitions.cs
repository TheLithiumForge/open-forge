namespace OpenForge.Cli.Core.Shell.Definitions;

internal enum CliOutputFormat
{
    Human,
    Json,
}

internal enum CliView
{
    Compact,
    Expanded,
}

internal enum CliVerbosity
{
    Normal,
    Verbose,
}

internal enum CliOutputTarget
{
    StandardOutput,
    StandardError,
}

internal sealed record CliPresentation(
    CliOutputFormat Format,
    CliView View,
    CliVerbosity Verbosity);

internal static class CliPresentationDefinitions
{
    internal const string Compact = "compact";
    internal const string Expanded = "expanded";

    internal static CliView ParseView(string value)
    {
        return value switch
        {
            Compact => CliView.Compact,
            Expanded => CliView.Expanded,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "The view is not defined."),
        };
    }

    internal static void Validate(CliPresentation presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        ValidateEnum(presentation.Format, nameof(presentation.Format));
        ValidateEnum(presentation.View, nameof(presentation.View));
        ValidateEnum(presentation.Verbosity, nameof(presentation.Verbosity));
    }

    internal static void ValidateTarget(CliOutputTarget target)
    {
        ValidateEnum(target, nameof(target));
    }

    internal static void ValidateOutputPolicy(
        CliSemanticStatus status,
        CliOutputFormat format,
        CliOutputTarget target)
    {
        var statusDefinition = CliStatusDefinitions.Read(status);
        ValidateEnum(format, nameof(format));
        ValidateTarget(target);
        var expected = format switch
        {
            CliOutputFormat.Human => statusDefinition.Disposition.HumanOutputTarget,
            CliOutputFormat.Json => CliOutputTarget.StandardOutput,
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "The output format is not defined."),
        };
        if (target != expected)
        {
            throw new ArgumentException(
                $"The {format} output target does not match the {statusDefinition.MachineName} status policy.",
                nameof(target));
        }
    }

    private static void ValidateEnum<T>(T value, string name)
        where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(name, value, $"The {typeof(T).Name} value is not defined.");
        }
    }
}
