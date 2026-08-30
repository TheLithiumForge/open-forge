namespace OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

internal sealed record PlannedDirectoryCreation
{
    private PlannedDirectoryCreation(FileExpectation expectation)
    {
        Expectation = expectation;
    }

    internal FileExpectation Expectation { get; }

    internal string LogicalPath => Expectation.LogicalPath;

    internal static PlannedDirectoryCreation Create(FileExpectation expectation)
    {
        ArgumentNullException.ThrowIfNull(expectation);
        if (expectation.Kind != FileExpectationKind.Missing)
        {
            throw new ArgumentException(
                "A missing expectation is required for a planned directory creation.",
                nameof(expectation));
        }

        return new PlannedDirectoryCreation(expectation);
    }
}
