---
open-forge:
  description: Layered system architecture, guidance naming, and Layer 2 memory lifecycle
  tags: [OpenForge, Layer, Core, Memory, Guidance, Crystallization, Architecture]
---

# Layered System Memory Architecture

## Summary

The layered model should separate framework behavior, durable memory, and optional capabilities:

```text
Layer 1: Core
Layer 2: Memory
Layer 3: Extensions
```

Layer 1 Core is mostly clear. Layer 2 Memory needs the most design work. Layer 3 Extensions will add workflows, skills, templates, integrations, and optional packs that read from and write to Memory.

## Layer 1 Naming: Guidelines

`guidelines` may be the wrong category name.

The current meaning is good: contextual, adaptable judgment for recurring scenarios. The problem is that `guidelines` can sound like soft rules, which makes it too close to `directives`.

Better candidates:

- `guidance`: best current candidate; means contextual help without implying authority
- `practices`: useful, but may imply project habits and overlap with `patterns`
- `principles`: good for high-level judgment, weaker for scenario-specific guidance
- `heuristics`: accurate for AI reasoning, but too technical and less friendly
- `recommendations`: too weak
- `guides`: too close to documentation/tutorials
- `playbooks`: overlaps with workflows
- `conventions`: overlaps with patterns and directives

Current recommendation:

```text
Rename `guidelines/` to `guidance/`.
```

The category would represent adaptable contextual guidance. Individual files may still be called guidance files or guide files, but the root should probably be `guidance/`.

This rename should happen before finalizing implementation wording if accepted, because it affects payload paths, governance descriptors, README, tests, and generated loader entries.

## Layer 2 Purpose

Layer 2 Memory should answer:

- what is happening now
- what happened before
- what did we notice
- what did we analyze
- what might we do later
- what did we decide
- what is the current distilled understanding
- where are the durable docs
- what should another agent or human know to resume

The deeper goal is self-growing memory. Active context decays, useful truth is extracted and validated, stale history is archived, and compact crystallized memory is updated as reality changes.

Layer 2 is not another behavior layer.

If memory changes mandatory behavior, it must be promoted into `directives`. If it defines repeatable shape, it must be promoted into `patterns`. If `guidelines` becomes `guidance`, recurring judgment must be promoted there. If memory defines a reusable capability or process, it must be promoted into `skills` or `workflows`.

## Memory States

Memory should be organized by state and use, not by one person's workflow.

### Active Memory

Active memory is current state.

It is small, temporary, and updated during work. It tracks current goal, scope, plan, blockers, links, next actions, and resume pointers.

It is not authority. It expires when the work ends.

### Raw Memory

Raw memory is the chronological trail.

Sessions and chat-derived logs live here. Raw memory can contain mistakes, false starts, duplicated text, and superseded plans.

It is useful for reconstruction and extraction, not direct authority.

### Candidate Memory

Candidate memory is possible future truth.

Ideas, brainstorms, questions, risks, rough options, and candidate backlog items live here.

Candidate memory must remain visibly unapproved.

### Observed Memory

Observed memory is noticed factual material.

Observations are stronger than ideas because they claim something was seen, but they still need scope and may need verification.

### Analysis Memory

Analysis memory is structured reasoning.

It captures option comparisons, tradeoff analysis, root-cause analysis, opportunity analysis, UX critique, research synthesis, architecture analysis, and similar material.

Analysis is not accepted truth until promoted.

### Accepted Memory

Accepted memory is approved rationale.

Decisions live here. A decision states what was chosen, why, alternatives considered, and what changed because of it.

### Crystallized Memory

Crystallized memory is compact durable understanding.

This is the part agents should be able to load cheaply before reading long sessions or documents. It contains the current essence of a project, product, domain, architecture, design system, person, team, or workspace.

The best shape is probably direct files inside `crystallized/`, not a separate `briefs/` folder.

`briefs/` overlaps with `documents/` because a brief is still a compact document. Keeping compact current truth directly in `crystallized/` avoids a parallel truth tree.

Examples:

```text
memory/crystallized/project.md
memory/crystallized/product.md
memory/crystallized/architecture.md
memory/crystallized/design.md
memory/crystallized/domain.md
memory/crystallized/user.md
```

Crystallized files should be curated and updated from sessions, observations, analysis, decisions, and documents.

### Document Memory

Document memory stores or routes durable long-form records.

Examples:

- PRD
- architecture document
- design document
- research summary
- product vision
- operating model
- domain glossary
- requirements document

Documents may live in `.agents/memory/documents/` or elsewhere with route files pointing to them.

### Transfer Memory

Transfer memory helps someone resume.

Handoffs live here. They are concise, scoped, and point to the raw or durable sources when details matter.

## Recommended Layer 2 Shape

Current accepted state-container shape:

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

The exact default child folders remain open for governance review. The accepted top-level Memory states are `working/`, `emerging/`, `crystallized/`, and `archived/`.

Earlier flat candidate:

```text
.agents/
  memory/
    _memory.md
    active/
      _active.md
    sessions/
      _sessions.md
    ideas/
      _ideas.md
    observations/
      _observations.md
    analysis/
      _analysis.md
    decisions/
      _decisions.md
    crystallized/
      _crystallized.md
    documents/
      _documents.md
    handoffs/
      _handoffs.md
```

