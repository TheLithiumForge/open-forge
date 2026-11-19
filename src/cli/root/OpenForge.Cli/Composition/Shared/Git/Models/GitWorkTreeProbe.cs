namespace OpenForge.Cli.Composition.Shared.Git.Models;

internal enum GitWorkTreeState
{
    Unavailable,
    Outside,
    Inside,
}

/// <summary>The work-tree answer, the exit code and every stdout line, so a combined rev-parse can read its later lines.</summary>
internal sealed record GitWorkTreeProbe(GitWorkTreeState State, int ExitCode, IReadOnlyList<string> Lines);
