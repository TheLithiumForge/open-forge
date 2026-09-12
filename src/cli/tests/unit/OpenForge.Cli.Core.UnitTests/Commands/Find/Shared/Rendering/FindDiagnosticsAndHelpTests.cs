using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.UnitTests.Commands.Find.Shared.Presentation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Find.Shared.Rendering;

public sealed class FindDiagnosticsAndHelpTests
{
    [Fact(DisplayName = "Find verbose diagnostics are bounded to status, workspace, counts, coverage, and next presence"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void DiagnosticSummaryUsesOnlyBoundedFacts()
    {
        var result = FindPresentationTestData.HostileResult();
        var request = FindPresentationTestData.PresentationRequest(
            result,
            CliOutputFormat.Human,
            CliView.Expanded,
            CliVerbosity.Verbose);

        var diagnostic = FindDiagnosticRenderer.Render(request);

        Assert.NotNull(diagnostic);
        Assert.InRange(diagnostic!.Length, 1, 4096);
        var lines = diagnostic.Split(Environment.NewLine, StringSplitOptions.None);
        var boundedLines = lines
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();
        Assert.Equal(
            [
                "status",
                "workspace",
                "candidates",
                "inspected",
                "matches",
                "coverage",
                "matching",
                "projection",
                "predicates",
                "findings",
                "next",
            ],
            boundedLines
                .Select(line => line[..line.IndexOf('=')])
                .ToArray());
        Assert.All(boundedLines, line => Assert.Contains('=', line));
        AssertLine(lines, "status=incomplete");
        AssertLine(lines, "candidates=1");
        AssertLine(lines, "inspected=1");
        AssertLine(lines, "matches=1");
        AssertLine(lines, "coverage=incomplete");
        AssertLine(lines, "matching=incomplete");
        AssertLine(lines, "projection=not-requested");
        AssertLine(lines, "predicates=2");
        AssertLine(lines, "findings=1");
        AssertLine(lines, "next=present");

        var workspaceLine = Assert.Single(
            lines,
            line => line.StartsWith("workspace=", StringComparison.Ordinal));
        var workspaceValue = workspaceLine["workspace=".Length..];
        Assert.InRange(workspaceValue.Length, 1, 240);
        var quoteIndex = workspaceValue.IndexOf('"');
        Assert.True(quoteIndex > 0 && workspaceValue[quoteIndex - 1] == '\\');
        var workspaceIndex = workspaceValue.IndexOf("workspace-", StringComparison.Ordinal);
        Assert.True(
            workspaceIndex >= 2
                && workspaceValue[workspaceIndex - 1] == '\\'
                && workspaceValue[workspaceIndex - 2] == '\\');
        Assert.EndsWith("...", workspaceValue, StringComparison.Ordinal);

        foreach (var payload in new[]
        {
            "hostile source subject must stay private",
            "base frontmatter",
            "overwrite frontmatter",
            "base body",
            "overwrite body",
            "base section",
            "overwrite section",
        })
        {
            Assert.DoesNotContain(payload, diagnostic, StringComparison.Ordinal);
        }

        foreach (var authoredValue in result.Matches
            .SelectMany(match => match.Evidence
                .SelectMany(evidence => new[] { evidence.Query, evidence.Authored }))
            .Concat(result.Matches.Select(match => match.Description))
            .Concat(result.Matches.SelectMany(match => match.Projections
                .SelectMany(projection => projection.Headings
                    .Select(heading => heading.Text)
                    .Append(projection.Text))))
            .Where(value => value is not null)
            .Select(value => value!)
            .Distinct(StringComparer.Ordinal))
        {
            Assert.DoesNotContain(authoredValue, diagnostic, StringComparison.Ordinal);
        }

        Assert.DoesNotContain("frontmatter", diagnostic, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("body", diagnostic, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("section", diagnostic, StringComparison.OrdinalIgnoreCase);
    }

    [Theory(DisplayName = "Find bounded diagnostics never split an escaped workspace token at the value limit"),
        InlineData("backslash", '\\'),
        InlineData("quote", '"'),
        InlineData("tab", '\t'),
        InlineData("control", '\u0001'),
        Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void DiagnosticTruncationPreservesCompleteEscapeTokens(string caseName, char boundaryCharacter)
    {
        var result = ResultWithBoundaryWorkspace(boundaryCharacter);
        var workspace = result.Workspace
            ?? throw new InvalidOperationException("The diagnostic token fixture requires a workspace.");
        var payloadStart = workspace.LexicalRoot.LastIndexOf(
            DiagnosticPayload,
            StringComparison.Ordinal);
        var boundaryPosition = payloadStart - 1;

        Assert.True(payloadStart > 0, $"The {caseName} fixture must retain its payload marker.");
        Assert.Equal(boundaryCharacter, workspace.LexicalRoot[boundaryPosition]);
        Assert.Equal(
            DiagnosticContentLength - 1,
            WindowsPathEscapedLength(workspace.LexicalRoot[..boundaryPosition]));

        var diagnostic = FindDiagnosticRenderer.Render(
            FindPresentationTestData.PresentationRequest(
                result,
                CliOutputFormat.Human,
                CliView.Compact,
                CliVerbosity.Verbose));

        Assert.NotNull(diagnostic);
        var lines = diagnostic!.Split(Environment.NewLine, StringSplitOptions.None);
        Assert.Equal(11, lines.Length);
        Assert.All(lines, line => Assert.Contains('=', line));

        var workspaceLine = Assert.Single(
            lines,
            line => line.StartsWith("workspace=", StringComparison.Ordinal));
        var workspaceValue = workspaceLine["workspace=".Length..];
        Assert.InRange(workspaceValue.Length, 1, DiagnosticValueLimit);
        Assert.EndsWith("...", workspaceValue, StringComparison.Ordinal);
        AssertCompleteEscapedValue(workspaceValue[..^3], caseName);
        Assert.DoesNotContain(DiagnosticPayload, diagnostic, StringComparison.Ordinal);
        Assert.DoesNotContain("hostile source subject must stay private", diagnostic, StringComparison.Ordinal);
        Assert.DoesNotContain("base frontmatter", diagnostic, StringComparison.Ordinal);
        Assert.DoesNotContain("base body", diagnostic, StringComparison.Ordinal);
        Assert.DoesNotContain("base section", diagnostic, StringComparison.Ordinal);
        Assert.DoesNotContain('\r', workspaceValue);
        Assert.DoesNotContain('\n', workspaceValue);
    }

    [Theory(DisplayName = "Find diagnostics retain every status without changing the typed status or primary presentation"),
        InlineData("Complete"),
        InlineData("Attention"),
        InlineData("Incomplete"),
        InlineData("Invalid"),
        InlineData("Blocked"),
        InlineData("Failed"),
        InlineData("Interrupted"),
        Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void DiagnosticSummaryCoversEveryStatus(string statusValue)
    {
        var result = statusValue == "Invalid"
            ? FindPresentationTestData.ProjectionCoverageNotStartedResult()
            : FindPresentationTestData.ForStatus(Enum.Parse<CliSemanticStatus>(statusValue));
        var request = FindPresentationTestData.PresentationRequest(
            result,
            CliOutputFormat.Json,
            CliView.Compact,
            CliVerbosity.Verbose);

        var diagnostic = FindDiagnosticRenderer.Render(request);

        Assert.NotNull(diagnostic);
        Assert.InRange(diagnostic!.Length, 1, 4096);
        Assert.Contains(
            $"status={statusValue.ToLowerInvariant()}",
            diagnostic,
            StringComparison.Ordinal);
        Assert.Equal(Enum.Parse<CliSemanticStatus>(statusValue), result.Status);
        Assert.Equal(CliOutputFormat.Json, request.Presentation.Format);
    }

    [Fact(DisplayName = "Find help owns the exact section order, grammar, exits, examples, related commands, and notes"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void HelpSectionsExposeTheCompleteFindContract()
    {
        var help = FindHelpSections.Create();

        Assert.Equal(
            [
                "Syntax",
                "Source references",
                "Predicates and regions",
                "Content and views",
                "Inherited global options",
                "Results and streams",
                "Examples",
                "Related commands",
                "Notes",
            ],
            help.Sections.Select(section => section.Heading));
        var syntax = SectionBody(help, "Syntax");
        var sourceReferences = SectionBody(help, "Source references");
        var predicates = SectionBody(help, "Predicates and regions");
        var content = SectionBody(help, "Content and views");
        var globalOptions = SectionBody(help, "Inherited global options");
        var results = SectionBody(help, "Results and streams");
        var examples = SectionBody(help, "Examples");
        var related = SectionBody(help, "Related commands");
        var notes = SectionBody(help, "Notes");

        Assert.Contains("open-forge find", syntax, StringComparison.Ordinal);
        Assert.Contains("--include", sourceReferences, StringComparison.Ordinal);
        Assert.Contains("--exclude", sourceReferences, StringComparison.Ordinal);
        foreach (var option in new[] { "--tag", "--heading", "--require", "--within" })
        {
            Assert.Contains(option, predicates, StringComparison.Ordinal);
        }

        foreach (var region in new[] { "document", "frontmatter", "body", "section:" })
        {
            Assert.Contains(region, predicates, StringComparison.Ordinal);
        }

        Assert.Contains("--content", content, StringComparison.Ordinal);
        Assert.Contains("compact", content, StringComparison.Ordinal);
        Assert.Contains("expanded", content, StringComparison.Ordinal);

        foreach (var option in new[] { "--workspace", "--json", "--view", "--verbose", "--help", "--version" })
        {
            Assert.Contains(option, globalOptions, StringComparison.Ordinal);
        }

        foreach (var status in new[] { "complete", "attention", "incomplete", "invalid", "blocked", "failed", "interrupted" })
        {
            Assert.Contains(status, results, StringComparison.Ordinal);
        }

        foreach (var exit in new[] { "0", "1", "2", "3", "4", "5", "130" })
        {
            Assert.Contains(exit, results, StringComparison.Ordinal);
        }

        Assert.Contains("stdout", results, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("stderr", results, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("open-forge find --tag=Architecture", examples, StringComparison.Ordinal);
        Assert.Contains("open-forge route list", related, StringComparison.Ordinal);
        Assert.Contains("open-forge doctor", related, StringComparison.Ordinal);
        Assert.False(string.IsNullOrWhiteSpace(notes));
    }

    [Fact(DisplayName = "Find help documents all seven semantic exits and keeps output ownership with the binding"), Trait("Feature", "find-presentation"), Trait("Evidence", "Unit")]
    public void HelpDocumentsAllSevenExitPolicies()
    {
        var expected = new Dictionary<CliSemanticStatus, (int ExitCode, CliOutputTarget Target)>
        {
            [CliSemanticStatus.Complete] = (0, CliOutputTarget.StandardOutput),
            [CliSemanticStatus.Attention] = (2, CliOutputTarget.StandardOutput),
            [CliSemanticStatus.Incomplete] = (3, CliOutputTarget.StandardOutput),
            [CliSemanticStatus.Invalid] = (4, CliOutputTarget.StandardError),
            [CliSemanticStatus.Blocked] = (5, CliOutputTarget.StandardError),
            [CliSemanticStatus.Failed] = (1, CliOutputTarget.StandardError),
            [CliSemanticStatus.Interrupted] = (130, CliOutputTarget.StandardError),
        };

        foreach (var pair in expected)
        {
            var definition = CliStatusDefinitions.Read(pair.Key);
            Assert.Equal(pair.Key, definition.Status);
            Assert.Equal(pair.Value.ExitCode, definition.Disposition.ExitCode);
            Assert.Equal(pair.Value.Target, definition.Disposition.HumanOutputTarget);
        }

        var help = FindHelpSections.Create();
        var results = Assert.Single(help.Sections, section => section.Heading == "Results and streams").Body;
        AssertExitLine(results, "complete", "0", "stdout");
        AssertExitLine(results, "attention", "2", "stdout");
        AssertExitLine(results, "incomplete", "3", "stdout");
        AssertExitLine(results, "invalid", "4", "stderr");
        AssertExitLine(results, "blocked", "5", "stderr");
        AssertExitLine(results, "failed", "1", "stderr");
        AssertExitLine(results, "interrupted", "130", "stderr");
    }

    private static void AssertExitLine(string body, string status, string exit, string stream)
        => Assert.Contains(
            body.Split('\n', StringSplitOptions.RemoveEmptyEntries),
            line => line.Contains(status, StringComparison.OrdinalIgnoreCase)
                && line.Contains(exit, StringComparison.Ordinal)
                && line.Contains(stream, StringComparison.OrdinalIgnoreCase));

    private static string SectionBody(CliHelpContent help, string heading)
        => Assert.Single(help.Sections, section => section.Heading == heading).Body;

    private static void AssertLine(IEnumerable<string> lines, string expected)
        => Assert.Contains(
            lines,
            line => string.Equals(line, expected, StringComparison.Ordinal));

    private static FindResult ResultWithBoundaryWorkspace(char boundaryCharacter)
    {
        var template = FindPresentationTestData.HostileResult();
        return new FindResult(
            template.Status,
            BoundaryWorkspace(boundaryCharacter),
            template.Universe,
            template.Query,
            template.Presentation,
            template.Coverage,
            template.Findings,
            template.Matches,
            template.Next);
    }

    private static CliWorkspace BoundaryWorkspace(char boundaryCharacter)
    {
        for (var fillerLength = 0; fillerLength < DiagnosticValueLimit; fillerLength++)
        {
            var candidate = string.Concat(
                BoundarySeed,
                new string('x', fillerLength),
                boundaryCharacter,
                DiagnosticPayload);
            var workspace = new CliWorkspace(
                candidate,
                candidate,
                CliWorkspaceSelectionMethod.ExplicitWorkspace);
            var payloadStart = workspace.LexicalRoot.LastIndexOf(
                DiagnosticPayload,
                StringComparison.Ordinal);
            var boundaryPosition = payloadStart - 1;
            if (payloadStart > 0
                && workspace.LexicalRoot[boundaryPosition] == boundaryCharacter
                && WindowsPathEscapedLength(workspace.LexicalRoot[..boundaryPosition])
                    == DiagnosticContentLength - 1)
            {
                return workspace;
            }
        }

        throw new InvalidOperationException(
            $"Could not place the {boundaryCharacter} diagnostic token at the value boundary.");
    }

    private static int WindowsPathEscapedLength(string value)
    {
        Assert.DoesNotContain('"', value);
        Assert.DoesNotContain('\t', value);
        Assert.DoesNotContain('\r', value);
        Assert.DoesNotContain('\n', value);
        return value.Length + value.Count(character => character == '\\');
    }

    private static void AssertCompleteEscapedValue(string value, string caseName)
    {
        const string lowerHexDigits = "0123456789abcdef";
        for (var index = 0; index < value.Length;)
        {
            var character = value[index];
            if (character != '\\')
            {
                Assert.True(
                    character != '"'
                        && !char.IsControl(character)
                        && !char.IsSurrogate(character),
                    $"The {caseName} diagnostic contains an unescaped scalar.");
                index++;
                continue;
            }

            Assert.True(
                index + 1 < value.Length,
                $"The {caseName} diagnostic ends with an incomplete escape token.");
            if (index + 1 >= value.Length)
            {
                return;
            }

            switch (value[index + 1])
            {
                case '\\':
                case '"':
                    index += 2;
                    break;
                case 'u':
                    Assert.True(
                        index + 6 <= value.Length,
                        $"The {caseName} diagnostic contains an incomplete Unicode escape token.");
                    if (index + 6 > value.Length)
                    {
                        return;
                    }

                    Assert.All(
                        value.Substring(index + 2, 4),
                        digit => Assert.True(
                            lowerHexDigits.IndexOf(digit) >= 0,
                            $"The {caseName} diagnostic contains a non-lowercase hexadecimal digit."));
                    index += 6;
                    break;
                default:
                    Assert.Fail(
                        $"The {caseName} diagnostic contains an unsupported escape token \\{value[index + 1]}.");
                    return;
            }
        }
    }

    private const int DiagnosticValueLimit = 240;
    private const int DiagnosticContentLength = DiagnosticValueLimit - 3;
    private const string BoundarySeed = @"C:\find-diagnostic-token-boundary";
    private const string DiagnosticPayload = "-diagnostic-payload-must-not-leak";
}
