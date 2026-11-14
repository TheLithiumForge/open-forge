using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Binding;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Application;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Binding;

internal static class ToolWriteRequestBinder
{
    internal static CliBindResult<ToolWriteRequest, ToolWriteResult> Bind(
        ToolWriteSymbols symbols, ToolCatalogue catalogue, IToolVersionService versions, CliBindingParse parse, CliInvocation invocation)
    {
        var repeatedChannel = symbols.Channel is { } channelOption && CliOptionResultFactsReader.Read(parse.Result, channelOption).IdentifierCount > 1;
        if (parse.Result.Errors.Count != 0 || repeatedChannel)
            return CliBindResult<ToolWriteRequest, ToolWriteResult>.Invalid(ToolWriteResults.Invalid(
                symbols.Kind, new(ToolWriteFindingCode.InvalidInput, CliSemanticStatus.Invalid), invocation.Workspace));
        var raw = parse.Result.GetValue(symbols.Spec);
        var spec = raw is null ? null : versions.ReadSpec(raw);
        var channel = symbols.Channel is { } option ? parse.Result.GetValue(option) : null;
        var allow = CliOptionResultFactsReader.ReadValues(parse.Result, symbols.AllowPath);
        if (raw is not null && spec is null
            || channel is not null && (spec is null || string.IsNullOrWhiteSpace(channel))
            || allow.Any(string.IsNullOrWhiteSpace)
            || symbols.Kind == ToolWriteKind.Remove && (spec is null || raw != spec.Id))
        {
            return CliBindResult<ToolWriteRequest, ToolWriteResult>.Invalid(ToolWriteResults.Invalid(
                symbols.Kind, new(ToolWriteFindingCode.InvalidInput, CliSemanticStatus.Invalid), invocation.Workspace));
        }
        if (spec is not null)
        {
            if (!catalogue.Entries.TryGetValue(spec.Id, out var entry)
                || !string.Equals(spec.Id, entry.Identity.Id, StringComparison.Ordinal))
                return CliBindResult<ToolWriteRequest, ToolWriteResult>.Invalid(ToolWriteResults.Invalid(
                    symbols.Kind, new(ToolWriteFindingCode.UnknownTool, CliSemanticStatus.Invalid, spec.Id), invocation.Workspace));
            if (channel is not null && !entry.Channels.Any(candidate => string.Equals(candidate.Id, channel, StringComparison.Ordinal)))
                return CliBindResult<ToolWriteRequest, ToolWriteResult>.Invalid(ToolWriteResults.Invalid(
                    symbols.Kind, new(ToolWriteFindingCode.ChannelInvalid, CliSemanticStatus.Invalid, spec.Id), invocation.Workspace));
        }
        if (invocation.Workspace is not { } workspace)
            return CliBindResult<ToolWriteRequest, ToolWriteResult>.Invalid(ToolWriteResults.Invalid(
                symbols.Kind, new(ToolWriteFindingCode.RecordsUnavailable, CliSemanticStatus.Blocked)));
        return CliBindResult<ToolWriteRequest, ToolWriteResult>.Bound(new()
        {
            Kind = symbols.Kind,
            Workspace = workspace,
            Spec = spec,
            Channel = channel,
            Yes = parse.Result.GetValue(symbols.Yes),
            Preview = parse.Result.GetValue(symbols.DryRun),
            Format = invocation.Presentation.Format,
            CanPrompt = invocation.Presentation.Format == CliFormat.Text,
            AllowPaths = [.. allow],
        });
    }

    internal static ToolWriteResult Invalid(ToolWriteKind kind, CliInvalidBindingInput input)
    {
        var status = input.InvalidInput.Source == CliInvalidInputSource.Workspace ? CliSemanticStatus.Blocked : CliSemanticStatus.Invalid;
        return ToolWriteResults.Invalid(kind, new(ToolWriteFindingCode.InvalidInput, status,
            Cause: string.Join("\n", input.InvalidInput.Diagnostics)));
    }
}
