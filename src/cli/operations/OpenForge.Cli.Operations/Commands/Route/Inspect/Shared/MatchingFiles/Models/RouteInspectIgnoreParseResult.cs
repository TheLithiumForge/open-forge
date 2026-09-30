using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.MatchingFiles.Models;

internal sealed record RouteInspectIgnoreParseResult(ImmutableArray<RouteInspectIgnoreRule> Rules, int SkippedLines);
