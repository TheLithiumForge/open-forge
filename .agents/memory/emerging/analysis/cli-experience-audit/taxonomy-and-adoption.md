---
open-forge:
  description: Whether the shipped memory categories and scoping model match how the workspace is actually used, why implementers skip the framework after planning, and how much should ship by default
  tags: [Memory, Analysis, Contextual, Candidate, Framework, Taxonomy, Memory, Scope, Adoption, Extension]
---

# Taxonomy And Adoption

## Conclusion

Measured against the only large real workspace — this repository — the shipped
taxonomy is **half right**. `crystallized` and `archived` earn their keep.
`working`'s shipped subcategories are nearly dead, and the structure that
actually emerged is one the framework does not model at all.

Separately, two of eight root routes are empty or near-empty here and both are
`#LoadNow`, and the reason an implementer skips Open Forge after planning is
structural: **no planning artifact carries an instruction to load it.**

## How the memory categories are actually used

Every non-entrypoint Markdown file under `.agents/memory` in this repository:

| Category                 | Files   | Verdict                |
| ------------------------ | ------- | ---------------------- |
| `working` (total)        | **140** | heavily used           |
| ├ `working/checkpoints`  | **1**   | effectively unused     |
| ├ `working/handoffs`     | 13      | lightly used           |
| └ ad-hoc subfolders      | ~126    | **the real structure** |
| `crystallized/documents` | 132     | heavily used           |
| `archived`               | 122     | heavily used           |
| `emerging/ideas`         | 24      | used                   |
| `crystallized/decisions` | 21      | used                   |
| `emerging/analysis`      | 14      | used                   |
| `emerging/observations`  | 12      | used                   |

The shipped answer for active work is `checkpoints` and `handoffs`: 14 files
between them. The structure that emerged instead:

```
.agents/memory/working/cli-development/
  _cli-development.md
  plan.md
  project-control.md
  overseer-memory.md
  edge-cases.md
  tasks/            ← 100 files
```

**The shipped taxonomy organises by artifact type; real use organises by
workstream.** `checkpoints` and `handoffs` are artifact types. `cli-development`
is a workstream that _contains_ a plan, a control record, a task set, and its own
notes. A second one, `framework-review`, has the same shape.

That is not a user error. It is the natural shape of long-running work, and the
framework offers no first-class place for it, so it had to be invented twice.

### Proposed result

- Make **workstream** a first-class shape under `working`: a scope with a plan,
  a current state, and tasks. Ship a template for it. This is what
  `working/cli-development` and `working/framework-review` both reinvented.
- Demote `checkpoints` and `handoffs` from shipped root categories to templates
  _within_ a workstream. A checkpoint is a section or a file inside the
  workstream it belongs to, not a parallel taxonomy branch. 1 file in 140 says
  they do not deserve their own route.
- Keep `crystallized/{decisions,documents}`, `archived`, and `emerging/*`
  unchanged. 325 files across them is a clear earn.
- Reconsider whether `emerging` needs three subcategories. 50 files split
  24/14/12 is healthy but the _placement decision_ between an idea, an analysis
  and an observation is genuinely ambiguous — and a wrong placement is invisible
  and never corrected.

## The scoping model

Root route usage in this repository:

| Route        | Files | `#LoadNow` |
| ------------ | ----- | ---------- |
| `templates`  | 25    | no         |
| `workflows`  | 21    | yes        |
| `directives` | 15    | yes        |
| `patterns`   | 12    | yes        |
| `guidance`   | 5     | yes        |
| `maps`       | **1** | yes        |
| `skills`     | **0** | yes        |

Two problems.

**Empty scopes are still resident.** `maps` holds one file and `skills` holds
none, and both are `#LoadNow` — so every task in the flagship workspace pays for
two scopes that contain nothing. `templates`, with the most files, is the only
one that is on demand, and in a bare install it is _also_ empty
([lifecycle-baselines-and-architecture.md](lifecycle-baselines-and-architecture.md)).
The loading tags are close to inverted relative to actual content.

