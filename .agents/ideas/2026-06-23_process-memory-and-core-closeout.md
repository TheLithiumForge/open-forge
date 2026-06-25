---
open-forge:
  description: Backlog and reasoning for Layer 1 core closeout and Layer 2 memory
  tags: [OpenForge, Core, Memory, Backlog, Architecture]
---

# Memory And Core Closeout

## Backlog

Current working backlog:

1. Finish Layer 1 Core review.
2. Decide whether Layer 1 needs any missing category or rule.
3. Design Layer 2 Memory as one coherent package.
4. Decide whether Memory is one umbrella category or several root categories.
5. Rewrite process-memory meanings from scratch.
6. Define governance descriptors and minimum payload files for the accepted Memory shape.
7. Update user documentation after the layer model is settled.
8. Later, design Layer 3 Extensions and the CLI module experience.

Further analysis refined the name: Layer 2 should probably be Memory, not Process Memory. Process Memory is a subdomain, while the layer also needs project memory such as documents, decisions, rationale, and project essence.

## Layer 1: Core Status

Layer 1 Core is structurally close to complete.

Current Core shape:

```text
AGENTS.md
.agents/
  loader.md
  directives/
    _directives.md
  guidelines/
    _guidelines.md
  patterns/
    _patterns.md
  skills/
    _skills.md
  workflows/
    _workflows.md
  workspace/
    _workspace.md
```

This is enough for the minimum framework because it provides:

- one entrypoint
- one loader
- category routing
- workspace discovery
- mandatory behavior through directives
- contextual judgment through guidelines
- concrete shapes through patterns
- reusable capabilities through skills
- larger goal modules through workflows

Layer 1 should not absorb process memory just because process memory is useful. The core should remain the smallest stable framework that can route and govern later material.

## Layer 1: Missing Pieces Check

Likely not missing from Core:

- `sessions/` because sessions record work history, not framework behavior
- `ideas/` because ideas are candidate future work, not active framework truth
- `handoffs/` because handoffs are process transfer notes, not core routing
- `tasks/` because tasks are work management, not framework structure
- `decisions/` because decisions are process memory or project memory, not required to run the core
- `archive/` because archive is a state or location convention, not a root primitive
- `rules/` because directives already cover mandatory requirements
- `prompts/` because skills and workflows cover reusable agent behavior without creating a prompt bucket
- `knowledge/` because users can add local routed categories when a workspace needs them

Possible Core refinements before calling it stable:

- review and approve or revise guidelines, skills, and workflows
- run a consistency pass across all payload axioms
- run a prompt-injection and generated-region safety pass
- make README wording match the final layer names
- decide whether the docs should call Layer 1 `Core` or `Core Context`

No new root category currently looks necessary for Layer 1.

## Layer 2: Memory Purpose

Memory records how work changes over time.

It should answer questions like:

- what happened before
- what is being considered
- what was decided
- what is being handed off
- what remains to do
- what is historical and no longer active truth

Memory should not become another place for active directives, patterns, guidelines, skills, workflows, or workspace routing. Those already have Core categories.

Memory likely needs three levels:

1. Active memory: short-lived state for the current work.
2. Raw memory: chronological or captured material.
3. Distilled memory: extracted useful conclusions and reusable summaries.

Active memory prevents handoff loss. Raw memory preserves the full trail. Distilled memory keeps agents from rereading long histories when only the conclusion matters.

The model must make stale active memory easy to detect and cheap to clean up.

## Layer 2: Candidate Concepts

### Sessions

Sessions are chronological work logs.

They capture context, progress, decisions-in-progress, commands, observations, and resumability for a period of work.

Keep if we want agents and humans to resume long work without reconstructing history from chat.

Sessions are raw memory. They may contain noise, false starts, superseded ideas, and temporary plans. They are useful for reconstruction, not automatic authority.

Default session creation could be per session, per day, or per substantial task. Per session is most precise. Per day is easier for humans but can mix unrelated work. Per substantial task is cleanest when task identity exists, but not every workspace has one.

### Ideas

Ideas are unapproved candidate material.

They capture proposals, possible designs, rejected or postponed options, and future exploration.

Keep if we want a clear parking lot that does not become active truth by accident.

Ideas are candidate memory. They can come from brainstorming, observations, problems noticed during work, or postponed possibilities.

Ideas should remain explicitly unapproved until promoted into a directive, pattern, guideline, skill, workflow, workspace route, decision, or task source.

### Observations

Observations are facts or notable findings noticed during work.

They are more factual than ideas but still may need verification, scope, and promotion before they affect behavior.

Observation may be its own category, a section inside sessions, or a promoted idea type. It is valuable because agents often discover useful project facts while doing unrelated work.

Keep observations if we want a lightweight bridge between raw sessions and durable routed truth.

### Decisions

Decisions are approved durable conclusions with rationale.

They answer what was decided, why, and what changed because of it.

Consider keeping only if decisions need to be easier to find than searching sessions and ideas. Otherwise decisions can begin as a convention inside sessions or an eventual optional module.

