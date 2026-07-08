---
open-forge:
  description: Deep Layer 2 memory analysis across personas, durable project records, and Layer 3 workflows
  tags: [Layer2, Memory, Persona, ProjectMemory, ProcessMemory, Workflow]
---

# Layer 2 Memory Deep Analysis

## Naming Correction

Layer 2 should probably be called **Memory**, not **Process Memory**.

Process Memory is part of the layer, but the layer must also hold long-term project memory: decisions, rationale, project essence, PRDs, architecture docs, design docs, research notes, product context, and other durable records.

Better naming:

- official layer name: Memory
- precise subdomain: Process Memory
- precise subdomain: Project Memory
- avoid as official layer name: Process Memory

The root folder should probably be:

```text
.agents/memory/
```

`process/` is too narrow. `project/` is too narrow for normal people, designers, product work, research, and private knowledge. `memory/` is broad, but that is acceptable if the category entrypoint makes the boundaries explicit.

## Core Distinction

Layer 1 Core tells agents how to route, obey, judge, shape, and work.

Layer 2 Memory tells agents what has happened, what is currently happening, what was learned, what was decided, and what durable project records exist.

Layer 3 Extensions provide optional skills, workflows, templates, integrations, and specialized packs that consume and update Layer 2 Memory.

Layer 2 must not become another hidden source of behavior rules. If something changes mandatory behavior, shape, judgment, reusable capability, workflow, or workspace routing, it must be promoted into the matching Core category.

## Jobs To Be Done

Layer 2 Memory should support these jobs:

- preserve current work state without relying on chat history
- preserve raw chronological work history
- capture brainstormed possibilities without making them true
- capture useful observations found during work
- store structured analyses that may later become decisions, documents, or tasks
- store approved decisions and their reasoning
- store compact crystallized briefs for cheap future loading
- store or route durable documents such as PRDs, architecture docs, design docs, and research summaries
- provide small handoffs for agents, threads, and humans
- give Layer 3 workflows stable places to read from and write to
- separate active, candidate, historical, accepted, and external truth
- make stale material easy to spot
- support one person, a team, many projects, or a non-code workspace

## Persona Pressure

### Developer

A developer needs:

- active implementation state
- session logs for what changed and why
- observations about codebase behavior
- architecture decisions
- refactor ideas
- testing notes
- links to external issues or pull requests
- handoffs when a thread or agent stops mid-work

Developer workflows in Layer 3 may include implementation, testing, refactoring, review, debugging, architecture, migration, and multi-phase delivery. Those workflows should write sessions, active memory, observations, decisions, handoffs, and possibly external task updates.

### Product

A product person needs:

- product goals
- PRDs
- requirements
- customer or stakeholder insights
- roadmap ideas
- decision rationale
- open questions
- handoffs to design or engineering

Product workflows in Layer 3 may include discovery, planning, PRD creation, prioritization, task creation, roadmap shaping, and stakeholder summaries. Those workflows should write ideas, observations, documents, decisions, handoffs, and external tracker items when integrations exist.

### Designer

A designer needs:

- design briefs
- UX rationale
- research findings
- design-system notes
- accessibility observations
- accepted design decisions
- explorations that are not accepted yet
- handoffs to engineering or product

Design workflows in Layer 3 may include UI/UX exploration, design review, research synthesis, design-system work, accessibility review, and implementation handoff. Some output belongs in Memory documents and decisions; repeatable design shapes may be promoted to patterns or guidelines.

### Normal Person

A normal person may use Open Forge for:

- personal projects
- writing
- study notes
- planning
- decisions
- conversations with AI over time
- simple task lists
- household or life admin

This persona should not need GitHub, Jira, architecture docs, or developer vocabulary. The same Memory shape must still work by treating documents, ideas, sessions, decisions, and handoffs as plain markdown records.

### Team Or Organization

A team needs:

- durable decisions
- shared context
- onboarding history
- handoffs across people
- source links to external systems
- records of why something exists
- separation between proposed, accepted, and obsolete material

Teams amplify the stale-memory problem. Layer 2 must make authority and freshness readable without adding a heavy database-like metadata system.

## Memory Types

### Active Memory

Active memory is short-lived state for current work.

It answers:

- what are we doing now
- what is in scope
- what is the current plan
- what is blocked
- what should happen next
- where is the active session or external task

Active memory is not authority. It is resumability context.

It must stay small, be updated aggressively, and be cleared or archived when the work ends.

### Raw Memory

Raw memory preserves what happened.

Sessions are the main raw memory.

Raw memory may be noisy, wrong, incomplete, or superseded. It is useful for reconstruction, audit, and extraction. It should not be treated as approved truth by default.

### Candidate Memory

Candidate memory captures what might be useful.

Ideas, brainstorms, questions, risks, and backlog-like notes live here.

Candidate memory is explicitly not accepted truth. It is a waiting room for later promotion, rejection, or archival.

### Observed Memory

