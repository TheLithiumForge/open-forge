using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Execution;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;

internal sealed record ToolWriterCatalogue(ToolCatalogueReader Reader, ToolWriterAdapterResolver Adapter);
internal delegate ToolWriterAdapter? ToolWriterAdapterResolver(ToolDefinition definition);

internal sealed record ToolWriterRuntime(
    ToolWriterCatalogue Catalogue,
    ToolStorePaths StorePaths,
    WorkspaceLockManager Locks,
    ToolExecutionHost Execution,
    string OpenForge);