**Four categories mean "how to do things".** Their shipped descriptions:

- `directives` — _"Required instructions loaded through selected routes"_
- `guidance` — _"Advice for recurring choices, tradeoffs, and work situations"_
- `patterns` — _"Reusable default shapes for code, files, APIs, documents"_
- `workflows` — _"Repeatable Markdown recipes for reaching a defined goal"_

Required rules / optional advice / default shapes / step recipes. The
distinctions are real, but the placement decision for any given document is hard,
and the framework gives no way to check whether a placement was right. A
mis-placed document is not a finding, is never surfaced, and quietly changes what
loads — because `directives` is required and `guidance` is advice.

### Proposed result

- Judge scope loading by content, not by category identity. A `#LoadNow` scope
  with zero entries should be a `doctor` finding: _"`skills` is `#LoadNow` and
  empty — it costs nothing to read and nothing to skip."_
- Do **not** collapse the four instruction categories. The distinction between
  required and advisory carries real loading semantics. But add the placement aid
  the framework is missing: `route inspect` should answer _"what belongs here"_
  from the entrypoint's `responsibility` line, which the Loader already defines
  and nothing currently uses.
- Reconsider `maps` specifically. One file, `#LoadNow`, and its job — pointing at
  other sources — overlaps `Entries` descriptions, which every entrypoint already
  carries. It may be a pattern rather than a root route.

## Why implementers skip Open Forge after a plan

The mechanism is structural, not behavioural.

A task record produced from the shipped template
(`.agents/templates/planning/task.md`, from the `development-toolkit` Extension)
has three sections: **Outcome**, **Plan**, **Current State**. A real one from
this repository (`working/cli-development/tasks/00-cli-development.md`) is dense
project state — commits, phases, milestones, acceptance.

**Neither contains any instruction to read `.agents/loader.md`.**

The only thing that carries the framework's entry instruction is `AGENTS.md` /
`CLAUDE.md` at the repository root. That works when an agent harness reads root
context. It does **not** work when:

- an implementer is handed a task file path and starts from it,
- a subagent is spawned with a task excerpt as its prompt,
- a plan is pasted into a fresh session,
- the harness is one that does not auto-read `AGENTS.md`.

In all four cases the plan is a complete, self-contained work order that never
mentions Open Forge. Skipping it is the correct reading of what was handed over.

### Proposed result

- Every planning and task template gains a short **Context** section at the top,
  before Outcome:

  ```markdown
  ## Context

  This task runs inside an Open Forge workspace. Before starting, read
  `.agents/loader.md` and load the scopes relevant to this work.
  Relevant scopes for this task: {directives/csharp, patterns/cli}
  ```

  Naming the _relevant scopes_ is the important half. It makes the plan carry its
  own selection, which is also the task-shaped scoping that
  [loading-and-scope-discipline.md](loading-and-scope-discipline.md) identifies as
  missing.

- `route create` and `route init` should offer to populate that line from the
  tags on the parent scope.
- Treat "a handoff must be self-sufficient" as an axiom. A record that assumes
  its reader already loaded the framework is a broken handoff, and the framework
  already has a `handoffs` category that should say so.

## How much should ship by default

Currently the base install ships 21 files and **zero templates**; the
`development-toolkit` Extension ships 21 more across skills, templates, and
workflows. The catalogue offers 6 packages.

The tension is real: a full development experience across verticals is the goal,
but the first run should not present six packages and a taxonomy of eight root
routes to someone who has not written a line yet.

### Proposed result

Three tiers rather than a catalogue.

- **Core** — the loader, the eight root entrypoints, and **a good template for
  every one of them**. Nothing else. This is the smallest thing that is complete:
  a user can author any category correctly on day one, which is exactly what is
  missing today.
- **One recommended bundle** — offered by name at the end of install, not as a
  list to choose from:

  ```
  Installed the Open Forge Framework.

    Add the development workflows (planning, review, debugging)?
    open-forge extension install development-toolkit
  ```

- **The rest of the catalogue** — discoverable through `extension list`, not
  surfaced during install.

