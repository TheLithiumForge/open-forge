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
- Commit `975008d6c046d9c5c9162ba113896c50c6e332e7`, exact tree
  `842666bd1bc7365aedb83dc57aed44ecc9e6b2fc`, completes Task 8 with the
  agent and workflow change audit and its generated navigation.
- Commit `e431395add33a95d266b71d05b46fc1768ebcbf9`, exact tree
  `656cccdf8fc5a13d84ab09537bd605704aa2faac`, completes Task 9 with the
  twelve-finding CLI Architecture authority audit, its Task record, and its
  generated navigation.
- Integration commit `f8377094f5cb4bfa056cf9e286c1cf8efd386986`, exact
  tree `c05c2ed6d1adef05bc6cdc3cb486fc29899f2c97`, squash-integrates Task 11
  closeout `f309f29fc0115f84231b0430a34f9f3cfd2e31fb`, the same exact tree, from
  local `develop` baseline `416ea6ce59e019b7dec0f32e8262640133c6dca5`, tree
  `0ee2597a1ca69656e941d2c88e1035cf6e992b4b`.
- The resulting local baseline contains CLI Quality Remediation, bounded review
  orchestration, permanent task identity and dynamic progress controls, the
  non-shipping Task 7 npm package and local-link preparation, the agent/workflow
  and CLI Architecture authority audits, the streamlined Task 4–6 trial,
  explicit repository agent-tooling placement, and Route Update.
- The replacement CLI remains non-shipping. No remote action or publication is
  authorized.

## Task Identity, Queue, And Active Work

| Permanent task ID and actual name                        | Task record                                                                           | Queue state | Completion grace | Outcome                                                                                                                | Queue order and dependency reason                                       | Profile                           | Lane, branch, worktree, and base                                                                                       | Responsible role                                                                             | Integration mapping                                                                  |
| -------------------------------------------------------- | ------------------------------------------------------------------------------------- | ----------- | ---------------- | ---------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------- | --------------------------------- | ---------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------ |
| Task 4 “Route Move”                                      | [Route Move](tasks/route-mutation/route-move.md)                                      | `ACTIVE`    | Not applicable   | Implement route movement with reference, overwrite, navigation, lifecycle, and recovery integrity.                     | Active at phase 4/6, milestone 5/12; M6 active.                         | Streamlined assured trial         | `codex/route-move`; base `272f5121`; worktree `open-forge-worktree/route-move`; accepted fixture checkpoint `67ed59bf` | Route Move Task Mastermind with Gray/Red boundaries and one continuous Brilliant Implementer | Separate future squash-integration boundary after behavior, review, and final gates. |
| Task 13 “Native linux-x64 CI and Reproducible Artifacts” | [Native CI and Reproducible Artifacts](tasks/delivery/02-native-ci.md)                | `ACTIVE`    | Not applicable   | Prepare the current CI/artifact gap and a future execution capsule without editing CI or claiming delivery acceptance. | Parallel preparation only; future mutation waits for complete commands. | Supervised Luna preparation trial | `codex/native-ci-preparation`; base `c8051478`; worktree `open-forge-worktree/native-ci-preparation`                   | Experimental Luna/max Task Mastermind; at most three evidence workers; one Sol/xhigh review  | Preparation returns to the queue; implementation integrates in the later horizon.    |
| Task 12 “CLI Architecture Authority Remediation”         | [Architecture Authority Remediation](tasks/cli-architecture-authority-remediation.md) | `QUEUED`    | Not applicable   | Apply Task 9's twelve authority-placement findings without changing accepted meaning or executable behavior.           | After Route Move integration and before Route Remove.                   | Streamlined assured documentation | Planned branch and worktree; exact base deferred until activation                                                      | Task Mastermind; explicit Gray/Red boundaries; one Brilliant Implementer; one fresh review   | Separate future integration boundary.                                                |
| Task 5 “Route Remove”                                    | [Route Remove](tasks/route-mutation/route-remove.md)                                  | `QUEUED`    | Not applicable   | Implement positive-unmanaged route removal with dependency, reference, navigation, and recovery integrity.             | After Route Move and Task 12's authority freeze.                        | Streamlined assured trial         | Planned `codex/route-remove`; exact base deferred until activation                                                     | Task Mastermind; Gray/Red owners; one Brilliant Implementer                                  | Separate future integration boundary.                                                |
| Task 6 “Root Update”                                     | [Root Update](tasks/lifecycle/update.md)                                              | `QUEUED`    | Not applicable   | Reconcile lifecycle-managed Framework files and regions from accepted identity.                                        | After the complete Route Mutation M2 lane.                              | Streamlined assured trial         | Planned branch and worktree; exact base deferred until activation                                                      | Task Mastermind; Gray/Red owners; one Brilliant Implementer                                  | Separate future integration boundary.                                                |
| Task 10 “CLI Command Surface Audit”                      | [CLI Command Surface Audit](tasks/cli-command-surface-audit.md)                       | `QUEUED`    | Not applicable   | Review the complete retained CLI for direct PR-level architecture, design, refactoring, and test-evidence problems.    | After all retained commands and before delivery and release acceptance. | Read-only strategic audit         | Planned branch and worktree; exact base deferred until activation                                                      | Dedicated Review Mastermind with bounded topic reviewers selected at Preflight               | Audit-only integration; accepted findings receive a later remediation task.          |

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

