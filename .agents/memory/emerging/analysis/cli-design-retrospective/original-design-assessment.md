---
open-forge:
  description: Which painpoints trace to the original Framework design rather than to the contracts or the implementation, judged against how the workspace was actually used
  tags: [Memory, Analysis, Contextual, Candidate, Framework, Design, Retrospective, Routing, Memory, Loading]
---

# Original Design Assessment

## Scope

Decisions made in the Framework itself, before the replacement CLI existed —
`70df4545` and `a5dddf16`, 2026-07-30, against the CLI reset at `768bd51a`,
2026-08-17. These are not implementation defects and not contract-writing
defects. They are design choices that the CLI faithfully encoded, and that using
the system has now tested.

Judged against evidence from the only large real workspace, this repository, and
from driving the CLI across ~60 invocations in nine workspace states.

## Decisions that held up

Recording these first, because the failures below are corrections to a design
that is mostly working.

- **Markdown-native, user-owned, file-first.** Every artifact remains readable
  and diffable without the tool. This is the premise and it survived contact
  with a 500-file workspace.
- **`crystallized` / `emerging` / `archived`.** 325 files across them, healthy
  distribution, and the acceptance semantics (`#CurrentTruth` versus
  `#Contextual`) are used and meaningful.
- **Descriptions as the routing signal.** An entry line carrying a one-sentence
  description is the right primitive for selection.
- **`SKILL.md` as its own authority.** `SourceIdentity` giving a skill its
  folder's ID is elegant, and the design intent — native formats keep their own
  metadata — is correct. The implementation violates it; the design does not.
- **The separation of loading (`#LoadNow`) from classification (`#Core`,
  `#Memory`).** Right idea, wrongly defaulted (below).

## Decisions that did not hold up

### 1. Generated `Entries` as stored content

The single most consequential original decision. Every entrypoint carries a
generated list of its children, written into the file, delimited by HTML comment
markers.

What it produced, all measured elsewhere in the audit:

- The `## Entries`-must-be-final rule, so one appended sentence invalidates a
  file and blocks the workspace.
- The generated region needing exclusion from fingerprints, which produced the
  second baseline, which is the ship-blocker that makes extensions
  uninstallable.
- `reference.cycle` × 1,817 — a parent lists its child and the child links back,
  so the mandated structure is reported as a finding.
- `index` existing at all, as a command a user must run to keep files legal.
- The marker syntax, which `route init` renders as escaped `<` in human
  output.

The alternative was available from the start: **compute the child list on read.**
The CLI has a full CommonMark parser and already locates `## Axioms` by heading
with no markers. Nothing needs to be stored.

The counter-argument is that stored Entries make the files useful without the
CLI — an agent reading `_memory.md` sees its children. That is real, and it is
why the decision was made. But the cost has been an entire class of failure, a
command, a fingerprint policy, and a ship-blocker.

**Assessment: correct goal, wrong mechanism.** Keep the guarantee that a file
shows its children; drop the requirement that the list be _stored and
integrity-checked_. Regenerate on read, write on demand, and never treat a stale
list as a defect.

### 2. `## Axioms` as required structure

A required, non-empty section on every entrypoint. Measured: it does not block
commands, but it is a permanent `doctor` warning on every custom scope, and
`route init` must emit a placeholder bullet to pass its own check.

The intent was that scopes carry their own rules. In practice, in this
repository, most scopes have no local rules and the framework's own scaffolder
writes _"inherited - No local axioms"_ to satisfy the requirement.

**Assessment: a structure was made mandatory to guarantee a behaviour that is
usually absent.** Inheritance should be the default and silent; a section should
appear only when a scope actually adds rules. This generalises: **a required
section that is usually empty is a design smell**, and the same applies to the
`Entries` section on leaf scopes.

### 3. The memory taxonomy is by artifact type, not by work

`working` holds 140 files. Its two shipped subcategories hold **1**
(`checkpoints`) and **13** (`handoffs`). The remaining ~126 are in
`working/cli-development/` and `working/framework-review/` — folders that had to
be invented, twice, each containing a plan, a control record, and a task set.

**Assessment: the taxonomy models artifacts; work is organised by workstream.**
A checkpoint is a _state of_ something, not a _kind of_ thing, and giving it a
parallel branch guaranteed it would be under-used. `crystallized` and `archived`
work because they classify by _acceptance status_, which is a genuine property of
a document. `checkpoints` and `handoffs` classify by document shape, which is not.

