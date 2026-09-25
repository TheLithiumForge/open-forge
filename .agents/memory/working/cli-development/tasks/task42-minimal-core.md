---
open-forge:
  description: Open Task 42 to shrink the installed core to what every workspace needs, moving deeper routes into Extensions a user opts into
  tags: [Memory, Working, CLI, Task, Core, Extensions, Routing, Beta, Contextual, Active]
---

# Task 42 — Minimal core

## Task state

- State: **Extraction and documentation aligned; historical upgrade evidence remains unqualified.** Raised by the maintainer on 2026-09-17 as a beta
  priority, because it changes what a user receives on `install`.
- Owner: Root.

The 2026-09-21 [documentation packet](../../../archived/cli-development/tasks/beta-follow-ups/task42-43-documentation.md) maps the seven extracted Memory roles to their current Extensions. The earlier decision record below is retained; do not repeat the payload move or infer that historical upgrade requirements are verified.

## The decision

**Every user receives the minimal core. Anything beyond that arrives as an
Extension they choose.** A route that is only useful to some workspaces does not
belong in what everyone installs.

The maintainer's example: `ideas`, `observations`, `documents` and `decisions`
are third-degree routes most workspaces will never touch. They most likely
belong in the planning Extension, or in a smaller Extension that planning
depends on.

**That grouping is the open question.** One planning Extension is simpler; a
smaller shared Extension that planning depends on is more composable and matches
how Extension dependencies already work. Decide it deliberately rather than by
what is easiest to move.

## Why it is a beta priority

It changes the shape of a fresh install, which is the first thing every beta
user sees, and it changes what `context` loads at startup, so it moves both the
first impression and the token cost of every session. Doing it after beta means
changing that shape under people who have already installed.

## Boundary

- Decide the grouping before moving anything.
- A removed default stays removed. Moving a route into an Extension must not
  break a workspace that already uses it; the upgrade path is part of the work.
- The routing model already supports this. Nothing here needs a new mechanism.

## Acceptance

- The core after this change is defensible route by route: each is there because
  every workspace needs it.
- Each moved route lives in a named Extension with its dependencies recorded.
- A fresh install is measurably smaller, in files and in startup tokens. Record
  both before and after.
- The install journeys in [Task 41](task41-beta-journey-scenarios.md) still pass.

## Outstanding fallout from the first pass

The reduced core landed on 2026-09-18. Eight payload maintenance contracts under
`.agents/memory/crystallized/documents/maintenance/payload/agents/` still
describe files Core no longer ships, and their source links are broken on
purpose rather than repointed — a broken link is honest signal that the contract
is stale, and repointing it would hide that.

| Contract                           | What changed under it                                                   |
| ---------------------------------- | ----------------------------------------------------------------------- |
| `memory/crystallized/decisions.md` | Now owned by `planning`; classification is `#Extension`, not `#LoadNow` |
| `memory/crystallized/documents.md` | Now owned by `project-documents`; same classification change            |
| `memory/emerging/analysis.md`      | Now owned by `planning`; same classification change                     |
| `memory/emerging/ideas.md`         | Now owned by `planning`; same classification change                     |
| `memory/emerging/observations.md`  | Now owned by `orchestration`; same classification change                |
| `memory/working/checkpoints.md`    | Now owned by `planning`; same classification change                     |
| `memory/working/handoffs.md`       | Now owned by `orchestration`; same classification change                |
| `workflows.md`                     | The payload route is retired outright; there is no current source       |

Each `## Contract` section still asserts the old `#LoadNow` classification, so
reconciling them is a content decision, not a link fix. Settle it here, because
this Task decided which files survive.

## Related

[Task 44](task44-template-content.md) covers what the surviving core files say,
which only matters once this decides which files survive.
[Task 43](task43-workflows-as-skill.md) decides the fate of the workflows route,
so the two should be settled together.
