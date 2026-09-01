using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;

namespace OpenForge.Cli.Core.Commands.Find.Models.Operation;

internal sealed record FindUniverseInput
{
    internal FindUniverseInput(FindRequest request, SourceReadSession sourceSession)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(sourceSession);
        Request = request;
        SourceSession = sourceSession;
    }

    internal FindRequest Request { get; }

    internal SourceReadSession SourceSession { get; }
}

internal sealed record FindRouteFactsInput
{
    internal FindRouteFactsInput(
        SourceReadSession sourceSession,
        SourceCatalogueSelection selection)
    {
        ArgumentNullException.ThrowIfNull(sourceSession);
        ArgumentNullException.ThrowIfNull(selection);
        SourceSession = sourceSession;
        Selection = selection;
    }

    internal SourceReadSession SourceSession { get; }

    internal SourceCatalogueSelection Selection { get; }
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
