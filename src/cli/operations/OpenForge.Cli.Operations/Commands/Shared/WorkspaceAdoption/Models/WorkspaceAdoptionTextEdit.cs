namespace OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;

internal sealed record WorkspaceAdoptionTextEdit
{
    internal WorkspaceAdoptionTextEdit(int start, int length, string replacement)
    {
        if (start < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(start));
        }

        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        ArgumentNullException.ThrowIfNull(replacement);
        Start = start;
        Length = length;
        Replacement = replacement;
    }

    internal int Start { get; }

    internal int Length { get; }

    internal string Replacement { get; }
}
