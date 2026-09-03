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

- Route Update integration started from local `develop` commit
  `996c2e17d1142ffb30dc7a2d17df657419566f97`, exact tree
  `877314d48db49013edc1c4dad1abb545b37caefc`.
- The commit containing this record squash-integrates accepted Task closeout
  `27df8325488a4c036a240d03ad54105be0f2cdfa`, tree
  `b68d4349e13879fccc607f78a284a49e61f955ea`. The final executable behavior is
  commit `8a398f2e8e6f50beb730bea2a9c658a1fc923ffa`, tree
  `3bb4a22440b32781376c4b9160d3d72d168da9ef`.
- The commit containing this record squash-integrates the restarted Task 7
  closeout `6ba0e060ed9aabc1954d70d0f1d2277bf5d8dff6`, tree
  `bccefb120f4e2d05a632cc80fc03b04ab2870e2d`, from restart baseline
  `195ff13ecff6a598dd18ef22a0335c1dc75e6736`.
- Integration commit `f8377094f5cb4bfa056cf9e286c1cf8efd386986`, exact
  tree `c05c2ed6d1adef05bc6cdc3cb486fc29899f2c97`, squash-integrates Task 11
  closeout `f309f29fc0115f84231b0430a34f9f3cfd2e31fb`, the same exact tree, from
  local `develop` baseline `416ea6ce59e019b7dec0f32e8262640133c6dca5`, tree
  `0ee2597a1ca69656e941d2c88e1035cf6e992b4b`.
- The resulting local baseline contains CLI Quality Remediation, bounded review
  orchestration, permanent task identity and dynamic progress controls, the
  non-shipping Task 7 npm package and local-link preparation, the streamlined
  Task 4–6 trial, explicit repository agent-tooling placement, and Route Update.
- The replacement CLI remains non-shipping. No remote action or publication is
  authorized.

## Task Identity, Queue, And Active Work

| Permanent task ID and actual name | Task record                                          | Queue state | Completion grace | Outcome                                                                                                    | Queue order and dependency reason               | Profile                   | Lane, branch, worktree, and base                                                                                      | Responsible role                                                                             | Integration mapping                                                                  |
| --------------------------------- | ---------------------------------------------------- | ----------- | ---------------- | ---------------------------------------------------------------------------------------------------------- | ----------------------------------------------- | ------------------------- | --------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------ |
| Task 4 “Route Move”               | [Route Move](tasks/route-mutation/route-move.md)     | `ACTIVE`    | Not applicable   | Implement route movement with reference, overwrite, navigation, lifecycle, and recovery integrity.         | Active at phase 4/6, milestone 5/12; M6 active. | Streamlined assured trial | `codex/route-move`; base `272f5121`; worktree `open-forge-worktree/route-move`; accepted oracle checkpoint `baf06a42` | Route Move Task Mastermind with Gray/Red boundaries and one continuous Brilliant Implementer | Separate future squash-integration boundary after behavior, review, and final gates. |
| Task 5 “Route Remove”             | [Route Remove](tasks/route-mutation/route-remove.md) | `QUEUED`    | Not applicable   | Implement positive-unmanaged route removal with dependency, reference, navigation, and recovery integrity. | After Route Move and its command-local freeze.  | Streamlined assured trial | Planned `codex/route-remove`; exact base deferred until activation                                                    | Task Mastermind; Gray/Red owners; one Brilliant Implementer                                  | Separate future integration boundary.                                                |
| Task 6 “Root Update”              | [Root Update](tasks/lifecycle/update.md)             | `QUEUED`    | Not applicable   | Reconcile lifecycle-managed Framework files and regions from accepted identity.                            | After the complete Route Mutation M2 lane.      | Streamlined assured trial | Planned branch and worktree; exact base deferred until activation                                                     | Task Mastermind; Gray/Red owners; one Brilliant Implementer                                  | Separate future integration boundary.                                                |

Queue order expresses dependency and priority, not numeric order. A completed
task remains in `RECENTLY_COMPLETED` on its completion-bearing update and exactly
two later progress-bearing Overseer updates. The ledger then moves it to the
completion record before the next update. Reopened work retains its permanent ID
and actual name and receives a new explicit Task-owned horizon.

## Simplified Flow Trial

Tasks 4–6 use the streamlined assured lane unless their Preflight returns a
material reason to change profile. The Task Mastermind performs Preflight;
Gray and Red remain explicit frozen boundaries; one Brilliant Implementer owns
Green, focused verification, and the grouped improvement pass; then the Task
Mastermind performs one fresh whole-task review that includes behavior,
production architecture and structure, and test/evidence quality. Separate Blue
and Purple phases and owners are skipped unless Preflight names a material risk
that requires an independent protected boundary.

Use Luna/max support workers liberally for disjoint menial or simple work, exact
shell/build/test execution, artifact inspection, and large-output summarization.
The Brilliant Implementer retains the coherent semantic mutation boundary;
support workers receive literal bounded packets and do not choose product,
architecture, test meaning, or commands.

