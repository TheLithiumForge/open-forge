using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.IntegrationTests.Framework.Documents.RepositoryDocumentation.Shared;

internal static class RepositoryMarkdownChecker
{
    private const string SolutionFileName = "OpenForge.Cli.slnx";

    internal static async Task<IReadOnlyList<string>> CheckAsync(
        string repositoryRoot,
        IReadOnlyList<string> sourcePaths,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRoot);
        ArgumentNullException.ThrowIfNull(sourcePaths);
        cancellationToken.ThrowIfCancellationRequested();

        var lexicalRoot = ValidateRepositoryRoot(repositoryRoot);
        var normalizedSourcePaths = sourcePaths
            .Select(path => SourceWorkspaceRelativePath.ValidateMarkdown(path, nameof(sourcePaths)))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToArray();
        var physicalPaths = new PhysicalPathResolver();
        var physicalRoot = physicalPaths.ResolveRoot(lexicalRoot);
        if (physicalRoot.State != PhysicalPathState.Contained)
        {
            throw new InvalidOperationException("The repository physical root could not be established safely.");
        }

        var workspace = new CliWorkspace(
            lexicalRoot,
            physicalRoot.GetContainedPhysicalPath(),
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var catalogue = new SourceCatalogue(workspace, [], [], [], isCancelled: false);
        var markdownParser = new MarkdownDocumentParser();
        var parsedFacts = new Dictionary<string, MarkdownDocumentFacts>(StringComparer.Ordinal);
        MarkdownDocumentFacts? lastParsedFacts = null;
        var diagnostics = new List<(string SourcePath, int Offset, string Value)>();

        MarkdownDocumentFacts ParseCached(string source)
        {
            if (!parsedFacts.TryGetValue(source, out var facts))
            {
                facts = markdownParser.Parse(source);
                parsedFacts.Add(source, facts);
            }

            lastParsedFacts = facts;
            return facts;
        }

        foreach (var sourcePath in normalizedSourcePaths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var sourcePhysical = ResolveSourcePath(physicalPaths, workspace, sourcePath);
            if (sourcePhysical.FailureCode is { } sourceFailureCode)
            {
                diagnostics.Add((
                    sourcePath,
                    0,
                    FormatDiagnostic(sourcePath, null, 0, sourceFailureCode, sourcePath)));
                continue;
            }

            var read = await StrictUtf8FileReader.ReadAsync(
                sourcePhysical.PhysicalPath
                    ?? throw new InvalidOperationException("A complete source resolution requires a physical path."),
                sourcePath,
                cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            var source = read.Value;
            if (read.State != FileReadState.Complete || source is null)
            {
                var sourceCode = SourceReadFailureCode(read.State);
                diagnostics.Add((
                    sourcePath,
                    0,
                    FormatDiagnostic(sourcePath, null, 0, sourceCode, sourcePath)));
                continue;
            }

            MarkdownDocumentFacts facts;
            try
            {
                facts = ParseCached(source);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                diagnostics.Add((
                    sourcePath,
                    0,
                    FormatDiagnostic(sourcePath, null, 0, "markdown-unavailable", sourcePath)));
                continue;
            }

            if (facts.BodySpan is null)
            {
                diagnostics.Add((
                    sourcePath,
                    0,
                    FormatDiagnostic(sourcePath, null, 0, "markdown-unavailable", sourcePath)));
                continue;
            }

            foreach (var link in facts.Links.Concat(facts.Images).OrderBy(link => link.Span.Start))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (IsOutsideOfflineGate(sourcePath, link.RawDestination))
                {
                    continue;
                }

                lastParsedFacts = null;
                string? capturedLexicalPath = null;
                PhysicalPathResolution? capturedPhysical = null;
                var resolver = new SourceLinkDestinationResolver(
                    physicalPathResolver: (linkWorkspace, lexicalPath) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        capturedLexicalPath = lexicalPath;
                        var resolution = physicalPaths.ResolveCandidate(
                            linkWorkspace.LexicalRoot,
                            linkWorkspace.PhysicalRoot,
                            lexicalPath);
                        capturedPhysical = resolution;
                        cancellationToken.ThrowIfCancellationRequested();
                        return resolution;
                    },
                    strictUtf8Reader: StrictUtf8FileReader.ReadAsync,
                    markdownParser: ParseCached);
                var resolved = await resolver.ResolveAsync(
                    new SourceLinkDestinationInput
                    {
                        Workspace = workspace,
                        Catalogue = catalogue,
                        SourceCanonicalPath = sourcePath,
                        RawDestination = link.RawDestination,
                    },
                    cancellationToken).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();

                var casing = capturedPhysical is { State: PhysicalPathState.Contained or PhysicalPathState.Missing }
                    && capturedLexicalPath is { } lexicalPath
                    ? ReadPathCasing(
                        physicalPaths,
                        workspace,
                        lexicalPath,
                        targetWasMissing: capturedPhysical?.State == PhysicalPathState.Missing,
                        cancellationToken: cancellationToken)
                    : (IsAvailable: true, IsMismatch: false);
                var directory = capturedPhysical is { State: PhysicalPathState.Contained } containedPhysical
                    ? ReadOrdinaryDirectory(containedPhysical)
                    : null;

                var code = ReadDiagnosticCode(resolved, lastParsedFacts, directory, casing);
                if (code is null)
                {
                    continue;
                }

                diagnostics.Add((
                    sourcePath,
                    link.Span.Start,
                    FormatDiagnostic(sourcePath, facts.Source, link.Span.Start, code, link.RawDestination)));
            }
        }

