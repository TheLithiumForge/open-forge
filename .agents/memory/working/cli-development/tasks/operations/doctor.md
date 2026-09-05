---
open-forge:
  description: Implement diagnosis domains, severity, recommendations, and complete doctor projections
  tags: [Memory, Working, CLI, Task, Doctor, Diagnosis, Contextual]
---

# Task 16: Doctor

## Task State

- State: Queued after Task 15 “Status”.
- Permanent mapping: Task 16 “Doctor” in the
  [project control ledger](../../project-control.md).
- Planned progress horizon: phase 0 of 5, milestone 0 of 8. The streamlined
  phases are Preflight, explicit Gray/Red, one coherent implementation and
  focused-verification pass, one fresh whole-task review with at most one
  grouped improvement pass, and acceptance.
- Incremental completeness: the first accepted horizon diagnoses the complete
  explicit contributor inventory frozen by Task 15. A healthy result means
  complete zero-finding evaluation for that declared inventory, never a guess
  from omitted or unavailable domains. Every later producer must extend the
  applicable typed facts and diagnostic domains before that producer is
  accepted. Final release requires a complete revalidated inventory for all
  retained producers.
- Parent: [Operational Commands](_operations.md).
- Contracts: [Interface](../../../../crystallized/documents/cli/contracts/doctor/interface.md) and [Behavior](../../../../crystallized/documents/cli/contracts/doctor/behavior.md).

## Read-Only Preparation And Activation Reconnaissance

The earlier Luna/max single-owner preparation against `develop` commit
`328599a006ef206fd82004e778296c2cac2bc10c`, tree
`56a24a7a50550702eae13bcbfaed9e9ead2a19f3`, remains historical context. Its
workflow limitation and later correction remain in the [trial
results](../../../../emerging/observations/2026-09-03_supervised-luna-preparation-trial-results.md);
they are not current authority for Task 16.

A later read-only activation reconnaissance began at root commit `53805940`. A
Sol/xhigh Task Mastermind successfully supervised three Luna/max Explorer
inventories from `/root/doctor_activation_recon`. No build, test, source or
workspace mutation, artifact, or activation occurred; the root advanced only
through coordination ledgers and is clean at `bbf2d87c`.

The reconnaissance confirmed that the six-domain, 112-kind Doctor contract is
unchanged: workspace/entry; recovery/residual; routes, metadata, overwrites, and
generated navigation; local references; Framework lifecycle; and Extension
lifecycle. Doctor consumes Task 15's immutable typed views; it does not parse
rendered Status output or fan directly into producers. Exact callable shapes
remain Status Gray-owned, and neutral readers remain reusable. Doctor must not
import another command's private `Shared/**` or introduce dependency injection,
a service locator, reflection, a runtime registry, a generic operational
engine, or mutation.

Task 16's prepared Extension lifecycle assumption is explicit: the Task 15
`ExtensionLifecycleDoctorView.Sources` member is a non-null, immutable,
materialized `IReadOnlyList<ExtensionSourceObservation>`. Each observation pairs
the exact nullable recorded source identity (`null` for the embedded catalogue)
with its own `ExtensionSourceReadResult`. There is exactly one observation per
distinct recorded source, ordered with the nullable embedded source first and
the remaining non-null identities by ordinal comparison. Doctor consumes that
complete list; it must not select a first source, substitute another source,
rebuild a nullable-key dictionary, or reread producers. The singular source
assumption is removed without changing Doctor's public contract, domain order,
or finding catalogue.

The activation order remains Task 14 integration, then Task 15 Gray and
acceptance, then Doctor. Task 7 may implement beside Status after Task 14 is
accepted and integrated. Further Doctor work has high staleness risk until the
Task 15 baseline is accepted, so Preflight must refresh the producer inventory
and bind the implementation to that exact baseline. Task 16 remains queued at
phase 0/5, milestone 0/8 and is suitable for the ordinary streamlined assured
flow only after those prerequisites are accepted.

## Expected Outcome

`doctor` evaluates accepted diagnostic domains from shared observed facts,
produces deterministic findings, severity, availability, and safe recommendations,
and performs no repair or cleanup.

## Architecture

- Keep command source at `Commands/Doctor/` with local
  `Shared/{Domains,Aggregation,Rendering}/`.
- Each domain receives typed facts and returns typed diagnostic findings. It does
  not reread another domain's files or parse Status output.
- Domain registration is explicit and statically ordered. No reflective discovery,
  diagnostic plug-in runtime, or universal rule expression language.
- Recommendations name accepted commands and exact safe next operations only.

## Requirements

Cover all contract diagnostic domains, unknown/unavailable facts, severity and
status precedence, deduplication without provenance loss, compact/expanded/JSON,
diagnostics, help, no writes, cancellation, and next actions. A healthy workspace
has a complete zero-finding result, not a guessed result from omitted domains.

## Evidence

Unit domain tests from fixed facts; Integration fixtures generated by actual
producer commands plus malformed/unsafe states; deterministic full-workspace
diagnosis; unchanged snapshots; process streams/exits; AOT; and Status regressions.

## Stop Conditions

Stop before applying a recommendation, inferring facts not observed by shared
readers, hiding unavailable domains, or introducing dynamic rule loading.
