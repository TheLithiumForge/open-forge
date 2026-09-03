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
- The Task 13 preparation candidate is
  `e76896965e1cce51b7295f899499c5a7d0cce252`, exact tree
  `c204c17b4bdda87bf43a479879d6ebcf87472283`, with direct parent
  `3c904a23ed6735ca91bb3ca2810156b01a25e5b9`, tree
  `f2a780b8cb1da6ac680b7d7d7586d9732cb20cf6`. Its direct-parent comparison
  changes only the Task 13 record. The commit containing this record integrates
  that semantic delta from local `develop` parent
  `de40d550c00e51f55fe7e8b5d39297c450f721e9`, tree
  `e6040c4996eb8af5375979b54a9bafc198438a34`.
- The resulting local baseline contains CLI Quality Remediation, bounded review
  orchestration, permanent task identity and dynamic progress controls, the
  non-shipping Task 7 npm package and local-link preparation, the agent/workflow
  and CLI Architecture authority audits, the streamlined Task 4–6 trial,
  explicit repository agent-tooling placement, and Route Update.
- The replacement CLI remains non-shipping. No remote action or publication is
  authorized.

## Task Identity, Queue, And Active Work

| Permanent task ID and actual name                        | Task record                                                                           | Queue state   | Completion grace | Outcome                                                                                                             | Queue order and dependency reason                                                              | Profile                           | Lane, branch, worktree, and base                                                                                       | Responsible role                                                                                     | Integration mapping                                                                                           |
| -------------------------------------------------------- | ------------------------------------------------------------------------------------- | ------------- | ---------------- | ------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------- | --------------------------------- | ---------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| Task 4 “Route Move”                                      | [Route Move](tasks/route-mutation/route-move.md)                                      | `ACTIVE`      | Not applicable   | Implement route movement with reference, overwrite, navigation, lifecycle, and recovery integrity.                  | Active at phase 4/6, milestone 6/12; M7 public composition, presentation, and evidence active. | Streamlined assured trial         | `codex/route-move`; base `272f5121`; worktree `open-forge-worktree/route-move`; accepted fixture checkpoint `67ed59bf` | Route Move Task Mastermind with Gray/Red boundaries and one continuous Brilliant Implementer         | Separate future squash-integration boundary after behavior, review, and final gates.                          |
| Task 12 “CLI Architecture Authority Remediation”         | [Architecture Authority Remediation](tasks/cli-architecture-authority-remediation.md) | `PREPARED`    | Not applicable   | Apply Task 9's twelve authority-placement findings without changing accepted meaning or executable behavior.        | Read-only preparation is ready; mutation follows Task 4 integration and destination approval.  | Streamlined assured documentation | Preparation reads current `develop`; exact implementation base is the integrated Task 4 result                         | Sol/xhigh Task Mastermind; explicit Gray/Red boundaries; one Brilliant Implementer; one fresh review | Separate future integration boundary.                                                                         |
| Task 14 “Extension Install”                              | [Extension Install](tasks/lifecycle/extension-install.md)                             | `PREPARED`    | Not applicable   | Install reviewed Extension packages with isolated lifecycle, collision, dependency, and recovery integrity.         | First adoption command after Task 12; four command-local Gray freezes await acceptance.        | Streamlined assured task          | Planned branch and worktree; exact base deferred until activation                                                      | Task Mastermind; explicit Gray/Red boundaries; one Brilliant Implementer; one fresh review           | Separate future integration boundary.                                                                         |
| Task 15 “Status”                                         | [Status](tasks/operations/status.md)                                                  | `QUEUED`      | Not applicable   | Report typed facts for every producer present at its frozen baseline without diagnosis or mutation.                 | After Task 14; establishes the first explicit contributor inventory.                           | Streamlined assured task          | Planned branch and worktree; exact base deferred until activation                                                      | Task Mastermind; explicit Gray/Red boundaries; one Brilliant Implementer; one fresh review           | Later producers extend the accepted contributor inventory before their acceptance.                            |
| Task 16 “Doctor”                                         | [Doctor](tasks/operations/doctor.md)                                                  | `QUEUED`      | Not applicable   | Diagnose the complete Task 15 contributor inventory with typed facts and deterministic recommendations.             | After Task 15; healthy means complete for the declared inventory.                              | Streamlined assured task          | Planned branch and worktree; exact base deferred until activation                                                      | Task Mastermind; explicit Gray/Red boundaries; one Brilliant Implementer; one fresh review           | Later producers extend both Status facts and applicable Doctor domains.                                       |
| Task 7 “npm Package Manager Release and Local Linking”   | [npm Packages](tasks/delivery/01-npm-packages.md)                                     | `QUEUED`      | Not applicable   | Extend the accepted package graph and local-link tooling to Linux, macOS, and Windows x64.                          | After Task 12 and Task 13's platform decision; may then run beside Tasks 14–16.                | Reopened streamlined task         | Planned branch and worktree; exact base deferred until activation                                                      | One Task Mastermind, one Brilliant Implementer, one fresh reviewer                                   | Preserves the accepted 8/8 historical closeout and adds a separate 0/7 horizon.                               |
| Task 5 “Route Remove”                                    | [Route Remove](tasks/route-mutation/route-remove.md)                                  | `QUEUED`      | Not applicable   | Implement positive-unmanaged route removal with dependency, reference, navigation, and recovery integrity.          | After Tasks 4 and 12; reprioritized behind the adoption slice.                                 | Streamlined assured trial         | Planned `codex/route-remove`; exact base deferred until activation                                                     | Task Mastermind; Gray/Red owners; one Brilliant Implementer                                          | Separate future integration boundary.                                                                         |
| Task 6 “Root Update”                                     | [Root Update](tasks/lifecycle/update.md)                                              | `QUEUED`      | Not applicable   | Reconcile lifecycle-managed Framework files and regions from accepted identity.                                     | After Task 5 completes Route Mutation M2.                                                      | Streamlined assured trial         | Planned branch and worktree; exact base deferred until activation                                                      | Task Mastermind; Gray/Red owners; one Brilliant Implementer                                          | Must extend the accepted Status/Doctor contributor inventory.                                                 |
| Task 17 “Extension Update”                               | [Extension Update](tasks/lifecycle/extension-update.md)                               | `QUEUED`      | Not applicable   | Update installed Extensions from reviewed source while preserving isolation, ownership, and recovery integrity.     | After Task 6 and the accepted Extension Install boundary.                                      | Streamlined assured task          | Planned branch and worktree; exact base deferred until activation                                                      | Task Mastermind; explicit Gray/Red boundaries; one Brilliant Implementer; one fresh review           | Must extend the accepted Status/Doctor contributor inventory.                                                 |
| Task 18 “Extension Remove”                               | [Extension Remove](tasks/lifecycle/extension-remove.md)                               | `QUEUED`      | Not applicable   | Remove only lifecycle-managed Extension content while preserving user and other-package state.                      | After Task 17.                                                                                 | Streamlined assured task          | Planned branch and worktree; exact base deferred until activation                                                      | Task Mastermind; explicit Gray/Red boundaries; one Brilliant Implementer; one fresh review           | Must extend the accepted Status/Doctor contributor inventory.                                                 |
| Task 19 “Repair”                                         | [Repair](tasks/operations/repair.md)                                                  | `QUEUED`      | Not applicable   | Turn supported Doctor findings into explicit verified repair plans under mutation safeguards.                       | After Task 18 and the complete contributor inventory.                                          | Streamlined assured task          | Planned branch and worktree; exact base deferred until activation                                                      | Task Mastermind; explicit Gray/Red boundaries; one Brilliant Implementer; one fresh review           | Separate future integration boundary.                                                                         |
| Task 20 “Cleanup”                                        | [Cleanup](tasks/operations/cleanup.md)                                                | `QUEUED`      | Not applicable   | Delete only positively recognized recovery artifacts after lease-bound revalidation.                                | After Task 19 and every artifact producer.                                                     | Streamlined assured task          | Planned branch and worktree; exact base deferred until activation                                                      | Task Mastermind; explicit Gray/Red boundaries; one Brilliant Implementer; one fresh review           | Separate future integration boundary.                                                                         |
| Task 10 “CLI Command Surface Audit”                      | [CLI Command Surface Audit](tasks/cli-command-surface-audit.md)                       | `QUEUED`      | Not applicable   | Review the complete retained CLI for direct PR-level architecture, design, refactoring, and test-evidence problems. | After Task 20 and before delivery and release acceptance.                                      | Read-only strategic audit         | Planned branch and worktree; exact base deferred until activation                                                      | Dedicated Review Mastermind with bounded topic reviewers selected at Preflight                       | Audit-only integration; accepted findings activate Task 21.                                                   |
| Task 21 “CLI Command Surface Remediation”                | [CLI Command Surface Remediation](tasks/cli-command-surface-remediation.md)           | `CONDITIONAL` | Not applicable   | Apply only the Task 10 findings accepted by the maintainer.                                                         | Identity reserved; scope waits for Task 10 acceptance.                                         | Deferred conditional remediation  | No branch, worktree, or implementation horizon until accepted findings exist                                           | Future Task Mastermind                                                                               | Activates only for an exact maintainer-accepted finding set.                                                  |
| Task 13 “Native linux-x64 CI and Reproducible Artifacts” | [Native CI and Reproducible Artifacts](tasks/delivery/02-native-ci.md)                | `QUEUED`      | Not applicable   | Implement the accepted Linux D1 boundary after the completed preparation and required x64 authority alignment.      | After all retained commands, Task 10, and conditional Task 21 remediation; before Task 22.     | Supervised Luna preparation trial | Preparation candidate `e7689696`, tree `c204c17b`; later implementation base deferred until activation                 | Preparation owner complete; later semantic implementation remains Sol/xhigh                          | Preparation is integrated by the commit containing this record; later implementation keeps this permanent ID. |
| Task 22 “Final Documentation, Acceptance, and Release”   | [Accept And Release The Complete CLI](tasks/delivery/04-release.md)                   | `QUEUED`      | Not applicable   | Align docs and packages, run complete acceptance, and perform only a separately authorized release.                 | Final task after Task 13 implementation and complete contributor gate.                         | Final acceptance and release      | Planned branch and worktree; exact base deferred until activation                                                      | Task Mastermind and maintainer                                                                       | No publication is authorized by queueing this task.                                                           |