The failure mode to avoid is the current one, where `templates` and `skills` are
root routes, are `#LoadNow`, and are empty — the framework advertises capacity it
does not ship. Better to ship fewer routes fully populated than eight routes with
two of them hollow.

## Rules must be suppressible

Recorded from the accepted direction, and supported by every measurement in this
audit: `doctor` produces 8,378 INFO findings on this repository, of which 5,550
say a link is valid
([repository-dogfood-and-configuration.md](repository-dogfood-and-configuration.md)).

Suppression is not a workaround for that — those findings should not exist. But
the general capability is still needed, because which warnings matter is a
workspace decision. Examples that are legitimately noise in one workspace and
signal in another: `route.axioms-invalid`, `reference.external-unchecked`,
`route.metadata-required-missing` on a scope the user deliberately keeps loose.

### Proposed result

In the single config file, alongside thresholds:

```json
{
  "rules": {
    "route.axioms-invalid": "off",
    "reference.external-unchecked": "off",
    "route.metadata-required-missing": "warn",
    "reference.target-missing": "error"
  },
  "thresholds": {
    "startupTokens": { "warn": 8000, "error": 16000 },
    "outputLines": { "warn": 200 }
  }
}
```

Every finding code already exists and is stable
(`DoctorDefinitions.cs:63` and its neighbours), so the key space is free. A rule
set to `off` is not counted, not printed, and does not affect exit status.

`doctor` should report what it suppressed, once:

```
No problems found.  2 rules disabled in open-forge.json
```

## One configuration file

Also accepted direction, consolidating
[repository-dogfood-and-configuration.md](repository-dogfood-and-configuration.md)
and [lifecycle-baselines-and-architecture.md](lifecycle-baselines-and-architecture.md):

**One `open-forge.json`.** It holds the authored config — `rules`, `thresholds`,
`allow` — and the minimal install index — what was installed and what the user
removed. No per-file baselines, no `workspacePath`, no separate permissions or
libraries files.

```json
{
  "schemaVersion": 2,
  "rules": { "route.axioms-invalid": "off" },
  "thresholds": { "startupTokens": { "warn": 8000 } },
  "allow": ["docs/**"],
  "installed": {
    "framework": { "version": "0.1.0", "files": ["…"] },
    "extensions": { "development": { "version": "0.1.0", "files": ["…"] } },
    "removedByUser": [".agents/maps/_maps.md"]
  }
}
```

The generated half is a plain file list, so the whole document stays reviewable
in a diff — which was the original point.

## Fixing in place, without a flag

Accepted narrowing of the `index --fix` proposal in
[structural-requirements-and-markers.md](structural-requirements-and-markers.md).

No new flag. `index` already rewrites these files and already computes the facts
that identify the problem. When a fix is mechanically derivable and safe, it
applies it as part of the normal run and reports it:

```
Updated 2 generated Entries, and fixed 3 structural problems.

  .agents/guidance/team/_team.md      added the missing Entries section
  .agents/guidance/team/notes.md      quoted a description containing ':'
  .agents/skills/pdf/SKILL.md         left alone — 'license' is not a key we can drop
```

`--dry-run` previews the fixes like any other change. Anything not safely
derivable stays a reported finding and is never guessed at.

The principle: **do not add commands or flags for work the command is already
positioned to do.** A user who runs `index` because their Entries are stale
should not then have to run a second thing to make the first thing legal.

## Axioms default

Accepted direction, refining
[structural-requirements-and-markers.md](structural-requirements-and-markers.md).

Rather than deleting the requirement outright, treat an absent or empty `## Axioms`
as meaning **inherited** — the same thing `route init`'s placeholder says in
words. The reader returns `Valid` with an `Inherited` state, no finding is
produced, and the placeholder bullet stops being generated. A scope that wants
local axioms writes them; a scope that does not writes nothing and costs nothing.

This is a smaller change than removing the section from the model, and it keeps
`route inspect`'s existing "Applicable rules (Axioms) — Inherited from: …"
output correct.
