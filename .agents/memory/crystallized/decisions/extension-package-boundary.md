---
open-forge:
  description: Extensions deliver optional whole files through existing routes, and installed files carry complete runtime meaning without package metadata or the CLI
  tags: [Memory, Decision, CurrentTruth, Extension, Package, RuntimeBoundary]
---

# Extension Package Boundary

## Context

Open Forge needed one way to distribute optional capabilities without enlarging Core, inventing package behavior for every primitive, or making installed workspaces depend on a package manager. Managed updates and removals also needed enough transparent state to protect user changes and shared files.

## Decision

An Extension is an optional, content-agnostic installation and ownership unit for complete files placed through existing `routes`.

An extension is not a runtime primitive or another `root route`. Installed files retain the meaning, loading behavior, scope, and authority of their destination `routes`. The #Extension tag records optional package provenance and composition without creating authority.

Package manifests, dependency edges, catalogue organization, and ownership receipts exist for installation and managed lifecycle operations. Installed content remains complete runtime truth and must remain understandable and usable without that metadata or the CLI.

The CLI reduces manual work and adds deterministic planning, validation, ownership checks, and recovery around this manual plain-file contract. It must preserve reviewable effects, explicit ownership, user changes, route integrity, and recoverability.

## Rationale

A content-agnostic unit lets one package contribute any useful combination of Core or Memory content without duplicating package machinery for each role.

Whole files remain inspectable, diffable, independently routable, and manually installable. Keeping package metadata outside runtime meaning preserves Open Forge's file-native and tool-optional boundary.

Transparent managed ownership makes safe reconciliation and removal possible without allowing the package manager to claim unowned content or become a hidden semantic database.

## Alternatives And Tradeoffs

- A dedicated Extension runtime primitive or root would create a second interpretation model and make optional provenance determine behavior
- Inline mutations and private companion additions would obscure authorship, complicate conflicts, and make manual installation and removal less reliable
- Requiring the CLI, manifest, dependency graph, or receipt at runtime would make runtime interpretation depend on private tool state
- Automatic capability substitution, remote registries, compatibility solving, and trust policy remain open design territory rather than implicit parts of the package boundary

The whole-file model may produce more files and explicit relationships than mutation-based packaging. Managed lifecycle safety also requires transparent metadata that is operationally important even though it is not runtime meaning.

## Consequences

- Package identity and source organization remain separate from installed route meaning
- Dependencies select installable units, while installed files still express their runtime relationships through ordinary routes and links
- A package may contain one primitive, mixed content, support files, or only dependencies
- First-party catalogue growth must not silently turn optional packages into a universal methodology
- Lifecycle and CLI designs may evolve, but they must not make installed interpretation depend on private package state
- The Extensions and CLI MVP Architectures define accepted current design; public documentation explains use, while implementation and tests express and verify exact mechanics

## Authoritative Sources

- [Extensions MVP Architecture](../documents/extensions/architecture.md)
- [CLI MVP Architecture](../documents/cli/architecture.md)
- [Current extension user contract](../../../../docs/extensions.md)
- [Current CLI user contract](../../../../docs/cli.md)
- [First-party extension catalogue](../../../../src/extensions/README.md)
- [Bundled extension source packages and manifests](../../../../src/extensions/)
- [Current CLI implementation](../../../../src/cli/cli.ts)

## Decision Relationships

- [Product direction](product-direction.md)
- [Distinct Core primitive roles](core-primitives.md)
- [Source and packaging](source-and-packaging.md)
