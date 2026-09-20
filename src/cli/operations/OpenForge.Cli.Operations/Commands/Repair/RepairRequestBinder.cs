using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics.CodeAnalysis;
using OpenForge.Cli.Core.Commands.Repair.Models.Binding;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Request;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;

namespace OpenForge.Cli.Core.Commands.Repair;

internal sealed class RepairRequestBinder(RepairSymbols symbols)
{
    private readonly RepairSymbols _symbols = symbols;

    internal CliBindResult<RepairRequest, RepairResult> Bind(
        CliBindingParse parse,
        CliInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(parse);
        ArgumentNullException.ThrowIfNull(invocation);
        var input = ReadInput(parse.Result);
        if (!TryReadRelinks(input.RelinkValues, out var relinks, out var invalid))
        {
            return CliBindResult<RepairRequest, RepairResult>.Invalid(
                RepairResult.Empty(
                    invocation.Workspace,
                    input.Mode,
                    input.Automatic,
                    RepairSelectionMode.NonInteractiveBlocked,
                    invalid));
        }

        if (invocation.Workspace is not { } workspace)
        {
            return CliBindResult<RepairRequest, RepairResult>.Invalid(
                RepairResult.Empty(
                    null,
                    input.Mode,
                    input.Automatic,
                    RepairSelectionMode.NonInteractiveBlocked,
                    new RepairFinding(
                        RepairFindingCode.DiagnosisBlocked,
                        "The selected workspace is unavailable.")));
        }

        var allowInteraction = invocation.Presentation.Format == CliFormat.Text
            && !input.Automatic
            && relinks.Count == 0;
        try
        {
            return CliBindResult<RepairRequest, RepairResult>.Bound(
                new RepairRequest(workspace, input.Mode, input.Automatic, relinks, allowInteraction));
        }
        catch (ArgumentException exception)
        {
            var code = exception.Message.Contains("contradict", StringComparison.OrdinalIgnoreCase)
                ? RepairFindingCode.ContradictoryRelink
                : RepairFindingCode.RelinkInvalid;
            return CliBindResult<RepairRequest, RepairResult>.Invalid(
                RepairResult.Empty(
                    workspace,
                    input.Mode,
                    input.Automatic,
                    RepairSelectionMode.NonInteractiveBlocked,
                    new RepairFinding(code, exception.Message)));
        }
    }

    internal RepairBindingInput ReadInput(ParseResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new RepairBindingInput(
            result.GetValue(_symbols.DryRun)
                ? RepairMode.DryRun
                : RepairMode.Apply,
            result.GetValue(_symbols.Automatic),
            CliOptionResultFactsReader.ReadValues(result, _symbols.Relink));
    }


    private static bool TryReadRelinks(
        IReadOnlyList<string> values,
        out IReadOnlyList<RepairRelinkRequest> relinks,
        [NotNullWhen(false)] out RepairFinding? invalid)
    {
        ArgumentNullException.ThrowIfNull(values);
        if (values.Count % 3 != 0)
        {
            relinks = [];
            invalid = new RepairFinding(
                RepairFindingCode.RelinkInvalid,
                "Each --relink occurrence requires exactly source-location, expected-destination, and target-path values.");
            return false;
        }

        var parsed = new List<RepairRelinkRequest>();
        for (var index = 0; index < values.Count; index += 3)
        {
            try
            {
                parsed.Add(RepairRelinkRequest.Parse(values[index], values[index + 1], values[index + 2]));
            }
            catch (ArgumentException exception)
            {
                relinks = [];
                invalid = new RepairFinding(RepairFindingCode.RelinkInvalid, exception.Message);
                return false;
            }
        }

        var distinct = RepairRelinkNormalizer.Distinct(parsed);
        if (RepairRelinkNormalizer.Contradiction(distinct) is { } contradiction)
        {
            relinks = [];
            invalid = new RepairFinding(
                RepairFindingCode.ContradictoryRelink,
                $"Two --relink values name the link at {contradiction.SourceLocation.SourceCanonicalPath}"
                    + $":{contradiction.SourceLocation.Line}:{contradiction.SourceLocation.Column}"
                    + " with different targets.",
                contradiction.SourceLocation.SourceCanonicalPath);
            return false;
        }

        relinks = distinct;
        invalid = null;
        return true;
    }
}
