---
open-forge:
  description: Package, prove, document, and release the complete native CLI without partial publication
  tags: [Memory, Working, CLI, Task, Distribution, NativeAOT, SupplyChain, Release, Contextual]
---

# CLI Delivery

## Task State

- State: Planned.
- Parent: [Complete The Replacement CLI](../00-cli-development.md).
- Prerequisite: Every retained command and operational command is accepted.

## Expected Outcome

The accepted distribution contains one main npm package and six optional
platform packages: Linux, macOS, and Windows, each on x64 and ARM64. The
[Distribution document](../../../../crystallized/documents/cli/distribution.md)
defines exact package identities. Task 7 implements the graph and package-owned
journeys. Task 13 proves native CI and artifacts on all six matching hosts.
Task 22 aligns documentation, accepts the complete graph, and prepares its
separately authorized atomic publication. Current package implementation and
completed evidence still cover only the earlier x64 scope described in Task 7.

## Delivery Invariants

- No command subset or partial package release.
- No platform subset may be presented as the complete accepted graph.
- No wrapper download, postinstall compilation, behavior, or fallback runtime.
- Checksums bind the exact accepted native and packed artifacts.
- Release credentials and remote publication remain outside local Tasks until the
  explicit release Task authorizes them.
- Target or evidence expansion beyond the accepted Task 7 package graph, plus
  signatures, SBOM, provenance, OIDC attestation, and support-floor matrices,
  requires a later explicit maintainer decision. It is not current D1 evidence.

## Child Tasks

- [x] [Prepare and prove the accepted thin npm package graph and local-link workflow](01-npm-packages.md) — Historical 8/8 complete and integrated
- [x] [Continue Task 7 with the accepted x64 platform-expansion horizon](01-npm-packages.md#platform-expansion-horizon) — Complete at phase 4/4, milestone 7/7; accepted lane `a2942781`, tree `fe36fc3f`, and squash integration `e19d429e` with the same tree. The Linux host journey passed; Darwin and Windows have stage-and-pack evidence only. This historical horizon supplies no ARM64 or publication proof.
- [ ] [Expand Task 7 to the accepted ARM64 packages](01-npm-packages.md#arm64-expansion-horizon) — Queued after Task 27 and before Task 13; phase and milestone horizon unassigned
- [ ] [Complete native CI and reproducible artifact collection for all six targets](02-native-ci.md) — Queued after Task 7; historical preparation phase 1/3, milestone 2/6 requires refreeze
- [ ] [Preserve future supply-chain expansion behind an explicit decision](03-supply-chain.md) — Deferred outside current D1 — Implementer: Not assigned
- [ ] [Align public documentation, accept all six targets, and prepare an authorized release](04-release.md) — Planned — Implementer: Not assigned

## Entries

<!-- open-forge:generated-index:start -->

- [Prepare and prove the thin npm package graph and explicit local-link workflow](01-npm-packages.md) - #Memory #Working #CLI #Task #Delivery #Npm #Package #Contextual
- [Prove native CI and reproducible artifact collection for all six accepted platforms](02-native-ci.md) - #Memory #Working #CLI #Task #Distribution #NativeAOT #CI #Contextual
- [Preserve future signatures, SBOM, provenance, and OIDC expansion behind explicit acceptance](03-supply-chain.md) - #Memory #Working #CLI #Task #Distribution #SupplyChain #Security #Contextual
- [Align public documentation, accept all six targets, and prepare an authorized release](04-release.md) - #Memory #Working #CLI #Task #Distribution #Documentation #Release #Contextual

<!-- open-forge:generated-index:end -->
