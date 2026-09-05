---
open-forge:
  description: Review the complete retained CLI command surface for direct PR-level architecture, design, refactoring, and test-evidence problems
  tags: [Memory, Working, Contextual, Planned, CLI, Task, Audit, Architecture, Refactoring, Testing, Review]
---

# Task 10: CLI Command Surface Audit

## Task State

- State: Queued after every retained CLI command is implemented and before
  delivery and release acceptance.
- Permanent mapping: Task 10 “CLI Command Surface Audit” in the
  [project control ledger](../project-control.md).
- Phase and milestone horizon: Deferred until activation. Queued work does not
  invent progress.
- Planned profile: Dedicated read-only Review Mastermind. Preflight may divide
  the review into bounded architecture, C# design, source-locality/refactoring,
  and test-evidence topics. One synthesis retains stable finding identities and
  removes duplicates. This audit does not implement its findings.
- Responsible role: A dedicated Review Mastermind in an isolated worktree based
  on the last integrated retained command.

This Task record preserves the accepted later audit boundary. The project
control ledger defines permanent identity, queue state, worktree mapping, and
integration state.

## Expected Outcome

Produce one decision-ready, PR-style review of the complete replacement CLI.
Flag material issues a strong reviewer would notice directly, including
Architecture or C# Directive violations, unclear responsibility boundaries,
missed higher-scope or shared refactors, unnecessary per-command file and model
proliferation, near-identical contexts or projections with uncertain authority,
and weak, redundant, wrongly tiered, or behaviorless tests.

This is a strategic first-pass review, not an exhaustive deep scrub. It records
stable findings with exact locations, consequences, correction direction,
responsible scope, dependencies, and proportional evidence. It also records
sound retained boundaries so later remediation does not generalize code merely
because it looks similar.

## Review Boundary

The audit covers the complete `src/cli/` production and test surface on its
frozen base, the current CLI Architecture and contracts, direct composition and
serialization boundaries, package-facing command reachability where relevant,
and the generated or shared sources that define CLI behavior.

The audit specifically examines commands that have accumulated large file
counts or families of similar request, result, fact, projection, context, and
renderer types. It distinguishes duplicated semantic authority from neutral
mechanism, and command-local policy from capabilities that have acquired a real
second consumer.

The audit does not change production code, tests, contracts, Architecture,
packages, generated runtime projections, dependencies, or public behavior. It
does not retest third-party libraries, runtimes, package managers, or framework
facilities. Test findings judge only Open Forge-owned behavior and evidence.

Every C# semantic reviewer must independently read the complete current
`.agents/directives/csharp/_csharp.md`, `design.md`, and `style.md` files and
report fresh SHA-256 fingerprints before reviewing. Reviewer packets must also
include the current CLI Architecture, applicable command contracts, source
locality, review evidence, and testing directives.

## Bounded Route Review Input

Task 10 retains one bounded audit input from Task 16's current route boundary.
`RouteInspectSourceProjectionBuilder` can form a command-local ambiguous-
overwrite state when one overwrite candidate's automatic ID maps to two or more
base sources; `RouteInspectOverwriteResolutionPolicy` then reports
`route-inspect.ambiguous-overwrite` as blocked. Task 10 must review whether this
synthetic overwrite-ambiguity conflict conflicts with the shared exact-pair
semantics or is separately justified by exact producer-observed facts under the
Route Inspect contract. This is one bounded
review input only: it is not a Doctor finding or a new task, does not restore any
removed Doctor route kind, and does not authorize broad search, inference, or
production change in the audit.

## Finding Standard

Each material finding must provide:

- one stable ID, severity, exact file and symbol or range, and reproduced fact;
- the violated authority or concrete maintainability consequence;
- the smallest credible correction direction and the scope responsible for it;
- dependencies, protected meaning, and proportional recheck evidence;
- a classification of defect, candidate improvement, deliberate tradeoff, or
  reviewer preference.

The Review Mastermind rejects duplicates, unsupported taste claims, speculative
frameworks, and test recommendations that exercise third-party behavior. It
must keep candidates as candidates when a second consumer or measurable
consequence is not yet proved.

## Acceptance Evidence

- Every retained command and its direct shared/composition/test neighborhood is
  covered once in the review inventory.
- Top-down architecture, callable design, source locality, responsibility and
  file shape, serialization/projection authority, and test evidence each have a
  recorded disposition.
- The final report separates confirmed defects, candidate refactors, accepted
  boundaries, dissent, and deferred deep-scrub questions.
- Only the Task record, audit report, and generated navigation needed for those
  records change. All executable and public surfaces remain byte-unchanged.
- A later remediation task is scheduled from accepted findings; this audit does
  not quietly implement or broaden them.

## Activation Boundary

Do not activate this Task until Route Move, Route Remove, Root Update, Extension
Install, Extension Update, Extension Remove, Status, Doctor, Repair, and Cleanup
are accepted and integrated or explicitly removed from the retained command
horizon. At activation, freeze the exact command inventory, review budget,
immutable base, protected surfaces, and finding taxonomy before delegation.
