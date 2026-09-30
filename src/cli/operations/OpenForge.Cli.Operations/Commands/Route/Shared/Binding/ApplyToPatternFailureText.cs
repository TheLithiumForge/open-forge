using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Binding;

internal static class ApplyToPatternFailureText
{
    internal static string ReadProblemClause(ApplyToPatternFailure failure)
        => failure switch
        {
            ApplyToPatternFailure.Empty => "--apply-to <glob> must contain a non-empty pattern with no empty path segments",
            ApplyToPatternFailure.AbsolutePath => "--apply-to <glob> must be workspace-relative",
            ApplyToPatternFailure.Traversal => "--apply-to <glob> must not contain . or .. path segments",
            ApplyToPatternFailure.UnsupportedSyntax => "--apply-to <glob> contains unsupported pattern syntax",
            _ => throw new ArgumentOutOfRangeException(nameof(failure), failure, "The applyTo pattern failure is not defined."),
        };

    internal static string? ReadHint(ApplyToPatternFailure failure)
        => failure == ApplyToPatternFailure.Empty
            ? "Use docs/** to match files under docs/."
            : null;

    internal static string ReadMessage(ApplyToPatternFailure failure)
    {
        var sentence = $"{ReadProblemClause(failure)}.";
        return ReadHint(failure) is { } hint
            ? $"{sentence} {hint}"
            : sentence;
    }
}
