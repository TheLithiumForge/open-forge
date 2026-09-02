using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Update;

internal sealed partial class RouteUpdateIntegrationWorkspace
{
    private const int VerificationBodyLength = 16 * 1024 * 1024;

    internal void SeedTargetForm(string path)
    {
        File.Delete(Absolute(TargetPath));
        WriteText(path, TargetDocument());
    }

    internal void SeedTargetText(string text)
        => _temporary.ReplaceText(TargetPath, text);

    internal void SeedEmptyEntrypointTarget(string path)
    {
        File.Delete(Absolute(TargetPath));
        WriteText(
            path,
            "---\nopen-forge:\n  description: Before overview\n"
            + "  responsibility: Owns the overview\n"
            + "  tags: [Before, Memory]\n---\n");
        var separator = path.LastIndexOf('/');
        var directory = path[..separator];
        WriteText(
            $"{directory}/child.md",
            OpenForgeDocumentSeed.Metadata("Child", ["Child"], body: "# Child\n"));
    }

    internal void SeedEntrypointTemplate(bool includeGeneratedRegion)
    {
        var body = "# Exact Template\n\nPreserve this authored introduction.\n";
        if (includeGeneratedRegion)
        {
            body += "\n" + OpenForgeDocumentSeed.GeneratedEntries(
                "- [Stale child](child.md) - #Stale");
        }

        SeedTemplate(body);
    }

    internal void SeedOverwrite()
        => WriteText(OverwritePath, OverwriteText);

    internal void RemoveAgentsRoot()
    {
        if (Directory.Exists(Absolute(".agents")))
        {
            Directory.Delete(Absolute(".agents"), recursive: true);
        }
    }

    internal bool TrySeedUnsafeAgentsRoot(TemporaryWorkspace outside)
    {
        RemoveAgentsRoot();
        return _temporary.TryCreateDirectorySymbolicLink(
            ".agents",
            outside.Path,
            out _);
    }

    internal bool TrySeedTargetPhysicalAlias()
        => _temporary.TryCreateFileSymbolicLink(
            ".agents/memory/project-alpha/target-alias.md",
            Absolute(TargetPath),
            out _);

    internal void SeedTemplate(
        string body = TemplateBody,
        string[]? tags = null)
    {
        WriteText(
            ".agents/templates/_templates.md",
            OpenForgeDocumentSeed.Metadata(
                description: "Templates",
                tags: ["Template"],
                body: "\n" + OpenForgeDocumentSeed.GeneratedEntries(
                    "- [Route Template](route.md) - #Template")));
        WriteText(
            TemplatePath,
            OpenForgeDocumentSeed.Metadata(
                description: "Route Template",
                tags: tags ?? ["Template"],
                body: body));
    }

    internal void SeedTemplateOverwrite()
        => WriteText(
            ".agents/templates/route.overwrite.md",
            "template overwrite\n");

    internal void SeedUnreadableTemplate()
        => _temporary.ReplaceBytes(TemplatePath, [0xC3, 0x28]);

    internal void SeedTemplateCollision()
        => WriteText(
            ".agents/templates/route/_route.md",
            OpenForgeDocumentSeed.Metadata(
                "Colliding Route Template",
                ["Template"],
                "# Collision\n"));

    internal void SeedEmptyBodyTarget()
        => _temporary.ReplaceText(
            TargetPath,
            "---\nopen-forge:\n  description: Before overview\n"
            + "  responsibility: Owns the overview\n"
            + "  tags: [Before, Memory]\n---\n");

    internal void SeedVerificationWindow()
        => _temporary.ReplaceText(
            TargetPath,
            TargetDocument() + new string('x', VerificationBodyLength) + "\n");

    internal void SeedAmbiguousTarget()
        => WriteText(
            ".agents/memory/project-alpha/overview/_overview.md",
            TargetDocument());

    internal void SeedDetachedTarget()
    {
        _temporary.ReplaceText(
            ".agents/loader.md",
            GeneratedLoaderDocumentBuilder.Build(string.Empty));
        WriteText(".agents/detached/_detached.md", TargetDocument());
    }

    internal void SeedOrphanTarget()
    {
        WriteText(
            ".agents/orphan/leaf.overwrite.md",
            "orphan overwrite bytes\n");
    }

    internal void SeedUnavailableRouteFacts()
        => _temporary.ReplaceBytes(".agents/loader.md", [0xC3, 0x28]);

    internal void SeedProtectedSkill()
        => WriteText(
            ".agents/memory/project-alpha/native/SKILL.md",
            OpenForgeDocumentSeed.SkillFrontmatter("native", "Protected native source"));

    internal void SeedSelfRegionTarget()
    {
        const string targetPath = ".agents/memory/project-alpha/overview/_overview.md";
        File.Delete(Absolute(TargetPath));
        WriteText(
            targetPath,
            TargetDocument()
            + "\n"
            + OpenForgeDocumentSeed.GeneratedEntries(
                "- [Child](child.md) - #Child"));
        WriteText(
            ".agents/memory/project-alpha/overview/child.md",
            OpenForgeDocumentSeed.Metadata("Child", ["Child"], body: "# Child\n"));
    }

    internal void MutateTargetAfterPlanning()
        => _temporary.ReplaceText(TargetPath, TargetDocument("Concurrent target"));

    internal void MutateOverwriteAfterPlanning()
        => _temporary.ReplaceText(OverwritePath, "concurrent overwrite bytes\n");

    internal void MutateTemplateAfterPlanning()
        => _temporary.ReplaceText(TemplatePath, TargetDocument("Concurrent template"));

    internal void MutateParentAfterPlanning()
        => _temporary.ReplaceText(
            ParentPath,
            ParentDocument("- changed - Concurrent - #Changed"));

    internal void ReplaceParentWithDirectory()
    {
        File.Delete(Absolute(ParentPath));
        Directory.CreateDirectory(Absolute(ParentPath));
    }

    internal void RemoveParentRaceDirectory()
        => Directory.Delete(Absolute(ParentPath));

    private void SeedOrdinaryRoute()
    {
        WriteText(
            ".agents/loader.md",
            GeneratedLoaderDocumentBuilder.Build(
                "- [Project Alpha](memory/project-alpha/_project-alpha.md) - #Project\n"
                + "- [Templates](templates/_templates.md) - #Template"));
        WriteText(
            ParentPath,
            ParentDocument("- [Before overview](overview.md) - #Before #Memory"));
        WriteText(TargetPath, TargetDocument());
    }

    private static string TargetDocument(string description = "Before overview")
        => $"---\nopen-forge:\n  description: {description}\n"
            + "  responsibility: Owns the overview\n"
            + "  tags: [Before, Memory]\n---\n\n"
            + "# Authored body\n\nPreserve this body.\n";

    private static string ParentDocument(string entry)
        => OpenForgeDocumentSeed.Metadata(
            "Project Alpha",
            ["Project"],
            body: "\n" + OpenForgeDocumentSeed.GeneratedEntries(entry));

    private void WriteText(string path, string contents)
        => _temporary.WriteText(path, contents);
}
