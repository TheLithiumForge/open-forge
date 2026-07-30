---
open-forge:
  description: Current Template role, instantiation, ownership transfer, specialization, update boundary, and relationship with continuing Core contracts
  responsibility: Define how copy-ready source content starts independently maintained artifacts without retaining authority over them
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Core, Template, Ownership, Instantiation]
---

# Templates

## Role

A Template is a reusable source artifact intended to be instantiated into independently maintained workspace content.

Templates reduce blank-page cost. They provide useful starting material without imposing a universal schema or remaining authoritative after use.

## Selection And Specialization

Templates expose the need or result they address so the most specific useful starting source can be selected before its contents are copied.

Generic Templates are fallbacks. A specialization earns its own source only when the content to be instantiated differs materially, not merely because a subject has a different name. This keeps useful starting points discoverable without turning Templates into a catalogue of every possible artifact.

## Instantiation And Ownership

Instantiation transfers ownership to the created artifact.

The created artifact receives its own accurate scope, state, authoritative relationships, and content. The instantiated artifact becomes the authoritative source for its content within that scope. Later Template changes do not update it, and the Template does not create continuing conformance.

This distinguishes Templates from Patterns. A Pattern continues to guide related results. A Template contributes starting content once. A Template may implement or link to a Pattern, while a Directive or Axiom may require continuing behavior, but those relationships retain their separate authoritative sources.

## Evolution And Updates

Templates can evolve independently from their existing instances. Improving a Template changes future starting content without claiming ownership of artifacts that were already created from it.

Users may edit, scope, replace, remove, or add Templates through ordinary file-native routing. Update and restoration tools may compare versions or offer selected changes, but independently maintained destinations are never silently treated as managed Template instances.

## Relationships And Boundaries

A Template may provide starting content for a document described by a Pattern, created during a Workflow, or constrained by a Directive. Those continuing relationships remain linked to their separate authoritative sources.

A worked example intended for study is not necessarily a Template. Guidance explains judgment, Patterns describe continuing shapes, and Templates exist specifically to be instantiated and then relinquish authority.

Concrete starter content earns shared distribution only when its expected cross-workspace value justifies the additional selection and maintenance surface. The Maintenance contract owns the current shipped-versus-dogfood boundary.

## Related Current Sources

- [Core primitive model](model.md)
- [Templates entrypoint](../../../../../templates/_templates.md)
- [Templates maintenance contract](../../maintenance/payload/agents/templates.md)
- [Patterns](patterns.md)

## Decisions And Rationale

- [Templates as a Core primitive](../../../decisions/template-primitive.md)
- [Distinct Core primitive roles](../../../decisions/core-primitives.md)
