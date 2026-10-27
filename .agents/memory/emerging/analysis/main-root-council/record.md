---
open-forge:
  description: What the main root council decided about delivering a configurable context folder in two parts, the layout seam, settings v4, the install question and the conditions for moving existing workspaces
  responsibility: Record council 6's question, method, rankings, chair decisions and declined ideas
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Council, Layout, Install, Settings]
---

# Main Root Council

This is council 6. It refreshes the configurable main root that the [route include council](../route-include-council/record.md#the-configurable-main-root-7) accepted, for [Task 85](../../../working/cli-development/tasks/task85-configurable-main-root.md). The [implementation plan](plan.md) turns the decisions into work packets.

## What the maintainer asked

On 2026-10-22 the main root was deferred because `route include` already covered reusing project documents. On 2026-10-23 the maintainer wrote "Let's do the multy root as well", to follow the lock work in [Task 86](../../../archived/cli-development/tasks/task86-workspace-runtime-lock.md), with "a council of astra xhigh for this task" and "astras high as implementers".

## Method

Six read-only Astra xhigh members worked from commit `d024f5087`, which holds the frozen Task 86 plan: the CLI architect, a contrarian minimalist, a migration and safety engineer, a contract and compatibility guardian, a user and agent experience designer and a test and qualification engineer. Each checked the 2026-10-21 plan against the code and answered seven questions. In Stage 2 each member checked the others' claims and ranked all six proposals in five areas. The chair decided, and the test and qualification engineer, which won PLAN unanimously, wrote the frozen plan.

## Rankings

Average rank from six reviewers, lower is better. ACCURACY covers what the old plan gets wrong, DESIGN the scope, layout seam and settings, QUESTIONS the open questions, EXPERIENCE the install question, help and pointers, PLAN the implementation plan.

| Proposal | ACCURACY | DESIGN | QUESTIONS | EXPERIENCE | PLAN |
| --- | ---: | ---: | ---: | ---: | ---: |
| P1 User and agent experience designer | 4.50 | 4.83 | 5.50 | **1.00** | 5.00 |
| P2 Contrarian minimalist | 2.00 | **1.00** | 5.50 | 4.00 | 3.00 |
| P3 Test and qualification engineer | 4.67 | 4.00 | 3.67 | 4.17 | **1.00** |
| P4 CLI architect | 5.83 | 2.00 | 2.50 | 5.17 | 2.17 |
| P5 Contract and compatibility guardian | 2.33 | 5.17 | 2.83 | 3.33 | 5.50 |
| P6 Migration and safety engineer | **1.67** | 4.00 | **1.00** | 3.33 | 4.33 |

The [proposals](proposals.md) and [reviews](reviews.md) are kept as evidence.

## What the council found

- **The old plan is partly stale.** Its prerequisite is done, the authoring guide it maps does not exist, and Route Include is now real code that assumes the category is the second path segment. The 334-file census counts audit hits, not edits.
- **Package paths and installed paths are tangled.** Payload validation and frontmatter rendering assume destinations start with `.agents/`, so mapping destinations alone would install a custom root wrongly.
- **A settings-reading resolver in Workspace would reverse a documented dependency.** The pure layout value belongs below Sources and its settings-backed reader with Settings.
- **Moving an existing workspace is not safe yet.** The old plan copies first and protects ignore rules afterward, does not define which file metadata survives, and relies on an Install recovery filter that treats any third state as history. A Library mapping can also become unrepresentable even when the whole registration moves.

## Decisions

1. **Two deliveries, Task 85 open between them.** Delivery 1 is a custom context folder chosen at first installation, absent or empty, with every command supporting it. Default-root adoption keeps working. An established installation refuses a different root before any effect. The option and question appear only when every command family supports the layout. Delivery 2, moving an existing workspace, starts after Delivery 1 ships and its safety conditions are planned and tested.
2. **One pure `WorkspaceLayout`,** with a settings-backed `WorkspaceLayoutReader` under Settings, resolved once per operation, carried through catalogues, sessions and plans, and revalidated under the lease. Its two content locations are the main root and native Skills, which stay at `.agents/skills`. Settings, ownership, `AGENTS.md`, `CLAUDE.md` and `.open-forge-state/` stay fixed. Literal paths, package coordinates and short IDs stay distinct.
3. **Settings v4** adds an optional `mainRoot`, absent meaning `.agents`. New settings files declare v4, existing declarations are preserved, and the default value is never inserted. Invalid, reserved or unsafe values are rejected. A hand edit moves nothing: read commands report the disagreement and writing commands stop. Older CLIs are unsupported for custom roots, and the docs say so. JSON output stays schema 3.
4. **The default stays byte-for-byte unchanged** against the Task 86 baseline, except new v4 settings declarations and the intended help, option and wizard changes. Direct byte comparisons prove it, because the snapshot comparer ignores line endings.
5. **The wizard asks "Where should Open Forge keep its context files?"** with `.agents` as the default, before route choices, explaining that settings and Skills stay in `.agents`. `--main-root <folder>` answers it, and previews, JSON and unattended runs never ask. `AGENTS.md` and `CLAUDE.md` point at the selected loader, `CLAUDE.md` keeping its `@AGENTS.md` bridge. Status names the context folder for a custom root. The shipped CLI Skill and README stop assuming every exact path starts with `.agents`.
6. **Delivery 1 open questions.** Recovery comes from Task 86 unchanged. Extensions map package coordinates to installed destinations before exclusions, permissions, collisions and ownership, and record where they installed. Libraries keep literal sources and registrations. Help keeps reading formats from the embedded README.
7. **Before Delivery 2:** a real inventory including ignored and untracked files, ignore protection of both locations for the whole move, a stated metadata contract, destination verification before deleting originals, a tested restoration that never overwrites later edits, and refusal when Library sources intersect the moved tree or a mapping stops being representable.
8. **Estimate:** 14 to 18 engineering days for Delivery 1, then 7 to 10 for Delivery 2.

## Declined

- A mount system or configurable locations beyond the main root.
- Writing default settings as v3 to keep older CLIs working. A new file declares v4.
- A minimum-version field that older CLIs would ignore anyway.
- Pinned Library islands, per-file mappings or a new ownership schema for moves.
- Adopting a populated custom destination at first install. `route include` covers existing documents.
