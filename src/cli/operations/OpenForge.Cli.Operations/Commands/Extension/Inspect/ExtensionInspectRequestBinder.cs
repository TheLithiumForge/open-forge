using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;
using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect;

internal sealed class ExtensionInspectRequestBinder(ExtensionInspectSymbols symbols)
{
    private readonly ExtensionInspectSymbols _symbols = symbols;

    internal CliBindResult<ExtensionInspectRequest, ExtensionInspectResult> Bind(
        CliBindingParse parse,
        CliInvocation invocation)
    {
        if (invocation.Workspace is not { } workspace)
        {
            return CliBindResult<ExtensionInspectRequest, ExtensionInspectResult>.Invalid(
                ExtensionInspectResultBuilder.WorkspaceBlocked(
                    parse.Result.GetValue(_symbols.StableId),
                    "The selected workspace is unavailable."));
        }

        var supplied = parse.Result.GetValue(_symbols.StableId);
        var source = parse.Result.GetValue(_symbols.Source);
        var sourceFacts = CliOptionResultFactsReader.Read(parse.Result, _symbols.Source);
        if (sourceFacts.IdentifierCount > 1
            || sourceFacts.IsExplicitWithoutValue
            || source is not null && string.IsNullOrWhiteSpace(source))
        {
            return CliBindResult<ExtensionInspectRequest, ExtensionInspectResult>.Invalid(
                ExtensionInspectResultBuilder.Invalid(
                    workspace,
                    supplied,
                    "--source accepts exactly one non-empty package or catalogue path."));
        }

        if (string.IsNullOrWhiteSpace(supplied)
            || !ExtensionIdentity.IsValidStableId(supplied))
        {
            return CliBindResult<ExtensionInspectRequest, ExtensionInspectResult>.Invalid(
                ExtensionInspectResultBuilder.InvalidStableId(
                    null,
                    supplied,
                    "The supplied Extension stable ID is missing or does not match the exact lowercase ID grammar."));
        }

        return CliBindResult<ExtensionInspectRequest, ExtensionInspectResult>.Bound(
            new ExtensionInspectRequest(workspace, supplied, source));
    }
}