### Tasks

Tasks are actionable work packages.

They capture what needs doing, status, scope, dependencies, and acceptance checks.

Consider carefully. Tasks can become a heavy project-management system if added too early. They may belong in workflows or external issue trackers instead of core Process Memory.

Tasks are a can of worms because workspaces often already have GitHub Issues, Jira, GitLab, Linear, or another source of truth.

Open Forge should not pretend local AI tasks automatically outrank those systems.

Possible split:

- external tasks: authoritative items from GitHub, Jira, GitLab, or another configured tracker
- AI task list: local proposed or decomposed next actions, derived from the current work and external tasks when available
- backlog: local candidate work that is not yet accepted as an external task

If a planning module exists, task creation and synchronization probably belong there as Layer 3. Layer 2 may only need a place to record local task-like notes without making them authoritative.

### Active Memory

Active memory is short-lived state for the current work.

It can contain:

- current goal
- current scope
- current plan or checklist
- next actions
- blockers
- current handoff summary
- links to the active session, idea, task, or external issue

Active memory should not be a long-term backlog. It should expire, be cleared, or be archived when the work is completed or abandoned.

Active memory may be implemented as:

```text
process/
  active/
    _active.md
    current.md
```

or as a section inside the active session file. A dedicated `active/` route is easier for agents to find but increases the installed surface.

Guardrails:

- active memory is resumability context, not authority
- it must point to the source material when possible
- it should be small
- it should be overwritten or archived instead of growing forever
- stale active memory must be visibly marked or removed

### Handoffs

Handoffs are transfer notes.

They capture the current state, what changed, what remains, risks, and where to resume.

Handoffs may be their own concept or a session output type. They should not become a second session system.

Handoffs are distilled active memory. They should capture the minimum context needed for another agent, thread, or human to resume safely.

A handoff can be created from a session, active memory, or an interrupted workflow. It should not duplicate the entire session.

### Backlog

Backlog is a local list of candidate work.

It may include ideas to revisit, AI-suggested next actions, cleanup items, or untriaged work. It should not automatically imply commitment.

Backlog is useful, but it overlaps tasks and external trackers. It may be safer as a postponed Memory subcategory or Layer 3 planning module until task ownership is clear.

### Archive

Archive means historical context, not active truth.

Archive should probably be a nested convention usable under any process-memory area:

```text
sessions/archive/
ideas/archive/
tasks/archive/
```

Archive does not need to be a root category unless we later discover cross-cutting archived material needs a single route.

## Layer 2 Shape Options

### Option A: Several Root Categories

```text
.agents/
  sessions/
  ideas/
  decisions/
  tasks/
  handoffs/
```

Pros:

- direct routing from loader
- each concept has a clear index
- easy to inspect one category at a time

Cons:

- too many root routes
- heavier first impression
- boundaries may feel artificial
- more governance files to maintain

### Option B: One Process Category

```text
.agents/
  process/
    _process.md
    active/
      _active.md
    sessions/
      _sessions.md
    ideas/
      _ideas.md
    handoffs/
      _handoffs.md
    observations/
      _observations.md
    analysis/
      _analysis.md
```

Pros:

- one root route
- makes Layer 2 feel like one package
- keeps the loader calm
- lets us add or trim subcategories later
- avoids over-promoting uncertain concepts

Cons:

- one extra routing hop
- `process` may be too generic as a name
- needs clear subcategory meanings to avoid becoming a junk drawer

### Option C: Memory Category

```text
.agents/
  memory/
    _memory.md
    sessions/
    ideas/
    decisions/
    handoffs/
```

Pros:

- user-facing name is warmer and less mechanical than process
- captures the durable record aspect
- one root route

Cons:

- may be confused with all knowledge or workspace context
- could overlap with `workspace/`
- less precise than the full Memory layer meaning

## Current Recommendation

Use one Layer 2 umbrella route, but keep the official name undecided until user docs are drafted.

Best current structure:

```text
.agents/
  process/
    _process.md
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
    handoffs/
      _handoffs.md
```

Start with active, sessions, ideas, observations, analysis, and handoffs.

Postpone decisions and tasks:

- decisions can be extracted from sessions or ideas once the need is proven
- tasks overlap strongly with workflows and external issue trackers
- backlog may be useful, but should wait until task ownership and planning-module boundaries are clearer

Keep archive as a nested convention, not a root category.

## Open Questions

- Should the umbrella folder be `process/`, `memory/`, or `process-memory/`?
- Should decisions be included in the first Memory package?
- Should handoffs be a category or a session file type?
- Should tasks be omitted entirely until workflows define task creation?
- Should active memory be a dedicated `active/` route or a required section in sessions?
- Should observations be first-class or a promotion path from sessions and ideas?
- Should analysis be first-class or a document/session subtype?
- Should backlog exist in Memory, or only in a planning/task module?
- Should Layer 2 ship by default after Core, be offered by the installer, or be installed through an explicit command?
- How much of Memory should be framework-managed versus immediately user-owned?
