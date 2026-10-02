namespace OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;

internal sealed record WorkspaceAdoptionPendingField
{
    internal WorkspaceAdoptionPendingField(string field, string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field);
        ArgumentNullException.ThrowIfNull(value);
        Field = field;
        Value = value;
    }

    internal string Field { get; }

    internal string Value { get; }
}
