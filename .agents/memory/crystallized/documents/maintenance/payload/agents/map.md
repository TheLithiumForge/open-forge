---
open-forge:
  description: Current maintenance contract for the installable Maps Core category entrypoint
  responsibility: Preserve coarse destination routing, destination authority, source alignment, and deterministic Map route validity
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Core, Map]
---

# Maps Category Maintenance Contract

## Source

[`src/open-forge/.agents/maps/_maps.md`](../../../../../../../src/open-forge/.agents/maps/_maps.md) is the canonical installed Maps entrypoint. The repository [Maps entrypoint](../../../../../../maps/_maps.md) keeps the same category rules with repository-specific Entries and a local #LoadNow tag. The shipped category stays on demand.

The [current Map document](../../../framework/primitives/map.md) defines coarse destination mapping, destination authority, granularity, scope, and relationships with other primitives. The [Core primitive model](../../../framework/primitives/model.md#roles) owns the comparative taxonomy.

## Contract

- The title is followed by `## Where is a useful local or external source, and when should it be used?` and a short paragraph describing the folder.
- Frontmatter uses #Core and #Map without a loading tag. Check the category when work needs an important local or external source.
- Each routed Map file links to one or more local or external sources and explains their contents and relevance
- Local destinations use containing-file-relative Markdown links. External destinations use their normal URLs.
- The map stays intentionally broad unless finer routing clearly justifies its selection and maintenance cost
- Route filenames, grouping, nesting, and destination types remain workspace-defined rather than imposed by a project taxonomy
- Maps retain each destination's detail and authority without replacing its current information.
- The installable source begins with no project-specific Map route files

## Verification

- Use the [payload integrity checks](../_payload.md#verification) for shipped bytes and frontmatter rendering.
- Review destination descriptions, relative links and retained destination authority. Compare shared rules with the repository catalogue while allowing its local loading tag and Entries.
- Check the shipped loader leaves Maps on demand. File presence alone is not evidence of automatic loading.
