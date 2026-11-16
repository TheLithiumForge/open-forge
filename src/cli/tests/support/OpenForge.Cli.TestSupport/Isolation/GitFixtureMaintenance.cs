namespace OpenForge.Cli.TestSupport.Isolation;

/// <summary>
/// Keeps Git from starting background work in a repository a test owns. Newer Git runs automatic
/// maintenance after a commit and detaches it, so its lock outlives the command, appears in the
/// test's read-only snapshots and competes with the product's bounded Git reads.
/// </summary>
public static class GitFixtureMaintenance
{
    /// <summary>Global options, placed before the Git subcommand, that turn automatic maintenance and collection off.</summary>
    public static IReadOnlyList<string> CommandOptions { get; } = ["-c", "maintenance.auto=false", "-c", "gc.auto=0"];
}