        return diagnostics
            .OrderBy(diagnostic => diagnostic.SourcePath, StringComparer.Ordinal)
            .ThenBy(diagnostic => diagnostic.Offset)
            .Select(diagnostic => diagnostic.Value)
            .ToArray();
    }

    private static string? ReadDiagnosticCode(
        SourceLinkDestinationFacts facts,
        MarkdownDocumentFacts? targetFacts,
        bool? directory,
        (bool IsAvailable, bool IsMismatch) casing)
    {
        if (directory == true
            && facts.Target.Resolution == SourceLinkTargetResolution.Malformed
            && facts.Finding?.Code == SourceLinkDestinationFindingCode.DestinationMalformed)
        {
            if (facts.Fragment is not null)
            {
                return "unverified-anchor";
            }

            if (!casing.IsAvailable)
            {
                return "target-unreadable";
            }

            return casing.IsMismatch ? "target-case-mismatch" : null;
        }

        if (!casing.IsAvailable && facts.Finding?.Code != SourceLinkDestinationFindingCode.TargetMissing)
        {
            return "target-unreadable";
        }

        if (casing.IsMismatch)
        {
            return "target-case-mismatch";
        }

        if (facts.Fragment is not null
            && facts.Target.Path is { } targetPath
            && !targetPath.EndsWith(".md", StringComparison.Ordinal))
        {
            return "unverified-anchor";
        }

        var finding = facts.Finding;
        if (finding is null)
        {
            return null;
        }

        if (facts.Fragment is not null && IsUnverifiedAnchor(targetFacts))
        {
            return "unverified-anchor";
        }

        return finding.Code switch
        {
            SourceLinkDestinationFindingCode.DestinationMalformed => "destination-malformed",
            SourceLinkDestinationFindingCode.DestinationUnsupported => "destination-unsupported",
            SourceLinkDestinationFindingCode.TargetUnsafe => "target-unsafe",
            SourceLinkDestinationFindingCode.InvalidEncoding => "invalid-encoding",
            SourceLinkDestinationFindingCode.TargetMissing => "target-missing",
            SourceLinkDestinationFindingCode.TargetUnreadable => "target-unreadable",
            SourceLinkDestinationFindingCode.TargetAmbiguous => "target-ambiguous",
            SourceLinkDestinationFindingCode.IdentityCollision => "identity-collision",
            SourceLinkDestinationFindingCode.FragmentMissing => "fragment-missing",
            _ => throw new ArgumentOutOfRangeException(
                nameof(facts),
                finding.Code,
                "The source-link destination finding code is not defined."),
        };
    }

    private static bool IsUnverifiedAnchor(MarkdownDocumentFacts? targetFacts)
    {
        if (targetFacts is null || targetFacts.BodySpan is null)
        {
            return targetFacts is not null;
        }

        return targetFacts.Headings.Any(heading =>
                heading.Form == MarkdownHeadingForm.Setext
                || !heading.IsCanonical
                || heading.FragmentIdentifier is null)
            || targetFacts.OpaqueSpans.Any(span => !span.IsCode);
    }

    private static bool? ReadOrdinaryDirectory(PhysicalPathResolution resolution)
    {
        if (resolution.State != PhysicalPathState.Contained)
        {
            return null;
        }

        var component = LinkTargetReader.Read(resolution.GetContainedPhysicalPath());
        if (component.State != PathComponentState.Ordinary || component.Attributes is not { } attributes)
        {
            return null;
        }

        return (attributes & FileAttributes.Directory) != 0;
    }

    private static (bool IsAvailable, bool IsMismatch) ReadPathCasing(
        PhysicalPathResolver physicalPaths,
        CliWorkspace workspace,
        string lexicalPath,
        bool targetWasMissing,
        CancellationToken cancellationToken)
    {
        var relativePath = Path.GetRelativePath(workspace.LexicalRoot, lexicalPath);
        if (relativePath == ".")
        {
            return (true, false);
        }

        var components = relativePath
            .Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);
        var physicalDirectory = workspace.PhysicalRoot;
        var actualSegments = new List<string>(components.Length);
        var mismatch = false;

        for (var index = 0; index < components.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entries = DirectoryEntryEnumerator.Enumerate(
                physicalDirectory,
                actualSegments.Count == 0 ? "." : string.Join('/', actualSegments),
                cancellationToken);
            if (entries.State == DirectoryEnumerationState.Cancelled)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            if (entries.State != DirectoryEnumerationState.Complete || entries.Entries is null)
            {
                return (false, false);
            }

            var matches = entries.Entries
                .Select(Path.GetFileName)
                .Where(name => name is not null
                    && string.Equals(name, components[index], StringComparison.OrdinalIgnoreCase))
                .OfType<string>()
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
            if (matches.Length == 0)
            {
                return targetWasMissing ? (true, false) : (false, false);
            }

            if (matches.Length != 1)
            {
                return (false, false);
            }

            var actualName = matches[0];
            mismatch |= !string.Equals(actualName, components[index], StringComparison.Ordinal);
            actualSegments.Add(actualName);
            var lexicalCandidate = Path.Combine([workspace.LexicalRoot, .. actualSegments]);
            var physical = physicalPaths.ResolveCandidate(
                workspace.LexicalRoot,
                workspace.PhysicalRoot,
                lexicalCandidate);
            if (physical.State != PhysicalPathState.Contained)
            {
                return (false, false);
            }

            var component = LinkTargetReader.Read(physical.GetContainedPhysicalPath());
            if (component.State != PathComponentState.Ordinary || component.Attributes is not { } attributes)
            {
                return (false, false);
            }

            var isLast = index == components.Length - 1;
            if (!isLast && (attributes & FileAttributes.Directory) == 0)
            {
                return (false, false);
            }

            physicalDirectory = physical.GetContainedPhysicalPath();
        }

        return (true, mismatch);
    }

    private static (string? PhysicalPath, string? FailureCode) ResolveSourcePath(
        PhysicalPathResolver physicalPaths,
        CliWorkspace workspace,
        string sourcePath)
    {
        var lexicalPath = Path.Combine(
            workspace.LexicalRoot,
            sourcePath.Replace('/', Path.DirectorySeparatorChar));
        var physical = physicalPaths.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            lexicalPath);
        if (physical.State != PhysicalPathState.Contained)
        {
            return (null, SourcePhysicalFailureCode(physical.State));
        }

        var component = LinkTargetReader.Read(physical.GetContainedPhysicalPath());
        if (component.State != PathComponentState.Ordinary || component.Attributes is not { } attributes)
        {
            return (null, "source-unreadable");
        }

        return (attributes & FileAttributes.Directory) != 0
            ? (null, "source-not-file")
            : (physical.GetContainedPhysicalPath(), null);
    }

    private static string SourcePhysicalFailureCode(PhysicalPathState state)
        => state switch
        {
            PhysicalPathState.Missing => "source-missing",
            PhysicalPathState.Inaccessible or PhysicalPathState.InputOutputFailure => "source-unreadable",
            PhysicalPathState.Invalid => "source-malformed",
            PhysicalPathState.Dangling
                or PhysicalPathState.External
                or PhysicalPathState.Cycle
                or PhysicalPathState.Unsupported => "source-unsafe",
            PhysicalPathState.Contained => throw new ArgumentOutOfRangeException(nameof(state), state, null),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
        };

    private static string SourceReadFailureCode(FileReadState state)
        => state switch
        {
            FileReadState.Missing => "source-missing",
            FileReadState.InvalidEncoding => "invalid-encoding",
            FileReadState.InvalidSyntax => "markdown-unavailable",
            FileReadState.AccessDenied or FileReadState.InputOutputFailure => "source-unreadable",
            FileReadState.Cancelled => throw new OperationCanceledException(),
            FileReadState.Complete => throw new ArgumentOutOfRangeException(nameof(state), state, null),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
        };

    private static bool IsOutsideOfflineGate(string sourcePath, string rawDestination)
    {
        if (rawDestination.StartsWith("//", StringComparison.Ordinal))
        {
            return true;
        }

        if (RepositoryMarkdownScope.IsPublishedSiteSource(sourcePath)
            && (rawDestination.StartsWith("/docs/", StringComparison.Ordinal)
                || rawDestination.StartsWith("/guides/", StringComparison.Ordinal)
                || rawDestination.StartsWith("/img/", StringComparison.Ordinal)))
        {
            return true;
        }

        if (!Uri.TryCreate(rawDestination, UriKind.Absolute, out var uri) || uri is null)
        {
            return false;
        }

        return uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase)
            || uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)
            || uri.Scheme.Equals("mailto", StringComparison.OrdinalIgnoreCase);
    }

    private static string FormatDiagnostic(
        string sourcePath,
        string? source,
        int offset,
        string code,
        string destination)
    {
        var (line, column) = source is null
            ? (1, 1)
            : ReadLineAndColumn(source, offset);
        return $"{sourcePath}:{line}:{column}: {code}: {destination}";
    }

    private static (int Line, int Column) ReadLineAndColumn(string source, int offset)
    {
        if (offset < 0 || offset > source.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), offset, "A Markdown span offset is outside the source.");
        }

        var line = 1;
        var lineStart = 0;
        for (var index = 0; index < offset; index++)
        {
            if (source[index] == '\n')
            {
                line++;
                lineStart = index + 1;
            }
        }

        return (line, offset - lineStart + 1);
    }

    private static string ValidateRepositoryRoot(string repositoryRoot)
    {
        if (!Path.IsPathFullyQualified(repositoryRoot))
        {
            throw new ArgumentException(
                "The repository root must be an explicit absolute path.",
                nameof(repositoryRoot));
        }

        var root = Path.GetFullPath(repositoryRoot);
        var rootComponent = LinkTargetReader.Read(root);
        if (rootComponent.State != PathComponentState.Ordinary
            || rootComponent.Attributes is not { } rootAttributes
            || (rootAttributes & FileAttributes.Directory) == 0)
        {
            throw new InvalidOperationException("The repository root must be an ordinary directory.");
        }

        var solutionPath = Path.Combine(root, SolutionFileName);
        var solutionComponent = LinkTargetReader.Read(solutionPath);
        if (solutionComponent.State != PathComponentState.Ordinary
            || solutionComponent.Attributes is not { } solutionAttributes
            || (solutionAttributes & FileAttributes.Directory) != 0)
        {
            throw new InvalidOperationException("The repository root must contain an ordinary OpenForge.Cli.slnx file.");
        }

        return root;
    }
}
