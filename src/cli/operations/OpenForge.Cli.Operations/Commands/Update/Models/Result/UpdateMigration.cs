using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models.Result;
using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Shared.WorkspaceAdoption.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;

namespace OpenForge.Cli.Core.Commands.Update.Models.Result;

internal enum UpdateMigrationOutcome
{
    Planned,
    Applied,
}

internal sealed record UpdateMigration
{
    internal UpdateMigration(
        string path,
        IEnumerable<WorkspaceAdoptionAction> actions,
        IEnumerable<string> fields,
        IEnumerable<WorkspaceAdoptionDerivation> derivation,
        UpdateMigrationOutcome outcome)
    {
        if (!PortableWorkspacePath.TryNormalize(path, out var normalizedPath)
            || !string.Equals(path, normalizedPath, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Update migration paths must be canonical workspace-relative paths.",
                nameof(path));
        }

        ArgumentNullException.ThrowIfNull(actions);
        ArgumentNullException.ThrowIfNull(fields);
        ArgumentNullException.ThrowIfNull(derivation);

        var actionValues = actions.ToArray();
        if (actionValues.Length == 0 || actionValues.Any(action => !Enum.IsDefined(action)))
        {
            throw new ArgumentException(
                "An Update migration must contain defined actions.",
                nameof(actions));
        }

        var fieldValues = fields
            .Select(field => string.IsNullOrWhiteSpace(field)
                ? throw new ArgumentException(
                    "Update migration fields cannot be blank.",
                    nameof(fields))
                : field)
            .ToArray();
        var derivationValues = derivation
            .Select(value => value ?? throw new ArgumentException(
                "Update migration derivation cannot contain null members.",
                nameof(derivation)))
            .ToArray();
        _ = outcome switch
        {
            UpdateMigrationOutcome.Planned or UpdateMigrationOutcome.Applied => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(outcome),
                outcome,
                "The Update migration outcome is not defined."),
        };

        Path = path;
        Actions = new ReadOnlyCollection<WorkspaceAdoptionAction>(actionValues);
        Fields = new ReadOnlyCollection<string>(fieldValues);
        Derivation = new ReadOnlyCollection<WorkspaceAdoptionDerivation>(derivationValues);
        Outcome = outcome;
    }

    internal string Path { get; }

    internal IReadOnlyList<WorkspaceAdoptionAction> Actions { get; }

    internal IReadOnlyList<string> Fields { get; }

    internal IReadOnlyList<WorkspaceAdoptionDerivation> Derivation { get; }

    internal UpdateMigrationOutcome Outcome { get; }

    internal bool IsUserOwnedSource { get; init; }
}
