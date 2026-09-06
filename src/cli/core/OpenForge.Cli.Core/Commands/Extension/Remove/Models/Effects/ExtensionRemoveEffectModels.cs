using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Effects;

internal sealed record ExtensionRemoveEffect
{
    internal ExtensionRemoveEffect(
        string path,
        string? packageId,
        ExtensionRemoveEffectKind kind,
        ExtensionRemoveEffectAction action,
        ExtensionRemoveEffectOutcome outcome,
        ExtensionRemoveEffectResidual residual)
    {
        if (!IsCanonicalRelative(path))
        {
            throw new ArgumentException(
                "Extension Remove effect paths must be canonical workspace-relative paths.",
                nameof(path));
        }

        if (packageId is not null && string.IsNullOrWhiteSpace(packageId))
        {
            throw new ArgumentException(
                "An Extension Remove effect package ID cannot be blank.",
                nameof(packageId));
        }

        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The Extension Remove effect kind is not defined.");
        }

        if (!Enum.IsDefined(action))
        {
            throw new ArgumentOutOfRangeException(
                nameof(action),
                action,
                "The Extension Remove effect action is not defined.");
        }

        if (!Enum.IsDefined(outcome))
        {
            throw new ArgumentOutOfRangeException(
                nameof(outcome),
                outcome,
                "The Extension Remove effect outcome is not defined.");
        }

        if (!Enum.IsDefined(residual))
        {
            throw new ArgumentOutOfRangeException(
                nameof(residual),
                residual,
                "The Extension Remove effect residual is not defined.");
        }

        Path = path;
        PackageId = packageId;
        Kind = kind;
        Action = action;
        Outcome = outcome;
        Residual = residual;
    }

    internal string Path { get; }

    internal string? PackageId { get; }

    internal ExtensionRemoveEffectKind Kind { get; }

    internal ExtensionRemoveEffectAction Action { get; }

    internal ExtensionRemoveEffectOutcome Outcome { get; }

    internal ExtensionRemoveEffectResidual Residual { get; }

    private static bool IsCanonicalRelative(string value)
        => !string.IsNullOrWhiteSpace(value)
            && !value.StartsWith('/')
            && !IsDriveQualified(value)
            && !value.Contains('\\')
            && value.Split('/', StringSplitOptions.None).All(segment => segment.Length != 0
                && segment != "."
                && segment != ".."
                && segment.All(character => !char.IsControl(character)));

    private static bool IsDriveQualified(string value)
        => value.Length >= 2
            && char.IsAsciiLetter(value[0])
            && value[1] == ':';
}
