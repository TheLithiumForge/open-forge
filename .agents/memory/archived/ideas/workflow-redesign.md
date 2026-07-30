---
open-forge:
  description: Historical rationale behind the former eight-section, mode-aware, and phase-aware Workflow contract
  tags: [Memory, Archived, Idea, Contextual, Historical, Workflow, Routing, Loading, Orchestration]
---

# Workflow Redesign

Status: archived 2026-07-18.  
Original route: `.agents/memory/emerging/ideas/workflow-redesign.md`.  
Archived because: the workflow contract was accepted, implemented, and subsequently refined with Mode, mandatory Constraints, and phase-aware routing.  
Current authoritative source: `.agents/memory/crystallized/decisions/workflow-shape.md` and the current Workflow Core route.

Status: historical rationale accepted 2026-07-10, revised into the later eight-section phase-aware contract, and superseded by the minimal Workflow contract accepted on 2026-07-29. The body below preserves the earlier design context and worked example. Do not treat its modes, sections, phase assumptions, selection ceremony, or local-Core behavior as current truth.

## What A Workflow Is

A workflow is a routed markdown recipe for reaching a defined goal that takes more than one step or more than one skill. It is goal-oriented by contract, deterministic where steps are known, iterative where they are not, and an orchestration surface where work spans skills, subagents, or other workflows. It is not a runtime orchestration object from an agent SDK; that line stays so Open Forge never redefines SDK-native or tool-native workflow concepts.

## Two Distinct Routes, By Design

- Generated `Entries` express containment: workflow-local files and child categories under the workflow folder. The CLI owns them.
- `Required Routes` express dependency: cross-tree edges to routes the workflow needs but does not contain. The author owns them. Generated entries structurally cannot carry these edges, so the two sections never compete.

Humans and agents reason about them the same way: "what is inside me" versus "what I need from elsewhere".

## Body Shape

```md
---
open-forge:
  description: {trigger plus outcome, decision-grade; this is the selection surface}
  tags: [{Layer}, Workflow, {topic tags}]
---

# {Name}

{One sentence: the outcome this workflow produces. Selection confirmation, not selection prose.}

## Goal

- outcome: what exists when this workflow succeeds
- acceptance: how agent and user recognize success
- stop: what ends the workflow early (user decision, blocker, scope change)

## Required Routes

Read every route below before Step 1. A route that cannot be read is a blocker to report, not a step to skip.

- `{path}` - {why this workflow needs it}

## Constraints

- {cross-step invariants; neither steps nor outcomes}

## Steps

1. {ordered actions; a step may invoke a skill, consult guidance, delegate to a subagent, or hand off to another workflow by route}

## Loop

{What triggers another pass, which steps repeat, and what stops it. Linear workflows state "linear; no loop".}

## Outputs

- {artifacts produced}

## Completion

- [ ] {checklist that is the stop condition; acceptance from Goal, verification, memory routing follow-up}
- [ ] final response or handoff names the workflow for auditability

## Entries

{generated; workflow-local routes only}
```

Section presence rules: `Goal`, `Steps`, `Loop`, `Outputs`, and `Completion` are always present. `Required Routes` states "none" when the workflow needs nothing beyond baseline context. `Constraints` is omitted when the workflow has no cross-step invariants; forcing it everywhere creates filler. The auditability line lives in `Completion`, not as a global workflow axiom.

## Execution Ergonomics

One shape covers the three modes; the mode emerges from which sections carry weight:

- Deterministic: rich `Steps`, `Loop` says linear. The recipe is the path.
- Iterative: `Steps` plus a `Loop` that repeats a step range until a condition. The recipe is the cycle.
- Goal-seeking: rich `Goal`, minimal assess-act-check `Steps`, `Loop` runs until acceptance. The recipe is the target.

Orchestration rules:

