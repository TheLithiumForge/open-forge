using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Shared.Models.Mutation;
using OpenForge.Cli.Core.Commands.Library;
using OpenForge.Cli.Core.Commands.Library.Detach;
using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Commands.Stripping.Models.Planning;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Stripping;

internal static class StripResultFactory
{

    internal static StripResult Invalid(StripCommandKind kind, CliWorkspace? workspace, string cause)
        => new()
        {
            Kind = kind,
            Status = CliSemanticStatus.Invalid,
            Workspace = workspace,
            Findings = [Error(StripFindingCode.InvalidInput, cause)],
            Next = new CliNextAction(Help(kind), "Correct the named input and retry."),
        };

    internal static StripResult Stop(StripRequest request, CliSemanticStatus status, StripFinding finding)
        => Stop(request, status, [finding], []);

    internal static StripResult Stop(StripRequest request, CliSemanticStatus status, ImmutableArray<StripFinding> findings, ImmutableArray<StripCollision> collisions)
        => new()
        {
            Kind = request.Command,
            Status = status,
            Workspace = request.Workspace,
            DryRun = request.DryRun,
            Parts = request.Parts,
            Flatten = request.Flatten,
            Collisions = collisions,
            Findings = findings,
            Next = NextFor(request, findings[0]),
        };

    internal static StripResult Preview(StripPlan plan)
        => new()
        {
            Kind = plan.Request.Command,
            Status = CliSemanticStatus.Complete,
            Workspace = plan.Request.Workspace,
            DryRun = plan.Request.DryRun,
            Parts = plan.Request.Parts,
            Flatten = plan.Request.Flatten,
            Plan = plan.Entries,
            Kept = plan.Kept,
            Findings = plan.Findings,
            Verified = !plan.Request.DryRun && plan.IsNoOp,
            Next = plan.Request.DryRun && !plan.IsNoOp
                ? new CliNextAction(RetryCommand(plan.Request, automatic: plan.Request.Automatic, dryRun: false), "Apply this plan.") { AppliesRequest = true }
                : null,
        };

    internal static StripResult Refuse(StripPlan plan, CliSemanticStatus status, StripFinding finding)
        => Preview(plan) with
        {
            Status = status,
            Findings = [finding, .. plan.Findings],
            Verified = false,
            Next = NextFor(plan.Request, finding),
        };

    internal static StripFinding Error(StripFindingCode code, string message, string? path = null, WorkspaceLockFailureInfo? lockFailure = null)
        => new() { Code = code, Severity = StripFindingSeverity.Error, Message = message, Path = path, LockFailure = lockFailure };

    internal static StripFinding Warning(StripFindingCode code, string message, string? path = null)
        => new() { Code = code, Severity = StripFindingSeverity.Warning, Message = message, Path = path };

    internal static StripFinding Info(StripFindingCode code, string message, string? path = null)
        => new() { Code = code, Severity = StripFindingSeverity.Info, Message = message, Path = path };

    internal static string RetryCommand(StripRequest request, bool automatic, bool dryRun)
    {
        var arguments = new List<string> { StrippingDefinitions.CommandIdentity(request.Command) };
        foreach (var part in request.Parts.Select(StrippingDefinitions.PartName))
        {
            if (request.Command == StripCommandKind.Strip) arguments.Add(part);
            else CliRetryCommand.Add(arguments, StrippingDefinitions.StripOption, part);
        }

        CliRetryCommand.Add(arguments, StrippingDefinitions.FlattenOption, request.Flatten);
        foreach (var path in request.RemoveEdited) CliRetryCommand.Add(arguments, StrippingDefinitions.RemoveEditedOption, path);
        if (request.Workspace.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace)
            CliRetryCommand.Add(arguments, CliSyntaxDefinitions.Workspace.Name, request.Workspace.LexicalRoot);
        if (automatic) arguments.Add(StrippingDefinitions.AutomaticOption);
        if (dryRun) arguments.Add(StrippingDefinitions.DryRunOption);
        CliRetryCommand.AddPresentation(arguments, request.RetryOptions);
        return CliRetryCommand.Build(arguments);
    }

    private static string Help(StripCommandKind kind)
        => $"{CliSyntaxDefinitions.ExecutableName} {StrippingDefinitions.CommandIdentity(kind)} {CliSyntaxDefinitions.Help.Name}";

    internal static CliNextAction? NextFor(StripRequest request, StripFinding finding)
        => finding.Code switch
        {
            StripFindingCode.ConfirmationRequired => new CliNextAction(RetryCommand(request, automatic: true, dryRun: false), "Apply the reviewed plan without a prompt.") { AppliesRequest = true },
            StripFindingCode.TargetChanged or StripFindingCode.Interrupted
                => new CliNextAction(RetryCommand(request, automatic: request.Automatic, dryRun: request.DryRun), "Run the same command again."),
            StripFindingCode.WriteFailed or StripFindingCode.TargetChangedDuringApply
                => new CliNextAction(CommandLines.Doctor, "Check the workspace before changing it again."),
            StripFindingCode.RecoveryArtifactRetained
                => new CliNextAction(CommandLines.Cleanup, "Review the retained recovery data."),
            StripFindingCode.LibraryAttached when finding.Identifier is { } library
                => new CliNextAction(LibraryDetach(library), "Detach the Library, then run uninstall again."),
            StripFindingCode.InvalidInput => new CliNextAction(Help(request.Command), "Correct the named input and retry."),
            _ => null,
        };

    internal static string LibraryDetach(string library)
        => CliRetryCommand.Build([LibraryDefinitions.Group.Name, LibraryDetachDefinitions.Command.Name, library]);

    // The shared invalid-input family: Cannot <command>: <the exact input problem>.
    internal static string Cannot(StripCommandKind kind, string problem)
        => $"Cannot {StrippingDefinitions.CommandIdentity(kind)}: {problem}";
}
