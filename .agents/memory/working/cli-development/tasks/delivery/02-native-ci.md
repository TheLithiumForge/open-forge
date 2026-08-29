---
open-forge:
  description: Complete current linux-x64 native CI and reproducible artifact collection
  tags: [Memory, Working, CLI, Task, Distribution, NativeAOT, CI, Contextual]
---

# Complete Current Native CI

## Task State

- State: Planned after complete command acceptance and package source.
- Parent: [CLI Delivery](_delivery.md).

## Expected Outcome

CI proves the current `linux-x64` native build and smoke, packed install and
invocation, and checksums. It records exact SDK/dependency and artifact facts and
collects reproducible bounded artifacts without publishing them.

## Matrix

- `linux-x64` on one native Linux runner.
- Restore from repository-root `NuGet.Config` with package audit.
- Native root build and direct smoke for `linux-x64`.
- Packed install/invocation journey for the accepted launcher and platform
  package.
- SHA-256 checksums for the exact bounded native and packed artifacts.

D1 consumes the separately recorded proportional whole-candidate managed and
Native AOT material gate. Full managed Unit, Integration, and EndToEnd suites and
native Integration/EndToEnd publishes or executions are Architecture/A1
evidence, not D1-owned requirements. A shared CI workflow may schedule them, but
that scheduling does not widen D1 scope.

Additional RIDs and support-floor jobs are outside current D1. They require a
later explicit maintainer decision with updated package, runner, evidence,
documentation, and maintenance boundaries.

## Reproducibility And Artifacts

Record SDK, `linux-x64`, commit, informational version, dependency graph, source
archive identity, binary/package hashes, package inventory, and smoke/packed
results. Upload only bounded release-candidate artifacts. Run publishes
sequentially when output is shared.

## Stop Conditions

Stop on emulated evidence presented as native, warning-bearing publish, skipped
required behavior, runner-specific uncommitted patch, root-path C# assumption,
automatic publication, or any broader RID/support-floor claim without explicit
acceptance.
