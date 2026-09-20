---
open-forge:
  description: Define automatic embedded Framework and Extension resources, source identity, hashes, and parity evidence
  responsibility: Define runtime resource realization for the distributed Framework and first-party Extension catalogue
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

The Framework project embeds the complete canonical `src/open-forge/` tree through
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

## First-Party Extension Catalogue

`Framework/Extensions/Embedded/` owns the first-party catalogue reader. The Framework
project embeds `src/extensions/` package files through ordinary `EmbeddedResource`
items under `OpenForge.Extensions.Payload/`, excluding the catalogue-level
README. Every package contributes its manifest, documentation and `content/`
files. Package additions, removals and wording changes enter the next build
automatically; there is no separately authored compressed archive or hash list.

The reader enumerates only that resource prefix and rejects unsafe, duplicate
or noncanonical asset paths. Each first-level package directory requires an
`extension.json`. The manifest defines its stable ID and ordered dependencies;
the directory name does not replace that identity. The reader validates manifests
through the existing source-generated JSON boundary, hashes exact embedded
payload bytes, and projects only `content/` files as installed destinations.
Documentation remains embedded source content, not an installed payload target.

Package facts are ordered by stable ID. Duplicate IDs, missing dependencies and
dependency cycles invalidate the catalogue. A package without `content/` retains
its empty-payload semantics, including a dependency-only bundle. Existing source
selection, dependency resolution and lifecycle ownership rules remain unchanged.
The runtime never reads package files from a source checkout as a fallback.

Parity evidence derives the complete expected asset set from source files and
checks exact resource paths and bytes independently of the runtime reader. It
also compares manifest values, dependencies, payload destinations, lengths and
SHA-256 hashes. Do not maintain a second literal inventory oracle. Published
Native AOT evidence must exercise the relocated executable without the checkout
to establish that its available package facts and payload come from resources.

Updating distributed content does not grant an ownership migration. Prior
installations still follow the accepted Extension Update contract; conflicts
remain visible until a supported sequence resolves them.

## Related Current Sources

- [CLI Architecture](../architecture.md)
- [Ownership And Source Alignment Technical Design](lifecycle-provenance.md)
- [Install Contract](../contracts/install/_install.md)
- [Update Contract](../contracts/update/_update.md)
- [Route Init Contract](../contracts/route/init/_init.md)
- [Extension Contracts](../contracts/extension/_extension.md)
