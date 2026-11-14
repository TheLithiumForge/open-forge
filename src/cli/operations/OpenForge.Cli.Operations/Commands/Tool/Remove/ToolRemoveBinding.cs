using System.CommandLine;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Binding;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Binding;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Remove;

internal static class ToolRemoveBinding
{
    internal static ToolWriteSymbols CreateSymbols(Command parent)
        => ToolWriteBinding.CreateSymbols(parent, ToolWriteKind.Remove);

    internal static CliRequestBinding<ToolWriteRequest, ToolWriteResult> CreateRequestBinding(
        ToolWriteSymbols symbols, Func<ToolRemoveOperation> operationFactory, ToolCatalogue catalogue, IToolVersionService versions, CliHelpContent help)
        => ToolWriteBinding.Create(symbols, catalogue, versions,
            (request, cancellationToken) => operationFactory().ExecuteAsync(request, cancellationToken), help);
}
