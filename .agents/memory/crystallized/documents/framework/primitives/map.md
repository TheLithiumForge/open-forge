---
open-forge:
  description: Current Map role, coarse destination mapping, destination authority, scalable organization, scope, and relationship with Framework routing
  responsibility: Define how durable maps expose important destinations without duplicating or replacing their truth
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Core, Map, Navigation, Scope]
---

# Map

## Role

Map `routes` provide concise durable navigation to important local and external destinations.

They help determine where relevant material lives and when to use it without requiring every destination to become Framework content. A Map `route` points to a destination; it does not copy the destination's meaning.

## Destination Authority

The routed destination or the authoritative source identified there retains its own truth. Map `routes` describe contents and relevance only far enough to support reliable selection.

When a destination changes, its Map `route` is updated only if the map has become inaccurate. Detailed project knowledge, accepted state, and working history remain in their appropriate systems or Memory `routes`.

## Granularity And Scale

A useful map is intentionally coarse. Projects, repositories, modules, systems, document collections, and external services usually earn `routes` before individual members or functions.

Finer `routes` are valid when they materially reduce discovery cost enough to justify their selection and maintenance surface. The organization of `routes` remains workspace-defined, so each environment can grow a useful shape, whether it serves one person, a project, a monorepo, a multi-repository system, or a discipline.

## Scope And Composition

Recursive Map `routes` may organize destinations by any positive scope. The `route` tree narrows discovery without claiming that physically nested destinations have greater authority.

Other primitives may link to Map `routes` when they need local or external destinations. Map may point to #Memory, source code, external systems, or other authoritative destinations without absorbing their semantics.

## Examples And Boundaries

Useful Map `routes` include:

- the source tree for a product component
- repositories participating in one integration
- the location of design assets or research
- an external issue tracker or deployed system
- a document collection for one discipline

A statement of accepted architecture belongs in a current document. A temporary list of files involved in active work belongs in Working Memory. Instructions for how to change a destination belong in the appropriate Directive, Guidance, Skill, Pattern, or Workflow.

## Related Current Sources

- [Core primitive model](model.md)
- [Maps entrypoint](../../../../../maps/_maps.md)
- [Map maintenance contract](../../maintenance/payload/agents/map.md)
- [Routing model](../routing/model.md)

## Decisions And Rationale

- [Distinct Core primitive roles](../../../decisions/framework/core-primitives.md)
- [Map primitive naming](../../../decisions/framework/map-primitive.md)