## Selected Luna Preparation Experiment

Task 13 explicitly selects the
[Supervised Luna Preparation Trial](../../../workflows/supervised-luna-preparation-trial.md).
It runs beside Route Move only because its writable result is limited to its
own preparation record and its inspected delivery/CI surfaces are protected.
The trial does not change the streamlined Tasks 4–6 profile, the ordinary Task
Mastermind role, or the default development Workflows.

The experimental Task Mastermind may supervise at most three bounded evidence
workers. One fresh Sol/xhigh whole-task review is mandatory, and at most one
grouped correction is allowed. The lane returns immediately if it must choose
product, architecture, public wire, safety, lifecycle, serialization,
shared-placement, or test-meaning authority. Its preparation must be revalidated
before later CI mutation.

## Current Integration Boundary

- Task 7 is Complete at phase 5/5, milestone 8/8. Accepted closeout `6ba0e060`,
  tree `bccefb12`, is squash-integrated by the commit containing this record from
  restart baseline `195ff13e`.
- Task 4 “Route Move” (phase 4/6): milestone 5/12 — M6 active in its isolated
  worktree. Its source remains separate from this root-tooling integration.
- Task 11 is Complete at phase 5/5, milestone 8/8. Accepted closeout `f309f29f`,
  tree `c05c2ed6`, is squash-integrated as `f8377094`, the same exact tree, from
  local `develop` baseline `416ea6ce`.
- Tasks 8 and 9 are Complete and dequeued. Their accepted commits and exact
  trees are recorded below.
- Task 12 is queued immediately after Route Move and before Route Remove. Tasks
  5 and 6 remain queued behind that authority correction and the complete Route
  Mutation lane respectively.
- Task 10 retains its permanent identity and is queued after every retained
  command and before delivery and release acceptance.
- Task 13 is Active only for phase 1 preparation on exact base `c8051478`, tree
  `ccc76211`. CI, package, production, test, and public surfaces are protected;
  implementation remains behind complete command acceptance, Task 10, and every
  accepted Task 10 remediation.

## Completion And Integration Ledger

