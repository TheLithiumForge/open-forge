---
open-forge:
  description: Current maintenance contract for the installable Patterns Core category entrypoint
  responsibility: Preserve Pattern selection, inspectable-shape semantics, recursive scope, source alignment, and deterministic route validity
  tags: [Memory, Document, CurrentTruth, Evergreen, Maintenance, Governance, Payload, Core, Pattern]
---

# Patterns Category Maintenance Contract

## Source

[`src/open-forge/.agents/patterns/_patterns.md`](../../../../../../../src/open-forge/.agents/patterns/_patterns.md) is the canonical installed Patterns entrypoint. The repository [Patterns entrypoint](../../../../../../patterns/_patterns.md) keeps repository-specific wording and local Entries. Shared shape obligations stay aligned without requiring byte equality.

The [current Patterns document](../../../framework/primitives/patterns.md) defines continuing structural reference, deliberate variation, scope, and relationships with other primitives. The [Core primitive model](../../../framework/primitives/model.md#roles) owns the comparative taxonomy.

## Contract

- Frontmatter uses #Core and #Pattern without a loading tag. The category is optional, has no dependents and stays on demand.
- The entrypoint directs agents to check Patterns when creating, changing, or reviewing something with visible structure
- Every routed Pattern defines one recognizable reusable shape and enough scope to judge relevance
- A new Pattern requires an accepted shape that remains useful for future related work
- One-off work, temporary transitions, and unsettled candidates do not become Patterns only because they have structure
- A Pattern shows one reusable shape as the main content through a concrete example, tree, table, diagram, or annotated outline of the expected result. A reviewer given only the Pattern and real code or files can compare their structure
- Brief applicability notes and labels identify required, optional and repeatable parts, their nesting and relationships, and which names, values and orders are fixed or only illustrative
- The entrypoint encourages links to related required rules, advice, maps, procedures, and starting files, identifies which sources bind the shape, and encourages links back from sources that apply it, without naming their categories
- Required behavior, procedures, and substantial choice rationale remain in their defining sources. Example names and values are illustrative unless an applicable requirement fixes them
- Concrete examples remain valid against their APIs, formats, and tools. Intentionally incomplete examples identify themselves as schematic and name their omissions
- An applicable Pattern remains the default shape. Deliberate adaptations are allowed within granted authority. Explain material departures before dependent work. Reviews report justified adaptations as departures separately from conformance. A mandatory shape requires authority for its exception
- Binding shape requirements come from a Directive, an active Axiom, or an accepted requirement rather than Pattern classification alone
- Child routes may narrow or preserve positive scope through the ordinary routing contract
- The installable source begins with no opinionated Pattern files

## Verification

- Use the [payload integrity checks](../_payload.md#verification) for shipped bytes and frontmatter rendering.
- Check the shipped loader leaves Patterns on demand. Compare shared obligations with repository Patterns while retaining its declared local wording differences.
- Manually compare one Pattern with a real artifact. The shape, applicability, fixed parts and permitted variations must be understandable from those two sources. Resource parity does not prove that usability.
