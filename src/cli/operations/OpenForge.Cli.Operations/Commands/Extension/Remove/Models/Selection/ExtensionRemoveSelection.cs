using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Selection;

internal enum ExtensionRemoveSelectionKind
{
    ExplicitIds,
    InteractiveIds,
}

internal sealed record ExtensionRemoveSelection
{
    internal ExtensionRemoveSelection(
        ExtensionRemoveSelectionKind selectedBy,
        IEnumerable<string> ids)
    {
        if (!Enum.IsDefined(selectedBy))
        {
            throw new ArgumentOutOfRangeException(
                nameof(selectedBy),
                selectedBy,
                "The Extension Remove selection kind is not defined.");
        }

        ArgumentNullException.ThrowIfNull(ids);
        var values = ids
            .Select(value =>
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(ids));
                return value;
            })
            .ToArray();
        if (values.Length == 0)
        {
            throw new ArgumentException(
                "An Extension Remove selection requires at least one ID.",
                nameof(ids));
        }

        if (values.Distinct(StringComparer.Ordinal).Count() != values.Length)
        {
            throw new ArgumentException(
                "Extension Remove selection IDs must be unique.",
                nameof(ids));
        }

        SelectedBy = selectedBy;
        Ids = new ReadOnlyCollection<string>(values);
    }

    internal ExtensionRemoveSelectionKind SelectedBy { get; }

    internal IReadOnlyList<string> Ids { get; }
}
