using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Framework.Sources.Shared.Applicability.Models;

internal sealed record SourceApplicabilityResult(
    SourceApplicabilityState State,
    ImmutableArray<SourceApplyToCondition> Conditions,
    ImmutableArray<string> MatchingPaths);