This looks like many folders, but it is one Layer 2 route from the loader. Since Memory is installed separately from Core, it can be more complete.

Superseded grouped candidate:

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

Current bias has shifted toward the state-container candidate. It expresses the memory lifecycle better while still keeping only one root `memory/` route. Each child folder can still have a category entrypoint and axioms.

## Should There Be An Inbox

An `inbox/` or `captures/` folder is tempting because agents often need a safe place to append uncertain material.

Current bias: do not add it first.

Reasons:

- `working/sessions/` already handles raw chronological capture
- unclear material can start as a session note or idea
- `inbox/` tends to become a stale junk drawer
- Memory should teach extraction, not hoarding

If a future workflow cannot classify output safely, a Layer 3 extension can add `memory/inbox/` with cleanup rules.

## Tasks And Backlog

Do not install `tasks/` or `backlog/` by default in Memory yet.

Tasks are operational state, not just memory. Many users already have GitHub Issues, Jira, GitLab, Linear, or a personal task system.

Recommended split:

- `working/` may contain a current temporary checklist
- `emerging/ideas/` may contain possible future work
- `emerging/analysis/` may compare possible work
- `crystallized/decisions/` may approve a direction
- `crystallized/documents/` may contain planning docs
- Layer 3 planning/task extensions may add `tasks/` and `backlog/`
- external trackers remain source of truth when configured

Layer 3 planning modules can define templates, synchronization, GitHub/Jira/GitLab/Linear behavior, and local task semantics.

## Crystallization

Crystallization is the process of turning noisy work memory into compact useful memory.

Flow:

```text
active work
  -> working sessions
  -> emerging observations / ideas / analysis
  -> crystallized decisions / files / documents
  -> Core categories or external systems when behavior or work tracking changes
```

Crystallization should happen:

- before a handoff
- before context compaction or thread switch
- after a substantial session
- after a decision is made
- after a document is created or materially changed
- when the same observation appears repeatedly
- when a workflow completes
- before closing working memory

Crystallization should not mean summarizing everything. It means extracting only material that changes future work.

Crystallization must validate memory against current reality. It should update or reshape existing crystallized files and documents instead of creating duplicate truth. When reality changes, memory can break and regrow, but the history remains archived.

## Cleaning Rules

Memory stays clean by separating state:

- `working/` is small and temporary
- `working/sessions/` can be long and raw
- `emerging/ideas/` are unapproved
- `emerging/observations/` are useful findings
- `emerging/analysis/` is structured reasoning
- `crystallized/decisions/` are accepted rationale
- direct files in `crystallized/` are compact current understanding
- `crystallized/documents/` are durable long-form records
- handoffs are session or workflow artifacts unless a workflow installs a more specific route
- `archived/` is historical context only

Every promoted item should point back to source material when useful.

When a memory item is superseded, mark it or move it to an `archive/` folder inside the same area.

## Persona Scenarios

### Developer

Uses `working/`, `working/sessions/`, `emerging/observations/`, `emerging/analysis/`, `crystallized/decisions`, `crystallized/architecture.md`, `crystallized/documents/`, and workflow-specific handoff artifacts when useful.

Layer 3 implementation, debugging, refactoring, testing, and architecture workflows write to these locations.

### Product

Uses `emerging/ideas/`, `emerging/analysis/`, `crystallized/decisions/`, `crystallized/product.md`, `crystallized/documents/`, and workflow-specific handoff artifacts when useful.

Layer 3 discovery, PRD, roadmap, prioritization, and task-creation workflows use these locations.

### Designer

Uses `emerging/ideas/`, `emerging/observations/`, `emerging/analysis/`, `crystallized/decisions/`, `crystallized/design.md`, `crystallized/documents/`, and workflow-specific handoff artifacts when useful.

Layer 3 UX, UI, accessibility, research, and design-system workflows use these locations.

### Normal Person

Uses `working/`, `emerging/`, `crystallized/`, and `archived/` without needing developer vocabulary.

The same model works for writing, study, personal planning, purchases, life admin, and long-running conversations with AI.

### Team

Uses all of Memory, especially `decisions/`, `crystallized/`, `documents/`, and `handoffs/`.

The team problem is not capture; it is stale capture. Crystallized files, decisions, and archive conventions are the safety mechanisms.

## Layer 3 Responsibilities

Layer 3 Extensions should provide:

- brainstorming workflows and skills
- planning workflows and skills
- task creation workflows and templates
- GitHub/Jira/GitLab/Linear integrations
- development workflows
- testing and refactoring workflows
- architecture workflows
- UI/UX workflows
- product workflows
- domain-specific packs

Layer 3 owns templates and specialized process.

Layer 2 owns the persisted outputs and links.

## Open Questions

- Should Layer 2 install the full Memory shape by default when selected?
- Should compact crystallized files be called crystals, summaries, or simply crystallized files in user docs?
- Should `guidelines/` be renamed to `guidance/` before the implementation wording pass?
- Should `documents/` be named `documents/`, `docs/`, or `records/`?
- Should `analysis/` be installed by default or introduced by workflows?
- Should working memory use one `current.md`, multiple active files, or sessions first?
- How should stale working memory be detected without adding heavy metadata?
- How should Layer 3 workflows know when to crystallize memory?
