using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;

internal sealed record ExtensionRemovePathPlan
{
    internal ExtensionRemovePathPlan(
        string path,
        ExtensionRemovePathClassification classification,
        IEnumerable<string> selectedOwnerIds,
        IEnumerable<string> remainingOwnerIds,
        ExtensionRemovePathAction action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(selectedOwnerIds);
        ArgumentNullException.ThrowIfNull(remainingOwnerIds);
        if (!Enum.IsDefined(classification))
        {
            throw new ArgumentOutOfRangeException(
                nameof(classification),
                classification,
                "The Extension Remove path classification is not defined.");
        }

        if (!Enum.IsDefined(action))
        {
            throw new ArgumentOutOfRangeException(
                nameof(action),
                action,
                "The Extension Remove path action is not defined.");
        }

        var selected = SnapshotOwnerIds(selectedOwnerIds, nameof(selectedOwnerIds));
        var remaining = SnapshotOwnerIds(remainingOwnerIds, nameof(remainingOwnerIds));
        if (selected.Count == 0)
        {
            throw new ArgumentException(
                "Extension Remove path plans require a selected owner.",
                nameof(selectedOwnerIds));
        }

        if (selected.Any(remaining.Contains))
        {
            throw new ArgumentException(
                "Extension Remove path owners cannot be both selected and remaining.",
                nameof(remainingOwnerIds));
        }

        ValidateAction(classification, remaining.Count, action);

        Path = path;
        Classification = classification;
        SelectedOwnerIds = selected;
        RemainingOwnerIds = remaining;
        Action = action;
    }

    internal string Path { get; }

    internal ExtensionRemovePathClassification Classification { get; }

    internal IReadOnlyList<string> SelectedOwnerIds { get; }

    internal IReadOnlyList<string> RemainingOwnerIds { get; }

    internal ExtensionRemovePathAction Action { get; }

    private static void ValidateAction(
        ExtensionRemovePathClassification classification,
        int remainingOwnerCount,
        ExtensionRemovePathAction action)
    {
        var valid = classification switch
        {
            ExtensionRemovePathClassification.Shared =>
                remainingOwnerCount > 0 && action == ExtensionRemovePathAction.RetainShared,
            ExtensionRemovePathClassification.UnchangedFinalOwner =>
                remainingOwnerCount == 0 && action == ExtensionRemovePathAction.Delete,
            ExtensionRemovePathClassification.ChangedFinalOwner =>
                remainingOwnerCount == 0
                && action is ExtensionRemovePathAction.Delete
                    or ExtensionRemovePathAction.KeepAsUnmanaged,
            ExtensionRemovePathClassification.Missing =>
                remainingOwnerCount == 0 && action == ExtensionRemovePathAction.ReleaseOwnership,
            _ => throw new ArgumentOutOfRangeException(
                nameof(classification),
                classification,
                "The Extension Remove path classification is not defined."),
        };

        if (!valid)
        {
            throw new ArgumentException(
                "Extension Remove path classification and action do not agree.",
                nameof(action));
        }
    }

    private static IReadOnlyList<string> SnapshotOwnerIds(
        IEnumerable<string> values,
        string parameterName)
    {
        var result = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var value in values)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
            if (!seen.Add(value))
            {
                throw new ArgumentException(
                    "Extension Remove owner IDs must be unique.",
                    parameterName);
            }

            result.Add(value);
        }

        result.Sort(StringComparer.Ordinal);
        return new ReadOnlyCollection<string>(result.ToArray());
    }
}
