namespace OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.Result;

internal sealed record WorkspaceAdoptionDerivation
{
    internal WorkspaceAdoptionDerivation(
        string field,
        WorkspaceAdoptionDerivationSource source)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        if (!Enum.IsDefined(source))
        {
            throw new ArgumentOutOfRangeException(
                nameof(source),
                source,
                "The workspace adoption derivation source is not defined.");
        }

        Field = field;
        Source = source;
    }

    internal string Field { get; }

    internal WorkspaceAdoptionDerivationSource Source { get; }
}
