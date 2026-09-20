using System.Text.RegularExpressions;

namespace OpenForge.Cli.Core.UnitTests.Architecture;

/// <summary>
/// Asserts the four layer boundaries from the accepted CLI Layers record by
/// reading the using graph across the production project source trees.
///
/// These hold today. The test exists because they are cheap to break invisibly:
/// the regression that prompted it was one using directive in one file, added
/// while deduplicating a shared mapping into a folder one layer too low.
/// Failures therefore name every offending file.
/// </summary>
public sealed class LayerBoundaryTests
{
    [Trait("Boundary", "Architecture")]
    [Fact(DisplayName = "Presentation follows the report layer dependencies"), Trait("Feature", "cli-presentation"), Trait("Evidence", "IntegrationContract")]
    public void PresentationDependenciesFollowAcceptedDirections()
    {
        var offences = new List<string>();
        foreach (var (path, source) in ReadCoreSources())
        {
            foreach (Match match in CoreUsing.Matches(source))
            {
                var imported = match.Groups["namespace"].Value;
                if ((path.StartsWith("Commands/", StringComparison.Ordinal) || path.StartsWith("Framework/", StringComparison.Ordinal))
                    && imported.StartsWith("Presentation.", StringComparison.Ordinal))
                    offences.Add($"{path}: {imported}");
                if (!path.StartsWith("Presentation/", StringComparison.Ordinal)
                    || path.StartsWith("Presentation/Legacy/", StringComparison.Ordinal)) continue;
                var allowedShell = imported.StartsWith("Shell.Definitions", StringComparison.Ordinal)
                    || imported == "Shell.Interaction"
                    || imported.StartsWith("Shell.Interaction.Models", StringComparison.Ordinal)
                    || imported.StartsWith("Shell.Pipeline.Models.Operation", StringComparison.Ordinal)
                    || imported.StartsWith("Shell.Presentation.Models", StringComparison.Ordinal)
                    || (path == "Presentation/Shared/Rendering/CliRenderingStage.cs"
                        && imported == "Shell.Pipeline.Models.Presentation");
                if (imported.StartsWith("Shell.", StringComparison.Ordinal) && !allowedShell)
                    offences.Add($"{path}: {imported}");
                if (path.StartsWith("Presentation/Shared/", StringComparison.Ordinal))
                {
                    if (imported.StartsWith("Commands.", StringComparison.Ordinal) || imported.StartsWith("Framework.", StringComparison.Ordinal))
                        offences.Add($"{path}: {imported}");
                    continue;
                }
                if (imported.StartsWith("Framework.", StringComparison.Ordinal)) offences.Add($"{path}: {imported}");
                if (imported.StartsWith("Commands.", StringComparison.Ordinal))
                {
                    var segments = imported.Split('.');
                    var models = Array.IndexOf(segments, "Models");
                    var owner = models < 0 ? string.Empty : string.Join('/', segments[1..models]);
                    if (owner.Length == 0 || !path.StartsWith($"Presentation/{owner}/", StringComparison.Ordinal))
                        offences.Add($"{path}: {imported}");
                }
            }
        }
        Assert.True(offences.Count == 0, string.Join(Environment.NewLine, offences));
    }

    private const string CoreNamespace = "OpenForge.Cli.Core.";

    private static readonly Regex CoreUsing = new(
        @"^using\s+OpenForge\.Cli\.Core\.(?<namespace>[\w.]+);",
        RegexOptions.Multiline | RegexOptions.Compiled);

    [Trait("Boundary", "Architecture")]
    [Fact(DisplayName = "Framework never references a command"), Trait("Feature", "architecture"), Trait("Evidence", "IntegrationContract")]
    public void FrameworkNeverReferencesCommands()
        => AssertNoEdge(
            from: "Framework",
            to: "Commands",
            because: "Framework holds reusable facts and must stay usable without any command.");

    [Trait("Boundary", "Architecture")]
    [Fact(DisplayName = "Framework never references the Shell"), Trait("Feature", "architecture"), Trait("Evidence", "IntegrationContract")]
    public void FrameworkNeverReferencesShell()
        => AssertNoEdge(
            from: "Framework",
            to: "Shell",
            because: "Facts must not depend on the process boundary that reports them.");

    [Trait("Boundary", "Architecture")]
    [Fact(DisplayName = "The Shell never references a concrete command"), Trait("Feature", "architecture"), Trait("Evidence", "IntegrationContract")]
    public void ShellNeverReferencesCommands()
        => AssertNoEdge(
            from: "Shell",
            to: "Commands",
            because: "The pipeline stays generic over command payloads. A shared owner the Shell "
                + "consumes belongs in the Shell, however many commands also consume it.");

    /// <summary>
    /// The one cross-command reach the accepted Architecture permits: Repair
    /// repairs what Doctor finds, and shares its library-recovery projection so
    /// both report the residual identically.
    ///
    /// It is named rather than tolerated silently. If G1 or G3 moves that
    /// projection to a shared owner, delete this entry and the test tightens on
    /// its own.
    /// </summary>
    private static readonly (string Consumer, string Owner)[] AcceptedReaches =
    [
        ("Commands.Repair", "Commands.Doctor"),
    ];