Queue order expresses dependency and priority, not numeric order. A completed
task remains in `RECENTLY_COMPLETED` on its completion-bearing update and exactly
two later progress-bearing Overseer updates. The ledger then moves it to the
completion record before the next update. Reopened work retains its permanent ID
and actual name and receives a new explicit Task-owned horizon.

## Simplified Flow Trial

Tasks 4–6 and 14–20 use the streamlined assured lane unless their Preflight
returns a material reason to change profile. The Task Mastermind performs Preflight;
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

- Task 7's accepted phase 5/5, milestone 8/8 closeout `6ba0e060`, tree
  `bccefb12`, remains immutable and squash-integrated. A separate phase 0/4,
  milestone 0/7 platform-expansion horizon is queued after Tasks 12 and 13's
  preparation decision.
- Task 4 “Route Move” (phase 4/6): milestone 6/12 — M7 public composition,
  presentation, and evidence are active in its isolated worktree. Its source
  remains separate from this root-tooling integration.
- Task 11 is Complete at phase 5/5, milestone 8/8. Accepted closeout `f309f29f`,
  tree `c05c2ed6`, is squash-integrated as `f8377094`, the same exact tree, from
  local `develop` baseline `416ea6ce`.
- Tasks 8 and 9 are Complete and dequeued. Their accepted commits and exact
  trees are recorded below.
