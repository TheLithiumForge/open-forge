---
open-forge:
  description: Goal statement for self-growing memory, decay, archive, validation, and crystallization
  tags: [OpenForge, Memory, Crystallization, Archive, Goal, README]
---

# Memory Crystallization Goal

## Raw User Goal

The goal for Memory is to create short-term memory for both the human and the AI.

That short-term memory fades over time. It rots in the sense that context becomes stale, partial, or no longer aligned with the current state of the project or life.

Old memory should not be deleted blindly. It should be archived, and the truth inside it should be extracted, validated against the current status quo, and consolidated into useful crystallized memory.

The extraction must be honest and useful. It must not lose important context, and it must consolidate with crystallized memory that already exists.

Memory can be imagined as a crystal. Sometimes a new crystal needs a seed. Sometimes the system builds upon an existing crystal. Sometimes things break and regrow because reality changed. The framework should not fear changing realities, but it should not forget history.

The larger goal is a self-growing system.

It starts from bases or seeds like documents. It keeps active memory while work is happening. It supports self-improving mechanisms such as observations: agent notes to itself when it notices something that may later evolve into documents, patterns, new memories, decisions, or other framework material.

## Product Wording

Open Forge Memory is a self-growing memory system for human-AI work.

It keeps short-term working context while the work is alive, then helps that context decay safely into history instead of pretending it remains current truth.

Useful truth is extracted from raw work, checked against the present reality, and crystallized into compact durable memory. Existing memory is updated rather than duplicated. When reality changes, memory is allowed to break, reshape, and regrow while preserving the historical trail.

The goal is not to remember everything forever. The goal is to keep the useful essence, preserve enough context to understand it, and make future work cheaper, safer, and more aligned.

## README-Style Candidate

Open Forge treats memory like something alive.

Work starts as active context: messy plans, observations, decisions-in-progress, and handoffs. Over time that context goes stale. Instead of letting old chat history quietly become fake truth, Open Forge gives agents a place to archive it, extract what still matters, validate it against the current project, and crystallize it into concise durable memory.

The result is a system that grows with the project. New documents can seed memory. Repeated observations can become decisions, compact crystallized files, patterns, or guidance. Old assumptions can break and regrow when reality changes. The history stays available, but the current truth stays compact.

## Architecture Implications

Memory needs at least these lifecycle states:

- active: current working state
- raw: sessions and captured work history
- observed: useful facts noticed during work
- candidate: ideas and possible future truth
- analyzed: structured reasoning and tradeoffs
- accepted: decisions and rationale
- crystallized: compact current understanding
- durable: long-form documents and records
- transfer: handoffs and resume notes
- archived: historical context only

Memory should support both seeding and growth:

- seed from documents, decisions, compact crystallized files, or existing project knowledge
- grow through sessions, observations, ideas, and analysis
- crystallize into compact current files, decisions, documents, and Core framework material
- archive stale raw material without deleting history
- revise crystallized memory when reality changes

## Possible Structure Reconsideration

The current candidate structure may not be the final best shape.

One possible flatter structure:

```text
memory/
  active/
  sessions/
  ideas/
  observations/
  analysis/
  decisions/
  crystallized/
    project.md
    product.md
    architecture.md
  documents/
  handoffs/
```

Earlier refined grouped structure:

```text
memory/
  active/
    current.md
  history/
    sessions/
    handoffs/
  seeds/
    observations/
    ideas/
  analysis/
  crystallized/
    project.md
    product.md
    architecture.md
    design.md
    decisions/
    documents/
```

That grouped structure was superseded by the state-container model.

Accepted state-container structure:

```text
memory/
  working/
    sessions/
  emerging/
    observations/
    ideas/
    analysis/
  crystallized/
    decisions/
    documents/
  archived/
    working/
    emerging/
    crystallized/
```

The exact default child folders remain open for governance review.

Flat structures are easier to inspect and easier for users to extend, but state containers give stronger routing semantics.

The framework is not restricted to either. Every folder can have an entrypoint with its own axioms and `Entries`, so even grouped memory can remain routable and explicit.

## Folder Design Questions

- Should `analysis/` be a default child under `emerging/`, or introduced by a workflow later?
- Should `sessions/` be the only default child under `working/`?
- Should `working/` include a small current-state route, or should sessions carry current context first?
- Should compact current understanding be direct files in `crystallized/`, or should a named child folder exist later if the direct-file model becomes crowded?
- Should `observations/`, `ideas/`, and `analysis/` be default children under `emerging/`?
- Should each Memory folder have its own category entrypoint and axioms? Current answer: yes, if it is installed as a routed folder.
- Should archival rules live in the root `memory/_memory.md`, each child entrypoint, or both?

## Superseded Bias

Earlier grouped candidate:

```text
memory/
  active/
  history/
    sessions/
    handoffs/
  seeds/
    observations/
    ideas/
  analysis/
  crystallized/
    decisions/
    documents/
```

Superseded reason:

- stronger lifecycle expression
- no duplicate `briefs/` versus `documents/` truth
- still only one root `memory/` route
- each folder can still own precise axioms
- lifecycle can be explained in `memory/_memory.md`

## Non-Negotiables

- Active memory must be short-lived.
- Raw memory must not become automatic truth.
- Archived memory must remain available but contextual.
- Crystallized memory must be validated against current reality.
- Crystallization must preserve enough source context to be auditable.
- Existing crystallized memory should be consolidated, not duplicated.
- Changed reality is allowed; history must not be erased.
- Agents should have explicit places to write observations and analyses.
- Memory must grow from both human input and agent-discovered facts.
