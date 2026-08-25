---
open-forge:
  description: Complete six-RID native CI, support-floor execution, and reproducible artifact collection
  tags: [Memory, Working, CLI, Task, Distribution, NativeAOT, CI, Contextual]
---

# Complete Native CI And Support Floors

## Task State

- State: Planned after complete command acceptance and package source.
- Parent: [CLI Delivery](_delivery.md).

## Expected Outcome

CI builds and executes the complete CLI and test evidence for all six native RIDs,
runs supported operating-system floors, records exact SDK/dependency graphs, and
collects reproducible bounded artifacts without publishing them.

## Matrix

- `win-x64`, `win-arm64`, `linux-x64`, `linux-arm64`, `osx-x64`, and `osx-arm64`
  on native runners.
- Restore from repository-root `NuGet.Config` with package audit.
- Format and managed build once in an appropriate primary job.
- Managed Unit, Integration, and EndToEnd evidence.
- Native root, Integration, and EndToEnd publishes and executions.
- Complete public scenario and packed package journey per appropriate RID.
- Support-floor jobs execute already-built compatible artifacts on declared floor
  environments rather than rebuilding with newer assumptions.

## Reproducibility And Artifacts

Record SDK, RID, commit, informational version, dependency graph, source archive
identity, binary hash, package inventory, and test result. Upload only bounded
release-candidate artifacts. Run publishes sequentially when output is shared.

## Stop Conditions

Stop on emulated evidence presented as native, warning-bearing publish, skipped
required behavior, runner-specific uncommitted patch, root-path C# assumption, or
automatic publication.
