---
open-forge:
  description: Maintainer-authored findings on recoverability, active-context size, lifecycle, and document ownership
  tags: [Memory, Emerging, AuthorsFindings, Author, Contextual, Candidate]
---

# Open Forge Authors' Findings Log

## Finding 2026-09-11 — Maximum recoverability, minimum active context

Source: maintainer-authored note recorded during the CLI documentation reset.
Status: emerging and protected; it is not yet a Directive or CurrentTruth.

### 1. Minimal sufficient context is still the goal

Open Forge may support very large or effectively unbounded context, but that
does not mean agents should consume all available context.

The operating principle is:

> Provide unlimited context availability, but require the smallest sufficient context.

The Framework should maximize how much information can be recovered when needed
while minimizing what must be loaded for any particular task.

### 2. More documents create maintenance cost

Having hundreds of detailed documents is useful until shared behavior changes.
If one underlying concept requires editing 30 or 40 documents, the knowledge
model has duplicated ownership.

Shared behavior should live in one authoritative source and be referenced from
dependent documents.

Prefer:

> one owned fact → many references

over:

> one fact → many synchronized copies

### 3. Analysis should terminate in execution

Retaining many analysis documents as active inputs while Tasks merely link back
to them makes Tasks non-self-sufficient, invites repeated or contradictory
analysis, and leaves old reasoning easy to mistake for current meaning.

The preferred lifecycle is:

> Analysis → conclusion → sealed Analysis → actionable Task

The Task should contain the actionable requirements necessary to execute the
work. It may reference the Analysis for rationale and provenance, but execution
should not require reconstructing the Task from a collection of analyses.

### 4. Tasks should absorb implementation reality

Once execution begins, implementation discoveries and deviations that affect the
Task belong in the Task or its active execution state. The original Analysis
normally remains sealed. Historical reasoning should not be continuously
rewritten because execution discovered something new.

Each knowledge level should support the next level rather than competing with
it.

### 5. Documents should have explicit lifecycle roles

The rough progression is:

> Analysis / Idea → Task → Evergreen / Current Truth → Sealed / Archived material

Analysis and Ideas hold reasoning and exploration. Tasks hold executable current
work. Evergreen and Current Truth hold durable accepted meaning. Sealed and
Archived material preserve provenance and historical evidence. Temporary sources
should stop acting as active authorities once their useful meaning has moved
upward.

### 6. Open Forge should optimize for deletion too

Open Forge should help users remove unnecessary structure, not only create more
files and context. It should encourage consolidation, replacement of duplicated
sections with shared references, sealing Analysis after its outcome reaches a
Task, retaining only durable summaries after completion, and archiving or
deleting temporary material whose useful information has already been promoted.

Creating documents is easy. Maintaining the smallest coherent knowledge system
is the harder and more valuable problem.

### Core product principle

> Open Forge should make it safe to have as much context as necessary while helping the workspace retain as little active context as possible.

Short form:

> Maximum recoverability. Minimum active context.

### Application boundary

This finding authorizes the current documentation cleanup as a user-directed
experiment. It does not by itself authorize changing accepted Framework or CLI
runtime behavior. Future promotion requires separate evidence and maintainer
acceptance.
