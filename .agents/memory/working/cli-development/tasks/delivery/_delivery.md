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

The canonical executable publishes for six RIDs, thin npm wrappers invoke the
matching binary without behavior or install-time compilation, all packages pass
packed journeys, supply-chain artifacts are produced, support floors execute,
public documentation matches the product, and the maintainer accepts one complete
main-only release.

## Delivery Invariants

- No command subset or partial package release.
- No wrapper download, postinstall compilation, behavior, or fallback runtime.
- Checksums, signatures, SBOM, provenance, and OIDC attestation bind exact native
  artifacts.
- Release credentials and remote publication remain outside local Tasks until the
  explicit release Task authorizes them.

## Child Tasks

- [ ] [Create and prove the thin launcher package and six platform packages from accepted native artifacts](01-npm-packages.md) — Planned — Implementer: Not assigned
- [ ] [Complete six-RID native CI, support-floor execution, and reproducible artifact collection](02-native-ci.md) — Planned — Implementer: Not assigned
- [ ] [Produce checksums, signatures, SBOM, provenance, and OIDC attestation for exact release artifacts](03-supply-chain.md) — Planned — Implementer: Not assigned
- [ ] [Align public documentation, run complete acceptance, publish from main, and close the program](04-release.md) — Planned — Implementer: Not assigned

## Entries

<!-- open-forge:generated-index:start -->

- [Create and prove the thin launcher package and six platform packages from accepted native artifacts](01-npm-packages.md) - #Memory #Working #CLI #Task #Distribution #Npm #Package #Contextual
- [Complete six-RID native CI, support-floor execution, and reproducible artifact collection](02-native-ci.md) - #Memory #Working #CLI #Task #Distribution #NativeAOT #CI #Contextual
- [Produce checksums, signatures, SBOM, provenance, and OIDC attestation for exact release artifacts](03-supply-chain.md) - #Memory #Working #CLI #Task #Distribution #SupplyChain #Security #Contextual
- [Align public documentation, run complete acceptance, publish from main, and close the program](04-release.md) - #Memory #Working #CLI #Task #Distribution #Documentation #Release #Contextual

<!-- open-forge:generated-index:end -->
