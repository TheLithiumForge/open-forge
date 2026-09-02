---
open-forge:
  description: Control permanent task identities, the dynamic queue, integration, and recovery for replacement CLI development
  tags: [Memory, Working, Contextual, Active, KeepInMind, CLI, Project, Orchestration, Task, Integration]
---

# CLI Project Control Ledger

## Project Identity

- Project: Replacement Open Forge CLI development.
- Repository and integration branch: repository root on local `develop`.
- Project principal: The user-facing Overseer.
- Decision authority: The maintainer accepts consequential product, Framework,
  architecture, priority, tradeoff, external-effect, and final-acceptance
  decisions.
- Local policy: Work remains local. Do not push, publish, deploy, install
  globally, contact remotes, or use destructive recovery without exact
  authorization.
- Declared stable repository-global task horizon: None. Permanent task IDs do
  not imply a fixed total task count.

The [CLI Architecture](../../crystallized/documents/cli/architecture.md),
[Development Plan](plan.md), and command contracts define accepted product and
program meaning. Each linked Task record defines its accepted phase and
milestone horizon, current phase, completed milestone count, current-state
suffix, task evidence, and task budgets. This ledger defines permanent task
identity, queue state, completion grace, worktree mapping, and integration state.

## Accepted Baseline

- Accepted local `develop` commit:
  `128b70b3a9eca6e4a653ac60499e0d25ada4d14b`.
- Exact tree: `0ff3d386f09770384e7529bb6101e0a7c108b63c`.
- The baseline contains CLI Quality Remediation, bounded review orchestration,
  permanent task identity and dynamic progress controls, and repository-local
  npm linking for the managed development CLI.
- The replacement CLI remains non-shipping. No remote action or publication is
  authorized.

## Task Identity, Queue, And Active Work

| Permanent task ID and actual name | Task record                                          | Queue state | Completion grace | Outcome                                                                                                    | Queue order and dependency reason                    | Profile      | Lane, branch, worktree, and base                                                                       | Responsible role                                                      | Integration mapping                                                                                                            |
| --------------------------------- | ---------------------------------------------------- | ----------- | ---------------- | ---------------------------------------------------------------------------------------------------------- | ---------------------------------------------------- | ------------ | ------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| Task 3 “Route Update”             | [Route Update](tasks/route-mutation/route-update.md) | `ACTIVE`    | Not applicable   | Implement bounded route content and metadata update without identity drift.                                | Current mutation task after CLI Quality Remediation. | Assured      | `codex/route-update`; `open-forge-worktree/route-update`; accepted task base `5aad04a`, tree `0f56b2c` | Route Update Task Mastermind with one continuous implementation owner | Revalidate against `128b70b`, tree `0ff3d38`, at the next coherent commit boundary; integrate only after full Task acceptance. |
| Task 4 “Route Move”               | [Route Move](tasks/route-mutation/route-move.md)     | `QUEUED`    | Not applicable   | Implement route movement with reference, overwrite, navigation, lifecycle, and recovery integrity.         | First after accepted Route Update integration.       | Not selected | Planned `codex/route-move`; exact base deferred until activation                                       | Not assigned                                                          | Separate future integration boundary.                                                                                          |
| Task 5 “Route Remove”             | [Route Remove](tasks/route-mutation/route-remove.md) | `QUEUED`    | Not applicable   | Implement positive-unmanaged route removal with dependency, reference, navigation, and recovery integrity. | After Route Move and its command-local freeze.       | Not selected | Planned `codex/route-remove`; exact base deferred until activation                                     | Not assigned                                                          | Separate future integration boundary.                                                                                          |
| Task 6 “Root Update”              | [Root Update](tasks/lifecycle/update.md)             | `QUEUED`    | Not applicable   | Reconcile lifecycle-managed Framework files and regions from accepted identity.                            | After the complete Route Mutation M2 lane.           | Not selected | Planned branch and worktree; exact base deferred until activation                                      | Not assigned                                                          | Separate future integration boundary.                                                                                          |

Queue order expresses dependency and priority, not numeric order. A completed
task remains in `RECENTLY_COMPLETED` on its completion-bearing update and exactly
two later progress-bearing Overseer updates. The ledger then moves it to the
completion record before the next update. Reopened work retains its permanent ID
and actual name and receives a new explicit Task-owned horizon.

## Current Integration Boundary

- Task 3 remains isolated on `codex/route-update`. Its Task Mastermind recorded
  the newer `develop` identity without rebasing, merging, or interrupting active
  M7 work.
- The next coherent Task 3 commit and evidence boundary must prove ancestry and
  exact path overlap against the two post-base integrations before later
  integration planning.
- Task 3 owns Route Update behavior and its declared integration neighborhood.
  The Overseer retains project sequencing, shared semantic authority, final
  integration, and project acceptance.

## Completion And Integration Ledger

| Permanent task ID and actual name      | Result                                                                                                                  | Evidence and residual boundary                                                                                                                                                                                                                                                  | State                                             |
| -------------------------------------- | ----------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------- |
| Task 1 “CLI Quality Remediation”       | Product squash `862cbf2a`, tree `571f104f`; closeout `5053bf0c`, tree `732d8ea6`.                                       | Warning-free Release build; managed Unit `1522/1522`, Integration `736/736`, EndToEnd `146/146`; supported Native AOT Integration `736/736`; format, diff, and independent review passed.                                                                                       | Complete and dequeued.                            |
| Task 2 “Review Orchestration Workflow” | Initial integration `5aad04ac`, tree `0f56b2ce`; permanent-identity and progress follow-up `3356eba1`, tree `af5efb2a`. | Immutable Git-object review gateway, bounded topic roles, quiet-child and takeover controls, APM `27/27/27/54`, Open Forge, formatting, projection, protected-path, and C# identity gates passed. Automatic per-file fan-out and reviewer-economics claims remain experimental. | Complete and dequeued.                            |
| Task 7 “npm Link Shims”                | Integration `128b70b3`, tree `0ff3d386`.                                                                                | Node tests `16/16`, exact three-file package dry-run, manifest, mode, formatting, diff, nonmutation, and protected-path gates passed. Live npm link/unlink and a live Windows journey remain intentionally unexecuted.                                                          | Complete; completion grace consumed and dequeued. |

## Recovery And Current State

- Last reconciled: 2026-09-02.
- Git authority: local commit and tree identities plus clean or explicitly
  reported worktree state.
- Progress visibility: Route Update progress is observed through the Task
  Mastermind, its continuous implementation owner, Git state, and evidence.
  Missing optional child messages alone are `progress unobserved`, not failure.
- Active task: Task 3 “Route Update”. Its linked Task record defines the current
  phase, completed milestone count, current-state suffix, and evidence.
- Next meaningful project boundary: Revalidate the Task's next accepted commit
  against current `develop`, then retain isolation until Task acceptance makes
  it integration-ready.
- Blocker: None.
