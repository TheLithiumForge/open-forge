using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;

internal static class ExtensionInspectDependencyPathBuilder
{
    internal static ExtensionInspectDependencyPathFacts Build(
        ExtensionInspectDependencyPathInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var dependencies = ExtensionInspectDependencyClosureBuilder.ReadDependencies(input);
        var availableClosure = ExtensionInspectDependencyClosureBuilder.ReadAvailableClosure(
            input.Source,
            input.AvailablePackage,
            dependencies);
        var sourcePathProjections = ExtensionInspectPathFactsBuilder.ReadSourcePathProjections(
            availableClosure,
            input.Findings);
        var pathFacts = ExtensionInspectPathFactsBuilder.Build(input, sourcePathProjections);

        return new ExtensionInspectDependencyPathFacts
        {
            Dependencies = dependencies,
            AvailableClosure = availableClosure,
            SourcePathProjections = sourcePathProjections,
            PathFacts = pathFacts,
        };
    }

    internal static ExtensionInspectPathFacts ReadEventPathFacts(
        IReadOnlyList<ExtensionInspectCurrentPath>? currentPaths)
        => ExtensionInspectPathFactsBuilder.ReadEventPathFacts(currentPaths);
}
