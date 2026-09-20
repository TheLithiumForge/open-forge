---
open-forge:
  description: Candidate Framework follow-up for memory authority, loading defaults, taxonomy, templates, and self-sufficient task context
  tags: [Memory, Working, Framework, CLI, Task, Potential, Contextual, Loading, Taxonomy]
---

# Candidate — Framework Context And Authoring

## Status

Candidate; not an active Task. Promote only after the [Local Planning](../../../local-planning.md)
settles the source-of-truth and archival questions.

## Why this exists

The retrospective found that the Framework's routing and memory model is a
source of recurring CLI and agent-workflow friction: entrypoints and leaf
documents carry overlapping loading signals, Memory contains prescriptive
language, Maps/Skills/Workflows lack a consistent placement aid, and task or
handoff records are not always self-sufficient. The experience audit also found
that the shipped loader still over-tags `#LoadNow` and has no visible context
budget or attribution discipline.

## Candidate decision bundle

- Clarify the entrypoint-versus-leaf loading axiom. Decide whether only
  actionable leaf Directives carry `#LoadNow`, where the scope gate lives, how
  context deltas are shown at route/task creation, and what startup budget and
  attribution are required.
- Apply the Memory authority boundary without contradicting the current
  canonical decision that `responsibility` is optional. Decide whether a
  redirect clause, mandatory responsibility on selected entrypoints, or a
  different mechanism is the narrowest source-owned fix.
- Give Maps, Skills, Workflows, Patterns, Templates, Tasks, and Handoffs a
  short placement aid. Decide whether Maps remains a coarse orientation route,
  and keep Skills as a runtime-discovered format family rather than adding a
  registry merely to make it look like a Framework category.
- Decide whether workstreams are first-class memory, which checkpoint and
  handoff shapes should become templates, and what context every Task/plan/
  handoff must carry so it can be resumed without reconstructing hidden scope.
- Define the minimum shipped authoring bundle and extension boundary. Include
  the maintenance CLI guidance in its accepted home rather than the loader,
  and align root entrypoints, templates, descriptions, and generated index
  behavior.
- Revisit `rules`, `thresholds`, and any authored-vs-generated state only in
  coordination with Task 30 G1; do not reopen the accepted two-file ownership
  model from this candidate.

## Promotion and acceptance

Promotion requires a current-source decision table, a migration/compatibility
plan for existing `.agents` content, and a measurable loader/context receipt.
Acceptance must name the narrowest source for each rule, remove former-role
metadata when a category changes, preserve provenance, and update incoming
links before archival. No Framework source mutation is authorized by this
candidate record alone.
