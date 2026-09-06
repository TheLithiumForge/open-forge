using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Models.Selection;

internal enum ExtensionUpdateSelectionKind
{
    ExplicitIds,
    ExplicitAll,
    SinglePackageInference,
    InteractiveIds,
    InteractiveAll,
}

internal sealed record ExtensionUpdateSelection
{
    internal ExtensionUpdateSelection(
        ExtensionUpdateSelectionKind selectedBy,
        IEnumerable<string> rootIds)
    {
        if (!Enum.IsDefined(selectedBy))
        {
            throw new ArgumentOutOfRangeException(
                nameof(selectedBy),
                selectedBy,
                "The Extension Update selection kind is not defined.");
        }

        ArgumentNullException.ThrowIfNull(rootIds);
        var values = rootIds
            .Select(value => value ?? throw new ArgumentException(
                "Extension Update selection IDs cannot contain null members.",
                nameof(rootIds)))
            .ToArray();
        if (values.Any(string.IsNullOrWhiteSpace)
            || values.Distinct(StringComparer.Ordinal).Count() != values.Length)
        {
            throw new ArgumentException(
                "Extension Update selection IDs must be unique and non-empty.",
                nameof(rootIds));
        }

        if (selectedBy is ExtensionUpdateSelectionKind.ExplicitAll
            or ExtensionUpdateSelectionKind.InteractiveAll
            && values.Length != 0)
        {
            throw new ArgumentException(
                "An all-package Extension Update selection cannot contain root IDs.",
                nameof(rootIds));
        }

        SelectedBy = selectedBy;
        RootIds = new ReadOnlyCollection<string>(values);
    }

    internal ExtensionUpdateSelectionKind SelectedBy { get; }

    internal IReadOnlyList<string> RootIds { get; }
}