### 4. Four adjacent categories for "how to do things"

`directives` (required), `guidance` (advice), `patterns` (default shapes),
`workflows` (recipes). Usage here: 15, 5, 12, 21 files.

The distinctions are real — `directives` is required and `guidance` is advice,
which carries loading semantics. But the placement decision for any given
document is genuinely hard, a wrong placement is invisible, never surfaced, and
never corrected, and the Framework defines a `responsibility` line intended
precisely to answer "what belongs here" that **nothing uses**.

**Assessment: the categories are defensible; the absence of any placement aid is
not.** Four categories with a `responsibility` line surfaced by `route inspect`
would be fine. Four categories with no aid is a coin flip repeated forever.

### 5. `#LoadNow` decided by the child, statically

The tag lives in the child's frontmatter, so the child decides for every parent
and for every task. There is no way to say "load `patterns` when implementing,
not when reviewing".

Worse, the shipped defaults contradict the shipped axiom: the Loader says _"Keep
entries on demand by default"_ and **15 of 19 entrypoints, and 7 of 8 root
routes, are `#LoadNow`** — a bare install is 81% startup share with zero user
content. The maintainer's own authored content follows the axiom (26 of 138,
19%); only the shipped payload violates it.

**Assessment: the mechanism is sound, the defaults inverted the lesson.** A
framework teaches by its payload far more than by its prose. Also missing: no
cost is shown at the moment the tag is chosen, and no budget exists — so nothing
ever pushes back.

### 6. Eight root routes, two of them hollow

`maps` holds **1 file** here and `skills` holds **0** — and both are `#LoadNow`.
`templates`, with the most files (25), is the only on-demand root, and in a bare
install it ships **empty**.

**Assessment: the route set advertises capacity the payload does not fill.**
`maps`, in particular, duplicates what every entrypoint's `Entries` descriptions
already provide. Better to ship fewer routes fully populated than eight with two
hollow — and a `#LoadNow` scope with zero entries should be a finding.

### 7. Nothing makes a handoff self-sufficient

A task record produced from the shipped template has Outcome / Plan / Current
State and **no instruction to read the Loader**. So an implementer handed a task
file — or a subagent spawned with a task excerpt — correctly treats it as a
complete work order and skips Open Forge entirely.

**Assessment: the Framework assumed ambient context that only exists when a
harness reads root `AGENTS.md`.** For a system whose premise is agent
continuity, the artifact designed for transfer is the one artifact that does not
carry its own entry instruction. This is a Framework-level omission, not a
template bug.

### 8. The Framework never stated an output budget

`progressive disclosure` appears **nowhere** in the CLI contract set. The word
`token` appears only as a lexer term. The Framework's entire premise is context
economy, and it was never written down as a requirement anything could be
checked against.

**Assessment: the most important non-functional requirement was left implicit.**
Everything downstream — the exhaustive interface contracts, the faithful
renderers, the 8.8 MB `doctor` — followed from a premise that existed in the
maintainer's head and in the framework's marketing, but in no contract.

## What generalises

Four patterns, each visible in more than one decision above.

- **A required structure that is usually empty is a smell.** `## Axioms` on
  scopes with no local rules; `## Entries` on leaves with no children.
  Requirements should attach to the case that carries information.
- **Storing what can be derived buys convenience and sells integrity.**
  Generated `Entries` produced a command, a fingerprint policy, a finding class,
  and a ship-blocker.
- **Classify by properties documents actually have.** Acceptance status
  (`crystallized`/`emerging`/`archived`) works. Document shape
  (`checkpoints`/`handoffs`) does not.
- **The payload is the teaching.** 7 of 8 root routes shipped `#LoadNow` taught
  every model to over-load, against an axiom in the same file saying the
  opposite.

## Relationship to the audit

Layer-3 defects — the crash, the strict `SKILL.md` keys, `references` returning
zero — are in the
[remediation task](../../../working/cli-development/tasks/task30-cli-experience-remediation.md) and should
be fixed there regardless of what is decided here.

The decisions above are different: fixing them changes the Framework, not the
CLI. Several audit tasks are downstream of them and would become unnecessary —
removing stored `Entries` would delete most of G2 and part of G1. That
dependency is worth resolving before the backlog is scheduled.
