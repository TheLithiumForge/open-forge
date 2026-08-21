---
open-forge:
  description: Extensions deliver optional whole files through existing routes, and installed files carry complete runtime meaning without package metadata or the CLI
  tags: [Memory, Decision, CurrentTruth, Extension, Package, RuntimeBoundary]
---

# Extension Package Boundary

## Context

Open Forge needs one way to distribute optional capabilities without enlarging
Core, inventing package behavior for every primitive, or making installed
workspaces depend on a package manager. Managed lifecycle operations also need
enough transparent state to protect user changes, shared files, and route
integrity.

## Decision

An Extension is an optional, content-agnostic installation and ownership unit
for complete files placed through existing `routes`.

An Extension is not a runtime primitive or another `root route`. Installed
files retain the meaning, loading behavior, scope, and authority of their
destination `routes`. The #Extension tag records optional package provenance
and composition without creating authority.

Package manifests, dependency edges, catalogue organization, and transparent
lifecycle ownership state exist for installation and managed lifecycle
operations. Installed content remains complete runtime truth and must remain
understandable and usable without that metadata or the CLI.

The CLI reduces manual work and adds deterministic planning, validation,
ownership checks, and recovery around this manual plain-file contract. It must
preserve reviewable effects, explicit ownership, user changes, route integrity,
and recoverability.

## Current Manager Boundary

This decision applies across two distinct CLI surfaces. The frozen TypeScript
`open-forge-old` executable uses `open-forge.extensions.json` as its Extension
receipt. The accepted non-shipping replacement uses the `extensions` section of
`.agents/open-forge.lifecycle.json`. It does not read, recognize, migrate, alias,
or fall back to the frozen receipt. The replacement leaves the old receipt
untouched; it remains ordinary workspace content outside replacement authority.
`open-forge-old` retains its existing receipt behavior. Both CLI surfaces remain
subordinate to the same runtime boundary: lifecycle evidence may support
managed operations, but it does not define the installed files' runtime meaning
or authority.

The accepted replacement's source-dependent operations use an embedded
catalogue or one explicitly selected local package or catalogue as one read-only
source universe. They resolve exact stable-ID dependencies offline. Extension
create writes to one selected catalogue destination, while remove needs no
package source bytes. The replacement does not add remote registries,
compatibility negotiation, version-range semantics, capability substitution, or
migration semantics. The replacement CLI Architecture and command contracts
define the exact current non-shipping mechanics without changing this package
boundary.

## Rationale

A content-agnostic unit lets one package contribute any useful combination of
Core or Memory content without duplicating package machinery for each role.

Whole files remain inspectable, diffable, independently routable, and manually
installable. Keeping package metadata outside runtime meaning preserves Open
Forge's file-native and tool-optional boundary.

Transparent managed ownership makes safe reconciliation and removal possible
without allowing the package manager to claim unowned content or become a
hidden semantic database.

## Alternatives And Tradeoffs

- A dedicated Extension runtime primitive or root would create a second
  interpretation model and make optional provenance determine behavior.
- Inline mutations and private companion additions would obscure authorship,
  complicate conflicts, and make manual installation and removal less reliable.
- Requiring the CLI, manifest, dependency graph, or lifecycle state at runtime
  would make interpretation depend on private tool state.
- Remote registries, remote trust and provenance policy, compatibility
  negotiation, capability substitution, migration, and multi-manager ownership
  remain future design territory. The initial replacement does not make them
  part of the package boundary or implement them.

The whole-file model may produce more files and explicit relationships than
mutation-based packaging. Managed lifecycle safety also requires transparent
metadata that is operationally important even though it is not runtime meaning.

## Consequences

- Package identity and source organization remain separate from installed route
  meaning.
- Dependencies select installable units, while installed files express their
  runtime relationships through ordinary routes and links.
- A package may contain one primitive, mixed content, support files, or only
  dependencies.
- First-party catalogue growth must not silently turn optional packages into a
  universal methodology.
- Lifecycle and CLI designs may evolve, but they must not make installed
  interpretation depend on private package state.
- The Extensions MVP Architecture defines current Extension package and runtime
  semantics. The replacement CLI Architecture and command contracts define the
  accepted non-shipping replacement mechanics.
- The CLI MVP Architecture and frozen source define only the executable
  historical reference.

## Sources Defining Current Results

### Extension Package And Runtime Boundary

- [Extensions MVP Architecture](../../documents/extensions/architecture.md)
- [Current Extension user contract](../../../../../docs/extensions.md)

### Accepted Replacement

- [Replacement CLI Architecture](../../documents/cli/architecture.md)
- [Replacement Extension command contracts](../../documents/cli/contracts/extension/_extension.md)
- [Shared replacement CLI operation contract](../../documents/cli/shared-operation-contract.md)
- [Current replacement CLI user contract](../../../../../docs/cli.md)

### Frozen Executable

- [CLI MVP Architecture](../../documents/cli/mvp-architecture.md)
- [Frozen MVP Extension implementation](../../../../../src/cli-mvp/cli.ts)

### Current Catalogue

- [First-party extension catalogue](../../../../../src/extensions/README.md)
- [Bundled extension source packages and manifests](../../../../../src/extensions/)

## Related Context

- [Historical CLI implementation reset](../../../archived/cli-release/implementation-reset-2026-08-21.md)
- [Future Extensions evolution questions](../../../emerging/ideas/extensions-overhaul.md)
- [Historical CLI-v2 evidence](../../../archived/cli-v2/_cli-v2.md)

## Decision Relationships

- [Product direction](../product/product-direction.md)
- [Distinct Core primitive roles](../framework/core-primitives.md)
- [Source and packaging](../framework/source-and-packaging.md)
