---
open-forge:
  description: Open Task 47 to make index and route navigation reach every recognized entrypoint form from the loader, so a catalogue does not need to be named by hand to stay current
  tags: [Memory, Working, CLI, Task, Routing, Navigation, Index, Beta, Contextual, Active]
---

# Task 47 — Entrypoint reachability

**Reviewed on 2026-09-28:** [review](../../../emerging/analysis/open-task-review/task47-entrypoint-reachability.md). Recommendation:
Do before 1.0. The review names any details in this record that are out of date.

## Task state

- State: **Task 47 “Entrypoint reachability” (phase 3/3): milestone 4/4.** Independently complete and ready for Root integration on the beta4 base. Full managed, supported-host win-x64 NativeAOT, package, and installed Planning gates passed for the exact reconstructed candidate.
- Completion owner: a_1bec444bebb4 (r_54dbdc5b958b), GPT-6.1 Sol/high. Root owns integration and combined wave acceptance.

See the [Task 47 qualification and review receipt](task47-default-skill-indexing.md#qualification-and-review-receipt-2026-10-01) and its [Root qualification addendum](task47-default-skill-indexing.md#root-qualification-addendum-2026-10-01). The [independent completion receipt](task47-default-skill-indexing.md#independent-completion-receipt-2026-10-02) records exact inventory, qualification, and merge readiness. Root owns squash integration, combined wave acceptance, and release qualification.

- Shares a root cause with [Task 46](../../../archived/cli-development/tasks/task46-routed-skill-resources.md). Settle
  them together; 46 decides whether a Skill can host a route, 47 decides what
  navigation does once it can.

## The problem

`open-forge index` with no argument rebuilds Entries from the loader roots. That
is correct and cheap. The original question was what counts as reachable: a
workspace could hold a current, well-formed catalogue beneath a native Skill that default Index never
selected. The frozen command-local bridge now reaches the accepted immediate
catalogues of rooted native Skills while preserving the boundaries below.

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

The forms worked in that baseline. The missing bridge was at the **host**:
default selection stopped at `SKILL.md`, which is not a catalogue entrypoint.
The accepted Index bridge now discovers immediate catalogue entrypoints below
eligible rooted native Skills and follows their normal closures. Naming and
shared runtime activation remain unchanged.

## Earlier questions (2026-09-18)

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

The targeted-advice defect above is now fixed. The new follow-up owns the
remaining default-traversal design and supersedes the earlier open choice about
whether to pursue it.

## Boundaries

Do not widen the default selection just to silence the warnings. The narrow
default exists because a full `.agents/` sweep is expensive in a large
workspace — this repository has over seven hundred Markdown files, and
[Task 30](../../../archived/cli-development/tasks/task30-cli-experience-remediation.md) already carries the cost
constraints. Decide the model first, then make the output honest about it.
