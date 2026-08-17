---
open-forge:
  description: Current maintenance contract for the installable Maps Core category entrypoint
  responsibility: Preserve coarse destination routing, destination authority, source alignment, and deterministic Map route validity
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Core, Map]
---

# Maps Category Maintenance Contract

## Source

[`src/open-forge/.agents/maps/_maps.md`](../../../../../../../src/open-forge/.agents/maps/_maps.md) is the canonical installed Maps entrypoint. The repository [Maps entrypoint](../../../../../../maps/_maps.md) dogfoods the same authored contract and may add local generated entries.

The [current Map document](../../../framework/primitives/map.md) defines coarse destination mapping, destination authority, granularity, scope, and relationships with other primitives. The [Core primitive model](../../../framework/primitives/model.md#roles) owns the comparative taxonomy.

## Contract

- Frontmatter uses #LoadNow, #Core, and #Map so the category and its selection rule enter baseline context
- Each routed Map file links to one or more local or external sources and explains their contents and relevance
- Local destinations use containing-file-relative Markdown links; external destinations use their normal URLs
- The map stays intentionally broad unless finer routing clearly justifies its selection and maintenance cost
- Route filenames, grouping, nesting, and destination types remain workspace-defined rather than imposed by a project taxonomy
- Map routes point to Memory and other sources without replacing their detail
- The installable source begins with no project-specific Map route files

## Verification

- Core installation tests verify that the category installs, indexes, and remains baseline-loaded
- Route tests verify arbitrary-depth entrypoints, containing-file-relative links, path containment, and generated-region integrity
