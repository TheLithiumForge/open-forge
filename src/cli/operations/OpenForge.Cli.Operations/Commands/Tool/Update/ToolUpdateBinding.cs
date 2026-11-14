using System.CommandLine;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Binding;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Binding;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Update;

internal static class ToolUpdateBinding
{
    internal static ToolWriteSymbols CreateSymbols(Command parent)
        => ToolWriteBinding.CreateSymbols(parent, ToolWriteKind.Update);

    internal static CliRequestBinding<ToolWriteRequest, ToolWriteResult> CreateRequestBinding(
        ToolWriteSymbols symbols, ToolUpdateOperation operation, IToolVersionService versions, CliHelpContent help)
        => ToolWriteBinding.Create(symbols, versions, operation.ExecuteAsync, help);
}
