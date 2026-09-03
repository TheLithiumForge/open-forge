---
open-forge:
  description: Accepted public package graph, x64 platform horizon, staging, packing, checksum, proof, and publication boundary for the replacement CLI
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
defines current Linux D1 CI and artifact preparation. Their records and Git retain
implementation state and evidence receipts.

## Accepted Package Graph

The accepted public graph contains exactly one main package and three x64
platform packages:

| Package                                  | Native payload               | npm platform metadata                      |
| ---------------------------------------- | ---------------------------- | ------------------------------------------ |
| `@thelithiumforge/open-forge`            | Thin launcher only           | Platform-independent main package          |
| `@thelithiumforge/open-forge-linux-x64`  | `linux-x64` glibc executable | `os: [linux]`, `cpu: [x64]`, `libc: glibc` |
| `@thelithiumforge/open-forge-darwin-x64` | `osx-x64` executable         | `os: [darwin]`, `cpu: [x64]`               |
| `@thelithiumforge/open-forge-win-x64`    | `win-x64` executable         | `os: [win32]`, `cpu: [x64]`                |

The main package uses exact synchronized optional dependencies on all accepted
platform packages. They are not peer dependencies. Every staged public manifest
uses the same release or development version. The main package alone owns the
`open-forge` executable mapping. Each platform package contains only its
manifest, license, and native executable.

ARM is undecided. No ARM RID, package, runner, artifact, or support claim is
accepted by this boundary. Any new operating system, architecture, RID, libc,
package, channel, or support-floor claim requires a new maintainer decision.

## Current Implementation And Delivery State

The accepted target is not the present implementation. Task 7's historical
baseline implemented Linux x64 and Windows x64 packages. The accepted macOS x64
package and its `osx-x64` payload remain follow-up work. Task 13's current D1
scope remains Linux only. No document, workflow row, local build, or historical
receipt may be read as completion of the macOS target, broader CI support, or
release.

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

Task 13 owns the current Linux D1 native build and smoke, packed Linux install and
invocation, checksums, bounded artifact collection, and its exact receipt. The
reopened Task 7 horizon owns macOS and Windows package journeys and synchronized
graph realization on their applicable hosts.

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
- [Task 13: Native linux-x64 CI and Reproducible Artifacts](../../../working/cli-development/tasks/delivery/02-native-ci.md)
- [CLI Delivery](../../../working/cli-development/tasks/delivery/_delivery.md)
- [Repository-Root CLI Tooling Decision](../../decisions/repository-root-cli-tooling.md)
