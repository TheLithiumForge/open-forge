using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Documents.RepositoryDocumentation.Shared;

internal static class RepositoryMarkdownFixtures
{
    internal static TemporaryWorkspace CreateLinkCases()
    {
        var workspace = TemporaryWorkspace.Create("repository-markdown-links");
        workspace.WriteText("OpenForge.Cli.slnx", "solution\n");
        workspace.WriteText("inline-cases.md", """
[inline](targets/ok.md)
![inline image](images/ok.png)
[full][full-target]
[collapsed][]
[shortcut]
![linked image][image-target]
[missing inline](targets/missing-inline.md)
[missing inline duplicate](targets/missing-inline.md)
[missing full][missing-full]
[missing collapsed][]
[missing shortcut]
![missing image][missing-image]
Unused definition remains unreferenced.
[unresolved]

[full-target]: <targets/space name.md>
[collapsed]: <targets/space name.md>
[shortcut]: <targets/café.md>
[image-target]: images/ok.png
[missing-full]: targets/missing-full.md
[missing collapsed]: targets/missing-collapsed.md
[missing shortcut]: targets/missing-shortcut.md
[missing-image]: images/missing.png
[unused]: targets/missing-unused.md
""");
        workspace.WriteText("edge-cases.md", """
[encoded space](targets/space%20name.md)
[encoded unicode](targets/caf%C3%A9.md)
[escaped parentheses](targets/escaped \(name\).md)
[literal reference][literal-space]
[bad encoding](targets/bad%ZZ.md)
[query](targets/ok.md?view=one)
[missing](targets/missing-edge.md)
[casing](targets/CaseTarget.md)
[containment](../outside.md)

[literal-space]: <targets/space name.md>
""");
        workspace.WriteText("fragments.md", """
# Local Heading
# Café 工作

[local](#local-heading)
[unicode](#café-工作)
[cross](fragment-target.md#target-heading)
[formatted](fragment-target.md#formatted-heading)
[duplicate](fragment-target.md#café-工作-1)
[directory](directories)
""");
        workspace.WriteText("fragment-failures.md", """
[missing](fragment-target.md#missing-heading)
[case](fragment-target.md#Target-Heading)
[setext](setext-target.md#setext-heading)
[html](html-target.md#html-heading)
[non-markdown](assets/page.html#html-heading)
[directory](directories#anything)
""");
        workspace.WriteText("code-cases.md", """
Inline code: `[hidden](missing-inline-code.md)`

```markdown
[fenced](missing-fenced.md)
```

    [indented](missing-indented.md)
""");
        workspace.WriteText("frontmatter-cases.md", """
---
title: "[frontmatter](missing-frontmatter.md)"
---
Body text only.
""");
        workspace.WriteText("src/docusaurus/docs/site.md", """
[docs route](/docs/guide)
[guide route](/guides/guide)
![image route](/img/logo.png)
[delegated](https://example.com/docs/guide)
[protocol](//example.com/docs/guide)
[mail](mailto:docs@example.com)
[relative](../../../targets/site.md)
[relative missing](../../../targets/site-missing.md)
""");
        workspace.WriteText("docs/cli-experience-fixtures/excluded.md", """
[excluded](missing-excluded.md)
""");
        workspace.WriteText("docs/extension-candidates/candidate.md", """
[candidate](missing-candidate.md)
""");
        workspace.WriteText("order-a.md", """
[first](missing-first.md)
[second](missing-second.md)
""");
        workspace.WriteText("order-z.md", """
[third](missing-third.md)
""");
        workspace.WriteBytes("invalid-utf8.md", [0x23, 0x20, 0xFF, 0x0A]);
        workspace.WriteText("unavailable.md", """
---
key: value
# Body
""");

        workspace.WriteText("targets/ok.md", "# OK\n");
        workspace.WriteText("targets/space name.md", "# Space\n");
        workspace.WriteText("targets/café.md", "# Café\n");
        workspace.WriteText("targets/escaped (name).md", "# Escaped\n");
        workspace.WriteText("targets/casetarget.md", "# Case\n");
        workspace.WriteText("targets/site.md", "# Site\n");
        workspace.WriteText("fragment-target.md", """
# Target Heading
# Café 工作
# Café 工作
# Formatted **Heading**
""");
        workspace.WriteText("setext-target.md", """
Setext Heading
--------------
""");
        workspace.WriteText("html-target.md", "<div id=\"html-heading\">HTML target</div>\n");
        workspace.WriteText("assets/page.html", "<h1 id=\"html-heading\">HTML</h1>\n");
        workspace.WriteText("directories/placeholder.md", "# Placeholder\n");
        workspace.WriteBytes("images/ok.png", [0x89, 0x50, 0x4E, 0x47]);

        return workspace;
    }

