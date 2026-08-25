using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Find.Models.Result;

internal sealed partial record FindResult : ICliCommandResult
{
    internal FindResult(
        CliSemanticStatus status,
        CliWorkspace? workspace,
        FindUniverse universe,
        FindQuery query,
        FindPresentationSelection presentation,
        FindCoverage coverage,
        IEnumerable<FindFinding> findings,
        IEnumerable<FindMatch> matches,
        CliNextAction? next)
    {
        _ = CliStatusDefinitions.Read(status);
        ArgumentNullException.ThrowIfNull(universe);
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(presentation);
        ArgumentNullException.ThrowIfNull(coverage);
        ArgumentNullException.ThrowIfNull(findings);
        ArgumentNullException.ThrowIfNull(matches);
        var materializedFindings = findings.ToArray();
        var materializedMatches = matches.ToArray();
        if (materializedFindings.Any(finding => finding is null)
            || materializedMatches.Any(match => match is null))
        {
            throw new ArgumentException("Find results cannot contain null findings or matches.");
        }

        if (status == CliSemanticStatus.Complete && materializedFindings.Length != 0)
        {
            throw new ArgumentException("A complete Find result cannot contain findings.", nameof(findings));
        }

        if (status is CliSemanticStatus.Invalid or CliSemanticStatus.Blocked
            && materializedMatches.Length != 0)
        {
            throw new ArgumentException("A pre-operation Find result cannot contain matches.", nameof(matches));
        }

        ValidateStatusAndCoverage(status, coverage, presentation, materializedFindings);
        ValidateNext(status, materializedFindings, next);
        ValidateMatches(universe, query, presentation.Content, materializedMatches);
        ValidateProjectionStates(coverage, presentation.Content, materializedFindings, materializedMatches);
        if (workspace is null
            && (materializedMatches.Length != 0
                || status is not (CliSemanticStatus.Invalid or CliSemanticStatus.Blocked)))
        {
            throw new ArgumentException("A Find result without workspace identity cannot carry normal matches.", nameof(workspace));
        }

        Status = status;
        Workspace = workspace;
        Universe = universe;
        Query = query;
        Presentation = presentation;
        Coverage = coverage;
        Findings = Array.AsReadOnly(materializedFindings);
        Matches = Array.AsReadOnly(materializedMatches);
        Next = next;
    }

    public string Command => FindDefinitions.CommandIdentity;

    public CliSemanticStatus Status { get; }

    public CliWorkspace? Workspace { get; }

    internal FindUniverse Universe { get; }

    internal FindQuery Query { get; }

    internal FindPresentationSelection Presentation { get; }

    internal FindCoverage Coverage { get; }

    internal IReadOnlyList<FindFinding> Findings { get; }

    internal IReadOnlyList<FindMatch> Matches { get; }

    public CliNextAction? Next { get; }
}
