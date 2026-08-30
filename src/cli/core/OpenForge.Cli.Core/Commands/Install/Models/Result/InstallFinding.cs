using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Install.Models.Result;

internal sealed record InstallFinding
{
    private const int MaximumCauseLength = 256;

    internal InstallFinding(
        InstallFindingCode code,
        string cause,
        string? subject = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        if (subject is not null && subject.Length == 0)
        {
            throw new ArgumentException(
                "An Install finding subject cannot be empty.",
                nameof(subject));
        }

        Code = code;
        Status = InstallDefinitions.ReadStatus(code);
        Cause = cause.Length <= MaximumCauseLength
            ? cause
            : cause[..MaximumCauseLength];
        Subject = subject;
    }

    internal InstallFindingCode Code { get; }

    internal CliSemanticStatus Status { get; }

    internal string Cause { get; }

    internal string? Subject { get; }
}