| Permanent task ID and actual name                      | Result                                                                                                                                                   | Evidence and residual boundary                                                                                                                                                                                                                                                                                                                                                                              | State                  |
| ------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------- |
| Task 1 “CLI Quality Remediation”                       | Product squash `862cbf2a`, tree `571f104f`; closeout `5053bf0c`, tree `732d8ea6`.                                                                        | Warning-free Release build; managed Unit `1522/1522`, Integration `736/736`, EndToEnd `146/146`; supported Native AOT Integration `736/736`; format, diff, and independent review passed.                                                                                                                                                                                                                   | Complete and dequeued. |
| Task 2 “Review Orchestration Workflow”                 | Initial integration `5aad04ac`, tree `0f56b2ce`; permanent-identity and progress follow-up `3356eba1`, tree `af5efb2a`.                                  | Immutable Git-object review gateway, bounded topic roles, quiet-child and takeover controls, APM `27/27/27/54`, Open Forge, formatting, projection, protected-path, and C# identity gates passed. Automatic per-file fan-out and reviewer-economics claims remain experimental.                                                                                                                             | Complete and dequeued. |
| Task 3 “Route Update”                                  | Accepted closeout `27df8325`, tree `b68d4349`; executable behavior `8a398f2e`, tree `3bb4a224`; squash integration is the commit containing this record. | Warning-free Release build; managed Unit `1602/1602`, Integration `809/809`, EndToEnd `152/152`; native Integration `809/809`, native EndToEnd `152/152`, managed-on-native EndToEnd `152/152`; dogfood, format, static, protected-path, and holistic review pass. Portable real interrupted-process proof remains deferred.                                                                                | Complete and dequeued. |
| Task 7 “npm Package Manager Release and Local Linking” | Restarted closeout `6ba0e060`, tree `bccefb12`; squash integration is the commit containing this record.                                                 | TypeScript, static, package stage/pack, and one isolated offline-install `PackageEndToEnd` journey pass. Publication, live link/unlink, CLI invocation, and live Windows evidence remain intentionally unexecuted. The former “npm Link Shims” label and integration `128b70b3`, tree `0ff3d386`, are rejected mistaken realization history for this same permanent Task ID, not a separate or reused Task. | Complete and dequeued. |
| Task 8 “Agent and Workflow Change Audit”               | Accepted commit `975008d6`, exact tree `842666bd`.                                                                                                       | The durable Observation classifies changes from 2026-08-28 through the Route Update baseline as accepted controls, additive capabilities, selected trials, unresolved ideas, generated projections, continuity state, deliberate replacements, or placement concerns. No executable or workflow authority changed in this audit.                                                                            | Complete and dequeued. |
| Task 9 “CLI Architecture Authority Audit”              | Accepted commit `e431395a`, exact tree `656cccdf`.                                                                                                       | Complete 1,053-line and 27-heading coverage, twelve stable `T9-ARCH-*` findings, repository reference and Git provenance evidence, reviewer dissent, protected executable surfaces, formatting, links, generated navigation, and diff checks passed. The Architecture and CLI behavior remained unchanged.                                                                                                  | Complete and dequeued. |
| Task 11 “Root Tooling Placement Remediation”           | Accepted closeout `f309f29f`, tree `c05c2ed6`; squash integration `f8377094`, exact tree `c05c2ed6`.                                                     | Review integration `7/7/103`, projection integration `1/1/4`, strict TypeScript, targeted ESLint and Prettier, patcher help, path classification, source inventory, protected-path, JSON, diff, and clean-state gates passed. No dependency install, generated-agent mutation, APM install, remote action, or publication occurred.                                                                         | Complete and dequeued. |

## Recovery And Current State

- Last reconciled: 2026-09-03.
- Git authority: local commit and tree identities plus clean or explicitly
  reported worktree state.
- Progress visibility: Task completion and current state are established by
  immutable Git state, reproduced focused evidence, and linked Task records.
  Missing optional child messages alone remain `progress unobserved`, not
  failure.
- Active tasks:
  - Task 4 “Route Move” (phase 4/6): milestone 5/12 — M6 active.
  - Task 13 “Native linux-x64 CI and Reproducible Artifacts” (phase 1/3):
    milestone 0/6 — M1 preparation inventory active.
- Task 11 “Root Tooling Placement Remediation” is dequeued after its
  completion-bearing update and two subsequent progress updates.
- Next meaningful project boundary: complete Route Move's grouped M6
  corrections, behavior, review, and gates, while Task 13 completes its bounded
  preparation and returns to the queue. Then activate Task 12 before Task 5.
- Blocker: None.
