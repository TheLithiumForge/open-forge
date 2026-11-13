using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Storage;
using OpenForge.Cli.Core.Framework.Tools.Models.Supply;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;

internal sealed record ToolWriterArtifact(string Id, ToolArtifact Artifact, Uri Source, string Path, string RecipeSha256, ToolStoredUnit? Cached);
internal sealed record ToolWriterSupply(ToolDefinition Definition, string Target, ToolWriterStoreRead Store, ToolWriterArtifact[] Artifacts);
internal sealed record ToolWriterStagedSupply(ToolPreparedStoreUnit[] Archives, ToolPreparedStoreUnit? Install);
