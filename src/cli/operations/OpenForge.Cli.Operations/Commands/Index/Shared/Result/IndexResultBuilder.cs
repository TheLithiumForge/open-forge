using OpenForge.Cli.Core.Commands.Index.Models.Binding;
using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Result;

internal sealed class IndexResultBuilder
{
    internal IndexResult Create(IndexOperationOutcome outcome)
    {
        return Create(new IndexResultFormation
        {
            Workspace = outcome.Request.Workspace,
            Mode = outcome.Request.Mode,
            Selection = outcome.Selection,
            Regions = outcome.Regions,
            Recovery = outcome.Recovery,
            Findings = outcome.Findings,
        });
    }

    internal IndexResult Create(IndexResultFormation formation)
        => new(formation);

    internal IndexResult CreateInvalid(
        IndexBindingInput input,
        CliWorkspace? workspace,
        IEnumerable<IndexFinding> findings)
    {
        var origin = input.SourceReferences.Count == 0
            ? IndexSelectionOrigin.AutomaticLoader
            : IndexSelectionOrigin.ExplicitSources;
        return Create(
            new IndexResultFormation
            {
                Workspace = workspace,
                Mode = input.Mode,
                Selection = IndexSelection.NotEstablished(origin),
                Regions = [],
                Recovery = IndexRecovery.NotRequired,
                Findings = findings,
            });
    }
}
