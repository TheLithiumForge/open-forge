---
open-forge:
  description: Accepted public package graph, x64 and ARM64 platform horizon, staging, packing, checksum, proof, and publication boundary for the replacement CLI
  responsibility: Define durable replacement CLI distribution meaning without presenting incomplete platform work or release evidence as complete
  tags: [Memory, Crystallized, Document, CurrentTruth, Evergreen, CLI, Distribution, Npm, NativeAOT, Release]
---

# CLI Distribution

## Status And Authority

This document defines the accepted public package graph, platform horizon,
package identities, staging and packing boundary, checksum ownership, proof
boundary, and atomic complete-publication rule for the non-shipping replacement
CLI. It does not claim that every accepted target is implemented or released.

[Task 7](../../../working/cli-development/tasks/delivery/01-npm-packages.md)
defines package-graph realization, npm release, and explicit local-linking work.
[Task 13](../../../working/cli-development/tasks/delivery/02-native-ci.md)
defines native CI and artifact preparation for all six accepted targets. Their records and Git retain
implementation state and evidence receipts.

## Accepted Package Graph

The accepted public graph contains exactly one main package and six
platform packages:

| Package                                    | Native payload                 | npm platform metadata                        |
| ------------------------------------------ | ------------------------------ | -------------------------------------------- |
| `@thelithiumforge/open-forge`              | Thin launcher only             | Platform-independent main package            |
| `@thelithiumforge/open-forge-linux-x64`    | `linux-x64` glibc executable   | `os: [linux]`, `cpu: [x64]`, `libc: glibc`   |
| `@thelithiumforge/open-forge-darwin-x64`   | `osx-x64` executable           | `os: [darwin]`, `cpu: [x64]`                 |
| `@thelithiumforge/open-forge-win-x64`      | `win-x64` executable           | `os: [win32]`, `cpu: [x64]`                  |
| `@thelithiumforge/open-forge-linux-arm64`  | `linux-arm64` glibc executable | `os: [linux]`, `cpu: [arm64]`, `libc: glibc` |
| `@thelithiumforge/open-forge-darwin-arm64` | `osx-arm64` executable         | `os: [darwin]`, `cpu: [arm64]`               |
| `@thelithiumforge/open-forge-win-arm64`    | `win-arm64` executable         | `os: [win32]`, `cpu: [arm64]`                |

The main package uses exact synchronized optional dependencies on all accepted
platform packages. They are not peer dependencies. Every staged public manifest
uses the same release or development version. The main package alone owns the
`open-forge` executable mapping. Each platform package contains only its
manifest, license, and native executable.

The maintainer accepted ARM64 alongside x64 on Linux, macOS, and Windows on
2026-09-09. This defines the required target graph, not completed implementation
or native proof. Additional operating systems, architectures, RIDs, libc variants,
channels, or support-floor claims require a new maintainer decision.

## Current Implementation And Delivery State

Task 7's previous x64 platform-expansion horizon is complete at phase 4/4,
milestone 7/7. Its accepted lane is `a2942781` with tree `fe36fc3f`, and its
squash integration is `e19d429e` with the same tree. That historical implementation
staged and packed the three x64 packages. Its installed-launcher journey was
run on Linux; macOS and Windows have stage-and-pack evidence only.

Task 7's new ARM64 expansion is active after Task 27 and before Task 13.
Task 13 must refreeze its earlier Linux-only preparation for all six targets.
The six-target package graph is now implemented with staging and packing
evidence for all six targets. Final local acceptance also proves the actual
stamped Linux x64 native package journey and complete managed/Linux native
suites. The other five matching-host journeys and six-target CI acceptance
remain pending in Task 13. Publication and global installation refresh are not
established by this local evidence.
Historical local-link receipts remain scoped to their exact authorized action.

The replacement remains non-shipping until the complete retained command set,
accepted package graph and native target set, documentation, and release evidence
are accepted together.

## Main Launcher Boundary

The launcher is thin. It recognizes only the accepted host/platform pairs,
resolves the matching optional package, and directly hands process arguments,
streams, environment, and process state to the native executable. It contains no
CLI domain behavior, download, postinstall compilation, fallback runtime,
telemetry, or hidden installation path.

Repository-local `open-forge-dev` publication and local development linking are
private developer tooling. They are not public package identities, release
channels, or fallback behavior.

## Staging, Packing, And Checksums

Package-owned tooling stages the main package plus the selected host packages
from exact native artifacts into caller-owned ignored artifact roots. Staging
compiles the launcher closure, copies the supplied native artifact and license,
emits the exact synchronized manifests, and does not publish or contact a
registry.

Release proof packs the staged main and applicable platform packages, installs
their tarballs together in an isolated prefix, resolves the installed main
launcher, and invokes the exact native payload on a matching native host. Each
journey asserts Open Forge package placement, launcher reachability, argument and
process handoff, and the candidate version. It does not test npm, Node, the
operating system, or unrelated CLI command behavior.

SHA-256 manifests bind the exact accepted native executable, packed packages,
and source archive for the candidate. A checksum pass proves traceable identity
and integrity. Byte-for-byte reproducibility requires a separate repeated-build
comparison and is not implied by one manifest.

Task 7 owns the synchronized six-target package graph, staging and packing,
and package-owned journeys. Task 13 owns native build and smoke, packed
installation and invocation, checksums, and bounded artifact collection on a
matching native host for each accepted target. It separately consumes the
Architecture's managed and Native AOT test evidence. Emulation or cross-build
success cannot substitute for native execution. Task 22 consumes the complete
set for final acceptance and separately authorized publication.

## Publication Boundary

Public release is one complete atomic product event. No command subset, platform
subset presented as the complete accepted graph, partial package graph, or
warning-bearing artifact is published. Every main-package optional dependency
must be available at the synchronized version when the main package is released.

Credentials, registry contact, remote publication, signatures, SBOM, provenance,
OIDC attestation, and support-floor matrices remain outside local preparation
unless their exact release Task authorizes them. The package graph, checksums,
documentation, native proof, and publication result must agree before the
maintainer accepts the replacement as shipping.

## Related Current Sources

- [Replacement CLI Architecture](architecture.md)
- [Task 7: npm Package Manager Release and Local Linking](../../../working/cli-development/tasks/delivery/01-npm-packages.md)
- [Task 13: Native CI and Reproducible Artifacts](../../../working/cli-development/tasks/delivery/02-native-ci.md)
- [CLI Delivery](../../../working/cli-development/tasks/delivery/_delivery.md)
- [Repository-Root CLI Tooling Decision](../../decisions/repository-root-cli-tooling.md)
