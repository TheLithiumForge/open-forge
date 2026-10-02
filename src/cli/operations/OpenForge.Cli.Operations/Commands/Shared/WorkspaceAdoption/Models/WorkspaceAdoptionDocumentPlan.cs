using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.Result;
namespace OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;

internal sealed record WorkspaceAdoptionDocumentPlan
{
    internal WorkspaceAdoptionDocumentPlan(
        byte[]? intendedBytes,
        IEnumerable<WorkspaceAdoptionAction> actions,
        IEnumerable<string> fields,
        IEnumerable<WorkspaceAdoptionDerivation> derivation,
        string? cause)
    {
        ArgumentNullException.ThrowIfNull(actions);
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentNullException.ThrowIfNull(derivation);

        var actionValues = actions.ToArray();
        var fieldValues = fields
            .Select(field => field ?? throw new ArgumentException(
                "Workspace adoption fields cannot contain null members.",
                nameof(fields)))
            .ToArray();
        var derivationValues = derivation
            .Select(value => value ?? throw new ArgumentException(
                "Workspace adoption derivation cannot contain null members.",
                nameof(derivation)))
            .ToArray();
        cause = string.IsNullOrWhiteSpace(cause) ? null : cause;

        if ((intendedBytes is null) != (cause is not null))
        {
            throw new ArgumentException(
                "A blocked workspace adoption plan must have a cause and no intended bytes.",
                nameof(cause));
        }

        if (intendedBytes is null
            && (actionValues.Length != 0 || fieldValues.Length != 0 || derivationValues.Length != 0))
        {
            throw new ArgumentException(
                "A blocked workspace adoption plan cannot retain partial actions or fields.",
                nameof(actions));
        }

        if (actionValues.Any(action => !Enum.IsDefined(action)))
        {
            throw new ArgumentOutOfRangeException(nameof(actions), "The plan contains an undefined action.");
        }

        IntendedBytes = intendedBytes?.ToArray();
        Actions = Array.AsReadOnly(actionValues);
        Fields = Array.AsReadOnly(fieldValues);
        Derivation = Array.AsReadOnly(derivationValues);
        Cause = cause;
    }

    internal byte[]? IntendedBytes { get; }

    internal IReadOnlyList<WorkspaceAdoptionAction> Actions { get; }

    internal IReadOnlyList<string> Fields { get; }

    internal IReadOnlyList<WorkspaceAdoptionDerivation> Derivation { get; }

    internal string? Cause { get; }
}