    [Trait("Boundary", "Architecture")]
    [Fact(DisplayName = "No command reaches into another command's private Shared namespace"), Trait("Feature", "architecture"), Trait("Evidence", "IntegrationContract")]
    public void CommandsNeverReachIntoAnotherCommandsShared()
    {
        var offences = new List<string>();
        foreach (var (path, source) in ReadCoreSources())
        {
            var consumer = LogicalNamespace(path);
            if (!consumer.StartsWith("Commands.", StringComparison.Ordinal))
            {
                continue;
            }

            foreach (Match match in CoreUsing.Matches(source))
            {
                var imported = match.Groups["namespace"].Value;
                if (!imported.StartsWith("Commands.", StringComparison.Ordinal))
                {
                    continue;
                }

                var owner = PrivateSharedOwner(imported);
                if (owner is null
                    || consumer == owner
                    || consumer.StartsWith(owner + ".", StringComparison.Ordinal)
                    || IsAcceptedReach(consumer, owner))
                {
                    continue;
                }

                offences.Add($"{path}\n      uses {CoreNamespace}{imported}\n      owned by {CoreNamespace}{owner}");
            }
        }

        Assert.True(
            offences.Count == 0,
            Explain(
                offences,
                "A command reaches into another command's private Shared namespace.",
                "Shared meaning moves to the nearest shared parent only when another real "
                    + "consumer needs identical meaning."));
    }

    /// <summary>
    /// The boundary that owns a private <c>Shared</c> namespace, or null when the
    /// imported namespace has no <c>Shared</c> segment. The owner is the prefix
    /// above <c>Shared</c>, so a leaf may use its own family's shared support and
    /// every command may use <c>Commands.Shared</c>.
    /// </summary>
    private static string? PrivateSharedOwner(string imported)
    {
        var segments = imported.Split('.');
        var index = Array.IndexOf(segments, "Shared");
        return index < 0 ? null : string.Join('.', segments[..index]);
    }

    private static bool IsAcceptedReach(string consumer, string owner)
        => AcceptedReaches.Any(accepted
            => owner == accepted.Owner
                && (consumer == accepted.Consumer
                    || consumer.StartsWith(accepted.Consumer + ".", StringComparison.Ordinal)));

    private static void AssertNoEdge(string from, string to, string because)
    {
        var offences = new List<string>();
        foreach (var (path, source) in ReadCoreSources())
        {
            if (Area(path) != from)
            {
                continue;
            }

            foreach (Match match in CoreUsing.Matches(source))
            {
                var imported = match.Groups["namespace"].Value;
                if (imported == to || imported.StartsWith(to + ".", StringComparison.Ordinal))
                {
                    offences.Add($"{path}\n      uses {CoreNamespace}{imported}");
                }
            }
        }

        Assert.True(
            offences.Count == 0,
            Explain(offences, $"{from} must not reference {to}.", because));
    }

    private static string Explain(IReadOnlyList<string> offences, string rule, string because)
    {
        if (offences.Count == 0)
        {
            return string.Empty;
        }

        var lines = new List<string>
        {
            rule,
            because,
            string.Empty,
            $"{offences.Count} offending reference(s):",
        };
        lines.AddRange(offences.Select(offence => "    " + offence));
        return string.Join(Environment.NewLine, lines);
    }

    private static string Area(string logicalPath) => logicalPath.Split('/')[0];

    /// <summary>The file's logical layer namespace, derived from its path under its project.</summary>
    private static string LogicalNamespace(string logicalPath)
    {
        var separator = logicalPath.LastIndexOf('/');
        var withoutFile = separator < 0 ? string.Empty : logicalPath[..separator];
        return withoutFile.Replace('/', '.');
    }

    private static IEnumerable<(string LogicalPath, string Source)> ReadCoreSources()
    {
        var sourceRoot = CoreSourceRoot();
        foreach (var (folder, assembly) in new[]
        {
            ("framework", "OpenForge.Cli.Framework"),
            ("shell", "OpenForge.Cli.Shell"),
            ("operations", "OpenForge.Cli.Operations"),
            ("rendering", "OpenForge.Cli.Rendering"),
            ("root", "OpenForge.Cli"),
        })
        {
            var root = Path.Combine(sourceRoot, folder, assembly);
            Assert.True(Directory.Exists(root), $"The production source tree was not found at {root}.");
            var files = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
                .Where(file => !Path.GetRelativePath(root, file).Replace(Path.DirectorySeparatorChar, '/')
                    .Split('/').Any(segment => segment is "obj" or "bin"))
                .ToArray();
            Assert.NotEmpty(files);
            foreach (var file in files)
            {
                var logical = Path.GetRelativePath(root, file).Replace(Path.DirectorySeparatorChar, '/');
                yield return (logical, File.ReadAllText(file));
            }
        }
    }

    /// <summary>
    /// Walks up from the test assembly to the repository, the way the CLI walks up
    /// to a workspace. Fails loudly rather than silently passing on zero files.
    /// </summary>
    private static string CoreSourceRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "OpenForge.Cli.slnx")))
        {
            directory = directory.Parent;
        }

        Assert.True(
            directory is not null,
            "The repository root was not found above the test assembly, so the layer "
                + "boundaries were not checked. This test must not pass by finding nothing.");

        var root = Path.Combine(directory!.FullName, "src", "cli");
        Assert.True(Directory.Exists(root), $"The CLI source tree was not found at {root}.");
        return root;
    }
}