- A step may hand off to another workflow by its route. The sub-workflow's `Required Routes` are read at that activation, not before; activation is the loading event.
- A step may delegate to a subagent or worker. The delegation handoff names the workflow route and the active step so the worker enters the same contract (validated need in the v6 orchestrator-worker seeds).
- Composition stays in `Steps`; a workflow never silently absorbs another workflow's axioms.

## Worked Example: Dev Workflow (TDD Red-Green-Blue)

The maintainer's primary extension workflow, mapped onto the shape to prove the ergonomics:

- Goal: outcome - the selected task's behavior change exists and is verified; acceptance - task acceptance criteria met and tests green after refactor; stop - blocker, scope change, or user pause.
- Required Routes: the implementation skill package, the shared workflow-primitives skill package, and the tasks route (backlog, issues folder, or external tracker route).
- Constraints: tests stay untouched during refactor; contracts before implementation.
- Steps: (1) select the task from the user or the next backlog/issues entry; (2) plan the change - for new behavior establish contracts (class shapes, function signatures, API surfaces), for existing behavior state the behavior delta and identify the moving parts; (3) red - write failing tests from the contracts, and for existing code ensure or add tests pinning current behavior of affected parts; (4) implement - fill the contracts or adjust the behavior; (5) green - run tests until they pass; (6) blue - review against the greater picture, record observations, refactor while tests stay green without being edited; (7) hand off to the post-work subflow - check acceptance criteria, route memory, write the handover.
- Loop: steps 4-5 repeat until green; step 6 repeats until the refactor stabilizes with green tests; return to step 2 when the plan proves wrong.
- Completion: acceptance criteria checked, tests green after refactor without test edits, memory routed, handover written, workflow named.

Every phase lands in an existing section; the post-work subflow is a workflow handoff, which is exactly the orchestration rule above. No new primitive needed.

## Required Routes Rules

- Name: `Required Routes`. "Required Skill Entries" is too narrow (dependencies may be skills, guidance, patterns, memory routes, workspace routes, or other workflows); "Required Route Entries" collides with the defined term `entry`, which belongs to generated regions.
- Flat and unconditional. Prefer entrypoint-level targets (`SKILL.md`, `_category.md`, workflow files); stable routed files such as one exact pattern or guidance file are allowed when the workflow truly depends on that one file.
- Keep the list short; when it grows, split the workflow or route through a package instead. Length is guidance, not contract.
- "none" is a valid value when the workflow needs nothing beyond baseline context.
- Lines use the generated entry format (backtick path, dash, reason) so tooling such as `open-forge context --follow-required` can parse and follow them.
- No directives in the list; workspace directives are already loaded. Required Routes hold only material that is not otherwise loaded and that the workflow cannot run correctly without.
- Loading stays two-tier per the loading-reliability decision: activation reads the workflow body plus all Required Routes; execution reads skill references per step.

## Workflow-Local Core

A workflow may own local #Core routes under its folder; they apply only while the workflow is active and are preferred over broader routes when safe - the natural consequence of the loader's narrower-scope preference, alongside `.overwrite.md` companions and direct edits. This is a brief capability note, not a headline feature.

- Typical use: local directives that are mandatory only while the workflow runs.
- Local skills are discouraged: native skill packages belong under `.agents/skills/` where runtimes discover them; share them through `Required Routes` instead.
- Local patterns and guidance stay possible through the normal recursive category contract; nothing workflow-specific is needed.
- A workflow is not an independent install: no loader, no AGENTS.md, no root categories.

## Contract Changes On Acceptance

- `_workflows.md`: replace the `Required Skill Packages` axiom with `Required Routes`; require Goal, Steps, Loop, Outputs, Completion; Constraints optional; Required Routes may state "none".
- Workflows descriptor and agent-primitives concept: same rename plus the two-routes distinction and the orchestration rules.
- Formatting concept: Required Routes lines use the entry format.
- workflow-essentials and benchmark seeds: migrate the three workflows, add the dev workflow as the fourth, and rerun a seeded round to validate compliance before release.
