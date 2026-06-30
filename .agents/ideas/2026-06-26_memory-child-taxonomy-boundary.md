---
open-forge:
  description: Boundary between Memory state containers and future child taxonomies
  tags: [OpenForge, Memory, Taxonomy, Core, Extension, Documentation]
---

# Memory Child Taxonomy Boundary

## Decision

Memory state entrypoints must not seed concrete child taxonomies by default.

The root Memory states are:

```text
working/
emerging/
crystallized/
archived/
```

Those files define only:

- what the state represents
- when to use its `Entries`
- what authority the state has
- how material moves, promotes, archives, or restores
- how child categories may be added safely

They must not prescribe future child folders such as sessions, ideas, observations, analysis, decisions, documents, handoffs, tasks, backlog, or similar specific routes.

## Why

Concrete child folders are not neutral. Even as examples, they can act like default instructions and push users toward a taxonomy they did not choose.

Open Forge should not prompt-inject a workspace shape through its base Memory files.

The base Memory layer should preserve flexibility, self-personalization, and recursive growth.

## Where Child Taxonomies Belong

Concrete Memory children may still exist later, but they belong in the layer that actually owns them:

- #Core product #Memory children, when a child route is universally useful and safe enough to install by default.
- #Extension payloads, when a child route is workflow-specific, persona-specific, tool-specific, or opinionated.
- User-owned local categories, when the workspace grows its own shape.
- User-facing documentation, when examples help explain possible organization without becoming installed behavior.

If a child category is installed later, its own entrypoint defines its meaning, scope, loading behavior, and generated `Entries`.

## Implementation Rule

Do not mention concrete future Memory child folders in the installed state files unless that child folder is actually installed and governed.

User docs and extension previews may show examples, but installed base files should stay minimal and state-based.

## Scoped Containers

The base Memory payload may install universal child routes, but broad buckets still need restraint.

`decisions/` should not be a default global folder for now. Accepted rationale can live inside any route that owns it, such as a document, project, product, architecture, workflow, or other scoped route. A scoped decision route is useful only when separate routing improves clarity.

`archived/` remains the generic historical Memory state. It may contain global archived material when that is genuinely the clearest scope, but scoped `archived/` routes may also live under any owning route when that preserves source origin and relevance better. State mirrors or targeted archive subroutes are created on demand, not installed by default.

User-facing docs and future extensions should explain scoped decisions and targeted archive routes as optional organization patterns that can appear anywhere useful, not base framework requirements.

## Observation Wording Priority

`ideas/` and `observations/` need a sharper distinction before their implementation files are approved.

Ideas are likely to be saved because the user asks an agent to capture an idea, possibility, future direction, or option.

Observations are less likely to be explicitly requested by the user. They are mainly agent-written notes extracted after work, especially across chat sessions, when the agent notices facts, recurring behavior, constraints, risks, or signals that may matter later.

The observation route must make that agent-initiated use clear without making observations authoritative. It should tell agents when to write observations, what evidence/source/scope/uncertainty to preserve, and when observations should be promoted, archived, or left alone.

This needs a wording pass in both:

- `docs/framework/payload/agents/memory/emerging/observations/_observations.md`
- `src/open-forge/.agents/memory/emerging/observations/_observations.md`

## Memory Promotion Direction Priority

Promotion is not only `observations/` behavior. It applies across #Memory.

Memory material can move in two broad directions:

- into another #Memory state or child route when its memory state changes
- out to #Core when it becomes operational behavior, reusable form, guidance, capability, workflow, workspace routing, or another #Core primitive

Observations are the clearest case because agent-written notes may reveal recurring structures, risks, constraints, or behavior. A repeated or validated observation may become crystallized memory, archived history, or a #Core file such as a pattern, directive, guidance file, skill, workflow, or workspace route.

The implementation wording should make this general without listing every possible destination in every child file. The root Memory and state entrypoints should define the broad rule. Child routes should state their local trigger, such as observations being promoted when they become repeated, validated, or important.

Each memory type file still needs enough explicit wording to avoid a logical leap. No memory route should rely only on the root Memory file to imply that its material can become another #Memory state or matching #Core material.
