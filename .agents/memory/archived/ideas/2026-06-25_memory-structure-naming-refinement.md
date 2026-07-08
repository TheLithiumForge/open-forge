---
open-forge:
  description: Refined Memory structure names, overlap analysis, and crystallized-memory consolidation
  tags: [Memory, Structure, Naming, CrystallizedMemory, Architecture]
---

# Memory Structure Naming Refinement

## Context

The grouped Memory structure is probably better than a flat list, but the earlier names were too lifeless:

```text
capture/
reasoning/
```

The stronger concepts from that pass were:

- `active/`
- `crystallized/`

That pass was later superseded by the accepted state-container set:

```text
working/
emerging/
crystallized/
archived/
```

The remaining structure should use names that are both readable and aligned with the self-growing memory model.

## Overlap Check

### `briefs/` And `documents/`

`briefs/` overlaps with `documents/`.

A brief is a compact document or compact crystallized memory. If `briefs/` is a peer of `documents/`, the system risks creating two places for durable truth:

```text
memory/briefs/project.md
memory/documents/project.md
```

That is exactly the kind of parallel truth Memory should avoid.

Better model:

- `crystallized/` is the durable current-truth area.
- direct files in `crystallized/` are compact briefs or crystals.
- `crystallized/documents/` holds long-form durable records.
- `crystallized/decisions/` holds accepted rationale.

This keeps `brief` as a file role, not necessarily a folder.

Example:

```text
memory/
  crystallized/
    _crystallized.md
    project.md
    product.md
    architecture.md
    design.md
    decisions/
      _decisions.md
    documents/
      _documents.md
```

In this model:

- `crystallized/project.md` is the compact current project brief.
- `crystallized/documents/prd.md` is the longer product requirements document.
- `crystallized/decisions/adr-001.md` is accepted rationale.

### `analysis/` And `documents/`

`analysis/` can overlap with `documents/` when an analysis becomes a durable report.

The distinction should be lifecycle:

- `analysis/` is structured reasoning that may still be revised, challenged, promoted, or archived.
- `crystallized/documents/` is durable long-form memory after it becomes accepted useful truth.

If an analysis becomes durable, promote the relevant truth into `crystallized/documents/`, `crystallized/decisions/`, or a compact crystallized file.

### `decisions/` And `documents/`

`decisions/` and `documents/` can overlap, but they answer different questions:

- `decisions/` answers "what did we choose and why?"
- `documents/` answers "what is the durable long-form record?"

Keep them separate under `crystallized/` because agents often need decisions without loading long docs.

### `observations/` And `ideas/`

`observations/` and `ideas/` should stay separate.

- `observations/` are noticed facts or findings.
- `ideas/` are candidate possibilities.

Both are seeds for future crystallization, but they have different truth posture.

### `active/`, `sessions/`, And `handoffs/`

`active/` should not grow into the entire work log.

Better distinction:

- `active/` contains current state and pointers.
- sessions preserve the chronological trail.
- handoffs are transfer snapshots.

`active/current.md` can link to the current session and current handoff. Sessions and handoffs can live under a history-like group so active memory stays small.

## Better Group Names

### `history/`

Better than `raw/` for normal users.

Purpose: chronological and transfer history.

Can contain:

```text
history/
  sessions/
  handoffs/
```

Alternative name: `traces/`.

`traces/` is more poetic and closer to memory language, but `history/` is clearer.

Superseded recommendation: `history/`.

### `seeds/`

Better than `capture/`.

Purpose: early growth material that may later crystallize.

Can contain:

```text
seeds/
  observations/
  ideas/
```

This fits the crystal model: a useful observation or idea can seed future memory, decisions, documents, patterns, or guidance.

### `analysis/`

Better than a generic `reasoning/` group for now.

Purpose: structured reasoning, comparison, investigation, and synthesis.

Keep `analysis/` as a direct Memory child unless more reasoning subtypes appear later.

Alternative names:

- `synthesis/`: more alive than reasoning, but less immediately clear
- `study/`: too narrow
- `investigation/`: too narrow
- `thinking/`: too casual

Superseded recommendation: direct `analysis/`.

### `crystallized/`

Strong name. Keep it.

Purpose: current durable memory after validation and consolidation.

Can contain compact direct files plus child folders for decisions and documents.

## Superseded Recommended Structure

Earlier grouped structure:

```text
.agents/
  memory/
    _memory.md
    active/
      _active.md
      current.md
    history/
      _history.md
      sessions/
        _sessions.md
      handoffs/
        _handoffs.md
    seeds/
      _seeds.md
      observations/
        _observations.md
      ideas/
        _ideas.md
    analysis/
      _analysis.md
    crystallized/
      _crystallized.md
      project.md
      product.md
      architecture.md
      design.md
      decisions/
        _decisions.md
      documents/
        _documents.md
```

This removed `briefs/` as a root memory folder. Compact current understanding lived directly inside `crystallized/`.

Current accepted direction:

```text
.agents/
  memory/
    _memory.md
    working/
      _working.md
      sessions/
        _sessions.md
    emerging/
      _emerging.md
      observations/
        _observations.md
      ideas/
        _ideas.md
      analysis/
        _analysis.md
    crystallized/
      _crystallized.md
      decisions/
        _decisions.md
      documents/
        _documents.md
    archived/
      _archived.md
      working/
      emerging/
      crystallized/
```

The exact default children remain open for governance review, but the top-level state names are `working/`, `emerging/`, `crystallized/`, and `archived/`.

## Why This Is Stronger

The accepted state-container structure maps to lifecycle:

```text
working
  -> emerging
  -> crystallized
  -> archived
```

It also maps to action:

- keep current context small in `working/`
- let candidate material take shape in `emerging/`
- consolidate useful truth in `crystallized/`
- preserve retired history in `archived/`

It avoids parallel truth between `briefs/` and `documents/`.

It keeps `tasks/` and `backlog/` out of default Memory until Layer 3 planning modules define ownership and external tracker behavior.

## Open Questions

- Which child folders should be installed by default under `working/`, `emerging/`, `crystallized/`, and `archived/`?
- Should `analysis/` be a default child under `emerging/`, or introduced by a workflow later?
- Should direct files in `crystallized/` be called briefs, crystals, summaries, or simply crystallized files?
- Should `crystallized/documents/` be named `documents/`, `docs/`, or `records/`?
- Should `working/current.md` be installed as a managed starter file or created by the first workflow that needs it?
