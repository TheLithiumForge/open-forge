---
open-forge:
  description: Assess remaining C# internals and implement justified simplifications beyond the completed strategic command audit
  tags: [Memory, Working, Contextual, CLI, Task, CSharp, Architecture, Refactoring]
---

# Task 27: C# Structural Streamlining

## Task State

- State: Planned, after Task 21 and before Task 7 ARM64 expansion and Task 13.
- Permanent identity: Task 27 in the [project control ledger](../project-control.md).
- Phase and milestone horizon: unassigned until preflight defines the bounded
  assessment and selected implementation slices.
- Owner: root Overseer, sequential; substantive assistance uses Astra/high.
- Authority: the user's standing instruction to finish refactoring and improve
  simplicity, clarified on 2026-09-09 by asking whether C# opportunities were
  actually being assessed, and explicitly approved later that day. No new
  product behavior or publication is implied.

## Outcome And Boundary

Assess the remaining production C# internals for concrete simplification
opportunities, then implement justified bounded changes. Task 10 already
reviewed all 28 commands at strategic primary-path level, including design,
shared scope and tests. Its [report](cli-command-surface-audit-report.md) records
selected helpers and explicit limits. Task 21 closes its seven findings.
Neither completion establishes that every internal capability has been assessed.
Do not repeat accepted reviews merely to produce another report.

Start from the Task 21 accepted candidate. Inventory command-private helpers,
family-shared capabilities, Framework mechanisms and Shell/root composition;
reconcile prior semantic coverage with this inventory. Inspect remaining areas
by responsibility and caller relationships. File length and count are discovery
signals, not findings or targets to reduce mechanically. Test helpers are in
scope only where they materially affect owned evidence or production design.

For each candidate, show exact source/callers, the maintenance or reasoning
problem, the smaller design, protected behavior and cheapest decisive evidence.
Separate violated current directives from optional improvements and intentional
boundaries. Prefer direct data flow, cohesive existing facts, nearest shared
ownership and explicit finite policy. Remove duplication of meaning where real
consumers agree; preserve distinct command policy even when syntax looks alike.
Do not add speculative frameworks, DI, registries, reflective/string dispatch,
compatibility layers, dependencies or warning-silencing null suppression.

Every C# author/reviewer personally reads the complete current C# directive,
design and style and reports fingerprints. Match design force to this local,
nonpublic developer tool's actual filesystem and recovery responsibilities.
Retain source preservation, permissions, no-follow boundaries, leases, truthful
partial outcomes and record-last publication. Do not remove safeguards merely
because they add branches, or imply stronger transactional guarantees.

Freeze candidate scope before changing production. Use existing behavior
characterization for pure refactoring and independent failing evidence for
newly proved defects. Preserve three simple public journeys per command and
separate Shell/artifact subjects. Run appropriate focused evidence per slice,
then the required combined supported Linux managed/native boundary, with exact
source/runtime identity and all new/moved/deleted files accounted. Establish
review and correction budgets at activation; do not start an open-ended review
loop or invent an exhaustive-correctness claim.

## Accepted Commit Boundaries

On 2026-09-09 the maintainer explicitly approved this task and required isolated
refactoring and test changes. Freeze the current callable contracts, observable
behavior and test assertions at Gray before each bounded slice. For a proved
behavior defect, freeze independent Red evidence first and commit the correction
separately from pure refactoring.

Blue changes production structure against frozen behavior and tests. Commit
that production-only change after its focused evidence passes. Purple changes
or removes tests in a separate commit, mapping each unique assertion to retained
or replacement evidence at the appropriate tier. Record why an assertion is
obsolete when removal is justified. Test removal does not authorize behavior
change, and moving assertions must not weaken the accepted contract.

Keep the same continuous author where useful; separate commits and protected
boundaries do not require separate agents. Refreeze all changed, moved, deleted
and formerly untracked files before staging. Final combined evidence follows the
accepted Task scope after the isolated increments are qualified.

## Preparation Observation

Read-only inventory at Task 21 M1 found 1,729 tracked production C# files under
`src/cli/`, excluding tests. This is an inventory count, not a semantic coverage
percentage. Extension Update planning is one concrete follow-up input: its
`ExtensionUpdatePlanner.BuildAsync` contains nested conditional selections in
recovery classification, selected IDs and selection kind, while current C#
Design forbids nested or chained conditionals. Verify these exact branches and
their independent evidence at activation. Its overall size alone does not
justify a new abstraction or a command-wide rewrite. Uninspected regions remain
unassessed, not implicitly sound or defective.

## Additional Preparation Candidates

Ignored evidence under `artifacts/task27-preparation/` supplies candidates,
not accepted implementation scope. Reconcile Task 10's narrative as well as its
explicit table locators: absence from the table does not mean unreviewed source.
The coverage note records the broader mechanisms already assessed.

`repair-branch-candidate.json` identifies two nested conditional selections in
RepairPostVerifier. Assess ordered local branches preserving evaluation and
fallback semantics; do not widen undefined-value policy during Blue work.
`install-dto-evidence-candidate.json` identifies the pre-existing Install
`JsonDtoGraphPreservesExactPacketValues` self-assignment test. Task 21 leaves it
byte-frozen and replaces its claimed wire ownership with actual composed
serialization evidence. Consider removing that tautology in isolated Purple
work after freezing real evidence. Assess neighboring CLR shape/nullability
checks separately against their accepted callable contracts.

The caller inventory and `commit-order-candidate.json` also distinguish dead
production helpers from helpers whose only callers are tests. Where needed,
freeze evidence through the live path and remove obsolete helper tests in a
Purple commit before deleting the helper in a separate Blue commit. Do not
claim an equivalent live oracle until the actual condition and assertion prove
it. Refreeze current source and select only justified candidates at activation.

`live-measurement-evidence.json` supersedes the earlier unresolved search for
live token-measurement evidence: StatusRouteTotalAvailableTests invokes the real
RouteContextReader and proves literal zero, two-rune/three-byte and nine-rune
measurements, plus unavailable states. Exact 1/4/5-character rounding boundaries
still need disposition before deleting the dead Status estimator's assertions.
Do not mistake its unused NotApplicable branch for a live provider outcome.

## Activation Requirements

Root first reconciles prior coverage and freezes a finite assessment packet,
including allowed areas, candidate standard, source identity and stopping
boundary. Implement only accepted in-scope findings in coherent sequential
slices. Carry unresolved consequential behavior or support choices to the user;
routine refactoring choices stay with the authorized owner. Record concrete
before/after design and tests rather than claiming model superiority or savings
without comparable evidence. Task 7 ARM64 expansion and Task 13 remain downstream of this work. The
local Linux managed/native gate proves this structural candidate only; Task 13
and Task 22 own the subsequent complete six-target delivery acceptance.
