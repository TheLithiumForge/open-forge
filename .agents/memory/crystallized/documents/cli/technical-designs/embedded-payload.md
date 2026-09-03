---
open-forge:
  description: Exact embedded Framework payload resource, identity, hashing, and source-parity design
  responsibility: Define the shared runtime payload realization consumed by Framework lifecycle commands
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, TechnicalDesign, Framework, Distribution, EmbeddedResource]
---

# Embedded Payload Technical Design

## Boundary

`Framework/Distribution/` owns one neutral embedded Framework payload reader and
immutable asset and inventory facts. Root Install and Update consume the complete
inventory. Framework-aware Route Init consumes canonical route entrypoint assets
and topology from the same payload. Command-local selection, findings, effects,
and results remain in their contracts.

## Resource Realization

The Core project embeds the complete canonical `src/open-forge/` tree through
ordinary C# project `EmbeddedResource` items under one fixed logical-name prefix.
Runtime code uses BCL manifest-resource APIs only for that exact prefix. Bounded
resource-name enumeration and exact resource access are permitted. Reflective
assembly or type discovery, plug-in registration, behavioral dispatch, and
runtime checkout reads are forbidden.

The reader derives canonical `/`-separated relative paths and rejects empty,
unsafe, duplicate, or noncanonical identities. It reads exact bytes, orders
assets ordinally, computes one lowercase SHA-256 for each asset, and derives one
deterministic inventory fingerprint from the complete ordered inventory.

## Parity And Isolation

Source-parity evidence computes the current `src/open-forge/` source set and its
exact differences from the embedded payload. It does not maintain a second
literal hash catalogue. Evidence covers paths, ordering, exact bytes, per-asset
hashes, the aggregate fingerprint, and complete set equality.

Published Native AOT evidence moves the executable away from the checkout before
reading every resource. That proves the payload is embedded and that runtime
operation does not fall back to repository source paths.

## Related Current Sources

- [CLI Architecture](../architecture.md)
- [Lifecycle Provenance Technical Design](lifecycle-provenance.md)
- [Install Contract](../contracts/install/_install.md)
- [Update Contract](../contracts/update/_update.md)
- [Route Init Contract](../contracts/route/init/_init.md)