- Task 12 preparation is ready without mutation. After Route Move integration
  and destination approval, Task 12 mutates first. Task 14 preparation is also
  ready with four exact maintainer freezes recorded in its Task; Tasks 14–16
  then form the prioritized adoption slice.
  Task 7 may run beside that slice after Task 12 and Task 13's platform decision.
  Tasks 5, 6, and 17–20 follow in the recorded priority order.
- Task 10 retains its permanent identity and is queued after every retained
  command and before delivery and release acceptance.
- Task 13 is queued at phase 1/3, milestone 2/6 after accepted preparation
  candidate `e7689696`, tree `c204c17b`, was integrated by the commit containing
  this record. Linux, macOS, and Windows x64 direction is accepted; ARM remains
  undecided. CI, package, production, test, architecture, contract, and public
  surfaces remain unchanged. Implementation waits for all retained commands,
  Task 10, and every conditional Task 21 remediation accepted from that audit.

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
  - Task 4 “Route Move” (phase 4/6): milestone 6/12 — M7 public composition,
    presentation, and evidence active.
- Prepared tasks:
  - Task 12 “CLI Architecture Authority Remediation” (phase 0/5): milestone
    0/1 — authority map ready; mutation waits for Task 4 integration and
    destination approval.
  - Task 14 “Extension Install” (phase 0/5): milestone 0/8 — preflight ready;
    four command-local freezes await acceptance and Task 12 completion.
- Queued preparation result: Task 13 “Native linux-x64 CI and Reproducible
  Artifacts” (phase 1/3): milestone 2/6 — preparation integrated; implementation
  waits for all retained commands, Task 10, and conditional Task 21 remediation.
- Task 11 “Root Tooling Placement Remediation” is dequeued after its
  completion-bearing update and two subsequent progress updates.
- Next meaningful project boundary: complete Route Move M7 composition,
  presentation, evidence, whole-task review, and final gates. Then integrate
  Route Move, activate the prepared Task 12 mutation after destination approval,
  and proceed through the prepared Extension Install boundary, Status, and
  Doctor. Task 13 remains queued at its later delivery boundary.
- Blocker: Route Move's final public acceptance awaits the maintainer's
  third-positional-operand decision. Standard pinned-parser behavior can safely
  report that input as shell `cli.parser.invalid`; no accepted command-local
  typed mechanism distinguishes it without prohibited token heuristics.
