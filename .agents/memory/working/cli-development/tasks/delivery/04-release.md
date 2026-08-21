---
open-forge:
  description: Align public documentation, run complete acceptance, publish from main, and close the program
  tags: [Memory, Working, CLI, Task, Distribution, Documentation, Release, Contextual]
---

# Accept And Release The Complete CLI

## Task State

- State: Planned after package, native CI, and supply-chain evidence.
- Responsible roles: Mastermind and maintainer.
- Parent: [CLI Delivery](_delivery.md).

## Expected Outcome

Public documentation, package metadata, command help, schemas, support matrix, and
release notes describe the exact complete product. One authorized main-only
release publishes verified native and package artifacts and passes public smoke
tests.

## Pre-Release Acceptance

1. Reconcile every command contract with help, docs, implementation, and evidence.
2. Run the full managed, Native AOT, six-RID, support-floor, packed-package,
   unchanged-state, mutation/recovery, and supply-chain gates.
3. Review the complete Git range from the greenfield baseline, current
   Architecture, dependencies, generated sources, workflows, wrappers, and public
   documentation.
4. Resolve every blocker. Preserve explicit residual risk and unsupported boundary.
5. Obtain maintainer acceptance for the exact version and artifact manifest.

## Release

Release only from the accepted main commit through the authorized workflow. Verify
registry contents, signatures, attestations, checksums, package installation,
direct native execution, version/help, representative read-only and mutation
journeys, and public links. Do not rebuild artifacts after acceptance.

## Closeout

Update current Architecture, contracts, Sources Of Truth, public docs, development
guide, release records, and package support matrix. Consolidate accepted Task
outcomes and evidence. Archive or prune temporary Plan/Task/Checkpoint detail.
Record follow-up work separately; do not hide it in a completed release.

## Stop Conditions

Stop before partial publication, non-main release, artifact rebuild, undocumented
contract deviation, unresolved high-severity finding, missing native platform,
failed smoke journey, or absent maintainer acceptance.
