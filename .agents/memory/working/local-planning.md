---
open-forge:
  description: Review local planning around the Planning Extension, templates, task lifecycle, completion tracking, organization, and archival before any source change
  tags: [Memory, Working, Task, Contextual, Active, Framework, Planning, Templates, Lifecycle, Archival]
---

# Local Planning Review — Planning Extension, Templates, and Work Lifecycle

## Task State

- State: **Open**. This task only checks and designs the local planning
  contract; it does not implement changes to the Planning Extension, templates,
  source documents, or entrypoints.
- Owner: Root, direct sequential review.
- Current phase: 1/5 — inventory the current Planning Extension, templates,
  local task records, and completion/archival rules.
- Current next step: compare the current package, repository dogfood, active
  task route, project-control rules, and archival contracts, then write one
  disposition and implementation boundary for each planning concern.

## Purpose

This is a review/decision task, not an implementation task. Local planning needs one coherent answer from accepted direction through
execution and closeout. The current optional `planning` Extension provides a
Planning Workflow, Work Records Pattern, and four Templates, while this
repository also has local task, phase, checkpoint, handoff, project-control,
and Archived Memory conventions. The package intentionally avoids importing
this repository's permanent IDs, completion-grace counters, and orchestration
ledger, so the boundary between a portable Extension and repository-local
planning must be explicit.

This task therefore covers both the existing rule review and the missing
operating contract: how to create and organize local work, when to use a Task,
Plan, Backlog, Checkpoint, or Handoff, how to distinguish active, blocked,
complete, superseded, and archived work, what evidence marks a task done, how
completed work remains discoverable without polluting the active route, and
how the Planning Extension and its templates should evolve without taking
authority from instantiated records.

The working name is **Local Planning**. It names this repository's planning
convention and task, not a rename of the optional package whose stable
extension ID remains `planning`.

## Current planning surfaces

- Canonical Extension source: `src/extensions/planning/`, containing the
  `planning` Workflow, Work Records Pattern, and Task, Plan, Backlog, and
  Checkpoint Templates.
- Repository dogfood: `.agents/workflows/planning.md`,
  `.agents/patterns/work-records.md`, and `.agents/templates/planning/`.
- Local execution records: Working Task routes, phase subfolders, the project
  control ledger where a project has one, and Checkpoints/Handoffs when their
  resumption or transfer boundary is real.
- Historical records: Archived Memory grouped by workstream or origin. A task
  being complete and a record being archived are related but different states.

The first review output is a current-source map showing which surface owns
outcome, sequence, queue, current state, completion evidence, recently
completed visibility, and historical retention. No source is changed merely
because another source links to it.

The Planning Extension, its templates, repository dogfood, source documents,
and entrypoints are read-only inputs for this task. Any accepted implementation
must be created as a separately scoped follow-up and explicitly authorized.

## Local task lifecycle and completion contract

The following is the recommended model to validate and either accept or
reword; it is not yet a new universal Framework rule:

- **Candidate** means possible work awaiting selection. It belongs in a
  Backlog or candidate route and is not an executable commitment.
- **Planned** means an accepted task exists but execution has not started.
- **Active** means the task is selected and its current phase/next action is
  known. The Task record owns that current state.
- **Blocked** means progress cannot continue without a named decision,
  dependency, capability, or safe external-state change. It is not complete.
- **Complete** means the accepted outcome passed its required evidence and
  acceptance boundary. It is the state that should be rendered as “done.”
- **Superseded** means the outcome was replaced or withdrawn before completion;
  the replacement or reason must be linked.
- **Archived** is a retention/location state after active use ends, not a
  substitute for recording whether the outcome completed.

The task must decide whether this vocabulary is the smallest useful local
contract and how it maps to any external tracker. It must not duplicate the
project-control ledger's permanent ID/name, queue, or completion-grace state.

### What marks a task done

A completion procedure must require, at minimum:

1. The owning Task records `Complete`, the accepted outcome, final phase and
   milestone state where that project convention applies, residual risk, and
   remaining work or `None`.
2. The Task links the decisive acceptance evidence, changed durable sources,
   artifacts/commits or other receipts, and the disposition of blocking and
   non-blocking findings. A status tag alone is never completion evidence.
3. The project-control ledger, when present, is updated for queue state and
   recently-completed visibility. Its projection must link to the Task and
   must not invent a second outcome or progress history.
4. The active Checkpoint loses `#Active` and `#KeepInMind` when its active
   need ends; it is archived or pruned according to Working Memory rules.
   A Handoff is sealed only for a real transfer or planned resumption.