    internal static TemporaryWorkspace CreateScopeCases()
    {
        var workspace = TemporaryWorkspace.Create("repository-markdown-scope");
        workspace.WriteText("OpenForge.Cli.slnx", "solution\n");
        workspace.WriteText("README.md", "# Root\n");
        workspace.WriteText("src/extensions/README.md", "# Extensions\n");
        workspace.WriteText("src/extensions/alpha/README.md", "# Alpha\n");
        workspace.WriteText("src/extensions/alpha/nested.md", "# Nested\n");
        workspace.WriteText("src/extensions/beta/README.md", "# Beta\n");

        workspace.WriteText(".agents/maps/root.md", "# Map\n");
        workspace.WriteText(".agents/maps/generated/map.md", "# Generated map\n");
        workspace.WriteText(".agents/maps/working/map.md", "# Working map\n");
        workspace.WriteText(
            ".agents/memory/crystallized/decisions/framework/workspace-state-files.md",
            "# Workspace state files\n");
        workspace.WriteText(".agents/memory/crystallized/documents/cli/_cli.md", "# CLI\n");
        workspace.WriteText(
            ".agents/memory/crystallized/documents/cli/architecture.md",
            "# Architecture\n");
        workspace.WriteText(
            ".agents/memory/crystallized/documents/cli/command-contract-set.md",
            "# Command contracts\n");
        workspace.WriteText(
            ".agents/memory/crystallized/documents/cli/distribution.md",
            "# Distribution\n");
        workspace.WriteText(
            ".agents/memory/crystallized/documents/cli/shared-operation-contract.md",
            "# Shared operations\n");
        workspace.WriteText(
            ".agents/memory/crystallized/documents/cli/contracts/contract.md",
            "# Contract\n");
        workspace.WriteText(
            ".agents/memory/crystallized/documents/cli/layers/layer.md",
            "# Layer\n");
        workspace.WriteText(
            ".agents/memory/crystallized/documents/cli/technical-designs/design.md",
            "# Design\n");
        workspace.WriteText(
            ".agents/memory/crystallized/documents/cli/mvp-architecture.md",
            "# Excluded MVP prose\n");
        workspace.WriteText(
            ".agents/memory/crystallized/documents/cli/experience/scenario.md",
            "# Excluded experience\n");

        workspace.WriteText("docs/cli.md", "# CLI guide\n");
        workspace.WriteText("docs/development.md", "# Development\n");
        workspace.WriteText("docs/demos/demo.md", "# Demo\n");
        workspace.WriteText("docs/extensions.md", "# Extensions guide\n");
        workspace.WriteText("docs/generated/in-scope.md", "# Generated documentation\n");
        workspace.WriteText("docs/guide.md", "# Guide\n");
        workspace.WriteText("docs/templates/in-scope.md", "# Template documentation\n");
        workspace.WriteText("docs/working/in-scope.md", "# Working documentation\n");
        workspace.WriteText(
            "docs/cli-experience-fixtures/fixture.md",
            "# Excluded fixture\n");
        workspace.WriteText(
            "docs/extension-candidates/candidate.md",
            "# Excluded candidate\n");

        workspace.WriteText("src/docusaurus/docs/demos/demo.md", "# Published demo\n");
        workspace.WriteText(
            "src/docusaurus/docs/generated/in-scope.md",
            "# Published generated page\n");
        workspace.WriteText("src/docusaurus/docs/guide.md", "# Published guide\n");
        workspace.WriteText(
            "src/docusaurus/docs/templates/in-scope.md",
            "# Published template page\n");
        workspace.WriteText(
            "src/docusaurus/docs/working/in-scope.md",
            "# Published working page\n");

        return workspace;
    }
}
