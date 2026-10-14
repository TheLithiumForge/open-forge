using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Index.Models.Planning;

namespace OpenForge.Cli.Core.Commands.Index.Models.Result;

internal sealed record IndexMetadataCompletion(string Path, ImmutableArray<string> Fields, string? Description, ImmutableArray<string>? Tags, IndexRegionOutcome Outcome);
internal sealed record IndexEntrypointCompletion(string Path, IndexRegionOutcome Outcome);
internal sealed record IndexFileDeletion(string Path, IndexRegionOutcome Outcome);
