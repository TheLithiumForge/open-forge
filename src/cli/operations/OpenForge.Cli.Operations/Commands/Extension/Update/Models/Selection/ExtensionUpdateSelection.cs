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
        : this(selectedBy, rootIds, frozenIds: null)
    {
    }

    internal ExtensionUpdateSelection(
        ExtensionUpdateSelectionKind selectedBy,
        IEnumerable<string> rootIds,
        IEnumerable<string>? frozenIds)
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
        FrozenIds = frozenIds is null
            ? null
            : new ReadOnlyCollection<string>(SnapshotIds(frozenIds, nameof(frozenIds)));
    }

    internal ExtensionUpdateSelectionKind SelectedBy { get; }

    internal IReadOnlyList<string> RootIds { get; }

    /// <summary>
    /// The resolved IDs captured for an all-package choice. All-package
    /// selections keep an empty public root set for result compatibility, so
    /// this internal snapshot carries the exact IDs through revalidation.
    /// </summary>
    internal IReadOnlyList<string>? FrozenIds { get; }

    private static string[] SnapshotIds(
        IEnumerable<string> ids,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(ids, parameterName);
        var values = ids
            .Select(value => value ?? throw new ArgumentException(
                "Extension Update selection IDs cannot contain null members.",
                parameterName))
            .ToArray();
        if (values.Any(string.IsNullOrWhiteSpace)
            || values.Distinct(StringComparer.Ordinal).Count() != values.Length)
        {
            throw new ArgumentException(
                "Extension Update selection IDs must be unique and non-empty.",
                parameterName);
        }

        return values;
    }
}
