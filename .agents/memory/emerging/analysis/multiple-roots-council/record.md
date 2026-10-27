---
open-forge:
  description: What the multiple roots council and the maintainer decided about routes outside the main root, folder includes as maintained routes, their identity and boundaries, and the test rules for Task 85 and Task 87
  responsibility: Record council 7's question, method, rankings, the maintainer's direction, chair decisions and declined ideas
  tags: [Memory, Analysis, Contextual, Candidate, CLI, Council, Routing, Include, Settings]
---

# Multiple Roots Council

This is council 7. It decided how [Task 85](../../../working/cli-development/tasks/task85-configurable-main-root.md), the configurable main root, and [Task 87](../../../archived/cli-development/tasks/task87-routes-outside-main-root.md), routes outside the main root, fit together. The [Task 87 plan](plan.md) turns the decisions into work packets.

## What the maintainer asked

On 2026-10-24 the maintainer asked that Task 85 support "1 main root, but also allowing multiple roots as we discussed", perhaps by extending `route include`, and to run that through a council. The maintainer also set rules for all this work: no magic strings, no tests that check literal strings except through snapshots, and clean test boundaries.

After Stage 1 the maintainer settled the main question:

> If you include s folder, you basically create a route outside the main root ... by multiple roots i basically mean let's support full folders as routes in a place outside the main root, but in the repo. That would allow all the commands.
>
> So as tasks
> - allow configurable root
> - allow routes outside root
>
> The 2nd one technically acts as a 'Root' too since a folder is scopable anyway

## Method

Five read-only Astra xhigh members worked from commit `7c86a8ba3`, the tip of the Task 86 branch: the CLI architect, a contrarian minimalist, a user and agent experience designer, a contract and compatibility guardian and a test and qualification engineer. Each answered seven questions in Stage 1 and ranked all five proposals in Stage 2. The chair decided with the maintainer's direction, and the CLI architect, which won MODEL unanimously and tied for PLAN, wrote the Task 87 plan.

## Rankings

Average rank from five reviewers, lower is better.

| Proposal | MEANING | MODEL | FIT | TESTS | PLAN |
| --- | ---: | ---: | ---: | ---: | ---: |
| P1 Contract and compatibility guardian | 3.40 | 2.20 | 3.20 | 4.80 | 3.60 |
| P2 Contrarian minimalist | **1.00** | 3.00 | **1.20** | 3.40 | **1.80** |
| P3 Test and qualification engineer | 4.60 | 4.80 | 3.80 | **1.00** | 2.80 |
| P4 CLI architect | 2.00 | **1.00** | 1.80 | 2.60 | **1.80** |
| P5 User and agent experience designer | 4.00 | 4.00 | 5.00 | 3.20 | 5.00 |

MEANING favored keeping included catalogues read-only. The maintainer's direction replaced it. The [proposals](proposals.md) and [reviews](reviews.md) are kept as evidence.

## Decisions

1. **Two tasks.** Task 85 Delivery 1 stays as frozen. Task 87 adds routes outside the main root and follows it.
2. **A folder include is a maintained route.** `route include <folder>` registers a folder outside the main root as a route that every command maintains and operates in. Its canonical catalogue is kept, or created when missing. `route exclude <folder>` removes the registration and loader row and never deletes content. Including a single file, including a catalogue file, stays read-only as today. There is no separate maintain command.
3. **Settings.** `roots` keeps its meaning, included files. Outside routes get their own list, `outsideRoutes`, in settings v5. The loader decides loading, and a registration never recreates a removed row.
4. **Identity.** Content in an outside route keeps exact `./path` identities, such as `./docs/team/testing.md`, before and after registration. No short IDs, aliases or search order for outside routes.
5. **Boundaries.** An outside route cannot overlap the main root, `.agents/`, another outside route, Git metadata or `.open-forge-state/`, cannot be the workspace root and cannot alias into a reserved place. Moves stay within one outside route at first. Registration grants bounded maintenance, not package ownership or implicit destinations.
6. **Separate facts.** Where a source is and its identity, its loader and catalogue edges, whether it may be maintained, and its ownership stay separate facts. Combining inventories keeps every included row's edges, conditions and order.
7. **Task 85 needs no API change now.** Its foundation documents and tests that `SourceRoots` implies no loading or write authority, that the primary mappings never choose among other roots, and that implicit content destinations stay separate. Catalogue reconstruction keeps included identities, aliases and conditions.
8. **Test corrections for Task 85,** from the test and qualification engineer: select the real Include and Exclude test classes, replace prose assertions with snapshots or parsed structure, keep tiers clean, keep published journeys to journeys, give EndToEnd and test support a tracked build-time projection of owned values instead of copied constants, and keep byte comparisons for file preservation.

## Declined

- Keeping included catalogues read-only and waiting for a concrete maintenance workflow. The maintainer asked for maintained folder routes.
- A separate `route maintain` command or a `--maintain` flag.
- Short IDs or aliases for outside routes, and adding outside routes to `roots`.
- A mount system or a writable owner chosen by registration order.
