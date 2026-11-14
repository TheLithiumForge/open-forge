using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing;

namespace OpenForge.Cli.Core.Commands.Tool.Update;

internal sealed class ToolUpdateOperation(ToolWriteOperation writer)
{
    internal ValueTask<ToolWriteResult> ExecuteAsync(ToolWriteRequest request, CancellationToken cancellationToken)
        => writer.ExecuteAsync(request with { Kind = ToolWriteKind.Update }, cancellationToken);
}
