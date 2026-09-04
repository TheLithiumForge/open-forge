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

The accepted distribution contains exactly one main npm package and three
optional x64 platform packages: `linux-x64`, `darwin/osx-x64`, and `win-x64`.
Task 7 owns that package graph and its applicable-host journeys. Task 13 owns
the later Linux-only D1 native build, checksum, and bounded-artifact evidence.
Task 22 owns final acceptance and separately authorized atomic publication of
the complete graph. Public documentation must match that exact boundary, and
the maintainer must accept one complete main-only release. ARM remains undecided
and outside this delivery.

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
- [ ] [Continue Task 7 with the accepted x64 platform-expansion horizon](01-npm-packages.md#platform-expansion-horizon) — Queued at phase 1/4, milestone 0/7; preparation is complete, and mutation waits for Task 14 acceptance and a fresh develop-based isolated lane
- [ ] [Complete current `linux-x64` native CI and reproducible artifact collection](02-native-ci.md) — Planned — Implementer: Not assigned
- [ ] [Preserve future supply-chain expansion behind an explicit decision](03-supply-chain.md) — Deferred outside current D1 — Implementer: Not assigned
- [ ] [Align public documentation, run complete x64 acceptance, publish from main, and close the program](04-release.md) — Planned — Implementer: Not assigned

## Entries

<!-- open-forge:generated-index:start -->

- [Prepare and prove the thin npm package graph and explicit local-link workflow](01-npm-packages.md) - #Memory #Working #CLI #Task #Delivery #Npm #Package #Contextual
- [Complete current linux-x64 native CI and reproducible artifact collection](02-native-ci.md) - #Memory #Working #CLI #Task #Distribution #NativeAOT #CI #Contextual
- [Preserve future signatures, SBOM, provenance, and OIDC expansion behind explicit acceptance](03-supply-chain.md) - #Memory #Working #CLI #Task #Distribution #SupplyChain #Security #Contextual
- [Align public documentation, run complete x64 acceptance, publish from main, and close the program](04-release.md) - #Memory #Working #CLI #Task #Distribution #Documentation #Release #Contextual

<!-- open-forge:generated-index:end -->
