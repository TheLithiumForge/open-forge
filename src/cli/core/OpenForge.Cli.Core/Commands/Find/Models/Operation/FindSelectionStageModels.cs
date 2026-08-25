using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Commands.Find.Models.Operation;

internal sealed record FindSourceReadContext
{
    internal FindSourceReadContext(
        SourceCatalogue catalogue,
        SourceDocumentReader documentReader,
        SourceCatalogueSelectionScope? defaultSelectionScope)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(documentReader);
        Catalogue = catalogue;
        DocumentReader = documentReader;
        DefaultSelectionScope = defaultSelectionScope;
    }

    internal SourceCatalogue Catalogue { get; }

    internal SourceDocumentReader DocumentReader { get; }

    internal SourceCatalogueSelectionScope? DefaultSelectionScope { get; }
}

internal sealed record FindUniverseInput
{
    internal FindUniverseInput(FindRequest request, FindSourceReadContext sourceContext)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(sourceContext);
        Request = request;
        SourceContext = sourceContext;
    }

    internal FindRequest Request { get; }

    internal FindSourceReadContext SourceContext { get; }
}

internal sealed record FindUniverseResolution
{
    internal FindUniverseResolution(
        FindUniverse universe,
        SourceCatalogueSelection selection,
        IEnumerable<FindFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(universe);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(findings);
        var materializedFindings = findings.ToArray();
        if (materializedFindings.Any(finding => finding is null))
        {
            throw new ArgumentException("Find universe findings cannot contain null values.", nameof(findings));
        }

        Universe = universe;
        Selection = selection;
        Findings = Array.AsReadOnly(materializedFindings);
    }

    internal FindUniverse Universe { get; }

    internal SourceCatalogueSelection Selection { get; }

    internal IReadOnlyList<FindFinding> Findings { get; }
}
