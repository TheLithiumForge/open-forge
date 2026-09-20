using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Presentation.Shared.Text.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Invariants;

public sealed class CliReportInvariantsTests
{
    private static readonly Regex SnapshotFilePattern = new(
        @"^(?<situation>.+?)(?:\.json)?\.(?<detail>minimal|standard|full|debug)(?<diagnostics>\.diagnostics)?\.txt$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex TextSeverityPattern = new(
        @"^\s{2}(?<severity>Error|Warning|Info)\s{2}",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex CatalogueCodePattern = new(
        @"(?<![A-Za-z0-9-])[a-z][a-z0-9-]*\.[a-z][a-z0-9-]*(?![A-Za-z0-9-])",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex VisibleEscapePattern = new(
        @"\\(?:n|r|t|u[0-9A-Fa-f]{4})",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // This is the one code emitted by the merged packet that is named in the
    // command's situation notes but not yet in its Findings catalogue table.
    // Keep the exception explicit until the command owner decides whether to
    // add the row or rename the code.
    private static readonly IReadOnlySet<string> CatalogueGapsUnderReview =
        new HashSet<string>(["extension-list.installed-source-missing"], StringComparer.Ordinal);

    // Rows whose shipped sentence and contract text genuinely disagree today. Each is a wording
    // decision rather than a typo, so they stay named here until the command owner rules, and this
    // set must only shrink.
    private static readonly IReadOnlySet<string> ContractDriftUnderReview =
        new HashSet<string>(StringComparer.Ordinal);

    private static readonly IReadOnlySet<string> ForbiddenText = new HashSet<string>(
        ["Status:", "Selected by:", "not-applicable", "not-requested", "residual: none", "Preflight", "Lifecycle:"],
        StringComparer.Ordinal);

    // These fields are deliberately null when a command status makes the
    // measurement inapplicable. A measured-but-unavailable count must still
    // link to a limitation below; this allow-list keeps the distinction
    // explicit until the shared JSON contract decides whether such fields
    // should be omitted instead.
    private static readonly IReadOnlySet<string> KnownNotApplicableCountNames = new HashSet<string>(
        [
            "addedBytes", "addedFiles", "addedTokens", "allTokens", "available", "dependencies", "depth",
            "descendants", "directChildren", "entriesSectionsCurrent", "entriesSectionsMissing",
            "entriesSectionsStale", "extensionsInstalled", "filesChanged", "filesMissing", "filesNew",
            "filesRetired", "filesUnchanged", "frameworkFilesChanged", "frameworkFilesCurrent",
            "frameworkFilesMissing", "incoming", "librariesRegistered", "libraryLinksChanged",
            "libraryLinksCurrent", "libraryLinksMissing", "loadNowBytes", "loadNowFiles", "loadNowTokens",
            "mayLoadAgainFiles", "mayLoadAgainTokens", "outgoing", "ownBytes", "ownTokens", "recoveryBundles",
            "recoveryDrafts", "rootCategories", "routedFiles", "sourcesCandidates", "sourcesInspected",
            "sourcesScanned", "startupFiles", "startupShare", "startupTokens"
        ],
        StringComparer.Ordinal);

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation-invariants"), Trait("Evidence", "Integration")]
    public void NativeTextSnapshotsSatisfyVocabularyEncodingEscapingSubjectsAndNextRules()
    {
        var corpus = SnapshotCorpus.Load();
        Assert.NotEmpty(corpus.TextSnapshots);

        foreach (var snapshot in corpus.TextSnapshots)
        {
            var text = ReadUtf8(snapshot.Path);
            AssertTextSnapshot(text, snapshot.Path, checkVocabulary: snapshot.IsHumanPrimary);
        }

        Assert.NotEmpty(corpus.HumanSnapshots);
        foreach (var snapshot in corpus.HumanSnapshots)
        {
            var text = ReadUtf8(snapshot.Path);
            AssertSeverityOrdering(text, snapshot.Path);
            AssertNextRule(text, snapshot.Path);
        }
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation-invariants"), Trait("Evidence", "Integration")]
    public void NativeJsonSnapshotsSatisfyEnvelopeMembershipScalarAndCatalogueRules()
    {
        var corpus = SnapshotCorpus.Load();
        var catalogueCodes = ReadCatalogueCodes();
        var validEnvelopeCount = 0;
        var shellDiagnosticCount = 0;

        foreach (var snapshot in corpus.JsonSnapshots)
        {
            var content = ReadUtf8(snapshot.Path);
            try
            {
                using var document = JsonDocument.Parse(content);
                validEnvelopeCount++;
                AssertJsonEnvelope(document.RootElement, snapshot, catalogueCodes, checkCatalogue: true);
            }
            catch (JsonException)
            {
                shellDiagnosticCount++;
                Assert.NotEmpty(content);
                Assert.DoesNotContain("schemaVersion", content, StringComparison.Ordinal);
            }
        }

        Assert.True(validEnvelopeCount > 0, "The native snapshot corpus contains no JSON envelopes.");
        Assert.True(shellDiagnosticCount > 0, "The shell-diagnostic boundary was not exercised.");
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation-invariants"), Trait("Evidence", "Integration")]
    public void NativeSnapshotsSatisfyLaddersDebugStdoutAndCrossFormatParity()
    {
        var corpus = SnapshotCorpus.Load();
        var jsonByKey = new Dictionary<string, SnapshotFile>(StringComparer.Ordinal);
        foreach (var snapshot in corpus.JsonSnapshots)
        {
            var content = ReadUtf8(snapshot.Path);
            try
            {
                using var document = JsonDocument.Parse(content);
                jsonByKey.Add(snapshot.Key, snapshot);
            }
            catch (JsonException)
            {
                // Parser failures before binding are text on stderr, not report envelopes.
            }
        }

        var groups = corpus.HumanSnapshots.GroupBy(snapshot => snapshot.SituationKey, StringComparer.Ordinal).ToArray();
        Assert.NotEmpty(groups);
        foreach (var group in groups)
        {
            var byDetail = group.ToDictionary(snapshot => snapshot.Detail, StringComparer.Ordinal);
            Assert.Equal(4, byDetail.Count);
            Assert.True(byDetail.ContainsKey("minimal"), group.Key);
            Assert.True(byDetail.ContainsKey("standard"), group.Key);
            Assert.True(byDetail.ContainsKey("full"), group.Key);
            Assert.True(byDetail.ContainsKey("debug"), group.Key);

            var minimalLines = CountLines(ReadUtf8(byDetail["minimal"].Path));
            var standardLines = CountLines(ReadUtf8(byDetail["standard"].Path));
            var fullLines = CountLines(ReadUtf8(byDetail["full"].Path));
            Assert.True(minimalLines <= standardLines, $"{group.Key}: {minimalLines} > {standardLines}");
            Assert.True(standardLines <= fullLines, $"{group.Key}: {standardLines} > {fullLines}");

            var fullText = ReadUtf8(byDetail["full"].Path);
            var debugText = ReadUtf8(byDetail["debug"].Path);
            Assert.True(
                NormalizeLineEndings(fullText) == NormalizeLineEndings(debugText),
                $"Debug text changed the primary output for {group.Key}.");

            foreach (var detail in new[] { "minimal", "standard", "full", "debug" })
            {
                var human = byDetail[detail];
                var jsonKey = $"{human.SituationKey}|{detail}";
                if (!jsonByKey.TryGetValue(jsonKey, out var json))
                {
                    // The 32 parser-failure cases intentionally have no JSON envelope.
                    continue;
                }

                using var document = JsonDocument.Parse(ReadUtf8(json.Path));
                AssertTextJsonParity(ReadUtf8(human.Path), document.RootElement, human.Path);
            }
        }
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation-invariants"), Trait("Evidence", "Integration")]
    public void SyntheticReportsSatisfyTheSameInvariantsAtEveryDetailLevel()
    {
        var report = SyntheticReport();
        var catalogueCodes = ReadCatalogueCodes();
        var textByDetail = new Dictionary<string, string>(StringComparer.Ordinal);
        var jsonByDetail = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var detail in Enum.GetValues<CliDetail>())
        {
            var selected = CliReportTrimmer.Trim(
                report,
                new CliSelection(detail, new HashSet<CliSeverity>(Enum.GetValues<CliSeverity>())),
                CliCommandShape.ChangeReport);
            var text = CliTextRenderer.Render(
                selected,
                CliTextStyle.Plain,
                static (_, _, _) => new CliTextDocument([CliTextSpan.FromAuthored(new CliAuthoredSpan("café\n"))])).Content;
            var json = CliJsonRenderer.Render(selected, InvariantDataContext.Default.SyntheticData);
            textByDetail[CliReportVocabulary.Name(detail)] = text;
            jsonByDetail[CliReportVocabulary.Name(detail)] = json;

            AssertTextSnapshot(text, $"synthetic/{CliReportVocabulary.Name(detail)}.txt", checkVocabulary: true);
            using var document = JsonDocument.Parse(json);
            AssertJsonEnvelope(
                document.RootElement,
                new SnapshotFile($"synthetic/{CliReportVocabulary.Name(detail)}{CommandOutputSnapshot.JsonContentNameSegment}txt", "synthetic", CliReportVocabulary.Name(detail), IsJson: true, IsDiagnostics: false),
                catalogueCodes,
                checkCatalogue: false);
            AssertTextJsonParity(text, document.RootElement, $"synthetic/{CliReportVocabulary.Name(detail)}");
        }

        Assert.True(CountLines(textByDetail["minimal"]) <= CountLines(textByDetail["standard"]));
        Assert.True(CountLines(textByDetail["standard"]) <= CountLines(textByDetail["full"]));
        Assert.Equal(textByDetail["full"], textByDetail["debug"]);
        Assert.DoesNotContain("\\u", jsonByDetail["full"], StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact, Trait("Feature", "cli-presentation-invariants"), Trait("Evidence", "Integration")]
    public void NoTwoFindingCodesOfOneCommandRenderTheSameMessage()
    {
        // Two commands may share a sentence: that is what the shared message families
        // are for, and a held workspace lock reads identically in thirteen commands.
        // Two codes of the *same* command sharing one sentence is the signature of an
        // overloaded finding code, where one code carries situations that need
        // different sentences and no single message can be right for all of them.
        var corpus = SnapshotCorpus.Load();
        var codesByCommandAndMessage =
            new Dictionary<(string Command, string Message), SortedSet<string>>();

        foreach (var snapshot in corpus.JsonSnapshots.Where(static file => !file.IsDiagnostics))
        {
            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(ReadUtf8(snapshot.Path));
            }
            catch (JsonException)
            {
                continue;
            }

            using (document)
            {
                if (!document.RootElement.TryGetProperty("findings", out var findings)
                    || findings.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var finding in findings.EnumerateArray())
                {
                    if (!finding.TryGetProperty("code", out var codeElement)
                        || !finding.TryGetProperty("message", out var messageElement)
                        || codeElement.GetString() is not { } code
                        || messageElement.GetString() is not { } message)
                    {
                        continue;
                    }

                    var separator = code.IndexOf('.', StringComparison.Ordinal);
                    if (separator <= 0)
                    {
                        continue;
                    }

                    var key = (code[..separator], message);
                    if (!codesByCommandAndMessage.TryGetValue(key, out var codes))
                    {
                        codes = new SortedSet<string>(StringComparer.Ordinal);
                        codesByCommandAndMessage[key] = codes;
                    }

                    codes.Add(code);
                }
            }
        }

        Assert.NotEmpty(codesByCommandAndMessage);
        var collisions = codesByCommandAndMessage
            .Where(static entry => entry.Value.Count > 1)
            .Select(static entry =>
                $"{entry.Key.Command}: {string.Join(", ", entry.Value)} all render \"{entry.Key.Message}\"")
            .OrderBy(static description => description, StringComparer.Ordinal)
            .ToArray();

        Assert.True(
            collisions.Length == 0,
            "Finding codes of one command must not share a message: "
            + string.Join("; ", collisions));
    }

    [Trait("Boundary", "Architecture")]
    [Fact(DisplayName = "Every command output snapshot directory has a live owning test"), Trait("Feature", "cli-presentation-invariants"), Trait("Evidence", "Integration")]
    public void CommandOutputSnapshotDirectoriesHaveLiveOwners()
    {
        var directories = SnapshotDirectories();
        Assert.NotEmpty(directories);

        // Each `__snapshots__` holds one directory per owning test class, so ownership is checked
        // per owner rather than per `__snapshots__`. Asking only whether a `__snapshots__` has
        // *some* live owner would pass a stale owner sitting beside a live one, which is exactly
        // how eight orphaned `DoctorLifecycleOutputSnapshotTests` captures survived until a manual
        // sweep found them. Today every `__snapshots__` holds exactly one owner, so the two forms
        // agree; this one still agrees when that stops being true.
        //
        // This guard can inspect only snapshot directories present in the corpus. A live test
        // situation that has never produced a capture is invisible to this check.
        var owners = directories
            .SelectMany(static directory => Directory.EnumerateDirectories(
                directory, "*", SearchOption.TopDirectoryOnly))
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();
        Assert.NotEmpty(owners);

        var orphans = owners
            .Where(static owner => !HasLiveOwner(owner))
            .Select(owner => Path.GetRelativePath(RepositoryRoot(), owner))
            .ToArray();

        Assert.True(
            orphans.Length == 0,
            "Snapshot directories without live owning tests: " + string.Join(", ", orphans));
    }

    [Trait("Boundary", "Architecture")]
    [Fact, Trait("Feature", "cli-presentation-invariants"), Trait("Evidence", "Integration")]
    public void CommandAndFrameworkSourcesDoNotReferenceThePresentationLayer()
    {
        var root = RepositoryRoot();
        foreach (var (folder, assembly, directoryName) in new[]
        {
            ("operations", "OpenForge.Cli.Operations", "Commands"),
            ("framework", "OpenForge.Cli.Framework", "Framework"),
        })
        {
            var directory = Path.Combine(root, "src", "cli", folder, assembly, directoryName);
            var sources = Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories).ToArray();
            Assert.NotEmpty(sources);
            foreach (var source in sources)
            {
                var content = File.ReadAllText(source);
                Assert.DoesNotContain("OpenForge.Cli.Core.Presentation", content, StringComparison.Ordinal);
            }
        }
    }

    private static void AssertTextSnapshot(string content, string path, bool checkVocabulary)
    {
        Assert.NotEmpty(content);
        Assert.DoesNotContain('\0', content);

        if (checkVocabulary)
        {
            foreach (var forbidden in ForbiddenText)
            {
                Assert.DoesNotContain(forbidden, content, StringComparison.Ordinal);
            }
        }

        var remaining = VisibleEscapePattern.Replace(content, string.Empty);
        Assert.True(!remaining.Contains('\\'), $"Unexpected backslash in {path}: {content}");
        AssertAsciiFraming(content, path);
    }

    private static void AssertAsciiFraming(string content, string path)
    {
        foreach (var line in NormalizeLineEndings(content).Split('\n'))
        {
            if (!line.Any(character => character > 0x7f))
            {
                continue;
            }

            var trimmed = line.TrimStart();
            var generated = trimmed.StartsWith("Workspace:", StringComparison.Ordinal)
                || trimmed.StartsWith("Next:", StringComparison.Ordinal)
                || TextSeverityPattern.IsMatch(line)
                || Regex.IsMatch(trimmed, @"^\S.*\s{2}\S", RegexOptions.CultureInvariant);
            Assert.False(generated, $"Non-ASCII framing in {path}: {line}");
        }
    }

    private static void AssertSeverityOrdering(string content, string path)
    {
        var previous = -1;
        foreach (Match match in TextSeverityPattern.Matches(NormalizeLineEndings(content)))
        {
            var current = match.Groups["severity"].Value switch
            {
                "Error" => 0,
                "Warning" => 1,
                "Info" => 2,
                _ => throw new InvalidOperationException("The text severity vocabulary is incomplete."),
            };
            Assert.True(current >= previous, $"Severity order in {path} is not errors, warnings, infos.");
            previous = current;
        }
    }

    private static void AssertNextRule(string content, string path)
    {
        var lines = NormalizeLineEndings(content).TrimEnd('\n').Split('\n', StringSplitOptions.None);
        var next = lines.Select((line, index) => (line, index))
            .Where(item => item.line.StartsWith("Next:", StringComparison.Ordinal))
            .ToArray();
        Assert.True(next.Length <= 1, $"More than one Next line in {path}.");
        if (next.Length == 1)
        {
            var trailing = lines[(next[0].index + 1)..];
            var catalogueRequiredContinuation = path.Contains(
                    "ExtensionInstallBeforeOutputSnapshotTests",
                    StringComparison.Ordinal)
                && trailing.Length == 1
                && trailing[0].StartsWith("  Or add ", StringComparison.Ordinal)
                && trailing[0].Contains("allowInstallPaths", StringComparison.Ordinal);
            Assert.True(
                next[0].index == lines.Length - 1 || catalogueRequiredContinuation,
                $"Next is not the last line in {path}.");
        }
    }

    private static void AssertJsonEnvelope(
        JsonElement root,
        SnapshotFile snapshot,
        IReadOnlySet<string> catalogueCodes,
        bool checkCatalogue)
    {
        Assert.Equal(JsonValueKind.Object, root.ValueKind);
        Assert.Equal(3, Required(root, "schemaVersion").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(RequiredString(root, "command")));
        Assert.False(string.IsNullOrWhiteSpace(RequiredString(root, "status")));
        var detailName = RequiredString(root, "detail");
        Assert.Equal(snapshot.Detail, detailName);

        var findings = Required(root, "findings");
        var effects = Required(root, "effects");
        var counts = Required(root, "counts");
        var limitations = Required(root, "limitations");
        Assert.Equal(JsonValueKind.Array, findings.ValueKind);
        Assert.Equal(JsonValueKind.Array, effects.ValueKind);
        Assert.Equal(JsonValueKind.Object, counts.ValueKind);
        Assert.Equal(JsonValueKind.Array, limitations.ValueKind);

        AssertJsonSeverityOrdering(findings, snapshot.Path);
        foreach (var finding in findings.EnumerateArray())
        {
            var code = RequiredString(finding, "code");
            if (checkCatalogue)
            {
                Assert.True(catalogueCodes.Contains(code) || CatalogueGapsUnderReview.Contains(code),
                    $"Finding code {code} in {snapshot.Path} is absent from a Findings catalogue table.");
            }
            AssertSubject(Required(finding, "subject"), snapshot.Path);
            AssertJsonMembership(finding, detailName, snapshot.Path);
            if (Required(finding, "actions").ValueKind != JsonValueKind.Array)
            {
                Assert.Fail($"Finding actions are not an array in {snapshot.Path}.");
            }

            AssertBlockingFindingHasConcreteNextAction(root, finding, snapshot.Path);

            if (finding.TryGetProperty("candidates", out var candidates))
            {
                foreach (var candidate in candidates.EnumerateArray())
                {
                    AssertSubject(Required(candidate, "subject"), snapshot.Path);
                }
            }
        }

        foreach (var effect in effects.EnumerateArray())
        {
            Assert.False(string.IsNullOrWhiteSpace(RequiredString(effect, "path")), snapshot.Path);
            AssertJsonMembership(effect, detailName, snapshot.Path);
        }

        foreach (var count in counts.EnumerateObject())
        {
            Assert.True(count.Value.ValueKind is JsonValueKind.Number or JsonValueKind.Null,
                $"Count {count.Name} in {snapshot.Path} is not a number or null.");
            if (count.Value.ValueKind == JsonValueKind.Null)
            {
                var normalizedName = NormalizeIdentity(count.Name);
                var linked = limitations.EnumerateArray().Any(limitation =>
                    limitation.TryGetProperty("what", out var what)
                    && what.ValueKind == JsonValueKind.String
                    && NormalizeIdentity(what.GetString() ?? string.Empty).StartsWith(
                        normalizedName,
                        StringComparison.Ordinal));
                // A null count without an UnavailableReason is the intentional
                // not-measured/not-applicable state of a command situation. When
                // the report carries a reason, the trimmer must expose it as a
                // matching limitation; that linkage is checked here. The
                // allow-list is deliberately finite so a new silent null
                // cannot enter the corpus unnoticed.
                Assert.True(
                    linked || KnownNotApplicableCountNames.Contains(count.Name),
                    $"Null count {count.Name} in {snapshot.Path} has no limitation or approved not-applicable classification.");
            }
        }

        foreach (var limitation in limitations.EnumerateArray())
        {
            Assert.False(string.IsNullOrWhiteSpace(RequiredString(limitation, "what")), snapshot.Path);
            Assert.False(string.IsNullOrWhiteSpace(RequiredString(limitation, "why")), snapshot.Path);
        }
    }

    private static void AssertBlockingFindingHasConcreteNextAction(
        JsonElement root,
        JsonElement finding,
        string path)
    {
        if (!finding.TryGetProperty("resolution", out var resolution)
            || resolution.ValueKind != JsonValueKind.String
            || !string.Equals(resolution.GetString(), "blocked-repair", StringComparison.Ordinal))
        {
            return;
        }

        var actionCommands = finding.GetProperty("actions")
            .EnumerateArray()
            .Select(action => RequiredString(action, "command"))
            .ToArray();
        foreach (var command in actionCommands)
        {
            Assert.False(string.IsNullOrWhiteSpace(command), path);
        }

        var nextCommand = root.TryGetProperty("next", out var next)
            && next.ValueKind == JsonValueKind.Object
            && next.TryGetProperty("command", out var nextCommandValue)
            && nextCommandValue.ValueKind == JsonValueKind.String
            ? nextCommandValue.GetString()
            : null;
        var commands = actionCommands.Length > 0
            ? actionCommands
            : string.IsNullOrWhiteSpace(nextCommand) ? [] : [nextCommand!];

        Assert.True(commands.Length > 0, $"Blocking finding in {path} has no rendered next command.");
        foreach (var command in commands)
        {
            Assert.StartsWith("open-forge ", command, StringComparison.Ordinal);
            Assert.DoesNotContain('<', command);
            Assert.DoesNotContain('>', command);
        }
    }

    private static void AssertJsonSeverityOrdering(JsonElement findings, string path)
    {
        var previous = -1;
        foreach (var finding in findings.EnumerateArray())
        {
            var current = RequiredString(finding, "severity") switch
            {
                "error" => 0,
                "warning" => 1,
                "info" => 2,
                var value => throw new InvalidOperationException($"Unknown JSON severity {value} in {path}."),
            };
            Assert.True(current >= previous, $"JSON severity order in {path} is not errors, warnings, infos.");
            previous = current;
        }
    }

    private static void AssertJsonMembership(JsonElement item, string detailName, string path)
    {
        var detail = detailName switch
        {
            "minimal" => CliDetail.Minimal,
            "standard" => CliDetail.Standard,
            "full" => CliDetail.Full,
            "debug" => CliDetail.Debug,
            _ => throw new InvalidOperationException($"Unknown detail {detailName} in {path}."),
        };
        var full = detail >= CliDetail.Full;
        var standard = detail >= CliDetail.Standard;

        if (item.TryGetProperty("resolution", out var resolution))
        {
            Assert.True(standard, path);
            Assert.True(resolution.ValueKind is JsonValueKind.String or JsonValueKind.Null, path);
        }

        foreach (var property in new[] { "candidates", "evidence" })
        {
            if (item.TryGetProperty(property, out var value))
            {
                Assert.True(full, path);
                Assert.Equal(JsonValueKind.Array, value.ValueKind);
            }
        }

        if (item.TryGetProperty("provenance", out var provenance))
        {
            Assert.True(full, path);
            Assert.True(provenance.ValueKind is JsonValueKind.Object or JsonValueKind.Null, path);
        }

        foreach (var property in new[] { "before", "after" })
        {
            if (item.TryGetProperty(property, out var value))
            {
                Assert.True(full, path);
                Assert.True(value.ValueKind is JsonValueKind.String or JsonValueKind.Null, path);
            }
        }
    }

    private static void AssertSubject(JsonElement subject, string path)
    {
        Assert.Equal(JsonValueKind.Object, subject.ValueKind);
        var hasPath = subject.TryGetProperty("path", out var pathValue)
            && pathValue.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(pathValue.GetString());
        var hasId = subject.TryGetProperty("id", out var idValue)
            && idValue.ValueKind == JsonValueKind.String
            && !string.IsNullOrWhiteSpace(idValue.GetString());
        Assert.True(hasPath || hasId, $"A listed subject in {path} has neither a path nor an identifier.");
    }

    private static void AssertTextJsonParity(string text, JsonElement root, string path)
    {
        var cursor = 0;
        var detail = RequiredString(root, "detail");
        var headline = Required(root, "summary").GetProperty("headline").GetString() ?? string.Empty;
        var findingCount = Required(root, "findings").GetArrayLength();
        foreach (var finding in Required(root, "findings").EnumerateArray())
        {
            var subject = TextIdentity(SubjectText(Required(finding, "subject")));
            var title = RequiredString(finding, "title");
            var marker = $"{CliText.Escape(subject)}  {CliText.Escape(title)}";
            var found = text.IndexOf(marker, cursor, StringComparison.Ordinal);
            var subjectFound = text.IndexOf(CliText.Escape(subject), cursor, StringComparison.Ordinal) >= 0;
            if (found < 0
                && (subjectFound
                    || detail == "minimal"
                    && (headline.Contains(subject, StringComparison.Ordinal)
                        || headline.Contains(title, StringComparison.Ordinal)
                        || HeadlineRepresentsFinding(headline, RequiredString(finding, "message"))
                        || findingCount == 1 && FirstLine(text) == headline
                        || Required(root, "effects").EnumerateArray()
                            .Select(effect => RequiredString(effect, "path"))
                            .Select(TextIdentity)
                            .Contains(subject, StringComparer.Ordinal))
                    || RequiredString(root, "status") == "cancelled"
                    && findingCount == 1
                    && FirstLine(text) == headline))
            {
                // The shared trimmer deliberately removes the single headline
                // finding from minimal text after moving its message to the
                // headline. JSON retains the finding identity at every level.
                continue;
            }

            Assert.True(found >= 0, $"Finding {subject}/{title} is missing or reordered in {path}.");
            cursor = found + marker.Length;
        }

        var routeMoveCatalogueProjection = path.Contains(
            "RouteMoveBeforeOutputSnapshotTests",
            StringComparison.Ordinal);
        var routeRemoveCatalogueProjection = path.Contains(
            "RouteRemoveBeforeOutputSnapshotTests",
            StringComparison.Ordinal);
        var routeUpdateCatalogueProjection = path.Contains(
            "RouteUpdateBeforeOutputSnapshotTests",
            StringComparison.Ordinal);
        foreach (var effect in Required(root, "effects").EnumerateArray())
        {
            var effectPath = TextIdentity(RequiredString(effect, "path"));
            if (routeMoveCatalogueProjection && detail == "minimal")
            {
                // The Route Move catalogue deliberately summarizes the
                // minimal receipt instead of listing every effect identity.
                // The other detail levels list identities, but pair old/new
                // paths and therefore cannot preserve the JSON receipt order.
                continue;
            }

            if (routeMoveCatalogueProjection)
            {
                Assert.Contains(CliText.Escape(effectPath), text, StringComparison.Ordinal);
                continue;
            }

            if (routeUpdateCatalogueProjection
                && path.Contains("PartialWriteFailure", StringComparison.Ordinal))
            {
                // The partial route-update catalogue shows the changed field
                // and the stopped-after summary, not the physical effect
                // paths whose later receipts were not started.
                continue;
            }

            if (routeUpdateCatalogueProjection
                && RequiredString(root, "status") is not "completed")
            {
                // Blocked and incomplete route-update catalogues carry the
                // logical target in the headline/finding and omit planned
                // physical effect rows.
                continue;
            }

            if (routeUpdateCatalogueProjection
                && detail == "minimal"
                && Required(root, "data").TryGetProperty("target", out var target)
                && target.TryGetProperty("id", out var targetId)
                && targetId.ValueKind == JsonValueKind.String
                && string.Equals(
                    effectPath,
                    $".agents/{targetId.GetString()}.md",
                    StringComparison.Ordinal))
            {
                // The route-update catalogue uses the routed logical ID in
                // the minimal headline and omits the physical .agents path.
                continue;
            }

            if (routeRemoveCatalogueProjection)
            {
                if (detail == "minimal"
                    && path.Contains("category-removed.minimal.txt", StringComparison.Ordinal))
                {
                    // The route-remove catalogue explicitly permits an
                    // ellipsis in a category's minimal deletion list.
                    continue;
                }

                // Route Remove prints deletions before updated sections and
                // detached links, while the JSON receipt is grouped by its
                // operation kinds. Every non-ellipsized identity is still
                // required, but the catalogue projection owns the order.
                Assert.Contains(CliText.Escape(effectPath), text, StringComparison.Ordinal);
                continue;
            }

            if (detail == "minimal"
                && path.Contains("LibraryDetachBeforeOutputSnapshotTests", StringComparison.Ordinal))
            {
                // The Library Detach catalogue summarizes removed links at
                // minimal and lists individual receipts from standard onward.
                continue;
            }

            if (RequiredString(effect, "kind") == "directory")
            {
                // Human Extension Install output lists file receipts and
                // summarizes the directories created to contain them.
                continue;
            }

            if (RequiredString(effect, "kind") is "setting" or "record"
                && (path.Contains("LibraryAttachBeforeOutputSnapshotTests", StringComparison.Ordinal)
                    || RequiredString(effect, "kind") == "record"
                        && detail == "minimal"
                        && path.Contains("LibrarySyncBeforeOutputSnapshotTests", StringComparison.Ordinal)))
            {
                // The Library Attach catalogue reports this settings write as
                // the saved grant line instead of repeating its path.
                continue;
            }

            var escapedPath = CliText.Escape(effectPath);
            var found = text.IndexOf(escapedPath, cursor, StringComparison.Ordinal);
            if (found < 0
                && RequiredString(effect, "action") == "created"
                && ((detail == "minimal"
                        && (path.Contains("ExtensionInstallBeforeOutputSnapshotTests", StringComparison.Ordinal)
                            || path.Contains("InstallBeforeOutputSnapshotTests", StringComparison.Ordinal)
                            || path.Contains("LibraryAttachBeforeOutputSnapshotTests", StringComparison.Ordinal)))
                    || path.Contains("InstallBeforeOutputSnapshotTests", StringComparison.Ordinal)
                        && RequiredString(effect, "outcome") == "not-started"
                    || path.Contains("LibraryAttachBeforeOutputSnapshotTests", StringComparison.Ordinal)
                        && RequiredString(effect, "outcome") == "not-started"))
            {
                // The Extension Install catalogue deliberately summarizes
                // created paths at minimal instead of listing each path.
                continue;
            }

            Assert.True(found >= 0, $"Effect {effectPath} is missing or reordered in {path}.");
            cursor = found + escapedPath.Length;
        }
    }

    private static bool HeadlineRepresentsFinding(string headline, string message)
    {
        var comparableMessage = Regex.Replace(
            message,
            @"\s+at\s+<[^>]+>",
            " ",
            RegexOptions.CultureInvariant);
        comparableMessage = Regex.Replace(
            comparableMessage,
            @"\s+",
            " ",
            RegexOptions.CultureInvariant).Trim().TrimEnd('.');
        var firstSentence = comparableMessage.Split(". ", 2, StringSplitOptions.None)[0].TrimEnd('.');
        return headline.Contains(comparableMessage, StringComparison.OrdinalIgnoreCase)
            || headline.Contains(firstSentence, StringComparison.OrdinalIgnoreCase);
    }

    private static string FirstLine(string content)
        => NormalizeLineEndings(content).Split('\n', 2, StringSplitOptions.None)[0];

    private static string SubjectText(JsonElement subject)
    {
        var value = subject.TryGetProperty("path", out var path) && path.ValueKind == JsonValueKind.String
            ? path.GetString()
            : null;
        value ??= subject.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String
            ? id.GetString()
            : null;
        Assert.False(string.IsNullOrWhiteSpace(value));
        if (subject.TryGetProperty("location", out var location) && location.ValueKind == JsonValueKind.Object)
        {
            value = $"{value}:{location.GetProperty("line").GetInt32()}:{location.GetProperty("column").GetInt32()}";
        }

        return value!;
    }

    private static string TextIdentity(string value)
        => value.Replace('\\', '/');

    private static CliReport<SyntheticData> SyntheticReport() => new()
    {
        Command = "invariant-test",
        Status = CliSemanticStatus.Complete,
        Headline = new CliHeadline("Synthetic report is complete.", CliHeadlineKind.Done),
        Workspace = new CliWorkspaceEcho("workspace", false),
        Findings =
        [
            SyntheticFinding(CliSeverity.Error, "synthetic.error", "a.md"),
            SyntheticFinding(CliSeverity.Warning, "synthetic.warning", "b.md"),
            SyntheticFinding(CliSeverity.Info, "synthetic.info", "c.md"),
        ],
        Effects =
        [
            new CliEffect
            {
                Path = "changed.md",
                Kind = CliEffectKind.File,
                Action = CliEffectAction.Replaced,
                Outcome = CliEffectOutcome.Done,
                Before = "before",
                After = "after",
            },
        ],
        Counts = [new CliCount("measured", "measured files", 2), new CliCount("unknown", "unknown files", null, "The boundary could not be read.")],
        Limitations = [new CliLimitation("unknown files", "The boundary could not be read.")],
        Data = new SyntheticData("value"),
        Next = new CliNextAction("open-forge status", "Review the result."),
        Diagnostics = ["synthetic diagnostics"],
    };

    private static CliFinding SyntheticFinding(CliSeverity severity, string code, string path) => new()
    {
        Severity = severity,
        Code = code,
        Title = "Synthetic finding",
        Message = "Synthetic message.",
        Subject = new CliSubject(CliSubjectKind.File, path),
        Actions = [new CliNextAction("open-forge status", "Review the finding.")],
        Resolution = CliResolution.ManualDecision,
        Candidates = [new CliCandidate(new CliSubject(CliSubjectKind.File, "candidate.md"), ["candidate reason"])],
        Evidence = [new CliEvidence("source", "value")],
        Provenance = new CliProvenance("synthetic", path),
    };

    [Trait("Boundary", "Architecture")]
    [Fact(DisplayName = "Every captured finding message matches the contract row that documents it"), Trait("Feature", "cli-presentation-invariants"), Trait("Evidence", "Integration")]
    public void CapturedMessagesMatchTheirContractRow()
    {
        // The command contracts are what a reader is promised, and nothing compared them to what
        // the commands actually print. A row could therefore describe a sentence the code stopped
        // emitting, which is how `extension-remove.lifecycle-observation` came to document Update's
        // wording. This walks the other way: every rendered message must match the row for its code.
        var templates = ReadContractMessageTemplates();
        Assert.NotEmpty(templates);
        var corpus = SnapshotCorpus.Load();
        var compared = 0;
        var drifted = new List<string>();
        foreach (var snapshot in corpus.JsonSnapshots)
        {
            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(ReadUtf8(snapshot.Path));
            }
            catch (JsonException)
            {
                continue;
            }

            using (document)
            {
                if (!document.RootElement.TryGetProperty("findings", out var findings)
                    || findings.ValueKind != JsonValueKind.Array)
                {
                    continue;
                }

                foreach (var finding in findings.EnumerateArray())
                {
                    if (!finding.TryGetProperty("code", out var codeValue)
                        || !finding.TryGetProperty("message", out var messageValue)
                        || codeValue.ValueKind != JsonValueKind.String
                        || messageValue.ValueKind != JsonValueKind.String)
                    {
                        continue;
                    }

                    var code = codeValue.GetString()!;
                    if (!templates.TryGetValue(code, out var forms) || ContractDriftUnderReview.Contains(code))
                    {
                        continue;
                    }

                    compared++;
                    var message = messageValue.GetString()!;
                    if (!forms.Any(form => MatchesContractForm(form, message)))
                    {
                        drifted.Add($"{code}\n     contract: {forms[0]}\n     rendered: {message}");
                    }
                }
            }
        }

        Assert.True(compared > 0, "No captured finding could be compared to a contract row.");
        Assert.True(drifted.Count == 0,
            "A contract row no longer describes what its command prints:\n  " + string.Join("\n  ", drifted.Distinct(StringComparer.Ordinal)));
    }
    private static bool MatchesContractForm(string form, string message)
    {
        // A contract row documents the line a reader sees. Three differences between that line and
        // the JSON `message` member are expected and are not drift:
        //  - a listing row prints its subject in a column, so the row reads `<path>  <tail>` while
        //    the message carries only the tail and the path travels in `subject`;
        //  - a capture rewrites a Windows separator, so a row's `/` can render as `\`;
        //  - a row abbreviated with `...` is deliberately quoting only part of the sentence.
        if (form.Contains("...", StringComparison.Ordinal))
        {
            return true;
        }

        // The column gap is layout, not wording, so compare with runs of spaces collapsed. The
        // listing shape is read from the raw row first, because that gap is what identifies it.
        var listing = Regex.Match(form, @"^<[^>]+>(?::l:c)?[ ]{2,}(?<tail>.+)$", RegexOptions.Singleline);
        var candidates = listing.Success ? new[] { form, listing.Groups["tail"].Value } : [form];
        return candidates.Any(candidate =>
            Regex.IsMatch(Collapse(message), ContractPattern(Collapse(candidate)), RegexOptions.Singleline));
    }

    private static string Collapse(string value) => Regex.Replace(value, @"[ ]{2,}", " ");

    private static string ContractPattern(string form)
    {
        var builder = new StringBuilder("^");
        foreach (var part in Regex.Split(form, @"(<[^>]+>|:l:c|:line:column)"))
        {
            if (part.Length == 0) continue;
            builder.Append(Regex.IsMatch(part, @"^(<[^>]+>|:l:c|:line:column)$")
                ? ".+?"
                : Regex.Escape(part).Replace("/", @"[/\\]", StringComparison.Ordinal));
        }

        return builder.Append('$').ToString();
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> ReadContractMessageTemplates()
    {
        // These independently reviewed forms were relocated verbatim from the interface contracts.
        // Factory output must never be used to regenerate this expectation fixture.
        var path = Path.Combine(
            RepositoryRoot(), "src", "cli", "tests", "integration", "OpenForge.Cli.IntegrationTests",
            "Presentation", "Invariants", "Fixtures", "ContractMessageTemplates.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var templates = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var row in document.RootElement.GetProperty("rows").EnumerateArray())
        {
            var code = row.GetProperty("code").GetString()!;
            if (!templates.TryGetValue(code, out var forms))
            {
                templates[code] = forms = [];
            }
            forms.AddRange(row.GetProperty("forms").EnumerateArray().Select(form => form.GetString()!));
        }
        return templates.ToDictionary(
            static pair => pair.Key,
            static pair => (IReadOnlyList<string>)pair.Value,
            StringComparer.Ordinal);
    }

    private static IReadOnlySet<string> ReadCatalogueCodes()
    {
        var codes = new HashSet<string>(StringComparer.Ordinal);
        var catalogueRoot = Path.Combine(RepositoryRoot(), ".agents", "memory", "working", "cli-development", "tasks", "task30-g4");
        foreach (var file in Directory.EnumerateFiles(catalogueRoot, "*.md", SearchOption.TopDirectoryOnly))
        {
            var inCatalogue = false;
            foreach (var line in File.ReadLines(file))
            {
                if (line.StartsWith("## Findings catalogue", StringComparison.Ordinal))
                {
                    inCatalogue = true;
                    continue;
                }

                if (inCatalogue && line.StartsWith("## ", StringComparison.Ordinal))
                {
                    inCatalogue = false;
                }

                if (!inCatalogue) continue;
                foreach (Match match in CatalogueCodePattern.Matches(line)) codes.Add(match.Value);
            }
        }

        Assert.NotEmpty(codes);
        return codes;
    }

    private static JsonElement Required(JsonElement parent, string property)
    {
        Assert.True(parent.TryGetProperty(property, out var value), $"JSON member {property} is missing.");
        return value;
    }

    private static string RequiredString(JsonElement parent, string property)
    {
        var value = Required(parent, property);
        Assert.Equal(JsonValueKind.String, value.ValueKind);
        return value.GetString()!;
    }

    private static string ReadUtf8(string path)
    {
        var bytes = File.ReadAllBytes(path);
        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true).GetString(bytes);
    }

    private static string NormalizeLineEndings(string value)
        => value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');

    private static int CountLines(string value)
        => NormalizeLineEndings(value).TrimEnd('\n').Split('\n', StringSplitOptions.None).Length;

    private static string NormalizeIdentity(string value)
        => string.Concat(value.Where(char.IsLetterOrDigit)).ToLowerInvariant();

    private static string IntegrationProjectRoot()
        => Path.Combine(RepositoryRoot(), "src", "cli", "tests", "integration", "OpenForge.Cli.IntegrationTests");

    private static string[] SnapshotDirectories()
        => Directory
            .EnumerateDirectories(IntegrationProjectRoot(), "__snapshots__", SearchOption.AllDirectories)
            .OrderBy(static path => path, StringComparer.Ordinal)
            .ToArray();

    /// <summary>
    /// True when the owner directory is named after a test class that still exists beside the
    /// enclosing <c>__snapshots__</c> and still declares a test.
    /// </summary>
    private static bool HasLiveOwner(string ownerDirectory)
    {
        var snapshotDirectory = Directory.GetParent(ownerDirectory);
        if (snapshotDirectory is null) return false;
        var parent = snapshotDirectory.Parent;
        if (parent is null) return false;

        var source = Path.Combine(parent.FullName, $"{new DirectoryInfo(ownerDirectory).Name}.cs");
        return File.Exists(source) && ContainsTestAttribute(source);
    }

    private static bool ContainsTestAttribute(string source)
        => File.ReadLines(source).Any(line =>
        {
            var trimmed = line.TrimStart();
            return trimmed.StartsWith("[Fact", StringComparison.Ordinal)
                || trimmed.StartsWith("[Theory", StringComparison.Ordinal);
        });

    private static string RepositoryRoot()
    {
        foreach (var start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            for (var directory = new DirectoryInfo(start); directory is not null; directory = directory.Parent)
            {
                if (File.Exists(Path.Combine(directory.FullName, "OpenForge.Cli.slnx"))) return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("The Open Forge repository root could not be located.");
    }

    private sealed record SnapshotFile(string Path, string SituationKey, string Detail, bool IsJson, bool IsDiagnostics)
    {
        internal string Key => $"{SituationKey}|{Detail}";
        internal bool IsHumanPrimary => !IsJson && !IsDiagnostics;
    }

    private sealed record SnapshotCorpus(
        IReadOnlyList<SnapshotFile> TextSnapshots,
        IReadOnlyList<SnapshotFile> HumanSnapshots,
        IReadOnlyList<SnapshotFile> JsonSnapshots)
    {
        internal static SnapshotCorpus Load()
        {
            // The captures live in the snapshot library's default location: one `__snapshots__`
            // directory beside each test class that owns them. The corpus is every such
            // directory under the integration project, so a new command's captures join it
            // without this loader being told about them.
            var root = IntegrationProjectRoot();
            var all = SnapshotDirectories()
                .SelectMany(directory => Directory.EnumerateFiles(directory, "*.txt", SearchOption.AllDirectories)
                    .Select(path => Parse(path, directory)))
                .ToArray();
            if (all.Length == 0)
            {
                throw new InvalidOperationException(
                    $"No command output snapshots were found under {root}.");
            }
            var human = all.Where(snapshot => snapshot.IsHumanPrimary).ToArray();
            var json = all.Where(snapshot => snapshot.IsJson && !snapshot.IsDiagnostics).ToArray();
            var humanDiagnostics = all.Where(snapshot => !snapshot.IsJson && snapshot.IsDiagnostics).ToArray();
            var shellDiagnostics = json.Where(snapshot => !IsJsonEnvelope(snapshot.Path)).ToArray();
            var text = human.Concat(humanDiagnostics).Concat(shellDiagnostics).ToArray();
            return new SnapshotCorpus(text, human, json);
        }

        private static SnapshotFile Parse(string path, string root)
        {
            var name = System.IO.Path.GetFileName(path);
            var match = SnapshotFilePattern.Match(name);
            if (!match.Success) throw new InvalidOperationException($"Unrecognized snapshot filename: {path}");
            var isJson = name.Contains(CommandOutputSnapshot.JsonContentNameSegment, StringComparison.Ordinal);
            var isDiagnostics = match.Groups["diagnostics"].Success;
            var relativeDirectory = System.IO.Path.GetRelativePath(root, System.IO.Path.GetDirectoryName(path)!);
            var situation = match.Groups["situation"].Value;
            return new SnapshotFile(path, $"{relativeDirectory}|{situation}", match.Groups["detail"].Value, isJson, isDiagnostics);
        }

        private static bool IsJsonEnvelope(string path)
        {
            try
            {
                using var document = JsonDocument.Parse(ReadUtf8(path));
                return document.RootElement.ValueKind == JsonValueKind.Object;
            }
            catch (JsonException)
            {
                return false;
            }
        }
    }
}

internal sealed record SyntheticData(string Value);

[JsonSerializable(typeof(SyntheticData))]
internal sealed partial class InvariantDataContext : JsonSerializerContext;
