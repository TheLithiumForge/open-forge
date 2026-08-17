---
open-forge:
  description: Finalize an authorized development Task after the full gate and Mastermind final review, then integrate it under the repository branch and release rules
  tags: [LoadNow, Workflow, Development, Orchestration, Acceptance, Continuation]
---

# Task Acceptance

## Goal

Finalize one complete Task after the selected full gate and Mastermind final review, then continue only within previously accepted authority and the repository's branch and release rules.

## Steps

1. Require a passing selected full gate and a Mastermind final review. Use an independent read-only review when it is useful, but do not treat it as mandatory ceremony. Apply the Development Workflow's maximum-two-cycle rule before acceptance.
2. If a material or blocking finding remains, return to the [Development Workflow correction rule](_development.md#steps). Acceptance does not redefine or extend the exceptional second-cycle boundary. Return unresolved blocking findings to the maintainer when that rule does not permit acceptance.
3. Update the authoritative Task with acceptance, commits, selected evidence, cycle disposition, and residual risk. Do not claim acceptance while a material decision or blocking finding remains unresolved.
4. The Mastermind integrates accepted work by squash-merging the focused `feature/<task>` branch into `develop`. Use a well-thought-out merge title and description that cover relevant changes, evidence, and references.
5. Release and publish only from `main`; do not release or publish from `feature/<task>` or `develop`.
6. If prior user direction authorizes a sequence and the next coherent Task is within scope, start a fresh Task Lifecycle with a new read-only Preflight and a new focused branch from exact `develop`. Present its plan as a progress update rather than silently expanding the accepted Task.
7. Stop before creating, integrating, or delegating the next Task when its outcome is not already authorized, a material decision remains unsettled, or safe branch isolation is unavailable.

## Completion

- The selected full gate passed and Mastermind final review found no blocking finding.
- Any material correction used the single exceptional correction cycle, reran the earliest invalidated phase and all applicable downstream work, and repeated review and the gate.
- The authoritative Task records accepted delivery state, evidence, commits, correction disposition, and residual risk.
- Accepted work is squash-merged into `develop` with a clear title and description covering relevant changes, evidence, and references.
- No release or publication occurred from `feature/<task>` or `develop`; release remains a `main`-branch action.
- Continuation began only within prior authority with a fresh Preflight and exact-`develop` branch, or stopped at the first new boundary.