5. The completed Task is removed from the active selection route when the
   local closeout rule permits it and retained under an organized Archived
   route when its provenance has future value. Reopening or follow-up work
   keeps the original identity and records a new explicit horizon rather than
   rewriting the old completion.

The task must define the exact order, generated-index updates, and link target
for the repository's `Recently completed` grace period. The portable Extension
must describe the completion meaning without assuming this repository's
numeric IDs, ledger, or two-update grace rule.

### Organization rules to settle

- One Task source owns outcome, current phase, completion evidence, findings,
  and residual risk. A Plan owns an independently maintained sequence only
  when an inline plan is insufficient; a Checkpoint owns resumability only;
  a Handoff owns a sealed transfer snapshot only.
- A Backlog or candidate route lists work awaiting selection and links to an
  existing Task once one exists. It does not copy the Task's changing status.
- Active Tasks and their phases remain in the current Working route. Completed
  and superseded history moves to an Archived route organized by workstream,
  origin, or another discoverable scope, with explicit replacement links.
- Generated `Entries` are navigation, not status. The authoritative status
  line and completion section remain in the Task or the project ledger that
  owns that fact.
- A separate `done` folder or completed-task Template is justified only if
  the Task-plus-ledger-plus-Archive relationship cannot make completion and
  discovery clear. Avoid a second copy of finished work.

## Named Rules Under Review

### 1. Narrowest-Source Ownership and Summary-Only Hierarchy Rule

Current wording:

> Put detail in the narrowest source that owns the question. Higher-level entrypoints and records keep only the summary needed to select that source and link to it; they do not duplicate lower-level requirements.

Source: the workspace and packaged [Open Forge loaders](../../loader.md).

Review whether this should define more precisely what “owns the question,” how
much summary is sufficient, how links differ from authority, and how the rule
handles evidence, current truth, generated indexes, and intentional exceptions.
The likely result is a stronger rewording or a narrower scoped rule.

### 2. Analysis-to-Task Termination and Sealed-Provenance Rule

Current wording:

> Let Analysis terminate in an actionable Task. Once its conclusion moves into the Task, treat the Analysis as sealed provenance; the Task and its subtasks own current implementation discoveries and execution evidence.

Sources: the workspace and packaged [Open Forge loaders](../../loader.md),
[Analysis contract](../emerging/analysis/_analysis.md), and the sealed CLI
Analysis routes.

Review whether Analysis should remain an ordinary Emerging Memory category or
whether Planning should become an Extension-provided category with explicit
rules for Analysis, Tasks, Plans, subtasks, evidence, sealing, and execution.
Decide whether any Analysis material should move out of normal Memory, what
would replace it, and how existing records would migrate without competing
authority.

This review now also covers the boundary between an optional Planning
Extension and this repository's Local Planning convention. The portable
package must remain useful without importing repository-specific orchestration
state, while the local convention may add explicit task IDs, queue projections,
phase/milestone status, and archive organization where the project accepts
them.

### 3. Meaning-Preserving Extraction and Archival Rule

