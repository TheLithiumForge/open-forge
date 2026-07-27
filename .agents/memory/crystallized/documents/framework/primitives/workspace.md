---
open-forge:
  description: Current Workspace role, coarse destination mapping, destination authority, scalable organization, scope, and relationship with Framework routing
  responsibility: Define how durable workspace orientation exposes important destinations without duplicating or replacing their truth
  tags: [Memory, Document, CurrentTruth, Evergreen, Framework, Core, Workspace, Navigation, Scope]
---

# Workspace

## Role

Workspace routes provide a concise durable map of important project and external locations.

They help determine where relevant material lives and when to use it without requiring every destination to become Framework content. A Workspace route is navigation to a destination, not a copy of the destination's meaning.

## Destination Authority

The routed destination or the authoritative source identified there retains its own truth. Workspace routes describe contents and relevance only far enough to support reliable selection.

When a destination changes, its Workspace route is updated only if the map has become inaccurate. Detailed project knowledge, accepted state, and working history remain in their appropriate systems or Memory routes.

## Granularity And Scale

A useful map is intentionally coarse. Projects, repositories, modules, systems, document collections, and external services usually earn routes before individual members or functions.

Finer routes are valid when they materially reduce discovery cost enough to justify their selection and maintenance surface. Route organization remains workspace-defined so a single project, monorepo, multi-repository system, or non-development workspace can grow its own useful shape.

## Scope And Composition

Recursive Workspace routes may organize destinations by any positive scope. The route tree narrows discovery without claiming that physically nested destinations have greater authority.

Other primitives may link to Workspace routes when they need project locations. Workspace may point to #Memory, source code, external systems, or other authoritative destinations without absorbing their semantics.

## Examples And Boundaries

Useful Workspace routes include:

- the source tree for a product component
- repositories participating in one integration
- the location of design assets or research
- an external issue tracker or deployed system
- a document collection for one discipline

A statement of accepted architecture belongs in a current document. A temporary list of files involved in active work belongs in Working Memory. Instructions for how to change a destination belong in the appropriate Directive, Guidance, Skill, Pattern, or Workflow.

## Related Current Sources

- [Core primitive model](model.md)
- [Workspace entrypoint](../../../../../workspace/_workspace.md)
- [Workspace maintenance contract](../../maintenance/payload/agents/workspace.md)
- [Routing model](../routing/model.md)

## Decisions And Rationale

- [Distinct Core primitive roles](../../../decisions/core-primitives.md)
