---
open-forge:
  description: What the existing published-process and package tests can actually prove
  tags: [Memory, CLI, Testing, Evidence, Contextual, Archived, Historical]
---

# Harness And Evidence Boundaries

This is a static source audit dated 2026-09-19, not a new test execution or a code-coverage measurement. Return to the [coverage review](./_cli-experience-coverage.md).

## Published CLI Tests

The [E2E project](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/OpenForge.Cli.EndToEndTests.csproj) contains 112 test methods in 35 classes: 100 Facts and 12 Theories with 63 InlineData rows, or **163 statically declared executions**. A theory row may execute several processes. This count says nothing about a current pass rate, instruction coverage or the percentage of user outcomes supported.

[PublishedExecutableTarget](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/Shared/PublishedProcess/PublishedExecutableTarget.cs) selects the published executable using configuration and optional RID, checking its version marker. Without a RID it selects the development publication; with a RID it selects the matching native publication. The EndToEnd label alone therefore does not establish Native AOT execution.

[PublishedProcessTestSupport](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/Shared/PublishedProcess/PublishedProcessTestSupport.cs) launches the actual executable with arguments, working directory and environment. Its read-only helper compares a caller-supplied snapshot before and after. That proves preservation only within the supplied snapshot's domain: a workspace-file hash map does not automatically include empty directories, permissions, external settings, locks or recovery archives.

[PublishedWorkspaceTreeSnapshot](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/Shared/PublishedProcess/PublishedWorkspaceTreeSnapshot.cs) can capture path kind, attributes, creation/write times, file length/hash and raw symlink targets without traversing links. Several richer fixtures include external recovery and lock state. These are in-memory state comparisons, not reviewed golden snapshots of user-visible output. The method inventory identifies relevant limits rather than crediting every test with the richest available helper.

[TestProcessIsolation](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/TestProcessIsolation.cs) redirects the per-user data home at test-process initialization. This is valuable isolation; it does not itself assert the absence of unexpected effects in every individual test.

## Interaction, Cancellation And Contention

[ProcessRunner](../../../../../src/cli/tests/end-to-end/OpenForge.Cli.EndToEndTests/Shared/PublishedProcess/ProcessRunner.cs) uses the real process argument list and working directory, redirects stdout/stderr and drains both. It does not supply a PTY or a prompt-answering driver; stdin is not explicitly redirected by this runner. Existing redirected-output confirmation tests establish the noninteractive boundary, not real once/always consent.

Runner cancellation kills the owned process tree and drains output. The cancellation E2E checks that mechanism. It does **not** establish that the CLI received and handled a real interrupt, distinguished cancellation before/after mutation, emitted a cancelled or partial receipt, or preserved recovery correctly after an effect.

The Cleanup fixture can hold a genuine workspace lease, but the E2E acquires it after eligible cleanup has finished and then checks a no-op. It proves a no-op avoids unnecessary lock acquisition. It does not exercise two simultaneous CLI writers.

There is useful **Integration**, rather than E2E, contention evidence:

- [WorkspaceLockManagerIntegrationTests:14](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Mutation/Locking/WorkspaceLockManagerIntegrationTests.cs), `AcquireOwnsExternalHandleAndPersistsReusableFile`, holds a real exclusive handle, rejects a contending request, releases it and reacquires the persistent zero-byte lock. Its cancellation case at line 67 cancels before acquisition and checks no infrastructure is created.
- [WorkspaceLockContractIntegrationTests:11](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Framework/Mutation/Locking/WorkspaceLockContractIntegrationTests.cs), `WorkspaceLockLeaseRequiresExactExternalHandle`, verifies lease identity and ownership boundaries.
- These do not prove the complete F20 journey: live writer A, blocked competing CLI, useful user explanation, unrelated workspace B proceeding, then successful retry after release. A persistent lock file alone is not live contention.

## Fixture Limits That Change The Assessment

- The Cleanup E2E's damaged archive belongs to a **foreign workspace bucket**. Preserving it while cleaning the selected bucket does not prove F23's same-workspace damaged-plus-valid continuation.
- Native Skill fixture data and helpers for metadata-incomplete or unreadable content exist without corresponding test assertions or callers. Definitions are potential reuse, not coverage.
- Most Library registrations and symlinks are seeded directly. Real symlinks are used, but a seeded state does not prove that public attach created the reviewed state. Sync preview→apply and the grant theory's attach→sync/detach are genuine shorter sequences.
- A null `LinkTarget` assertion alone does not distinguish an absent path from an ordinary replacement file. Full snapshot comparisons and independent existence checks establish different facts.
- The Update fixture's retired member is seeded by editing ownership/version data. It is not a pair of independently published historical/current packages.
- No current test execution or cross-platform capability result is claimed. A real symlink operation in a fixture is not a test of symlink denial or unsupported capability.