Observed memory captures useful findings.

Observations are stronger than ideas because they claim something was noticed, but they may still need verification, scope, and promotion.

Examples:

- this package owns auth
- this API has a rate limit
- this UX flow confuses users
- this test fails only on Windows

Observed memory is valuable because AI agents often discover useful facts while doing another job.

### Analysis Memory

Analysis memory stores structured reasoning outputs.

Analysis is deeper than an observation but not necessarily accepted truth. It may compare options, investigate causes, synthesize research, or explain tradeoffs.

Examples:

- architecture option comparison
- product opportunity analysis
- UI/UX critique
- root-cause analysis
- bug investigation summary
- performance analysis
- research synthesis before it becomes a document

Analysis memory is useful across developers, product people, designers, teams, and normal personal projects.

### Accepted Memory

Accepted memory records decisions and durable conclusions.

Decisions explain what was accepted, why, and what alternatives were rejected.

Accepted memory can influence future work, but it still does not automatically become a directive, pattern, guideline, skill, workflow, or workspace route. Those promotions must be explicit.

### Crystallized Memory

Crystallized memory stores compact current understanding.

Compact direct files in `crystallized/` are the main crystallized memory. They are shorter than documents and more curated than sessions, ideas, observations, or analysis.

Examples:

- project brief
- product brief
- architecture brief
- design brief
- domain brief
- user or team preference brief

Crystallized direct files should be cheap for agents to load before deciding whether they need longer documents or raw history.

### Document Memory

Document memory holds or routes durable artifacts.

Examples:

- PRD
- architecture document
- design brief
- research synthesis
- project essence
- product vision
- domain glossary
- operating model
- release plan

Documents may live inside `.agents/memory/documents/` or outside `.agents/` with a Memory route pointing to them. The framework should support both.

### Transfer Memory

Transfer memory helps another agent, thread, or person resume.

Handoffs are the main transfer memory. They should be short and distilled, not another raw session log.

## Proposed Layer 2 Shape

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
    documents/
      _documents.md
    handoffs/
      _handoffs.md
```

This is more extensive than the previous Process Memory proposal, but it matches the real output targets needed by Layer 3 workflows.

The installed package may still choose to seed only the umbrella entrypoint and a subset of child categories. The conceptual model should allow all of them.

## Category Meanings

### `memory/`

Root Memory route.

It explains authority boundaries, promotion rules, freshness expectations, and the generated routes to child memory categories.

### `active/`

Current live state.

Use for small current goals, plans, blockers, and resume pointers.

Do not use for long-term backlog or accepted truth.

### `sessions/`

Chronological raw work logs.

Use for per-session, per-day, or per-substantial-task records. Per session is most precise. Per day is easier but may mix unrelated work. Per substantial task is best when a stable task identity exists.

### `ideas/`

Candidate thinking.

Use for brainstorms, proposed designs, postponed improvements, alternative approaches, and unapproved possibilities.

Ideas can be grouped by topic, project, workflow, or date.

### `observations/`

Useful findings.

Use for facts noticed during work that may matter later but are not yet promoted to durable docs, decisions, directives, patterns, guidelines, or workspace routes.

Observation is the bridge between raw sessions and durable knowledge.

### `analysis/`

Structured reasoning.

Use for tradeoff analysis, investigations, critiques, research synthesis, option comparisons, and other reasoning outputs that should be easier to find than raw session material.

Analysis can later be promoted to decisions, documents, Core categories, external tasks, or archived historical context.

### `decisions/`

Accepted decisions and rationale.

Use for "we chose X because Y" records. Architecture decisions, product decisions, design decisions, process decisions, and personal project decisions can all live here.

If a decision creates mandatory behavior, promote that behavior into directives. If it creates a repeatable shape, promote the shape into patterns. If it creates recurring judgment, promote that into guidelines.

### `crystallized/` direct files

Compact current understanding.

Use direct files in `crystallized/` for distilled project, product, architecture, design, domain, user, or team context that should be cheap to load before longer records.

These files are crystallized from sessions, observations, analysis, decisions, and documents.

### `documents/`

Durable long-form project records.

Use for PRDs, architecture docs, design docs, research summaries, project essence, domain glossaries, product vision, and similar artifacts.

Documents may be real files in Memory or route files pointing to external/internal document locations.

### `handoffs/`

Distilled transfer notes.

Use for "resume from here" material between agents, threads, humans, workflows, or sessions.

Handoffs should point to raw sessions or active memory when details matter.

## Tasks And Backlog

Tasks are still dangerous as a default Layer 2 category.

Reasons:

- many workspaces already have GitHub Issues, Jira, GitLab, Linear, or another task authority
- local AI tasks can become stale
- task status is operational state, not just memory
- task templates and decomposition are usually workflow or integration behavior

Recommended split:

- active memory may include a temporary checklist for current work
- ideas may include candidate future work
- decisions may approve work direction
- documents may include planning docs
- external trackers remain authoritative when configured
- a Layer 3 planning extension may add `memory/tasks/` or `memory/backlog/`

Layer 2 should permit tasks but not require them.

Possible future planning extension:

```text
.agents/
  memory/
    tasks/
      _tasks.md
    backlog/
      _backlog.md
  workflows/
    planning/
      _planning.md
  skills/
    task-creation.md
