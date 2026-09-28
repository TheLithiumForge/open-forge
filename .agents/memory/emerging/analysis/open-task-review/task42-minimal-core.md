---
open-forge:
  description: Review of Task 42 Minimal Core, covering what it implies, dependencies, remaining work, pros and cons, and a recommendation
  tags: [Memory, Analysis, TaskReview, Contextual, Candidate]
---

# Task 42 Minimal Core Review

## Question

Is Task 42 still worth doing, what exactly remains, and when should it run?

**Task record:** [Task 42](../../../archived/cli-development/tasks/task42-minimal-core.md)

## Current Conclusion

**Recommendation:** Close as done.

**Size:** Small, because only record keeping remains: correct the stale body, hand one open question to Task 53, and archive.

The reduced Core shipped on 2026-09-18 and is what `0.9.0-beta.1` installs. The only gap the record keeps open is "historical upgrade evidence", meaning proof that a workspace installed with the old, larger Core can move to the new layout. That population is effectively empty. The first npm publication of `@thelithiumforge/open-forge` was `0.9.0-beta.1` on 2026-09-24, six days after the reduction. The [beta release record](../../../archived/cli-development/tasks/task51-beta-release.md) shows npm returning E404 when it created the first scoped package. The older `v0.001` and `v0.002` tags are a July prototype with version `0.0.0` and a different CLI. Only the maintainer's own pre-beta workspaces ever held the old Core. The requirement no longer protects any user, so it should not hold the task open.

## What It Implies

Closing changes nothing for users, the Framework, or the CLI. It removes a "Blocking beta" item that is already delivered from the [task index](../../../working/cli-development/tasks/_tasks.md). The one live idea in the task, "Core is defensible route by route", continues in [Task 53](../../../working/cli-development/tasks/task53-loading-and-scoping-audit.md), which asks whether each startup entrypoint earns its cost.

## State Today

Verified against `815324f9`.

| Acceptance item                                                        | State today                                                                                                                                                                                                                                                                                             |
| ---------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Grouping decided before moving                                         | Done. The [focused packages decision](../../../crystallized/decisions/extensions/focused-extension-packages.md), accepted on 2026-09-23, split the roles into Planning, Project Documents, and Observations and Handoffs                                                                                |
| Each moved route lives in a named Extension with dependencies recorded | Done. Each package's `extension.json` and the [catalogue](../../../../../src/extensions/README.md) record them                                                                                                                                                                                          |
| Fresh install measurably smaller                                       | Done. The [development guide](../../../../../docs/development.md#measure-context-size) records 19 startup and 23 total files before, and 11 and 15 now. The before figures use tiktoken and the after figures use the CLI's character estimate, so the token numbers are not a like-for-like comparison |
| Install journeys still pass                                            | Done. The beta qualification ran installed npm journeys on six hosts                                                                                                                                                                                                                                    |
| Upgrade path for existing workspaces                                   | Unverified, and no public user needs it                                                                                                                                                                                                                                                                 |

The record is stale in three places:

- The "Outstanding fallout" table says the eight payload contracts have links "broken on purpose". The 2026-09-21 [documentation packet](../../../archived/cli-development/tasks/beta-follow-ups/task42-43-documentation.md) repaired them. For example, the [Decisions contract](../../../crystallized/documents/maintenance/payload/agents/memory/crystallized/decisions.md) links its Planning source and classifies it as `#Extension`.
- The table says Orchestration owns Observations and Handoffs. Since `a887e730` they belong to the `observations-and-handoffs` package.
- "That grouping is the open question" is answered by the decision above.

The task is also missing from the [project control ledger](../../../working/cli-development/project-control.md).

The CLI already handles the general mechanics. Framework `update` classifies a file Core no longer ships as retired. `UpdatePlanningPolicy.cs` deletes it when it is eligible and blocks otherwise. No command adopts an unowned file. Two similar transitions have documented manual paths: the [package split migration](../../../../../docs/extensions.md#moving-from-the-earlier-package-layout) and the Collaboration README's note on Adaptive Collaboration leaving Core.

## Dependencies

- **Blocked by:** nothing.
- **Blocks:** nothing. Task 44 waited on it, and that dependency is already satisfied.
- **Overlaps with:** Task 43 (closed together), Task 53 (inherits the route-by-route question), and Tasks 55 and 63, which own the migration questions that will matter for real users after beta.

## Remaining Work

1. Replace the "Outstanding fallout" section with one line stating that the packet repaired the contracts, and correct the Orchestration ownership.
2. State the upgrade boundary plainly: no public release carried the pre-reduction Core, so no migration is promised.
3. Add "Is each startup Core entrypoint defensible?" to Task 53's plan if it is not already covered. Task 53 already lists Guidance, Patterns, Maps, Crystallized, and Emerging.
4. Mark the task complete and move it to Archived with Task 43.

## Pros And Cons

| Pros                                                          | Cons                                                                   |
| ------------------------------------------------------------- | ---------------------------------------------------------------------- |
| Removes a delivered item from the beta ordering               | Leaves the maintainer's own old workspaces without a written migration |
| Stops an unqualified requirement from looking like open risk  | The before and after token figures use different methods               |
| Puts the remaining Core question in the task that measures it |                                                                        |

## Risks And Open Questions

- Any private or early-access workspace installed before 2026-09-18 would need a manual move. The maintainer knows whether one exists outside this repository.
- The real upgrade risk is now between `0.9.0-beta.1` and 1.0. Any Task 53 or Task 55 change to the shipped layout needs its own migration note. Do not carry this task's requirement forward as a generic promise.

## Next Check

**Action:** the maintainer confirms that no user workspace outside their own holds the pre-reduction Core, then the record is corrected and archived.

**Would change the conclusion:** a known external workspace installed from a pre-beta build, or a decision to promise automatic migration from pre-beta layouts.

**Acceptance needed:** the maintainer.
