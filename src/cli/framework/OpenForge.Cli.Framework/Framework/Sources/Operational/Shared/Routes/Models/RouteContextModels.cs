using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Context;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;

internal sealed record RouteContextObservation
{
    public required ContextMeasurementObservation InitialStartup { get; init; }

    public required ContextMeasurementObservation CurrentStartup { get; init; }

    public required ContextMeasurementObservation TotalAvailable { get; init; }

    public required ContextMeasurementObservation Continuity { get; init; }

    public required IReadOnlyList<ContextSourceContributionObservation> ContinuitySources { get; init; }

    public required IReadOnlyList<string> InitialRootCategories { get; init; }

    public required IReadOnlyList<string> CurrentRootCategories { get; init; }

    public required bool IsIncomplete { get; init; }
}

internal sealed record RouteContextLayer(
    string Path,
    string? Text);

internal sealed record RouteContextSource
{
    public required string? Id { get; init; }

    public required string Path { get; init; }

    public required SourceDocumentForm Form { get; init; }

    public required SourceRouteState RouteState { get; init; }

    public required IReadOnlyList<RouteContextLayer> Layers { get; init; }

    public required SourceAuthoredMetadataFacts Metadata { get; init; }

    public required SourceGeneratedEntriesFacts GeneratedEntries { get; init; }

    public required string? ParentPath { get; init; }

    internal bool IsEntrypoint => SourceFormClassifier.IsEntrypoint(Form);

    internal bool IsLoader => Form == SourceDocumentForm.Loader;
}

internal sealed record RouteContextSet
{
    public required RouteContextSource WorkspaceEntry { get; init; }

    public required IReadOnlyList<RouteContextSource> Sources { get; init; }

    public required IReadOnlyList<string> RootPaths { get; init; }

    public required IReadOnlyList<string> RootCategories { get; init; }

    public required bool IsComplete { get; init; }
}

internal sealed record RouteContextClosure
{
    public required IReadOnlyList<RouteContextSource> Startup { get; init; }

    public required IReadOnlyList<RouteContextSource> Continuity { get; init; }

    public required bool IsComplete { get; init; }
}

internal sealed record RouteContextRootFacts(
    IReadOnlyList<string> Paths,
    IReadOnlyList<string> Categories);
