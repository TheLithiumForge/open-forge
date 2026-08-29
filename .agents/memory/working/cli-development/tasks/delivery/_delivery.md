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

The canonical `linux-x64` executable and thin package wrapper pass native build
and smoke, packed install and invocation, and checksum evidence. Public
documentation matches that exact product boundary, and the maintainer accepts
one complete main-only release.

## Delivery Invariants

- No command subset or partial package release.
- No wrapper download, postinstall compilation, behavior, or fallback runtime.
- Checksums bind the exact accepted native and packed artifacts.
- Release credentials and remote publication remain outside local Tasks until the
  explicit release Task authorizes them.
- Additional RIDs, signatures, SBOM, provenance, OIDC attestation, and support-
  floor matrices are future expansions that require a later explicit maintainer
  decision. They are not current D1 evidence.

## Child Tasks

- [ ] [Create and prove the thin launcher and `linux-x64` platform package from the accepted native artifact](01-npm-packages.md) — Planned — Implementer: Not assigned
- [ ] [Complete current `linux-x64` native CI and reproducible artifact collection](02-native-ci.md) — Planned — Implementer: Not assigned
- [ ] [Preserve future supply-chain expansion behind an explicit decision](03-supply-chain.md) — Deferred outside current D1 — Implementer: Not assigned
- [ ] [Align public documentation, run complete acceptance, publish from main, and close the program](04-release.md) — Planned — Implementer: Not assigned

## Entries

<!-- open-forge:generated-index:start -->
- [Create and prove the thin launcher and linux-x64 platform package from accepted native artifacts](01-npm-packages.md) - #Memory #Working #CLI #Task #Distribution #Npm #Package #Contextual
- [Complete current linux-x64 native CI and reproducible artifact collection](02-native-ci.md) - #Memory #Working #CLI #Task #Distribution #NativeAOT #CI #Contextual
- [Preserve future signatures, SBOM, provenance, and OIDC expansion behind explicit acceptance](03-supply-chain.md) - #Memory #Working #CLI #Task #Distribution #SupplyChain #Security #Contextual
- [Align public documentation, run complete acceptance, publish from main, and close the program](04-release.md) - #Memory #Working #CLI #Task #Distribution #Documentation #Release #Contextual
<!-- open-forge:generated-index:end -->
