---
open-forge:
  description: Open Task 47 to make index and route navigation reach every recognized entrypoint form from the loader, so a catalogue does not need to be named by hand to stay current
  tags: [Memory, Working, CLI, Task, Routing, Navigation, Index, Beta, Contextual, Active]
---

# Task 47 — Entrypoint reachability

## Task state

- State: **Targeted Doctor corrections pass the published journey; broader indexing choice remains open.** Raised by the maintainer on 2026-09-18.
- Owner: Root.

The 2026-09-21 [navigation packet](beta-follow-ups/task46-47-navigation.md) reproduces a stale detached Skill catalogue after default Index and successful repair through explicit selection. The implemented correction provides truthful targeted advice and correct missing-entry identity; automatic traversal remains a separate choice.

- Shares a root cause with [Task 46](task46-routed-skill-resources.md). Settle
  them together; 46 decides whether a Skill can host a route, 47 decides what
  navigation does once it can.

## The problem

`open-forge index` with no argument rebuilds Entries from the loader roots. That
is correct and cheap. The question this task has to answer is what counts as
reachable, because right now a workspace can hold a current, well-formed
catalogue that the loader never sees.

Measured on 2026-09-18 in a scratch workspace with every Extension installed:

```sh
open-forge index                                                   # 21 regions, none under the Skill
open-forge index .agents/skills/use-workflow/references/_references.md   # 4 regions, all correct
```

Both succeed. They just do not agree on what exists. A user who runs the plain
form after adding a recipe gets "Entries sections are current" and a stale
catalogue, with nothing saying the two are different claims.

## The entrypoint forms are not the problem

Worth stating, because it is the obvious suspicion and it is wrong. The loader
recognizes `_{folder-name}.md` and the compatibility names `index.md`,
`_index.md`, `references.md` and `_references.md`. Tested on 2026-09-18: a folder
using `index.md` under a routed parent indexes correctly and picks up its child
on the first run.

```text
.agents/patterns/compat/index.md  0 -> 1 entries
```

So the forms work. What fails is the **host**: nothing below
`.agents/skills/<name>/` is reachable, because `SKILL.md` is not an entrypoint
and the chain stops there. Any fix aimed at the naming would miss it.

## What to settle

- When a route exists but no loaded parent reaches it, is that a `doctor`
  warning, an `index` responsibility, or both? Today it is a warning that
  `index` cannot act on, which is the worst of the three.
- Should the plain `index` form cover every routed source under `.agents/`
  rather than only the loader-reachable set, or should it stay narrow and say
  clearly what it did not cover? Either is defensible. Silence is not.
- The suggested next action on those findings is `open-forge index`, and running
  it changes nothing. Whatever this task decides, that suggestion has to become
  true or stop being printed.
- `doctor` distinguishes "looks like a route but no Loader entry or parent
  reaches it" from "is routed but no parent lists it" and offers `Fix it by
hand` for the first. Check that split still earns its keep once reachability
  is decided.

## Boundaries

Do not widen the default selection just to silence the warnings. The narrow
default exists because a full `.agents/` sweep is expensive in a large
workspace — this repository has over seven hundred Markdown files, and
[Task 30](task30-cli-experience-remediation.md) already carries the cost
constraints. Decide the model first, then make the output honest about it.
