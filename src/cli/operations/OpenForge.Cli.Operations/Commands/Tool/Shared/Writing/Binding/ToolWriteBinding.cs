using System.CommandLine;
using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Binding;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Application;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Binding;

internal static class ToolWriteBinding
{
    internal const string YesOption = "--yes";
    internal const string DryRunOption = "--dry-run";
    internal const string ChannelOption = "--channel";
    internal const string SpecArgument = "id[@version]";
    internal const string IdArgument = "id";

    internal static ToolWriteSymbols CreateSymbols(Command parent, ToolWriteKind kind)
    {
        ArgumentNullException.ThrowIfNull(parent);
        var command = new Command(Name(kind), Description(kind));
        var spec = new Argument<string?>(kind == ToolWriteKind.Remove ? IdArgument : SpecArgument)
        {
            Description = kind == ToolWriteKind.Remove ? "The declared or locked tool ID." : "A tool ID, ID@major.minor or ID@exact stable version.",
            Arity = kind == ToolWriteKind.Remove ? ArgumentArity.ExactlyOne : ArgumentArity.ZeroOrOne,
        };
        command.Arguments.Add(spec);
        var yes = new Option<bool>(YesOption) { Description = "Accept the shown plan, including downloads. Keep PATH and shared files unchanged.", Arity = ArgumentArity.Zero };
        var dryRun = new Option<bool>(DryRunOption) { Description = "Show the checked plan without applying changes.", Arity = ArgumentArity.Zero };
        var allow = new Option<string[]>(PermissionOptions.AllowPath.Name)
        {
            Description = PermissionOptions.AllowPath.Description,
            HelpName = PermissionOptions.AllowPath.ValueName,
            Arity = ArgumentArity.OneOrMore,
            AllowMultipleArgumentsPerToken = false,
        };
        command.Options.Add(yes);
        command.Options.Add(dryRun);
        command.Options.Add(allow);
        Option<string?>? channel = null;
        if (kind != ToolWriteKind.Remove)
        {
            channel = new(ChannelOption) { Description = "Select an advertised channel for the named tool.", Arity = ArgumentArity.ExactlyOne };
            command.Options.Add(channel);
        }
        parent.Subcommands.Add(command);
        return new() { Kind = kind, Command = command, Spec = spec, Channel = channel, Yes = yes, DryRun = dryRun, AllowPath = allow };
    }

    internal static CliRequestBinding<ToolWriteRequest, ToolWriteResult> Create(
        ToolWriteSymbols symbols, ToolCatalogue catalogue, IToolVersionService versions,
        CliOperation<ToolWriteRequest, ToolWriteResult> operation, CliHelpContent help)
        => new()
        {
            Command = symbols.Command,
            Help = help,
            WorkspaceRequirement = CliWorkspaceRequirement.Required,
            Binder = (parse, invocation) => ToolWriteRequestBinder.Bind(symbols, catalogue, versions, parse, invocation),
            InvalidResultFactory = input => ToolWriteRequestBinder.Invalid(symbols.Kind, input),
            Operation = operation,
        };

    internal static string Name(ToolWriteKind kind) => kind switch
    {
        ToolWriteKind.Install => "install",
        ToolWriteKind.Update => "update",
        ToolWriteKind.Remove => "remove",
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    internal static async ValueTask<ToolWriteResult> InvokeAsync<TLeaf>(
        ToolWriteRequest request, CancellationToken cancellationToken,
        Func<ToolWriteRequest, CancellationToken, ValueTask<TLeaf>> operationFactory,
        Func<TLeaf, ToolWriteRequest, CancellationToken, ValueTask<ToolWriteResult>> execute)
    {
        TLeaf operation;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            operation = await operationFactory(request, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
        }
        catch (OperationCanceledException)
        {
            return Failure(ToolWriteFindingCode.Interrupted, CliSemanticStatus.Interrupted);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException)
        {
            return Failure(ToolWriteFindingCode.UnexpectedFailure, CliSemanticStatus.Failed);
        }
        return await execute(operation, request, cancellationToken).ConfigureAwait(false);

        ToolWriteResult Failure(ToolWriteFindingCode code, CliSemanticStatus status)
            => ToolWriteResults.Invalid(request.Kind, new(code, status, request.Spec?.Id), request.Workspace, request.Preview);
    }

    private static string Description(ToolWriteKind kind) => kind switch
    {
        ToolWriteKind.Install => "Install tools and add configuration.",
        ToolWriteKind.Update => "Update tools to a supported version.",
        ToolWriteKind.Remove => "Uninstall tool setup and remove its additions.",
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
}
