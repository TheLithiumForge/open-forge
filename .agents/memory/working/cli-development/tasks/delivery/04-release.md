---
open-forge:
  description: Align public documentation, accept all six targets, and prepare an authorized release
  tags: [Memory, Working, CLI, Task, Distribution, Documentation, Release, Contextual]
---

# Task 22: Final Documentation, Acceptance, and Release

## Task State

- State: Queued last, after Task 21, Task 27, Task 7's new ARM64 package
  expansion, Task 13's six-target native CI and artifact evidence, and the final
  complete Status/Doctor contributor gate. The maintainer approved this work
  on 2026-09-09; publication remains a separate exact authorization.
- Permanent mapping: Task 22 “Final Documentation, Acceptance, and Release” in
  the [project control ledger](../../project-control.md).
- Responsible roles: Mastermind and maintainer.
- Parent: [CLI Delivery](_delivery.md).

## Expected Outcome

Public documentation, package metadata, command help, schemas, the complete
accepted six-target package graph (`linux-x64`, `linux-arm64`, `osx-x64`,
`osx-arm64`, `win-x64`, and `win-arm64`), and release
notes describe the exact product. One separately authorized main-only release
publishes the verified native and packed artifacts and passes public smoke tests.
Current implementation and receipts do not yet prove the new ARM64 targets.

## Pre-Release Acceptance

1. Reconcile every command contract with help, docs, implementation, and evidence.
2. Run the proportional full managed and native gates, consume Task 13's evidence
   for every accepted native host and Task 7's six-target package-graph journeys, then verify
   packed installation/invocation, checksums, unchanged-state, mutation/recovery,
   and public smoke evidence.
3. Review the complete Git range from the greenfield baseline, current
   Architecture, dependencies, generated sources, the current native/checksum
   workflow, wrappers, and public documentation.
4. Resolve every blocker. Preserve explicit residual risk and unsupported boundary.
5. Obtain maintainer acceptance for the exact version and artifact manifest.

## Release

Release only from the accepted main commit through a separately authorized,
atomic workflow. Verify registry contents, the complete accepted six-target package
graph and checksums, package installation and launcher reachability for the
accepted platform artifacts, direct native execution, version/help,
representative read-only and mutation journeys, and public links. Do not rebuild
artifacts after acceptance. Do not present a platform subset as the complete
graph.

Do not claim additional RIDs, signatures, SBOM, provenance, OIDC attestation, or
support-floor coverage. Each requires a later explicit maintainer decision and
its own updated Architecture, Plan, Task, documentation, and evidence boundary.

## Closeout

Update current Architecture, contracts, Sources Of Truth, public docs, development
guide, release records, and the exact complete six-target package support statement.
Consolidate accepted Task outcomes and evidence. Archive or prune temporary
Plan/Task/Checkpoint detail.
Record follow-up work separately; do not hide it in a completed release.

## Stop Conditions

Stop before partial publication, non-main release, artifact rebuild, undocumented
contract deviation, unresolved high-severity finding, a missing accepted target
artifact or package, failed package reachability or native smoke, broader
unsupported delivery claim, or absent maintainer acceptance and exact publication
authorization.