## Existing Integration Evidence For Partial Effects

These are actual operation/factory calls with real filesystem effects, not fabricated result objects. They remain a different boundary from a published executable and do not complete F19:

| Existing test | What is independently asserted | What it does not establish |
| --- | --- | --- |
| [UpdateBeforeOutputSnapshotTests:148](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Update/UpdateBeforeOutputSnapshotTests.cs), `PartialWriteFailure` | Windows sharing denial on loader; earlier Guidance replacement restores captured intended bytes; loader retains edited bytes; first effect Verified; overall Failed and recovery Retained. | No recovery payload check or explicit ownership comparison; residual path is only conditionally checked. The full snapshot wrongly attributes the blocked subject to the ownership file, so it does not prove an ownership-publication fault. |
| [InstallBeforeOutputSnapshotTests:90](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Install/InstallBeforeOutputSnapshotTests.cs), `PartialWriteFailure` | Windows sharing denial on Memory entrypoint; blocked bytes unchanged, earlier loader exists, ownership absent, earlier effect Verified, blocked effect NotStarted, non-null retained recovery bundle exists. | No full earlier-effect inventory, ZIP payload validation or subsequent diagnosis/continuation. |
| [UpdateOwnershipSafetyIntegrationTests:12](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Update/UpdateOwnershipSafetyIntegrationTests.cs), `UnwritableOwnershipDoesNotInvalidateVerifiedContentEffects` | A directory occupies the ownership-file path; Update completes with Verified content, an OwnershipObservation finding, restored loader and surviving directory. | No assertion that a publication attempt failed, exact restored bytes, preserved prior claims or later commands refusing to invent ownership. Adjacent evidence for X29, not the complete scenario. |
| [UpdateBeforeOutputSnapshotTests:124](../../../../../src/cli/tests/integration/OpenForge.Cli.IntegrationTests/Commands/Update/UpdateBeforeOutputSnapshotTests.cs), `Cancelled` | Scripted confirmation rejection returns Interrupted and unchanged workspace hashes. | No actual terminal signal or after-effect cancellation. |

## Package Delivery And CI

[package-manager.e2e.test.ts](../../../../../scripts/delivery/npm/__tests__/package-manager.e2e.test.ts) stages local npm tarballs, installs them offline and executes the npm shim with `--version`. Its Linux payload is a **shell fixture**, checking argument/environment forwarding and exact output. It does not execute the real CLI workspace flows.

[installed-native.ts](../../../../../scripts/delivery/npm/__tests__/installed-native.ts), invoked by the [native package journey](../../../../../scripts/delivery/npm/__tests__/native-package-journey.ts), checks real native payload hashes and versions, launches the installed package launcher with `--version`, checks exact output and verifies source artifacts remain unchanged. Its receipt explicitly does not claim npm-shim execution. This is real native delivery evidence, but only a version launch, not Framework installation or any of the 26 reviewed journeys.

[build.yml](../../../../../.github/workflows/build.yml) configures verification plus six RID jobs across Linux, macOS and Windows, x64 and arm64, with built managed/native tests and package checks. [test.ts](../../../../../scripts/delivery/test.ts) selects the built test assemblies. This is **configured execution scope**, not evidence that those jobs passed for the current worktree.

## Assertions And Output Snapshots

The current suite has valuable semantic JSON, stream/exit, exact-byte and before/after-state assertions. Some human-output checks assert only selected phrases or the absence of a heading. Those cannot certify a complete helpful, nonrepetitive report. Many tests use precise values appropriately for paths, status, membership or state; these should not be dismissed as arbitrary strings.

For the later approved work, use named snapshots when the expected object is a complete human transcript or structured view. Multiple named snapshots in one test or suite are already allowed by the [testing directive](../../../../directives/open-forge/testing/evidence-integrity.md). Keep direct assertions for independent semantic facts and effects: actual file sets, exact authored bytes, link identity, ownership, preserved user files, and state passed to the next action. Do not turn the CLI's printed receipt into its own correctness oracle.

The earlier [manual run](../cli-experience-run.md) is separate evidence. Its observations do not become automated coverage merely because they exercised the same commands.