The Overseer evaluates this trial after each Task and cumulatively after Task 6.
Record elapsed critical-path time, agent/context handoffs, correction cycles,
accepted review findings by behavior, production-structure, and test/evidence
source, rejected/duplicate/preference/false-positive findings, focused or full
gate failures, integration conflicts or semantic convergence work, and any
missed defect discovered after acceptance. Compare those facts with the recent
heavier Route Create and Route Update flows. Retain, adjust, or stop the trial
from observed quality and rework rather than token count or raw finding count
alone.

## Current Integration Boundary

- Task 7 is Complete at phase 5/5, milestone 8/8. Accepted closeout `6ba0e060`,
  tree `bccefb12`, is squash-integrated by the commit containing this record from
  restart baseline `195ff13e`.
- Task 4 “Route Move” (phase 4/6): milestone 5/12 — M6 active in its isolated
  worktree. Its source remains separate from this root-tooling integration.
- Task 11 is Complete at phase 5/5, milestone 8/8. Accepted closeout `f309f29f`,
  tree `c05c2ed6`, is squash-integrated as `f8377094`, the same exact tree, from
  local `develop` baseline `416ea6ce`.
- Tasks 5 and 6 remain queued behind Route Move and the complete Route Mutation
  lane respectively.

## Completion And Integration Ledger

| Permanent task ID and actual name                      | Result                                                                                                                                                   | Evidence and residual boundary                                                                                                                                                                                                                                                                                                                                                                              | State                  |
| ------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------- |
| Task 1 “CLI Quality Remediation”                       | Product squash `862cbf2a`, tree `571f104f`; closeout `5053bf0c`, tree `732d8ea6`.                                                                        | Warning-free Release build; managed Unit `1522/1522`, Integration `736/736`, EndToEnd `146/146`; supported Native AOT Integration `736/736`; format, diff, and independent review passed.                                                                                                                                                                                                                   | Complete and dequeued. |
| Task 2 “Review Orchestration Workflow”                 | Initial integration `5aad04ac`, tree `0f56b2ce`; permanent-identity and progress follow-up `3356eba1`, tree `af5efb2a`.                                  | Immutable Git-object review gateway, bounded topic roles, quiet-child and takeover controls, APM `27/27/27/54`, Open Forge, formatting, projection, protected-path, and C# identity gates passed. Automatic per-file fan-out and reviewer-economics claims remain experimental.                                                                                                                             | Complete and dequeued. |
| Task 3 “Route Update”                                  | Accepted closeout `27df8325`, tree `b68d4349`; executable behavior `8a398f2e`, tree `3bb4a224`; squash integration is the commit containing this record. | Warning-free Release build; managed Unit `1602/1602`, Integration `809/809`, EndToEnd `152/152`; native Integration `809/809`, native EndToEnd `152/152`, managed-on-native EndToEnd `152/152`; dogfood, format, static, protected-path, and holistic review pass. Portable real interrupted-process proof remains deferred.                                                                                | Complete and dequeued. |
| Task 7 “npm Package Manager Release and Local Linking” | Restarted closeout `6ba0e060`, tree `bccefb12`; squash integration is the commit containing this record.                                                 | TypeScript, static, package stage/pack, and one isolated offline-install `PackageEndToEnd` journey pass. Publication, live link/unlink, CLI invocation, and live Windows evidence remain intentionally unexecuted. The former “npm Link Shims” label and integration `128b70b3`, tree `0ff3d386`, are rejected mistaken realization history for this same permanent Task ID, not a separate or reused Task. | Complete and dequeued. |
| Task 11 “Root Tooling Placement Remediation”           | Accepted closeout `f309f29f`, tree `c05c2ed6`; squash integration `f8377094`, exact tree `c05c2ed6`.                                                     | Review integration `7/7/103`, projection integration `1/1/4`, strict TypeScript, targeted ESLint and Prettier, patcher help, path classification, source inventory, protected-path, JSON, diff, and clean-state gates passed. No dependency install, generated-agent mutation, APM install, remote action, or publication occurred.                                                                         | Complete and dequeued. |

## Recovery And Current State

- Last reconciled: 2026-09-03.
- Git authority: local commit and tree identities plus clean or explicitly
  reported worktree state.
- Progress visibility: Task 11 completion is established by immutable Git state,
  reproduced focused evidence, and its accepted Task record. Missing optional
  child messages alone remain `progress unobserved`, not failure.
- Active task: Task 4 “Route Move” (phase 4/6): milestone 5/12 — M6 active.
- Task 11 “Root Tooling Placement Remediation” is dequeued after its
  completion-bearing update and two subsequent progress updates.
- Next meaningful project boundary: complete Route Move's structural correction,
  behavior, review, and gates without changing Tasks 5–6 queue order.
- Blocker: None.
