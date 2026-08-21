---
open-forge:
  description: Adopt Preflight, plan and isolate one development Task from exact develop, preserve coherent commits, and keep authoritative progress current
  tags: [LoadNow, Workflow, Development, Orchestration, Planning, Git, Branch, Commit, Progress]
---

# Task Lifecycle

## Goal

Prepare and run one implementation Task after read-only Preflight, with accepted planning and architecture, a focused branch from exact `develop`, reviewable commits, and authoritative progress.

## Steps

1. Start with the read-only [Phase 0 - Preflight](phase-0-preflight.md). The Mastermind owns and adopts its blueprint. Preflight may run before Task state, branch state, or plan approval changes.
2. The Mastermind records the outcome, scope, baseline, accepted architecture, authority, allowed and forbidden surfaces, decision-relevant Preflight analysis, alternatives and deferred ideas, phase outputs, active toolchain, focused and full evidence, public scenario, likely commits, correction boundary, and continuation boundary. Preserve enough reasoning to reconstruct the plan after an unexpected context loss. Present the concise Task plan for any required maintainer approval, unless existing direction already authorizes uninterrupted continuation. Obtain any required plan or architecture acceptance before mutation. Stop while a material decision remains unresolved.
3. Verify isolation and the exact starting commit. For future development work in this repository, branch from exact `develop` into one focused `feature/<task>` branch. Do not disturb unrelated work or use a broad branch for unrelated Tasks.
4. Keep each phase handoff bounded to the Task, relevant authority, baseline and branch, allowed and forbidden surfaces, required evidence, and preceding result. The Mastermind retains phase ownership and transitions. A helper may supply optional bounded evidence when a separate context materially helps, but no helper is a mandatory phase owner or ceremony.
5. The Mastermind performs integrated inspection of changed paths, the diff, and reproduced evidence after each phase result and before accepting that phase commit. The Mastermind owns exact-path formatting and derived-state regeneration when Preflight or repository rules require them.
6. Preserve coherent, readable feature commits. Only the Mastermind stages exact intended paths and commits accepted work after inspecting the actual result. Helpers do not edit Task state, stage, commit, merge, push, or accept work.
7. After each mutating Gray, Red, Green, Blue, or Purple phase, the Mastermind updates the authoritative Task progress in the same coherent commit and commits that accepted phase before the next phase mutates files. A no-change phase carries its evidence into the next coherent phase or acceptance commit rather than receiving a status-only or empty commit.
8. Pause for an unexpected workspace change, baseline conflict, material decision, authority boundary, or unsafe branch condition. Do not continue mutation merely to preserve a phase sequence.

## Phase Boundary Commits

The phase commit is both an integration checkpoint and a protected-surface
boundary.

- The Gray commit freezes the callable production surface. It contains the
  accepted production skeleton or callable contract, its required compilable
  support, and the same-commit authoritative Task progress. It contains no
  tests or domain behavior.
- The Red commit may intentionally contain executable failing tests, but those
  tests must compile and the expected failures must be explicitly recorded. Red
  must not alter Gray production or the frozen contract.
- The Green commit makes the frozen Red evidence pass. Green must not alter Red
  expectations, fixtures, or snapshots.
- Blue and Purple each receive a separate commit when they mutate files. Blue
  must not alter the contract or tests. Purple must not alter the contract or
  production behavior; it may update only the narrow production test-access
  declaration required by an accepted test-project rename or split, or relocate
  an explicitly accepted test-only production probe that has no product consumer
  into its owning test tier.
- Gray starts from the exact Task baseline. At every later boundary, compare the
  next phase's starting tree with the preceding phase commit when that phase
  mutated files, otherwise the most recent mutating-phase commit plus the
  recorded no-change result. Verify protected and frozen surfaces before
  mutation.
- If the phase-commit rule arrives after phases are already inseparably
  complete, do not rewrite history or manufacture phase snapshots. Record the
  exception and commit the truthful current state. Later Blue and Purple phases
  receive separate commits when they mutate files; no-change evidence carries
  forward without an empty commit.
- When new maintainer direction arrives after its ordinary phase has been
  truthfully completed, do not silently reopen or rename that history. The active
  Task must define any authorized Task-local refinement with its owner, exact
  baseline, allowed and protected surfaces, behavior and expectation boundary,
  evidence, and separate commit before later phases continue. State whether the
  refinement is a new direction or consumes the exceptional correction cycle.

## Task Record

For each new or active Task, keep one concise authoritative Task record that links to accepted sources and contains these sections:

- `Outcome` states the delivery.
- `Authority` links the relevant contracts, Patterns, Directives, and Guidance.
- `Baseline` identifies the exact starting commit and focused `feature/<task>` branch.
- `Analysis And Accepted Plan` preserves the decision-relevant requirements, alternatives, assumptions, accepted clarifications, and deferred ideas needed to reconstruct the Task after context loss.
- `Progress` records Preflight, Gray, Red, Green, Blue, Purple, any explicitly authorized Task-local refinement, public scenario, full gate, optional independent review, correction, and Acceptance with compact evidence links.
- `Decisions Needed` contains only material choices that block or could change the work.
- `Evidence` records the selected toolchain, focused and full commands, exact-path formatting, derived-state regeneration, public scenario, review result, gate result, correction disposition, and residual risk.
- `Completion` states the exact closeout conditions.

## Completion

- The Mastermind adopted a read-only Preflight blueprint and accepted the Task plan and architecture within applicable maintainer authority.
- The Task has one focused `feature/<task>` branch whose starting commit is exact `develop`, and unrelated work remains isolated.
- Only the Mastermind staged and committed accepted work, each mutating phase boundary was inspected and committed before continuation, and the feature commits remain coherent and readable.
- The authoritative Task records current progress, decisions, commits, selected evidence, correction disposition, residual risk, and completion.
