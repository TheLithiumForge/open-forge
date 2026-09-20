using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Repair.Models.Diagnosis;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Models.Selection;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Interaction;

internal sealed record RepairInteractionFacts
{
    internal RepairInteractionFacts(
        RepairRequest request,
        RepairDiagnosisCoverage coverage,
        RepairCatalogueRead catalogue,
        ImmutableArray<RepairLibraryRecoveryProposal> libraryProposals,
        IReadOnlyList<RepairFinding> initialFindings)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(coverage);
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(initialFindings);
        if (libraryProposals.IsDefault)
        {
            throw new ArgumentException("Repair interaction facts require initialized Library proposals.", nameof(libraryProposals));
        }

        Request = request;
        Coverage = coverage;
        Catalogue = catalogue;
        LibraryProposals = libraryProposals;
        InitialFindings = initialFindings;
    }

    internal RepairRequest Request { get; }

    internal RepairDiagnosisCoverage Coverage { get; }

    internal RepairCatalogueRead Catalogue { get; }

    internal ImmutableArray<RepairLibraryRecoveryProposal> LibraryProposals { get; }

    internal IReadOnlyList<RepairFinding> InitialFindings { get; }
}