```

That extension can define how local AI tasks relate to GitHub, Jira, GitLab, Linear, or plain markdown checklists.

## Layer 3 Interaction

Layer 3 Extensions should use Memory as their stable persistence target.

Examples:

- brainstorming workflow writes sessions and ideas
- planning workflow reads ideas, decisions, documents, and external tasks, then writes active memory, handoffs, and optional task records
- task creation workflow writes external issue templates or local planning module files
- implementation workflow writes active memory, sessions, observations, and handoffs
- testing workflow writes observations and possibly decisions when testing strategy changes
- refactoring workflow writes sessions, observations, decisions, and follow-up ideas
- architecture workflow writes decisions and documents
- UI/UX workflow writes ideas, observations, analyses, design decisions, design documents, and handoffs
- product workflow writes PRDs, decisions, ideas, and external tickets

Templates belong to Layer 3 modules. Filled outputs usually belong to Layer 2 Memory or external systems.

External templates, such as GitHub issue templates, can be routed by workspace files or by a Layer 3 integration. Memory should store the local record, summary, or pointer, not silently duplicate the external source of truth.

## Promotion Flow

Memory should support a clear promotion flow:

```text
active work
  -> sessions
  -> observations / ideas / analysis
  -> decisions / crystallized files / documents
  -> core categories or external systems when needed
```

Promotion targets:

- directive: mandatory behavior
- pattern: repeatable inspectable shape
- guideline: recurring contextual judgment
- skill: reusable capability
- workflow: repeatable goal-oriented module
- workspace route: important destination
- decision: accepted rationale
- document: long-form durable record
- external task: issue tracker or planning system

Anything not promoted remains historical or candidate context.

## Authority Model

Memory category authority should be explicit:

```text
active/        current resumability context, not authority
sessions/      historical raw context, not authority
ideas/         candidate context, not authority
observations/  useful findings, verify before promotion
analysis/      structured reasoning, promote before treating as accepted truth
decisions/     accepted rationale within stated scope
crystallized/  compact current understanding through direct files
documents/     durable project record within stated scope
handoffs/      resume summary, not full truth
archive/       historical context only
```

Core categories remain the authority for agent behavior.

External systems remain authority when a workspace declares them as source of truth.

## Extraction Loop

Layer 2 should encourage extraction, not hoarding.

Agents and workflows should periodically ask:

- did this session produce an observation
- did this work produce an analysis worth keeping
- did this brainstorm produce an idea worth keeping
- did this discussion produce a decision
- did this change require updating a crystallized file
- did this project need a document update
- did this repeated behavior belong in a directive, pattern, guideline, skill, or workflow
- did this work create or update an external task
- should active memory be cleared or handed off

This loop is where long process history becomes compact durable memory.

## Archive

Archive should remain a nested convention.

Examples:

```text
memory/sessions/archive/
memory/ideas/archive/
memory/handoffs/archive/
memory/documents/archive/
```

Archive means historical context only. It is not active truth unless restored or explicitly referenced.

## Install Surface

There are two viable install surfaces.

### Full Memory Layer

```text
memory/
  _memory.md
  active/_active.md
  sessions/_sessions.md
  ideas/_ideas.md
  observations/_observations.md
  analysis/_analysis.md
  decisions/_decisions.md
  documents/_documents.md
  handoffs/_handoffs.md
```

This is clearer for serious use and gives Layer 3 immediate output targets.

### Minimal Memory Layer

```text
memory/
  _memory.md
  active/_active.md
  sessions/_sessions.md
  ideas/_ideas.md
  handoffs/_handoffs.md
```

This is calmer, but it lacks explicit places for observations, decisions, and durable documents. Layer 3 would quickly need to create them.

Current bias: install the full Memory Layer when Layer 2 is explicitly installed. It is not Core, so it can afford to be more complete.

## Open Design Questions

- Should Layer 2 install by default after Core, be offered by a wizard, or require an explicit command?
- Should the folder be `memory/` or something more specific?
- Should `documents/` be called `docs/`, `records/`, `knowledge/`, or `documents/`?
- Should observations be first-class or just extracted session notes?
- Should analysis be first-class or a document/session subtype?
- Should decisions be first-class from the start?
- Should compact crystallized files be called briefs, crystals, summaries, or simply crystallized files?
- Should handoffs be first-class or generated from active memory and sessions?
- Should active memory be one `current.md` file or many files by task/session?
- How should stale active memory be detected?
- How should external task systems declare authority without making Layer 2 tool-specific?
- Should a planning extension add `tasks/` and `backlog/`, or should Memory install them empty?
