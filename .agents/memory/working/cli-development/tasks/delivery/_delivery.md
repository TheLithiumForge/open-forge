---
open-forge:
  description: Package, prove, document, and release the complete native CLI without partial publication
  tags: [Memory, Working, CLI, Task, Distribution, NativeAOT, SupplyChain, Release, Contextual]
---

# CLI Delivery

## Target Selection Continuation

The maintainer accepts an exact target subset for an individual published
version. The wrapper must list precisely that selection, and release collection
and publication validate it as a complete selected graph. The default Actions
graph remains all six. [Task 13](02-native-ci.md#unified-delivery-cli-and-visible-stages)
records this accepted continuation, which supersedes earlier fixed-graph
constraints only for explicitly selected versions.

## Delivery Simplification Continuation

The maintainer approved shared local/CI build, test and packaging commands,
synchronized versioning and pipeline-only releases on 2026-09-12.
[Task 13](02-native-ci.md#delivery-simplification-continuation) completed this bounded continuation
at phase 3/3, milestone 6/6. Feature `0101f25c` was squash-integrated at
`1b8475ae` with exact tree `5461e002`. Its current capsule supersedes earlier workflow/
version constraints for this change only. Earlier completed horizons and
the local-only external-effect boundary remain intact.

## Task State

- State: Complete locally; Tasks 13 and 22 are squash-integrated at `3bf03e0e`.
- Parent: [Complete The Replacement CLI](../00-cli-development.md).
- Prerequisite: Every retained command and operational command is accepted.

## Expected Outcome

The accepted distribution contains one main npm package and six optional
platform packages: Linux, macOS, and Windows, each on x64 and ARM64. The
[Distribution document](../../../../crystallized/documents/cli/distribution.md)
defines exact package identities. Task 7 implements the graph and package-owned
journeys. Task 13 proves native CI and artifacts on all six matching hosts.
Task 22 aligns documentation, accepts the complete graph, and prepares its
separately authorized atomic publication. Task 7 has implemented the six-target package graph with all-six layout proof.
The user accepted Linux execution and static review for local completion on
2026-09-10. Foreign-host execution and publication are outside this horizon;
all remote Git/GitHub operations are prohibited.

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
- [x] [Expand Task 7 to the accepted ARM64 packages](01-npm-packages.md#arm64-expansion-horizon) — Complete at phase 4/4, milestone 6/6; exact integration7eeeb19d and local evidence accepted
- [x] [Complete native CI and reproducible artifact collection for all six targets](02-native-ci.md) — Complete locally at phase 3/3, milestone 6/6; integrated at `3bf03e0e`
- [ ] [Preserve future supply-chain expansion behind an explicit decision](03-supply-chain.md) — Deferred outside current D1 — Implementer: Not assigned
- [x] [Align public documentation, accept all six targets, and prepare an authorized release](04-release.md) — Complete locally at phase 3/3, milestone 6/6; integrated at `3bf03e0e`

## Entries

<!-- open-forge:generated-index:start -->

- [Prepare and prove the thin npm package graph and explicit local-link workflow](01-npm-packages.md) - #Memory #Working #CLI #Task #Delivery #Npm #Package #Contextual
- [Prove native CI and reproducible artifact collection for all six accepted platforms](02-native-ci.md) - #Memory #Working #CLI #Task #Distribution #NativeAOT #CI #Contextual
- [Preserve future signatures, SBOM, provenance, and OIDC expansion behind explicit acceptance](03-supply-chain.md) - #Memory #Working #CLI #Task #Distribution #SupplyChain #Security #Contextual
- [Align public documentation, accept all six targets, and prepare an authorized release](04-release.md) - #Memory #Working #CLI #Task #Distribution #Documentation #Release #Contextual

<!-- open-forge:generated-index:end -->
