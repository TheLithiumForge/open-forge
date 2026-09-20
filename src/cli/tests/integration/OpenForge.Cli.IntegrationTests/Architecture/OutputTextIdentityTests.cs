using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace OpenForge.Cli.IntegrationTests.Architecture;

public sealed class OutputTextIdentityTests
{
    [Fact, Trait("Boundary", "Architecture"), Trait("Feature", "output-text"), Trait("Evidence", "IntegrationContract")]
    public void OutputTextDefinitionsAndDocumentReferencesAreValid()
    {
        // Independent controls qualify the reader before relying on its repository result.
        MarkerSource[] validDefinitions = [new("valid.cs", "// @OpenForgeText sample.message\n// @OpenForge .agents/example.md\n// @OpenForgeTextRef sample.ignored\n// @openforgetext sample.ignored")];
        MarkerSource[] validReferences = [new("valid.md", "<!-- @OpenForgeTextRef sample.message -->\n```markdown\n<!-- @OpenForgeTextRef example.unresolved -->\n```\n~~~markdown\n<!-- @OpenForgeTextRef example.unresolved -->\n~~~")];
        Assert.Empty(Validate(validDefinitions, validReferences));
        Assert.Contains(Validate([.. validDefinitions, new("duplicate.cs", "// @OpenForgeText sample.message")], validReferences), error => error.StartsWith("Duplicate definition:", StringComparison.Ordinal));
        Assert.Contains(Validate(validDefinitions, [new("unresolved.md", "<!-- @OpenForgeTextRef sample.missing -->")]), error => error.StartsWith("Unresolved reference:", StringComparison.Ordinal));
        foreach (var invalid in new[] { "sample", "Sample.message", "sample.1message", "sample.message extra", "sample..message" })
        {
            Assert.Contains(Validate([new("invalid.cs", "// @OpenForgeText " + invalid)], []), error => error.StartsWith("Malformed definition:", StringComparison.Ordinal));
            Assert.Contains(Validate(validDefinitions, [new("invalid.md", "<!-- @OpenForgeTextRef " + invalid + " -->")]), error => error.StartsWith("Malformed reference:", StringComparison.Ordinal));
        }

        var root = RepositoryRoot();
        var sources = ReadSources(Path.Combine(root, "src", "cli", "output-text", "OpenForge.Cli.OutputText"), "*.cs");
        var documents = ReadSources(Path.Combine(root, ".agents", "memory", "crystallized", "documents", "cli", "contracts"), "*.md");
        Assert.NotEmpty(sources);
        Assert.NotEmpty(documents);
        Assert.Contains(sources, source => DefinitionStart.IsMatch(source.Text));
        Assert.Contains(documents, source => VisibleLines(source.Text).Any(line => ReferenceStart.IsMatch(line)));
        var errors = Validate(sources, documents);
        Assert.True(errors.Count == 0, string.Join(Environment.NewLine, errors));
    }

    [Fact, Trait("Boundary", "Architecture"), Trait("Feature", "output-text"), Trait("Evidence", "IntegrationContract")]
    public void OutputTextHasOnlyBclDependencies()
    {
        var directory = Path.Combine(RepositoryRoot(), "src", "cli", "output-text", "OpenForge.Cli.OutputText");
        var project = XDocument.Load(Path.Combine(directory, "OpenForge.Cli.OutputText.csproj"));
        Assert.Empty(project.Descendants("ProjectReference"));
        Assert.Empty(project.Descendants("PackageReference"));
        Assert.Equal("true", Assert.Single(project.Descendants("IsAotCompatible")).Value);
        Assert.Equal("true", Assert.Single(project.Descendants("IsTrimmable")).Value);
        Assert.Equal("false", Assert.Single(project.Descendants("JsonSerializerIsReflectionEnabledByDefault")).Value);
        var sources = ReadSources(directory, "*.cs");
        Assert.NotEmpty(sources);
        foreach (var source in sources)
        {
            Assert.False(Regex.IsMatch(source.Text, @"\bOpenForge\.Cli\.(?!OutputText\b)"), source.Path);
        }
    }

    private const string Identity = @"[a-z][a-z0-9-]*(?:\.[a-z][a-z0-9-]*)+";
    private static readonly Regex DefinitionStart = new(@"^\s*//\s+@OpenForgeText(?:\s|$)", RegexOptions.Multiline);
    private static readonly Regex Definition = new(@"^\s*//\s+@OpenForgeText (?<id>" + Identity + @")\s*$");
    private static readonly Regex ReferenceStart = new(@"^\s*<!--\s+@OpenForgeTextRef(?:\s|$)");
    private static readonly Regex Reference = new(@"^\s*<!--\s+@OpenForgeTextRef (?<id>" + Identity + @") -->\s*$");

    private static List<string> Validate(IReadOnlyList<MarkerSource> sources, IReadOnlyList<MarkerSource> documents)
    {
        var errors = new List<string>();
        var definitions = new HashSet<string>(StringComparer.Ordinal);
        foreach (var source in sources)
        {
            foreach (var line in source.Text.Split('\n'))
            {
                if (!DefinitionStart.IsMatch(line)) continue;
                var match = Definition.Match(line);
                if (!match.Success) errors.Add($"Malformed definition: {source.Path}: {line.Trim()}");
                else if (!definitions.Add(match.Groups["id"].Value)) errors.Add($"Duplicate definition: {match.Groups["id"].Value}");
            }
        }
        foreach (var document in documents)
        {
            foreach (var line in VisibleLines(document.Text))
            {
                if (!ReferenceStart.IsMatch(line)) continue;
                var match = Reference.Match(line);
                if (!match.Success) errors.Add($"Malformed reference: {document.Path}: {line.Trim()}");
                else if (!definitions.Contains(match.Groups["id"].Value)) errors.Add($"Unresolved reference: {document.Path}: {match.Groups["id"].Value}");
            }
        }
        return errors;
    }

    private static IEnumerable<string> VisibleLines(string text)
    {
        var fence = '\0';
        var fenceLength = 0;
        foreach (var line in text.Split('\n'))
        {
            var trimmed = line.TrimStart();
            if (trimmed.Length >= 3 && trimmed[0] is '`' or '~')
            {
                var length = trimmed.TakeWhile(character => character == trimmed[0]).Count();
                if (length >= 3 && fence == '\0')
                {
                    fence = trimmed[0];
                    fenceLength = length;
                    continue;
                }
                if (trimmed[0] == fence && length >= fenceLength && trimmed[length..].Trim().Length == 0)
                {
                    fence = '\0';
                    continue;
                }
            }
            if (fence == '\0') yield return line;
        }
    }

    private static MarkerSource[] ReadSources(string directory, string pattern)
        => Directory.EnumerateFiles(directory, pattern, SearchOption.AllDirectories)
            .Where(path => !Path.GetRelativePath(directory, path).Replace('\\', '/').Split('/').Any(segment => segment is "obj" or "bin"))
            .Select(path => new MarkerSource(path, File.ReadAllText(path)))
            .ToArray();

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "OpenForge.Cli.slnx"))) directory = directory.Parent;
        Assert.NotNull(directory);
        return directory.FullName;
    }

    private sealed record MarkerSource(string Path, string Text);
}
