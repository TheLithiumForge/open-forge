---
open-forge:
  description: Current maintenance contract for the installable Workspace Core category entrypoint
  responsibility: Preserve coarse destination routing, destination authority, source alignment, and deterministic Workspace route validity
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Core, Workspace]
---

# Workspace Category Maintenance Contract

## Source

[`src/open-forge/.agents/workspace/_workspace.md`](../../../../../../../src/open-forge/.agents/workspace/_workspace.md) is the canonical installed Workspace entrypoint. The repository [Workspace entrypoint](../../../../../../workspace/_workspace.md) dogfoods the same authored contract and may add local generated entries.

The [current Workspace document](../../../framework/primitives/workspace.md) defines coarse destination mapping, destination authority, granularity, scope, and relationships with other primitives. The [Core primitive model](../../../framework/primitives/model.md#roles) owns the comparative taxonomy.

## Contract

- Frontmatter uses #LoadNow, #Core, and #Workspace so the category and its selection rule enter baseline context
- Each routed Workspace file identifies one or more related destinations and explains their contents and relevance
- Local destinations use containing-file-relative Markdown links; external destinations use their normal URLs
- The map stays intentionally coarse unless finer routing earns its ongoing selection and maintenance cost
- Route filenames, grouping, nesting, and destination types remain workspace-defined rather than imposed by a project taxonomy
- Workspace routes point to #Memory and other authoritative destinations without replacing their detailed truth
- The installable source begins with no project-specific Workspace route files
## Verification

- Core installation tests verify that the category installs, indexes, and remains baseline-loaded
- Route tests verify arbitrary-depth entrypoints, containing-file-relative links, path containment, and generated-region integrity
