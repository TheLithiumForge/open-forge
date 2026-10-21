---
open-forge:
  description: Current maintenance contract for the installed workspace file-format reference
  responsibility: Preserve authoring formats, explicit pre-write loading, overwrite behavior and help concepts reuse
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Framework, Authoring]
---

# Authoring Reference Maintenance Contract

## Source

[`src/open-forge/.agents/authoring.md`](../../../../../../../src/open-forge/.agents/authoring.md) is the installed file-format reference for workspace context. The repository [authoring reference](../../../../../../authoring.md) carries the same shared text.

The [loader contract](loader.md) defines when it must be read. The [route include council](../../../../../emerging/analysis/route-include-council/record.md#help-concepts-the-removable-guide-and-the-authoring-reference) records the separation from the optional guide.

## Contract

- Every preset installs this reference beside the loader. It is unrouted, has no frontmatter and is not a shipped authoring Directive or an automatic startup read.
- The loader links it before writing workspace context, catalogues or metadata. `open-forge context .agents/authoring.md` returns it with an adjacent `authoring.overwrite.md`. The overwrite customizes this reference within its role.
- It describes formats agents and the CLI read. It keeps Entries syntax, catalogue names and compatibility names, Axioms placement, the inherited marker, non-empty rule Instructions and valid empty lists.
- Entries examples distinguish descriptions, relative destinations, optional loading tags and trailing conditions. Foreign files take their metadata from the loader row rather than requiring frontmatter.
- Metadata covers required description and tags, optional responsibility and applyTo, the workspace's root or scoped form and the scoped missing-setting default. Responsibility states an edit boundary without authority. Equal duplicate applyTo sets count once and differing sets are invalid. Native SKILL.md metadata remains native.
- Placement guidance keeps detail in the narrowest defining source, related material linked and scopes independently selectable. The scope example demonstrates that a required descendant does not open an unselected branch.
- The terms explain routes, root routes, slugs and the Core, Memory and Extension labels without granting authority, loading or management.
- It explains adjacent overwrites and update-plan review. Full command reference and advice about how to perform the work stay elsewhere.
- `open-forge help concepts` appends the complete shipped reference and says the workspace reference and its overwrite apply inside a workspace. It does not require the README or a workspace to show the built-in text.

## Verification

- Use the [payload integrity checks](../_payload.md#verification) for the installed reference and metadata rendering.
- [MarkdownFrontmatterParserTests](../../../../../../../src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Documents/Markdown/MarkdownFrontmatterParserTests.cs) and [SourceLoaderParserTests](../../../../../../../src/cli/tests/unit/OpenForge.Cli.Core.UnitTests/Framework/Sources/Routing/SourceLoaderParserTests.cs) cover format mechanisms, not the reference's completeness.
- Compare source and repository text directly. Check each documented shape against its reader and review the examples as examples rather than live workspace links.
- Verify selected Context returns the base followed by its overwrite and startup does not add it. Check Help Concepts ends with the complete shipped reference and still works outside a workspace and with README removed. Exercise these command paths in isolated integration fixtures.
