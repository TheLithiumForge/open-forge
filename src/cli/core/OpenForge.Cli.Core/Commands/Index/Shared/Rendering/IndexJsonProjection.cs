using OpenForge.Cli.Core.Commands.Index.Models.Operation;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;
using OpenForge.Cli.Core.Commands.Index.Models.Presentation;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Commands.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Rendering;

internal static class IndexJsonProjection
{
    internal static IndexJsonDocument Create(IndexResult result)
    {
        CliOperationStage.ValidateResult(result);
        return new IndexJsonDocument
        {
            SchemaVersion = IndexDefinitions.SchemaVersion,
            Command = result.Command,
            Status = CliStatusDefinitions.Read(result.Status).MachineName,
            Workspace = result.Workspace is null ? null : Workspace(result.Workspace),
            Result = new IndexJsonResult
            {
                Mode = IndexDefinitions.ReadMachineName(result.Mode),
                Selection = Selection(result.Selection),
                Regions = result.Regions.Select(Region).ToArray(),
                Recovery = Recovery(result.Recovery),
                Findings = result.Findings.Select(Finding).ToArray(),
                Counts = Counts(result.Counts),
            },
            Next = result.Next is null
                ? null
                : new IndexJsonNext
                {
                    Command = result.Next.Command,
                    Reason = result.Next.Reason,
                },
        };
    }

    private static IndexJsonWorkspace Workspace(CliWorkspace workspace)
        => new()
        {
            Path = workspace.LexicalRoot,
            SelectedBy = WorkspaceSelectionWireVocabulary.Read(workspace.SelectedBy),
        };

    private static IndexJsonSelection Selection(IndexSelection selection)
        => new()
        {
            Origin = IndexDefinitions.ReadMachineName(selection.Origin),
            Scope = IndexDefinitions.ReadMachineName(selection.Scope),
            Sources = selection.Sources.Select(Source).ToArray(),
        };

    private static IndexJsonLogicalSource Source(IndexLogicalSource source)
        => new()
        {
            Id = source.Id,
            Path = source.Path,
            Scope = IndexDefinitions.ReadMachineName(source.Scope),
        };

    private static IndexJsonRegion Region(IndexRegion region)
        => new()
        {
            Source = Source(region.Source),
            Action = IndexDefinitions.ReadMachineName(region.Action),
            BeforeEntryCount = region.BeforeEntryCount,
            ExpectedEntryCount = region.ExpectedEntryCount,
            Change = region.Change is null
                ? null
                : new IndexJsonChange
                {
                    BeforeBody = region.Change.BeforeBody,
                    ExpectedBody = region.Change.ExpectedBody,
                },
            Outcome = IndexDefinitions.ReadMachineName(region.Outcome),
        };

    private static IndexJsonRecovery Recovery(IndexRecovery recovery)
        => new()
        {
            State = IndexDefinitions.ReadMachineName(recovery.State),
            ResidualPath = recovery.ResidualPath,
        };

    private static IndexJsonFinding Finding(IndexFinding finding)
        => new()
        {
            Code = IndexDefinitions.ReadMachineName(finding.Code),
            Status = CliStatusDefinitions.Read(finding.Status).MachineName,
            SourceOccurrence = finding.SourceOccurrence,
            Source = finding.Source is null ? null : Source(finding.Source),
            Cause = finding.Cause,
            Candidates = finding.Candidates.Select(Source).ToArray(),
        };

    private static IndexJsonCounts Counts(IndexCounts counts)
        => new()
        {
            Regions = counts.Regions,
            Updates = counts.Updates,
            Unchanged = counts.Unchanged,
            Applied = counts.Applied,
            Verified = counts.Verified,
        };
}
