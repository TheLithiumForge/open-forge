---
open-forge:
  description: Produce checksums, signatures, SBOM, provenance, and OIDC attestation for exact release artifacts
  tags: [Memory, Working, CLI, Task, Distribution, SupplyChain, Security, Contextual]
---

# Produce Supply-Chain Evidence

## Task State

- State: Planned after six-RID artifacts and packed packages are final.
- Parent: [CLI Delivery](_delivery.md).

## Expected Outcome

Every native binary and package tarball has an exact checksum, signature,
software bill of materials, build provenance, and OIDC-backed attestation tied to
one source commit and CI run.

## Requirements

- One deterministic manifest lists every release artifact, size, SHA-256, RID or
  package identity, version, and media type.
- Signatures and attestations reference manifest and artifact digests, not mutable
  names alone.
- SBOM includes resolved managed packages, SDK/runtime components required by the
  artifact, package wrapper files, licenses, and build provenance.
- Provenance identifies source commit, workflow, runner image, SDK, build command,
  parameters, and artifact outputs without secrets or local identifiers.
- Verification runs in a clean independent job before release authorization.

## Evidence

Tamper tests prove checksum/signature/attestation rejection. Inventory comparison
proves no unlisted artifact. Package and executable versions agree. Security and
license audits have no unresolved release blocker.

## Stop Conditions

Stop before using long-lived release secrets where OIDC is accepted, signing a
mutable directory instead of exact artifacts, omitting a platform package, or
publishing before independent verification.
