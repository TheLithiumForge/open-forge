---
open-forge:
  description: Accepted Memory state containers and next implementation priorities
  tags: [OpenForge, Memory, State, Architecture, Governance, Loader, Implementation]
---

# Memory State Containers

## Accepted Direction

Layer 2 Memory uses state containers:

```text
memory/
  working/
  emerging/
  crystallized/
  archived/
```

These names are accepted as the current direction because they read naturally as memory states:

- working memory
- emerging memory
- crystallized memory
- archived memory

`working/` and `emerging/` describe active processes. `crystallized/` and `archived/` describe material whose current state is already formed.

The lifecycle is:

```text
working -> emerging -> crystallized -> archived
```

## Container Meanings

### `working/`

`working/` is memory that is alive right now.

It contains current context, sessions, active notes, temporary plans, in-progress handoffs, and other short-lived material created while work is happening.

Default likely child:

```text
working/
  sessions/
```

A session can produce code, ideas, analysis, decisions, documents, nothing useful, or a handoff. Because of that, handoffs do not need to be a default top-level Memory state. They can be artifacts inside working memory or conventions added by a workflow.

### `emerging/`

`emerging/` is memory that is becoming useful but is not accepted truth.

It can contain observations, ideas, analysis, options, rough conclusions, candidate decisions, and material that needs user validation or synthesis before becoming durable.

Default likely children:

```text
emerging/
  observations/
  ideas/
  analysis/
```

These are not mandatory forever. They are sane starter routes. Users and Layer 3 extensions can restructure or extend them.

### `crystallized/`

`crystallized/` is accepted current durable memory.

It can contain project vision, architecture, product truth, design truth, domain knowledge, decisions, durable documents, and compact current understanding.

Default likely children:

```text
crystallized/
  decisions/
  documents/
```

Users and Layer 3 decide the deeper taxonomy. Valid shapes include project-first, document-type-first, team-first, package-first, domain-first, or a mixture when it remains understandable.

### `archived/`

`archived/` is memory that is preserved for context but is no longer current.

It must not become a dump. Archived memory should either mirror the source path or otherwise preserve enough path/context to answer where it came from and what replaced it.

Preferred default shape:

```text
archived/
  working/
  emerging/
  crystallized/
```

Example:

```text
memory/emerging/ideas/routing.md
```

archives to:

```text
memory/archived/emerging/ideas/routing.md
```

Before archiving, useful essence should be extracted and promoted to the right current route.

## Growth Contract

Memory must be self-growing, personal, and project-shaped over time.

The default structure is only a shared starting point. Different users and repositories should diverge naturally as the work creates different needs.

Subcategories are strongly encouraged when they improve routing, ownership, and clarity.

Examples:

```text
crystallized/projects/project-a/documents/
crystallized/projects/project-b/architecture/
emerging/frontend/react/analysis/
working/releases/2026-06/
```

New top-level Memory states should be rare. If material does not fit the current top-level states, the agent should usually propose a subcategory first. If the top-level model itself is wrong, the agent should discuss the new state with the user before creating it.

Agents must not shoehorn material into a bad location. When the right route is unclear, they should suggest a small extension and ask for user confirmation when it changes the top-level Memory model or creates an important long-lived taxonomy.

## Required Follow-Up Work

1. Write governance descriptors for `memory/`, `working/`, `emerging/`, `crystallized/`, and `archived/`.
2. Each descriptor must define what the state is, what it contains, when to use it, why it exists, and its axioms.
3. The `memory/` descriptor must define recursive customization, subcategory growth, and the rule that agents propose extensions instead of shoehorning material.
4. Update the loader contract so the `memory/` entrypoint is always loaded when Memory is installed.
5. Create the implementation payload files for the Memory package after the governors are approved.
