using System.Collections.Immutable;
using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Install.Models.Binding;
using OpenForge.Cli.Core.Commands.Install.Models.Operation;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Shared.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Install.Models.Result;

internal sealed record InstallResult : ICliCommandResult
{
    private static readonly ImmutableArray<CliSemanticStatus> StatusPrecedence =
    [
        CliSemanticStatus.Failed,
        CliSemanticStatus.Interrupted,
        CliSemanticStatus.Invalid,
        CliSemanticStatus.Blocked,
        CliSemanticStatus.Incomplete,
        CliSemanticStatus.Attention,
    ];

    internal InstallResult(
        CliWorkspace? workspace,
        InstallBindingInput input,
        IEnumerable<InstallFinding> findings,
        InstallOperationSummary? summary = null,
        InstallResultFacts? facts = null)
    {
        var materialized = findings
            .Select(finding => finding ?? throw new ArgumentException(
                "Install findings cannot contain null members.",
                nameof(findings)))
            .OrderBy(finding => finding.Code)
            .ThenBy(finding => finding.Subject, StringComparer.Ordinal)
            .ThenBy(finding => finding.Cause, StringComparer.Ordinal)
            .ToArray();

        Workspace = workspace;
        Input = input;
        Findings = new ReadOnlyCollection<InstallFinding>(materialized);
        Status = ReadStatus(materialized);
        Next = InstallDefinitions.ReadNextAction(Status, Findings, input);
        Summary = summary;
        Facts = facts ?? (summary is null
            ? InstallResultFactsFactory.Empty()
            : InstallResultFactsFactory.FromSummary(summary));
    }

    public string Command => InstallDefinitions.CommandIdentity;

    public CliSemanticStatus Status { get; }

    public CliWorkspace? Workspace { get; }

    public CliNextAction? Next { get; }

    internal InstallBindingInput Input { get; }

    internal InstallMode Mode => Input.Mode;

    internal bool Force => Input.Force;

    internal bool Automatic => Input.Automatic;

    internal IReadOnlyList<InstallFinding> Findings { get; }

    internal InstallOperationSummary? Summary { get; }

    internal InstallResultFacts Facts { get; }

    internal static InstallResult Invalid(
        InstallBindingInput input,
        CliWorkspace? workspace,
        IEnumerable<InstallFinding> findings)
    {
        return new InstallResult(
            workspace: workspace,
            input: input,
            findings: findings);
    }

    internal static InstallResult Create(
        InstallRequest request,
        IEnumerable<InstallFinding> findings,
        InstallOperationSummary summary)
    {
        return new InstallResult(
            workspace: request.Workspace,
            input: new InstallBindingInput(
                Force: request.Force,
                Automatic: request.Automatic,
                Mode: request.Mode),
            findings: findings,
            summary: summary);
    }

    internal static InstallResult Create(
        InstallRequest request,
        IEnumerable<InstallFinding> findings,
        InstallOperationSummary summary,
        InstallResultFacts facts)
    {
        return new InstallResult(
            workspace: request.Workspace,
            input: new InstallBindingInput(
                Force: request.Force,
                Automatic: request.Automatic,
                Mode: request.Mode),
            findings: findings,
            summary: summary,
            facts: facts);
    }

    private static CliSemanticStatus ReadStatus(
        IReadOnlyList<InstallFinding> findings)
    {
        return StatusPrecedence.FirstOrDefault(
            status => findings.Any(finding => finding.Status == status),
            CliSemanticStatus.Complete);
    }
}
