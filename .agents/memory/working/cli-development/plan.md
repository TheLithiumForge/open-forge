---
open-forge:
  description: Current selection and order of open work after the public beta
  tags: [LoadNow, Memory, Working, CLI, Plan, Contextual, Active]
---

# Development Plan

## 2026-10-02 execution window

The maintainer requested the first-install preservation fixes on a branch by
12:00, with squash commit, push, and release permitted only from 12:00 through
13:00 Europe/Zurich. The candidate is therefore qualified before committing,
using its actual HEAD, complete source-change fingerprint, and immutable built
artifacts. Freeze its complete file tree for exact comparison with the develop
squash. This timing instruction supersedes the earlier requirement to commit
candidate C before local qualification; every qualification gate remains.
The [Task 70 record](tasks/task70-existing-workspace-adoption-during-installation.md)
tracks the current correction and evidence. No individual requalification of
Tasks 32, 47, 54, or 64 was requested.

## Completed beta4 stabilization horizon

[Task 69](tasks/task69-next-beta-stabilization-release.md) completed beta4
qualification and release. [Task 68](tasks/task68-repository-link-validation.md)
completed the offline link-validation gate. Their phase, milestone, package,
hosted-run, documentation, upgrade, and demonstration receipts remain in those
Task records. `0.9.0-beta.4` remains the current public release. The selected
beta5 release target is `0.9.0-beta.5`, and its version preparation is
complete. Tasks 47, 54, 32, and 64 are squash-integrated into clean local
`develop`; beta4 remains public, and beta5 has not been pushed or published.
See the [wave
capsule](one-zero-polish-wave.md) for the preparation receipt and current
release boundary.

## Beta5 qualification horizon

The selected beta5 scope is Tasks 32, 47, 54, 64, and 70. All local combined
gates now pass: 10,360 runtime checks, 34 platform exclusions, package/site
qualification, and the actual published-beta4 upgrade journey. The
[Task 70 record](tasks/task70-existing-workspace-adoption-during-installation.md)
records final identities and evidence. Root accepts the candidate for integration.
Tasks 32, 47, 54, and 64 already have their individual squash commits on develop.
Task 70 and the beta5 version preparation remain on the qualified branch.

During the authorized noon window, squash the frozen candidate into develop as
M and require exact tree equality. Push develop, require a green exact-M
six-host hosted Build, then follow normal main/release integration and public
postchecks. Do not repeat the completed local suites without a new failure or
source change. Beta4 remains public until publication succeeds; no release notes
are required. Tasks 34 and 39 stay paused. Task 48 remains deferred beyond beta5
and required before 1.0. Task closure remains pending release acceptance.

## Original 1.0 polish selection

On 2026-10-01, the maintainer selected the 1.0 polish wave recorded in the
[wave capsule](one-zero-polish-wave.md) and [Task index](tasks/_tasks.md#current-order).
Current task states and lane ownership are in the
[project-control ledger](project-control.md#active-task-ledger); frozen
decisions, selection boundaries, and deferred work are in the [wave
capsule](one-zero-polish-wave.md).

Task 32 remains included on the assumption that the spoken reference to 42
meant Task 32; optional clarification is not a blocker.

Task 55 is complete at phase 1/1, milestone 1/1. The maintainer retained the
current root; APM coexistence remains product direction, not certification.
Task 65 is complete at phase 1/1, milestone 1/1. It closes only the placement
question; the broader [Local Planning review](../local-planning.md) remains
open.

Frozen decisions, name mappings, and deferred work are in the wave capsule. Task 59
retains the historical beta2 release receipt. Tasks 68 and 69 retain their
completed beta4 and link-validation receipts unchanged.

## Open work

The [Task index](tasks/_tasks.md#current-order) holds the full order, and the
[ledger](project-control.md#active-task-ledger) holds each Task's state. The
remaining beta follow-ups stay in [Beta follow-ups](tasks/beta-follow-ups.md).

## History

The plan as it stood at the beta release is preserved as
[Plan at beta](../../archived/cli-development/plan-beta.md). Completed Tasks and
beta preparation records moved to Archived Memory on 2026-09-25.
