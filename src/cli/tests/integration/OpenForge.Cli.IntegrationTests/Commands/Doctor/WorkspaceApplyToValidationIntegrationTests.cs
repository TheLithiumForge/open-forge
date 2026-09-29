using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Doctor;

public sealed class DoctorWorkspaceApplyToValidationIntegrationTests
{
    private const string InvalidSkillPath = ".agents/skills/native-tool/SKILL.md";
    private const string InvalidSourcePath = ".agents/status/invalid-apply-to.md";
    private const string ValidGlobPath = ".agents/status/valid-glob-apply-to.md";
    private const string EquivalentDualPath = ".agents/status/equivalent-dual-apply-to.md";
    private const string NonmatchingValidPath = ".agents/status/nonmatching-apply-to.md";
    private const string UnbalancedBracesPath = ".agents/status/unbalanced-braces-apply-to.md";
    private const string UnterminatedClassPath = ".agents/status/unterminated-class-apply-to.md";

    [Fact(DisplayName = "Doctor reports malformed applyTo syntax and accepts valid glob forms"), Trait("Feature", "doctor-command"), Trait("Evidence", "Integration")]
    public async Task NormalInventoryReportsInvalidApplyToWithoutFilteringValidConditions()
    {
        using var workspace = CreateWorkspace();
        var before = workspace.SnapshotHashes();

        var run = await CliHostCapture.RunAsync(
            ["doctor", "--workspace", workspace.Path, "--format", "json", "--detail", "full"],
            workspace.Path);

        Assert.NotEqual(0, run.ExitCode);
        Assert.Equal(string.Empty, run.Error);
        using var document = JsonDocument.Parse(run.Output);
        var findings = document.RootElement.GetProperty("findings").EnumerateArray().ToArray();
        var malformedApplyToPaths = findings
            .Where(finding => finding.GetProperty("code").GetString() == "workspace.frontmatter-malformed")
            .Select(SubjectPath)
            .Where(path => path is InvalidSkillPath or InvalidSourcePath or UnbalancedBracesPath or UnterminatedClassPath)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            new[] { InvalidSkillPath, InvalidSourcePath, UnbalancedBracesPath, UnterminatedClassPath }
                .Order(StringComparer.Ordinal),
            malformedApplyToPaths);
        Assert.DoesNotContain(findings, finding =>
            finding.GetProperty("code").GetString() == "workspace.frontmatter-malformed"
            && SubjectPath(finding) is ValidGlobPath or EquivalentDualPath or NonmatchingValidPath);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static TemporaryWorkspace CreateWorkspace()
    {
        var workspace = TemporaryWorkspace.Create("doctor-apply-to-validation");
        workspace.WriteText("AGENTS.md", "# Workspace\n\nRead `.agents/loader.md`.\n");
        workspace.WriteText(
            ".agents/loader.md",
            GeneratedLoaderDocumentBuilder.Build("- [Status](status/_status.md) - #Status"));
        workspace.WriteText(
            ".agents/status/_status.md",
            OpenForgeDocumentSeed.Metadata(
                description: "Status",
                tags: ["Status"],
                body: "\n# Status\n\nDoctor evidence.\n"));
        workspace.WriteText(
            InvalidSkillPath,
            """
            ---
            name: "Native tool"
            applyTo: ["../outside.md"]
            ---
            # Native tool
            """);
        workspace.WriteText(
            InvalidSourcePath,
            """
            ---
            open-forge:
              description: Invalid ordinary source
              tags: [Guidance]
              applyTo: ["../outside.md"]
            ---
            # Invalid ordinary source
            """);
        workspace.WriteText(
            ValidGlobPath,
            """
            ---
            open-forge:
              description: Valid glob syntax
              tags: [Guidance]
              applyTo: ["{src,test}/**/*.[ct]s"]
            ---
            # Valid glob syntax
            """);
        workspace.WriteText(
            EquivalentDualPath,
            """
            ---
            applyTo: "{src,test}/**/*.[ct]s,docs/*.md"
            open-forge:
              description: Equivalent future condition
              tags: [Guidance]
              applyTo: ["{src,test}/**/*.[ct]s", "docs/*.md"]
            ---
            # Equivalent future condition
            """);
        workspace.WriteText(
            NonmatchingValidPath,
            """
            ---
            open-forge:
              description: Valid future condition
              tags: [Guidance]
              applyTo: ["future/**/*.cs"]
            ---
            # Valid future condition
            """);
        workspace.WriteText(
            UnbalancedBracesPath,
            """
            ---
            open-forge:
              description: Unbalanced braces
              tags: [Guidance]
              applyTo: "{src,test/**/*.cs"
            ---
            # Unbalanced braces
            """);
        workspace.WriteText(
            UnterminatedClassPath,
            """
            ---
            open-forge:
              description: Unterminated class
              tags: [Guidance]
              applyTo: ["[abc"]
            ---
            # Unterminated class
            """);
        return workspace;
    }

    private static string SubjectPath(JsonElement finding)
        => finding.GetProperty("subject").GetProperty("path").GetString()
            ?? throw new InvalidOperationException("Doctor workspace findings require a subject path.");
}