Current authority already exists in [Archived Memory](../crystallized/documents/framework/memory/archived.md)
and [Memory Transitions](../crystallized/documents/framework/memory/transitions.md#replacement-and-archival):
extract useful current meaning first, preserve origin and replacement, remove
metadata that asserts former authority, and retain, consolidate, reduce, or
transform the remainder according to future value.

Review whether this wording clearly means lossless preservation of useful
meaning rather than byte-for-byte preservation: accepted requirements,
conditions, exceptions, limits, rationale, provenance, and unresolved risk must
not be lost merely because the record is split or transformed. Determine which
detail may be safely removed and what evidence proves that extraction was
complete. Use the [CLI authority audit](../archived/cli-development/analysis/2026-09-02_cli-architecture-authority-audit.md)
as prior analysis; no active archival review task existed.

### 4. Authors-Findings Retention Rule

This rule is **accepted and not a revision target**. The workspace-only
[Authors' Findings](../emerging/authors-findings/_authors-findings.md) route is
retained by default and must not be archived, deleted, consolidated,
overwritten, or pruned without explicit maintainer direction. Recording a note
does not promote it to a Directive, Decision, CurrentTruth, or Task.

Verify only that this rule remains separate from ordinary findings, is not
shipped as a source-package category, and is not accidentally changed by the
other review outcomes.

### 5. Planning Extension and Template Boundary

The current [Planning Extension](../../../src/extensions/planning/README.md)
contains the Planning Workflow, Work Records Pattern, and four planning
Templates. The repository dogfoods corresponding files under `.agents/`.

Review whether the package and dogfood copies have one clear source of truth,
whether each Template answers a distinct starting question, and whether the
Extension explains enough of the relationship between Task, Plan, Backlog,
Checkpoint, Handoff, and Archived history without importing local policy.
Decide which improvements belong in the portable package, which belong only in
Local Planning, and what parity or installation evidence protects that split.

### 6. Task Completion, Organization, and Closeout Rule

Define the local procedure for creating, selecting, progressing, blocking,
completing, superseding, reopening, and archiving work. In particular, decide
how a task is marked done, which source owns each status fact, how completed
tasks remain discoverable, how `Recently completed` is tracked and dequeued,
and how a new follow-up horizon relates to the original task.

The result must distinguish an accepted completed outcome from an archived
record, preserve evidence and residual risk, prevent duplicate active/history
copies, and keep generated `Entries` as navigation rather than status. It must
also cover partial completion, blocked work, abandoned or rejected candidates,
and external task systems.

## Phase Map

1. Inventory the current Planning Extension, repository dogfood, task routes,
   project-control ledger, checkpoints, handoffs, and archive organization.
2. Establish authority and relationships for Task, Plan, Backlog, Checkpoint,
   Handoff, candidate, completed, superseded, and archived records.
3. Review and improve the portable planning Workflow, Work Records Pattern,
   and four Templates, including source/dogfood parity and installation shape.
4. Define the Local Planning lifecycle: status vocabulary, task organization,
   completion evidence, closeout, `Recently completed`, and follow-up horizons.
5. Produce accepted dispositions, migration examples, verification receipts,
   and separately scoped implementation work.

## Review Lanes

1. Compare the loader rules with current Memory authority, routing, and loading
   rules. Identify overlap, ambiguity, missing exceptions, and the strongest
   wording that remains implementable.
2. Compare Analysis, Task, Plan, subtask, Backlog, Checkpoint, Handoff, and
   Archive responsibilities. Do not create or remove a Planning category until
   its authority and migration are accepted.
3. Compare the portable `planning` Extension with repository-local planning.
   Check the Workflow, Work Records Pattern, four Templates, source/dogfood
   parity, package boundaries, and install/update behavior.
4. Define the task state and organization model. Make status ownership,
   permanent identity, queue projection, phase/milestone progress, candidate
   selection, and active/history placement explicit.
5. Define the done procedure. Specify the required outcome, evidence, finding
   disposition, residual risk, durable-source reconciliation, ledger update,
   checkpoint closeout, archive/recent-completion handling, and reopening rule.
6. Validate the archival contract against semantic-lossless extraction and the
   completed CLI cleanup. Check provenance, link updates, tag removal,
   transformations, and deletion authority.
7. Produce one disposition per named rule and one implementation boundary per
   accepted change: **retain**, **reword**, **narrow**, **move**, **remove**, or
   **defer**, with migration and verification evidence.

## Boundaries

- Do not edit the loaders, Memory taxonomy, Planning Extension, planning
  Templates, archival contract, or existing Analysis records until this task's
  recommendation is accepted.
- Do not treat the Authors-Findings rule as an ordinary candidate for removal.
- Do not rewrite sealed Analysis to make it agree with an evaluation result;
  record any new conclusion in this Task or a new Analysis.
- Do not change CLI source, tests, public contracts, or runtime behavior.
- Do not make the repository's permanent task IDs, completion-grace counters,
  project-control ledger, or orchestration display rules mandatory in the
  portable Planning Extension without a separate acceptance decision.

## Acceptance

This task is complete only when:

- all six named rules have an explicit disposition;
- the first rule has a proposed stronger wording or a justified decision to
  retain it;
- the second rule has a decision on Analysis placement and whether Planning
  needs its own Extension/category rules;
- the existing archival wording is either accepted as sufficient or replaced
  with an accepted meaning-preserving formulation;
- Authors-Findings remains a separate workspace-only category with its current
  retention rule;
- the Planning Extension, Work Records Pattern, and Task, Plan, Backlog, and
  Checkpoint Templates each have a clear source/dogfood disposition,
  installation/update boundary, and parity verification plan;
- Local Planning has an accepted state/transition matrix that distinguishes
  candidate, planned, active, blocked, complete, superseded, and archived
  work, or records a smaller justified vocabulary;
- the task-closeout procedure states exactly how to mark work done, capture
  outcome/evidence/residual risk, update the project-control ledger when one
  exists, close Checkpoints, preserve Handoffs, expose `Recently completed`,
  dequeue it, and archive or prune the task;
- completed, blocked, superseded, reopened, and follow-up examples show where
  each record lives and which source owns its changing status;
- any implementation work is split into separately accepted follow-up tasks;
- links, route metadata, and migration evidence are recorded in the files that
  own them.
